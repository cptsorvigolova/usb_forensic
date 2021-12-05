Write-Host "Rights check..."
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(`
[Security.Principal.WindowsBuiltInRole] "Administrator")) {
Write-Warning "Administrator rights required. Open PowerShell with administrator rights and run again."
Break
}
else {
Write-Host "Continue..." -ForegroundColor Green
}

$currentDriveLetter = Get-Location | Split-Path -Qualifier;

$destination = "$currentDriveLetter\Data\$([system.environment]::MachineName)";
$kapeLocation = "$currentDriveLetter\4&6\Utils\KAPE";
$username = "PPetrov";
$recentPath = "C:\Users\$username\AppData\Roaming\Microsoft\Windows\Recent";

$location = Get-Location;

New-Item -ItemType Directory -Force -Path $destination;

Get-ItemProperty -Path HKLM:\System\CurrentControlSet\Enum\USBSTOR\*\* | 
select FriendlyName, ClassGUID, Service, PSChildName | 
Export-Clixml -Path "$destination\Devices.xml";

Set-Location "$kapeLocation\Modules\bin";

.\EvtxECmd\EvtxECmd.exe -d "C:\Windows\System32\winevt\Logs" --csv $destination --csvf AllEvents.csv;

.\JLECmd.exe -d "$recentPath\AutomaticDestinations\" --csv "$destination\AutoDest";
.\JLECmd.exe -d "$recentPath\CustomDestinations\" --csv "$destination\CustDest";
.\LECmd.exe -d $recentPath --csv "$destination\LNK";
.\AmcacheParser.exe -f C:\Windows\appcompat\Programs\Amcache.hve --csv "$destination\Amcache";
.\AppCompatCacheParser.exe --csv "$destination\Shimcache";
.\SBECmd.exe -l --csv "$destination\Shellbags";
.\SrumECmd.exe -d C:\Windows\System32\sru\ --csv "$destination\SRUM";

Set-Location $kapeLocation;

$disks = Get-Partition | 
Where-Object {$_.DiskPath -like "\\?\scsi*" -and $_.DriveLetter -ne 0} | 
select DriveLetter
foreach ($disk in $disks){
    .\kape.exe --tsource $($disk.DriveLetter) --target FileSystem --tdest "$destination\USNJournal\$($disk.DriveLetter)";
    .\kape.exe --msource "$destination\USNJournal\$($disk.DriveLetter)" --module MFTECmd --mdest "$destination\USNJournal\$($disk.DriveLetter)\Parsed";
    }
Set-Location $location;