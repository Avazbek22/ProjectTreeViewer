using System;
using System.Collections.Generic;

namespace ProjectTreeViewer;

public static class LocalizationCatalog
{
	public static IReadOnlyDictionary<string, string> Get(AppLanguage lang)
	{
		return lang switch
		{
			AppLanguage.Ru => Ru,
			AppLanguage.En => En,
			AppLanguage.Uz => Uz,
			AppLanguage.Tg => Tg,
			AppLanguage.Kk => Kk,
			AppLanguage.Fr => Fr,
			AppLanguage.De => De,
			AppLanguage.It => It,
			_ => En
		};
	}

	// Ключи (единые для всех языков):
	// Menu.File, Menu.File.Open, Menu.File.Refresh, Menu.File.Exit
	// Menu.Copy, Menu.Copy.FullTree, Menu.Copy.SelectedTree, Menu.Copy.SelectedContent
	// Menu.View, Menu.View.ExpandAll, Menu.View.CollapseAll, Menu.View.ZoomIn, Menu.View.ZoomOut, Menu.View.ZoomReset
	// Menu.Options, Menu.Options.TreeSettings
	// Menu.Language
	// Menu.Help, Menu.Help.About
	// Title.Default, Title.WithPath
	// Dialog.SelectRoot
	// Settings.IgnoreBin, Settings.IgnoreObj, Settings.IgnoreDot, Settings.All
	// Settings.Extensions, Settings.RootFolders, Settings.Font, Settings.Apply
	// Msg.ErrorTitle, Msg.InfoTitle, Msg.PathNotFound, Msg.NoCheckedTree, Msg.NoCheckedFiles, Msg.ElevationCanceled, Msg.AccessDeniedRoot, Msg.AboutStub

	private static readonly IReadOnlyDictionary<string, string> Ru = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		["Menu.File"] = "Файл",
		["Menu.File.Open"] = "Открыть папку...",
		["Menu.File.Refresh"] = "Обновить",
		["Menu.File.Exit"] = "Выход",

		["Menu.Copy"] = "Копировать",
		["Menu.Copy.FullTree"] = "Скопировать полное дерево",
		["Menu.Copy.SelectedTree"] = "Скопировать дерево выбранных файлов",
		["Menu.Copy.SelectedContent"] = "Скопировать содержимое выбранных файлов",

		["Menu.View"] = "Вид",
		["Menu.View.ExpandAll"] = "Развернуть всё",
		["Menu.View.CollapseAll"] = "Свернуть всё",
		["Menu.View.ZoomIn"] = "Увеличить",
		["Menu.View.ZoomOut"] = "Уменьшить",
		["Menu.View.ZoomReset"] = "Сбросить масштаб",

		["Menu.Options"] = "Параметры",
		["Menu.Options.TreeSettings"] = "Параметры дерева",

		["Menu.Language"] = "Язык",

		["Menu.Help"] = "Справка",
		["Menu.Help.About"] = "О программе",

		["Title.Default"] = "Project Tree Viewer by Avazbek",
		["Title.WithPath"] = "Project Tree Viewer - {0}",

		["Dialog.SelectRoot"] = "Выберите корневую папку проекта",

		["Settings.IgnoreBin"] = "Игнорировать все папки bin",
		["Settings.IgnoreObj"] = "Игнорировать все папки obj",
		["Settings.IgnoreDot"] = "Игнорировать скрытые файлы/папки (с точкой в начале)",
		["Settings.All"] = "Все",
		["Settings.Extensions"] = "Типы файлов:",
		["Settings.RootFolders"] = "Папки верхнего уровня:",
		["Settings.Font"] = "Шрифт дерева:",
		["Settings.Apply"] = "Применить настройки",

