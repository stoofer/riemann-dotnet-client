using System;
using RiemannClient;
using RiemannClient.Contract;

namespace DemoClient
{
    class Program
    {
        static void Main()
        {
            using(var client = new RiemannTcpClient())
            {
                client.Send(service: "Demo Client",state: "warn", description: "Simple event description");

                client.Send(new StateEntry { Host = "c#", Service = "Demo Client (1)", State = "ok", Metric = 34.5f },
                            new StateEntry { Service = "Demo Client (2)", Metric = 34.5f });
                
                var results = client.Query("service =~ \"Demo%\"");

                foreach (var eventRecord in results)
                {
                    Console.WriteLine("Host:{0}|Service:{1}|Metric:{2}|State:{3}",
                                      eventRecord.Host, eventRecord.Service, eventRecord.metric_f, eventRecord.State);
                }
            }    
        }
    }
}
