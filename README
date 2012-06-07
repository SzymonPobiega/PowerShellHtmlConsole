# PowerShellHtmlConsole

As name suggests, this is **PowerShell** host with **HTML** (JS/AJAX) user interface. It's primary purpose is to enable running PowerShell scripts as part of interaction with a website, for example, running deployment scripts straight from release tracking website.

PSHC was built to extend deployment features of my [ReleaseCandidateTracker](https://github.com/SzymonPobiega/ReleaseCandidateTracker) application but is not dependant on RCT in any way. 

PowerShellHtmlConsole used following components:

 * JQuery
 * [JQueryTerminal](http://terminal.jcubic.pl/)
 * [NDesk.Options](http://www.ndesk.org/Options)
 * log4net
 * ASP.NET Web API
 * Newtonsoft.JSON

## Execution modes

PowerShellHtmlConsole has two execution modes.

### Interactive console

In this mode user just enters commands and watches results. To run PSHC in this mode, type

    PowerShellHtmlConsole.exe --listen=<some_url>

### Script supervision

In this mode user sees results of executing particular script. User is allowed to type only when the script requests user input (e.g. by executing Read-Host cmdlet).

To run PSHC in this mode, type

    PowerShellHtmlConsole.exe --listen=<some_url> --script=<some_script.ps1>