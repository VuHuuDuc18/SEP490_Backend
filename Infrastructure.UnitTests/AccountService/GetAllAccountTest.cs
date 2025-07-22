using Domain.Settings;
using Entities.EntityModel;
using Infrastructure.Identity.Contexts;
using Infrastructure.Repository;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Services.Implements;
using Xunit;
using Assert = Xunit.Assert;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.WebUtilities;

namespace Infrastructure.UnitTests.AccountService
{
    public class GetAllAccountTest
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<RoleManager<Role>> _roleManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IOptions<JWTSettings>> _jwtSettingsMock;
        private readonly Mock<IRepository<User>> _userRepositoryMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly IdentityContext _context;

        public GetAllAccountTest()
        {
            // Mock UserManager
            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

            // Mock RoleManager
            var roleStoreMock = new Mock<IRoleStore<Role>>();
            _roleManagerMock = new Mock<RoleManager<Role>>(roleStoreMock.Object, null, null, null, null);

            // Mock IUserClaimsPrincipalFactory
            var claimsPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<User>>();

            // Mock IOptions<IdentityOptions>
            var identityOptionsMock = new Mock<IOptions<IdentityOptions>>();
            identityOptionsMock.Setup(x => x.Value).Returns(new IdentityOptions());

            // Mock ILogger
            var loggerMock = new Mock<ILogger<SignInManager<User>>>();

            // Mock IAuthenticationSchemeProvider
            var authSchemeProviderMock = new Mock<IAuthenticationSchemeProvider>();

            // Mock SignInManager
            var contextAccessorMock = new Mock<IHttpContextAccessor>();

            // Mock IUrlEncoder
            var urlEncoder = UrlEncoder.Default;
            // Mock SignInManager với đầy đủ tham số
            _signInManagerMock = new Mock<SignInManager<User>>(
                _userManagerMock.Object,
                _httpContextAccessorMock.Object,
                claimsPrincipalFactoryMock.Object,
                identityOptionsMock.Object,
                loggerMock.Object,
                authSchemeProviderMock.Object,
                urlEncoder // Có thể thêm IUrlEncoder nếu cần
            );

            // Mock EmailService
            _emailServiceMock = new Mock<IEmailService>();

            // Mock JWTSettings
            _jwtSettingsMock = new Mock<IOptions<JWTSettings>>();
            _jwtSettingsMock.Setup(x => x.Value).Returns(new JWTSettings());

            // Mock UserRepository
            _userRepositoryMock = new Mock<IRepository<User>>();

            // Mock HttpContextAccessor
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            // Setup InMemory Database
            var options = new DbContextOptionsBuilder<IdentityContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new IdentityContext(options);
        }

        [Fact]
        public async Task GetAllAccountsAsync_ReturnsListOfUsers_WhenUsersExist()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), Email = "user1@example.com", UserName = "user1@example.com" },
                new User { Id = Guid.NewGuid(), Email = "user2@example.com", UserName = "user2@example.com" }
            };
            var queryableUsers = users.AsQueryable();
            _userManagerMock.Setup(x => x.Users).Returns(queryableUsers);

            var service = new Infrastructure.Services.Implements.AccountService(
                _userManagerMock.Object,
                _roleManagerMock.Object,
                _jwtSettingsMock.Object,
                _signInManagerMock.Object,
                _emailServiceMock.Object,
                _context,
                _userRepositoryMock.Object,
                _httpContextAccessorMock.Object
            );

            // Act
            var result = await service.GetAllAccountsAsync();

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal(users, (IAsyncEnumerable<User>?)result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal("Lấy danh sách tài khoản thành công.", result.Message);
        }

        [Fact]
        public async Task GetAllAccountsAsync_ReturnsEmptyList_WhenNoUsersExist()
        {
            // Arrange
            var users = new List<User>();
            var queryableUsers = users.AsQueryable();
            _userManagerMock.Setup(x => x.Users).Returns(queryableUsers);

            var service = new Infrastructure.Services.Implements.AccountService(
                _userManagerMock.Object,
                _roleManagerMock.Object,
                _jwtSettingsMock.Object,
                _signInManagerMock.Object,
                _emailServiceMock.Object,
                _context,
                _userRepositoryMock.Object,
                _httpContextAccessorMock.Object
            );

            // Act
            var result = await service.GetAllAccountsAsync();

            // Assert
            Assert.True(result.Succeeded);
            Assert.Empty(result.Data);
            Assert.Equal("Lấy danh sách tài khoản thành công.", result.Message);
        }
    }
}
