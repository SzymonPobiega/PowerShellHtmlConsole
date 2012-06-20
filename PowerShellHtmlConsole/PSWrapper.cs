using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using log4net;

namespace PowerShellHtmlConsole
{
    public class PSWrapper : IPSRemoteHostCallback, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PSWrapper));

        private readonly InputOutputBuffers _buffers;
        private readonly Action _exitCallback;
        private readonly PSRemoteHost _psRemoteHost;
        private Runspace _runspace;
        private Timer _timer;        

        public PSWrapper(InputOutputBuffers buffers, Action exitCallback)
        {
            _buffers = buffers;
            _exitCallback = exitCallback;
            _psRemoteHost = new PSRemoteHost(buffers, this);

            var sessionState = InitialSessionState.CreateDefault();
            sessionState.Variables.Add(new SessionStateVariableEntry("cls_handler", new ClearHostHandler(buffers),"cls_handler"));

            _runspace = RunspaceFactory.CreateRunspace(_psRemoteHost, sessionState);
            _runspace.Open();

            using (var powerShell = PowerShell.Create())
            {
                powerShell.Runspace = _runspace;
                powerShell.AddScript("function Clear-Host() { $cls_handler.Clear() }");
                powerShell.Invoke();
            }
        }

        private void Execute(string cmd)
        {
            if (String.IsNullOrEmpty(cmd))
            {
                return;
            }
            Log.InfoFormat("Executing command: {0}", cmd);
            using (var powerShell = PowerShell.Create())
            {
                powerShell.Runspace = _runspace;
                powerShell.AddScript(cmd);


                // Add the default outputter to the end of the pipe and then 
                // call the MergeMyResults method to merge the output and 
                // error streams from the pipeline. This will result in the 
                // output being written using the PSHost and PSHostUserInterface 
                // classes instead of returning objects to the host application.
                powerShell.AddCommand("out-default");
                powerShell.Commands.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
                powerShell.Invoke();
            }
            if (_timer != null)
            {
                _timer.Dispose();
            }
            _timer = new Timer(x => _exitCallback(), null, TimeSpan.FromMinutes(5), TimeSpan.FromMilliseconds(-1));
        }

        /// <summary>
        /// To display an exception using the display formatter, 
        /// run a second pipeline passing in the error record.
        /// The runtime will bind this to the $input variable,
        /// which is why $input is being piped to the Out-String
        /// cmdlet. The WriteErrorLine method is called to make sure 
        /// the error gets displayed in the correct error color.
        /// </summary>
        /// <param name="e">The exception to display.</param>
        private void ReportException(Exception e)
        {
            if (e == null)
            {
                return;
            }
            var icer = e as IContainsErrorRecord;
            object error = icer != null 
                               ? icer.ErrorRecord 
                               : new ErrorRecord(e, "Host.ReportException", ErrorCategory.NotSpecified, null);

            using (var powerShell = PowerShell.Create())
            {
                powerShell.Runspace = _runspace;
                powerShell.AddScript("$input").AddCommand("out-string");

                // Do not merge errors, this function will swallow errors.
                var inputCollection = new PSDataCollection<object> {error};
                inputCollection.Complete();
                var result = powerShell.Invoke(inputCollection);

                if (result.Count > 0)
                {
                    var str = result[0].BaseObject as string;
                    if (!string.IsNullOrEmpty(str))
                    {
                        // Remove \r\n, which is added by the Out-String cmdlet.    
                        _psRemoteHost.UI.WriteErrorLine(str.Substring(0, str.Length - 2));
                    }
                }
            }
        }

        public void TryExecute(string cmd)
        {
            try
            {
                Execute(cmd);
            }
            catch (RuntimeException rte)
            {
                ReportException(rte);
            }
            finally
            {
                _buffers.QueueOutCommand(OutCommand.CreateReadLine(false, null));
            }
        }

        public void Exit(int code)
        {
            Log.Info("Exiting");
            _exitCallback();
        }

        public Runspace Runspace
        {
            get { return _runspace; }
            set { _runspace = value; }
        }

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Dispose();
            }
            _runspace.Close();
            _runspace.Dispose();
        }
    }
}