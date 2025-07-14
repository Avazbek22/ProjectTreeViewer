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
			System.ComponentModel.ComponentResourceManager resources =
				new System.ComponentModel.ComponentResourceManager(typeof(Form1));

			// ────── создание компонентов ──────
			this.components = new System.ComponentModel.Container();
			this.panelToolbar = new Panel();
			this.btnOpen = new Button();
			this.btnCopy = new Button();
			this.btnRefresh = new Button();
			this.btnSettings = new Button();
			this.panelSettings = new Panel();
			this.cbIgnoreBin = new CheckBox();
			this.cbIgnoreObj = new CheckBox();
			this.cbIgnoreDot = new CheckBox();
			this.labelExtensions = new Label();
			this.lstExtensions = new CheckedListBox();
			this.labelRootFolders = new Label();
			this.lstRootFolders = new CheckedListBox();
			this.labelFont = new Label();
			this.cboFont = new ComboBox();
			this.btnApply = new Button();
			this.txtTree = new TextBox();

			this.panelToolbar.SuspendLayout();
			this.panelSettings.SuspendLayout();
			this.SuspendLayout();

			// ────── panelToolbar ──────
			this.panelToolbar.Controls.AddRange(new Control[]
			{
				this.btnOpen, this.btnCopy, this.btnRefresh, this.btnSettings
			});
			this.panelToolbar.Dock = DockStyle.Top;
			this.panelToolbar.Location = new Point(0, 0);
			this.panelToolbar.Name = "panelToolbar";
			this.panelToolbar.Size = new Size(893, 40);

			// btnOpen
			this.btnOpen.Location = new Point(10, 5);
			this.btnOpen.Name = "btnOpen";
			this.btnOpen.Size = new Size(140, 30);
			this.btnOpen.Text = "Открыть папку";
			this.btnOpen.Click += this.btnOpen_Click;

			// btnCopy
			this.btnCopy.Location = new Point(160, 5);
			this.btnCopy.Name = "btnCopy";
			this.btnCopy.Size = new Size(140, 30);
			this.btnCopy.Text = "Скопировать всё";
			this.btnCopy.Click += this.btnCopy_Click;

			// btnRefresh
			this.btnRefresh.Location = new Point(310, 5);
			this.btnRefresh.Name = "btnRefresh";
			this.btnRefresh.Size = new Size(140, 30);
			this.btnRefresh.Text = "Обновить";
			this.btnRefresh.Enabled = false;
			this.btnRefresh.Click += this.btnRefresh_Click;

			// btnSettings
			this.btnSettings.Location = new Point(460, 5);
			this.btnSettings.Name = "btnSettings";
			this.btnSettings.Size = new Size(140, 30);
			this.btnSettings.Text = "Настройки";
			this.btnSettings.Click += this.btnSettings_Click;

			// ────── panelSettings ──────
			this.panelSettings.Controls.AddRange(new Control[]
			{
				this.cbIgnoreBin, this.cbIgnoreObj, this.cbIgnoreDot,
				this.labelExtensions, this.lstExtensions,
				this.labelRootFolders, this.lstRootFolders,
				this.labelFont, this.cboFont,
				this.btnApply
			});
			this.panelSettings.Dock = DockStyle.Top;
			this.panelSettings.Location = new Point(0, 40);
			this.panelSettings.Name = "panelSettings";
			this.panelSettings.Size = new Size(893, 223);
			this.panelSettings.Visible = false;
			this.panelSettings.BackColor = SystemColors.Control;
			this.panelSettings.BorderStyle = BorderStyle.FixedSingle;

			// cbIgnoreBin
			this.cbIgnoreBin.AutoSize = true;
			this.cbIgnoreBin.Checked = true;
			this.cbIgnoreBin.Location = new Point(10, 10);
			this.cbIgnoreBin.Name = "cbIgnoreBin";
			this.cbIgnoreBin.Size = new Size(230, 24);
			this.cbIgnoreBin.Text = "Игнорировать все папки bin";
			this.cbIgnoreBin.CheckedChanged += this.cbIgnoreBin_CheckedChanged;

			// cbIgnoreObj
			this.cbIgnoreObj.AutoSize = true;
			this.cbIgnoreObj.Checked = true;
			this.cbIgnoreObj.Location = new Point(10, 40);
			this.cbIgnoreObj.Name = "cbIgnoreObj";
			this.cbIgnoreObj.Size = new Size(231, 24);
			this.cbIgnoreObj.Text = "Игнорировать все папки obj";
			this.cbIgnoreObj.CheckedChanged += this.cbIgnoreObj_CheckedChanged;

			// cbIgnoreDot
			this.cbIgnoreDot.AutoSize = true;
			this.cbIgnoreDot.Checked = true;
			this.cbIgnoreDot.Location = new Point(10, 70);
			this.cbIgnoreDot.Name = "cbIgnoreDot";
			this.cbIgnoreDot.Size = new Size(431, 24);
			this.cbIgnoreDot.Text = "Игнорировать скрытые файлы/папки (с точкой в начале)";
			this.cbIgnoreDot.CheckedChanged += this.cbIgnoreDot_CheckedChanged;

			// labelExtensions
			this.labelExtensions.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			this.labelExtensions.AutoSize = true;
			this.labelExtensions.Location = new Point(460, 10);
			this.labelExtensions.Name = "labelExtensions";
			this.labelExtensions.Size = new Size(105, 20);
			this.labelExtensions.Text = "Типы файлов:";

			// lstExtensions
			this.lstExtensions.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			this.lstExtensions.CheckOnClick = true;
			this.lstExtensions.Location = new Point(463, 33);
			this.lstExtensions.Name = "lstExtensions";
			this.lstExtensions.Size = new Size(200, 180);

			// labelRootFolders
			this.labelRootFolders.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			this.labelRootFolders.AutoSize = true;
			this.labelRootFolders.Location = new Point(681, 10);
			this.labelRootFolders.Name = "labelRootFolders";
			this.labelRootFolders.Size = new Size(178, 20);
			this.labelRootFolders.Text = "Папки верхнего уровня:";

			// lstRootFolders
			this.lstRootFolders.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			this.lstRootFolders.CheckOnClick = true;
			this.lstRootFolders.Location = new Point(681, 33);
			this.lstRootFolders.Name = "lstRootFolders";
			this.lstRootFolders.Size = new Size(200, 180);

			// labelFont
			this.labelFont.AutoSize = true;
			this.labelFont.Location = new Point(12, 126);
			this.labelFont.Name = "labelFont";
			this.labelFont.Size = new Size(113, 20);
			this.labelFont.Text = "Шрифт дерева:";

			// cboFont
			this.cboFont.DropDownStyle = ComboBoxStyle.DropDownList;
			this.cboFont.Location = new Point(12, 149);
			this.cboFont.Name = "cboFont";
			this.cboFont.Size = new Size(200, 28);
			this.cboFont.SelectedIndexChanged += this.cboFont_SelectedIndexChanged;

			// btnApply
			this.btnApply.Location = new Point(10, 183);
			this.btnApply.Name = "btnApply";
			this.btnApply.Size = new Size(202, 37);
			this.btnApply.Text = "Применить настройки";
			this.btnApply.Click += this.btnApply_Click;

			// txtTree
			this.txtTree.Dock = DockStyle.Fill;
			this.txtTree.Font = new Font("Consolas", 9F);
			this.txtTree.Location = new Point(0, 263);
			this.txtTree.Multiline = true;
			this.txtTree.Name = "txtTree";
			this.txtTree.ReadOnly = true;
			this.txtTree.ScrollBars = ScrollBars.Both;
			this.txtTree.WordWrap = false;
			this.txtTree.Size = new Size(893, 659);

			// ────── Form ──────
			this.AutoScaleMode = AutoScaleMode.None;
			this.ClientSize = new Size(893, 922);
			this.Controls.Add(this.txtTree);
			this.Controls.Add(this.panelSettings);
			this.Controls.Add(this.panelToolbar);
			this.Icon = (Icon)resources.GetObject("$this.Icon");
			this.MinimumSize = new Size(600, 400);
			this.Name = "Form1";
			this.Text = "Project Tree Viewer by Avazbek";

			this.panelToolbar.ResumeLayout(false);
			this.panelSettings.ResumeLayout(false);
			this.panelSettings.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		// ────── поля компонентов ──────
		private System.ComponentModel.IContainer components = null!;
		private Panel panelToolbar;
		private Button btnOpen;
		private Button btnCopy;
		private Button btnRefresh;
		private Button btnSettings;

		private Panel panelSettings;
		private CheckBox cbIgnoreBin;
		private CheckBox cbIgnoreObj;
		private CheckBox cbIgnoreDot;
		private Label labelExtensions;
		private CheckedListBox lstExtensions;
		private Label labelRootFolders;
		private CheckedListBox lstRootFolders;
		private Label labelFont;
		private ComboBox cboFont;
		private Button btnApply;

		private TextBox txtTree;
	}
}
