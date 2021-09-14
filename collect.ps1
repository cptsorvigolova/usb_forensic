$sourcePath = Get-Location;
$regKeyUSBSTOR = 'HKLM:\SYSTEM\CurrentControlSet\Enum\USBSTOR\';
$regKeyINF = 'C:\Windows\INF';
$config = Get-Content .\config.json | ConvertFrom-Json;
$desktopPath = [Environment]::GetFolderPath("Desktop");
$saveBasePath = $desktopPath+$config.saveBasePath;
$setupapiDevPath = $regKeyINF+'\setupapi.dev.log';

Set-Location $regKeyUSBSTOR;
$devNames = Get-ChildItem;
New-Item -ItemType Directory -Force -Path $saveBasePath;
ForEach($devName in $devNames){
	Set-Location $regKeyUSBSTOR; 
	Set-Location $devName.PSObject.Properties['PSChildName'].Value;
	ForEach ($device in Get-ChildItem){
		$start = Get-Location;
		$id = $device.PSObject.Properties['PSChildName'].Value;
		Set-Location $id;
		$name = (Get-Location | Get-ItemProperty -Name FriendlyName).FriendlyName;
		Write-Host "Processing... $name $id";
		$savePath = $saveBasePath+$id+'_useLogs.txt';
		Out-File -InputObject $name -FilePath $savePath
		Select-String -Path $setupapiDevPath -Pattern $id |
		Out-File -FilePath $savePath -Append
		Set-Location $start
	}
}
Set-Location $sourcePath;