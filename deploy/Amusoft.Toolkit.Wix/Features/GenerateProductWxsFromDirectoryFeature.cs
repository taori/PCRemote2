using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GlobExpressions;

namespace Amusoft.Toolkit.Wix.Features
{
	public class GenerateProductWxsFromDirectoryFeature
	{
		private readonly string _ignoreFile;
		private readonly string _directory;
		private readonly string _targetWxsPath;

		public GenerateProductWxsFromDirectoryFeature(string directory, string targetWxsPath, string ignoreFile = null)
		{
			_ignoreFile = ignoreFile;
			_directory = directory;
			_targetWxsPath = targetWxsPath;
		}

		public async Task<int> ExecuteAsync()
		{
			var ignoreLines = await GetIgnoreLinesAsync();
			var wxsLines = GetProductWxsLines(ignoreLines);
			return await GenerateProductWxsFileAsync(wxsLines);
		}

		private async Task<int> GenerateProductWxsFileAsync(IEnumerable<string> wxsLines)
		{
			using var fileStream = new FileStream(_targetWxsPath, FileMode.Create);
			using var streamWriter = new StreamWriter(fileStream);
			foreach (var line in wxsLines)
			{
				await AppendWxsEntryAsync(line, streamWriter);
			}

			return 0;
		}

		private async Task AppendWxsEntryAsync(string file, StreamWriter streamWriter)
		{
			await streamWriter.WriteLineAsync("");
		}

		private IEnumerable<string> GetProductWxsLines(string[] ignoreLines)
		{
			var allFiles = Directory.EnumerateFiles(_directory, "*", SearchOption.AllDirectories);
			foreach (var file in allFiles)
			{
				if (ignoreLines.Length == 0)
				{
					yield return file;
					continue;
				}

				if(ignoreLines.Any(ignore => Glob.IsMatch(file, ignore, GlobOptions.CaseInsensitive)))
					continue;

				yield return file;
			}
		}

		private async Task<string[]> GetIgnoreLinesAsync()
		{
			if (!File.Exists(_ignoreFile))
				return Array.Empty<string>();

			return await File.ReadAllLinesAsync(_ignoreFile);
		}
	}
}