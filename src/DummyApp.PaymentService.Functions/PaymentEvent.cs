namespace DummyApp.PaymentService.Functions;

public sealed record PaymentEvent(
    string OrderId,
    string SiteId,
    string PaymentStatus,
    string EventType
);
