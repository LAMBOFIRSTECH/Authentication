using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace TestAuthentications;
public class UnitTestAuthentificationBasicMiddleware
{
    [Fact]
    public void Test1()
    {
        //https://learn.microsoft.com/en-us/aspnet/core/test/middleware?view=aspnetcore-9.0
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
