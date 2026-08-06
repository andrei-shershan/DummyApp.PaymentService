using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace DummyApp.PaymentService.Functions;

public sealed class ServiceBusPaymentEventPublisher : IPaymentEventPublisher
{
    private readonly ServiceBusSender _sender;
    private readonly ILogger<ServiceBusPaymentEventPublisher> _logger;

    public ServiceBusPaymentEventPublisher(ServiceBusSender sender, ILogger<ServiceBusPaymentEventPublisher> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task PublishAsync(PaymentEvent paymentEvent)
    {
        var body = JsonSerializer.Serialize(paymentEvent);
        var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(body))
        {
            ContentType = "application/json",
            Subject = paymentEvent.EventType,
            ApplicationProperties =
            {
                ["OrderId"] = paymentEvent.OrderId,
                ["SiteId"] = paymentEvent.SiteId,
                ["PaymentStatus"] = paymentEvent.PaymentStatus
            }
        };

        try
        {
            await _sender.SendMessageAsync(message);
            _logger.LogInformation("Payment event for order {OrderId} published to Service Bus queue.", paymentEvent.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish payment event for order {OrderId} to Service Bus.", paymentEvent.OrderId);
            throw;
        }
    }
}
