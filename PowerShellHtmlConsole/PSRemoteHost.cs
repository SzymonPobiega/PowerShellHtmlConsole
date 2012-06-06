using System;
using System.Globalization;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;

namespace PowerShellHtmlConsole
{
    /// <summary>
    /// A sample implementation of the PSHost abstract class for console
    /// applications. Not all members are implemented. Those that are not 
    /// implemented throw a NotImplementedException exception.
    /// </summary>
    public class PSRemoteHost : PSHost, IHostSupportsInteractiveSession
    {
        private static readonly Guid instanceId = Guid.NewGuid();
        private readonly IPSRemoteHostCallback _callbacks;
        private readonly PSRemoteUserInterface _psRemoteUserInterface;

        /// <summary>
        /// A reference to the runspace used to start an interactive session.
        /// </summary>
        private Runspace _pushedRunspace;

        public PSRemoteHost(InputOutputBuffers buffers, IPSRemoteHostCallback callbacks)
        {
            _callbacks = callbacks;
            _psRemoteUserInterface = new PSRemoteUserInterface(buffers);
        }

        
        public override CultureInfo CurrentCulture
        {
            get { return CultureInfo.InvariantCulture; }
        }

        public override CultureInfo CurrentUICulture
        {
            get { return CultureInfo.InvariantCulture; }
        }

        public override Guid InstanceId
        {
            get { return instanceId; }
        }

        public override string Name
        {
            get { return "PowerShellHtmlConsole"; }
        }

        public override PSHostUserInterface UI
        {
            get { return _psRemoteUserInterface; }
        }

        public override Version Version
        {
            get { return new Version(1, 0, 0, 0); }
        }

        #region IHostSupportsInteractiveSession Properties

        /// <summary>
        /// Gets a value indicating whether a request 
        /// to open a PSSession has been made.
        /// </summary>
        public bool IsRunspacePushed
        {
            get { return _pushedRunspace != null; }
        }

        /// <summary>
        /// Gets or sets the runspace used by the PSSession.
        /// </summary>
        public Runspace Runspace
        {
            get { return _callbacks.Runspace; }
            private set { _callbacks.Runspace = value; }
        }
        #endregion IHostSupportsInteractiveSession Properties

        /// <summary>
        /// Instructs the host to interrupt the currently running pipeline 
        /// and start a new nested input loop. Not implemented by this example class. 
        /// The call fails with an exception.
        /// </summary>
        public override void EnterNestedPrompt()
        {
            throw new NotImplementedException("Cannot suspend the shell, EnterNestedPrompt() method is not implemented by MyHost.");
        }

        /// <summary>
        /// Instructs the host to exit the currently running input loop. Not 
        /// implemented by this example class. The call fails with an 
        /// exception.
        /// </summary>
        public override void ExitNestedPrompt()
        {
            throw new NotImplementedException("The ExitNestedPrompt() method is not implemented by MyHost.");
        }

        /// <summary>
        /// Notifies the host that the Windows PowerShell runtime is about to 
        /// execute a legacy command-line application. Typically it is used 
        /// to restore state that was changed by a child process after the 
        /// child exits. This implementation does nothing and simply returns.
        /// </summary>
        public override void NotifyBeginApplication()
        {
            return;  // Do nothing.
        }

        /// <summary>
        /// Notifies the host that the Windows PowerShell engine has 
        /// completed the execution of a legacy command. Typically it 
        /// is used to restore state that was changed by a child process 
        /// after the child exits. This implementation does nothing and 
        /// simply returns.
        /// </summary>
        public override void NotifyEndApplication()
        {
            return; // Do nothing.
        }

        /// <summary>
        /// Indicate to the host application that exit has
        /// been requested. Pass the exit code that the host
        /// application should use when exiting the process.
        /// </summary>
        /// <param name="exitCode">The exit code that the host application should use.</param>
        public override void SetShouldExit(int exitCode)
        {
            _callbacks.Exit(exitCode);
        }

        #region IHostSupportsInteractiveSession Methods

        /// <summary>
        /// Requests to close a PSSession.
        /// </summary>
        public void PopRunspace()
        {
            Runspace = _pushedRunspace;
            _pushedRunspace = null;
        }

        /// <summary>
        /// Requests to open a PSSession.
        /// </summary>
        /// <param name="runspace">Runspace to use.</param>
        public void PushRunspace(Runspace runspace)
        {
            _pushedRunspace = Runspace;
            Runspace = runspace;
        }

        #endregion IHostSupportsInteractiveSession Methods
    }
}

