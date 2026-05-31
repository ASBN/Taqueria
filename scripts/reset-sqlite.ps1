param(
    [switch] $AzureHome,
    [switch] $IncludeProductionFile
)

$ErrorActionPreference = "Stop"

if ($AzureHome) {
    if ([string]::IsNullOrWhiteSpace($env:HOME)) {
        throw "La variable HOME no existe. No se puede resolver HOME/Data/Taqueria."
    }

    $dataDirectory = Join-Path $env:HOME "Data/Taqueria"
}
else {
    $localData = [Environment]::GetFolderPath([Environment+SpecialFolder]::LocalApplicationData)
    $dataDirectory = Join-Path $localData "Taqueria"
}

$fileNames = @("taqueria.dev.db")
if ($IncludeProductionFile -or $AzureHome) {
    $fileNames += "taqueria.db"
}

foreach ($fileName in $fileNames) {
    foreach ($suffix in @("", "-shm", "-wal", "-journal")) {
        $file = Join-Path $dataDirectory ($fileName + $suffix)
        if (Test-Path $file) {
            Remove-Item $file -Force
            Write-Host "Eliminado: $file"
        }
    }
}

# Limpieza del almacenamiento legado utilizado antes de RC1.
foreach ($legacyFile in @(
    "src/Taqueria.Api/App_Data/taqueria.db",
    "src/Taqueria.Api/App_Data/taqueria.db-shm",
    "src/Taqueria.Api/App_Data/taqueria.db-wal",
    "src/Taqueria.Api/App_Data/taqueria.dev.db",
    "src/Taqueria.Api/App_Data/taqueria.dev.db-shm",
    "src/Taqueria.Api/App_Data/taqueria.dev.db-wal"
)) {
    if (Test-Path $legacyFile) {
        Remove-Item $legacyFile -Force
        Write-Host "Eliminado legado: $legacyFile"
    }
}

Write-Host "La próxima ejecución aplicará InitialCreateRc1 y recreará los datos seed."
