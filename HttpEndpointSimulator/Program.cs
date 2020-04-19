using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpEndpointSimulator
{
    class Program
    {
        private static Configuration _configuration;
        private static CancellationTokenSource _cts;

        static void Main(string[] args)
        {
            // Configure members
            _configuration = new Configuration();
            _cts = new CancellationTokenSource();


            // Configure the HTTP Listening
            var listener = new HttpListener()
            {
                Prefixes = { _configuration.ListenUrl }
            };

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            var listenerLoop = new Thread(() => ListenerLoop(listener, _cts.Token));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed


            listener.Start();
            Console.WriteLine("Listening...");

            listenerLoop.Start();

            Console.ReadKey();
            Console.WriteLine("Stopping Listener...");

            // Stop running the HTTP Listener
            _cts.Cancel();
            listener.Stop();
        }

        private static async Task ListenerLoop(HttpListener listener, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var context = await listener.GetContextAsync();

                ProcessResult(context);
            }
        }

        private static void ProcessResult(HttpListenerContext context)
        {
            using (var response = context.Response)
            {
                var url = context.Request.Url.LocalPath.Trim('/');
                var fileName = url.Substring(url.LastIndexOf('/') + 1); // The file is the final part of the URL
                var folderName = url.Substring(0, url.LastIndexOf('/')); // The folder is everything but the final part of the URL
                
                Console.Write($"Received request for {url} - ");

                if (Directory.Exists(folderName))
                {
                    var files = Directory.GetFiles(folderName);
                    var fileLocation = url.Replace('/', '\\') + ".json";
                    
                    if (!files.Contains(fileLocation))
                    {
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"file '{fileName}' not found, returning 404");
                        Console.ResetColor();

                        return;
                    }

                    dynamic file = JObject.Parse(File.ReadAllText(fileLocation));

                    response.StatusCode = (int)file.responseCode;

                    byte[] responseBody = Encoding.UTF8.GetBytes(file.responseBody.ToString());
                    response.ContentLength64 = responseBody.Length;
                    response.OutputStream.Write(responseBody, 0, responseBody.Length);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"success, returning {response.StatusCode}");
                    Console.ResetColor();
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"folder '{folderName}' not found, returning 404");
                    Console.ResetColor();
                }
            }
        }
    }
}
