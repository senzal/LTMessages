$bhm = 'C:\Users\steve\Documents\Guild Wars 2\addons\blishhud\modules\LTMessages.bhm'
$dll  = 'C:\Users\steve\Documents\GitHub\LTMessages\bin\Debug\net48\LTMessages.dll'
$pdb  = 'C:\Users\steve\Documents\GitHub\LTMessages\bin\Debug\net48\LTMessages.pdb'

Add-Type -AssemblyName System.IO.Compression.FileSystem

$zip = [System.IO.Compression.ZipFile]::Open($bhm, 'Update')
try {
    foreach ($name in @('LTMessages.dll', 'LTMessages.pdb')) {
        $src = if ($name -eq 'LTMessages.dll') { $dll } else { $pdb }
        $e = $zip.GetEntry($name)
        if ($e) { $e.Delete() }
        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $src, $name) | Out-Null
    }
} finally {
    $zip.Dispose()
}

Write-Host 'LTMessages.bhm updated OK'
