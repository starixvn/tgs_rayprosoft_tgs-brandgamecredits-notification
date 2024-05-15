using System.Collections.Concurrent;

using Microsoft.Extensions.DependencyInjection;

using Tgs.BrandGameCredits.Notification.Core.ApplicationServices;

namespace Tgs.BrandGameCredits.Notification.Core;

internal class NotificationDispatcher(IServiceProvider serviceProvider) : INotificationDispatcher, IDisposable, IAsyncDisposable
{
	private readonly CancellationTokenSource _cancellationTokenSource = new();
	private readonly ConcurrentDictionary<string, NotificationChannel> _counters = new();
	private bool _disposedValue;

	~NotificationDispatcher()
	{
		Dispose(false);
	}

	public void Dispose()
	{
		// 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public ValueTask DisposeAsync()
	{
		Dispose(true);
		GC.SuppressFinalize(this);

		return ValueTask.CompletedTask;
	}

	public async ValueTask PushCommandAsync(
		NotificationCommand command,
		CancellationToken cancellationToken = default)
	{
		using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token);

		var key = string.Join('-', [command.ResellerCode, command.LicenseeCode, command.BrandCode]);
		if (!_counters.TryGetValue(key, out NotificationChannel? channel))
		{
			await InitAsync(
				key: key,
				cancellationToken: cts.Token)
				.ConfigureAwait(false);

			channel = _counters.GetValueOrDefault(key)!;
		}

		await channel.PushCommandAsync(command, cts.Token).ConfigureAwait(false);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				_cancellationTokenSource.Cancel();
			}

			_cancellationTokenSource.Dispose();
			_disposedValue = true;
		}
	}

	private ValueTask InitAsync(
				string key,
		CancellationToken cancellationToken = default)
	{
		using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token);
		var counter = _counters.GetOrAdd(
		key: key,
		valueFactory: key =>
		{
			var scope = serviceProvider.CreateScope();
			var newCounter = ActivatorUtilities.CreateInstance<NotificationChannel>(
			scope.ServiceProvider,
			key);
			newCounter.Disposed += (_, _) =>
			{
				_ = _counters.TryRemove(newCounter.Key, out _);
				scope.Dispose();
			};

			return newCounter;
		});

		return counter.InitAsync(cts.Token);
	}
}
