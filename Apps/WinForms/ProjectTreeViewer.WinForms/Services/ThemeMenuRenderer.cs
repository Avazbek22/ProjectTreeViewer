using System.Drawing;
using System.Windows.Forms;

namespace ProjectTreeViewer.WinForms.Services;

public sealed class ThemeMenuRenderer : ToolStripProfessionalRenderer
{
	private readonly ThemePalette _palette;

	public ThemeMenuRenderer(ThemePalette palette) : base(new ThemeMenuColorTable(palette))
	{
		_palette = palette;
	}

	protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
	{
		using var pen = new Pen(_palette.MenuBorder);
		var bounds = new Rectangle(0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
		e.Graphics.DrawRectangle(pen, bounds);
	}
}

public sealed class ThemeMenuColorTable : ProfessionalColorTable
{
	private readonly ThemePalette _palette;

	public ThemeMenuColorTable(ThemePalette palette)
	{
		_palette = palette;
		UseSystemColors = false;
	}

	public override Color ToolStripDropDownBackground => _palette.MenuBackground;
	public override Color MenuBorder => _palette.MenuBorder;
	public override Color MenuItemBorder => _palette.MenuBorder;
	public override Color MenuItemSelected => _palette.MenuSelection;
	public override Color MenuItemSelectedGradientBegin => _palette.MenuSelection;
	public override Color MenuItemSelectedGradientEnd => _palette.MenuSelection;
	public override Color MenuItemPressedGradientBegin => _palette.MenuSelection;
	public override Color MenuItemPressedGradientEnd => _palette.MenuSelection;
	public override Color ImageMarginGradientBegin => _palette.MenuBackground;
	public override Color ImageMarginGradientMiddle => _palette.MenuBackground;
	public override Color ImageMarginGradientEnd => _palette.MenuBackground;
	public override Color SeparatorDark => _palette.MenuBorder;
	public override Color SeparatorLight => _palette.MenuBorder;
}
