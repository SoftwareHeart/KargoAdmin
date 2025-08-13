using System.Collections.Generic;
using System.Linq;
using KargoAdmin.Data;

namespace KargoAdmin.Services
{
	public interface ISettingsService
	{
		string? GetValue(string key);
	}

	public class SettingsService : ISettingsService
	{
		private readonly Dictionary<string, string> _settings;

		public SettingsService(ApplicationDbContext context)
		{
			_settings = context.Settings
				.AsQueryable()
				.ToDictionary(s => s.Key, s => s.Value);
		}

		public string? GetValue(string key)
		{
			if (string.IsNullOrWhiteSpace(key)) return null;
			return _settings.TryGetValue(key, out var value) ? value : null;
		}
	}
}


