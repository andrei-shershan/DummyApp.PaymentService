using DummyApp.PaymentService.Functions.Extensions;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureAppConfiguration(config => config.AddKeyVaultFromConfiguration())
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) => services.AddBlobStorageServices(context.Configuration))
    .Build();

host.Run();