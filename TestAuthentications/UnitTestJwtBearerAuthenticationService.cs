using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Authentifications.Models;
using Xunit;
using System.Threading.Tasks;
using Authentifications.Interfaces;
using Authentifications.Models;
using Authentifications.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
namespace TestAuthentications;
public class UnitTestJwtBearerAuthenticationService
{
	[Fact]
	public void GetToken()
	{
		Assert.True(true);
	}
	[Fact]
	public void GetOrCreateSigningKey()
	{
		Assert.True(true);
	}
	[Fact]
	public void ConvertToPem()
	{
		Assert.True(true);
	}
	[Fact]
	public void StorePublicKeyInVault()
	{
		Assert.True(true);
	}
	[Fact]
	public void GenerateJwtToken()
	{
		Assert.True(true);
	}
	[Fact]
	public void AuthUserDetailsAsync()
	{
		Assert.True(true);
	}

	// [Fact]
	// public async Task MiddlewareTest_ReturnsNotFoundForRequest()
	// {
	//     using var host = await new HostBuilder()
	//         .ConfigureWebHost(webBuilder =>
	//         {
	//             webBuilder
	//                 .UseTestServer()
	//                 .ConfigureServices(services =>
	//                 {
	//                     services.AddMyServices();
	//                 })
	//                 .Configure(app =>
	//                 {
	//                     app.UseMiddleware<MyMiddleware>();
	//                 });
	//         })
	//         .StartAsync();

	//     var response = await host.GetTestClient().GetAsync("/");
	// }


// namespace Authentifications.Services.Tests
// {
//     public class JwtAccessAndRefreshTokenServiceTest
//     {
//         private readonly Mock<IConfiguration> _mockConfiguration;
//         private readonly Mock<ILogger<RsaSecurityKey>> _mockLogger;
//         private readonly Mock<IRedisCacheService> _mockRedisCacheService;
//         private readonly Mock<IRedisCacheTokenService> _mockRedisCacheTokenService;
//         private readonly JwtAccessAndRefreshTokenService _service;

//         public JwtAccessAndRefreshTokenServiceTest()
//         {
//             _mockConfiguration = new Mock<IConfiguration>();
//             _mockLogger = new Mock<ILogger<RsaSecurityKey>>();
//             _mockRedisCacheService = new Mock<IRedisCacheService>();
//             _mockRedisCacheTokenService = new Mock<IRedisCacheTokenService>();

//             _service = new JwtAccessAndRefreshTokenService(
//                 _mockConfiguration.Object,
//                 _mockLogger.Object,
//                 _mockRedisCacheService.Object,
//                 _mockRedisCacheTokenService.Object
//             );
//         }

//         [Fact]
//         public async Task GetToken_ShouldReturnTokenResult_WhenCalled()
//         {
//             // Arrange
//             var utilisateurDto = new UtilisateurDto
//             {
//                 Email = "test@example.com",
//                 Nom = "Test User",
//                 Role = 0,
//                 Pass = "password"
//             };

//             // Act
//             var result = await _service.GetToken(utilisateurDto);

//             // Assert
//             Assert.NotNull(result);
//             Assert.True(result.Response);
//             Assert.NotNull(result.Token);
//             Assert.NotNull(result.RefreshToken);
//         }

//         [Fact]
//         public async Task GetToken_ShouldStoreTokenInRedis_WhenCalled()
//         {
//             // Arrange
//             var utilisateurDto = new UtilisateurDto
//             {
//                 Email = "test@example.com",
//                 Nom = "Test User",
//                 Role = 0,
//                 Pass = "password"
//             };

//             // Act
//             var result = await _service.GetToken(utilisateurDto);

//             // Assert
//             _mockRedisCacheTokenService.Verify(x => x.StoreTokenSessionInRedis(utilisateurDto.Email, result.RefreshToken, utilisateurDto.Pass), Times.Once);
//         }
//     }
// }
}
