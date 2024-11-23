using FTM.App;

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

var app = Application.Create();
await app.Run(cts.Token);

Console.WriteLine("Application stopped.");