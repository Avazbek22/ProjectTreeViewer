using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using ProjectTreeViewer.Infrastructure.ThemePresets;
using ProjectTreeViewer.Tests.Unit.Helpers;
using Xunit;

namespace ProjectTreeViewer.Tests.Unit;

public sealed class ThemePresetStoreTests
{
	[Fact]
	public void Load_ReturnsDefaultsWithoutCreatingFileWhenMissing()
	{
		using var scope = new AppDataScope();
		var store = new ThemePresetStore();
		var path = store.GetPath();

		Assert.False(File.Exists(path));

		var db = store.Load();

		Assert.False(File.Exists(path));
		Assert.NotEmpty(db.Presets);

		foreach (var theme in Enum.GetValues<ThemeVariant>())
		{
			foreach (var effect in Enum.GetValues<ThemeEffectMode>())
			{
				var key = $"{theme}.{effect}";
				Assert.True(db.Presets.ContainsKey(key));
				Assert.Equal(theme, db.Presets[key].Theme);
				Assert.Equal(effect, db.Presets[key].Effect);
			}
		}
	}

	[Fact]
	public void GetPreset_AddsMissingPresetToDb()
	{
		using var scope = new AppDataScope();
		var store = new ThemePresetStore();
		var db = new ThemePresetDb { Presets = new Dictionary<string, ThemePreset>() };

		var preset = store.GetPreset(db, ThemeVariant.Dark, ThemeEffectMode.Acrylic);

		Assert.NotNull(preset);
		Assert.Equal(ThemeVariant.Dark, preset.Theme);
		Assert.Equal(ThemeEffectMode.Acrylic, preset.Effect);
		Assert.True(db.Presets.ContainsKey("Dark.Acrylic"));
		Assert.Same(preset, db.Presets["Dark.Acrylic"]);
	}

	[Fact]
	public void Save_And_Load_RoundTripsCustomPreset()
	{
		using var scope = new AppDataScope();
		var store = new ThemePresetStore();
		var custom = new ThemePreset
		{
			Theme = ThemeVariant.Light,
			Effect = ThemeEffectMode.Mica,
			MaterialIntensity = 12.5,
			BlurRadius = 9.4,
			PanelContrast = 7.3,
			MenuChildIntensity = 6.2,
			BorderStrength = 5.1
		};

		var db = new ThemePresetDb
		{
			SchemaVersion = 99,
			Presets = new Dictionary<string, ThemePreset>(),
			LastSelected = "Light.Mica"
		};

		store.SetPreset(db, ThemeVariant.Light, ThemeEffectMode.Mica, custom);
		store.Save(db);

		var loaded = store.Load();
		var loadedPreset = loaded.Presets["Light.Mica"];

		Assert.Equal(custom.Theme, loadedPreset.Theme);
		Assert.Equal(custom.Effect, loadedPreset.Effect);
		Assert.Equal(custom.MaterialIntensity, loadedPreset.MaterialIntensity);
		Assert.Equal(custom.BlurRadius, loadedPreset.BlurRadius);
		Assert.Equal(custom.PanelContrast, loadedPreset.PanelContrast);
		Assert.Equal(custom.MenuChildIntensity, loadedPreset.MenuChildIntensity);
		Assert.Equal(custom.BorderStrength, loadedPreset.BorderStrength);
		Assert.Equal("Light.Mica", loaded.LastSelected);
		Assert.True(loaded.SchemaVersion > 0);
	}

