using Domain.Dto.Response.BarnPlan;
using Domain.Services.Interfaces;
using Entities.EntityModel;
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
            if (req.IsDaily == true)
            {
                req.StartDate = DateTime.Today.AddDays(1);
                req.EndDate = DateTime.Today.AddDays(1).AddHours(23).AddMinutes(59);
            }
            // insert barn
            BarnPlan barnPlanDetail = new BarnPlan()
            {
                Id = Guid.NewGuid(),               
                Note = req.Note,
                StartDate = (DateTime)req.StartDate,
                EndDate = (DateTime)req.EndDate
            };

            _barnplanrepo.Insert(barnPlanDetail);
            if (await _barnplanrepo.CommitAsync() < 0)
            {
                throw new Exception("Không thể tạo kế hoạch");
            }
            
            
            // insert food plan
            foreach(var it in req.foodPlans)
            {
                var foodPlan = new BarnPlanFood()
                {
                    Note = it.Note,
                    FoodId = it.FoodId,
                    Stock = it.Stock,
                    BarnPlanId = barnPlanDetail.Id
                };

                _bpfoodrepo.Insert(foodPlan);
            }
            if (await _bpfoodrepo.CommitAsync() < 0)
            {
                throw new Exception("Không thể tạo kế hoạch thức ăn");
            }
            // insert medicine plan
            foreach (var it in req.medicinePlans)
            {
                var medicinePlan = new BarnPlanMedicine()
                {
                    Note = it.Note,
                    MedicineId = it.MedicineId,
                    Stock = it.Stock,
                    BarnPlanId = barnPlanDetail.Id
                };

                _bpmedicinerepo.Insert(medicinePlan);
            }
            if (await _bpmedicinerepo.CommitAsync() < 0)
            {
                throw new Exception("Không thể tạo kế hoạch thuốc");
            }

            // create response
            
            return true;
        }

        public async Task<ViewBarnPlanResponse> GetById(Guid id)
        {
            var barnPlanDetail = await _barnplanrepo.GetById(id);
            if (barnPlanDetail == null)
            {
                throw new Exception("Không tìm thấy kế hoạch cho chuồng ");
            }
            ViewBarnPlanResponse result = new ViewBarnPlanResponse()
            {
                Id = id,
                EndDate = barnPlanDetail.EndDate,
                StartDate = barnPlanDetail.StartDate,
                Note = barnPlanDetail.Note,
                foodPlans = await _bpfoodrepo.GetQueryable(x => x.IsActive)
                                        .Where(x=>x.BarnPlanId == id)
                                        .Select(it => new FoodPlan()
                                        {
                                            FoodName = it.Food.FoodName,
                                            Note = it.Note,
                                            Stock = it.Stock
                                        }).ToListAsync(),
                medicinePlans = await _bpmedicinerepo.GetQueryable(x => x.IsActive)
                                        .Where(x => x.BarnPlanId == id)
                                        .Select(it => new MedicinePlan()
                                        {
                                            MedicineName = it.Medicine.MedicineName,
                                            Note = it.Note,
                                            Stock = it.Stock
                                        }).ToListAsync(),
            };
            return result;   
        }
    }
}
