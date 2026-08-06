namespace DummyApp.PaymentService.Functions;

public interface IPaymentEventPublisher
{
    Task PublishAsync(PaymentEvent paymentEvent);
}
