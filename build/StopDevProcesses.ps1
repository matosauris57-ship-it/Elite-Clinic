# Stops stale development hosts that keep build outputs locked.
# Only processes launched from this repository are stopped.
param(
    [string[]]$ProcessNames = @("DentalCare.Admin", "Elite Clinic")
)

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$currentProcessId = $PID

foreach ($processName in $ProcessNames) {
    Get-Process -Name $processName -ErrorAction SilentlyContinue | ForEach-Object {
        if ($_.Id -eq $currentProcessId) {
            return
        }

        $processPath = $null
        try {
            $processPath = $_.Path
        }
        catch {
            $processPath = $null
        }

        if ([string]::IsNullOrWhiteSpace($processPath)) {
            return
        }

        if (-not $processPath.StartsWith($repoRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
            return
        }

        Write-Host "Stopping stale dev process $($_.ProcessName) (PID $($_.Id))"
        Stop-Process -Id $_.Id -Force
    }
}
