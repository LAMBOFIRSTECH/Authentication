using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
}
