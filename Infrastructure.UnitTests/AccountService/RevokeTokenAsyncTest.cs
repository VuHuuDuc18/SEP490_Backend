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
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.AccountService
{
    public class RevokeTokenAsyncTest
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

        public RevokeTokenAsyncTest()
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
        public async Task RevokeTokenAsync_Successful()
        {
            // Arrange
            var token = "valid-token";
            var ipAddress = "192.168.1.1";
            var refreshToken = new Entities.EntityModel.RefreshToken
            {
                Id = 1,
                Token = token,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(1),
                CreatedByIp = "104.268.1.1",
                Revoked = null,
                RevokedByIp = null
            };

            var options = new DbContextOptionsBuilder<TestIdentityDbContext5>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestIdentityDbContext5(options);
            context.RefreshTokens.Add(refreshToken);
            await context.SaveChangesAsync();

            var refreshTokens = context.RefreshTokens.AsQueryable();
            var mockDbSet = new Mock<DbSet<Entities.EntityModel.RefreshToken>>();
            mockDbSet.As<IQueryable<Entities.EntityModel.RefreshToken>>()
                .Setup(m => m.Provider)
                .Returns(refreshTokens.Provider);
            mockDbSet.As<IQueryable<Entities.EntityModel.RefreshToken>>()
                .Setup(m => m.Expression)
                .Returns(refreshTokens.Expression);
            mockDbSet.As<IQueryable<Entities.EntityModel.RefreshToken>>()
                .Setup(m => m.ElementType)
                .Returns(refreshTokens.ElementType);
            mockDbSet.As<IQueryable<Entities.EntityModel.RefreshToken>>()
                .Setup(m => m.GetEnumerator())
                .Returns(refreshTokens.GetEnumerator());

            _contextMock.Setup(x => x.RefreshTokens).Returns(mockDbSet.Object);
            _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1)
                .Callback(() => context.SaveChanges());

            // Act
            var result = await _service.RevokeTokenAsync(token, ipAddress);

            // Assert
            Assert.True(result.Succeeded, $"Succeeded is false. Message: {result.Message}");
            Assert.Equal("Token đã bị hủy.", result.Message);
            Assert.Equal("", result.Data);
            var updatedToken = context.RefreshTokens.FirstOrDefault(t => t.Token == token);
            Assert.NotNull(updatedToken);
            Assert.False(updatedToken.IsActive);
            Assert.NotNull(updatedToken.Revoked);
            Assert.Equal(ipAddress, updatedToken.RevokedByIp);
            _contextMock.Verify(x => x.RefreshTokens, Times.Once());
            _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        //[Fact]
        //public async Task RevokeTokenAsync_TokenNotFound()
        //{
        //    // Arrange
        //    var token = "invalid-token";
        //    var ipAddress = "192.168.1.1";

        //    var refreshTokens = new List<Entities.EntityModel.RefreshToken>().AsQueryable();
        //    var mockDbSet = new Mock<DbSet<Entities.EntityModel.RefreshToken>>();
        //    mockDbSet.As<IQueryable<Entities.EntityModel.RefreshToken>>()
        //        .Setup(m => m.Provider)
        //        .Returns(refreshTokens.Provider);
        //    mockDbSet.As<IQueryable<Entities.EntityModel.RefreshToken>>()
        //        .Setup(m => m.Expression)
        //        .Returns(refreshTokens.Expression);
        //    mockDbSet.As<IQueryable<Entities.EntityModel.RefreshToken>>()
        //        .Setup(m => m.ElementType)
        //        .Returns(refreshTokens.ElementType);
        //    mockDbSet.As<IQueryable<Entities.EntityModel.RefreshToken>>()
        //        .Setup(m => m.GetEnumerator())
        //        .Returns(refreshTokens.GetEnumerator());

        //    _contextMock.Setup(x => x.RefreshTokens).Returns(mockDbSet.Object);

        //    // Act
        //    var result = await _service.RevokeTokenAsync(token, ipAddress);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal("Token không hợp lệ.", result.Message);
        //    Assert.Null(result.Data);
        //    _contextMock.Verify(x => x.RefreshTokens, Times.Once());
        //    _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        //}

        [Fact]
        public async Task RevokeTokenAsync_TokenAlreadyRevoked()
        {
            // Arrange
            var token = "revoked-token";
            var ipAddress = "192.168.1.1";
            var refreshToken = new Entities.EntityModel.RefreshToken
            {
                Id = 1,
                Token = token,
                Created = DateTime.UtcNow.AddHours(-2),
                Expires = DateTime.UtcNow.AddDays(1),
                CreatedByIp = "104.268.1.1",
                Revoked = DateTime.UtcNow.AddHours(-1),
                RevokedByIp = "192.168.1.2"
            };

            var options = new DbContextOptionsBuilder<TestIdentityDbContext5>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestIdentityDbContext5(options);
            context.RefreshTokens.Add(refreshToken);
            await context.SaveChangesAsync();

            var refreshTokens = context.RefreshTokens.AsQueryable();
            var mockDbSet = new Mock<DbSet<Entities.EntityModel.RefreshToken>>();
            mockDbSet.As<IQueryable<Entities.EntityModel.RefreshToken>>()
                .Setup(m => m.Provider)
                .Returns(refreshTokens.Provider);
            mockDbSet.As<IQueryable<Entities.EntityModel.RefreshToken>>()
                .Setup(m => m.Expression)
                .Returns(refreshTokens.Expression);
            mockDbSet.As<IQueryable<Entities.EntityModel.RefreshToken>>()
                .Setup(m => m.ElementType)
                .Returns(refreshTokens.ElementType);
            mockDbSet.As<IQueryable<Entities.EntityModel.RefreshToken>>()
                .Setup(m => m.GetEnumerator())
                .Returns(refreshTokens.GetEnumerator());

            _contextMock.Setup(x => x.RefreshTokens).Returns(mockDbSet.Object);

            // Act
            var result = await _service.RevokeTokenAsync(token, ipAddress);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Token đã bị hủy.", result.Message);
            Assert.Null(result.Data);
            var unchangedToken = context.RefreshTokens.FirstOrDefault(t => t.Token == token);
            Assert.NotNull(unchangedToken);
            Assert.False(unchangedToken.IsActive);
            Assert.Equal(refreshToken.Revoked, unchangedToken.Revoked);
            Assert.Equal(refreshToken.RevokedByIp, unchangedToken.RevokedByIp);
            _contextMock.Verify(x => x.RefreshTokens, Times.Once());
            _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        //[Fact]
        //public async Task RevokeTokenAsync_NullOrEmptyToken()
        //{
        //    // Arrange
        //    string nullToken = null;
        //    string emptyToken = "";
        //    var ipAddress = "192.168.1.1";

        //    var refreshTokens = new List<Entities.EntityModel.RefreshToken>().AsQueryable();
        //    var mockDbSet = new Mock<DbSet<Entities.EntityModel.RefreshToken>>();
        //    mockDbSet.As<IQueryable<Entities.EntityModel.RefreshToken>>()
        //        .Setup(m => m.Provider)
        //        .Returns(refreshTokens.Provider);
        //    mockDbSet.As<IQueryable<Entities.EntityModel.RefreshToken>>()
        //        .Setup(m => m.Expression)
        //        .Returns(refreshTokens.Expression);
        //    mockDbSet.As<IQueryable<Entities.EntityModel.RefreshToken>>()
        //        .Setup(m => m.ElementType)
        //        .Returns(refreshTokens.ElementType);
        //    mockDbSet.As<IQueryable<Entities.EntityModel.RefreshToken>>()
        //        .Setup(m => m.GetEnumerator())
        //        .Returns(refreshTokens.GetEnumerator());

        //    _contextMock.Setup(x => x.RefreshTokens).Returns(mockDbSet.Object);

        //    // Act
        //    var resultNull = await _service.RevokeTokenAsync(nullToken, ipAddress);
        //    var resultEmpty = await _service.RevokeTokenAsync(emptyToken, ipAddress);

        //    // Assert
        //    Assert.False(resultNull.Succeeded);
        //    Assert.Equal("Token không hợp lệ.", resultNull.Message);
        //    Assert.Null(resultNull.Data);

        //    Assert.False(resultEmpty.Succeeded);
        //    Assert.Equal("Token không hợp lệ.", resultEmpty.Message);
        //    Assert.Null(resultEmpty.Data);

        //    _contextMock.Verify(x => x.RefreshTokens, Times.Exactly(2));
        //    _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        //}

        [Fact]
        public async Task RevokeTokenAsync_DatabaseException()
        {
            // Arrange
            var token = "valid-token";
            var ipAddress = "192.168.1.1";

            var refreshTokens = new List<Entities.EntityModel.RefreshToken>().AsQueryable();
            var mockDbSet = new Mock<DbSet<Entities.EntityModel.RefreshToken>>();
            mockDbSet.As<IQueryable<Entities.EntityModel.RefreshToken>>()
                .Setup(m => m.Provider)
                .Throws(new Exception("Database error"));
            mockDbSet.As<IQueryable<Entities.EntityModel.RefreshToken>>()
                .Setup(m => m.Expression)
                .Returns(refreshTokens.Expression);
            mockDbSet.As<IQueryable<Entities.EntityModel.RefreshToken>>()
                .Setup(m => m.ElementType)
                .Returns(refreshTokens.ElementType);
            mockDbSet.As<IQueryable<Entities.EntityModel.RefreshToken>>()
                .Setup(m => m.GetEnumerator())
                .Returns(refreshTokens.GetEnumerator());

            _contextMock.Setup(x => x.RefreshTokens).Returns(mockDbSet.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await _service.RevokeTokenAsync(token, ipAddress));
            Assert.Equal("Database error", exception.Message);
            _contextMock.Verify(x => x.RefreshTokens, Times.Once());
            _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }
    }

    public class TestIdentityDbContext5 : DbContext
    {
        public TestIdentityDbContext5(DbContextOptions<TestIdentityDbContext5> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Entities.EntityModel.RefreshToken> RefreshTokens { get; set; }
    }
}

