using DummyApp.PaymentService.Functions.Extensions;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureAppConfiguration(config => config.AddKeyVaultFromConfiguration())
    .ConfigureFunctionsWorkerDefaults()
    .Build();

host.Run();
