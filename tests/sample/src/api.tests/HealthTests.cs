using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

public sealed class HealthTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Health_is_200()
    {
        var response = await factory.CreateClient().GetAsync("/api/v1/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
