// ─────────────────────────────────────────────────────────────
//  Form1.Designer.cs   (полная версия, совместимая с WinForms-Designer)
// ─────────────────────────────────────────────────────────────
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
			panelToolbar = new Panel();
			btnOpen = new Button();
			btnCopy = new Button();
			btnRefresh = new Button();
			btnSettings = new Button();
			panelSettings = new Panel();
			cbIgnoreBin = new CheckBox();
			cbIgnoreObj = new CheckBox();
			cbIgnoreDot = new CheckBox();
			labelExtensions = new Label();
			lstExtensions = new CheckedListBox();
			labelRootFolders = new Label();
			lstRootFolders = new CheckedListBox();
			labelFont = new Label();
			cboFont = new ComboBox();
			btnApply = new Button();
			txtTree = new TextBox();
			panelToolbar.SuspendLayout();
			panelSettings.SuspendLayout();
			SuspendLayout();
			// 
			// panelToolbar
			// 
			panelToolbar.Controls.Add(btnOpen);
			panelToolbar.Controls.Add(btnCopy);
			panelToolbar.Controls.Add(btnRefresh);
			panelToolbar.Controls.Add(btnSettings);
			panelToolbar.Dock = DockStyle.Top;
			panelToolbar.Location = new Point(0, 0);
			panelToolbar.Name = "panelToolbar";
			panelToolbar.Size = new Size(906, 40);
			panelToolbar.TabIndex = 2;
			// 
			// btnOpen
			// 
			btnOpen.Location = new Point(10, 5);
			btnOpen.Name = "btnOpen";
			btnOpen.Size = new Size(140, 30);
			btnOpen.TabIndex = 0;
			btnOpen.Text = "Открыть папку";
			btnOpen.UseVisualStyleBackColor = true;
			btnOpen.Click += btnOpen_Click;
			// 
			// btnCopy
			// 
			btnCopy.Location = new Point(160, 5);
			btnCopy.Name = "btnCopy";
			btnCopy.Size = new Size(140, 30);
			btnCopy.TabIndex = 1;
			btnCopy.Text = "Скопировать всё";
			btnCopy.UseVisualStyleBackColor = true;
			btnCopy.Click += btnCopy_Click;
			// 
			// btnRefresh
			// 
			btnRefresh.Enabled = false;
			btnRefresh.Location = new Point(310, 5);
			btnRefresh.Name = "btnRefresh";
			btnRefresh.Size = new Size(140, 30);
			btnRefresh.TabIndex = 2;
			btnRefresh.Text = "Обновить";
			btnRefresh.UseVisualStyleBackColor = true;
			btnRefresh.Click += btnRefresh_Click;
			// 
			// btnSettings
			// 
			btnSettings.Location = new Point(460, 5);
			btnSettings.Name = "btnSettings";
			btnSettings.Size = new Size(140, 30);
			btnSettings.TabIndex = 3;
			btnSettings.Text = "Настройки";
			btnSettings.UseVisualStyleBackColor = true;
			btnSettings.Click += btnSettings_Click;
			// 
			// panelSettings
			// 
			panelSettings.Controls.Add(cbIgnoreBin);
			panelSettings.Controls.Add(cbIgnoreObj);
			panelSettings.Controls.Add(cbIgnoreDot);
			panelSettings.Controls.Add(labelExtensions);
			panelSettings.Controls.Add(lstExtensions);
			panelSettings.Controls.Add(labelRootFolders);
			panelSettings.Controls.Add(lstRootFolders);
			panelSettings.Controls.Add(labelFont);
			panelSettings.Controls.Add(cboFont);
			panelSettings.Controls.Add(btnApply);
			panelSettings.Dock = DockStyle.Top;
			panelSettings.Location = new Point(0, 40);
			panelSettings.Name = "panelSettings";
			panelSettings.Size = new Size(906, 223);
			panelSettings.TabIndex = 1;
			panelSettings.Visible = false;
			// 
			// cbIgnoreBin
			// 
			cbIgnoreBin.AutoSize = true;
			cbIgnoreBin.Checked = true;
			cbIgnoreBin.CheckState = CheckState.Checked;
			cbIgnoreBin.Location = new Point(10, 10);
			cbIgnoreBin.Name = "cbIgnoreBin";
			cbIgnoreBin.Size = new Size(201, 24);
			cbIgnoreBin.TabIndex = 0;
			cbIgnoreBin.Text = "Игнорировать папку bin";
			// 
			// cbIgnoreObj
			// 
			cbIgnoreObj.AutoSize = true;
			cbIgnoreObj.Checked = true;
			cbIgnoreObj.CheckState = CheckState.Checked;
			cbIgnoreObj.Location = new Point(10, 40);
			cbIgnoreObj.Name = "cbIgnoreObj";
			cbIgnoreObj.Size = new Size(202, 24);
			cbIgnoreObj.TabIndex = 1;
			cbIgnoreObj.Text = "Игнорировать папку obj";
			// 
			// cbIgnoreDot
			// 
			cbIgnoreDot.AutoSize = true;
			cbIgnoreDot.Checked = true;
			cbIgnoreDot.CheckState = CheckState.Checked;
			cbIgnoreDot.Location = new Point(10, 70);
			cbIgnoreDot.Name = "cbIgnoreDot";
			cbIgnoreDot.Size = new Size(431, 24);
			cbIgnoreDot.TabIndex = 2;
			cbIgnoreDot.Text = "Игнорировать скрытые файлы/папки (с точкой в начале)";
			// 
			// labelExtensions
			// 
			labelExtensions.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			labelExtensions.AutoSize = true;
			labelExtensions.Location = new Point(476, 11);
			labelExtensions.Name = "labelExtensions";
			labelExtensions.Size = new Size(105, 20);
			labelExtensions.TabIndex = 3;
			labelExtensions.Text = "Типы файлов:";
			// 
			// lstExtensions
			// 
			lstExtensions.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			lstExtensions.CheckOnClick = true;
			lstExtensions.Location = new Point(476, 33);
			lstExtensions.Name = "lstExtensions";
			lstExtensions.Size = new Size(200, 180);
			lstExtensions.TabIndex = 4;
			// 
			// labelRootFolders
			// 
			labelRootFolders.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			labelRootFolders.AutoSize = true;
			labelRootFolders.Location = new Point(694, 10);
			labelRootFolders.Name = "labelRootFolders";
			labelRootFolders.Size = new Size(178, 20);
			labelRootFolders.TabIndex = 5;
			labelRootFolders.Text = "Папки верхнего уровня:";
			// 
			// lstRootFolders
			// 
			lstRootFolders.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			lstRootFolders.CheckOnClick = true;
			lstRootFolders.Location = new Point(694, 33);
			lstRootFolders.Name = "lstRootFolders";
			lstRootFolders.Size = new Size(200, 180);
			lstRootFolders.TabIndex = 6;
			// 
			// labelFont
			// 
			labelFont.AutoSize = true;
			labelFont.Location = new Point(12, 126);
			labelFont.Name = "labelFont";
			labelFont.Size = new Size(113, 20);
			labelFont.TabIndex = 7;
			labelFont.Text = "Шрифт дерева:";
			// 
			// cboFont
			// 
			cboFont.DropDownStyle = ComboBoxStyle.DropDownList;
			cboFont.Location = new Point(12, 149);
			cboFont.Name = "cboFont";
			cboFont.Size = new Size(200, 28);
			cboFont.TabIndex = 8;
			cboFont.SelectedIndexChanged += cboFont_SelectedIndexChanged;
			// 
			// btnApply
			// 
			btnApply.Location = new Point(10, 183);
			btnApply.Name = "btnApply";
			btnApply.Size = new Size(202, 37);
			btnApply.TabIndex = 9;
			btnApply.Text = "Применить настройки";
			btnApply.UseVisualStyleBackColor = true;
			btnApply.Click += btnApply_Click;
			// 
			// txtTree
			// 
			txtTree.Dock = DockStyle.Fill;
			txtTree.Font = new Font("Consolas", 9F);
			txtTree.Location = new Point(0, 263);
			txtTree.Multiline = true;
			txtTree.Name = "txtTree";
			txtTree.ReadOnly = true;
			txtTree.ScrollBars = ScrollBars.Both;
			txtTree.Size = new Size(906, 659);
			txtTree.TabIndex = 0;
			txtTree.WordWrap = false;
			// 
			// Form1
			// 
			AutoScaleMode = AutoScaleMode.None;
			ClientSize = new Size(906, 922);
			Controls.Add(txtTree);
			Controls.Add(panelSettings);
			Controls.Add(panelToolbar);
			Icon = (Icon)resources.GetObject("$this.Icon");
			MinimumSize = new Size(600, 400);
			Name = "Form1";
			Text = "Project Tree Viewer by Avazbek";
			panelToolbar.ResumeLayout(false);
			panelSettings.ResumeLayout(false);
			panelSettings.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}
		#endregion

		// ────── поля компонентов ──────
		private System.ComponentModel.IContainer components = null!;
		private System.Windows.Forms.Panel panelToolbar;
		private System.Windows.Forms.Button btnOpen;
		private System.Windows.Forms.Button btnCopy;
		private System.Windows.Forms.Button btnRefresh;
		private System.Windows.Forms.Button btnSettings;

		private System.Windows.Forms.Panel panelSettings;
		private System.Windows.Forms.CheckBox cbIgnoreBin;
		private System.Windows.Forms.CheckBox cbIgnoreObj;
		private System.Windows.Forms.CheckBox cbIgnoreDot;
		private System.Windows.Forms.Label labelExtensions;
		private System.Windows.Forms.CheckedListBox lstExtensions;
		private System.Windows.Forms.Label labelRootFolders;
		private System.Windows.Forms.CheckedListBox lstRootFolders;
		private System.Windows.Forms.Label labelFont;
		private System.Windows.Forms.ComboBox cboFont;
		private System.Windows.Forms.Button btnApply;

		private System.Windows.Forms.TextBox txtTree;
	}
}
