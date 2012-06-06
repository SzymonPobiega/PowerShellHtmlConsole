using System.Management.Automation.Runspaces;

namespace PowerShellHtmlConsole
{
    public interface IPSRemoteHostCallback
    {
        void Exit(int code);
        Runspace Runspace { get; set; }
    }
}