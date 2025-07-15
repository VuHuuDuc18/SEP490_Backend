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
    public class ChangePasswordTest
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IdentityContext> _contextMock;
        private readonly Mock<IOptions<JWTSettings>> _jwtSettingsMock;
        private readonly Infrastructure.Services.Implements.UserService _userService;

        public ChangePasswordTest()
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
        public async Task ChangePassword_ValidRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var userId = "2eaedaf7-afd6-4340-53de-08ddc0fec23a";
            var user = new User { Id = Guid.Parse(userId), Email = "luongcongduy826@gmail.com", UserName = "luongcongduy", EmailConfirmed = true };
            var request = new ChangePasswordRequest
            {
                UserId = Guid.Parse(userId),
                OldPassword = "OldPassword123!",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };
            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.ChangePasswordAsync(user, request.OldPassword, request.NewPassword))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.ChangePassword(request);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("Đổi mật khẩu thành công!", result.Message);
        }

        [Fact]
        public async Task ChangePassword_UserNotFound_ReturnsErrorResponse()
        {
            // Arrange
            var userId = "2eaedaf7-afd6-4340-53de-08ddc0fec23a";
            var request = new ChangePasswordRequest
            {
                UserId = Guid.Parse(userId),
                OldPassword = "OldPassword123!",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };
            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync((User)null);

            // Act
            var result = await _userService.ChangePassword(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Không tìm thấy tài khoản", result.Message);
        }

        [Fact]
        public async Task ChangePassword_InvalidOldPassword_ReturnsErrorResponse()
        {
            // Arrange
            var userId = "2eaedaf7-afd6-4340-53de-08ddc0fec23a";
            var user = new User { Id = Guid.Parse(userId), Email = "luongcongduy826@gmail.com", UserName = "luongcongduy", EmailConfirmed = true };
            var request = new ChangePasswordRequest
            {
                UserId = Guid.Parse(userId),
                OldPassword = "WrongPassword123!",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };
            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.ChangePasswordAsync(user, request.OldPassword, request.NewPassword))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Mật khẩu cũ không đúng." }));

            // Act
            var result = await _userService.ChangePassword(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Đổi mật khẩu không thành công.", result.Message);
            Assert.NotNull(result.Errors);
            Assert.Contains("Mật khẩu cũ không đúng.", result.Errors);
        }

        //[Fact]
        //public async Task ChangePassword_NullRequest_ReturnsErrorResponse()
        //{
        //    // Arrange
        //    ChangePasswordRequest request = null;

        //    // Act
        //    var result = await _userService.ChangePassword(request);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Không tìm thấy tài khoản", result.Message); // Giả định dựa trên logic hiện tại khi user là null
        //}

        [Fact]
        public async Task ChangePassword_MissingUserId_ReturnsErrorResponse()
        {
            // Arrange
            var userId = "2eaedaf7-afd6-4340-53de-08ddc0fec23a";
            var request = new ChangePasswordRequest
            {
                UserId = Guid.Empty, // Thiếu UserId hợp lệ
                OldPassword = "OldPassword123!",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };
            var user = new User { Id = Guid.Parse(userId), Email = "luongcongduy826@gmail.com", UserName = "luongcongduy", EmailConfirmed = true };
            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _userService.ChangePassword(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Không tìm thấy tài khoản", result.Message); 
        }

        [Fact]
        public async Task ChangePassword_MissingOldPassword_ReturnsErrorResponse()
        {
            // Arrange
            var userId = "2eaedaf7-afd6-4340-53de-08ddc0fec23a";
            var request = new ChangePasswordRequest
            {
                UserId = Guid.Parse(userId),
                OldPassword = null, // Thiếu OldPassword
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };
            var user = new User { Id = Guid.Parse(userId), Email = "luongcongduy826@gmail.com", UserName = "luongcongduy", EmailConfirmed = true };
            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _userService.ChangePassword(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Dữ liệu không hợp lệ.", result.Message);
            Assert.NotNull(result.Errors);
            Assert.Contains("The OldPassword field is required.", result.Errors);
        }

        [Fact]
        public async Task ChangePassword_MissingNewPassword_ReturnsErrorResponse()
        {
            // Arrange
            var userId = "2eaedaf7-afd6-4340-53de-08ddc0fec23a";
            var request = new ChangePasswordRequest
            {
                UserId = Guid.Parse(userId),
                OldPassword = "OldPassword123!",
                NewPassword = null, // Thiếu NewPassword
                ConfirmPassword = "NewPassword123!"
            };
            var user = new User { Id = Guid.Parse(userId), Email = "luongcongduy826@gmail.com", UserName = "luongcongduy", EmailConfirmed = true };
            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _userService.ChangePassword(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Dữ liệu không hợp lệ.", result.Message);
            Assert.NotNull(result.Errors);
            Assert.Contains("The NewPassword field is required.", result.Errors);
        }

        [Fact]
        public async Task ChangePassword_MissingConfirmPassword_ReturnsErrorResponse()
        {
            // Arrange
            var userId = "2eaedaf7-afd6-4340-53de-08ddc0fec23a";
            var request = new ChangePasswordRequest
            {
                UserId = Guid.Parse(userId),
                OldPassword = "OldPassword123!",
                NewPassword = "NewPassword123!",
                ConfirmPassword = null // Thiếu ConfirmPassword
            };
            var user = new User { Id = Guid.Parse(userId), Email = "luongcongduy826@gmail.com", UserName = "luongcongduy", EmailConfirmed = true };
            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _userService.ChangePassword(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Dữ liệu không hợp lệ.", result.Message);
            Assert.NotNull(result.Errors);
            Assert.Contains("The ConfirmPassword field is required.", result.Errors);
        }


        [Fact]
        public async Task ChangePassword_MismatchedPasswords_ReturnsErrorResponse()
        {
            // Arrange
            var userId = "2eaedaf7-afd6-4340-53de-08ddc0fec23a";
            var request = new ChangePasswordRequest
            {
                UserId = Guid.Parse(userId),
                OldPassword = "OldPassword123!",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "MismatchPassword123!" // Không khớp
            };
            var user = new User { Id = Guid.Parse(userId), Email = "luongcongduy826@gmail.com", UserName = "luongcongduy", EmailConfirmed = true };
            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _userService.ChangePassword(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains("Dữ liệu không hợp lệ.", result.Message);
        }

        [Fact]
        public async Task ChangePassword_FailedChange_ReturnsErrorResponse()
        {
            // Arrange
            var userId = "2eaedaf7-afd6-4340-53de-08ddc0fec23a";
            var user = new User { Id = Guid.Parse(userId), Email = "luongcongduy826@gmail.com", UserName = "luongcongduy", EmailConfirmed = true };
            var request = new ChangePasswordRequest
            {
                UserId = Guid.Parse(userId),
                OldPassword = "OldPassword123!",
                NewPassword = "short", // Mật khẩu không hợp lệ
                ConfirmPassword = "short"
            };
            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.ChangePasswordAsync(user, request.OldPassword, request.NewPassword))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Mật khẩu mới không đáp ứng yêu cầu." }));

            // Act
            var result = await _userService.ChangePassword(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Đổi mật khẩu không thành công.", result.Message);
            Assert.NotNull(result.Errors);
            Assert.Contains("Mật khẩu mới không đáp ứng yêu cầu.", result.Errors);
        }
    }
}