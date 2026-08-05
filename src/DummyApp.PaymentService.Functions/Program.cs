using DummyApp.PaymentService.Functions.Extensions;
using DummyApp.PaymentService.Functions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Stripe;

var host = new HostBuilder()
    .ConfigureAppConfiguration(config => config.AddKeyVaultFromConfiguration())
    .ConfigureServices((context, services) =>
    {
        services.Configure<StripeOptions>(context.Configuration.GetSection(StripeOptions.SectionName));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<StripeOptions>>().Value);
    })
    .ConfigureFunctionsWorkerDefaults()
    .Build();

var stripeOptions = host.Services.GetRequiredService<StripeOptions>();
if (!string.IsNullOrWhiteSpace(stripeOptions.SecretKey))
{
    StripeConfiguration.ApiKey = stripeOptions.SecretKey;
}

host.Run();
