// ─────────────────────────────────────────────────────────────
//  Form1.Designer.cs   (полный, рабочий)
// ─────────────────────────────────────────────────────────────
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProjectTreeViewer
{
	partial class Form1
	{
		/// <inheritdoc/>
		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null) components.Dispose();
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			menuStripMain = new MenuStrip();
			miFile = new ToolStripMenuItem();
			miFileOpen = new ToolStripMenuItem();
			miFileRefresh = new ToolStripMenuItem();
			miFileSep1 = new ToolStripSeparator();
			miFileExit = new ToolStripMenuItem();
			miCopy = new ToolStripMenuItem();
			miCopyFullTree = new ToolStripMenuItem();
			miCopySelectedTree = new ToolStripMenuItem();
			miCopySelectedContent = new ToolStripMenuItem();
			miCopyFullTreeAndContent = new ToolStripMenuItem();
			miView = new ToolStripMenuItem();
			miViewExpandAll = new ToolStripMenuItem();
			miViewCollapseAll = new ToolStripMenuItem();
			miViewSep1 = new ToolStripSeparator();
			miViewZoomIn = new ToolStripMenuItem();
			miViewZoomOut = new ToolStripMenuItem();
			miViewZoomReset = new ToolStripMenuItem();
			miOptions = new ToolStripMenuItem();
			miOptionsTreeSettings = new ToolStripMenuItem();
			miLanguage = new ToolStripMenuItem();
			miLangRu = new ToolStripMenuItem();
			miLangEn = new ToolStripMenuItem();
			miLangUz = new ToolStripMenuItem();
			miLangTg = new ToolStripMenuItem();
			miLangKk = new ToolStripMenuItem();
			miLangFr = new ToolStripMenuItem();
			miLangDe = new ToolStripMenuItem();
			miLangIt = new ToolStripMenuItem();
			miHelp = new ToolStripMenuItem();
			miHelpAbout = new ToolStripMenuItem();
			panelSettings = new Panel();
			labelFont = new Label();
			cboFont = new ComboBox();
			btnApply = new Button();
			labelIgnore = new Label();
			lstIgnore = new CheckedListBox();
			labelExtensions = new Label();
			checkBoxAll = new CheckBox();
			lstExtensions = new CheckedListBox();
			labelRootFolders = new Label();
			lstRootFolders = new CheckedListBox();
			treeProject = new TreeView();
			menuStripMain.SuspendLayout();
			panelSettings.SuspendLayout();
			SuspendLayout();
			//
			// menuStripMain
			//
			menuStripMain.Dock = DockStyle.Top;
			menuStripMain.ImageScalingSize = new Size(20, 20);
			menuStripMain.Items.AddRange(new ToolStripItem[] { miFile, miCopy, miView, miOptions, miLanguage, miHelp });
			menuStripMain.Location = new Point(0, 0);
			menuStripMain.Name = "menuStripMain";
			menuStripMain.Size = new Size(893, 28);
			menuStripMain.TabIndex = 0;
			//
			// miFile
			//
			miFile.DropDownItems.AddRange(new ToolStripItem[] { miFileOpen, miFileRefresh, miFileSep1, miFileExit });
			miFile.Name = "miFile";
			miFile.Size = new Size(57, 24);
			miFile.Text = "Файл";
			//
			// miFileOpen
			//
			miFileOpen.Name = "miFileOpen";
			miFileOpen.ShortcutKeys = Keys.Control | Keys.O;
			miFileOpen.Size = new Size(254, 26);
			miFileOpen.Text = "Открыть папку...";
			miFileOpen.Click += miFileOpen_Click;
			//
			// miFileRefresh
			//
			miFileRefresh.Enabled = false;
			miFileRefresh.Name = "miFileRefresh";
			miFileRefresh.ShortcutKeys = Keys.F5;
			miFileRefresh.Size = new Size(254, 26);
			miFileRefresh.Text = "Обновить";
			miFileRefresh.Click += miFileRefresh_Click;
			//
			// miFileSep1
			//
			miFileSep1.Name = "miFileSep1";
			miFileSep1.Size = new Size(251, 6);
			//
			// miFileExit
			//
			miFileExit.Name = "miFileExit";
			miFileExit.Size = new Size(254, 26);
			miFileExit.Text = "Выход";
			miFileExit.Click += miFileExit_Click;
			//
			// miCopy
			//
			miCopy.DropDownItems.AddRange(new ToolStripItem[] { miCopyFullTree, miCopySelectedTree, miCopySelectedContent, miCopyFullTreeAndContent });
			miCopy.Name = "miCopy";
			miCopy.Size = new Size(98, 24);
			miCopy.Text = "Копировать";
			//
			// miCopyFullTree
			//
			miCopyFullTree.Enabled = false;
			miCopyFullTree.Name = "miCopyFullTree";
			miCopyFullTree.ShortcutKeys = Keys.Control | Keys.Shift | Keys.C;
			miCopyFullTree.Size = new Size(420, 26);
			miCopyFullTree.Text = "Скопировать полное дерево";
			miCopyFullTree.Click += miCopyFullTree_Click;
			//
			// miCopySelectedTree
			//
			miCopySelectedTree.Enabled = false;
			miCopySelectedTree.Name = "miCopySelectedTree";
			miCopySelectedTree.ShortcutKeys = Keys.Control | Keys.Alt | Keys.C;
			miCopySelectedTree.Size = new Size(420, 26);
			miCopySelectedTree.Text = "Скопировать дерево выбранных файлов";
			miCopySelectedTree.Click += miCopySelectedTree_Click;
			//
			// miCopySelectedContent
			//
			miCopySelectedContent.Enabled = false;
			miCopySelectedContent.Name = "miCopySelectedContent";
			miCopySelectedContent.ShortcutKeys = Keys.Control | Keys.Alt | Keys.V;
			miCopySelectedContent.Size = new Size(420, 26);
			miCopySelectedContent.Text = "Скопировать содержимое выбранных файлов";
			miCopySelectedContent.Click += miCopySelectedContent_Click;
			//
			// miCopyFullTreeAndContent
			//
			miCopyFullTreeAndContent.Enabled = false;
			miCopyFullTreeAndContent.Name = "miCopyFullTreeAndContent";
			miCopyFullTreeAndContent.Size = new Size(420, 26);
			miCopyFullTreeAndContent.Text = "Скопировать полное дерево и содержимое";
			miCopyFullTreeAndContent.Click += miCopyFullTreeAndContent_Click;
			//
			// miView
			//
			miView.DropDownItems.AddRange(new ToolStripItem[] { miViewExpandAll, miViewCollapseAll, miViewSep1, miViewZoomIn, miViewZoomOut, miViewZoomReset });
			miView.Name = "miView";
			miView.Size = new Size(46, 24);
			miView.Text = "Вид";
			//
			// miViewExpandAll
			//
			miViewExpandAll.Enabled = false;
			miViewExpandAll.Name = "miViewExpandAll";
			miViewExpandAll.ShortcutKeys = Keys.Control | Keys.E;
			miViewExpandAll.Size = new Size(275, 26);
			miViewExpandAll.Text = "Развернуть всё";
			miViewExpandAll.Click += miViewExpandAll_Click;
			//
			// miViewCollapseAll
			//
			miViewCollapseAll.Enabled = false;
			miViewCollapseAll.Name = "miViewCollapseAll";
			miViewCollapseAll.ShortcutKeys = Keys.Control | Keys.W;
			miViewCollapseAll.Size = new Size(275, 26);
			miViewCollapseAll.Text = "Свернуть всё";
			miViewCollapseAll.Click += miViewCollapseAll_Click;
			//
			// miViewSep1
			//
			miViewSep1.Name = "miViewSep1";
			miViewSep1.Size = new Size(272, 6);
			//
			// miViewZoomIn
			//
			miViewZoomIn.Name = "miViewZoomIn";
			miViewZoomIn.ShortcutKeys = Keys.Control | Keys.Oemplus;
			miViewZoomIn.Size = new Size(275, 26);
			miViewZoomIn.Text = "Увеличить";
			miViewZoomIn.Click += miViewZoomIn_Click;
			//
			// miViewZoomOut
			//
			miViewZoomOut.Name = "miViewZoomOut";
			miViewZoomOut.ShortcutKeys = Keys.Control | Keys.OemMinus;
			miViewZoomOut.Size = new Size(275, 26);
			miViewZoomOut.Text = "Уменьшить";
			miViewZoomOut.Click += miViewZoomOut_Click;
			//
			// miViewZoomReset
			//
			miViewZoomReset.Name = "miViewZoomReset";
			miViewZoomReset.ShortcutKeys = Keys.Control | Keys.D0;
			miViewZoomReset.Size = new Size(275, 26);
			miViewZoomReset.Text = "Сбросить масштаб";
			miViewZoomReset.Click += miViewZoomReset_Click;
			//
			// miOptions
			//
			miOptions.DropDownItems.AddRange(new ToolStripItem[] { miOptionsTreeSettings });
			miOptions.Name = "miOptions";
			miOptions.Size = new Size(93, 24);
			miOptions.Text = "Параметры";
			//
			// miOptionsTreeSettings
			//
			miOptionsTreeSettings.Enabled = false;
			miOptionsTreeSettings.Name = "miOptionsTreeSettings";
			miOptionsTreeSettings.ShortcutKeys = Keys.Control | Keys.P;
			miOptionsTreeSettings.Size = new Size(252, 26);
			miOptionsTreeSettings.Text = "Параметры дерева";
			miOptionsTreeSettings.Click += miOptionsTreeSettings_Click;
			//
			// miLanguage
			//
			miLanguage.DropDownItems.AddRange(new ToolStripItem[] { miLangRu, miLangEn, miLangUz, miLangTg, miLangKk, miLangFr, miLangDe, miLangIt });
			miLanguage.Name = "miLanguage";
			miLanguage.Size = new Size(59, 24);
			miLanguage.Text = "Язык";
			//
			// miLangRu
			//
			miLangRu.Name = "miLangRu";
			miLangRu.Size = new Size(224, 26);
			miLangRu.Text = "Русский";
			miLangRu.Click += miLangRu_Click;
			//
			// miLangEn
			//
			miLangEn.Name = "miLangEn";
			miLangEn.Size = new Size(224, 26);
			miLangEn.Text = "English";
			miLangEn.Click += miLangEn_Click;
			//
			// miLangUz
			//
			miLangUz.Name = "miLangUz";
			miLangUz.Size = new Size(224, 26);
			miLangUz.Text = "O‘zbek";
			miLangUz.Click += miLangUz_Click;
			//
			// miLangTg
			//
			miLangTg.Name = "miLangTg";
			miLangTg.Size = new Size(224, 26);
			miLangTg.Text = "Тоҷикӣ";
			miLangTg.Click += miLangTg_Click;
			//
			// miLangKk
			//
			miLangKk.Name = "miLangKk";
			miLangKk.Size = new Size(224, 26);
			miLangKk.Text = "Қазақша";
			miLangKk.Click += miLangKk_Click;
			//
			// miLangFr
			//
			miLangFr.Name = "miLangFr";
			miLangFr.Size = new Size(224, 26);
			miLangFr.Text = "Français";
			miLangFr.Click += miLangFr_Click;
			//
			// miLangDe
			//
			miLangDe.Name = "miLangDe";
			miLangDe.Size = new Size(224, 26);
			miLangDe.Text = "Deutsch";
			miLangDe.Click += miLangDe_Click;
			//
			// miLangIt
			//
			miLangIt.Name = "miLangIt";
			miLangIt.Size = new Size(224, 26);
			miLangIt.Text = "Italiano";
			miLangIt.Click += miLangIt_Click;
			//
			// miHelp
			//
			miHelp.DropDownItems.AddRange(new ToolStripItem[] { miHelpAbout });
			miHelp.Name = "miHelp";
			miHelp.Size = new Size(77, 24);
			miHelp.Text = "Справка";
			//
			// miHelpAbout
			//
			miHelpAbout.Name = "miHelpAbout";
			miHelpAbout.Size = new Size(186, 26);
			miHelpAbout.Text = "О программе";
			miHelpAbout.Click += miHelpAbout_Click;
			//
			// panelSettings
			//
			panelSettings.BackColor = SystemColors.Control;
			panelSettings.BorderStyle = BorderStyle.FixedSingle;
			panelSettings.Controls.Add(labelFont);
			panelSettings.Controls.Add(cboFont);
			panelSettings.Controls.Add(btnApply);
			panelSettings.Controls.Add(labelIgnore);
			panelSettings.Controls.Add(lstIgnore);
			panelSettings.Controls.Add(labelExtensions);
			panelSettings.Controls.Add(checkBoxAll);
			panelSettings.Controls.Add(lstExtensions);
			panelSettings.Controls.Add(labelRootFolders);
			panelSettings.Controls.Add(lstRootFolders);
			panelSettings.Dock = DockStyle.Top;
			panelSettings.Location = new Point(0, 28);
			panelSettings.Name = "panelSettings";
			panelSettings.Size = new Size(893, 227);
			panelSettings.TabIndex = 1;
			panelSettings.Visible = false;
			//
			// labelFont
			//
			labelFont.AutoSize = true;
			labelFont.Location = new Point(12, 10);
			labelFont.Name = "labelFont";
			labelFont.Size = new Size(113, 20);
			labelFont.TabIndex = 0;
			labelFont.Text = "Шрифт дерева:";
			//
			// cboFont
			//
			cboFont.DropDownStyle = ComboBoxStyle.DropDownList;
			cboFont.Location = new Point(12, 36);
			cboFont.Name = "cboFont";
			cboFont.Size = new Size(200, 28);
			cboFont.TabIndex = 1;
			cboFont.SelectedIndexChanged += cboFont_SelectedIndexChanged;
			//
			// btnApply
			//
			btnApply.Location = new Point(12, 70);
			btnApply.Name = "btnApply";
			btnApply.Size = new Size(200, 37);
			btnApply.TabIndex = 2;
			btnApply.Text = "Применить настройки";
			btnApply.Click += btnApply_Click;
			//
			// labelIgnore
			//
			labelIgnore.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			labelIgnore.AutoSize = true;
			labelIgnore.Location = new Point(235, 10);
			labelIgnore.Name = "labelIgnore";
			labelIgnore.Size = new Size(119, 20);
			labelIgnore.TabIndex = 3;
			labelIgnore.Text = "Игнорировать:";
			//
			// lstIgnore
			//
			lstIgnore.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			lstIgnore.CheckOnClick = true;
			lstIgnore.FormattingEnabled = true;
			lstIgnore.Location = new Point(235, 36);
			lstIgnore.Name = "lstIgnore";
			lstIgnore.Size = new Size(250, 180);
			lstIgnore.TabIndex = 4;
			//
			// labelExtensions
			//
			labelExtensions.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			labelExtensions.AutoSize = true;
			labelExtensions.Location = new Point(495, 10);
			labelExtensions.Name = "labelExtensions";
			labelExtensions.Size = new Size(105, 20);
			labelExtensions.TabIndex = 5;
			labelExtensions.Text = "Типы файлов:";
			//
			// checkBoxAll
			//
			checkBoxAll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			checkBoxAll.AutoSize = true;
			checkBoxAll.Checked = true;
			checkBoxAll.CheckState = CheckState.Checked;
			checkBoxAll.Location = new Point(625, 10);
			checkBoxAll.Name = "checkBoxAll";
			checkBoxAll.Size = new Size(55, 24);
			checkBoxAll.TabIndex = 6;
			checkBoxAll.Text = "Все";
			checkBoxAll.CheckedChanged += checkBoxAll_CheckedChanged;
			//
			// lstExtensions
			//
			lstExtensions.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			lstExtensions.CheckOnClick = true;
			lstExtensions.FormattingEnabled = true;
			lstExtensions.Location = new Point(495, 36);
			lstExtensions.Name = "lstExtensions";
			lstExtensions.Size = new Size(190, 180);
			lstExtensions.TabIndex = 7;
			//
			// labelRootFolders
			//
			labelRootFolders.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			labelRootFolders.AutoSize = true;
			labelRootFolders.Location = new Point(695, 10);
			labelRootFolders.Name = "labelRootFolders";
			labelRootFolders.Size = new Size(178, 20);
			labelRootFolders.TabIndex = 8;
			labelRootFolders.Text = "Папки верхнего уровня:";
			//
			// lstRootFolders
			//
			lstRootFolders.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			lstRootFolders.CheckOnClick = true;
			lstRootFolders.FormattingEnabled = true;
			lstRootFolders.Location = new Point(695, 36);
			lstRootFolders.Name = "lstRootFolders";
			lstRootFolders.Size = new Size(190, 180);
			lstRootFolders.TabIndex = 9;
			//
			// treeProject
			//
			treeProject.CheckBoxes = true;
			treeProject.Dock = DockStyle.Fill;
			treeProject.Font = new Font("Consolas", 9F);
			treeProject.HideSelection = false;
			treeProject.LineColor = Color.Black;
			treeProject.Location = new Point(0, 255);
			treeProject.Name = "treeProject";
			treeProject.Size = new Size(893, 667);
			treeProject.TabIndex = 2;
			treeProject.AfterCheck += treeProject_AfterCheck;
			treeProject.MouseWheel += treeProject_MouseWheel;
			treeProject.MouseEnter += treeProject_MouseEnter;
			//
			// Form1
			//
			AutoScaleMode = AutoScaleMode.None;
			ClientSize = new Size(893, 922);
			Controls.Add(treeProject);
			Controls.Add(panelSettings);
			Controls.Add(menuStripMain);
			Icon = (Icon)resources.GetObject("$this.Icon");
			MainMenuStrip = menuStripMain;
			MinimumSize = new Size(700, 450);
			Name = "Form1";
			Text = "Project Tree Viewer by Avazbek";
			menuStripMain.ResumeLayout(false);
			menuStripMain.PerformLayout();
			panelSettings.ResumeLayout(false);
			panelSettings.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}
		#endregion

		private System.ComponentModel.IContainer components = null!;

		private MenuStrip menuStripMain;

		private ToolStripMenuItem miFile;
		private ToolStripMenuItem miFileOpen;
		private ToolStripMenuItem miFileRefresh;
		private ToolStripSeparator miFileSep1;
		private ToolStripMenuItem miFileExit;

		private ToolStripMenuItem miCopy;
		private ToolStripMenuItem miCopyFullTree;
		private ToolStripMenuItem miCopySelectedTree;
		private ToolStripMenuItem miCopySelectedContent;
		private ToolStripMenuItem miCopyFullTreeAndContent;

		private ToolStripMenuItem miView;
		private ToolStripMenuItem miViewExpandAll;
		private ToolStripMenuItem miViewCollapseAll;
		private ToolStripSeparator miViewSep1;
		private ToolStripMenuItem miViewZoomIn;
		private ToolStripMenuItem miViewZoomOut;
		private ToolStripMenuItem miViewZoomReset;

		private ToolStripMenuItem miOptions;
		private ToolStripMenuItem miOptionsTreeSettings;

		private ToolStripMenuItem miLanguage;
		private ToolStripMenuItem miLangRu;
		private ToolStripMenuItem miLangEn;
		private ToolStripMenuItem miLangUz;
		private ToolStripMenuItem miLangTg;
		private ToolStripMenuItem miLangKk;
		private ToolStripMenuItem miLangFr;
		private ToolStripMenuItem miLangDe;
		private ToolStripMenuItem miLangIt;

		private ToolStripMenuItem miHelp;
		private ToolStripMenuItem miHelpAbout;

		private Panel panelSettings;

		private Label labelFont;
		private ComboBox cboFont;
		private Button btnApply;

		private Label labelIgnore;
		private CheckedListBox lstIgnore;

		private Label labelExtensions;
		private CheckedListBox lstExtensions;
		private CheckBox checkBoxAll;

		private Label labelRootFolders;
		private CheckedListBox lstRootFolders;

		private TreeView treeProject;
	}
}
