# FoodFleet — Stop All Backend Services
# Kills all dotnet processes running on the service ports

$ports = @(5213, 5214, 5246, 5228, 5208, 5000)

foreach ($port in $ports) {
    $pid = (Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue).OwningProcess | Select-Object -First 1
    if ($pid) {
        $proc = Get-Process -Id $pid -ErrorAction SilentlyContinue
        if ($proc) {
            Write-Host "Stopping port $port (PID $pid - $($proc.Name))" -ForegroundColor Red
            Stop-Process -Id $pid -Force
        }
    } else {
        Write-Host "Port $port not in use" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "Done." -ForegroundColor Green
