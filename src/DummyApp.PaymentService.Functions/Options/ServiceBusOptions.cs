namespace DummyApp.PaymentService.Functions.Options;

public sealed class ServiceBusOptions
{
    public const string SectionName = "ServiceBus";

    public string? ConnectionString { get; init; }
    public string? QueueName { get; init; }
}
