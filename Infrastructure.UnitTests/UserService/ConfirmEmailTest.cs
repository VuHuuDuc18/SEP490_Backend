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
    public class ConfirmEmailTest
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IdentityContext> _contextMock;
        private readonly Mock<IOptions<JWTSettings>> _jwtSettingsMock;
        private readonly Infrastructure.Services.Implements.UserService _userService;

        public ConfirmEmailTest()
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

            // Cấu hình HttpContext với ClaimsPrincipal
            var userId = "2eaedaf7-afd6-4340-53de-08ddc0fec23a"; 
            var claims = new List<Claim> { new Claim("uid", userId) };
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
        public async Task ConfirmEmailAsync_ValidUserAndCode_ReturnsSuccessResponse()
        {
            // Arrange
            var userId = "2eaedaf7-afd6-4340-53de-08ddc0fec23a";
            var user = new User { Id = Guid.Parse(userId), Email = "luongcongduy826@gmail.com", UserName = "luongcongduy", EmailConfirmed = false };
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("valid-code"));
            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.ConfirmEmailAsync(It.Is<User>(u => u.Id == user.Id), It.Is<string>(c => c == "valid-code"))).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.ConfirmEmailAsync(userId, code);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal(userId, result.Data);
            Assert.Equal("Tài khoản đã được xác thực thành công.", result.Message);
        }

        [Fact]
        public async Task ConfirmEmailAsync_UserNotFound_ReturnsErrorResponse()
        {
            // Arrange
            var userId = "11111111-1111-1111-1111-111111111111";
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("valid-code"));
            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync((User)null);

            // Act
            var result = await _userService.ConfirmEmailAsync(userId, code);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Null(result.Data);
            Assert.Equal("Không tìm thấy tài khoản.", result.Message);
        }


        [Fact]
        public async Task ConfirmEmailAsync_InvalidCode_ReturnsErrorResponse()
        {
            // Arrange
            var userId = "2eaedaf7-afd6-4340-53de-08ddc0fec23a";
            var user = new User { Id = Guid.Parse(userId), Email = "luongcongduy826@gmail.com", UserName = "luongcongduy", EmailConfirmed = false };
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("invalid-code"));
            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.ConfirmEmailAsync(It.Is<User>(u => u.Id == user.Id), It.Is<string>(c => c == "invalid-code"))).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Mã xác thực không hợp lệ" }));

            // Act
            var result = await _userService.ConfirmEmailAsync(userId, code);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Null(result.Data);
            Assert.Equal("Xác thực tài khoản không thành công.", result.Message);
            Assert.NotNull(result.Errors);
            Assert.Contains("Mã xác thực không hợp lệ", result.Errors);
        }

        [Fact]
        public async Task ConfirmEmailAsync_NullUserId_ReturnsErrorResponse()
        {
            // Arrange
            string userId = null;
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("valid-code"));

            // Act
            var result = await _userService.ConfirmEmailAsync(userId, code);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Null(result.Data);
            Assert.Equal("The UserId field is a require.", result.Message);
        }

        [Fact]
        public async Task ConfirmEmailAsync_NullCode_ReturnsErrorResponse()
        {
            // Arrange
            var userId = "2eaedaf7-afd6-4340-53de-08ddc0fec23a";
            string code = null;
            var user = new User { Id = Guid.Parse(userId), Email = "luongcongduy826@gmail.com", UserName = "luongcongduy", EmailConfirmed = false };
            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            // Không cần mock ConfirmEmailAsync vì code null sẽ được xử lý trước

            // Act
            var result = await _userService.ConfirmEmailAsync(userId, code);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Null(result.Data);
            Assert.Equal("The Code field is a require.", result.Message);
        }
    }
}