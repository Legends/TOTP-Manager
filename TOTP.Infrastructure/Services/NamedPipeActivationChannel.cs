using System.IO.Pipes;
using TOTP.Core.Platform;

namespace TOTP.Infrastructure.Services;

public sealed class NamedPipeActivationDispatcher(string pipeName) : IActivationDispatcher
{
    public bool TryDispatch(ApplicationActivationRequest request)
    {
        if (!request.IsSupported) return false;
        try
        {
            using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
            client.Connect(1000);
            client.WriteByte((byte)request.Version);
            client.WriteByte((byte)request.Kind);
            client.Flush();
            return true;
        }
        catch (IOException)
        {
            return false;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }
}

public sealed class NamedPipeActivationListener(string pipeName) : IActivationListener
{
    private readonly CancellationTokenSource _lifetime = new();
    private bool _started;

    public void Start(Action<ApplicationActivationRequest> onActivation)
    {
        ArgumentNullException.ThrowIfNull(onActivation);
        if (_started) throw new InvalidOperationException("The activation listener can only be started once.");
        _started = true;
        _ = ListenAsync(onActivation, _lifetime.Token);
    }

    private async Task ListenAsync(
        Action<ApplicationActivationRequest> onActivation,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await using var server = new NamedPipeServerStream(
                    pipeName,
                    PipeDirection.In,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
                await server.WaitForConnectionAsync(cancellationToken);
                var version = server.ReadByte();
                var kind = server.ReadByte();
                if (version < 0 || kind < 0) continue;

                var request = new ApplicationActivationRequest(
                    version,
                    (ApplicationActivationKind)kind);
                if (request.IsSupported) onActivation(request);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (IOException)
            {
            }
        }
    }

    public void Dispose()
    {
        _lifetime.Cancel();
        _lifetime.Dispose();
    }
}
