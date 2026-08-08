namespace DummyApp.PaymentService.Functions.Options;

public sealed class ApplicationOptions
{
    public const string SectionName = "Application";

    public string? SiteId { get; init; }
}
