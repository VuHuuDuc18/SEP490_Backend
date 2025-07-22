using Application.Wrappers;
using Domain.Dto.Request;
using Domain.Dto.Response;
using Domain.Dto.Response.Barn;
using Domain.Dto.Response.BarnPlan;
using Domain.Dto.Response.Bill;
using Domain.Dto.Response.LivestockCircle;
using Domain.Dto.Response.User;
using Domain.DTOs.Request.Order;
using Domain.DTOs.Response.Order;
using Domain.Helper;
using Domain.Helper.Constants;
using Domain.IServices;
using Entities.EntityModel;
using Infrastructure.DBContext;
using Infrastructure.Extensions;
using Infrastructure.Repository;
using MailKit.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infrastructure.Services.Implements
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<User> _userRepository;
        private UserManager<User> _userManager;
        private RoleManager<Role> _roleManager;
        private IRepository<Role> _roleRepository;
        private readonly IRepository<LivestockCircle> _livestockCircleRepository;
        private readonly IRepository<Breed> _breedRepository;
        private readonly IRepository<ImageLivestockCircle> _imageLivestockCircleRepository;
        private readonly IRepository<BreedCategory> _breedCategoryRepository;
        private readonly Guid _currentUserId;
        private readonly LCFMSDBContext _dbContext;
        public OrderService
        (
            IRepository<Order> orderRepository,
            IHttpContextAccessor httpContextAccessor,
            IRepository<LivestockCircle> livestockCircleRepository,
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IRepository<User> userRepository,
            IRepository<Breed> breedrepo,
            IRepository<BreedCategory> bcrepo,
            IRepository<ImageLivestockCircle> imageLivestockCircleRepository,
            IRepository<Role> roleRepository,
            DbContext dbContext
        )
        {
            _dbContext = (LCFMSDBContext)dbContext;
            _userRepository = userRepository;
            _orderRepository = orderRepository;
            _breedRepository = breedrepo;
            _livestockCircleRepository = livestockCircleRepository;
            _breedCategoryRepository = bcrepo;
            _userManager = userManager;
            _roleManager = roleManager;
            _imageLivestockCircleRepository = imageLivestockCircleRepository;
            _roleRepository = roleRepository;

            // Lấy current user từ JWT token claims
            _currentUserId = Guid.Empty;
            var currentUser = httpContextAccessor.HttpContext?.User;
            if (currentUser != null)
            {
                var userIdClaim = currentUser.FindFirst("uid")?.Value;
                if (!string.IsNullOrEmpty(userIdClaim))
                {
                    _currentUserId = Guid.Parse(userIdClaim);
                }
            }
        }

        public async Task<Response<string>> CustomerCreateOrder(CreateOrderRequest request, CancellationToken cancellationToken = default)
        {
            if (_currentUserId == Guid.Empty)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Hãy đăng nhập và thử lại",
                    Errors = new List<string>() { "Hãy đăng nhập và thử lại" }
                };
            }
            if (request.GoodUnitStock <= 0 || request.BadUnitStock < 0)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Số lượng con tốt hoặc con xấu phải lớn hơn 0",
                    Errors = new List<string>() { "Số lượng con tốt hoặc con xấu phải lớn hơn 0" }
                };
            }
            if (request.PickupDate < DateTime.UtcNow)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Ngày lấy hàng phải lớn hơn hoặc bằng ngày hiện tại",
                    Errors = new List<string>() { "Ngày lấy hàng phải lớn hơn hoặc bằng ngày hiện tại" }
                };
            }
            try
            {
                //Kiểm tra đơn hàng đã tồn tại chưa
                var existingOrder = _orderRepository.GetQueryable(x => x.CustomerId == _currentUserId && x.LivestockCircleId == request.LivestockCircleId && x.Status!=OrderStatus.CANCELLED);
                if (!existingOrder.IsNullOrEmpty())
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Đã tồn tại đơn hàng với chuồng nuôi hiện tại. Vui lòng kiểm tra lại các đơn hàng của bạn.",
                    };
                }
                // Lấy danh sách các Sale Staff và tổng số đơn hàng mỗi Sale đang xử lý
                var Orders = _orderRepository.GetQueryable();
                //var Staffs = _userManager.GetUsersInRoleAsync(RoleConstant.SalesStaff).Result.AsQueryable();
                var Staffs = _dbContext.Set<UserRole>()
                    .Include(x => x.User)
                    .Include(x => x.Role)
                    .Where(x => x.User.IsActive && x.Role.Name == RoleConstant.SalesStaff)
                    .Select(x => x.User); // lấy danh sách người dùng (User) là nhân viên sale
                var saleStaffs = Staffs
                    .GroupJoin(
                        Orders,
                        staff => staff.Id,
                        order => order.SaleStaffId,
                        (staff, orderGroup) => new { staff, orderGroup }
                    )
                    .Select(x => new
                    {
                        SaleStaffId = x.staff.Id,
                        SaleStaffName = x.staff.FullName,
                        TotalOrders = x.orderGroup.Count()
                    })
                    .OrderByDescending(x => x.TotalOrders)
                    .ToList();
                if (saleStaffs.IsNullOrEmpty())
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Không có nhân viên sale nào xử lý đơn hàng.",
                    };
                }
                var saleStaffId = saleStaffs.FirstOrDefault().SaleStaffId;
                //Tạo đơn hàng mới
                var order = new Order()
                {
                    CustomerId = _currentUserId,
                    LivestockCircleId = request.LivestockCircleId,
                    GoodUnitStock = request.GoodUnitStock,
                    BadUnitStock = request.BadUnitStock,
                    Status = OrderStatus.PENDING,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = _currentUserId,
                    IsActive = true,
                    PickupDate = request.PickupDate,
                    SaleStaffId = saleStaffId
                };

                var livestockCircle = await _livestockCircleRepository.GetByIdAsync(request.LivestockCircleId);
                if (livestockCircle == null)
                {
                    return new Response<string>("Chuồng nuôi không khả dụng. Vui lòng thử lại sau.")
                    {
                        Errors = new List<string>()
                        {
                            "Không tìm thấy chu kì nuôi. ID: "+ request.LivestockCircleId
                        }
                    };
                }
                if (order.GoodUnitStock > livestockCircle.GoodUnitNumber || order.BadUnitStock > livestockCircle.BadUnitNumber)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Số lượng con tốt hoặc con xấu vượt quá số lượng con trong chu kì nuôi.",
                        Errors = new List<string>() { "Số lượng con tốt hoặc con xấu vượt quá số lượng con trong chu kì nuôi." }
                    };
                }
                if (((DateTime)order.PickupDate - (DateTime)livestockCircle.ReleaseDate).Days > 3)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Ngày lấy hàng phải trong vòng 3 ngày kể từ ngày xuất chuồng",
                        Errors = new List<string>() { "Ngày lấy hàng phải trong vòng 3 ngày kể từ ngày xuất chuồng" }
                    };
                }
                _orderRepository.Insert(order);
                await _orderRepository.CommitAsync(cancellationToken);
                return new Response<string>()
                {
                    Succeeded = true,
                    Message = "Tạo đơn hàng thành công",
                    Data = "Đơn hàng đã được tạo thành công. ID: " + order.Id
                };
            }
            catch (Exception ex)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi tạo đơn hàng",
                    Errors = new List<string>() { ex.Message }
                };
            }
        }

        public async Task<Response<OrderResponse>> CustomerOrderDetails(Guid OrderId, CancellationToken cancellationToken = default)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(OrderId);
                if (order == null || !order.IsActive)
                {
                    return new Response<OrderResponse>("Đơn hàng không tồn tại hoặc đã bị xóa.");
                }
                var livestockCircle = await _livestockCircleRepository.GetByIdAsync(order.LivestockCircleId);
                var images = await _imageLivestockCircleRepository.GetQueryable(x => x.IsActive && x.LivestockCircleId == order.LivestockCircleId)
                    .Select(x => x.ImageLink).ToListAsync();
                var customer = await _userManager.FindByIdAsync(_currentUserId.ToString());
                var result = new OrderResponse()
                {
                    Id = order.Id,
                    CustomerId = order.CustomerId,
                    LivestockCircleId = order.LivestockCircleId,
                    GoodUnitStock = order.GoodUnitStock,
                    BadUnitStock = order.BadUnitStock,
                    TotalBill = order.TotalBill,
                    Status = order.Status,
                    CreateDate = order.CreatedDate,
                    PickupDate = order.PickupDate,
                    BreedName = order.LivestockCircle.Breed.BreedName,
                    BreedCategory = order.LivestockCircle.Breed.BreedCategory.Name
                };
                result.LivestockCircle = AutoMapperHelper.AutoMap<LivestockCircle, ReleasedLivetockDetail>(livestockCircle);
                result.LivestockCircle.ImageLinks = images;
                result.Customer = AutoMapperHelper.AutoMap<User, UserItemResponse>(customer);
                result.Barn = AutoMapperHelper.AutoMap<Barn, BarnResponse>(livestockCircle.Barn);
                return new Response<OrderResponse>()
                {
                    Succeeded = true,
                    Message = "Xem chi tiết đơn hàng thành công",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new Response<OrderResponse>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi xem chi tiết đơn hàng",
                    Errors = new List<string>() { ex.Message }
                };
            }
        }

        public async Task<Response<string>> CustomerUpdateOrder(UpdateOrderRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(request.OrderId);
                if (order == null || !order.IsActive)
                {
                    return new Response<string>("Đơn hàng không tồn tại hoặc đã bị xóa.");
                }
                if (order.CustomerId != _currentUserId)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Bạn không thể sửa đơn hàng này.",
                        Errors = new List<string>() { "Không thể sửa đơn hàng của người khác." }
                    };
                }
                if (order.Status != OrderStatus.PENDING)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Đơn hàng không thể cập nhật với trạng thái hiện tại.",
                        Errors = new List<string>() { "Đơn hàng không thể cập nhật với trạng thái hiện tại - " + order.Status }
                    };
                }
                if (request.GoodUnitStock != null)
                {
                    if (request.GoodUnitStock > order.LivestockCircle.GoodUnitNumber)
                    {
                        return new Response<string>("Số lượng con tốt vượt quá số lượng con trong chuồng nuôi");
                    }
                }
                if (request.BadUnitStock != null)
                {
                    if (request.BadUnitStock > order.LivestockCircle.BadUnitNumber)
                    {
                        return new Response<string>("Số lượng con xấu vượt quá số lượng con trong chuồng nuôi");
                    }
                }
                if (request.PickupDate != null)
                {
                    if (request.PickupDate < DateTime.UtcNow)
                    {
                        return new Response<string>("Ngày lấy hàng phải lớn hơn hoặc bằng ngày hiện tại");
                    }
                    if (((DateTime)request.PickupDate - (DateTime)order.LivestockCircle.ReleaseDate).Days > 3)
                    {
                        return new Response<string>("Ngày lấy hàng phải trong vòng 3 ngày kể từ ngày xuất chuồng");
                    }
                }
                order.GoodUnitStock = request.GoodUnitStock ?? order.GoodUnitStock;
                order.BadUnitStock = request.BadUnitStock ?? order.BadUnitStock;
                order.PickupDate = request.PickupDate ?? order.PickupDate;
                order.UpdatedDate = DateTime.UtcNow;
                order.UpdatedBy = _currentUserId;
                _orderRepository.Update(order);
                await _orderRepository.CommitAsync(cancellationToken);
                return new Response<string>()
                {
                    Succeeded = true,
                    Message = "Cập nhật đơn hàng thành công",
                    Data = "Đơn hàng đã được cập nhật thành công. ID: " + order.Id
                };
            }
            catch (Exception ex)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi cập nhật đơn hàng",
                    Errors = new List<string>() { ex.Message }
                };
            }
        }

        public async Task<Response<string>> CustomerCancelOrder(Guid OrderId, CancellationToken cancellationToken = default)
        {

            try
            {
                var order = await _orderRepository.GetByIdAsync(OrderId);
                if (order == null || !order.IsActive)
                {
                    return new Response<string>("Đơn hàng không tồn tại hoặc đã bị xóa.");
                }
                if (order.CustomerId != _currentUserId)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Bạn không thể hủy đơn hàng này.",
                        Errors = new List<string>() { "Không thể hủy đơn hàng của người khác." }
                    };
                }
                if (order.Status != OrderStatus.PENDING)
                {
                    return new Response<string>()
                    {
                        Succeeded = false,
                        Message = "Đơn hàng không thể hủy với trạng thái hiện tại.",
                        Errors = new List<string>() { "Đơn hàng không thể hủy với trạng thái hiện tại - " + order.Status }
                    };
                }
                order.Status = OrderStatus.CANCELLED;
                order.UpdatedDate = DateTime.UtcNow;
                order.UpdatedBy = _currentUserId;
                _orderRepository.Update(order);
                await _orderRepository.CommitAsync(cancellationToken);
                return new Response<string>()
                {
                    Succeeded = true,
                    Message = "Hủy đơn hàng thành công",
                    Data = "Đơn hàng đã được hủy thành công. ID: " + order.Id
                };
            }
            catch (Exception ex)
            {
                return new Response<string>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi hủy đơn hàng",
                    Errors = new List<string>() { ex.Message }
                };
            }
        }

        public async Task<Response<List<OrderResponse>>> CustomerGetAllOrders(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentUserId == Guid.Empty)
                {
                    return new Response<List<OrderResponse>>()
                    {
                        Succeeded = false,
                        Message = "Hãy đăng nhập và thử lại",
                        Errors = new List<string>() { "Hãy đăng nhập và thử lại" }
                    };
                }
                var orders = await _orderRepository.GetQueryable(x => x.CustomerId == _currentUserId && x.IsActive).ToListAsync();
                var result = orders.Select(x => new OrderResponse()
                {
                    Id = x.Id,
                    CustomerId = x.CustomerId,
                    LivestockCircleId = x.LivestockCircleId,
                    GoodUnitStock = x.GoodUnitStock,
                    BadUnitStock = x.BadUnitStock,
                    TotalBill = x.TotalBill,
                    Status = x.Status,
                    CreateDate = x.CreatedDate,
                    PickupDate = x.PickupDate,
                    BreedName = x.LivestockCircle.Breed.BreedName,
                    BreedCategory = x.LivestockCircle.Breed.BreedCategory.Name,
                    Barn = AutoMapperHelper.AutoMap<Barn, BarnResponse>(x.LivestockCircle.Barn)
                }).OrderByDescending(x => x.CreateDate).ToList();
                return new Response<List<OrderResponse>>(result, "Lấy danh sách đơn hàng thành công");

            }
            catch (Exception ex)
            {
                return new Response<List<OrderResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách đơn hàng",
                    Errors = new List<string>() { ex.Message }
                };
            }
        }

        public async Task<Response<PaginationSet<OrderResponse>>> CustomerGetPagiantionList(ListingRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                    return new Response<PaginationSet<OrderResponse>>("Yêu cầu không được null.");
                if (request.PageIndex < 1 || request.PageSize < 1)
                    return new Response<PaginationSet<OrderResponse>>("PageIndex và PageSize phải lớn hơn 0.");

                var validFields = typeof(OrderResponse).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                    return new Response<PaginationSet<OrderResponse>>($"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}")
                    {
                        Errors = new List<string>()
                        {
                            $"Trường hợp lệ: {string.Join(",",validFields)}"
                        }
                    };
                if (!validFields.Contains(request.Sort?.Field))
                {
                    return new Response<PaginationSet<OrderResponse>>($"Trường sắp xếp không hợp lệ: {request.Sort?.Field}")
                    {
                        Errors = new List<string>()
                        {
                            $"Trường hợp lệ: {string.Join(",",validFields)}"
                        }
                    };
                }

                var orders = _orderRepository.GetQueryable(x => x.IsActive && x.CustomerId == _currentUserId)
                    .Select(x => new OrderResponse()
                    {
                        Id = x.Id,
                        CustomerId = x.CustomerId,
                        LivestockCircleId = x.LivestockCircleId,
                        GoodUnitStock = x.GoodUnitStock,
                        BadUnitStock = x.BadUnitStock,
                        TotalBill = x.TotalBill,
                        Status = x.Status,
                        CreateDate = x.CreatedDate,
                        PickupDate = x.PickupDate,
                        BreedName = x.LivestockCircle.Breed.BreedName,
                        BreedCategory = x.LivestockCircle.Breed.BreedCategory.Name,
                        Barn = AutoMapperHelper.AutoMap<Barn,BarnResponse>(x.LivestockCircle.Barn)
                    });
                
                if (request.SearchString?.Any() == true)
                    orders = orders.SearchString(request.SearchString);

                if (request.Filter?.Any() == true)
                    orders = orders.Filter(request.Filter);

                var paginationResult = await orders.Pagination(request.PageIndex, request.PageSize, request.Sort);

                return new Response<PaginationSet<OrderResponse>>(paginationResult, "Lấy dữ liệu thành công.");
            }
            catch (Exception ex)
            {
                return new Response<PaginationSet<OrderResponse>>()
                {
                    Succeeded = false,
                    Message = "Lỗi khi lấy danh sách đơn hàng",
                    Errors = new List<string>() { ex.Message }
                };
            }

        }

        public async Task<Response<StatisticsOrderResponse>> GetStatisticData(StatisticsOrderRequest request)
        {
            DateTime froms, to;
            try
            {
                (froms, to) = DateTimeExcutor.TimeRangeSetting(request);
            }
            catch (Exception ex)
            {
                return new Response<StatisticsOrderResponse>(ex.Message);
            }

            var LivestockCircles = _livestockCircleRepository.GetQueryable();
            var Orders = _orderRepository.GetQueryable();
            var Breeds = _breedRepository.GetQueryable();
            var BreedCategories = _breedCategoryRepository.GetQueryable();

            var query = from l in LivestockCircles
                        join o in Orders on l.Id equals o.LivestockCircleId
                        join b in Breeds on l.BreedId equals b.Id
                        join bc in BreedCategories on b.BreedCategoryId equals bc.Id
                        where o.CreatedDate <= to && o.CreatedDate >= froms
                        group o by new { b.Id, b.BreedName, bc.Name } into g
                        select new OrderItem
                        {
                            BreedId = g.Key.Id,
                            BreedName = g.Key.BreedName,
                            GoodUnitStockSold = g.Sum(x => x.GoodUnitStock),
                            AverageGoodUnitPrice = g.Average(x => x.GoodUnitPrice ?? 0),
                            BadUnitStockSold = g.Sum(x => x.BadUnitStock),
                            AverageBadUnitPrice = g.Average(x => x.BadUnitPrice ?? 0),
                            BreedCategoryName = g.Key.Name,
                            Revenue = g.Sum(x => x.TotalBill ?? 0)
                        };
            var ListItem = await query.ToListAsync();
            var result = new StatisticsOrderResponse()
            {
                Datas = ListItem,
                TotalRevenue = ListItem.Sum(x => x.Revenue ?? 0),
                TotalBadUnitStockSold = ListItem.Sum(x => x.BadUnitStockSold ?? 0),
                TotalGoodUnitStockSold = ListItem.Sum(x => x.GoodUnitStockSold ?? 0),
            };
            return new Response<StatisticsOrderResponse>()
            {
                Succeeded = true,
                Data = result
            };
        }

        public async Task<Response<PaginationSet<OrderResponse>>> SaleGetAllOrder(ListingRequest request)
        {
            try
            {
                if (request == null)
                    return new Response<PaginationSet<OrderResponse>>("Yêu cầu không được null.");
                if (request.PageIndex < 1 || request.PageSize < 1)
                    return new Response<PaginationSet<OrderResponse>>("PageIndex và PageSize phải lớn hơn 0.");

                var validFields = typeof(BillItem).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                    return new Response<PaginationSet<OrderResponse>>($"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}");



                var query = _orderRepository.GetQueryable(x => x.IsActive);

                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                var result = await query.Select(x => new OrderResponse()
                {
                    Id = x.Id,
                    CustomerId = x.CustomerId,
                    LivestockCircleId = x.LivestockCircleId,
                    GoodUnitStock = x.GoodUnitStock,
                    BadUnitStock = x.BadUnitStock,
                    TotalBill = x.TotalBill,
                    Status = x.Status,
                    CreateDate = x.CreatedDate,
                    PickupDate = x.PickupDate,
                    BreedName = x.LivestockCircle.Breed.BreedName,
                    BreedCategory = x.LivestockCircle.Breed.BreedCategory.Name
                }).Pagination(request.PageIndex, request.PageSize, request.Sort);

                return new Response<PaginationSet<OrderResponse>>()
                {
                    Succeeded = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new Response<PaginationSet<OrderResponse>>($"Lỗi khi lấy danh sách: {ex.Message}");
            }

        }

        public async Task<Response<bool>> ApproveOrder(ApproveOrderRequest request)
        {
            try
            {
                var orderItem = await _orderRepository.GetByIdAsync(request.OrderId);
                if (orderItem == null)
                {
                   return new Response<bool>("Không tìm thấy đơn ");
                }
                orderItem.Status = StatusConstant.APPROVED;
                orderItem.GoodUnitPrice = request.GoodUnitPrice;
                orderItem.BadUnitPrice = request.BadUnitPrice;
                orderItem.TotalBill = request.GoodUnitPrice * orderItem.GoodUnitStock + request.BadUnitPrice * orderItem.BadUnitStock;

                _orderRepository.Update(orderItem);
                await _orderRepository.CommitAsync();
                var livestockCircleDetail = await _livestockCircleRepository.GetByIdAsync(orderItem.LivestockCircleId);
                livestockCircleDetail.GoodUnitNumber -= orderItem.GoodUnitStock;
                livestockCircleDetail.BadUnitNumber -= orderItem.BadUnitStock;
                if (livestockCircleDetail.GoodUnitNumber == 0 && livestockCircleDetail.BadUnitNumber == 0)
                {
                    livestockCircleDetail.Status = StatusConstant.DONESTAT;
                }
                _livestockCircleRepository.Update(livestockCircleDetail);

                await _livestockCircleRepository.CommitAsync();
                return new Response<bool>()
                {
                    Succeeded = true,
                    Message = "Cập nhật thành công"
                };

            }
            catch (Exception ex)
            {
                return new Response<bool>()
                {
                    Succeeded = false,
                    Message = "Cập nhật thất bại"
                };
            }
        }

        public async Task<Response<PaginationSet<OrderResponse>>> WorkerGetallOrder(ListingRequest request)
        {
            try
            {
                if (request == null)
                {
                    return new Response<PaginationSet<OrderResponse>>()
                    {
                        Succeeded = false,
                        Message = "Yêu cầu không được để trống",
                        Errors = new List<string> { "Yêu cầu không được để trống" }
                    };
                }

                if (request.PageIndex < 1 || request.PageSize < 1)
                {
                    return new Response<PaginationSet<OrderResponse>>()
                    {
                        Succeeded = false,
                        Message = "PageIndex và PageSize phải lớn hơn 0",
                        Errors = new List<string> { "PageIndex và PageSize phải lớn hơn 0" }
                    };
                }

                var validFields = typeof(BillItem).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var invalidFields = request.Filter?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
    .Select(f => f.Field).ToList() ?? new List<string>();
                var invalidFieldsSearch = request.SearchString?.Where(f => !string.IsNullOrEmpty(f.Field) && !validFields.Contains(f.Field))
                    .Select(f => f.Field).ToList() ?? new List<string>();
                if (invalidFields.Any())
                {
                    return new Response<PaginationSet<OrderResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường lọc không hợp lệ: {string.Join(", ", invalidFields)}",
                        Errors = new List<string> { $"Trường hợp lệ: {string.Join(", ", validFields)}" }
                    };
                }

                if (invalidFieldsSearch.Any())
                {
                    return new Response<PaginationSet<OrderResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường tìm kiếm không hợp lệ: {string.Join(", ", invalidFields)}",
                        Errors = new List<string> { $"Trường hợp lệ: {string.Join(", ", validFields)}" }
                    };
                }


                if (!validFields.Contains(request.Sort?.Field))
                {
                    return new Response<PaginationSet<OrderResponse>>()
                    {
                        Succeeded = false,
                        Message = $"Trường sắp xếp không hợp lệ: {request.Sort?.Field}",
                        Errors = new List<string> { $"Trường hợp lệ: {string.Join(", ", validFields)}" }
                    };
                }


                var query = _orderRepository.GetQueryable(x => x.IsActive).Where(x => x.LivestockCircle.Barn.WorkerId == _currentUserId);

                if (request.SearchString?.Any() == true)
                    query = query.SearchString(request.SearchString);

                if (request.Filter?.Any() == true)
                    query = query.Filter(request.Filter);

                var result = await query.Select(x => new OrderResponse()
                {
                    Id = x.Id,
                    CustomerId = x.CustomerId,
                    LivestockCircleId = x.LivestockCircleId,
                    GoodUnitStock = x.GoodUnitStock,
                    BadUnitStock = x.BadUnitStock,
                    TotalBill = x.TotalBill,
                    Status = x.Status,
                    CreateDate = x.CreatedDate,
                    PickupDate = x.PickupDate,
                    BreedName = x.LivestockCircle.Breed.BreedName,
                    BreedCategory = x.LivestockCircle.Breed.BreedCategory.Name
                }).Pagination(request.PageIndex, request.PageSize, request.Sort);

                return new Response<PaginationSet<OrderResponse>>()
                {
                    Succeeded = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new Response<PaginationSet<OrderResponse>>($"Lỗi khi lấy danh sách: {ex.Message}");
            }
        }
    }
}
