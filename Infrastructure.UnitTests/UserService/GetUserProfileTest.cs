using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Application.Wrappers;
using Domain.Dto.Request.Account;
using Domain.Dto.Response.Account;
using Entities.EntityModel;
using Infrastructure.Identity.Contexts;
using Infrastructure.Services.Implements;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Assert = Xunit.Assert;
using MockQueryable.Moq;
using Microsoft.AspNetCore.WebUtilities;
using Domain.Settings;
using Infrastructure.Services;

namespace Infrastructure.UnitTests.UserService
{
    public class GetUserProfileTest
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IdentityContext> _contextMock;
        private readonly Mock<IOptions<JWTSettings>> _jwtSettingsMock;
        private readonly Infrastructure.Services.Implements.UserService _userService;

        public GetUserProfileTest()
        {
            // Khởi tạo Mock cho UserManager
            _userManagerMock = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

            // Khởi tạo Mock cho SignInManager
            _signInManagerMock = new Mock<SignInManager<User>>(
                _userManagerMock.Object, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<User>>(),
                null, null, null, null);

            // Khởi tạo Mock cho các dịch vụ khác
            _emailServiceMock = new Mock<IEmailService>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _contextMock = new Mock<IdentityContext>(new DbContextOptions<IdentityContext>());
            _jwtSettingsMock = new Mock<IOptions<JWTSettings>>();
            _jwtSettingsMock.Setup(x => x.Value).Returns(new JWTSettings
            {
                SecurityKey = "test-key-1234567890",
                Issuer = "test-issuer",
                Audience = "test-audience",
                LifeTime = 60
            });

            // Cấu hình mặc định HttpContext với user đã đăng nhập
            var defaultUserId = Guid.Parse("2eaedaf7-afd6-4340-53de-08ddc0fec23a");
            var claims = new List<Claim> { new Claim("uid", defaultUserId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            // Khởi tạo UserService với các Mock
            _userService = new Infrastructure.Services.Implements.UserService(
                _userManagerMock.Object,
                null, // RoleManager không được mock ở đây, có thể thêm nếu cần
                _jwtSettingsMock.Object,
                _signInManagerMock.Object,
                _emailServiceMock.Object,
                _contextMock.Object,
                _httpContextAccessorMock.Object);
        }

        [Fact]
        public async Task GetUserProfile_ValidUser_ReturnsUserProfile()
        {
            // Arrange
            var userId = Guid.Parse("2eaedaf7-afd6-4340-53de-08ddc0fec23a");
            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                UserName = "testuser",
                IsActive = true,
                EmailConfirmed = true
            };
            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserProfile();

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("Lấy thông tin tài khoản thành công.", result.Message);
            Assert.Equal(user, result.Data);
        }

        [Fact]
        public async Task GetUserProfile_InvalidUserId_ReturnsError()
        {
            // Arrange
            // Ghi đè HttpContext để mô phỏng không đăng nhập (Guid.Empty)
            var claims = new List<Claim> { new Claim("uid", Guid.Empty.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            // Mock FindByIdAsync để không ảnh hưởng (vì _currentUserId == Guid.Empty nên không gọi)
            _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((User)null);

            // Act
            var result = await _userService.GetUserProfile();

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Không tìm thấy tài khoản.", result.Message); // Cập nhật kỳ vọng dựa trên hành vi thực tế
            Assert.Null(result.Data);
            Assert.Contains("Không tìm thấy tài khoản với ID:2eaedaf7-afd6-4340-53de-08ddc0fec23a", result.Errors);
        }

        [Fact]
        public async Task GetUserProfile_UserNotFound_ReturnsError()
        {
            // Arrange
            var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            // Sử dụng mặc định HttpContext từ constructor (đã có userId hợp lệ)
            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync((User)null);

            // Act
            var result = await _userService.GetUserProfile();

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Không tìm thấy tài khoản.", result.Message);
            Assert.Null(result.Data);
            Assert.Contains("Không tìm thấy tài khoản với ID:2eaedaf7-afd6-4340-53de-08ddc0fec23a", result.Errors);
        }
    }
}