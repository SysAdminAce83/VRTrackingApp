using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using VRTrackingApp.Data.Models;

namespace VRTrackingApp.Web.Services.Remediation;

/// <summary>
/// Real Windows remediator. Runs PowerShell Remoting (WinRM) against the target
/// and drives the Windows Update Agent (WUA) COM API — the same thing you do
/// manually in Settings &gt; Windows Update. Active only when Remediation:Mode = "Live".
///
/// Requirements on the target: WinRM enabled + reachable from the app server, and
/// the app identity (or configured account) has local admin rights.
///
/// NOTE: remote WUA install has environmental caveats (double-hop, service state).
/// Validate against a test server before enabling for production hosts.
/// </summary>
public class WinRmWindowsUpdateProvider : IRemediationProvider
{
    private readonly WinRmOptions _opt;
    private readonly ILogger<WinRmWindowsUpdateProvider> _log;

    public WinRmWindowsUpdateProvider(IOptions<RemediationOptions> opt, ILogger<WinRmWindowsUpdateProvider> log)
    {
        _opt = opt.Value.WinRm;
        _log = log;
    }

    public string Name => "WinRM/WUA";

    public bool CanHandle(string operatingSystem) =>
        operatingSystem.Contains("windows", StringComparison.OrdinalIgnoreCase);

    public async Task<ProviderStepResult> CheckAsync(RemediationContext ctx, CancellationToken ct)
    {
        if (ctx.Registry is not null)
            return await RegistryCheckAsync(ctx, ct);

        if (string.IsNullOrWhiteSpace(ctx.PatchId))
            return ProviderStepResult.Ok(RemediationJobStates.NotApplicable,
                "No KB number was identified for this finding — manual review required.", "");

        var remote = $$"""
            $ErrorActionPreference = 'Stop'
            $kb = '{{ctx.PatchId}}'
            $installed = $null -ne (Get-HotFix -Id $kb -ErrorAction SilentlyContinue)
            $available = $false
            try {
                $session = New-Object -ComObject Microsoft.Update.Session
                $searcher = $session.CreateUpdateSearcher()
                $r = $searcher.Search("IsInstalled=0 and Type='Software'")
                foreach ($u in $r.Updates) { foreach ($k in $u.KBArticleIDs) { if (("KB$k") -ieq $kb) { $available = $true } } }
            } catch {}
            $reboot = Test-Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootPending'
            [pscustomobject]@{ Installed=$installed; Available=$available; RebootPending=$reboot } | ConvertTo-Json -Compress
            """;

        var (ok, stdout, stderr) = await RunAsync(ctx.Host, remote, ct);
        if (!ok)
            return ProviderStepResult.Fail($"Could not query {ctx.Host} over WinRM.", stderr);

        try
        {
            using var doc = JsonDocument.Parse(ExtractJson(stdout));
            var root = doc.RootElement;
            var installed = root.GetProperty("Installed").GetBoolean();
            var available = root.GetProperty("Available").GetBoolean();
            var reboot = root.GetProperty("RebootPending").GetBoolean();

            var summary = installed
                ? (reboot ? $"{ctx.PatchId} is INSTALLED but a REBOOT is pending."
                          : $"{ctx.PatchId} is INSTALLED and verified.")
                : (available ? $"{ctx.PatchId} is MISSING but AVAILABLE (ready to install)."
                             : $"{ctx.PatchId} is MISSING and not yet available on the host.");

            return ProviderStepResult.Ok(RemediationJobStates.Succeeded, summary, stdout);
        }
        catch (Exception ex)
        {
            return ProviderStepResult.Fail("Unexpected response from host.", ex + "\n" + stdout);
        }
    }

    public async Task<ProviderStepResult> InstallAsync(RemediationContext ctx, CancellationToken ct)
    {
        if (ctx.Registry is not null)
            return await RegistrySetAsync(ctx, ct);

        if (string.IsNullOrWhiteSpace(ctx.PatchId))
            return ProviderStepResult.Ok(RemediationJobStates.NotApplicable,
                "No KB number identified — cannot install automatically.", "");

        // If we have a direct download URL from MSRC enrichment, use it
        var downloadUrl = ctx.PatchDownloadUrl;
        var allKbs = ctx.AllKbNumbers;

        var remote = BuildInstallScript(ctx.PatchId, downloadUrl);

        var (ok, stdout, stderr) = await RunAsync(ctx.Host, remote, ct);
        if (!ok)
            return ProviderStepResult.Fail($"Install command failed on {ctx.Host}.", stderr);

        try
        {
            using var doc = JsonDocument.Parse(ExtractJson(stdout));
            var root = doc.RootElement;
            var status = root.GetProperty("Status").GetString();
            if (status == "NotAvailable")
                return ProviderStepResult.Ok(RemediationJobStates.NotApplicable,
                    $"{ctx.PatchId} is not available to install on {ctx.Host} yet.", stdout);

            var code = root.GetProperty("ResultCode").GetInt32();      // 2 = Succeeded, 3 = SucceededWithErrors
            var reboot = root.GetProperty("RebootRequired").GetBoolean();
            var success = code == 2 || code == 3;

            var source = root.TryGetProperty("Source", out var src) ? src.GetString() : "WUA";
            var prefix = source == "DirectDownload" ? "Downloaded & installed" : "Installed";

            return success
                ? ProviderStepResult.Ok(RemediationJobStates.Succeeded,
                    reboot ? $"{ctx.PatchId} {prefix}. Reboot required." : $"{ctx.PatchId} {prefix} and verified.", stdout)
                : ProviderStepResult.Fail($"{ctx.PatchId} install returned code {code}.", stdout);
        }
        catch (Exception ex)
        {
            return ProviderStepResult.Fail("Unexpected response from host.", ex + "\n" + stdout);
        }
    }

