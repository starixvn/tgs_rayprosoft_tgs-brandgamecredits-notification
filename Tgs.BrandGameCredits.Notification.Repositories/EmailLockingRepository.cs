using System.Text.Json;
using StackExchange.Redis;
using Tgs.BrandGameCredits.Common;
using Tgs.BrandGameCredits.Notification.Core;

namespace Tgs.BrandGameCredits.Notification.Repositories;

internal class EmailLockingRepository(
	IDatabase database,
	TimeSpan expiry) : IEmailLockingRepository
{
	public async ValueTask<bool> LockedAsync(string code, BrandGameCreditRiskTypes riskType, CancellationToken cancellationToken = default)
		=> await database.StringSetAsync(
			key: GetKey(code, riskType),
			value: JsonSerializer.Serialize(new
			{
				BrandCode = code,
				MarkOn = DateTimeOffset.UtcNow
			}),
			expiry: expiry,
			when: When.NotExists)
		.ConfigureAwait(false);

	public async ValueTask UnlockAsync(string code, BrandGameCreditRiskTypes riskType, CancellationToken cancellationToken = default)
		=> await database.KeyDeleteAsync(
			key: GetKey(code, riskType),
			flags: CommandFlags.FireAndForget)
		.ConfigureAwait(false);

	private static string GetKey(string code, BrandGameCreditRiskTypes riskType)
		=> $"brandgamecredit:notification:email:locked:{code}:{riskType}";
}
