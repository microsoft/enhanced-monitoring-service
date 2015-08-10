
param(
    [Parameter(Mandatory=$false)]
    [Switch]$loop=$true,
)


Function Show-KVP
{

    $path = 'HKLM:\SOFTWARE\Microsoft\Virtual Machine\External'
    $kvp = Get-Item -Path $path;
    if($kvp -eq $null)
    {
        Write-Error "Registry key not found:$path";
        exit(-1);
    }
    $part_0 = $kvp.GetValue("Enhanced_Monitoring_Metric_Data_Item_Part_0");

    if($part_0 -eq $null)
    {
        Write-Error "Performance data not found";
        exit(-1);
    }
    $data_0 = $part_0 | ConvertFrom-Xml
    if($data_0 -eq $null)
    {
        Write-Error "Convert to Xml failed:$part_0";
        exit(-1);
    }

    $dataStr = $data_0.data; 
    $UtcTime = Get-Date -Date "1970-01-01 00:00:00Z"
    $timestamp = $UtcTime.AddMilliseconds($data_0.ts); 

    for($i = 1; $i -lt $data_0.all; $i++)
    {
        $part_i = $kvp.GetValue("Enhanced_Monitoring_Metric_Data_Item_Part_" + $i);
        $data_i = $part_i | ConvertFrom-Xml
        if($data_i -eq $null)
        {
            Write-Error "Convert to xml failed:$part_i";
            exit(-1);
        }

        $dataStr += $data_i.data
    }

    $dataStr = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($dataStr))

    $data = ConvertFrom-Xml $dataStr

    if($selector -ne $null -and $selector -ne '')
    {
        $data = $data | Select -ExpandProperty $selector
    }

    Write-Host "-------------------------------------------------------------------------------------------";
    Write-Host $data
    Write-Host $timestamp.ToString("yyyy/MM/dd HH:mm:ss zzz")
}


Show-KVP
while($loop -eq $true)
{
    Write-Host "Waiting..." -nonewline
    for($i=60; $i -gt 0; $i--)
    {
         Write-Host "`rWaiting $i..." -nonewline
         Start-Sleep -s 1
    }
    Write-Host "`r" -nonewline
    Show-KVP
}