	[Fact]
	public void Load_PartialPresetList_FillsMissingCombinations()
	{
		using var scope = new AppDataScope();
		var store = new ThemePresetStore();
		var path = store.GetPath();
		var customPreset = new ThemePreset
		{
			Theme = ThemeVariant.Dark,
			Effect = ThemeEffectMode.Transparent,
			MaterialIntensity = 1,
			BlurRadius = 2,
			PanelContrast = 3,
			MenuChildIntensity = 4,
			BorderStrength = 5
		};

		var db = new ThemePresetDb
		{
			SchemaVersion = 1,
			Presets = new Dictionary<string, ThemePreset>
			{
				["Dark.Transparent"] = customPreset
			},
			LastSelected = "Dark.Transparent"
		};

		WritePresetFile(path, db);

		var loaded = store.Load();

		foreach (var theme in Enum.GetValues<ThemeVariant>())
		{
			foreach (var effect in Enum.GetValues<ThemeEffectMode>())
			{
				var key = $"{theme}.{effect}";
				Assert.True(loaded.Presets.ContainsKey(key));
			}
		}

		var reloadedPreset = loaded.Presets["Dark.Transparent"];
		Assert.Equal(customPreset.MaterialIntensity, reloadedPreset.MaterialIntensity);
		Assert.Equal(customPreset.BlurRadius, reloadedPreset.BlurRadius);
		Assert.Equal(customPreset.PanelContrast, reloadedPreset.PanelContrast);
		Assert.Equal(customPreset.MenuChildIntensity, reloadedPreset.MenuChildIntensity);
		Assert.Equal(customPreset.BorderStrength, reloadedPreset.BorderStrength);
	}

	[Fact]
	public void Load_InvalidLastSelected_ResetsToExistingKey()
	{
		using var scope = new AppDataScope();
		var store = new ThemePresetStore();
		var path = store.GetPath();
		var db = new ThemePresetDb
		{
			SchemaVersion = 1,
			Presets = new Dictionary<string, ThemePreset>(),
			LastSelected = "Not.A.Valid.Key"
		};

		WritePresetFile(path, db);

		var loaded = store.Load();

		Assert.False(string.IsNullOrWhiteSpace(loaded.LastSelected));
		Assert.True(loaded.Presets.ContainsKey(loaded.LastSelected));
		Assert.True(store.TryParseKey(loaded.LastSelected, out _, out _));
	}

	[Fact]
	public void Load_CorruptJson_RecreatesFile()
	{
		using var scope = new AppDataScope();
		var store = new ThemePresetStore();
		var path = store.GetPath();
		var directory = Path.GetDirectoryName(path);
		if (!string.IsNullOrWhiteSpace(directory))
			Directory.CreateDirectory(directory);
		File.WriteAllText(path, "{ invalid json");

		var db = store.Load();

		Assert.True(File.Exists(path));
		var json = File.ReadAllText(path);
		using var doc = JsonDocument.Parse(json);
		Assert.True(doc.RootElement.TryGetProperty("presets", out _));
		Assert.True(doc.RootElement.TryGetProperty("schemaVersion", out _));
		Assert.NotEmpty(db.Presets);
	}

	private static void WritePresetFile(string path, ThemePresetDb db)
	{
		var directory = Path.GetDirectoryName(path);
		if (!string.IsNullOrWhiteSpace(directory))
			Directory.CreateDirectory(directory);
		var json = JsonSerializer.Serialize(db, SerializerOptions);
		File.WriteAllText(path, json);
	}

	private static JsonSerializerOptions SerializerOptions => new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = true,
		Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
	};

	private sealed class AppDataScope : IDisposable
	{
		private readonly TemporaryDirectory _temp = new();
		private readonly string? _originalHome;
		private readonly string? _originalXdgConfig;
		private readonly string? _originalAppData;
		private readonly string? _originalLocalAppData;

		public AppDataScope()
		{
			_originalHome = Environment.GetEnvironmentVariable("HOME");
			_originalXdgConfig = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
			_originalAppData = Environment.GetEnvironmentVariable("APPDATA");
			_originalLocalAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA");

			Environment.SetEnvironmentVariable("HOME", _temp.Path);
			Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", _temp.Path);
			Environment.SetEnvironmentVariable("APPDATA", _temp.Path);
			Environment.SetEnvironmentVariable("LOCALAPPDATA", _temp.Path);
		}

		public void Dispose()
		{
			Environment.SetEnvironmentVariable("HOME", _originalHome);
			Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", _originalXdgConfig);
			Environment.SetEnvironmentVariable("APPDATA", _originalAppData);
			Environment.SetEnvironmentVariable("LOCALAPPDATA", _originalLocalAppData);
			_temp.Dispose();
		}
	}
}
