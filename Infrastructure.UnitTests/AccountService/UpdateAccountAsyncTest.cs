using Domain.Dto.Request.Account;
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
    public class UpdateAccountAsyncTest
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

        public UpdateAccountAsyncTest()
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
        public async Task UpdateAccountAsync_Successful_AllFields()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var request = new UpdateAccountRequest
            {
                UserId = userId,
                Email = "new@example.com",
                PhoneNumber = "1234567890",
                FullName = "New Name"
            };
            var user = new User
            {
                Id = Guid.Parse(userId),
                Email = "old@example.com",
                UserName = "old@example.com",
                FullName = "Old Name",
                PhoneNumber = "0987654321",
                IsActive = true,
                CreatedBy = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow.AddDays(-1)
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.UpdateAccountAsync(request);

            // Assert
            Assert.True(result.Succeeded, $"Succeeded is false. Message: {result.Message}");
            Assert.Equal("Tài khoản đã được cập nhật.", result.Message);
            Assert.Equal(userId, result.Data);
            Assert.Equal(request.Email, user.Email);
            Assert.Equal(request.PhoneNumber, user.PhoneNumber);
            Assert.Equal(request.FullName, user.FullName);
            Assert.Equal(_currentUserId, user.UpdatedBy);
            Assert.NotNull(user.UpdatedDate);
            Assert.True(user.UpdatedDate > DateTime.UtcNow.AddMinutes(-1));
            _userManagerMock.Verify(x => x.FindByIdAsync(userId), Times.Once());
            _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once());
        }

        [Fact]
        public async Task UpdateAccountAsync_Successful_PartialFields()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var request = new UpdateAccountRequest
            {
                UserId = userId,
                Email = "new@example.com",
                PhoneNumber = null,
                FullName = ""
            };
            var user = new User
            {
                Id = Guid.Parse(userId),
                Email = "old@example.com",
                UserName = "old@example.com",
                FullName = "Old Name",
                PhoneNumber = "0987654321",
                IsActive = true,
                CreatedBy = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow.AddDays(-1)
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.UpdateAccountAsync(request);

            // Assert
            Assert.True(result.Succeeded, $"Succeeded is false. Message: {result.Message}");
            Assert.Equal("Tài khoản đã được cập nhật.", result.Message);
            Assert.Equal(userId, result.Data);
            Assert.Equal(request.Email, user.Email);
            Assert.Equal("0987654321", user.PhoneNumber); // Unchanged
            Assert.Equal("Old Name", user.FullName); // Unchanged
            Assert.Equal(_currentUserId, user.UpdatedBy);
            Assert.NotNull(user.UpdatedDate);
            Assert.True(user.UpdatedDate > DateTime.UtcNow.AddMinutes(-1));
            _userManagerMock.Verify(x => x.FindByIdAsync(userId), Times.Once());
            _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once());
        }

        [Fact]
        public async Task UpdateAccountAsync_UserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var request = new UpdateAccountRequest
            {
                UserId = userId,
                Email = "new@example.com",
                PhoneNumber = "1234567890",
                FullName = "New Name"
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act
            var result = await _service.UpdateAccountAsync(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal($"Không tìm thấy tài khoản với ID:{userId}.", result.Message);
            Assert.Null(result.Data);
            _userManagerMock.Verify(x => x.FindByIdAsync(userId), Times.Once());
            _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never());
        }

        //[Fact]
        //public async Task UpdateAccountAsync_NullRequest()
        //{
        //    // Arrange
        //    UpdateAccountRequest request = null;

        //    // Act & Assert
        //    await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        //        await _service.UpdateAccountAsync(request));
        //    _userManagerMock.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never());
        //    _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never());
        //}

        [Fact]
        public async Task UpdateAccountAsync_EmptyFields()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var request = new UpdateAccountRequest
            {
                UserId = userId,
                Email = "",
                PhoneNumber = null,
                FullName = null
            };
            var user = new User
            {
                Id = Guid.Parse(userId),
                Email = "old@example.com",
                UserName = "old@example.com",
                FullName = "Old Name",
                PhoneNumber = "0987654321",
                IsActive = true,
                CreatedBy = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow.AddDays(-1)
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.UpdateAccountAsync(request);

            // Assert
            Assert.True(result.Succeeded, $"Succeeded is false. Message: {result.Message}");
            Assert.Equal("Tài khoản đã được cập nhật.", result.Message);
            Assert.Equal(userId, result.Data);
            Assert.Equal("old@example.com", user.Email); // Unchanged
            Assert.Equal("0987654321", user.PhoneNumber); // Unchanged
            Assert.Equal("Old Name", user.FullName); // Unchanged
            Assert.Equal(_currentUserId, user.UpdatedBy);
            Assert.NotNull(user.UpdatedDate);
            Assert.True(user.UpdatedDate > DateTime.UtcNow.AddMinutes(-1));
            _userManagerMock.Verify(x => x.FindByIdAsync(userId), Times.Once());
            _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once());
        }

        [Fact]
        public async Task UpdateAccountAsync_DatabaseException()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var request = new UpdateAccountRequest
            {
                UserId = userId,
                Email = "new@example.com",
                PhoneNumber = "1234567890",
                FullName = "New Name"
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await _service.UpdateAccountAsync(request));
            Assert.Equal("Database error", exception.Message);
            _userManagerMock.Verify(x => x.FindByIdAsync(userId), Times.Once());
            _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never());
        }
    
}
}
