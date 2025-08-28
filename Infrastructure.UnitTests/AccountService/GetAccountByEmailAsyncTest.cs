using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Wrappers;
using Domain.Dto.Response;
using Domain.Dto.Response.Account;
using Domain.Dto.Response.User;
using Domain.DTOs.Response.Role;
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
    public class GetAccountByEmailAsyncTest
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<RoleManager<Role>> _roleManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IdentityContext> _contextMock;
        private readonly Mock<IRepository<User>> _userRepositoryMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Infrastructure.Services.Implements.AccountService _service;

        public GetAccountByEmailAsyncTest()
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
                new Claim("uid", Guid.NewGuid().ToString())
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

        //[Fact]
        //public async Task GetAccountByEmailAsync_Successful()
        //{
        //    // Arrange
        //    var options = new DbContextOptionsBuilder<TestIdentityDbContext1>()
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        //        .Options;
        //    using var context = new TestIdentityDbContext1(options);

        //    var user = new User
        //    {
        //        Id = Guid.NewGuid(),
        //        Email = "user1@example.com",
        //        FullName = "User One",
        //        IsActive = true,
        //        CreatedDate = DateTime.UtcNow,
        //        CreatedBy = Guid.NewGuid()
        //    };
        //    context.Users.Add(user);
        //    await context.SaveChangesAsync();

        //    _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
        //        .ReturnsAsync((string email) => context.Users.FirstOrDefault(u => u.Email == email));

        //    // Act
        //    var result = await _service.GetAccountByEmailAsync("user1@example.com");

        //    // Assert
        //    Assert.True(result.Succeeded, $"Succeeded is false. Message: {result.Message}");
        //    Assert.Equal("Lấy tài khoản thành công.", result.Message);
        //    Assert.NotNull(result.Data);
        //    Assert.Equal("user1@example.com", result.Data.Email);
        //    Assert.Equal(user.Id, result.Data.Id);
        //    Assert.Equal(user.FullName, result.Data.FullName);
        //}

        [Fact]
        public async Task GetAccountByEmailAsync_UserNotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestIdentityDbContext1>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestIdentityDbContext1(options);

            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((string email) => context.Users.FirstOrDefault(u => u.Email == email));

            // Act
            var result = await _service.GetAccountByEmailAsync("nonexistent@example.com");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Không tìm thấy tài khoản với email nonexistent@example.com.", result.Message);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetAccountByEmailAsync_NullOrEmptyEmail()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestIdentityDbContext1>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestIdentityDbContext1(options);

            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((string email) => context.Users.FirstOrDefault(u => u.Email == email));

            // Act
            var resultNull = await _service.GetAccountByEmailAsync(null);
            var resultEmpty = await _service.GetAccountByEmailAsync("");

            // Assert
            Assert.False(resultNull.Succeeded);
            Assert.Equal("Không tìm thấy tài khoản với email .", resultNull.Message);
            Assert.Null(resultNull.Data);

            Assert.False(resultEmpty.Succeeded);
            Assert.Equal("Không tìm thấy tài khoản với email .", resultEmpty.Message);
            Assert.Null(resultEmpty.Data);
        }

        //[Fact]
        //public async Task GetAccountByEmailAsync_ExceptionOccurs()
        //{
        //    // Arrange
        //    _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
        //        .ThrowsAsync(new Exception("Database error"));

        //    // Act & Assert
        //    await Assert.ThrowsAsync<Exception>(async () =>
        //        await _service.GetAccountByEmailAsync("user1@example.com"));
        //}
    }

    public class TestIdentityDbContext1 : DbContext
    {
        public TestIdentityDbContext1(DbContextOptions<TestIdentityDbContext1> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
    }
}