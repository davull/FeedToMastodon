using FTM.App;
using FTM.Lib;
using FTM.Lib.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

#pragma warning disable S2930
var cts = new CancellationTokenSource();
#pragma warning restore S2930

Console.CancelKeyPress += (_, e) =>
{
    cts.Cancel();
    e.Cancel = true;
};

AppDomain.CurrentDomain.ProcessExit += (_, _) => cts.Cancel();

Console.WriteLine("Press Cntrl+C to stop application...");

Database.Initialize();

var host = new HostBuilder()
    .ConfigureServices(services => services
        .RegisterDependencies()
        .RegisterWorkers())
    .Build();
await host.RunAsync(cts.Token);

Console.WriteLine("Application stopped.");