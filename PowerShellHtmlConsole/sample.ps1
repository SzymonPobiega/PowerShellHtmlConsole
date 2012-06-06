$yes = New-Object System.Management.Automation.Host.ChoiceDescription "&Yes",""
$no = New-Object System.Management.Automation.Host.ChoiceDescription "&No",""
$choices = [System.Management.Automation.Host.ChoiceDescription[]]($yes,$no)
$caption = "Warning!"
$message = "Do you want to proceed"
$result = $Host.UI.PromptForChoice($caption,$message,$choices,0)
if($result -eq 0) { Write-Host "You answered YES"
if($result -eq 1) { Write-Host "You answered NO" }
Read-Host
