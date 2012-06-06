using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.SelfHost;
using log4net;
using log4net.Config;
using NDesk.Options;

namespace PowerShellHtmlConsole
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        private static readonly ManualResetEventSlim ExitEvent = new ManualResetEventSlim(false);

        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

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

            Log.Info("Initializing PowerShell");
            var powerShell = new PSWrapper(buffers, () => ExitEvent.Set());

            buffers.RegisterForInCommand((cmd, scope) => powerShell.TryExecute(cmd.TextLine));

            var config = new HttpSelfHostConfiguration(listenAddress);            
            config.Services.Insert(typeof(IHttpControllerActivator), 0, new ControllerActivator(buffers));

            config.Routes.MapHttpRoute("session", "session", new { controller = "Session" });
            config.Routes.MapHttpRoute("content", "{contentFile}", new { controller = "Content" });

            var server = new HttpSelfHostServer(config);
            Log.InfoFormat("Staring HTTP listener at {0}", listenAddress);
            server.OpenAsync().Wait();

            if (script != null)
            {
                RunScript(script, powerShell);
            }
            else
            {
                StartInteractivePrompt(buffers);
            }
            Log.InfoFormat("System ready");
            ExitEvent.Wait();
            server.CloseAsync().Wait();

            powerShell.Dispose();
        }

        private static void StartInteractivePrompt(InputOutputBuffers buffers)
        {
            Log.Info("Staring interactive prompt");
            buffers.QueueOutCommand(OutCommand.CreateReadLine(false));
        }

        private static void RunScript(string script, PSWrapper powerShell)
        {
            Log.InfoFormat("Executing script {0}", script);
            powerShell.TryExecute(". " + script);
            powerShell.Exit(0);
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: PowerShellHtmlConsole --listen=<url-to-listen> [--script=<path-to-script>]");
            Console.WriteLine("   listen: URL on which host will listen to HTTP requests");
            Console.WriteLine("   script (optional): if provided, will execute given script. Otherwise will start interactive prompt");
            Console.WriteLine("Example: PowerShellHtmlConsole --listen=http://localhost:12345 --script=.\\sample.ps1");
        }
    }
}
