using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace TestAuthentications;
public class UnitTestJwtBearerAuthenticationMiddleware
{
    [Fact]
    public void Test1()
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
