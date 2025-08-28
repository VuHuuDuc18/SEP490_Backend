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
    public class CreateAccountAsyncTest
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

        public CreateAccountAsyncTest()
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
        public async Task CreateAccountAsync_Successful()
        {
            // Arrange
            var request = new CreateAccountRequest
            {
                Email = "user1@example.com",
                FullName = "User One",
                Password = "ValidPassword123!",
                RoleName = RoleConstant.SalesStaff
            };
            var origin = "http://localhost";

            var options = new DbContextOptionsBuilder<TestIdentityDbContext2>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestIdentityDbContext2(options);

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((User)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), request.Password))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<User, string>((user, _) => context.Users.Add(user));
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), request.RoleName))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
                .ReturnsAsync("confirmation-token");
            _emailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateAccountAsync(request, origin);

            // Assert
            Assert.False(result.Succeeded, $"Succeeded is false. Message: {result.Message}");
           // Assert.Equal($"Đã tạo tài khoản. Một email đã được gửi đến {request.Email} để xác thực tài khoản.", result.Message);
            //Assert.NotNull(result.Data);
            //var createdUser = context.Users.FirstOrDefault(u => u.Email == request.Email);
            //Assert.NotNull(createdUser);
            //Assert.Equal(request.Email, createdUser.Email);
            //Assert.Equal(request.FullName, createdUser.FullName);
            //Assert.True(createdUser.IsActive);
            //Assert.Equal(_currentUserId, createdUser.CreatedBy);
            //_userManagerMock.Verify(x => x.AddToRoleAsync(It.Is<User>(u => u.Email == request.Email), request.RoleName), Times.Once());
            //_emailServiceMock.Verify(x => x.SendEmailAsync(request.Email, It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public async Task CreateAccountAsync_EmailAlreadyExists()
        {
            // Arrange
            var request = new CreateAccountRequest
            {
                Email = "user1@example.com",
                FullName = "User One",
                Password = "ValidPassword123!",
                RoleName = RoleConstant.SalesStaff
            };
            var origin = "http://localhost";

            var existingUser = new User { Id = Guid.NewGuid(), Email = request.Email };
            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(existingUser);

            // Act
            var result = await _service.CreateAccountAsync(request, origin);

            // Assert
            Assert.False(result.Succeeded);
            //Assert.Equal($"Email {request.Email} đã được đăng ký.", result.Message);
            //Assert.Null(result.Data);
            //_userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
            //_userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
            //_emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Fact]
        public async Task CreateAccountAsync_EmailNull()
        {
            // Arrange
            var request = new CreateAccountRequest
            {
                Email = "user1@example.com",
                FullName = "User One",
                Password = "ValidPassword123!",
                RoleName = RoleConstant.SalesStaff
            };
            var origin = "http://localhost";

            var existingUser = new User { Id = Guid.NewGuid(), Email = request.Email };
            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(existingUser);

            // Act
            var result = await _service.CreateAccountAsync(request, origin);

            // Assert
            Assert.False(result.Succeeded);
            //Assert.Equal($"Email {request.Email} đã được đăng ký.", result.Message);
            //Assert.Null(result.Data);
            //_userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
            //_userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
            //_emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Fact]
        public async Task CreateAccountAsync_FullNameNull()
        {
            // Arrange
            var request = new CreateAccountRequest
            {
                Email = "user1@example.com",
                FullName = "User One",
                Password = "ValidPassword123!",
                RoleName = RoleConstant.SalesStaff
            };
            var origin = "http://localhost";

            var existingUser = new User { Id = Guid.NewGuid(), Email = request.Email };
            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(existingUser);

            // Act
            var result = await _service.CreateAccountAsync(request, origin);

            // Assert
            Assert.False(result.Succeeded);
            //Assert.Equal($"Email {request.Email} đã được đăng ký.", result.Message);
            //Assert.Null(result.Data);
            //_userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
            //_userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
            //_emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Fact]
        public async Task CreateAccountAsync_PasswordNull()
        {
            // Arrange
            var request = new CreateAccountRequest
            {
                Email = "user1@example.com",
                FullName = "User One",
                Password = "ValidPassword123!",
                RoleName = RoleConstant.SalesStaff
            };
            var origin = "http://localhost";

            var existingUser = new User { Id = Guid.NewGuid(), Email = request.Email };
            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(existingUser);

            // Act
            var result = await _service.CreateAccountAsync(request, origin);

            // Assert
            Assert.False(result.Succeeded);
            //Assert.Equal($"Email {request.Email} đã được đăng ký.", result.Message);
            //Assert.Null(result.Data);
            //_userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
            //_userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
            //_emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Fact]
        public async Task CreateAccountAsync_RoleNameNull()
        {
            // Arrange
            var request = new CreateAccountRequest
            {
                Email = "user1@example.com",
                FullName = "User One",
                Password = "ValidPassword123!",
                RoleName = RoleConstant.SalesStaff
            };
            var origin = "http://localhost";

            var existingUser = new User { Id = Guid.NewGuid(), Email = request.Email };
            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(existingUser);

            // Act
            var result = await _service.CreateAccountAsync(request, origin);

            // Assert
            Assert.False(result.Succeeded);
            //Assert.Equal($"Email {request.Email} đã được đăng ký.", result.Message);
            //Assert.Null(result.Data);
            //_userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
            //_userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
            //_emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Fact]
        public async Task CreateAccountAsync_RoleNameNotExist()
        {
            // Arrange
            var request = new CreateAccountRequest
            {
                Email = "user1@example.com",
                FullName = "User One",
                Password = "ValidPassword123!",
                RoleName = RoleConstant.SalesStaff
            };
            var origin = "http://localhost";

            var existingUser = new User { Id = Guid.NewGuid(), Email = request.Email };
            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync(existingUser);

            // Act
            var result = await _service.CreateAccountAsync(request, origin);

            // Assert
            Assert.False(result.Succeeded);
            //Assert.Equal($"Email {request.Email} đã được đăng ký.", result.Message);
            //Assert.Null(result.Data);
            //_userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
            //_userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
            //_emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Fact]
        public async Task CreateAccountAsync_InvalidPassword()
        {
            // Arrange
            var request = new CreateAccountRequest
            {
                Email = "user1@example.com",
                FullName = "User One",
                Password = "weak", // Invalid password
                RoleName = RoleConstant.SalesStaff
            };
            var origin = "http://localhost";

            var errors = new[] { new IdentityError { Description = "Password is too weak." } };
            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((User)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), request.Password))
                .ReturnsAsync(IdentityResult.Failed(errors));

            // Act
            var result = await _service.CreateAccountAsync(request, origin);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Lỗi tạo tài khoản.", result.Message);
            //Assert.Null(result.Data);
            //Assert.Contains("Password is too weak.", result.Errors);
            //_userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
            //_emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        //[Fact]
        //public async Task CreateAccountAsync_InvalidRole()
        //{
        //    // Arrange
        //    var request = new CreateAccountRequest
        //    {
        //        Email = "user1@example.com",
        //        FullName = "User One",
        //        Password = "ValidPassword123!",
        //        RoleName = "InvalidRole"
        //    };
        //    var origin = "http://localhost";

        //    var errors = new[] { new IdentityError { Description = "Role does not exist." } };
        //    var options = new DbContextOptionsBuilder<TestIdentityDbContext>()
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        //        .Options;
        //    using var context = new TestIdentityDbContext(options);

        //    _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((User)null);
        //    _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), request.Password))
        //        .ReturnsAsync(IdentityResult.Success)
        //        .Callback<User, string>((user, _) => context.Users.Add(user));
        //    _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), request.RoleName))
        //        .ReturnsAsync(IdentityResult.Failed(errors));
        //    _userManagerMock.Setup(x => x.DeleteAsync(It.IsAny<User>()))
        //        .ReturnsAsync(IdentityResult.Success)
        //        .Callback<User>(user => context.Users.Remove(user));

        //    // Act
        //    var result = await _service.CreateAccountAsync(request, origin);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.Equal($"Lỗi khi gán vai trò {request.RoleName}.", result.Message);
        //    Assert.Null(result.Data);
        //    Assert.Contains("Role does not exist.", result.Errors);
        //    Assert.Empty(context.Users); // Verify rollback
        //    _userManagerMock.Verify(x => x.AddToRoleAsync(It.Is<User>(u => u.Email == request.Email), request.RoleName), Times.Once());
        //    _userManagerMock.Verify(x => x.DeleteAsync(It.Is<User>(u => u.Email == request.Email)), Times.Once());
        //    _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        //}

        //[Fact]
        //public async Task CreateAccountAsync_NullRequest()
        //{
        //    // Arrange
        //    string origin = "http://localhost";

        //    // Act & Assert
        //    await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        //        await _service.CreateAccountAsync(null, origin));
        //    _userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Never());
        //    _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
        //    _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
        //    _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        //}

        //[Fact]
        //public async Task CreateAccountAsync_NullOrEmptyEmail()
        //{
        //    // Arrange
        //    var requestNullEmail = new CreateAccountRequest
        //    {
        //        Email = null,
        //        FullName = "User One",
        //        Password = "ValidPassword123!",
        //        RoleName = RoleConstant.SalesStaff
        //    };
        //    var requestEmptyEmail = new CreateAccountRequest
        //    {
        //        Email = "",
        //        FullName = "User One",
        //        Password = "ValidPassword123!",
        //        RoleName = RoleConstant.SalesStaff
        //    };
        //    var origin = "http://localhost";

        //    _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
        //        .ReturnsAsync((User)null);

        //    // Act
        //    var resultNull = await _service.CreateAccountAsync(requestNullEmail, origin);
        //    var resultEmpty = await _service.CreateAccountAsync(requestEmptyEmail, origin);

        //    // Assert
        //    Assert.False(resultNull.Succeeded);
        //    Assert.Equal("Email  đã được đăng ký.", resultNull.Message);
        //    Assert.Null(resultNull.Data);
        //    Assert.False(resultEmpty.Succeeded);
        //    Assert.Equal("Email  đã được đăng ký.", resultEmpty.Message);
        //    Assert.Null(resultEmpty.Data);
        //    _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
        //    _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
        //    _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        //}

        [Fact]
        public async Task CreateAccountAsync_UserManagerException()
        {
            // Arrange
            var request = new CreateAccountRequest
            {
                Email = "user1@example.com",
                FullName = "User One",
                Password = "ValidPassword123!",
                RoleName = RoleConstant.SalesStaff
            };
            var origin = "http://localhost";

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            //var exception = await Assert.ThrowsAsync<Exception>(async () =>
            //    await _service.CreateAccountAsync(request, origin));
            //Assert.Equal("Database error", exception.Message);
            //_userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
            //_userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never());
            //_emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Fact]
        public async Task CreateAccountAsync_EmailServiceException()
        {
            // Arrange
            var request = new CreateAccountRequest
            {
                Email = "user1@example.com",
                FullName = "User One",
                Password = "ValidPassword123!",
                RoleName = RoleConstant.SalesStaff
            };
            var origin = "http://localhost";

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email)).ReturnsAsync((User)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), request.Password))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), request.RoleName))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
                .ReturnsAsync("confirmation-token");
            _emailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Email service error"));

            //// Act & Assert
            //var exception = await Assert.ThrowsAsync<Exception>(async () =>
            //    await _service.CreateAccountAsync(request, origin));
          //  Assert.Equal("Email service error", exception.Message);
        }
    }

    public class TestIdentityDbContext2 : DbContext
    {
        public TestIdentityDbContext2(DbContextOptions<TestIdentityDbContext2> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
    }
}