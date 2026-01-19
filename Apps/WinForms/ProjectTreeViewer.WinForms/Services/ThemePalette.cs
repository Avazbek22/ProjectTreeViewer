using System.Drawing;

namespace ProjectTreeViewer.WinForms.Services;

public sealed class ThemePalette
{
	public static ThemePalette Light { get; } = new(
		formBackground: SystemColors.Control,
		panelBackground: SystemColors.ControlLight,
		surfaceBackground: SystemColors.Window,
		menuBackground: SystemColors.Control,
		menuSelection: SystemColors.Highlight,
		menuBorder: SystemColors.ControlDark,
		textPrimary: SystemColors.ControlText,
		textMuted: SystemColors.GrayText,
		buttonBackground: SystemColors.ControlLight,
		inputBackground: SystemColors.Window,
		border: SystemColors.ControlDark,
		treeLine: SystemColors.ControlDark
	);

	public static ThemePalette Dark { get; } = new(
		formBackground: Color.FromArgb(30, 30, 30),
		panelBackground: Color.FromArgb(37, 37, 38),
		surfaceBackground: Color.FromArgb(30, 30, 30),
		menuBackground: Color.FromArgb(45, 45, 48),
		menuSelection: Color.FromArgb(62, 62, 66),
		menuBorder: Color.FromArgb(63, 63, 70),
		textPrimary: Color.FromArgb(241, 241, 241),
		textMuted: Color.FromArgb(200, 200, 200),
		buttonBackground: Color.FromArgb(51, 51, 55),
		inputBackground: Color.FromArgb(31, 31, 31),
		border: Color.FromArgb(63, 63, 70),
		treeLine: Color.FromArgb(63, 63, 70)
	);

	public Color FormBackground { get; }
	public Color PanelBackground { get; }
	public Color SurfaceBackground { get; }
	public Color MenuBackground { get; }
	public Color MenuSelection { get; }
	public Color MenuBorder { get; }
	public Color TextPrimary { get; }
	public Color TextMuted { get; }
	public Color ButtonBackground { get; }
	public Color InputBackground { get; }
	public Color Border { get; }
	public Color TreeLine { get; }

	private ThemePalette(
		Color formBackground,
		Color panelBackground,
		Color surfaceBackground,
		Color menuBackground,
		Color menuSelection,
		Color menuBorder,
		Color textPrimary,
		Color textMuted,
		Color buttonBackground,
		Color inputBackground,
		Color border,
		Color treeLine)
	{
		FormBackground = formBackground;
		PanelBackground = panelBackground;
		SurfaceBackground = surfaceBackground;
		MenuBackground = menuBackground;
		MenuSelection = menuSelection;
		MenuBorder = menuBorder;
		TextPrimary = textPrimary;
		TextMuted = textMuted;
		ButtonBackground = buttonBackground;
		InputBackground = inputBackground;
		Border = border;
		TreeLine = treeLine;
	}
}
