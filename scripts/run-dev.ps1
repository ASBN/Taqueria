$ErrorActionPreference = "Stop"

Write-Host "Iniciando API en http://localhost:5082..." -ForegroundColor Cyan
$api = Start-Process dotnet -ArgumentList "run --project src/Taqueria.Api" -PassThru

Write-Host "Iniciando React Vite en http://localhost:5173..." -ForegroundColor Cyan
Push-Location src/Taqueria.Web
try {
    npm run dev
}
finally {
    Pop-Location
    if (-not $api.HasExited) {
        Stop-Process -Id $api.Id
    }
}
