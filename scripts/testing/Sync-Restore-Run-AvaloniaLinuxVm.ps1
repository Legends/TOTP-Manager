<#
.SYNOPSIS
Synchronizes the Windows working tree into an Ubuntu VM-local directory, restores it,
and starts the Avalonia desktop application in the VM's active desktop session.

.EXAMPLE
.\scripts\testing\Sync-Restore-Run-AvaloniaLinuxVm.ps1

.EXAMPLE
.\scripts\testing\Sync-Restore-Run-AvaloniaLinuxVm.ps1 -VmHost 192.168.1.50

.EXAMPLE
.\scripts\testing\Sync-Restore-Run-AvaloniaLinuxVm.ps1 -MountedRepository /mnt/totp-manager
#>
[CmdletBinding()]
param(
    [string]$VmHost,
    [string]$VmName = "Ubuntu 26.04",
    [string]$VmUser = "bushido",
    [string]$PreferredNetworkAdapter = "Stable RDP",
    [string]$LocalRepository,
    [string]$MountedRepository,
    [string]$VmRepository = "~/source/TOTP-Manager",
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

if (-not (Get-Command ssh -ErrorAction SilentlyContinue)) {
    throw "OpenSSH client (ssh.exe) is required on the Windows host."
}

if (-not [string]::IsNullOrWhiteSpace($MountedRepository) -and
    -not [string]::IsNullOrWhiteSpace($LocalRepository)) {
    throw "Specify either -LocalRepository or -MountedRepository, not both."
}

$usesMountedRepository = -not [string]::IsNullOrWhiteSpace($MountedRepository)
$localArchive = $null
$remoteArchive = $null

if (-not $usesMountedRepository) {
    if ([string]::IsNullOrWhiteSpace($LocalRepository)) {
        $LocalRepository = [IO.Path]::GetFullPath(
            (Join-Path $PSScriptRoot "..\.."))
    }
    else {
        $LocalRepository = (Resolve-Path -LiteralPath $LocalRepository).Path
    }

    if (-not (Test-Path -LiteralPath (
        Join-Path $LocalRepository "TOTP.UI.Avalonia.Desktop\TOTP.UI.Avalonia.Desktop.csproj") -PathType Leaf)) {
        throw "The local TOTP Manager repository was not found at '$LocalRepository'."
    }

    if (-not (Get-Command scp -ErrorAction SilentlyContinue)) {
        throw "OpenSSH secure copy (scp.exe) is required on the Windows host."
    }
    if (-not (Get-Command tar -ErrorAction SilentlyContinue)) {
        throw "A tar executable is required on the Windows host."
    }
}

if ([string]::IsNullOrWhiteSpace($VmHost)) {
    if (-not (Get-Command Get-VMNetworkAdapter -ErrorAction SilentlyContinue)) {
        throw "The Hyper-V PowerShell module is unavailable. Supply the VM address with -VmHost."
    }

    try {
        $vmAdapters = @(Get-VMNetworkAdapter -VMName $VmName -ErrorAction Stop |
            Sort-Object @{ Expression = {
                if ($_.Name -eq $PreferredNetworkAdapter) { 0 } else { 1 }
            } })
        $candidateAddresses = $vmAdapters | Select-Object -ExpandProperty IPAddresses
    }
    catch {
        throw "Could not inspect Hyper-V VM '$VmName'. Supply its IPv4 address with -VmHost. $($_.Exception.Message)"
    }

    $VmHost = $candidateAddresses |
        Where-Object {
            $parsedAddress = $null
            [Net.IPAddress]::TryParse($_, [ref]$parsedAddress) -and
                $parsedAddress.AddressFamily -eq [Net.Sockets.AddressFamily]::InterNetwork -and
                -not $parsedAddress.IsIPv6LinkLocal -and
                -not $_.StartsWith("127.", [StringComparison]::Ordinal) -and
                -not $_.StartsWith("169.254.", [StringComparison]::Ordinal)
        } |
        Select-Object -First 1

    if ([string]::IsNullOrWhiteSpace($VmHost)) {
        foreach ($vmAdapter in $vmAdapters) {
            $adapterMacAddress = $vmAdapter.MacAddress -replace "[:-]", ""
            $VmHost = Get-NetNeighbor -AddressFamily IPv4 -ErrorAction SilentlyContinue |
                Where-Object {
                    $neighborMac = $_.LinkLayerAddress -replace "[:-]", ""
                    $_.State -ne "Unreachable" -and
                        $neighborMac -eq $adapterMacAddress
                } |
                Select-Object -ExpandProperty IPAddress -First 1
            if (-not [string]::IsNullOrWhiteSpace($VmHost)) {
                break
            }
        }
    }

    if ([string]::IsNullOrWhiteSpace($VmHost)) {
        throw "No IPv4 address could be discovered for Hyper-V VM '$VmName'. Start and sign in to the VM, or supply its address with -VmHost."
    }

    Write-Host "Resolved Hyper-V VM '$VmName' to $VmHost."
}

$remoteScript = @'
set -Eeuo pipefail

source_root="$(printf '%s' "$1" | base64 --decode)"
source_mode="$(printf '%s' "$2" | base64 --decode)"
target_input="$(printf '%s' "$3" | base64 --decode)"
configuration="$(printf '%s' "$4" | base64 --decode)"

case "$target_input" in
    "~") target_root="$HOME" ;;
    "~/"*) target_root="$HOME/${target_input:2}" ;;
    /*) target_root="$target_input" ;;
    *) printf 'VM repository must be an absolute path or start with ~/\n' >&2; exit 2 ;;
esac

target_root="$(realpath -m -- "$target_root")"

case "$target_root" in
    "$HOME"/source/*) ;;
    *) printf 'Refusing to synchronize outside %s/source/.\n' "$HOME" >&2; exit 2 ;;
esac

archive_path=''
staging_root=''
cleanup_sync_inputs() {
    if [[ -n "$staging_root" ]]; then
        rm -rf -- "$staging_root"
    fi
    if [[ -n "$archive_path" ]]; then
        rm -f -- "$archive_path"
    fi
}
trap cleanup_sync_inputs EXIT

case "$source_mode" in
    mounted)
        source_root="$(realpath -m -- "$source_root")"
        if [[ ! -f "$source_root/TOTP.UI.Avalonia.Desktop/TOTP.UI.Avalonia.Desktop.csproj" ]]; then
            printf 'The mounted repository was not found at %s\n' "$source_root" >&2
            exit 2
        fi
        ;;
    archive)
        case "$source_root" in
            /tmp/totp-manager-sync-*.tar.gz) ;;
            *) printf 'Refusing to read an unexpected synchronization archive path.\n' >&2; exit 2 ;;
        esac
        if [[ ! -f "$source_root" ]]; then
            printf 'The synchronization archive was not found at %s\n' "$source_root" >&2
            exit 2
        fi
        archive_path="$source_root"
        mkdir -p -- "$HOME/source"
        staging_root="$(mktemp -d "$HOME/source/.totp-manager-sync.XXXXXX")"
        tar -xzf "$source_root" -C "$staging_root"
        if [[ ! -f "$staging_root/TOTP.UI.Avalonia.Desktop/TOTP.UI.Avalonia.Desktop.csproj" ]]; then
            printf 'The synchronization archive did not contain the TOTP Manager repository.\n' >&2
            exit 2
        fi
        source_root="$staging_root"
        ;;
    *)
        printf 'Unknown synchronization source mode: %s\n' "$source_mode" >&2
        exit 2
        ;;
esac

printf 'Synchronizing %s to %s...\n' "$source_root" "$target_root"
mkdir -p -- "$target_root"
rsync -a --delete \
    --exclude='.git/' \
    --exclude='.vs/' \
    --exclude='bin/' \
    --exclude='obj/' \
    --exclude='artifacts/' \
    "$source_root/" "$target_root/"

if [[ "$source_mode" == 'archive' ]]; then
    rm -f -- "$archive_path"
    rm -rf -- "$staging_root"
    archive_path=''
    staging_root=''
    source_root=''
fi

cd -- "$target_root"

printf 'Restoring the Avalonia desktop project...\n'
dotnet restore \
    TOTP.UI.Avalonia.Desktop/TOTP.UI.Avalonia.Desktop.csproj \
    --configfile NuGet.config

user_id="$(id -u)"
desktop_session_pid="$(pgrep -u "$user_id" -o xfce4-session || true)"
if [[ -z "$desktop_session_pid" ]]; then
    desktop_session_pid="$(pgrep -u "$user_id" -o xfce4-panel || true)"
fi
if [[ -n "$desktop_session_pid" && -r "/proc/$desktop_session_pid/environ" ]]; then
    while IFS= read -r -d '' session_entry; do
        case "$session_entry" in
            DISPLAY=*|WAYLAND_DISPLAY=*|XAUTHORITY=*|DBUS_SESSION_BUS_ADDRESS=*|\
            XDG_RUNTIME_DIR=*|XDG_SESSION_TYPE=*|XDG_CURRENT_DESKTOP=*|\
            XDG_CONFIG_HOME=*|XDG_DATA_HOME=*|XDG_STATE_HOME=*)
                export "$session_entry"
                ;;
        esac
    done < "/proc/$desktop_session_pid/environ"
fi

export DISPLAY="${DISPLAY:-:0}"
export XDG_RUNTIME_DIR="${XDG_RUNTIME_DIR:-/run/user/$user_id}"
if [[ -z "${DBUS_SESSION_BUS_ADDRESS:-}" && -S "$XDG_RUNTIME_DIR/bus" ]]; then
    export DBUS_SESSION_BUS_ADDRESS="unix:path=$XDG_RUNTIME_DIR/bus"
fi
if [[ -z "${XAUTHORITY:-}" && -f "$HOME/.Xauthority" ]]; then
    export XAUTHORITY="$HOME/.Xauthority"
fi

printf 'Starting TOTP Manager (%s) on display %s...\n' "$configuration" "$DISPLAY"
set +e
dotnet run \
    --project TOTP.UI.Avalonia.Desktop/TOTP.UI.Avalonia.Desktop.csproj \
    --configuration "$configuration" \
    --no-restore
app_exit_code=$?
set -e

if (( app_exit_code != 0 )); then
    log_file="${XDG_STATE_HOME:-$HOME/.local/state}/totp-manager/logs/app.log"
    printf 'TOTP Manager exited with code %s.\n' "$app_exit_code" >&2
    if [[ -f "$log_file" ]]; then
        printf 'Recent redacted application log entries from %s:\n' "$log_file" >&2
        tail -n 25 -- "$log_file" >&2
    else
        printf 'No application log was found at %s.\n' "$log_file" >&2
    fi
fi
exit "$app_exit_code"
'@

$remoteScript = $remoteScript.Replace("`r`n", "`n").Replace("`r", "`n")
$encodedScript = [Convert]::ToBase64String(
    [Text.Encoding]::UTF8.GetBytes($remoteScript))
$encodedVmRepository = [Convert]::ToBase64String(
    [Text.Encoding]::UTF8.GetBytes($VmRepository))
$encodedConfiguration = [Convert]::ToBase64String(
    [Text.Encoding]::UTF8.GetBytes($Configuration))
$remoteCommand = "printf '%s' '$encodedScript' | base64 --decode | bash -s --"
$destination = "${VmUser}@${VmHost}"

try {
    if ($usesMountedRepository) {
        $sourceMode = "mounted"
        $sourceInput = $MountedRepository
    }
    else {
        $sourceMode = "archive"
        $archiveName = "totp-manager-sync-$([Guid]::NewGuid().ToString('N')).tar.gz"
        $localArchive = Join-Path ([IO.Path]::GetTempPath()) $archiveName
        $remoteArchive = "/tmp/$archiveName"

        Write-Host "Packaging $LocalRepository..."
        Push-Location $LocalRepository
        try {
            & tar -czf $localArchive `
                --exclude='./.git' `
                --exclude='./.vs' `
                --exclude='*/bin' `
                --exclude='*/obj' `
                --exclude='./artifacts' `
                .
            if ($LASTEXITCODE -ne 0) {
                throw "Creating the repository synchronization archive failed with exit code $LASTEXITCODE."
            }
        }
        finally {
            Pop-Location
        }

        Write-Host "Uploading the working tree to $destination..."
        & scp -o ConnectTimeout=10 $localArchive "${destination}:$remoteArchive"
        if ($LASTEXITCODE -ne 0) {
            throw "Uploading the repository synchronization archive failed with exit code $LASTEXITCODE."
        }
        $sourceInput = $remoteArchive
    }

    $encodedSourceInput = [Convert]::ToBase64String(
        [Text.Encoding]::UTF8.GetBytes($sourceInput))
    $encodedSourceMode = [Convert]::ToBase64String(
        [Text.Encoding]::UTF8.GetBytes($sourceMode))

    Write-Host "Connecting to $destination..."
    & ssh -t -o ConnectTimeout=10 $destination $remoteCommand `
        $encodedSourceInput $encodedSourceMode $encodedVmRepository $encodedConfiguration
    if ($LASTEXITCODE -ne 0) {
        if ($LASTEXITCODE -eq 255) {
            throw "SSH could not connect to $destination. In the VM, install and start OpenSSH with: sudo apt install openssh-server rsync && sudo systemctl enable --now ssh"
        }
        throw "The VM test command failed with exit code $LASTEXITCODE."
    }
}
finally {
    if (-not [string]::IsNullOrWhiteSpace($localArchive) -and
        (Test-Path -LiteralPath $localArchive)) {
        Remove-Item -LiteralPath $localArchive -Force
    }
}
