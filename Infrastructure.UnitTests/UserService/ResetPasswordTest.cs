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
    public class ResetPasswordTest
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IdentityContext> _contextMock;
        private readonly Mock<IOptions<JWTSettings>> _jwtSettingsMock;
        private readonly Infrastructure.Services.Implements.UserService _userService;

        public ResetPasswordTest()
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
        public async Task ResetPassword_ValidRequest_ReturnsSuccessResponse()
        {
            // Arrange
            var email = "luongcongduy826@gmail.com";
            var user = new User { Id = Guid.Parse("2eaedaf7-afd6-4340-53de-08ddc0fec23a"), Email = email, UserName = "testuser", IsActive = true, EmailConfirmed = true };
            var request = new ResetPasswordRequest { Email = email, Token = "valid-token", Password = "NewPassword123!", ConfirmPassword = "NewPassword123!" };
            _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.ResetPasswordAsync(user, request.Token, request.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userService.ResetPassword(request);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("Đã đặt lại mật khẩu.", result.Message);
            Assert.Equal(email, result.Data);
        }

        [Fact]
        public async Task ResetPassword_UserNotFound_ReturnsErrorResponse()
        {
            // Arrange
            var email = "nonexistent@gmail.com";
            var request = new ResetPasswordRequest { Email = email, Token = "valid-token", Password = "NewPassword123!", ConfirmPassword = "NewPassword123!" };
            _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync((User)null);

            // Act
            var result = await _userService.ResetPassword(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal($"Không tìm thấy tài khoản với email {email}.", result.Message);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task ResetPassword_UserNotActive_ReturnsErrorResponse()
        {
            // Arrange
            var email = "inactive@gmail.com";
            var user = new User { Id = Guid.Parse("2eaedaf7-afd6-4340-53de-08ddc0fec23a"), Email = email, UserName = "testuser", IsActive = false, EmailConfirmed = true };
            var request = new ResetPasswordRequest { Email = email, Token = "valid-token", Password = "NewPassword123!", ConfirmPassword = "NewPassword123!" };
            _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync(user);

            // Act
            var result = await _userService.ResetPassword(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal($"Tài khoản với email {email} đã bị khóa.", result.Message);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task ResetPassword_InvalidToken_ReturnsErrorResponse()
        {
            // Arrange
            var email = "luongcongduy826@gmail.com";
            var user = new User { Id = Guid.Parse("2eaedaf7-afd6-4340-53de-08ddc0fec23a"), Email = email, UserName = "testuser", IsActive = true, EmailConfirmed = true };
            var request = new ResetPasswordRequest { Email = email, Token = "invalid-token", Password = "NewPassword123!", ConfirmPassword = "NewPassword123!" };
            _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.ResetPasswordAsync(user, request.Token, request.Password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Token không hợp lệ." }));

            // Act
            var result = await _userService.ResetPassword(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Đặt lại mật khẩu không thành công.", result.Message);
            Assert.NotNull(result.Errors);
            Assert.Contains("Token không hợp lệ.", result.Errors);
        }

        //[Fact]
        //public async Task ResetPassword_NullRequest_ReturnsErrorResponse()
        //{
        //    // Arrange
        //    ResetPasswordRequest request = null;

        //    // Act
        //    var result = await _userService.ResetPassword(request);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Không tìm thấy tài khoản với email .", result.Message); // Giả định dựa trên logic hiện tại khi user là null
        //    Assert.Null(result.Data);
        //}

        [Fact]
        public async Task ResetPassword_MissingEmail_ReturnsErrorResponse()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = null,
                Token = "valid-token",
                Password = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };

            // Act
            var result = await _userService.ResetPassword(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Dữ liệu không hợp lệ.", result.Message);
            Assert.NotNull(result.Errors);
            Assert.Contains("The Email field is required.", result.Errors);
        }

        //[Fact]
        //public async Task ResetPassword_InvalidEmail_ReturnsErrorResponse()
        //{
        //    // Arrange
        //    var request = new ResetPasswordRequest
        //    {
        //        Email = "invalid-email",
        //        Token = "valid-token",
        //        Password = "NewPassword123!",
        //        ConfirmPassword = "NewPassword123!"
        //    };

        //    // Act
        //    var result = await _userService.ResetPassword(request);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Dữ liệu không hợp lệ.", result.Message);
        //    Assert.NotNull(result.Errors);
        //    Assert.Contains("The Email field is not a valid e-mail address.", result.Errors);
        //}

        [Fact]
        public async Task ResetPassword_MissingToken_ReturnsErrorResponse()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = "luongcongduy826@gmail.com",
                Token = null,
                Password = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };

            // Act
            var result = await _userService.ResetPassword(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Dữ liệu không hợp lệ.", result.Message);
            Assert.NotNull(result.Errors);
            Assert.Contains("The Token field is required.", result.Errors);
        }

        [Fact]
        public async Task ResetPassword_MissingPassword_ReturnsErrorResponse()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = "luongcongduy826@gmail.com",
                Token = "valid-token",
                Password = null,
                ConfirmPassword = "NewPassword123!"
            };

            // Act
            var result = await _userService.ResetPassword(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Dữ liệu không hợp lệ.", result.Message);
            Assert.NotNull(result.Errors);
            Assert.Contains("The Password field is required.", result.Errors);
        }

        //[Fact]
        //public async Task ResetPassword_ShortPassword_ReturnsErrorResponse()
        //{
        //    // Arrange
        //    var request = new ResetPasswordRequest
        //    {
        //        Email = "luongcongduy826@gmail.com",
        //        Token = "valid-token",
        //        Password = "short",
        //        ConfirmPassword = "short"
        //    };

        //    // Act
        //    var result = await _userService.ResetPassword(request);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Dữ liệu không hợp lệ.", result.Message);
        //    Assert.NotNull(result.Errors);
        //    Assert.Contains("The field Password must be a string or array type ", result.Errors);
        //}

        [Fact]
        public async Task ResetPassword_MissingConfirmPassword_ReturnsErrorResponse()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = "luongcongduy826@gmail.com",
                Token = "valid-token",
                Password = "NewPassword123!",
                ConfirmPassword = null
            };

            // Act
            var result = await _userService.ResetPassword(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Dữ liệu không hợp lệ.", result.Message);
            Assert.NotNull(result.Errors);
            Assert.Contains("The ConfirmPassword field is required.", result.Errors);
        }

        [Fact]
        public async Task ResetPassword_MismatchedPasswords_ReturnsErrorResponse()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = "luongcongduy826@gmail.com",
                Token = "valid-token",
                Password = "NewPassword123!",
                ConfirmPassword = "MismatchPassword123!" // Không khớp
            };

            // Act
            var result = await _userService.ResetPassword(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Dữ liệu không hợp lệ.", result.Message);
            Assert.NotNull(result.Errors);
            Assert.Contains("The ConfirmPassword field must match the Password field.", result.Errors);
        }
    }
}