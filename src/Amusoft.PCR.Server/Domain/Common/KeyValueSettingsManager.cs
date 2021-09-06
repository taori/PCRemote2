using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amusoft.PCR.Model;
using Amusoft.PCR.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Amusoft.PCR.Server.Domain.Common
{
	public class KeyValueSettingsManager
	{
		private readonly ILogger<KeyValueSettingsManager> _log;
		private readonly ApplicationDbContext _dbContext;

		public KeyValueSettingsManager(ILogger<KeyValueSettingsManager> log, ApplicationDbContext dbContext)
		{
			_log = log;
			_dbContext = dbContext;
		}

		public async Task<List<KeyValueSetting>> GetAllAsync(CancellationToken cancellationToken)
		{
			return await _dbContext.KeyValueSettings.ToListAsync(cancellationToken);
		}

		public async Task<KeyValueSetting> GetByKindAsync(CancellationToken cancellationToken, KeyValueKind kind)
		{
			return await _dbContext.KeyValueSettings.FirstOrDefaultAsync(d => d.Key == kind, cancellationToken);
		}

		public async Task<bool> GetByKindAsBoolAsync(CancellationToken cancellationToken, KeyValueKind kind, bool defaultValue)
		{
			var value = await _dbContext.KeyValueSettings.FirstOrDefaultAsync(d => d.Key == kind, cancellationToken);
			return (bool.TryParse(value?.Value, out var parsed) && parsed ) || defaultValue;
		}

		public async Task<string> GetByKindAsStringAsync(CancellationToken cancellationToken, KeyValueKind kind, string defaultValue)
		{
			var value = await _dbContext.KeyValueSettings.FirstOrDefaultAsync(d => d.Key == kind, cancellationToken);
			return value?.Value ?? defaultValue;
		}

		public async Task<bool> UpdateSingleAsync(CancellationToken cancellationToken, KeyValueKind kind, string content)
		{
			var value = await _dbContext.KeyValueSettings.FirstOrDefaultAsync(d => d.Key == kind, cancellationToken);
			if (value == null)
			{
				_log.LogDebug("Updating value for {Key} to {Value}", kind, content);

				_dbContext.KeyValueSettings.Add(new KeyValueSetting() {Key = kind, Value = content});
				return await _dbContext.SaveChangesAsync(cancellationToken) > 0;
			}
			else
			{
				_log.LogDebug("Updating value for {Key} to {Value}", kind, content);
				value.Value = content;
				_dbContext.Update(value);

				return await _dbContext.SaveChangesAsync(cancellationToken) > 0;
			}
		}

		public async Task<bool> UpdateAsync(CancellationToken cancellationToken, params (KeyValueKind kind, string content)[] items)
		{
			var keyList = items.Select(d => d.kind).ToArray();
			var matchingValues = await _dbContext.KeyValueSettings.Where(d => keyList.Contains(d.Key)).ToListAsync(cancellationToken);
			var matchingLookup = matchingValues.ToDictionary(d => d.Key);
			foreach (var tuple in items)
			{
				if(matchingLookup.TryGetValue(tuple.kind, out var setting))
				{
					setting.Value = tuple.content;
					_dbContext.KeyValueSettings.Update(setting);
				} else
				{
					_dbContext.KeyValueSettings.Add(new KeyValueSetting() {Key = tuple.kind, Value = tuple.content});
				}
			}

			return await _dbContext.SaveChangesAsync(cancellationToken) > 0;
		}
	}
}