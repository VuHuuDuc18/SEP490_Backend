using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Wrappers;
using Domain.Dto.Request.Account;
using Domain.Helper.Constants;
using Entities.EntityModel;
using Infrastructure.Identity.Contexts;
using Infrastructure.Repository;
using Infrastructure.Services;
using Infrastructure.Services.Implements;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.AccountService
{
    public class ResetPasswordTest
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<RoleManager<Role>> _roleManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IdentityContext> _contextMock;
        private readonly Mock<IRepository<User>> _userRepositoryMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.AccountService _service;
        private readonly Guid _currentUserId = Guid.NewGuid();

        public ResetPasswordTest()
        {
            // Mock dependencies for UserManager
            var userStoreMock = new Mock<IUserStore<User>>();
            var identityOptionsMock = new Mock<IOptions<IdentityOptions>>();
            var passwordHasherMock = new Mock<IPasswordHasher<User>>();
            var lookupNormalizerMock = new Mock<ILookupNormalizer>();
            var identityErrorDescriberMock = new Mock<IdentityErrorDescriber>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerUserManagerMock = new Mock<ILogger<UserManager<User>>>();

            // Mock UserManager
            _userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object,
                identityOptionsMock.Object,
                passwordHasherMock.Object,
                new IUserValidator<User>[0],
                new IPasswordValidator<User>[0],
                lookupNormalizerMock.Object,
                identityErrorDescriberMock.Object,
                serviceProviderMock.Object,
                loggerUserManagerMock.Object
            );

            // Mock dependencies for RoleManager
            var roleStoreMock = new Mock<IRoleStore<Role>>();
            var loggerRoleManagerMock = new Mock<ILogger<RoleManager<Role>>>();

            // Mock RoleManager
            _roleManagerMock = new Mock<RoleManager<Role>>(
                roleStoreMock.Object,
                new IRoleValidator<Role>[0],
                lookupNormalizerMock.Object,
                identityErrorDescriberMock.Object,
                loggerRoleManagerMock.Object
            );

            // Mock dependencies for SignInManager
            var claimsPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<User>>();
            var authenticationSchemeProviderMock = new Mock<IAuthenticationSchemeProvider>();
            var userConfirmationMock = new Mock<IUserConfirmation<User>>();
            var loggerSignInManagerMock = new Mock<ILogger<SignInManager<User>>>();

            // Mock SignInManager
            _signInManagerMock = new Mock<SignInManager<User>>(
                _userManagerMock.Object,
                new Mock<IHttpContextAccessor>().Object,
                claimsPrincipalFactoryMock.Object,
                identityOptionsMock.Object,
                loggerSignInManagerMock.Object,
                authenticationSchemeProviderMock.Object,
                userConfirmationMock.Object
            );
            _signInManagerMock.Setup(x => x.SignInAsync(It.IsAny<User>(), It.IsAny<bool>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            _emailServiceMock = new Mock<IEmailService>();
            _contextMock = new Mock<IdentityContext>(new DbContextOptions<IdentityContext>());
            _userRepositoryMock = new Mock<IRepository<User>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            var claims = new List<Claim>
            {
                new Claim("uid", _currentUserId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = claimsPrincipal };
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            _service = new Infrastructure.Services.Implements.AccountService(
                _userManagerMock.Object,
                _roleManagerMock.Object,
                Options.Create(new Domain.Settings.JWTSettings()),
                _signInManagerMock.Object,
                _emailServiceMock.Object,
                _contextMock.Object,
                _userRepositoryMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public async Task ResetPassword_Successful()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = "user1@example.com",
                Token = "valid-token",
                Password = "NewPassword123!"
            };
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                UserName = request.Email,
                FullName = "User One"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.ResetPasswordAsync(user, request.Token, request.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.ResetPassword(request);

            // Assert
            Assert.True(result.Succeeded, $"Succeeded is false. Message: {result.Message}");
            Assert.Equal("Đã đặt lại mật khẩu.", result.Message);
            Assert.Equal(request.Email, result.Data);
            _userManagerMock.Verify(x => x.FindByEmailAsync(request.Email), Times.Once());
            _userManagerMock.Verify(x => x.ResetPasswordAsync(user, request.Token, request.Password), Times.Once());
        }

        [Fact]
        public async Task ResetPassword_UserNotFound()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = "nonexistent@example.com",
                Token = "valid-token",
                Password = "NewPassword123!"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((User)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApiException>(async () =>
                await _service.ResetPassword(request));
            Assert.Equal($"No Accounts Registered with {request.Email}.", exception.Message);
            _userManagerMock.Verify(x => x.FindByEmailAsync(request.Email), Times.Once());
            _userManagerMock.Verify(x => x.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Fact]
        public async Task ResetPassword_InvalidTokenOrPassword()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = "user1@example.com",
                Token = "invalid-token",
                Password = "weak" // Invalid password
            };
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                UserName = request.Email,
                FullName = "User One"
            };
            var errors = new[] { new IdentityError { Description = "Invalid token or password too weak." } };

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.ResetPasswordAsync(user, request.Token, request.Password))
                .ReturnsAsync(IdentityResult.Failed(errors));

            // Act
            var result = await _service.ResetPassword(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Lỗi khi đặt lại mật khẩu.", result.Message);
            Assert.Null(result.Data);
            Assert.Contains("Invalid token or password too weak.", result.Errors);
            _userManagerMock.Verify(x => x.FindByEmailAsync(request.Email), Times.Once());
            _userManagerMock.Verify(x => x.ResetPasswordAsync(user, request.Token, request.Password), Times.Once());
        }

        //[Fact]
        //public async Task ResetPassword_NullRequest()
        //{
        //    // Act & Assert
        //    await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        //        await _service.ResetPassword(null));
        //    _userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Never());
        //    _userManagerMock.Verify(x => x.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        //}

        [Fact]
        public async Task ResetPassword_NullOrEmptyEmail()
        {
            // Arrange
            var requestNullEmail = new ResetPasswordRequest
            {
                Email = null,
                Token = "valid-token",
                Password = "NewPassword123!"
            };
            var requestEmptyEmail = new ResetPasswordRequest
            {
                Email = "",
                Token = "valid-token",
                Password = "NewPassword123!"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            // Act & Assert
            var exceptionNull = await Assert.ThrowsAsync<ApiException>(async () =>
                await _service.ResetPassword(requestNullEmail));
            Assert.Equal("No Accounts Registered with .", exceptionNull.Message);

            var exceptionEmpty = await Assert.ThrowsAsync<ApiException>(async () =>
                await _service.ResetPassword(requestEmptyEmail));
            Assert.Equal("No Accounts Registered with .", exceptionEmpty.Message);

            _userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Exactly(2));
            _userManagerMock.Verify(x => x.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }


        [Fact]
        public async Task ResetPassword_NullOrEmptyToken()
        {
            // Arrange
            var requestNullEmail = new ResetPasswordRequest
            {
                Email = null,
                Token = "valid-token",
                Password = "NewPassword123!"
            };
            var requestEmptyEmail = new ResetPasswordRequest
            {
                Email = "",
                Token = "valid-token",
                Password = "NewPassword123!"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            // Act & Assert
            var exceptionNull = await Assert.ThrowsAsync<ApiException>(async () =>
                await _service.ResetPassword(requestNullEmail));
            Assert.Equal("No Accounts Registered with .", exceptionNull.Message);

            var exceptionEmpty = await Assert.ThrowsAsync<ApiException>(async () =>
                await _service.ResetPassword(requestEmptyEmail));
            Assert.Equal("No Accounts Registered with .", exceptionEmpty.Message);

            _userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Exactly(2));
            _userManagerMock.Verify(x => x.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }
        [Fact]
        public async Task ResetPassword_NullOrEmptyPass()
        {
            // Arrange
            var requestNullEmail = new ResetPasswordRequest
            {
                Email = null,
                Token = "valid-token",
                Password = "NewPassword123!"
            };
            var requestEmptyEmail = new ResetPasswordRequest
            {
                Email = "",
                Token = "valid-token",
                Password = "NewPassword123!"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            // Act & Assert
            var exceptionNull = await Assert.ThrowsAsync<ApiException>(async () =>
                await _service.ResetPassword(requestNullEmail));
            Assert.Equal("No Accounts Registered with .", exceptionNull.Message);

            var exceptionEmpty = await Assert.ThrowsAsync<ApiException>(async () =>
                await _service.ResetPassword(requestEmptyEmail));
            Assert.Equal("No Accounts Registered with .", exceptionEmpty.Message);

            _userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Exactly(2));
            _userManagerMock.Verify(x => x.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }
        [Fact]
        public async Task ResetPassword_NullRequest()
        {
            // Arrange
            var requestNullEmail = new ResetPasswordRequest
            {
                Email = null,
                Token = "valid-token",
                Password = "NewPassword123!"
            };
            var requestEmptyEmail = new ResetPasswordRequest
            {
                Email = "",
                Token = "valid-token",
                Password = "NewPassword123!"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            // Act & Assert
            var exceptionNull = await Assert.ThrowsAsync<ApiException>(async () =>
                await _service.ResetPassword(requestNullEmail));
            Assert.Equal("No Accounts Registered with .", exceptionNull.Message);

            var exceptionEmpty = await Assert.ThrowsAsync<ApiException>(async () =>
                await _service.ResetPassword(requestEmptyEmail));
            Assert.Equal("No Accounts Registered with .", exceptionEmpty.Message);

            _userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Exactly(2));
            _userManagerMock.Verify(x => x.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }
        [Fact]
        public async Task ResetPassword_UserManagerException()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = "user1@example.com",
                Token = "valid-token",
                Password = "NewPassword123!"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await _service.ResetPassword(request));
            Assert.Equal("Database error", exception.Message);
            _userManagerMock.Verify(x => x.FindByEmailAsync(request.Email), Times.Once());
            _userManagerMock.Verify(x => x.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }
    }

    public class TestIdentityDbContext3 : DbContext
    {
        public TestIdentityDbContext3(DbContextOptions<TestIdentityDbContext3> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
    }
}