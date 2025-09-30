using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TodoApp.Tests.Ui;

public sealed class TodoAppUiFactory : IAsyncDisposable
{
    private readonly Process _process;
    private readonly string _dbDirectory;
    private readonly Uri _rootUri;
    private readonly Task _stdoutTask;
    private readonly Task _stderrTask;

    private TodoAppUiFactory(Process process, string dbDirectory, Uri rootUri, Task stdoutTask, Task stderrTask)
    {
        _process = process;
        _dbDirectory = dbDirectory;
        _rootUri = rootUri;
        _stdoutTask = stdoutTask;
        _stderrTask = stderrTask;
    }

    public Uri RootUri => _rootUri;

    public static async Task<TodoAppUiFactory> StartAsync(CancellationToken cancellationToken = default)
    {
        var port = GetFreeTcpPort();
        var dbDirectory = Path.Combine(Path.GetTempPath(), $"todoapp-ui-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dbDirectory);
        var dbPath = Path.Combine(dbDirectory, "todo.db");

        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "TodoApp.Api"));

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{projectRoot}\" --urls http://127.0.0.1:{port} --no-build",
            WorkingDirectory = projectRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Testing";
        startInfo.Environment["ConnectionStrings__Default"] = $"Data Source={dbPath}";
        startInfo.Environment["DOTNET_PRINT_TELEMETRY_MESSAGE"] = "false";

        var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start TodoApp.Api process.");

        var stdoutTask = Task.Run(() => DrainStreamAsync(process.StandardOutput, cancellationToken));
        var stderrTask = Task.Run(() => DrainStreamAsync(process.StandardError, cancellationToken));

        var baseUri = new Uri($"http://127.0.0.1:{port}");
        await WaitForHealthyAsync(baseUri, process, cancellationToken).ConfigureAwait(false);

        return new TodoAppUiFactory(process, dbDirectory, baseUri, stdoutTask, stderrTask);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (!_process.HasExited)
            {
                _process.Kill(true);
            }
        }
        catch
        {
            // ignore shutdown failures
        }

        await Task.WhenAll(_stdoutTask, _stderrTask).ConfigureAwait(false);

        try
        {
            if (Directory.Exists(_dbDirectory))
            {
                Directory.Delete(_dbDirectory, recursive: true);
            }
        }
        catch
        {
            // ignore cleanup failures
        }
    }

    private static async Task DrainStreamAsync(StreamReader reader, CancellationToken cancellationToken)
    {
        char[] buffer = new char[1024];
        while (!cancellationToken.IsCancellationRequested)
        {
            var read = await reader.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }
        }
    }

    private static async Task WaitForHealthyAsync(Uri baseUri, Process process, CancellationToken cancellationToken)
    {
        using var client = new HttpClient { BaseAddress = baseUri, Timeout = TimeSpan.FromSeconds(2) };

        for (var attempt = 0; attempt < 30; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (process.HasExited)
            {
                throw new InvalidOperationException($"TodoApp.Api process exited with code {process.ExitCode} before becoming healthy.");
            }

            try
            {
                var response = await client.GetAsync("/health", cancellationToken).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch
            {
                // wait and retry
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken).ConfigureAwait(false);
        }

        throw new TimeoutException("Timed out waiting for TodoApp.Api to report healthy state.");
    }

    private static int GetFreeTcpPort()
    {
        var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
