using Domain.Dto.Request;
using Domain.Dto.Request.BarnPlan;
using Domain.Dto.Response;
using Domain.Dto.Response.BarnPlan;
using Domain.Dto.Response.Bill;
using Infrastructure.Extensions;
using Domain.IServices;
using Entities.EntityModel;
using Infrastructure.Core;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Implements
{
    public class BarnPlanService : IBarnPlanService
    {
        private readonly IRepository<BarnPlan> _barnplanrepo;
        private readonly IRepository<BarnPlanFood> _bpfoodrepo;
        private readonly IRepository<BarnPlanMedicine> _bpmedicinerepo;

        public BarnPlanService(IRepository<BarnPlan> barnplanrepo, IRepository<BarnPlanFood> bpfoodrepo, IRepository<BarnPlanMedicine> bpmedicinerepo)
        {
            _barnplanrepo = barnplanrepo;
            _bpfoodrepo = bpfoodrepo;
            _bpmedicinerepo = bpmedicinerepo;
        }

        public async Task<bool> CreateBarnPlan(Domain.Dto.Request.BarnPlan.CreateBarnPlanRequest req)
        {
            // xu ly daily
            DateTime formatedStartDate, formatedEndDate;
            ValidTime(true, (bool)(req.IsDaily == null ? false : req.IsDaily), req.StartDate, req.EndDate, out formatedStartDate, out formatedEndDate);

            // insert barn
            BarnPlan barnPlanDetail = new BarnPlan()
            {
                LivestockCircleId = req.livstockCircleId,
                Id = Guid.NewGuid(),
                Note = req.Note,
                StartDate = formatedStartDate,
                EndDate = formatedEndDate
            };

            _barnplanrepo.Insert(barnPlanDetail);
            if (await _barnplanrepo.CommitAsync() < 0)
            {
                throw new Exception("Không thể tạo kế hoạch");
            }


            // insert food plan
            InsertFoodPlan(req.foodPlans, barnPlanDetail.Id);
            // insert medicine plan

            await InsertMedicinePlan(req.medicinePlans, barnPlanDetail.Id);
            // create response

            return true;
        }

        public async Task<PaginationSet<ViewBarnPlanResponse>> ListingHistoryBarnPlan(Guid livestockCircleId, ListingRequest request)
        {
            try
            {
                if (request == null)
                    throw new Exception("Yêu cầu không được null.");
                if (request.PageIndex < 1 || request.PageSize < 1)
                    throw new Exception("PageIndex và PageSize phải lớn hơn 0.");

                var validFields = typeof(BillItem).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                    throw new Exception($"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}");



                var query = _barnplanrepo.GetQueryable(x => x.IsActive).Where(it => it.LivestockCircleId == livestockCircleId);

                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                var result = await query.Select(i => new ViewBarnPlanResponse
                {
                    Id = i.Id,
                    EndDate = i.EndDate,
                    foodPlans = null,
                    medicinePlans = null,
                    Note = i.Note,
                    StartDate = i.StartDate
                }).Pagination(request.PageIndex, request.PageSize, request.Sort);

                return (result);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách: {ex.Message}");
            }
        }

        public async Task<bool> DisableBarnPlan(Guid id)
        {
            var deleteItem = await _barnplanrepo.GetByIdAsync(id);
            if (deleteItem == null)
            {
                throw new Exception("Kế khoạch không tồn tại");
            }

            var barnPlanFood = _bpfoodrepo.GetQueryable(x => x.IsActive).Where(x => x.BarnPlanId == id).ToList();
            foreach (var it in barnPlanFood)
            {
                it.IsActive = false;
            }
            await _bpfoodrepo.CommitAsync();

            var barnPlanMedicine = _bpmedicinerepo.GetQueryable(x => x.IsActive).Where(x => x.BarnPlanId == id).ToList();
            foreach (var it in barnPlanMedicine)
            {
                it.IsActive = false;
            }
            await _bpmedicinerepo.CommitAsync();

            if (deleteItem.EndDate >= DateTime.Now)
            {
                deleteItem.IsActive = false;
                deleteItem.EndDate = DateTime.Now;
                return await _barnplanrepo.CommitAsync() > 0;
            }
            deleteItem.IsActive = false;
            return await _barnplanrepo.CommitAsync() > 0;

        }
        public async Task<ViewBarnPlanResponse> GetByLiveStockCircleId(Guid id)
        {
            var barnPlanDetail = await _barnplanrepo.GetQueryable(x => x.IsActive)
                .FirstOrDefaultAsync(it => it.EndDate >= DateTime.Now && it.StartDate <= DateTime.Now && it.LivestockCircleId == id);

            if (barnPlanDetail == null)
            {
                throw new Exception("Không tìm thấy kế hoạch cho chuồng ");
            }
            ViewBarnPlanResponse result = new ViewBarnPlanResponse()
            {
                Id = barnPlanDetail.Id,
                EndDate = barnPlanDetail.EndDate,
                StartDate = barnPlanDetail.StartDate,
                Note = barnPlanDetail.Note,
                foodPlans = await _bpfoodrepo.GetQueryable(x => x.IsActive)
                                        .Where(x => x.BarnPlanId == barnPlanDetail.Id)
                                        .Include(x => x.Food)
                                        .Select(it => new Domain.Dto.Response.BarnPlan.FoodPlan()
                                        {
                                            FoodId = it.FoodId,
                                            FoodName = it.Food.FoodName,
                                            Note = it.Note,
                                            Stock = it.Stock
                                        }).ToListAsync(),
                medicinePlans = await _bpmedicinerepo.GetQueryable(x => x.IsActive)
                                        .Where(x => x.BarnPlanId == barnPlanDetail.Id)
                                        .Include(x => x.Medicine)
                                        .Select(it => new Domain.Dto.Response.BarnPlan.MedicinePlan()
                                        {
                                            MedicineId = it.MedicineId,
                                            MedicineName = it.Medicine.MedicineName,
                                            Note = it.Note,
                                            Stock = it.Stock
                                        }).ToListAsync(),
            };
            return result;

        }
        public async Task<ViewBarnPlanResponse> GetById(Guid id)
        {
            var barnPlanDetail = await _barnplanrepo.GetByIdAsync(id);
            if (barnPlanDetail == null)
            {
                throw new Exception("Không tìm thấy kế hoạch cho chuồng ");
            }
            ViewBarnPlanResponse result = new ViewBarnPlanResponse()
            {
                Id = barnPlanDetail.Id,
                EndDate = barnPlanDetail.EndDate,
                StartDate = barnPlanDetail.StartDate,
                Note = barnPlanDetail.Note,
                foodPlans = await _bpfoodrepo.GetQueryable(x => x.IsActive)
                                        .Where(x => x.BarnPlanId == id)
                                        .Include(x => x.Food)
                                        .Select(it => new Domain.Dto.Response.BarnPlan.FoodPlan()
                                        {
                                            FoodId = it.FoodId,
                                            FoodName = it.Food.FoodName,
                                            Note = it.Note,
                                            Stock = it.Stock
                                        }).ToListAsync(),
                medicinePlans = await _bpmedicinerepo.GetQueryable(x => x.IsActive)
                                        .Where(x => x.BarnPlanId == id)
                                        .Include(x => x.Medicine)
                                        .Select(it => new Domain.Dto.Response.BarnPlan.MedicinePlan()
                                        {
                                            MedicineId = it.MedicineId,
                                            MedicineName = it.Medicine.MedicineName,
                                            Note = it.Note,
                                            Stock = it.Stock
                                        }).ToListAsync(),
            };
            return result;
        }

        public async Task<bool> UpdateBarnPlan(UpdateBarnPlanRequest req)
        {
            var updateItem = await _barnplanrepo.GetByIdAsync(req.Id);

            DateTime formatedStartDate, formatedEndDate;
            if (req.IsDaily == true)
            {
                ValidTime(false, false, updateItem.StartDate, updateItem.EndDate, out formatedStartDate, out formatedEndDate);
            }
            else
            {
                ValidTime(false, (bool)(req.IsDaily == null ? false : req.IsDaily), req.StartDate, req.EndDate, out formatedStartDate, out formatedEndDate);
            }



            if (updateItem == null)
            {
                throw new Exception("Kế hoạch không tồn tại");
            }
            updateItem.Note = req.Note;
            updateItem.StartDate = formatedStartDate;
            updateItem.EndDate = formatedEndDate;

            await InsertFoodPlan(req.foodPlans, req.Id);
            await InsertMedicinePlan(req.medicinePlans, req.Id);
            _barnplanrepo.Update(updateItem);
            return await _barnplanrepo.CommitAsync() > 0;
        }


        //function
        #region common func
        protected async Task InsertFoodPlan(List<Domain.Dto.Request.BarnPlan.FoodPlan> req, Guid bpid)
        {

            // xoa bo plan cu
            var previous_Food_Plan = await _bpfoodrepo.GetQueryable().Where(it => it.BarnPlanId == bpid).ToListAsync();

            foreach (var item in previous_Food_Plan)
            {
                _bpfoodrepo.Remove(item);
            }
            await _bpfoodrepo.CommitAsync();

            if (req == null) return;
            // them plan moi
            foreach (var it in req)
            {
                var foodPlan = new BarnPlanFood()
                {
                    Note = it.Note,
                    FoodId = it.FoodId,
                    Stock = it.Stock,
                    BarnPlanId = bpid
                };

                _bpfoodrepo.Insert(foodPlan);
            }
            if (await _bpfoodrepo.CommitAsync() < 0)
            {
                throw new Exception("Không thể tạo kế hoạch thức ăn");
            }
        }
        protected async Task InsertMedicinePlan(List<Domain.Dto.Request.BarnPlan.MedicinePlan> req, Guid bpid)
        {

            // xoa bo plan cu
            var previous_Medicine_Plan = await _bpmedicinerepo.GetQueryable().Where(it => it.BarnPlanId == bpid).ToListAsync();
            foreach (var item in previous_Medicine_Plan)
            {
                _bpmedicinerepo.Remove(item);
            }
            await _bpmedicinerepo.CommitAsync();

            if (req == null) return;
            // insert plan moi
            foreach (var it in req)
            {
                var medicinePlan = new BarnPlanMedicine()
                {
                    Note = it.Note,
                    MedicineId = it.MedicineId,
                    Stock = it.Stock,
                    BarnPlanId = bpid
                };

                _bpmedicinerepo.Insert(medicinePlan);
            }
            if (await _bpmedicinerepo.CommitAsync() < 0)
            {
                throw new Exception("Không thể tạo kế hoạch thuốc");
            }
        }
        protected void ValidTime(bool isCreate, bool isDaily, DateTime? StartDate, DateTime? EndDate, out DateTime FormatedStartDate, out DateTime FormatedEndDate)
        {
            if (!isDaily && StartDate == null && EndDate == null)
            {
                throw new Exception("Phải có dữ liệu ngày");
            }
            if (isDaily)
            {
                FormatedStartDate = DateTime.Today.AddDays(1);
                FormatedEndDate = DateTime.Today.AddDays(1).AddHours(23).AddMinutes(59);

            }
            else
            {
                FormatedStartDate = ((DateTime)StartDate).Date;
                FormatedEndDate = ((DateTime)EndDate).Date.AddDays(1).AddSeconds(-1);
            }
            StartDate = FormatedStartDate;
            EndDate = FormatedEndDate;
            if (StartDate >= EndDate)
            {
                throw new Exception("Thời gian kết thúc phải sau thời gian bắt đầu");
            }
            var conflictTimeItem = _barnplanrepo.GetQueryable(x => x.IsActive)
                                    .FirstOrDefault(it => (EndDate <= it.EndDate) && (StartDate >= it.StartDate));
            if (conflictTimeItem != null && isCreate)
            {
                throw new ArgumentException("Đã đặt kế hoạch cho ngày này id: " + conflictTimeItem.Id);
            }

        }


    }


    #endregion

}

