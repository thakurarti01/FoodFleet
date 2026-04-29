# FoodFleet — Start All Backend Services
# Usage: .\start-all.ps1
# Each service opens in its own terminal window

$root = $PSScriptRoot

$services = @(
    @{ name = "UserService";         path = "backend\UserService" },
    @{ name = "RestaurantService";   path = "backend\RestaurantService" },
    @{ name = "OrderService";        path = "backend\OrderService" },
    @{ name = "PaymentService";      path = "backend\PaymentService" },
    @{ name = "NotificationService"; path = "backend\NotificationService" },
    @{ name = "ApiGateway";          path = "backend\ApiGateway" }
)

foreach ($svc in $services) {
    $fullPath = Join-Path $root $svc.path
    Write-Host "Starting $($svc.name) at $fullPath ..." -ForegroundColor Cyan
    Start-Process "powershell" -ArgumentList "-NoExit", "-Command", "cd '$fullPath'; dotnet run --launch-profile http"
    Start-Sleep -Milliseconds 500
}

Write-Host ""
Write-Host "All 6 services started in separate windows." -ForegroundColor Green
Write-Host ""
Write-Host "  UserService         -> http://localhost:5213/swagger" -ForegroundColor Yellow
Write-Host "  RestaurantService   -> http://localhost:5214/swagger" -ForegroundColor Yellow
Write-Host "  OrderService        -> http://localhost:5246/swagger" -ForegroundColor Yellow
Write-Host "  PaymentService      -> http://localhost:5228/swagger" -ForegroundColor Yellow
Write-Host "  NotificationService -> http://localhost:5208/swagger" -ForegroundColor Yellow
Write-Host "  ApiGateway          -> http://localhost:5000"         -ForegroundColor Yellow
Write-Host ""
Write-Host "Frontend: cd frontend && ng serve" -ForegroundColor Magenta
