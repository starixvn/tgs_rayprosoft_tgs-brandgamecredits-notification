using System.Threading.Channels;

using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Logging;

using Tgs.BrandGameCredits.Notification.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.BrandGameCredits.Notification.Core;

internal class NotificationChannel(
    string key,
    ILogger<NotificationChannel> logger,
    IServiceProvider serviceProvider) : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly Channel<NotificationCommand> _channel = Channel.CreateUnbounded<NotificationCommand>(
        new UnboundedChannelOptions
        {
            SingleReader = true
        });

    private readonly SemaphoreSlim _locker = new(1, 1);
    private Task? _backgroundTask;
    private bool _disposedValue;
    /*
    private int _expiry = 330;
    private Timer? _timer;
    */

    public event EventHandler? Disposed;

    public string Key { get; } = key;

    ~NotificationChannel()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token);

        if (_backgroundTask is not null)
            return;

        await _locker.WaitAsync(cts.Token).ConfigureAwait(false);

        try
        {
            if (_backgroundTask is not null)
            {
                return;
            }
            /*
            _timer = new Timer(_ =>
            {
                _ = Interlocked.Decrement(ref _expiry);

                if (_expiry <= 0)
                {
                    Dispose();
                }
            }, null, 0, 1000);
            */

            _backgroundTask = Task.Run(
                () => ProcessCommandLoopAsync(_cancellationTokenSource.Token),
                _cancellationTokenSource.Token);
        }
        finally
        {
            _ = _locker.Release();
        }
    }

    public async ValueTask PushCommandAsync(NotificationCommand command, CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            _cancellationTokenSource.Token);

        await _channel.Writer.WriteAsync(
            command,
            cts.Token)
            .ConfigureAwait(false);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                /*
                _timer?.Dispose();
                _timer = null;
                */

                Disposed?.Invoke(this, EventArgs.Empty);
                _channel.Writer.Complete();
                _cancellationTokenSource.Cancel();
            }

            _cancellationTokenSource.Dispose();
            _disposedValue = true;
        }
    }

    private async Task ProcessCommandLoopAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        var reader = _channel.Reader;
        while (await reader
            .WaitToReadAsync(cancellationToken)
            .ConfigureAwait(false))
        {
            if (reader.TryRead(out var command))
            {
                try
                {
                    logger.LogDebug("LogOn:{logOn} Key:{key} Command:{command}", DateTime.Now.ToString("o"), Key, command);

                    await using var scope = serviceProvider.CreateAsyncScope();
                    var service = scope.ServiceProvider.GetRequiredService<ICommandService<NotificationCommand>>();
                    await service.ExecuteAsync(
                        command: command,
                        cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "LogOn:{logOn} Mesage:Process command occur errors.({command})", DateTime.Now.ToString("o"), command);
                }
            }
        }
    }
}
