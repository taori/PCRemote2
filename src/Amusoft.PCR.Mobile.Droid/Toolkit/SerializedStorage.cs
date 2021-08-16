using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using Xamarin.Essentials;

namespace Amusoft.PCR.Mobile.Droid.Toolkit
{
	public abstract class SerializedStorage<TItem>
	{
		private static readonly Logger Log = LogManager.GetLogger("SerializedStorage");

		private static readonly List<TItem> Empty = new List<TItem>();

		protected abstract string GetPath();

		protected abstract bool ItemEqual(TItem a, TItem b);

		public async Task<List<TItem>> LoadAsync()
		{
			var loadPath = GetPath();
			if (!File.Exists(loadPath))
				return Empty;

			Log.Trace("Loading storage file from {Path}", loadPath);
			return JsonConvert.DeserializeObject<List<TItem>>(await File.ReadAllTextAsync(loadPath));
		}

		public async Task SaveAsync(List<TItem> items)
		{
			var path = GetPath();
			Log.Trace("Saving {Count} items to storage file {Path}", items.Count, path);
			await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(items));
		}

		public async Task AddOrUpdateAsync(TItem item)
		{
			var items = await LoadAsync();
			var index = items.FindIndex(current => ItemEqual(current, item));
			if (index >= 0)
			{
				Log.Trace("Updating item for storage {Name}", GetType().FullName);
				items[index] = item;
			}
			else
			{
				Log.Trace("Adding item for storage {Name}", GetType().FullName);
				items.Add(item);
			}

			await SaveAsync(items);
		}
	}
}