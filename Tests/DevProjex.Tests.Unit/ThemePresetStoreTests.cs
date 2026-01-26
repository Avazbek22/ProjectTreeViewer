using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using DevProjex.Infrastructure.ThemePresets;
using DevProjex.Tests.Unit.Helpers;
using Xunit;

namespace DevProjex.Tests.Unit;

public sealed class ThemePresetStoreTests
{
	[Fact]
	// Ensures loading without a file returns defaults without requiring persistence.
	public void Load_ReturnsDefaultsWithoutCreatingFileWhenMissing()
	{
		using var scope = new AppDataScope();
		var store = new ThemePresetStore();
		var path = store.GetPath();

		if (File.Exists(path))
			File.Delete(path);

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
	// Ensures GetPreset populates missing preset entries in the database.
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
	// Ensures a custom preset survives Save/Load persistence round-trips.
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
	// Ensures missing preset combinations are filled during normalization.
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
	// Ensures invalid lastSelected values are corrected to a valid preset key.
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
	// Ensures corrupted JSON is handled and a valid file is recreated.
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

	[Fact]
	// Ensures Save writes a file that can be read as JSON with required properties.
	public void Save_WritesReadableJson()
	{
		using var scope = new AppDataScope();
		var store = new ThemePresetStore();
		var db = new ThemePresetDb
		{
			SchemaVersion = 1,
			Presets = new Dictionary<string, ThemePreset>(),
			LastSelected = "Dark.Transparent"
		};

		store.Save(db);

		var json = File.ReadAllText(store.GetPath());
		using var doc = JsonDocument.Parse(json);
		Assert.True(doc.RootElement.TryGetProperty("schemaVersion", out _));
		Assert.True(doc.RootElement.TryGetProperty("presets", out _));
		Assert.True(doc.RootElement.TryGetProperty("lastSelected", out _));
	}

	[Fact]
	// Ensures Save does not throw and creates the target directory when needed.
	public void Save_CreatesDirectoryWhenMissing()
	{
		using var scope = new AppDataScope();
		var store = new ThemePresetStore();
		var path = store.GetPath();
		var directory = Path.GetDirectoryName(path);
		if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
			Directory.Delete(directory, recursive: true);

		store.Save(new ThemePresetDb());

		Assert.True(File.Exists(path));
	}

	[Fact]
	// Ensures SetPreset overwrites existing entries.
	public void SetPreset_OverridesExistingPreset()
	{
		using var scope = new AppDataScope();
		var store = new ThemePresetStore();
		var db = new ThemePresetDb { Presets = new Dictionary<string, ThemePreset>() };
		var original = new ThemePreset { Theme = ThemeVariant.Dark, Effect = ThemeEffectMode.Mica };
		var updated = new ThemePreset { Theme = ThemeVariant.Dark, Effect = ThemeEffectMode.Mica, BlurRadius = 42 };

		store.SetPreset(db, ThemeVariant.Dark, ThemeEffectMode.Mica, original);
		store.SetPreset(db, ThemeVariant.Dark, ThemeEffectMode.Mica, updated);

		Assert.Same(updated, db.Presets["Dark.Mica"]);
	}

	[Fact]
	// Ensures GetPreset returns existing presets without replacing them.
	public void GetPreset_ReturnsExistingPresetInstance()
	{
		using var scope = new AppDataScope();
		var store = new ThemePresetStore();
		var preset = new ThemePreset { Theme = ThemeVariant.Light, Effect = ThemeEffectMode.Transparent, BlurRadius = 1 };
		var db = new ThemePresetDb
		{
			Presets = new Dictionary<string, ThemePreset> { ["Light.Transparent"] = preset }
		};

		var loaded = store.GetPreset(db, ThemeVariant.Light, ThemeEffectMode.Transparent);

		Assert.Same(preset, loaded);
		Assert.Equal(1, db.Presets["Light.Transparent"].BlurRadius);
	}

	[Fact]
	// Ensures normalization preserves a valid lastSelected key.
	public void Load_ValidLastSelected_IsPreserved()
	{
		using var scope = new AppDataScope();
		var store = new ThemePresetStore();
		var path = store.GetPath();
		var db = new ThemePresetDb
		{
			SchemaVersion = 1,
			Presets = new Dictionary<string, ThemePreset>
			{
				["Light.Acrylic"] = new ThemePreset { Theme = ThemeVariant.Light, Effect = ThemeEffectMode.Acrylic }
			},
			LastSelected = "Light.Acrylic"
		};

		WritePresetFile(path, db);

		var loaded = store.Load();

		Assert.Equal("Light.Acrylic", loaded.LastSelected);
	}

	[Fact]
	// Ensures normalization fills presets without removing existing custom ones.
	public void Load_PreservesCustomPresetValues()
	{
		using var scope = new AppDataScope();
		var store = new ThemePresetStore();
		var path = store.GetPath();
		var custom = new ThemePreset
		{
			Theme = ThemeVariant.Light,
			Effect = ThemeEffectMode.Transparent,
			MaterialIntensity = 11,
			BlurRadius = 22,
			PanelContrast = 33,
			MenuChildIntensity = 44,
			BorderStrength = 55
		};
		var db = new ThemePresetDb
		{
			SchemaVersion = 1,
			Presets = new Dictionary<string, ThemePreset> { ["Light.Transparent"] = custom },
			LastSelected = "Light.Transparent"
		};

		WritePresetFile(path, db);

		var loaded = store.Load();

		Assert.Equal(11, loaded.Presets["Light.Transparent"].MaterialIntensity);
		Assert.Equal(22, loaded.Presets["Light.Transparent"].BlurRadius);
		Assert.Equal(33, loaded.Presets["Light.Transparent"].PanelContrast);
		Assert.Equal(44, loaded.Presets["Light.Transparent"].MenuChildIntensity);
		Assert.Equal(55, loaded.Presets["Light.Transparent"].BorderStrength);
	}

	[Fact]
	// Ensures Load returns defaults when JSON file is empty.
	public void Load_EmptyFile_ReturnsDefaults()
	{
		using var scope = new AppDataScope();
		var store = new ThemePresetStore();
		var path = store.GetPath();
		var directory = Path.GetDirectoryName(path);
		if (!string.IsNullOrWhiteSpace(directory))
			Directory.CreateDirectory(directory);
		File.WriteAllText(path, string.Empty);

		var db = store.Load();

		Assert.NotEmpty(db.Presets);
		Assert.True(db.SchemaVersion > 0);
	}

	[Fact]
	// Ensures TryParseKey succeeds with exact enum names.
	public void TryParseKey_ParsesExactCase()
	{
		var store = new ThemePresetStore();

		Assert.True(store.TryParseKey("Dark.Transparent", out var theme, out var effect));
		Assert.Equal(ThemeVariant.Dark, theme);
		Assert.Equal(ThemeEffectMode.Transparent, effect);
	}

	[Fact]
	// Ensures TryParseKey is case-insensitive for theme and effect.
	public void TryParseKey_ParsesCaseInsensitive()
	{
		var store = new ThemePresetStore();

		Assert.True(store.TryParseKey("lIgHt.aCrYlIc", out var theme, out var effect));
		Assert.Equal(ThemeVariant.Light, theme);
		Assert.Equal(ThemeEffectMode.Acrylic, effect);
	}

	[Fact]
	// Ensures TryParseKey trims whitespace around the dot-separated parts.
	public void TryParseKey_TrimsWhitespace()
	{
		var store = new ThemePresetStore();

		Assert.True(store.TryParseKey("  Dark . Mica ", out var theme, out var effect));
		Assert.Equal(ThemeVariant.Dark, theme);
		Assert.Equal(ThemeEffectMode.Mica, effect);
	}

	[Fact]
	// Ensures TryParseKey rejects null or empty strings.
	public void TryParseKey_RejectsNullOrEmpty()
	{
		var store = new ThemePresetStore();

		Assert.False(store.TryParseKey(null, out _, out _));
		Assert.False(store.TryParseKey(string.Empty, out _, out _));
		Assert.False(store.TryParseKey("   ", out _, out _));
	}

	[Fact]
	// Ensures TryParseKey rejects keys with the wrong format.
	public void TryParseKey_RejectsInvalidFormat()
	{
		var store = new ThemePresetStore();

		Assert.False(store.TryParseKey("Dark", out _, out _));
		Assert.False(store.TryParseKey("Dark.Transparent.Extra", out _, out _));
		Assert.False(store.TryParseKey("Dark-Transparent", out _, out _));
	}

	[Theory]
	// Ensures TryParseKey rejects unknown theme names.
	[InlineData("Blue.Transparent")]
	[InlineData("Unknown.Transparent")]
	[InlineData("Darkness.Transparent")]
	[InlineData("Lightish.Transparent")]
	[InlineData("Transparent.Transparent")]
	[InlineData(".Transparent")]
	[InlineData(" .Transparent")]
	[InlineData("Dark..")]
	[InlineData("..")]
	[InlineData(".")]
	[InlineData("Transparent.")]
	public void TryParseKey_RejectsInvalidTheme(string key)
	{
		var store = new ThemePresetStore();

		Assert.False(store.TryParseKey(key, out _, out _));
	}

	[Theory]
	// Ensures TryParseKey rejects unknown effect names.
	[InlineData("Dark.Glow")]
	[InlineData("Light.Blur")]
	[InlineData("Light.Transparentish")]
	[InlineData("Dark.Micaa")]
	[InlineData("Light.*")]
	[InlineData("Dark.")]
	[InlineData("Dark. ")]
	[InlineData("Dark..")]
	public void TryParseKey_RejectsInvalidEffect(string key)
	{
		var store = new ThemePresetStore();

		Assert.False(store.TryParseKey(key, out _, out _));
	}

	[Theory]
	// Ensures TryParseKey parses supported themes and effects.
	[InlineData("Light.Transparent", ThemeVariant.Light, ThemeEffectMode.Transparent)]
	[InlineData("Light.Mica", ThemeVariant.Light, ThemeEffectMode.Mica)]
	[InlineData("Light.Acrylic", ThemeVariant.Light, ThemeEffectMode.Acrylic)]
	[InlineData("Dark.Transparent", ThemeVariant.Dark, ThemeEffectMode.Transparent)]
	[InlineData("Dark.Mica", ThemeVariant.Dark, ThemeEffectMode.Mica)]
	[InlineData("Dark.Acrylic", ThemeVariant.Dark, ThemeEffectMode.Acrylic)]
	[InlineData("light.transparent", ThemeVariant.Light, ThemeEffectMode.Transparent)]
	[InlineData("dark.mica", ThemeVariant.Dark, ThemeEffectMode.Mica)]
	[InlineData("LIGHT.ACRYLIC", ThemeVariant.Light, ThemeEffectMode.Acrylic)]
	[InlineData("DaRk.TrAnSpArEnT", ThemeVariant.Dark, ThemeEffectMode.Transparent)]
	public void TryParseKey_ParsesAllValidCombinations(string key, ThemeVariant theme, ThemeEffectMode effect)
	{
		var store = new ThemePresetStore();

		Assert.True(store.TryParseKey(key, out var parsedTheme, out var parsedEffect));
		Assert.Equal(theme, parsedTheme);
		Assert.Equal(effect, parsedEffect);
	}

	[Fact]
	// Ensures Save persists schema version updates from Normalize on Load.
	public void Load_UpdatesSchemaVersionToCurrent()
	{
		using var scope = new AppDataScope();
		var store = new ThemePresetStore();
		var path = store.GetPath();
		var db = new ThemePresetDb
		{
			SchemaVersion = -1,
			Presets = new Dictionary<string, ThemePreset>(),
			LastSelected = string.Empty
		};

		WritePresetFile(path, db);

		var loaded = store.Load();

		Assert.True(loaded.SchemaVersion > 0);
	}

	[Fact]
	// Ensures GetPath includes the expected filename and folder.
	public void GetPath_IncludesExpectedSegments()
	{
		var store = new ThemePresetStore();
		var path = store.GetPath();

		Assert.EndsWith(Path.Combine("DevProjex", "theme-presets.json"), path);
	}

	[Theory]
	// Ensures Save/Load preserves lastSelected for multiple valid keys.
	[InlineData("Light.Transparent")]
	[InlineData("Light.Mica")]
	[InlineData("Light.Acrylic")]
	[InlineData("Dark.Transparent")]
	[InlineData("Dark.Mica")]
	[InlineData("Dark.Acrylic")]
	public void Load_PreservesValidLastSelectedKeys(string lastSelected)
	{
		using var scope = new AppDataScope();
		var store = new ThemePresetStore();
		var path = store.GetPath();
		var db = new ThemePresetDb
		{
			SchemaVersion = 1,
			Presets = new Dictionary<string, ThemePreset>
			{
				[lastSelected] = new ThemePreset
				{
					Theme = lastSelected.StartsWith("Light", StringComparison.OrdinalIgnoreCase)
						? ThemeVariant.Light
						: ThemeVariant.Dark,
					Effect = lastSelected.EndsWith("Mica", StringComparison.OrdinalIgnoreCase)
						? ThemeEffectMode.Mica
						: lastSelected.EndsWith("Acrylic", StringComparison.OrdinalIgnoreCase)
							? ThemeEffectMode.Acrylic
							: ThemeEffectMode.Transparent
				}
			},
			LastSelected = lastSelected
		};

		WritePresetFile(path, db);

		var loaded = store.Load();

		Assert.Equal(lastSelected, loaded.LastSelected);
	}

	[Fact]
	// Ensures Load initializes presets dictionary even when null in JSON.
	public void Load_NullPresets_InitializesDictionary()
	{
		using var scope = new AppDataScope();
		var store = new ThemePresetStore();
		var path = store.GetPath();
		var json = """
		{
		  "schemaVersion": 1,
		  "presets": null,
		  "lastSelected": "Dark.Transparent"
		}
		""";
		var directory = Path.GetDirectoryName(path);
		if (!string.IsNullOrWhiteSpace(directory))
			Directory.CreateDirectory(directory);
		File.WriteAllText(path, json);

		var loaded = store.Load();

		Assert.NotNull(loaded.Presets);
		Assert.NotEmpty(loaded.Presets);
	}

	[Fact]
	// Ensures Load corrects missing lastSelected values to a valid key.
	public void Load_EmptyLastSelected_IsFixed()
	{
		using var scope = new AppDataScope();
		var store = new ThemePresetStore();
		var path = store.GetPath();
		var db = new ThemePresetDb
		{
			SchemaVersion = 1,
			Presets = new Dictionary<string, ThemePreset>(),
			LastSelected = string.Empty
		};

		WritePresetFile(path, db);

		var loaded = store.Load();

		Assert.False(string.IsNullOrWhiteSpace(loaded.LastSelected));
		Assert.True(loaded.Presets.ContainsKey(loaded.LastSelected));
	}

	[Fact]
	// Ensures Save/Load keeps preset dictionary count stable.
	public void Load_KeepsPresetCountStable()
	{
		using var scope = new AppDataScope();
		var store = new ThemePresetStore();
		var db = store.Load();
		var initialCount = db.Presets.Count;

		store.Save(db);
		var loaded = store.Load();

		Assert.Equal(initialCount, loaded.Presets.Count);
	}

	[Fact]
	// Ensures Save accepts an empty database and still writes defaults on load.
	public void Save_EmptyDb_LoadsWithDefaults()
	{
		using var scope = new AppDataScope();
		var store = new ThemePresetStore();
		var db = new ThemePresetDb { Presets = new Dictionary<string, ThemePreset>(), LastSelected = string.Empty };

		store.Save(db);

		var loaded = store.Load();
		Assert.NotEmpty(loaded.Presets);
		Assert.False(string.IsNullOrWhiteSpace(loaded.LastSelected));
	}

	[Theory]
	// Ensures invalid JSON payloads trigger fallback and recovery.
	[InlineData("{")]
	[InlineData("{\"schemaVersion\":}")]
	[InlineData("{\"presets\":")]
	[InlineData("{\"lastSelected\":\"Dark.Transparent\"")]
	[InlineData("{\"schemaVersion\":\"not-a-number\"}")]
	[InlineData("not json at all")]
	[InlineData("[1,2,3]")]
	[InlineData("\"just a string\"")]
	[InlineData("null")]
	[InlineData("true")]
	public void Load_InvalidJsonPayloads_Recovers(string payload)
	{
		using var scope = new AppDataScope();
		var store = new ThemePresetStore();
		var path = store.GetPath();
		var directory = Path.GetDirectoryName(path);
		if (!string.IsNullOrWhiteSpace(directory))
			Directory.CreateDirectory(directory);
		File.WriteAllText(path, payload);

		var loaded = store.Load();

		Assert.NotEmpty(loaded.Presets);
		Assert.True(File.Exists(path));
	}

	[Theory]
	// Ensures GetPreset always returns presets with matching theme/effect metadata.
	[InlineData(ThemeVariant.Light, ThemeEffectMode.Transparent)]
	[InlineData(ThemeVariant.Light, ThemeEffectMode.Mica)]
	[InlineData(ThemeVariant.Light, ThemeEffectMode.Acrylic)]
	[InlineData(ThemeVariant.Dark, ThemeEffectMode.Transparent)]
	[InlineData(ThemeVariant.Dark, ThemeEffectMode.Mica)]
	[InlineData(ThemeVariant.Dark, ThemeEffectMode.Acrylic)]
	public void GetPreset_ReturnsMatchingMetadata(ThemeVariant theme, ThemeEffectMode effect)
	{
		using var scope = new AppDataScope();
		var store = new ThemePresetStore();
		var db = new ThemePresetDb { Presets = new Dictionary<string, ThemePreset>() };

		var preset = store.GetPreset(db, theme, effect);

		Assert.Equal(theme, preset.Theme);
		Assert.Equal(effect, preset.Effect);
	}

	[Theory]
	// Ensures SetPreset stores presets under the expected key.
	[InlineData(ThemeVariant.Light, ThemeEffectMode.Transparent)]
	[InlineData(ThemeVariant.Light, ThemeEffectMode.Mica)]
	[InlineData(ThemeVariant.Light, ThemeEffectMode.Acrylic)]
	[InlineData(ThemeVariant.Dark, ThemeEffectMode.Transparent)]
	[InlineData(ThemeVariant.Dark, ThemeEffectMode.Mica)]
	[InlineData(ThemeVariant.Dark, ThemeEffectMode.Acrylic)]
	public void SetPreset_UsesExpectedKey(ThemeVariant theme, ThemeEffectMode effect)
	{
		using var scope = new AppDataScope();
		var store = new ThemePresetStore();
		var db = new ThemePresetDb { Presets = new Dictionary<string, ThemePreset>() };
		var preset = new ThemePreset { Theme = theme, Effect = effect, BlurRadius = 99 };

		store.SetPreset(db, theme, effect, preset);

		Assert.Same(preset, db.Presets[$"{theme}.{effect}"]);
	}

	[Theory]
	// Ensures Save does not throw for various lastSelected values.
	[InlineData("")]
	[InlineData(" ")]
	[InlineData("Dark.Transparent")]
	[InlineData("Light.Acrylic")]
	[InlineData("Invalid.Key")]
	public void Save_AllowsAnyLastSelectedValue(string lastSelected)
	{
		using var scope = new AppDataScope();
		var store = new ThemePresetStore();
		var db = new ThemePresetDb
		{
			SchemaVersion = 1,
			Presets = new Dictionary<string, ThemePreset>(),
			LastSelected = lastSelected
		};

		store.Save(db);

		Assert.True(File.Exists(store.GetPath()));
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
