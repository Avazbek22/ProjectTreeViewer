// ─────────────────────────────────────────────────────────────
//  Form1.Designer.cs (full, working)
//  Right-side vertical settings panel + stable layout
// ─────────────────────────────────────────────────────────────
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProjectTreeViewer.WinForms
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
			checkBoxIgnoreAll = new CheckBox();
			labelExtensions = new Label();
			checkBoxAll = new CheckBox();
			lstExtensions = new CheckedListBox();
			labelRootFolders = new Label();
			checkBoxRootAll = new CheckBox();
			lstRootFolders = new CheckedListBox();
			treeProject = new TreeView();
			menuStripMain.SuspendLayout();
			panelSettings.SuspendLayout();
			SuspendLayout();
			// 
			// menuStripMain
			// 
			menuStripMain.ImageScalingSize = new Size(20, 20);
			menuStripMain.Items.AddRange(new ToolStripItem[] { miFile, miCopy, miView, miOptions, miLanguage, miHelp });
			menuStripMain.Location = new Point(0, 0);
			menuStripMain.Name = "menuStripMain";
			menuStripMain.Size = new Size(1128, 24);
			menuStripMain.TabIndex = 0;
			// 
			// miFile
			// 
			miFile.DropDownItems.AddRange(new ToolStripItem[] { miFileOpen, miFileRefresh, miFileSep1, miFileExit });
			miFile.Name = "miFile";
			miFile.Size = new Size(14, 20);
			// 
			// miFileOpen
			// 
			miFileOpen.Name = "miFileOpen";
			miFileOpen.ShortcutKeys = Keys.Control | Keys.O;
			miFileOpen.Size = new Size(136, 26);
			miFileOpen.Click += miFileOpen_Click;
			// 
			// miFileRefresh
			// 
			miFileRefresh.Enabled = false;
			miFileRefresh.Name = "miFileRefresh";
			miFileRefresh.ShortcutKeys = Keys.F5;
			miFileRefresh.Size = new Size(136, 26);
			miFileRefresh.Click += miFileRefresh_Click;
			// 
			// miFileSep1
			// 
			miFileSep1.Name = "miFileSep1";
			miFileSep1.Size = new Size(133, 6);
			// 
			// miFileExit
			// 
			miFileExit.Name = "miFileExit";
			miFileExit.Size = new Size(136, 26);
			miFileExit.Click += miFileExit_Click;
			// 
			// miCopy
			// 
			miCopy.DropDownItems.AddRange(new ToolStripItem[] { miCopyFullTree, miCopySelectedTree, miCopySelectedContent, miCopyFullTreeAndContent });
			miCopy.Name = "miCopy";
			miCopy.Size = new Size(14, 20);
			// 
			// miCopyFullTree
			// 
			miCopyFullTree.Enabled = false;
			miCopyFullTree.Name = "miCopyFullTree";
			miCopyFullTree.ShortcutKeys = Keys.Control | Keys.Shift | Keys.C;
			miCopyFullTree.Size = new Size(174, 26);
			miCopyFullTree.Click += miCopyFullTree_Click;
			// 
			// miCopySelectedTree
			// 
			miCopySelectedTree.Enabled = false;
			miCopySelectedTree.Name = "miCopySelectedTree";
			miCopySelectedTree.ShortcutKeys = Keys.Control | Keys.Alt | Keys.C;
			miCopySelectedTree.Size = new Size(174, 26);
			miCopySelectedTree.Click += miCopySelectedTree_Click;
			// 
			// miCopySelectedContent
			// 
			miCopySelectedContent.Enabled = false;
			miCopySelectedContent.Name = "miCopySelectedContent";
			miCopySelectedContent.ShortcutKeys = Keys.Control | Keys.Alt | Keys.V;
			miCopySelectedContent.Size = new Size(174, 26);
			miCopySelectedContent.Click += miCopySelectedContent_Click;
			// 
			// miCopyFullTreeAndContent
			// 
			miCopyFullTreeAndContent.Enabled = false;
			miCopyFullTreeAndContent.Name = "miCopyFullTreeAndContent";
			miCopyFullTreeAndContent.ShortcutKeys = Keys.Control | Keys.Shift | Keys.V;
			miCopyFullTreeAndContent.Size = new Size(174, 26);
			miCopyFullTreeAndContent.Click += miCopyFullTreeAndContent_Click;
			// 
			// miView
			// 
			miView.DropDownItems.AddRange(new ToolStripItem[] { miViewExpandAll, miViewCollapseAll, miViewSep1, miViewZoomIn, miViewZoomOut, miViewZoomReset });
			miView.Name = "miView";
			miView.Size = new Size(14, 20);
			// 
			// miViewExpandAll
			// 
			miViewExpandAll.Enabled = false;
			miViewExpandAll.Name = "miViewExpandAll";
			miViewExpandAll.ShortcutKeys = Keys.Control | Keys.E;
			miViewExpandAll.Size = new Size(196, 26);
			miViewExpandAll.Click += miViewExpandAll_Click;
			// 
			// miViewCollapseAll
			// 
			miViewCollapseAll.Enabled = false;
			miViewCollapseAll.Name = "miViewCollapseAll";
			miViewCollapseAll.ShortcutKeys = Keys.Control | Keys.W;
			miViewCollapseAll.Size = new Size(196, 26);
			miViewCollapseAll.Click += miViewCollapseAll_Click;
			// 
			// miViewSep1
			// 
			miViewSep1.Name = "miViewSep1";
			miViewSep1.Size = new Size(193, 6);
			// 
			// miViewZoomIn
			// 
			miViewZoomIn.Name = "miViewZoomIn";
			miViewZoomIn.ShortcutKeys = Keys.Control | Keys.Oemplus;
			miViewZoomIn.Size = new Size(196, 26);
			miViewZoomIn.Click += miViewZoomIn_Click;
			// 
			// miViewZoomOut
			// 
			miViewZoomOut.Name = "miViewZoomOut";
			miViewZoomOut.ShortcutKeys = Keys.Control | Keys.OemMinus;
			miViewZoomOut.Size = new Size(196, 26);
			miViewZoomOut.Click += miViewZoomOut_Click;
			// 
			// miViewZoomReset
			// 
			miViewZoomReset.Name = "miViewZoomReset";
			miViewZoomReset.ShortcutKeys = Keys.Control | Keys.D0;
			miViewZoomReset.Size = new Size(196, 26);
			miViewZoomReset.Click += miViewZoomReset_Click;
			// 
			// miOptions
			// 
			miOptions.Enabled = false;
			miOptions.Name = "miOptions";
			miOptions.ShortcutKeys = Keys.Control | Keys.P;
			miOptions.ShowShortcutKeys = false;
			miOptions.Size = new Size(14, 20);
			miOptions.Click += miOptions_Click;
			// 
			// miLanguage
			// 
			miLanguage.DropDownItems.AddRange(new ToolStripItem[] { miLangRu, miLangEn, miLangUz, miLangTg, miLangKk, miLangFr, miLangDe, miLangIt });
			miLanguage.Name = "miLanguage";
			miLanguage.Size = new Size(14, 20);
			// 
			// miLangRu
			// 
			miLangRu.Name = "miLangRu";
			miLangRu.Size = new Size(83, 26);
			miLangRu.Click += miLangRu_Click;
			// 
			// miLangEn
			// 
			miLangEn.Name = "miLangEn";
			miLangEn.Size = new Size(83, 26);
			miLangEn.Click += miLangEn_Click;
			// 
			// miLangUz
			// 
			miLangUz.Name = "miLangUz";
			miLangUz.Size = new Size(83, 26);
			miLangUz.Click += miLangUz_Click;
			// 
			// miLangTg
			// 
			miLangTg.Name = "miLangTg";
			miLangTg.Size = new Size(83, 26);
			miLangTg.Click += miLangTg_Click;
			// 
			// miLangKk
			// 
			miLangKk.Name = "miLangKk";
			miLangKk.Size = new Size(83, 26);
			miLangKk.Click += miLangKk_Click;
			// 
			// miLangFr
			// 
			miLangFr.Name = "miLangFr";
			miLangFr.Size = new Size(83, 26);
			miLangFr.Click += miLangFr_Click;
			// 
			// miLangDe
			// 
			miLangDe.Name = "miLangDe";
			miLangDe.Size = new Size(83, 26);
			miLangDe.Click += miLangDe_Click;
			// 
			// miLangIt
			// 
			miLangIt.Name = "miLangIt";
			miLangIt.Size = new Size(83, 26);
			miLangIt.Click += miLangIt_Click;
			// 
			// miHelp
			// 
			miHelp.DropDownItems.AddRange(new ToolStripItem[] { miHelpAbout });
			miHelp.Name = "miHelp";
			miHelp.Size = new Size(14, 20);
			// 
			// miHelpAbout
			// 
			miHelpAbout.Name = "miHelpAbout";
			miHelpAbout.Size = new Size(83, 26);
			miHelpAbout.Click += miHelpAbout_Click;
			// 
			// panelSettings
			// 
			panelSettings.AutoScroll = true;
			panelSettings.BackColor = SystemColors.Control;
			panelSettings.BorderStyle = BorderStyle.FixedSingle;
			panelSettings.Controls.Add(labelFont);
			panelSettings.Controls.Add(cboFont);
			panelSettings.Controls.Add(btnApply);
			panelSettings.Controls.Add(labelIgnore);
			panelSettings.Controls.Add(lstIgnore);
			panelSettings.Controls.Add(checkBoxIgnoreAll);
			panelSettings.Controls.Add(labelExtensions);
			panelSettings.Controls.Add(checkBoxAll);
			panelSettings.Controls.Add(lstExtensions);
			panelSettings.Controls.Add(labelRootFolders);
			panelSettings.Controls.Add(checkBoxRootAll);
			panelSettings.Controls.Add(lstRootFolders);
			panelSettings.Location = new Point(808, 28);
			panelSettings.Name = "panelSettings";
			panelSettings.Size = new Size(320, 962);
			panelSettings.TabIndex = 1;
			panelSettings.Visible = false;
			// 
			// labelFont
			// 
			labelFont.AutoSize = true;
			labelFont.Location = new Point(12, 12);
			labelFont.Name = "labelFont";
			labelFont.Size = new Size(0, 20);
			labelFont.TabIndex = 0;
			// 
			// cboFont
			// 
			cboFont.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			cboFont.DropDownStyle = ComboBoxStyle.DropDownList;
			cboFont.Location = new Point(12, 38);
			cboFont.Name = "cboFont";
			cboFont.Size = new Size(291, 28);
			cboFont.TabIndex = 1;
			cboFont.SelectedIndexChanged += cboFont_SelectedIndexChanged;
			// 
			// btnApply
			// 
			btnApply.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			btnApply.Location = new Point(12, 74);
			btnApply.Name = "btnApply";
			btnApply.Size = new Size(291, 37);
			btnApply.TabIndex = 2;
			btnApply.Click += btnApply_Click;
			// 
			// labelIgnore
			// 
			labelIgnore.AutoSize = true;
			labelIgnore.Location = new Point(13, 132);
			labelIgnore.Name = "labelIgnore";
			labelIgnore.Size = new Size(0, 20);
			labelIgnore.TabIndex = 3;
			// 
			// lstIgnore
			// 
			lstIgnore.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lstIgnore.CheckOnClick = true;
			lstIgnore.FormattingEnabled = true;
			lstIgnore.Location = new Point(13, 155);
			lstIgnore.Name = "lstIgnore";
			lstIgnore.Size = new Size(290, 202);
			lstIgnore.TabIndex = 4;
			// 
			// checkBoxIgnoreAll
			// 
			checkBoxIgnoreAll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			checkBoxIgnoreAll.AutoSize = true;
			checkBoxIgnoreAll.Checked = true;
			checkBoxIgnoreAll.CheckState = CheckState.Checked;
			checkBoxIgnoreAll.Location = new Point(285, 132);
			checkBoxIgnoreAll.Name = "checkBoxIgnoreAll";
			checkBoxIgnoreAll.Size = new Size(18, 17);
			checkBoxIgnoreAll.TabIndex = 5;
			checkBoxIgnoreAll.CheckedChanged += checkBoxIgnoreAll_CheckedChanged;
			// 
			// labelExtensions
			// 
			labelExtensions.AutoSize = true;
			labelExtensions.Location = new Point(9, 388);
			labelExtensions.Name = "labelExtensions";
			labelExtensions.Size = new Size(0, 20);
			labelExtensions.TabIndex = 6;
			// 
			// checkBoxAll
			// 
			checkBoxAll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			checkBoxAll.AutoSize = true;
			checkBoxAll.Checked = true;
			checkBoxAll.CheckState = CheckState.Checked;
			checkBoxAll.Location = new Point(285, 387);
			checkBoxAll.Name = "checkBoxAll";
			checkBoxAll.Size = new Size(18, 17);
			checkBoxAll.TabIndex = 7;
			checkBoxAll.CheckedChanged += checkBoxAll_CheckedChanged;
			// 
			// lstExtensions
			// 
			lstExtensions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lstExtensions.CheckOnClick = true;
			lstExtensions.FormattingEnabled = true;
			lstExtensions.Location = new Point(12, 411);
			lstExtensions.Name = "lstExtensions";
			lstExtensions.Size = new Size(291, 202);
			lstExtensions.TabIndex = 8;
			lstExtensions.ItemCheck += lstExtensions_ItemCheck;
			// 
			// labelRootFolders
			// 
			labelRootFolders.AutoSize = true;
			labelRootFolders.Location = new Point(9, 638);
			labelRootFolders.Name = "labelRootFolders";
			labelRootFolders.Size = new Size(0, 20);
			labelRootFolders.TabIndex = 9;
			// 
			// checkBoxRootAll
			// 
			checkBoxRootAll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			checkBoxRootAll.AutoSize = true;
			checkBoxRootAll.Checked = true;
			checkBoxRootAll.CheckState = CheckState.Checked;
			checkBoxRootAll.Location = new Point(285, 637);
			checkBoxRootAll.Name = "checkBoxRootAll";
			checkBoxRootAll.Size = new Size(18, 17);
			checkBoxRootAll.TabIndex = 10;
			checkBoxRootAll.CheckedChanged += checkBoxRootAll_CheckedChanged;
			// 
			// lstRootFolders
			// 
			lstRootFolders.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			lstRootFolders.CheckOnClick = true;
			lstRootFolders.FormattingEnabled = true;
			lstRootFolders.Location = new Point(13, 661);
			lstRootFolders.Name = "lstRootFolders";
			lstRootFolders.Size = new Size(290, 202);
			lstRootFolders.TabIndex = 11;
			lstRootFolders.ItemCheck += lstRootFolders_ItemCheck;
			// 
			// treeProject
			// 
			treeProject.CheckBoxes = true;
			treeProject.Font = new Font("Consolas", 9F);
			treeProject.HideSelection = false;
			treeProject.Location = new Point(0, 28);
			treeProject.Name = "treeProject";
			treeProject.Size = new Size(812, 962);
			treeProject.TabIndex = 2;
			treeProject.AfterCheck += treeProject_AfterCheck;
			treeProject.MouseEnter += treeProject_MouseEnter;
			treeProject.MouseWheel += treeProject_MouseWheel;
			// 
			// Form1
			// 
			AutoScaleMode = AutoScaleMode.None;
			ClientSize = new Size(1128, 989);
			Controls.Add(panelSettings);
			Controls.Add(treeProject);
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
		private CheckBox checkBoxIgnoreAll;

        private Label labelExtensions;
        private CheckedListBox lstExtensions;
        private CheckBox checkBoxAll;

        private Label labelRootFolders;
        private CheckedListBox lstRootFolders;
		private CheckBox checkBoxRootAll;

        private TreeView treeProject;
    }
}
