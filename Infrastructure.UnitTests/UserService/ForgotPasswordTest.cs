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
    public class ForgotPasswordTest
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IdentityContext> _contextMock;
        private readonly Mock<IOptions<JWTSettings>> _jwtSettingsMock;
        private readonly Infrastructure.Services.Implements.UserService _userService;

        public ForgotPasswordTest()
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
        public async Task ForgotPassword_ValidRequest_SendsEmail()
        {
            // Arrange
            var email = "luongcongduy826@gmail.com";
            var user = new User { Id = Guid.Parse("2eaedaf7-afd6-4340-53de-08ddc0fec23a"), Email = email, UserName = "testuser", IsActive = true, EmailConfirmed = true };
            var request = new ForgotPasswordRequest { Email = email };
            var origin = "https://example.com";
            var token = "test-token";
            _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync(token);

            // Act
            await _userService.ForgotPassword(request, origin);

            // Assert
            _emailServiceMock.Verify(x => x.SendEmailAsync(
                email,
                It.Is<string>(s => s == "QUÊN MẬT KHẨU"), 
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ForgotPassword_UserNotFound_NoAction()
        {
            // Arrange
            var email = "nonexistent@gmail.com";
            var request = new ForgotPasswordRequest { Email = email };
            var origin = "https://example.com";
            _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync((User)null);

            // Act & Assert
            var exception = await Record.ExceptionAsync(() => _userService.ForgotPassword(request, origin));
            Assert.Null(exception); // Không ném ngoại lệ
            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ForgotPassword_UserNotActive_NoAction()
        {
            // Arrange
            var email = "inactive@gmail.com";
            var user = new User { Id = Guid.Parse("2eaedaf7-afd6-4340-53de-08ddc0fec23a"), Email = email, UserName = "testuser", IsActive = false, EmailConfirmed = true };
            var request = new ForgotPasswordRequest { Email = email };
            var origin = "https://example.com";
            _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync(user);

            // Act & Assert
            var exception = await Record.ExceptionAsync(() => _userService.ForgotPassword(request, origin));
            Assert.Null(exception); 
            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        //[Fact]
        //public async Task ForgotPassword_NullRequest_NoAction()
        //{
        //    // Arrange
        //    ForgotPasswordRequest request = null;
        //    var origin = "https://example.com";

        //    // Act & Assert
        //    var exception = await Record.ExceptionAsync(() => _userService.ForgotPassword(request, origin));
        //    Assert.Null(exception); // Không ném ngoại lệ
        //    _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        //}

        [Fact]
        public async Task ForgotPassword_EmptyEmail_NoAction()
        {
            // Arrange
            var request = new ForgotPasswordRequest { Email = "" };
            var origin = "https://example.com";
            _userManagerMock.Setup(x => x.FindByEmailAsync("")).ReturnsAsync((User)null);

            // Act & Assert
            var exception = await Record.ExceptionAsync(() => _userService.ForgotPassword(request, origin));
            Assert.Null(exception); // Không ném ngoại lệ
            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}