		["Msg.ErrorTitle"] = "Ошибка",
		["Msg.InfoTitle"] = "Информация",
		["Msg.PathNotFound"] = "Папка не найдена:\n{0}",
		["Msg.NoCheckedTree"] = "Не выбраны файлы или папки (галочки в дереве).",
		["Msg.NoCheckedFiles"] = "Не выбраны файлы (галочки должны стоять на файлах).",
		["Msg.ElevationCanceled"] = "Повышение прав отменено. Папка может открыться не полностью.",
		["Msg.AccessDeniedRoot"] = "Нет доступа к выбранной папке. Попробуйте запустить приложение от имени администратора.",
		["Msg.AboutStub"] = "Здесь будет информация о программе (заглушка)."
	};

	private static readonly IReadOnlyDictionary<string, string> En = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		["Menu.File"] = "File",
		["Menu.File.Open"] = "Open folder...",
		["Menu.File.Refresh"] = "Refresh",
		["Menu.File.Exit"] = "Exit",

		["Menu.Copy"] = "Copy",
		["Menu.Copy.FullTree"] = "Copy full tree",
		["Menu.Copy.SelectedTree"] = "Copy selected tree",
		["Menu.Copy.SelectedContent"] = "Copy content of selected files",

		["Menu.View"] = "View",
		["Menu.View.ExpandAll"] = "Expand all",
		["Menu.View.CollapseAll"] = "Collapse all",
		["Menu.View.ZoomIn"] = "Zoom in",
		["Menu.View.ZoomOut"] = "Zoom out",
		["Menu.View.ZoomReset"] = "Reset zoom",

		["Menu.Options"] = "Options",
		["Menu.Options.TreeSettings"] = "Tree settings",

		["Menu.Language"] = "Language",

		["Menu.Help"] = "Help",
		["Menu.Help.About"] = "About",

		["Title.Default"] = "Project Tree Viewer by Avazbek",
		["Title.WithPath"] = "Project Tree Viewer - {0}",

		["Dialog.SelectRoot"] = "Select the project root folder",

		["Settings.IgnoreBin"] = "Ignore all bin folders",
		["Settings.IgnoreObj"] = "Ignore all obj folders",
		["Settings.IgnoreDot"] = "Ignore dot files/folders",
		["Settings.All"] = "All",
		["Settings.Extensions"] = "File types:",
		["Settings.RootFolders"] = "Top-level folders:",
		["Settings.Font"] = "Tree font:",
		["Settings.Apply"] = "Apply settings",

		["Msg.ErrorTitle"] = "Error",
		["Msg.InfoTitle"] = "Info",
		["Msg.PathNotFound"] = "Folder not found:\n{0}",
		["Msg.NoCheckedTree"] = "No checked files or folders (use checkboxes in the tree).",
		["Msg.NoCheckedFiles"] = "No checked files (checkboxes must be on files).",
		["Msg.ElevationCanceled"] = "Elevation was canceled. The folder may not be fully accessible.",
		["Msg.AccessDeniedRoot"] = "Access denied for the selected folder. Try running as administrator.",
		["Msg.AboutStub"] = "Program information will be here (stub)."
	};

	private static readonly IReadOnlyDictionary<string, string> Uz = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		["Menu.File"] = "Fayl",
		["Menu.File.Open"] = "Jildni ochish...",
		["Menu.File.Refresh"] = "Yangilash",
		["Menu.File.Exit"] = "Chiqish",

		["Menu.Copy"] = "Nusxa",
		["Menu.Copy.FullTree"] = "To‘liq daraxtni nusxalash",
		["Menu.Copy.SelectedTree"] = "Tanlangan daraxtni nusxalash",
		["Menu.Copy.SelectedContent"] = "Tanlangan fayllar matnini nusxalash",

		["Menu.View"] = "Ko‘rinish",
		["Menu.View.ExpandAll"] = "Hammasini ochish",
		["Menu.View.CollapseAll"] = "Hammasini yopish",
		["Menu.View.ZoomIn"] = "Kattalashtirish",
		["Menu.View.ZoomOut"] = "Kichiklashtirish",
		["Menu.View.ZoomReset"] = "Masshtabni tiklash",

		["Menu.Options"] = "Sozlamalar",
		["Menu.Options.TreeSettings"] = "Daraxt sozlamalari",

		["Menu.Language"] = "Til",

		["Menu.Help"] = "Yordam",
		["Menu.Help.About"] = "Dastur haqida",

		["Title.Default"] = "Project Tree Viewer by Avazbek",
		["Title.WithPath"] = "Project Tree Viewer - {0}",

		["Dialog.SelectRoot"] = "Loyiha ildiz jildini tanlang",

		["Settings.IgnoreBin"] = "bin jildlarini e’tiborsiz qoldirish",
		["Settings.IgnoreObj"] = "obj jildlarini e’tiborsiz qoldirish",
		["Settings.IgnoreDot"] = "Nuqta bilan boshlanuvchi yashirin fayl/jildlarni yashirish",
		["Settings.All"] = "Barchasi",
		["Settings.Extensions"] = "Fayl turlari:",
		["Settings.RootFolders"] = "Yuqori darajadagi jildlar:",
		["Settings.Font"] = "Daraxt shrift:",
		["Settings.Apply"] = "Sozlamalarni qo‘llash",

		["Msg.ErrorTitle"] = "Xatolik",
		["Msg.InfoTitle"] = "Ma’lumot",
		["Msg.PathNotFound"] = "Jild topilmadi:\n{0}",
		["Msg.NoCheckedTree"] = "Tanlangan fayl yoki jild yo‘q (daraxtdagi belgilardan foydalaning).",
		["Msg.NoCheckedFiles"] = "Tanlangan fayllar yo‘q (belgilar fayllarda bo‘lishi kerak).",
		["Msg.ElevationCanceled"] = "Administrator huquqlari bekor qilindi.",
		["Msg.AccessDeniedRoot"] = "Tanlangan jildga ruxsat yo‘q. Administrator sifatida ishga tushiring.",
		["Msg.AboutStub"] = "Bu yerda dastur haqida ma’lumot bo‘ladi (stub)."
	};

	private static readonly IReadOnlyDictionary<string, string> Tg = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		["Menu.File"] = "Файл",
		["Menu.File.Open"] = "Кушодани ҷузвдон...",
		["Menu.File.Refresh"] = "Навсозӣ",
		["Menu.File.Exit"] = "Баромад",

		["Menu.Copy"] = "Нусха",
		["Menu.Copy.FullTree"] = "Нусхаи дарахти пурра",
		["Menu.Copy.SelectedTree"] = "Нусхаи дарахти интихобшуда",
		["Menu.Copy.SelectedContent"] = "Нусхаи мундариҷаи файлҳои интихобшуда",

		["Menu.View"] = "Намоиш",
		["Menu.View.ExpandAll"] = "Кушодани ҳама",
		["Menu.View.CollapseAll"] = "Пӯшидани ҳама",
		["Menu.View.ZoomIn"] = "Калон кардан",
		["Menu.View.ZoomOut"] = "Хурд кардан",
		["Menu.View.ZoomReset"] = "Барқарор кардани миқёс",

		["Menu.Options"] = "Танзимот",
		["Menu.Options.TreeSettings"] = "Танзимоти дарахт",

		["Menu.Language"] = "Забон",

		["Menu.Help"] = "Ёрӣ",
		["Menu.Help.About"] = "Дар бораи барнома",

		["Title.Default"] = "Project Tree Viewer by Avazbek",
		["Title.WithPath"] = "Project Tree Viewer - {0}",

		["Dialog.SelectRoot"] = "Ҷузвдони решаи лоиҳаро интихоб кунед",

		["Settings.IgnoreBin"] = "Нодида гирифтани ҷузвдонҳои bin",
		["Settings.IgnoreObj"] = "Нодида гирифтани ҷузвдонҳои obj",
		["Settings.IgnoreDot"] = "Нодида гирифтани файл/ҷузвдонҳои пинҳонӣ (бо нуқта оғоз мешаванд)",
		["Settings.All"] = "Ҳама",
		["Settings.Extensions"] = "Навъҳои файл:",
		["Settings.RootFolders"] = "Ҷузвдонҳои сатҳи боло:",
		["Settings.Font"] = "Шрифти дарахт:",
		["Settings.Apply"] = "Истифодаи танзимот",

		["Msg.ErrorTitle"] = "Хато",
		["Msg.InfoTitle"] = "Маълумот",
		["Msg.PathNotFound"] = "Ҷузвдон ёфт нашуд:\n{0}",
		["Msg.NoCheckedTree"] = "Файл ё ҷузвдони интихобшуда нест (қуттиҳои интихобро истифода баред).",
		["Msg.NoCheckedFiles"] = "Файлҳои интихобшуда нестанд (қуттиҳо бояд дар файлҳо бошанд).",
		["Msg.ElevationCanceled"] = "Баланд бардоштани ҳуқуқҳо бекор шуд.",
		["Msg.AccessDeniedRoot"] = "Ба ҷузвдони интихобшуда дастрасӣ нест. Бо ҳуқуқи админ оғоз кунед.",
		["Msg.AboutStub"] = "Дар ин ҷо маълумоти барнома хоҳад буд (stub)."
	};

	private static readonly IReadOnlyDictionary<string, string> Kk = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		["Menu.File"] = "Файл",
		["Menu.File.Open"] = "Қалтаны ашу...",
		["Menu.File.Refresh"] = "Жаңарту",
		["Menu.File.Exit"] = "Шығу",

		["Menu.Copy"] = "Көшіру",
		["Menu.Copy.FullTree"] = "Толық ағашты көшіру",
		["Menu.Copy.SelectedTree"] = "Таңдалған ағашты көшіру",
		["Menu.Copy.SelectedContent"] = "Таңдалған файлдардың мәтінін көшіру",

		["Menu.View"] = "Көрініс",
		["Menu.View.ExpandAll"] = "Барлығын ашу",
		["Menu.View.CollapseAll"] = "Барлығын жабу",
		["Menu.View.ZoomIn"] = "Үлкейту",
		["Menu.View.ZoomOut"] = "Кішірейту",
		["Menu.View.ZoomReset"] = "Масштабты қалпына келтіру",

		["Menu.Options"] = "Параметрлер",
		["Menu.Options.TreeSettings"] = "Ағаш параметрлері",

		["Menu.Language"] = "Тіл",

		["Menu.Help"] = "Анықтама",
		["Menu.Help.About"] = "Бағдарлама туралы",

		["Title.Default"] = "Project Tree Viewer by Avazbek",
		["Title.WithPath"] = "Project Tree Viewer - {0}",

		["Dialog.SelectRoot"] = "Жоба түбір қалтасын таңдаңыз",

		["Settings.IgnoreBin"] = "bin қалталарын елемеу",
		["Settings.IgnoreObj"] = "obj қалталарын елемеу",
		["Settings.IgnoreDot"] = "Нүктемен басталатын жасырын файл/қалталарды елемеу",
		["Settings.All"] = "Барлығы",
		["Settings.Extensions"] = "Файл түрлері:",
		["Settings.RootFolders"] = "Жоғарғы деңгей қалталары:",
		["Settings.Font"] = "Ағаш қарпі:",
		["Settings.Apply"] = "Қолдану",

		["Msg.ErrorTitle"] = "Қате",
		["Msg.InfoTitle"] = "Ақпарат",
		["Msg.PathNotFound"] = "Қалта табылмады:\n{0}",
		["Msg.NoCheckedTree"] = "Таңдалған файл/қалта жоқ (ағаштағы белгілерді қолданыңыз).",
		["Msg.NoCheckedFiles"] = "Таңдалған файл жоқ (белгілер файлда болуы керек).",
		["Msg.ElevationCanceled"] = "Әкімші құқықтары күшін жойды.",
		["Msg.AccessDeniedRoot"] = "Таңдалған қалтаға рұқсат жоқ. Әкімші ретінде іске қосыңыз.",
		["Msg.AboutStub"] = "Мұнда бағдарлама туралы ақпарат болады (stub)."
	};

	private static readonly IReadOnlyDictionary<string, string> Fr = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		["Menu.File"] = "Fichier",
		["Menu.File.Open"] = "Ouvrir le dossier...",
		["Menu.File.Refresh"] = "Actualiser",
		["Menu.File.Exit"] = "Quitter",

		["Menu.Copy"] = "Copier",
		["Menu.Copy.FullTree"] = "Copier l’arborescence complète",
		["Menu.Copy.SelectedTree"] = "Copier l’arborescence sélectionnée",
		["Menu.Copy.SelectedContent"] = "Copier le contenu des fichiers sélectionnés",

		["Menu.View"] = "Affichage",
		["Menu.View.ExpandAll"] = "Tout développer",
		["Menu.View.CollapseAll"] = "Tout réduire",
		["Menu.View.ZoomIn"] = "Agrandir",
		["Menu.View.ZoomOut"] = "Rétrécir",
		["Menu.View.ZoomReset"] = "Réinitialiser le zoom",

		["Menu.Options"] = "Options",
		["Menu.Options.TreeSettings"] = "Paramètres de l’arborescence",

		["Menu.Language"] = "Langue",

		["Menu.Help"] = "Aide",
		["Menu.Help.About"] = "À propos",

		["Title.Default"] = "Project Tree Viewer by Avazbek",
		["Title.WithPath"] = "Project Tree Viewer - {0}",

		["Dialog.SelectRoot"] = "Sélectionnez le dossier racine du projet",

		["Settings.IgnoreBin"] = "Ignorer les dossiers bin",
		["Settings.IgnoreObj"] = "Ignorer les dossiers obj",
		["Settings.IgnoreDot"] = "Ignorer les fichiers/dossiers commençant par un point",
		["Settings.All"] = "Tous",
		["Settings.Extensions"] = "Types de fichiers :",
		["Settings.RootFolders"] = "Dossiers de premier niveau :",
		["Settings.Font"] = "Police de l’arborescence :",
		["Settings.Apply"] = "Appliquer",

		["Msg.ErrorTitle"] = "Erreur",
		["Msg.InfoTitle"] = "Info",
		["Msg.PathNotFound"] = "Dossier introuvable :\n{0}",
		["Msg.NoCheckedTree"] = "Aucun fichier/dossier coché (utilisez les cases dans l’arborescence).",
		["Msg.NoCheckedFiles"] = "Aucun fichier coché (les cases doivent être sur des fichiers).",
		["Msg.ElevationCanceled"] = "L’élévation a été annulée.",
		["Msg.AccessDeniedRoot"] = "Accès refusé au dossier sélectionné. Essayez en administrateur.",
		["Msg.AboutStub"] = "Les informations sur le programme seront ici (stub)."
	};

	private static readonly IReadOnlyDictionary<string, string> De = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		["Menu.File"] = "Datei",
		["Menu.File.Open"] = "Ordner öffnen...",
		["Menu.File.Refresh"] = "Aktualisieren",
		["Menu.File.Exit"] = "Beenden",

		["Menu.Copy"] = "Kopieren",
		["Menu.Copy.FullTree"] = "Kompletten Baum kopieren",
		["Menu.Copy.SelectedTree"] = "Ausgewählten Baum kopieren",
		["Menu.Copy.SelectedContent"] = "Inhalt ausgewählter Dateien kopieren",

		["Menu.View"] = "Ansicht",
		["Menu.View.ExpandAll"] = "Alles erweitern",
		["Menu.View.CollapseAll"] = "Alles reduzieren",
		["Menu.View.ZoomIn"] = "Vergrößern",
		["Menu.View.ZoomOut"] = "Verkleinern",
		["Menu.View.ZoomReset"] = "Zoom zurücksetzen",

		["Menu.Options"] = "Optionen",
		["Menu.Options.TreeSettings"] = "Baum-Einstellungen",

		["Menu.Language"] = "Sprache",

		["Menu.Help"] = "Hilfe",
		["Menu.Help.About"] = "Über",

		["Title.Default"] = "Project Tree Viewer by Avazbek",
		["Title.WithPath"] = "Project Tree Viewer - {0}",

		["Dialog.SelectRoot"] = "Projekt-Stammordner auswählen",

		["Settings.IgnoreBin"] = "bin-Ordner ignorieren",
		["Settings.IgnoreObj"] = "obj-Ordner ignorieren",
		["Settings.IgnoreDot"] = "Dot-Dateien/Ordner ignorieren",
		["Settings.All"] = "Alle",
		["Settings.Extensions"] = "Dateitypen:",
		["Settings.RootFolders"] = "Ordner der obersten Ebene:",
		["Settings.Font"] = "Baum-Schrift:",
		["Settings.Apply"] = "Anwenden",

		["Msg.ErrorTitle"] = "Fehler",
		["Msg.InfoTitle"] = "Info",
		["Msg.PathNotFound"] = "Ordner nicht gefunden:\n{0}",
		["Msg.NoCheckedTree"] = "Keine markierten Dateien/Ordner (Checkboxen im Baum verwenden).",
		["Msg.NoCheckedFiles"] = "Keine markierten Dateien (Checkboxen müssen auf Dateien sein).",
		["Msg.ElevationCanceled"] = "Erhöhung wurde abgebrochen.",
		["Msg.AccessDeniedRoot"] = "Zugriff verweigert. Als Administrator ausführen.",
		["Msg.AboutStub"] = "Programminfo kommt hier hin (Stub)."
	};

	private static readonly IReadOnlyDictionary<string, string> It = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		["Menu.File"] = "File",
		["Menu.File.Open"] = "Apri cartella...",
		["Menu.File.Refresh"] = "Aggiorna",
		["Menu.File.Exit"] = "Esci",

		["Menu.Copy"] = "Copia",
		["Menu.Copy.FullTree"] = "Copia albero completo",
		["Menu.Copy.SelectedTree"] = "Copia albero selezionato",
		["Menu.Copy.SelectedContent"] = "Copia contenuto dei file selezionati",

		["Menu.View"] = "Visualizza",
		["Menu.View.ExpandAll"] = "Espandi tutto",
		["Menu.View.CollapseAll"] = "Comprimi tutto",
		["Menu.View.ZoomIn"] = "Zoom avanti",
		["Menu.View.ZoomOut"] = "Zoom indietro",
		["Menu.View.ZoomReset"] = "Reimposta zoom",

		["Menu.Options"] = "Opzioni",
		["Menu.Options.TreeSettings"] = "Impostazioni albero",

		["Menu.Language"] = "Lingua",

		["Menu.Help"] = "Aiuto",
		["Menu.Help.About"] = "Informazioni",

		["Title.Default"] = "Project Tree Viewer by Avazbek",
		["Title.WithPath"] = "Project Tree Viewer - {0}",

		["Dialog.SelectRoot"] = "Seleziona la cartella radice del progetto",

		["Settings.IgnoreBin"] = "Ignora cartelle bin",
		["Settings.IgnoreObj"] = "Ignora cartelle obj",
		["Settings.IgnoreDot"] = "Ignora file/cartelle con punto iniziale",
		["Settings.All"] = "Tutti",
		["Settings.Extensions"] = "Tipi di file:",
		["Settings.RootFolders"] = "Cartelle di primo livello:",
		["Settings.Font"] = "Font albero:",
		["Settings.Apply"] = "Applica",

		["Msg.ErrorTitle"] = "Errore",
		["Msg.InfoTitle"] = "Info",
		["Msg.PathNotFound"] = "Cartella non trovata:\n{0}",
		["Msg.NoCheckedTree"] = "Nessun file/cartella selezionato (usa le checkbox nell’albero).",
		["Msg.NoCheckedFiles"] = "Nessun file selezionato (le checkbox devono essere sui file).",
		["Msg.ElevationCanceled"] = "Elevazione annullata.",
		["Msg.AccessDeniedRoot"] = "Accesso negato. Prova come amministratore.",
		["Msg.AboutStub"] = "Le info del programma saranno qui (stub)."
	};
}