    private static string BuildInstallScript(string patchId, string? downloadUrl)
    {
        if (!string.IsNullOrWhiteSpace(downloadUrl))
        {
            return $$"""
                $ErrorActionPreference = 'Stop'
                $kb = '{{patchId}}'
                $downloadUrl = '{{downloadUrl}}'
                $localPath = "$env:TEMP\{{patchId}}.msu"
                Write-Host "Downloading from $downloadUrl..."
                try { Invoke-WebRequest -Uri $downloadUrl -OutFile $localPath -UseBasicParsing } catch { Write-Error "Download failed: $_"; exit 1 }
                Write-Host "Installing $localPath..."
                $result = Start-Process -FilePath 'wusa.exe' -ArgumentList "$localPath /quiet /norestart" -Wait -PassThru
                $rebootRequired = $result.ExitCode -eq 3010
                [pscustomobject]@{ Status='Installed'; ResultCode=$result.ExitCode; RebootRequired=$rebootRequired; Source='DirectDownload' } | ConvertTo-Json -Compress
                """;
        }

        return $$"""
            $ErrorActionPreference = 'Stop'
            $kb = '{{patchId}}'
            $session = New-Object -ComObject Microsoft.Update.Session
            $searcher = $session.CreateUpdateSearcher()
            $r = $searcher.Search("IsInstalled=0 and Type='Software'")
            $coll = New-Object -ComObject Microsoft.Update.UpdateColl
            foreach ($u in $r.Updates) { foreach ($k in $u.KBArticleIDs) { if (("KB$k") -ieq $kb) { $u.AcceptEula(); [void]$coll.Add($u) } } }
            if ($coll.Count -eq 0) { [pscustomobject]@{ Status='NotAvailable' } | ConvertTo-Json -Compress; return }
            $dl = $session.CreateUpdateDownloader(); $dl.Updates = $coll; [void]$dl.Download()
            $inst = $session.CreateUpdateInstaller(); $inst.Updates = $coll; $res = $inst.Install()
            [pscustomobject]@{ Status='Installed'; ResultCode=$res.ResultCode; RebootRequired=$res.RebootRequired } | ConvertTo-Json -Compress
            """;
    }

    // ---- Registry configuration remediation (e.g. Intel BHI) -------------------

    private async Task<ProviderStepResult> RegistryCheckAsync(RemediationContext ctx, CancellationToken ct)
    {
        var pb = ctx.Registry!;
        var remote = BuildDefs(pb) + """
            $res = foreach ($d in $defs) {
                $cur = $null
                try { $cur = (Get-ItemProperty -Path $d.Path -Name $d.Name -ErrorAction Stop).$($d.Name) } catch {}
                [pscustomobject]@{ Name=$d.Name; Expected=$d.Expected; Current="$cur"; Ok=("$cur" -eq "$($d.Expected)") }
            }
            @($res) | ConvertTo-Json -Compress
            """;

        var (ok, stdout, stderr) = await RunAsync(ctx.Host, remote, ct);
        if (!ok) return ProviderStepResult.Fail($"Could not read registry on {ctx.Host}.", stderr);

        var (bad, total) = CountMismatches(stdout);
        var summary = bad == 0
            ? $"Mitigation CORRECTLY configured ({total} registry value(s) match)."
            : $"Mitigation NOT fully applied: {bad} of {total} registry value(s) incorrect.";
        if (pb.RequiresMicrocode) summary += " CPU microcode/BIOS support must also be present.";
        return ProviderStepResult.Ok(RemediationJobStates.Succeeded, summary, stdout);
    }

