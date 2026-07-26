using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace DummyApp.PaymentService.Functions;

public sealed class PaymentServiceFunction
{
    [Function("PaymentService")]
    public HttpResponseData Run(
// #if DEBUG
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "payment")] HttpRequestData req)
// #else
//         [HttpTrigger(AuthorizationLevel.Function, "get", Route = "payment")] HttpRequestData req)
// #endif
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.WriteString("Hello world");
        return response;
    }
}
