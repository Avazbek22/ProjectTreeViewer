using System;
using System.Collections.Generic;
using System.Globalization;

namespace ProjectTreeViewer;

public sealed class LocalizationService
{
	private readonly Dictionary<string, (string Ru, string En)> _texts = new(StringComparer.OrdinalIgnoreCase)
	{
		["Title.Default"] = ("Project Tree Viewer by Avazbek", "Project Tree Viewer by Avazbek"),
		["Title.WithPath"] = ("Project Tree Viewer - {0}", "Project Tree Viewer - {0}"),

		["Button.Open"] = ("Открыть папку", "Open folder"),
		["Button.Copy"] = ("Скопировать всё", "Copy all"),
		["Button.Refresh"] = ("Обновить", "Refresh"),
		["Button.Settings"] = ("Настройки", "Settings"),
		["Button.Apply"] = ("Применить настройки", "Apply settings"),

		["Label.Extensions"] = ("Типы файлов:", "File types:"),
		["Label.RootFolders"] = ("Папки верхнего уровня:", "Top-level folders:"),
		["Label.Font"] = ("Шрифт дерева:", "Tree font:"),

		["Check.All"] = ("Все", "All"),
		["Check.IgnoreBin"] = ("Игнорировать все папки bin", "Ignore all bin folders"),
		["Check.IgnoreObj"] = ("Игнорировать все папки obj", "Ignore all obj folders"),
		["Check.IgnoreDot"] = ("Игнорировать скрытые файлы/папки (с точкой в начале)", "Ignore dot files/folders"),

		["Dialog.SelectRoot"] = ("Выберите корневую папку проекта", "Select the project root folder"),

		["Msg.ErrorTitle"] = ("Ошибка", "Error"),
		["Msg.PathNotFound"] = ("Папка не найдена:\n{0}", "Folder not found:\n{0}"),
		["Msg.ElevationCanceled"] = ("Повышение прав отменено. Папка может открыться не полностью.", "Elevation was canceled. The folder may not be fully accessible.")
	};

	public AppLanguage CurrentLanguage { get; private set; }

	public event EventHandler? LanguageChanged;

	public LocalizationService(AppLanguage initialLanguage)
	{
		CurrentLanguage = initialLanguage;
	}

	public static AppLanguage DetectSystemLanguage()
	{
		var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
		return lang.Equals("ru", StringComparison.OrdinalIgnoreCase) ? AppLanguage.Ru : AppLanguage.En;
	}

	public void ToggleLanguage()
	{
		CurrentLanguage = CurrentLanguage == AppLanguage.Ru ? AppLanguage.En : AppLanguage.Ru;
		LanguageChanged?.Invoke(this, EventArgs.Empty);
	}

	public string this[string key]
	{
		get
		{
			if (!_texts.TryGetValue(key, out var v))
				return $"[[{key}]]";

			return CurrentLanguage == AppLanguage.Ru ? v.Ru : v.En;
		}
	}

	public string Format(string key, params object[] args) => string.Format(this[key], args);
}