    private async Task<ProviderStepResult> RegistrySetAsync(RemediationContext ctx, CancellationToken ct)
    {
        var pb = ctx.Registry!;
        var remote = BuildDefs(pb) + """
            foreach ($d in $defs) {
                if (-not (Test-Path $d.Path)) { New-Item -Path $d.Path -Force | Out-Null }
                $val = if ($d.Type -eq 'String') { $d.Expected } else { [int64]$d.Expected }
                New-ItemProperty -Path $d.Path -Name $d.Name -Value $val -PropertyType $d.Type -Force | Out-Null
            }
            $res = foreach ($d in $defs) {
                $cur = $null
                try { $cur = (Get-ItemProperty -Path $d.Path -Name $d.Name -ErrorAction Stop).$($d.Name) } catch {}
                [pscustomobject]@{ Name=$d.Name; Expected=$d.Expected; Current="$cur"; Ok=("$cur" -eq "$($d.Expected)") }
            }
            @($res) | ConvertTo-Json -Compress
            """;

        var (ok, stdout, stderr) = await RunAsync(ctx.Host, remote, ct);
        if (!ok) return ProviderStepResult.Fail($"Could not set registry on {ctx.Host}.", stderr);

        var (bad, total) = CountMismatches(stdout);
        if (bad > 0)
            return ProviderStepResult.Fail($"Applied registry values but {bad}/{total} did not verify.", stdout);

        var summary = $"Applied {total} registry value(s) for '{pb.Name}'.";
        if (pb.RequiresReboot) summary += " Reboot required to take effect.";
        return ProviderStepResult.Ok(RemediationJobStates.Succeeded, summary, stdout);
    }

    private static string BuildDefs(RegistryPlaybook pb)
    {
        var lines = pb.Settings.Select(s =>
            $"  [pscustomobject]@{{ Path='{Escape(s.Path)}'; Name='{Escape(s.Name)}'; Type='{Escape(s.Type)}'; Expected='{Escape(s.ExpectedDisplay)}' }}");
        return "$ErrorActionPreference='Stop'\n$defs = @(\n" + string.Join(",\n", lines) + "\n)\n";
    }

    private static (int bad, int total) CountMismatches(string stdout)
    {
        try
        {
            var json = ExtractJsonArray(stdout);
            using var doc = JsonDocument.Parse(json);
            var arr = doc.RootElement;
            int bad = 0, total = 0;
            foreach (var el in arr.EnumerateArray())
            {
                total++;
                if (!el.GetProperty("Ok").GetBoolean()) bad++;
            }
            return (bad, total);
        }
        catch { return (1, 1); }
    }

    private static string ExtractJsonArray(string s)
    {
        var start = s.IndexOf('[');
        var end = s.LastIndexOf(']');
        if (start >= 0 && end > start) return s[start..(end + 1)];
        // Single object (ConvertTo-Json collapses a 1-element array) -> wrap it.
        var os = s.IndexOf('{');
        var oe = s.LastIndexOf('}');
        return os >= 0 && oe > os ? "[" + s[os..(oe + 1)] + "]" : "[]";
    }

    /// <summary>Invokes a script block on the remote host via Invoke-Command over WinRM.</summary>
    private async Task<(bool ok, string stdout, string stderr)> RunAsync(string host, string remoteScript, CancellationToken ct)
    {
        // Build a local PowerShell that opens a remote session and runs the script.
        var sb = new StringBuilder();
        sb.AppendLine("$ErrorActionPreference='Stop'");
        sb.AppendLine($"$h='{Escape(host)}'");
        sb.AppendLine($"$so = New-PSSessionOption -SkipCACheck -SkipCNCheck");
        var cred = "";
        if (!string.IsNullOrWhiteSpace(_opt.AuthUser))
        {
            var pwd = Environment.GetEnvironmentVariable(_opt.AuthPasswordEnvVar) ?? "";
            sb.AppendLine($"$sec = ConvertTo-SecureString '{Escape(pwd)}' -AsPlainText -Force");
            sb.AppendLine($"$cred = New-Object System.Management.Automation.PSCredential('{Escape(_opt.AuthUser)}', $sec)");
            cred = "-Credential $cred ";
        }
        var ssl = _opt.UseSsl ? "-UseSSL " : "";
        sb.AppendLine($"$block = [ScriptBlock]::Create(@'\n{remoteScript}\n'@)");
        sb.AppendLine($"Invoke-Command -ComputerName $h -Port {_opt.Port} {ssl}{cred}-SessionOption $so -ScriptBlock $block");

        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = "-NoProfile -NonInteractive -ExecutionPolicy Bypass -Command -",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var proc = Process.Start(psi)!;
            await proc.StandardInput.WriteAsync(sb.ToString());
            proc.StandardInput.Close();
            var stdoutTask = proc.StandardOutput.ReadToEndAsync(ct);
            var stderrTask = proc.StandardError.ReadToEndAsync(ct);
            await proc.WaitForExitAsync(ct);
            var stdout = await stdoutTask;
            var stderr = await stderrTask;
            return (proc.ExitCode == 0 && string.IsNullOrWhiteSpace(stderr), stdout, stderr);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "WinRM invocation to {Host} failed", host);
            return (false, "", ex.Message);
        }
    }

    private static string Escape(string s) => s.Replace("'", "''");

    private static string ExtractJson(string s)
    {
        var start = s.IndexOf('{');
        var end = s.LastIndexOf('}');
        return start >= 0 && end > start ? s[start..(end + 1)] : s;
    }
}
