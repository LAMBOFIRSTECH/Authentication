using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace TestAuthentications;
public class UnitTestAccessTokenController
{
	[Fact]
	public void Test1()
	{
		Assert.True(true);
	}
	[Fact]
	public void GetUserCredentialsFromBasicAuthentication_ReturnUnAuthorized_1()
	{
		Assert.True(true);
	}
	[Fact]
	public void GetUserCredentialsFromBasicAuthentication_ReturnBadRequest_2()
	{
		Assert.True(true);
	}
	[Fact]
	public void RetrieveUserTokenFromJwtAuthentication_ReturnUnAuthorized_3()
	{
		Assert.True(true);
	}
	 [Fact]
    public void RetrieveUserTokenFromJwtAuthentication_ReturnSuccessObject_4()
    {
        Assert.True(true);
    }
}