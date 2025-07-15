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
using Domain.Settings;
using Infrastructure.Services;

namespace Infrastructure.UnitTests.UserService
{
    public class LoginTest
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IdentityContext> _contextMock;
        private readonly Mock<IOptions<JWTSettings>> _jwtSettingsMock;
        private readonly Infrastructure.Services.Implements.UserService _userService;

        public LoginTest()
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
            var userId = Guid.NewGuid().ToString();
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
        public async Task LoginAsync_ValidCredentials_ReturnsSuccessResponse()
        {
            // Arrange
            var request = new AuthenticationRequest { Email = "luongcongduy826@gmail.com", Password = "Admin@123" };
            var user = new User { Id = Guid.NewGuid(), Email = "luongcongduy826@gmail.com", UserName = "luongcongduy", IsActive = true, EmailConfirmed = true };
            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);
            _signInManagerMock.Setup(x => x.PasswordSignInAsync(user.UserName, request.Password, false, false)).ReturnsAsync(SignInResult.Success);
            _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
            _userManagerMock.Setup(x => x.GetClaimsAsync(user)).ReturnsAsync(new List<Claim>());

            // Mock RefreshTokens như một DbSet
            var refreshTokenDbSetMock = new Mock<DbSet<RefreshToken>>();
            refreshTokenDbSetMock
                .Setup(x => x.Add(It.IsAny<RefreshToken>()))
                .Verifiable(); // Đảm bảo được gọi

            _contextMock.Setup(x => x.RefreshTokens).Returns(refreshTokenDbSetMock.Object);
            _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _userService.LoginAsync(request, "127.0.0.1");

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Equal("Đăng nhập thành công.", result.Message);
            Assert.Equal(user.Id, result.Data.Id);
            Assert.Equal(user.Email, result.Data.Email);
            Assert.NotNull(result.Data.JWToken);
            Assert.NotNull(result.Data.RefreshToken);
        }

        [Fact]
        public async Task LoginAsync_UserNotFound_ReturnsErrorResponse()
        {
            // Arrange
            var request = new AuthenticationRequest { Email = "nonexistent@gmail.com", Password = "Admin@123" };
            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((User)null);

            // Act
            var result = await _userService.LoginAsync(request, "127.0.0.1");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Null(result.Data);
            Assert.Contains("Không tìm thấy tài khoản với email nonexistent@gmail.com.", result.Message);
        }

        [Fact]
        public async Task LoginAsync_InactiveUser_ReturnsErrorResponse()
        {
            // Arrange
            var request = new AuthenticationRequest { Email = "Inactive@gmail.com", Password = "Admin@123" };
            var user = new User { Id = Guid.NewGuid(), Email = "Inactive@gmail.com", UserName = "Inactive", IsActive = false, EmailConfirmed = true };
            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);

            // Act
            var result = await _userService.LoginAsync(request, "127.0.0.1");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Null(result.Data);
            Assert.Contains("Tài khoản với email Inactive@example.com đã bị khóa.", result.Message);
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ReturnsErrorResponse()
        {
            // Arrange
            var request = new AuthenticationRequest { Email = "luongcongduy826@gmail.com", Password = "WrongPassword!123" };
            var user = new User { Id = Guid.NewGuid(), Email = "luongcongduy826@gmail.com", UserName = "luongcongduy", IsActive = true, EmailConfirmed = true };
            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);
            _signInManagerMock.Setup(x => x.PasswordSignInAsync(user.UserName, request.Password, false, false)).ReturnsAsync(SignInResult.Failed);

            // Act
            var result = await _userService.LoginAsync(request, "127.0.0.1");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Null(result.Data);
            Assert.Equal("Mật khẩu không chính xác.", result.Message);
        }

        [Fact]
        public async Task LoginAsync_EmailNotConfirmed_ReturnsErrorResponse()
        {
            // Arrange
            var request = new AuthenticationRequest { Email = "EmailNotConfirm@gmail.com", Password = "Admin@123" };
            var user = new User { Id = Guid.NewGuid(), Email = "EmailNotConfirm@gmail.com", UserName = "NotConFirm", IsActive = true, EmailConfirmed = false };
            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);
            _signInManagerMock.Setup(x => x.PasswordSignInAsync(user.UserName, request.Password, false, false)).ReturnsAsync(SignInResult.Success);

            // Act
            var result = await _userService.LoginAsync(request, "127.0.0.1");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Null(result.Data);
            Assert.Contains("Tài khoản chưa được xác thực.", result.Message);
        }

        [Fact]
        public async Task LoginAsync_EmailIsNullOrEmpty_ReturnsErrorResponse()
        {
            // Arrange
            var request = new AuthenticationRequest { Email = "", Password = "Admin@123" };
            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User)null);

            // Act
            var result = await _userService.LoginAsync(request, "127.0.0.1");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Null(result.Data);
            Assert.Contains("Không tìm thấy tài khoản với email", result.Message); 
        }

        [Fact]
        public async Task LoginAsync_PasswordIsNullOrEmpty_ReturnsErrorResponse()
        {
            // Arrange
            var request = new AuthenticationRequest { Email = "luongcongduy826@gmail.com", Password = "" };
            var user = new User { Id = Guid.NewGuid(), Email = "luongcongduy826@gmail.com", UserName = "luongcongduy", IsActive = true, EmailConfirmed = true };
            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);
            _signInManagerMock.Setup(x => x.PasswordSignInAsync(user.UserName, It.IsAny<string>(), false, false)).ReturnsAsync(SignInResult.Failed); // Mật khẩu không hợp lệ

            // Act
            var result = await _userService.LoginAsync(request, "127.0.0.1");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Null(result.Data);
            Assert.Contains("Mật khẩu không chính xác.", result.Message); 
        }
    }
}