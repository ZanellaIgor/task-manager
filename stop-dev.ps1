$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$runDir = Join-Path $root ".run"

function Stop-TrackedProcess {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,
        [Parameter(Mandatory = $true)]
        [string]$PidFile
    )

    if (-not (Test-Path $PidFile)) {
        Write-Host "$Name não está registrado."
        return
    }

    $rawPid = Get-Content $PidFile -ErrorAction SilentlyContinue | Select-Object -First 1

    if ($rawPid) {
        $process = Get-Process -Id ([int]$rawPid) -ErrorAction SilentlyContinue

        if ($process) {
            cmd /c "taskkill /PID $($process.Id) /T /F" | Out-Null
            Write-Host "$Name finalizado (PID $($process.Id))."
        }
        else {
            Write-Host "$Name já não estava em execução."
        }
    }

    Remove-Item $PidFile -Force -ErrorAction SilentlyContinue
}

Stop-TrackedProcess -Name "Frontend" -PidFile (Join-Path $runDir "frontend.pid")
Stop-TrackedProcess -Name "Backend" -PidFile (Join-Path $runDir "backend.pid")
