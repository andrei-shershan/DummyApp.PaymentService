using Azure.Messaging.ServiceBus;
using DummyApp.PaymentService.Functions;
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
        services.Configure<ServiceBusOptions>(context.Configuration.GetSection(ServiceBusOptions.SectionName));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<StripeOptions>>().Value);
        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ServiceBusOptions>>().Value;
            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                throw new InvalidOperationException($"{ServiceBusOptions.SectionName}:{nameof(ServiceBusOptions.ConnectionString)} is not configured.");
            }

            return new ServiceBusClient(options.ConnectionString);
        });

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<ServiceBusOptions>>().Value;
            if (string.IsNullOrWhiteSpace(options.QueueName))
            {
                throw new InvalidOperationException($"{ServiceBusOptions.SectionName}:{nameof(ServiceBusOptions.QueueName)} is not configured.");
            }

            var client = sp.GetRequiredService<ServiceBusClient>();
            return client.CreateSender(options.QueueName);
        });

        services.AddSingleton<IPaymentEventPublisher, ServiceBusPaymentEventPublisher>();
    })
    .ConfigureFunctionsWorkerDefaults()
    .Build();

var stripeOptions = host.Services.GetRequiredService<StripeOptions>();
if (!string.IsNullOrWhiteSpace(stripeOptions.SecretKey))
{
    StripeConfiguration.ApiKey = stripeOptions.SecretKey;
}

host.Run();
