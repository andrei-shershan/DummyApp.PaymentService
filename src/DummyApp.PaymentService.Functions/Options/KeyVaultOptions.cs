namespace DummyApp.PaymentService.Functions.Options;

public sealed class KeyVaultOptions
{
    public const string SectionName = "KeyVault";

    public string? Url { get; init; }
}

public sealed class StripeOptions
{
    public const string SectionName = "Stripe";

    public string? SecretKey { get; init; }
    public string? WebhookSecret { get; init; }
    public string? SiteId { get; init; }
    public string? SuccessUrl { get; init; }
    public string? CancelUrl { get; init; }
}
