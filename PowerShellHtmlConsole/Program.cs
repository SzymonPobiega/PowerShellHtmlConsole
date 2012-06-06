using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.SelfHost;
using NDesk.Options;

namespace PowerShellHtmlConsole
{
    class Program
    {
        private static readonly ManualResetEventSlim ExitEvent = new ManualResetEventSlim(false);

        static void Main(string[] args)
        {
            string listenAddress = null;
            string script = null;
            bool help = false;
            var options = new OptionSet
                        {
                            {"listen=", x => listenAddress = x },
                            {"script=", x => script = x },
                            {"h|?|help", x => help = x != null},
                        };

            options.Parse(args);
            if (listenAddress == null || help)
            {
                PrintUsage();
                return;
            }

            var buffers = new InputOutputBuffers();
            var powerShell = new PSWrapper(buffers, () => ExitEvent.Set());

            buffers.RegisterForInCommand((cmd, scope) => powerShell.TryExecute(cmd.TextLine));

            var config = new HttpSelfHostConfiguration(listenAddress);            
            config.Services.Insert(typeof(IHttpControllerActivator), 0, new ControllerActivator(buffers));

            config.Routes.MapHttpRoute("session", "session", new { controller = "Session" });
            config.Routes.MapHttpRoute("content", "{contentFile}", new { controller = "Content" });

            var server = new HttpSelfHostServer(config);
            server.OpenAsync().Wait();

            if (script != null)
            {
                RunScript(script, powerShell);
            }
            else
            {
                StartInteractivePrompt(buffers);
            }
            Console.WriteLine("System ready");
            ExitEvent.Wait();
            server.CloseAsync().Wait();

            powerShell.Dispose();
        }

        private static void StartInteractivePrompt(InputOutputBuffers buffers)
        {
            buffers.QueueOutCommand(OutCommand.CreateReadLine(false));
        }

        private static void RunScript(string script, PSWrapper powerShell)
        {
            powerShell.TryExecute(". " + script);
            powerShell.Exit(0);
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
        }
    }
}
