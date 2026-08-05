using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DummyApp.PaymentService.Functions.Options;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace DummyApp.PaymentService.Functions;

public sealed class PaymentServiceFunction
{
    private readonly StripeOptions _stripeOptions;
    private readonly ILogger<PaymentServiceFunction> _logger;

    public PaymentServiceFunction(IOptions<StripeOptions> stripeOptions, ILogger<PaymentServiceFunction> logger)
    {
        _stripeOptions = stripeOptions.Value;
        _logger = logger;
        if (!string.IsNullOrWhiteSpace(_stripeOptions.SecretKey))
        {
            StripeConfiguration.ApiKey = _stripeOptions.SecretKey;
        }
    }

    [Function("PaymentService")]
    public HttpResponseData Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "payment")] HttpRequestData req)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.WriteString("Payment service is running.");
        return response;
    }

    [Function("StripeWebhook")]
    public async Task<HttpResponseData> RunWebhook(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "payment/webhook")] HttpRequestData req)
    {
        if (!req.Headers.TryGetValues("Stripe-Signature", out var signatureValues))
        {
            _logger.LogWarning("Stripe webhook received without Stripe-Signature header.");
            return CreateBadRequest(req, "Missing Stripe-Signature header.");
        }

        var signature = signatureValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(signature))
        {
            _logger.LogWarning("Stripe webhook received with empty Stripe-Signature header.");
            return CreateBadRequest(req, "Invalid Stripe-Signature header.");
        }

        var webhookSecret = _stripeOptions.WebhookSecret;
        if (string.IsNullOrWhiteSpace(webhookSecret))
        {
            _logger.LogError("Stripe webhook secret is not configured.");
            return CreateBadRequest(req, "Stripe webhook secret is not configured.");
        }

        string payload;
        using (var reader = new StreamReader(req.Body))
        {
            payload = await reader.ReadToEndAsync();
        }

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(payload, signature, webhookSecret);
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Stripe webhook signature validation failed.");
            return CreateBadRequest(req, "Invalid webhook signature.");
        }

        _logger.LogInformation("Stripe webhook received event {EventType}.", stripeEvent.Type);

        if (stripeEvent.Type == "checkout.session.completed")
        {
            var session = stripeEvent.Data.Object as Session;
            if (session is null)
            {
                _logger.LogWarning("Stripe webhook event data is not a Checkout Session.");
            }
            else
            {
                await HandleCheckoutSessionCompletedAsync(session);
            }
        }
        else
        {
            _logger.LogInformation("Stripe webhook event {EventType} is not handled by this service.", stripeEvent.Type);
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.WriteString("Received");
        return response;
    }

    private async Task HandleCheckoutSessionCompletedAsync(Session session)
    {
        var orderId = session.Metadata.TryGetValue("orderId", out var orderIdValue) ? orderIdValue : null;
        var siteId = session.Metadata.TryGetValue("siteId", out var siteIdValue) ? siteIdValue : _stripeOptions.SiteId ?? "unknown";

        if (string.IsNullOrWhiteSpace(orderId))
        {
            _logger.LogWarning("Stripe checkout.session.completed webhook missing orderId metadata.");
            return;
        }

        _logger.LogInformation("Stripe checkout completed for order {OrderId} on site {SiteId}.", orderId, siteId);

        await PublishPaymentEventAsync(orderId, siteId, session.PaymentStatus ?? session.Status);
    }

    private Task PublishPaymentEventAsync(string orderId, string siteId, string paymentStatus)
    {
        _logger.LogInformation("Publishing payment event for order {OrderId}, site {SiteId}, status {PaymentStatus}.", orderId, siteId, paymentStatus);
        // TODO: Replace this placeholder with Azure Service Bus event publishing or direct order update logic.
        return Task.CompletedTask;
    }

    private static HttpResponseData CreateBadRequest(HttpRequestData req, string message)
    {
        var response = req.CreateResponse(HttpStatusCode.BadRequest);
        response.WriteString(message);
        return response;
    }
}
