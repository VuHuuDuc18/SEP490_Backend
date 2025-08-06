using Domain.Settings;
using Entities.EntityModel;
using Infrastructure.Identity.Contexts;
using Infrastructure.Repository;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Assert = Xunit.Assert;

namespace Infrastructure.UnitTests.AccountService
{
    public class SendVerificationEmailTest
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

        public SendVerificationEmailTest()
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
                Options.Create(new JWTSettings()),
                _signInManagerMock.Object,
                _emailServiceMock.Object,
                _contextMock.Object,
                _userRepositoryMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        private async Task<string> InvokeSendVerificationEmail(User user, string origin)
        {
            var methodInfo = typeof(Infrastructure.Services.Implements.AccountService).GetMethod("SendVerificationEmail", BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodInfo == null)
                throw new Exception("Method SendVerificationEmail not found.");

            return await (Task<string>)methodInfo.Invoke(_service, new object[] { user, origin });
        }

        [Fact]
        public async Task SendVerificationEmail_Successful_ReturnsVerificationUri()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                UserName = "test@example.com"
            };
            var origin = "https://example.com";
            var token = "confirmation-token";
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var expectedUri = $"{origin}/api/user/confirm-email/?userId={user.Id}&code={encodedToken}";
            var expectedSubject = "XÁC NHẬN EMAIL";

            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
                .ReturnsAsync(token);
            _emailServiceMock.Setup(x => x.SendEmailAsync(user.Email, expectedSubject, It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Callback<string, string, string>((email, subject, body) =>
                {
                    Assert.Equal(user.Email, email);
                    Assert.Equal(expectedSubject, subject);
                    Assert.Contains(user.Email, body);
                    Assert.Contains(expectedUri, body);
                    Assert.Contains("Yêu cầu xác thực tài khoản!", body);
                    Assert.Contains("Vui lòng nhấp vào nút bên dưới để xác thực tài khoản", body);
                });

            // Act
            var result = await InvokeSendVerificationEmail(user, origin);

            // Assert
            Assert.Equal(expectedUri, result);
            _userManagerMock.Verify(x => x.GenerateEmailConfirmationTokenAsync(user), Times.Once());
            _emailServiceMock.Verify(x => x.SendEmailAsync(user.Email, expectedSubject, It.IsAny<string>()), Times.Once());
        }

        // Other test cases remain unchanged
        [Fact]
        public async Task SendVerificationEmail_NullUser_ThrowsArgumentNullException()
        {
            // Arrange
            var origin = "https://example.com";

            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(null))
                .ThrowsAsync(new ArgumentNullException("user"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await InvokeSendVerificationEmail(null, origin));
            _userManagerMock.Verify(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()), Times.Once());
            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Fact]
        public async Task SendVerificationEmail_EmptyEmail_ThrowsArgumentException()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "",
                UserName = ""
            };
            var origin = "https://example.com";
            var token = "confirmation-token";

            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
                .ReturnsAsync(token);
            _emailServiceMock.Setup(x => x.SendEmailAsync("", It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException("Email cannot be empty."));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => await InvokeSendVerificationEmail(user, origin));
            _userManagerMock.Verify(x => x.GenerateEmailConfirmationTokenAsync(user), Times.Once());
            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public async Task SendVerificationEmail_NullOrigin_ThrowsUriFormatException()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                UserName = "test@example.com"
            };
            string origin = null;
            var token = "confirmation-token";

            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
                .ReturnsAsync(token);

            // Act & Assert
            await Assert.ThrowsAsync<UriFormatException>(async () => await InvokeSendVerificationEmail(user, origin));
            _userManagerMock.Verify(x => x.GenerateEmailConfirmationTokenAsync(user), Times.Once());
            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Fact]
        public async Task SendVerificationEmail_InvalidOrigin_ThrowsUriFormatException()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                UserName = "test@example.com"
            };
            var origin = "invalid-url";
            var token = "confirmation-token";

            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
                .ReturnsAsync(token);

            // Act & Assert
            await Assert.ThrowsAsync<UriFormatException>(async () => await InvokeSendVerificationEmail(user, origin));
            _userManagerMock.Verify(x => x.GenerateEmailConfirmationTokenAsync(user), Times.Once());
            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Fact]
        public async Task SendVerificationEmail_TokenGenerationFailure_ThrowsException()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                UserName = "test@example.com"
            };
            var origin = "https://example.com";

            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
                .ThrowsAsync(new InvalidOperationException("Token generation failed."));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await InvokeSendVerificationEmail(user, origin));
            _userManagerMock.Verify(x => x.GenerateEmailConfirmationTokenAsync(user), Times.Once());
            _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Fact]
        public async Task SendVerificationEmail_EmailSendingFailure_ThrowsException()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                UserName = "test@example.com"
            };
            var origin = "https://example.com";
            var token = "confirmation-token";
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var expectedUri = $"{origin}/api/user/confirm-email/?userId={user.Id}&code={encodedToken}";
            var expectedSubject = "XÁC NHẬN EMAIL";

            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
                .ReturnsAsync(token);
            _emailServiceMock.Setup(x => x.SendEmailAsync(user.Email, expectedSubject, It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Email sending failed."));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await InvokeSendVerificationEmail(user, origin));
            _userManagerMock.Verify(x => x.GenerateEmailConfirmationTokenAsync(user), Times.Once());
            _emailServiceMock.Verify(x => x.SendEmailAsync(user.Email, expectedSubject, It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public async Task SendVerificationEmail_EmptyToken_ReturnsValidUri()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                UserName = "test@example.com"
            };
            var origin = "https://example.com";
            var token = "";
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var expectedUri = $"{origin}/api/user/confirm-email/?userId={user.Id}&code={encodedToken}";
            var expectedSubject = "XÁC NHẬN EMAIL";

            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
                .ReturnsAsync(token);
            _emailServiceMock.Setup(x => x.SendEmailAsync(user.Email, expectedSubject, It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Callback<string, string, string>((email, subject, body) =>
                {
                    Assert.Equal(user.Email, email);
                    Assert.Equal(expectedSubject, subject);
                    Assert.Contains(user.Email, body);
                    Assert.Contains(expectedUri, body);
                });

            // Act
            var result = await InvokeSendVerificationEmail(user, origin);

            // Assert
            Assert.Equal(expectedUri, result);
            _userManagerMock.Verify(x => x.GenerateEmailConfirmationTokenAsync(user), Times.Once());
            _emailServiceMock.Verify(x => x.SendEmailAsync(user.Email, expectedSubject, It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public async Task SendVerificationEmail_SpecialCharactersInEmail_HandlesCorrectly()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test+special@exämple.com",
                UserName = "test+special@exämple.com"
            };
            var origin = "https://example.com";
            var token = "confirmation-token";
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var expectedUri = $"{origin}/api/user/confirm-email/?userId={user.Id}&code={encodedToken}";
            var expectedSubject = "XÁC NHẬN EMAIL";

            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
                .ReturnsAsync(token);
            _emailServiceMock.Setup(x => x.SendEmailAsync(user.Email, expectedSubject, It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Callback<string, string, string>((email, subject, body) =>
                {
                    Assert.Equal(user.Email, email);
                    Assert.Equal(expectedSubject, subject);
                    Assert.Contains(user.Email, body);
                    Assert.Contains(expectedUri, body);
                });

            // Act
            var result = await InvokeSendVerificationEmail(user, origin);

            // Assert
            Assert.Equal(expectedUri, result);
            _userManagerMock.Verify(x => x.GenerateEmailConfirmationTokenAsync(user), Times.Once());
            _emailServiceMock.Verify(x => x.SendEmailAsync(user.Email, expectedSubject, It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public async Task SendVerificationEmail_LongToken_HandlesCorrectly()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                UserName = "test@example.com"
            };
            var origin = "https://example.com";
            var token = new string('a', 1000); // Long token
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var expectedUri = $"{origin}/api/user/confirm-email/?userId={user.Id}&code={encodedToken}";
            var expectedSubject = "XÁC NHẬN EMAIL";

            _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
                .ReturnsAsync(token);
            _emailServiceMock.Setup(x => x.SendEmailAsync(user.Email, expectedSubject, It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Callback<string, string, string>((email, subject, body) =>
                {
                    Assert.Equal(user.Email, email);
                    Assert.Equal(expectedSubject, subject);
                    Assert.Contains(user.Email, body);
                    Assert.Contains(expectedUri, body);
                });

            // Act
            var result = await InvokeSendVerificationEmail(user, origin);

            // Assert
            Assert.Equal(expectedUri, result);
            _userManagerMock.Verify(x => x.GenerateEmailConfirmationTokenAsync(user), Times.Once());
            _emailServiceMock.Verify(x => x.SendEmailAsync(user.Email, expectedSubject, It.IsAny<string>()), Times.Once());
        }

        //[Fact]
        //public async Task SendVerificationEmail_MissingEmailSubjectConstant_ThrowsException()
        //{
        //    // Arrange
        //    var user = new User
        //    {
        //        Id = Guid.NewGuid(),
        //        Email = "test@example.com",
        //        UserName = "test@example.com"
        //    };
        //    var origin = "https://example.com";
        //    var token = "confirmation-token";
        //    var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        //    var expectedUri = $"{origin}/api/user/confirm-email/?userId={user.Id}&code={encodedToken}";

        //    _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
        //        .ReturnsAsync(token);
        //    _emailServiceMock.Setup(x => x.SendEmailAsync(user.Email, null, It.IsAny<string>()))
        //        .ThrowsAsync(new ArgumentNullException("subject"));

        //    // Act & Assert
        //    await Assert.ThrowsAsync<ArgumentNullException>(async () => await InvokeSendVerificationEmail(user, origin));
        //    _userManagerMock.Verify(x => x.GenerateEmailConfirmationTokenAsync(user), Times.Once());
        //    _emailServiceMock.Verify(x => x.SendEmailAsync(user.Email, null, It.IsAny<string>()), Times.Once());
        //}
    }
}
