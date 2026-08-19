param(
    [string]$OutputDir = "samples\benchmarks"
)

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

$mixedBlock = @(
    "Aug 15 10:01:01 host sshd[1001]: Failed password for root from 192.168.1.10 port 51240 ssh2",
    "Aug 15 10:01:02 host sshd[1002]: Accepted password for alice from 192.168.1.20 port 51241 ssh2",
    "Aug 15 10:01:03 host sshd[1003]: Invalid user guest from 192.168.1.30 port 51242",
    "Aug 15 10:01:04 host sshd[1004]: Failed password for invalid user admin from 192.168.1.40 port 51243 ssh2",
    "Aug 15 10:01:05 host sudo: alice : TTY=pts/0 ; PWD=/home/alice ; USER=root ; COMMAND=/usr/bin/whoami",
    "Aug 15 10:01:06 host systemd[1]: Started unrelated service",
    "Aug 15 10:01:07 host kernel: unrelated system activity",
    "Aug 15 10:01:08 host cron[2001]: session opened for user root"
)

$noiseBlock = @(
    "Aug 15 10:01:01 host systemd[1]: Started unrelated service",
    "Aug 15 10:01:02 host kernel: unrelated system activity",
    "Aug 15 10:01:03 host cron[2001]: session opened for user root",
    "Aug 15 10:01:04 host systemd[1]: Finished routine maintenance task",
    "Aug 15 10:01:05 host NetworkManager[500]: device state changed",
    "Aug 15 10:01:06 host systemd[1]: Starting scheduled service",
    "Aug 15 10:01:07 host CRON[3001]: running scheduled task",
    "Aug 15 10:01:08 host systemd[1]: Routine service completed"
)

$securityBlock = @(
    "Aug 15 10:01:01 host sshd[1001]: Failed password for root from 192.168.1.10 port 51240 ssh2",
    "Aug 15 10:01:02 host sshd[1002]: Accepted password for alice from 192.168.1.20 port 51241 ssh2",
    "Aug 15 10:01:03 host sshd[1003]: Invalid user guest from 192.168.1.30 port 51242",
    "Aug 15 10:01:04 host sshd[1004]: Failed password for invalid user admin from 192.168.1.40 port 51243 ssh2",
    "Aug 15 10:01:05 host sudo: alice : TTY=pts/0 ; PWD=/home/alice ; USER=root ; COMMAND=/usr/bin/whoami"
)

function New-LogFile {
    param(
        [string]$Path,
        [int64]$TargetBytes,
        [string]$Workload
    )

    Write-Host "Generating $Path (~$([math]::Round($TargetBytes / 1MB, 0)) MB)..."

    $writer = [System.IO.StreamWriter]::new(
        $Path,
        $false,
        [System.Text.UTF8Encoding]::new($false),
        65536
    )

    try {
        $index = 0
        $securityIndex = 0

        while ($writer.BaseStream.Position -lt $TargetBytes) {

            if ($Workload -eq "Mixed") {
                $line = $mixedBlock[$index % $mixedBlock.Count]
            }
            else {
                # 95% noise, 5% security events
                if (($index % 20) -lt 19) {
                    $line = $noiseBlock[$index % $noiseBlock.Count]
                }
                else {
                    $line = $securityBlock[$securityIndex % $securityBlock.Count]
                    $securityIndex++
                }
            }

            $writer.WriteLine($line)
            $index++
        }
    }
    finally {
        $writer.Dispose()
    }

    $actualBytes = (Get-Item $Path).Length
    Write-Host "Created $Path - $([math]::Round($actualBytes / 1MB, 2)) MB"
}

# Mixed workload
New-LogFile "$OutputDir\mixed_10mb.log"  (10MB)   "Mixed"
New-LogFile "$OutputDir\mixed_100mb.log" (100MB)  "Mixed"
New-LogFile "$OutputDir\mixed_500mb.log" (500MB)  "Mixed"
New-LogFile "$OutputDir\mixed_1gb.log"   (1GB)    "Mixed"

# Noise-heavy workload
New-LogFile "$OutputDir\noise_100mb.log" (100MB) "Noise"
New-LogFile "$OutputDir\noise_500mb.log" (500MB) "Noise"
New-LogFile "$OutputDir\noise_1gb.log"   (1GB)   "Noise"
