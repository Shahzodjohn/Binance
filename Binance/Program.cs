using System.Diagnostics;
using System.Net;
using Websocket.Client;

try
{
    Console.Write("Enter valutes. For example ''btcusdt'' => ");
    var valute = Console.ReadLine().ToLower();
    if (valute == null)
        Console.WriteLine("Valute not found...");
    var url = $"https://api.binance.com/api/v3/exchangeInfo?symbol={valute.ToUpper()}";
    var request = WebRequest.Create(url);
    request.Method = "GET";

    using var webResponse = request.GetResponse();
    using var webStream = webResponse.GetResponseStream();

    using var reader = new StreamReader(webStream);
    var data = reader.ReadToEnd();

    Console.WriteLine(data + "\n\n" + "Press Enter to subscribe Aggregate Trade Streams...");
    Console.Read();

    var exitEvent = new ManualResetEvent(false);
    var urlAddress = new Uri($"wss://stream.binance.com:9443/ws/{valute}@aggTrade");
    Stopwatch stopWatch = Stopwatch.StartNew();

    int startNum = 1;
    using (var client = new WebsocketClient(urlAddress))
    {
        client.ReconnectTimeout = TimeSpan.FromSeconds(30);
        client.ReconnectionHappened.Subscribe(info =>
        {
            Console.WriteLine("Reconnection happened, type: " + info.Type);
        });

        client.MessageReceived.Subscribe(msg =>
        {

            Console.WriteLine($"Message received: {startNum} => " + msg);
            if (startNum != 1000)
                startNum++;
            else if (startNum == 1000)
                exitEvent.Set();
        });
        client.Start();
        exitEvent.WaitOne();
    }
    Console.WriteLine($"Trade total time => {stopWatch.Elapsed.Hours} hours {stopWatch.Elapsed.Minutes} minutes {stopWatch.Elapsed.Seconds} seconds");
}
catch (Exception ex)
{
    Console.WriteLine("ERROR: " + ex.ToString());
}
Console.ReadKey();