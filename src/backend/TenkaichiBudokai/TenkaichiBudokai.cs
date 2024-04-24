using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace TenkaichiBudokai;

public class TenkaichiBudokai
{
    private readonly ILogger logger;

    public TenkaichiBudokai(ILoggerFactory loggerFactory)
    {
        logger = loggerFactory.CreateLogger("negotiate");
    }

    [Function(nameof(Negotiate))]
    public async Task<HttpResponseData> Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
            [SignalRConnectionInfoInput(HubName = "tenkaichibudokai")] SignalRConnectionInfo connectionInfo )
    {
        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await response.WriteAsJsonAsync(connectionInfo);
        return response;
    }
}

public class SignalRConnectionInfo
{
    public string Url { get; set; }

    public string AccessToken { get; set; }
}

