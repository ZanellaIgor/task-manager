$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$runDir = Join-Path $root ".run"

New-Item -ItemType Directory -Force -Path $runDir | Out-Null

function Get-TrackedProcess {
    param(
        [Parameter(Mandatory = $true)]
        [string]$PidFile
    )

    if (-not (Test-Path $PidFile)) {
        return $null
    }

    $rawPid = (Get-Content $PidFile -ErrorAction SilentlyContinue | Select-Object -First 1)

    if (-not $rawPid) {
        Remove-Item $PidFile -Force -ErrorAction SilentlyContinue
        return $null
    }

    $process = Get-Process -Id ([int]$rawPid) -ErrorAction SilentlyContinue

    if (-not $process) {
        Remove-Item $PidFile -Force -ErrorAction SilentlyContinue
        return $null
    }

    return $process
}

function Start-TrackedProcess {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,
        [Parameter(Mandatory = $true)]
        [string]$FilePath,
        [Parameter(Mandatory = $true)]
        [string]$Arguments,
        [Parameter(Mandatory = $true)]
        [string]$WorkingDirectory,
        [Parameter(Mandatory = $true)]
        [string]$PidFile,
        [Parameter(Mandatory = $true)]
        [string]$OutLogFile,
        [Parameter(Mandatory = $true)]
        [string]$ErrLogFile
    )

    $existing = Get-TrackedProcess -PidFile $PidFile

    if ($existing) {
        Write-Host "$Name já está em execução (PID $($existing.Id))."
        return $existing
    }

    if (Test-Path $OutLogFile) {
        Remove-Item $OutLogFile -Force
    }

    if (Test-Path $ErrLogFile) {
        Remove-Item $ErrLogFile -Force
    }

    $process = Start-Process `
        -FilePath $FilePath `
        -ArgumentList $Arguments `
        -WorkingDirectory $WorkingDirectory `
        -RedirectStandardOutput $OutLogFile `
        -RedirectStandardError $ErrLogFile `
        -PassThru

    Set-Content -Path $PidFile -Value $process.Id
    Write-Host "$Name iniciado (PID $($process.Id))."
    return $process
}

$backendPidFile = Join-Path $runDir "backend.pid"
$frontendPidFile = Join-Path $runDir "frontend.pid"
$backendOutLogFile = Join-Path $runDir "backend.out.log"
$backendErrLogFile = Join-Path $runDir "backend.err.log"
$frontendOutLogFile = Join-Path $runDir "frontend.out.log"
$frontendErrLogFile = Join-Path $runDir "frontend.err.log"

$backend = Start-TrackedProcess `
    -Name "Backend" `
    -FilePath "dotnet" `
    -Arguments "run --project .\TaskManager.Api\TaskManager.Api.csproj" `
    -WorkingDirectory (Join-Path $root "backend") `
    -PidFile $backendPidFile `
    -OutLogFile $backendOutLogFile `
    -ErrLogFile $backendErrLogFile

$frontend = Start-TrackedProcess `
    -Name "Frontend" `
    -FilePath "cmd.exe" `
    -Arguments "/c npm run dev -- --host 127.0.0.1" `
    -WorkingDirectory (Join-Path $root "frontend") `
    -PidFile $frontendPidFile `
    -OutLogFile $frontendOutLogFile `
    -ErrLogFile $frontendErrLogFile

Start-Sleep -Seconds 5

Write-Host ""
Write-Host "URLs esperadas:"
Write-Host "Backend:  http://localhost:5000/swagger"
Write-Host "Frontend: http://127.0.0.1:5173"
Write-Host ""
Write-Host "Logs:"
Write-Host $backendOutLogFile
Write-Host $backendErrLogFile
Write-Host $frontendOutLogFile
Write-Host $frontendErrLogFile
