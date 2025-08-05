using CloudinaryDotNet;
using Domain.Dto.Request;
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
    public class GetListAccountTest
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

        public GetListAccountTest()
        {
            // Mock dependencies for UserManager
            var userStoreMock = new Mock<IUserStore<User>>();
            var identityOptionsMock = new Mock<IOptions<IdentityOptions>>();
            var passwordHasherMock = new Mock<IPasswordHasher<User>>();
            var lookupNormalizerMock = new Mock<ILookupNormalizer>();
            var identityErrorDescriberMock = new Mock<IdentityErrorDescriber>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerUserManagerMock = new Mock<ILogger<UserManager<User>>>();

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

        private List<User> GetSampleUsers()
        {
            return new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                Email = "user1@example.com",
                UserName = "user1@example.com",
                FullName = "User One",
                PhoneNumber = "1234567890",
                IsActive = true,
                CreatedBy = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow.AddDays(-2),
                UpdatedBy = Guid.NewGuid(),
                UpdatedDate = DateTime.UtcNow.AddDays(-1)
            },
            new User
            {
                Id = Guid.NewGuid(),
                Email = "user2@example.com",
                UserName = "user2@example.com",
                FullName = "User Two",
                PhoneNumber = "0987654321",
                IsActive = false,
                CreatedBy = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow.AddDays(-3),
                UpdatedBy = Guid.NewGuid(),
                UpdatedDate = DateTime.UtcNow.AddDays(-2)
            }
        };
        }

        // In-memory DbContext for users
        public class TestUserDbContext : DbContext
        {
            public TestUserDbContext(DbContextOptions<TestUserDbContext> options) : base(options) { }
            public DbSet<User> Users { get; set; }
        }

        //[Fact]
        //public async Task GetListAccount_NullRequest_ThrowsNullReferenceException()
        //{
        //    // Act & Assert
        //    await Assert.ThrowsAsync<NullReferenceException>(async () => await _service.GetListAccount(null));
        //    _userRepositoryMock.Verify(x => x.GetQueryable(), Times.Never());
        //    _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Never());
        //}

        [Fact]
        public async Task GetListAccount_InvalidFilterField_ReturnsAllUsers()
        {
            // Arrange
            var users = GetSampleUsers();
            var options = new DbContextOptionsBuilder<TestUserDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestUserDbContext(options);
            context.Users.AddRange(users);
            context.SaveChanges();

            _userRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.Users.AsQueryable());
            _userManagerMock.Setup(x => x.GetRolesAsync(users[0]))
                .ReturnsAsync(new List<string> { "Admin" });
            _userManagerMock.Setup(x => x.GetRolesAsync(users[1]))
                .ReturnsAsync(new List<string> { "User" });

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 2,

                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField", Value = "true" } }
            };

            // Act
            var result = await _service.GetListAccount(request);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.PageIndex);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal(2, result.Data.TotalCount);
            Assert.Equal(1, result.Data.TotalPages);
            Assert.Equal(2, result.Data.Items.Count);
            Assert.Equal(users[0].Email, result.Data.Items[0].Email);
            Assert.Equal("Admin", result.Data.Items[0].RoleName);
            Assert.Equal(users[1].Email, result.Data.Items[1].Email);
            Assert.Equal("User", result.Data.Items[1].RoleName);
            _userRepositoryMock.Verify(x => x.GetQueryable(), Times.Once());
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetListAccount_InvalidSearchField_ReturnsAllUsers()
        {
            // Arrange
            var users = GetSampleUsers();
            var options = new DbContextOptionsBuilder<TestUserDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestUserDbContext(options);
            context.Users.AddRange(users);
            context.SaveChanges();

            _userRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.Users.AsQueryable());
            _userManagerMock.Setup(x => x.GetRolesAsync(users[0]))
                .ReturnsAsync(new List<string> { "Admin" });
            _userManagerMock.Setup(x => x.GetRolesAsync(users[1]))
                .ReturnsAsync(new List<string> { "User" });

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 2,
                SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "InvalidField", Value = "user1" } }
            };

            // Act
            var result = await _service.GetListAccount(request);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.PageIndex);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal(2, result.Data.TotalCount);
            Assert.Equal(1, result.Data.TotalPages);
            Assert.Equal(2, result.Data.Items.Count);
            Assert.Equal(users[0].Email, result.Data.Items[0].Email);
            Assert.Equal("Admin", result.Data.Items[0].RoleName);
            Assert.Equal(users[1].Email, result.Data.Items[1].Email);
            Assert.Equal("User", result.Data.Items[1].RoleName);
            _userRepositoryMock.Verify(x => x.GetQueryable(), Times.Once());
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetListAccount_NonStringSearchField_ReturnsAllUsers()
        {
            // Arrange
            var users = GetSampleUsers();
            var options = new DbContextOptionsBuilder<TestUserDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestUserDbContext(options);
            context.Users.AddRange(users);
            context.SaveChanges();

            _userRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.Users.AsQueryable());
            _userManagerMock.Setup(x => x.GetRolesAsync(users[0]))
                .ReturnsAsync(new List<string> { "Admin" });
            _userManagerMock.Setup(x => x.GetRolesAsync(users[1]))
                .ReturnsAsync(new List<string> { "User" });

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 2,
                Sort = new SearchObjectForCondition { Field = "IsActive", Value = "asc" },
                SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "IsActive", Value = "true" } }
            };

            // Act
            var result = await _service.GetListAccount(request);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.PageIndex);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal(2, result.Data.TotalCount);
            Assert.Equal(1, result.Data.TotalPages);
            Assert.Equal(2, result.Data.Items.Count);
            //Assert.Equal(users[0].Email, result.Data.Items[0].Email);
            //Assert.Equal("Admin", result.Data.Items[0].RoleName);
            //Assert.Equal(users[1].Email, result.Data.Items[1].Email);
            //Assert.Equal("User", result.Data.Items[1].RoleName);
            //_userRepositoryMock.Verify(x => x.GetQueryable(), Times.Once());
            //_userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetListAccount_InvalidSortField_ReturnsAllUsers()
        {
            // Arrange
            var users = GetSampleUsers();
            var options = new DbContextOptionsBuilder<TestUserDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestUserDbContext(options);
            context.Users.AddRange(users);
            context.SaveChanges();

            _userRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.Users.AsQueryable());
            _userManagerMock.Setup(x => x.GetRolesAsync(users[0]))
                .ReturnsAsync(new List<string> { "Admin" });
            _userManagerMock.Setup(x => x.GetRolesAsync(users[1]))
                .ReturnsAsync(new List<string> { "User" });

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 2,
                Sort = new SearchObjectForCondition { Field = "InvalidField", Value = "asc" }
            };

            // Act
            var result = await _service.GetListAccount(request);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.PageIndex);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal(2, result.Data.TotalCount);
            Assert.Equal(1, result.Data.TotalPages);
            Assert.Equal(2, result.Data.Items.Count);
            Assert.Equal(users[0].Email, result.Data.Items[0].Email);
            Assert.Equal("Admin", result.Data.Items[0].RoleName);
            Assert.Equal(users[1].Email, result.Data.Items[1].Email);
            Assert.Equal("User", result.Data.Items[1].RoleName);
            _userRepositoryMock.Verify(x => x.GetQueryable(), Times.Once());
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetListAccount_Successful_NoFilteringSorting()
        {
            // Arrange
            var users = GetSampleUsers();
            var options = new DbContextOptionsBuilder<TestUserDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestUserDbContext(options);
            context.Users.AddRange(users);
            context.SaveChanges();

            _userRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.Users.AsQueryable());
            _userManagerMock.Setup(x => x.GetRolesAsync(users[0]))
                .ReturnsAsync(new List<string> { "Admin" });
            _userManagerMock.Setup(x => x.GetRolesAsync(users[1]))
                .ReturnsAsync(new List<string> { "User" });

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 2
            };

            // Act
            var result = await _service.GetListAccount(request);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.PageIndex);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal(2, result.Data.TotalCount);
            Assert.Equal(1, result.Data.TotalPages);
            Assert.Equal(2, result.Data.Items.Count);
            Assert.Equal(users[0].Email, result.Data.Items[0].Email);
            Assert.Equal("Admin", result.Data.Items[0].RoleName);
            Assert.Equal(users[1].Email, result.Data.Items[1].Email);
            Assert.Equal("User", result.Data.Items[1].RoleName);
            _userRepositoryMock.Verify(x => x.GetQueryable(), Times.Once());
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetListAccount_Successful_WithFiltering()
        {
            // Arrange
            var users = GetSampleUsers();
            var options = new DbContextOptionsBuilder<TestUserDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestUserDbContext(options);
            context.Users.AddRange(users);
            context.SaveChanges();

            _userRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.Users.AsQueryable());
            _userManagerMock.Setup(x => x.GetRolesAsync(users[0]))
                .ReturnsAsync(new List<string> { "Admin" });
            _userManagerMock.Setup(x => x.GetRolesAsync(users[1]))
                .ReturnsAsync(new List<string> { "User" });

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 2,
                Sort = new SearchObjectForCondition { Field = "IsActive", Value = "asc" },
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "IsActive", Value = "true" } }
            };

            // Act
            var result = await _service.GetListAccount(request);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.PageIndex);
            Assert.Equal(1, result.Data.Count);
            Assert.Equal(1, result.Data.TotalCount);
            Assert.Equal(1, result.Data.TotalPages);
            Assert.Single(result.Data.Items);
            Assert.Equal(users[0].Email, result.Data.Items[0].Email);
            Assert.True(result.Data.Items[0].IsActive);
            _userRepositoryMock.Verify(x => x.GetQueryable(), Times.Once());
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetListAccount_Successful_WithSearching()
        {
            // Arrange
            var users = GetSampleUsers();
            var options = new DbContextOptionsBuilder<TestUserDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestUserDbContext(options);
            context.Users.AddRange(users);
            context.SaveChanges();

            _userRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.Users.AsQueryable());
            _userManagerMock.Setup(x => x.GetRolesAsync(users[0]))
                .ReturnsAsync(new List<string> { "Admin" });
            _userManagerMock.Setup(x => x.GetRolesAsync(users[1]))
                .ReturnsAsync(new List<string> { "User" });

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 2,
                Sort = new SearchObjectForCondition { Field = "Email", Value = "asc" },
                SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "Email", Value = "user1" } }
            };

            // Act
            var result = await _service.GetListAccount(request);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.PageIndex);
            Assert.Equal(1, result.Data.Count);
            Assert.Equal(1, result.Data.TotalCount);
            Assert.Equal(1, result.Data.TotalPages);
            Assert.Single(result.Data.Items);
            Assert.Equal(users[0].Email, result.Data.Items[0].Email);
            _userRepositoryMock.Verify(x => x.GetQueryable(), Times.Once());
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetListAccount_Successful_WithSorting()
        {
            // Arrange
            var users = GetSampleUsers();
            var options = new DbContextOptionsBuilder<TestUserDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestUserDbContext(options);
            context.Users.AddRange(users);
            context.SaveChanges();

            _userRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.Users.AsQueryable());
            _userManagerMock.Setup(x => x.GetRolesAsync(users[0]))
                .ReturnsAsync(new List<string> { "Admin" });
            _userManagerMock.Setup(x => x.GetRolesAsync(users[1]))
                .ReturnsAsync(new List<string> { "User" });

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 2,
                Sort = new SearchObjectForCondition { Field = "Email", Value = "asc" }
            };

            // Act
            var result = await _service.GetListAccount(request);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.PageIndex);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal(2, result.Data.TotalCount);
            Assert.Equal(1, result.Data.TotalPages);
            Assert.Equal(2, result.Data.Items.Count);
            Assert.Equal(users[0].Email, result.Data.Items[0].Email); // user1@example.com comes first
            Assert.Equal(users[1].Email, result.Data.Items[1].Email);
            _userRepositoryMock.Verify(x => x.GetQueryable(), Times.Once());
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetListAccount_Successful_CombinedFilteringSearchingSortingPagination()
        {
            // Arrange
            var users = GetSampleUsers();
            var options = new DbContextOptionsBuilder<TestUserDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestUserDbContext(options);
            context.Users.AddRange(users);
            context.SaveChanges();

            _userRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.Users.AsQueryable());
            _userManagerMock.Setup(x => x.GetRolesAsync(users[0]))
                .ReturnsAsync(new List<string> { "Admin" });
            _userManagerMock.Setup(x => x.GetRolesAsync(users[1]))
                .ReturnsAsync(new List<string> { "User" });

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 1,
                Filter = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "IsActive", Value = "true" } },
                SearchString = new List<SearchObjectForCondition> { new SearchObjectForCondition { Field = "Email", Value = "user1" } },
                Sort = new SearchObjectForCondition { Field = "Email", Value = "asc" }
            };

            // Act
            var result = await _service.GetListAccount(request);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.PageIndex);
            Assert.Equal(1, result.Data.Count);
            Assert.Equal(1, result.Data.TotalCount);
            Assert.Equal(1, result.Data.TotalPages);
            Assert.Single(result.Data.Items);
            Assert.Equal(users[0].Email, result.Data.Items[0].Email);
            Assert.True(result.Data.Items[0].IsActive);
            _userRepositoryMock.Verify(x => x.GetQueryable(), Times.Once());
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetListAccount_EmptyUserList()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<TestUserDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new TestUserDbContext(options);

            _userRepositoryMock.Setup(x => x.GetQueryable())
                .Returns(context.Users.AsQueryable());

            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 2,
                Sort = new SearchObjectForCondition { Field = "IsActive", Value = "asc" }
            };

            // Act
            var result = await _service.GetListAccount(request);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal("", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.PageIndex);
            Assert.Equal(0, result.Data.Count);
            Assert.Equal(0, result.Data.TotalCount);
            Assert.Equal(0, result.Data.TotalPages);
            Assert.Empty(result.Data.Items);
            _userRepositoryMock.Verify(x => x.GetQueryable(), Times.Once());
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Never());
        }

        [Fact]
        public async Task GetListAccount_DatabaseException()
        {
            // Arrange
            var request = new ListingRequest
            {
                PageIndex = 1,
                PageSize = 2
            };

            _userRepositoryMock.Setup(x => x.GetQueryable())
                .Throws(new Exception("Database error"));

            // Act
            var result = await _service.GetListAccount(request);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Xảy ra lỗi khi lấy thông tin.", result.Message);
            Assert.NotNull(result.Errors);
            Assert.Contains("Database error", result.Errors);
            Assert.Null(result.Data);
            _userRepositoryMock.Verify(x => x.GetQueryable(), Times.Once());
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Never());
        }
    }
}



