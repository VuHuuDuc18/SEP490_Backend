using Entities.EntityModel;
using Infrastructure.Identity.Contexts;
using Infrastructure.Repository;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.AccountService
{
    public class EnableAccountAsyncTest
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
        public EnableAccountAsyncTest()
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
        public async Task EnableAccountAsync_Successful()
        {
            // Arrange
            var email = "test@example.com";
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                UserName = email,
                FullName = "Test User",
                IsActive = false,
                CreatedBy = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow.AddDays(-1)
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.EnableAccountAsync(email);

            // Assert
            Assert.True(result.Succeeded, $"Succeeded is false. Message: {result.Message}");
            Assert.Equal("Tài khoản đã được kích hoạt.", result.Message);
            Assert.Equal(email, result.Data);
            Assert.True(user.IsActive);
            Assert.Equal(_currentUserId, user.UpdatedBy);
            Assert.NotNull(user.UpdatedDate);
            Assert.True(user.UpdatedDate > DateTime.UtcNow.AddMinutes(-1));
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once());
            _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once());
        }

        [Fact]
        public async Task EnableAccountAsync_UserNotFound()
        {
            // Arrange
            var email = "nonexistent@example.com";

            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync((User)null);

            // Act
            var result = await _service.EnableAccountAsync(email);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal($"Không tìm thấy tài khoản với email {email}.", result.Message);
            Assert.Null(result.Data);
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once());
            _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never());
        }

        [Fact]
        public async Task EnableAccountAsync_UserAlreadyEnabled()
        {
            // Arrange
            var email = "test@example.com";
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                UserName = email,
                FullName = "Test User",
                IsActive = true,
                CreatedBy = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow.AddDays(-1)
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync(user);

            // Act
            var result = await _service.EnableAccountAsync(email);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal($"Tài khoản đã được kích hoạt - {email}.", result.Message);
            Assert.Null(result.Data);
            Assert.True(user.IsActive);
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once());
            _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never());
        }

        [Fact]
        public async Task EnableAccountAsync_NullOrEmptyEmail()
        {
            // Arrange
            string nullEmail = null;
            string emptyEmail = "";

            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            // Act
            var resultNull = await _service.EnableAccountAsync(nullEmail);
            var resultEmpty = await _service.EnableAccountAsync(emptyEmail);

            // Assert
            Assert.False(resultNull.Succeeded);
            Assert.Equal($"Không tìm thấy tài khoản với email {nullEmail}.", resultNull.Message);
            Assert.Null(resultNull.Data);

            Assert.False(resultEmpty.Succeeded);
            Assert.Equal($"Không tìm thấy tài khoản với email {emptyEmail}.", resultEmpty.Message);
            Assert.Null(resultEmpty.Data);

            _userManagerMock.Verify(x => x.FindByEmailAsync(nullEmail), Times.Once());
            _userManagerMock.Verify(x => x.FindByEmailAsync(emptyEmail), Times.Once());
            _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never());
        }

        [Fact]
        public async Task EnableAccountAsync_DatabaseException()
        {
            // Arrange
            var email = "test@example.com";

            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await _service.EnableAccountAsync(email));
            Assert.Equal("Database error", exception.Message);
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once());
            _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never());
        }

    }
}

