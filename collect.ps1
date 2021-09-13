$sourcePath = Get-Location;
$saveBasePath = 'C:\Users\Klim\Desktop\Forensic\'
$regKeyUSBSTOR = 'HKLM:\SYSTEM\CurrentControlSet\Enum\USBSTOR\';
$regKeyINF = 'C:\Windows\INF';
$setupapiDevPath = $regKeyINF+'\setupapi.dev.log';
Set-Location $regKeyUSBSTOR;
$devices = Get-ChildItem;
$devices | ForEach-Object {
	Set-Location $regKeyUSBSTOR; 
	Set-Location $_.PSObject.Properties['PSChildName'].Value;
	$device = Get-ChildItem;
	$devData = $device.PSObject.Properties['PSChildName'].Value;
	$name = Get-ItemProperty $devData -Name FriendlyName;
	echo $name.FriendlyName;
	$savePath =  $saveBasePath+$name.FriendlyName+'_useLogs.txt';
	Clear-Content $savePath
	Select-String -Path $setupapiDevPath -Pattern $name.PSChildName |
	Out-File -FilePath $savePath -Append
}
Set-Location $sourcePath;