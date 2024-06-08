// Microsoft.SqlServer.Data.Tools.ExceptionMessageBox, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.Data.Tools.ExceptionMessageBox.ExceptionMessageBoxForm

using System.Windows.Forms;



namespace BlackbirdSql.Sys.Controls;


partial class AdvancedMessageBox
{
	/// <summary> 
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.IContainer components = null;

	/// <summary> 
	/// Clean up any resources being used.
	/// </summary>
	/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
	protected override void Dispose(bool disposing)
	{
		if (disposing && (components != null))
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	#region Windows Form Designer generated code

	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>

	private void InitializeComponent()
	{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdvancedMessageBox));
			this.pnlForm = new System.Windows.Forms.TableLayoutPanel();
			this.pnlIcon = new System.Windows.Forms.Panel();
			this.pnlMessage = new System.Windows.Forms.TableLayoutPanel();
			this.lblTopMessage = new System.Windows.Forms.LinkLabel();
			this.lblAdditionalInfo = new System.Windows.Forms.Label();
			this.pnlAdditional = new System.Windows.Forms.TableLayoutPanel();
			this.chkDontShow = new BlackbirdSql.Sys.Controls.Widgets.WrappingCheckBox();
			this.grpSeparator = new System.Windows.Forms.GroupBox();
			this.pnlButtons = new System.Windows.Forms.TableLayoutPanel();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.tbBtnHelp = new System.Windows.Forms.ToolStripDropDownButton();
			this.tbBtnHelpSingle = new System.Windows.Forms.ToolStripButton();
			this.tbBtnCopy = new System.Windows.Forms.ToolStripButton();
			this.tbBtnAdvanced = new System.Windows.Forms.ToolStripButton();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.button4 = new System.Windows.Forms.Button();
			this.button5 = new System.Windows.Forms.Button();
			this.imgIcons = new System.Windows.Forms.ImageList(this.components);
			this.pnlForm.SuspendLayout();
			this.pnlMessage.SuspendLayout();
			this.pnlButtons.SuspendLayout();
			this.toolStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// pnlForm
			// 
			resources.ApplyResources(this.pnlForm, "pnlForm");
			this.pnlForm.Controls.Add(this.pnlIcon, 0, 0);
			this.pnlForm.Controls.Add(this.pnlMessage, 1, 0);
			this.pnlForm.Controls.Add(this.chkDontShow, 1, 1);
			this.pnlForm.Controls.Add(this.grpSeparator, 0, 2);
			this.pnlForm.Controls.Add(this.pnlButtons, 0, 3);
			this.pnlForm.Name = "pnlForm";
			this.pnlForm.Click += new System.EventHandler(this.HideBorderLines);
			// 
			// pnlIcon
			// 
			resources.ApplyResources(this.pnlIcon, "pnlIcon");
			this.pnlIcon.Name = "pnlIcon";
			this.pnlIcon.Click += new System.EventHandler(this.HideBorderLines);
			this.pnlIcon.Paint += new System.Windows.Forms.PaintEventHandler(this.PnlIcon_Paint);
			// 
			// pnlMessage
			// 
			resources.ApplyResources(this.pnlMessage, "pnlMessage");
			this.pnlMessage.Controls.Add(this.lblTopMessage, 0, 0);
			this.pnlMessage.Controls.Add(this.lblAdditionalInfo, 0, 1);
			this.pnlMessage.Controls.Add(this.pnlAdditional, 0, 2);
			this.pnlMessage.Name = "pnlMessage";
			this.pnlMessage.Click += new System.EventHandler(this.HideBorderLines);
			// 
			// lblTopMessage
			// 
			resources.ApplyResources(this.lblTopMessage, "lblTopMessage");
			this.lblTopMessage.Name = "lblTopMessage";
			this.lblTopMessage.Click += new System.EventHandler(this.HideBorderLines);
			// 
			// lblAdditionalInfo
			// 
			resources.ApplyResources(this.lblAdditionalInfo, "lblAdditionalInfo");
			this.lblAdditionalInfo.Name = "lblAdditionalInfo";
			this.lblAdditionalInfo.Click += new System.EventHandler(this.HideBorderLines);
			// 
			// pnlAdditional
			// 
			resources.ApplyResources(this.pnlAdditional, "pnlAdditional");
			this.pnlAdditional.Name = "pnlAdditional";
			this.pnlAdditional.Click += new System.EventHandler(this.HideBorderLines);
			// 
			// chkDontShow
			// 
			resources.ApplyResources(this.chkDontShow, "chkDontShow");
			this.chkDontShow.Name = "chkDontShow";
			this.chkDontShow.Click += new System.EventHandler(this.HideBorderLines);
			// 
			// grpSeparator
			// 
			resources.ApplyResources(this.grpSeparator, "grpSeparator");
			this.pnlForm.SetColumnSpan(this.grpSeparator, 2);
			this.grpSeparator.Name = "grpSeparator";
			this.grpSeparator.TabStop = false;
			// 
			// pnlButtons
			// 
			resources.ApplyResources(this.pnlButtons, "pnlButtons");
			this.pnlForm.SetColumnSpan(this.pnlButtons, 2);
			this.pnlButtons.Controls.Add(this.toolStrip1, 0, 0);
			this.pnlButtons.Controls.Add(this.button1, 1, 0);
			this.pnlButtons.Controls.Add(this.button2, 2, 0);
			this.pnlButtons.Controls.Add(this.button3, 3, 0);
			this.pnlButtons.Controls.Add(this.button4, 4, 0);
			this.pnlButtons.Controls.Add(this.button5, 5, 0);
			this.pnlButtons.Name = "pnlButtons";
			this.pnlButtons.Click += new System.EventHandler(this.HideBorderLines);
			// 
			// toolStrip1
			// 
			this.toolStrip1.AllowMerge = false;
			this.toolStrip1.CanOverflow = false;
			resources.ApplyResources(this.toolStrip1, "toolStrip1");
			this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tbBtnHelp,
            this.tbBtnHelpSingle,
            this.tbBtnCopy,
            this.tbBtnAdvanced});
			this.toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.toolStrip1.TabStop = true;
			// 
			// tbBtnHelp
			// 
			this.tbBtnHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.tbBtnHelp, "tbBtnHelp");
			this.tbBtnHelp.Name = "tbBtnHelp";
			this.tbBtnHelp.Click += new System.EventHandler(this.TbBtnHelp_Click);
			// 
			// tbBtnHelpSingle
			// 
			this.tbBtnHelpSingle.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.tbBtnHelpSingle, "tbBtnHelpSingle");
			this.tbBtnHelpSingle.Margin = new System.Windows.Forms.Padding(1, 1, 0, 2);
			this.tbBtnHelpSingle.Name = "tbBtnHelpSingle";
			this.tbBtnHelpSingle.Click += new System.EventHandler(this.TbBtnHelp_Click);
			// 
			// tbBtnCopy
			// 
			this.tbBtnCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.tbBtnCopy, "tbBtnCopy");
			this.tbBtnCopy.Margin = new System.Windows.Forms.Padding(1, 1, 0, 2);
			this.tbBtnCopy.Name = "tbBtnCopy";
			this.tbBtnCopy.Click += new System.EventHandler(this.ItemCopy_Click);
			// 
			// tbBtnAdvanced
			// 
			this.tbBtnAdvanced.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.tbBtnAdvanced, "tbBtnAdvanced");
			this.tbBtnAdvanced.Margin = new System.Windows.Forms.Padding(1, 1, 0, 2);
			this.tbBtnAdvanced.Name = "tbBtnAdvanced";
			this.tbBtnAdvanced.Click += new System.EventHandler(this.ItemShowDetails_Click);
			// 
			// button1
			// 
			resources.ApplyResources(this.button1, "button1");
			this.button1.Name = "button1";
			this.button1.Click += new System.EventHandler(this.Btn_Click);
			// 
			// button2
			// 
			resources.ApplyResources(this.button2, "button2");
			this.button2.Name = "button2";
			this.button2.Click += new System.EventHandler(this.Btn_Click);
			// 
			// button3
			// 
			resources.ApplyResources(this.button3, "button3");
			this.button3.Name = "button3";
			this.button3.Click += new System.EventHandler(this.Btn_Click);
			// 
			// button4
			// 
			resources.ApplyResources(this.button4, "button4");
			this.button4.Name = "button4";
			this.button4.Tag = "z";
			this.button4.Click += new System.EventHandler(this.Btn_Click);
			// 
			// button5
			// 
			resources.ApplyResources(this.button5, "button5");
			this.button5.Name = "button5";
			this.button5.Click += new System.EventHandler(this.Btn_Click);
			// 
			// imgIcons
			// 
			this.imgIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgIcons.ImageStream")));
			this.imgIcons.TransparentColor = System.Drawing.Color.Transparent;
			this.imgIcons.Images.SetKeyName(0, "indentarrow.bmp");
			this.imgIcons.Images.SetKeyName(1, "indentarrow_right.bmp");
			// 
			// AdvancedMessageBox
			// 
			this.AccessibleRole = System.Windows.Forms.AccessibleRole.Alert;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.pnlForm);
			this.DoubleBuffered = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AdvancedMessageBox";
			this.ShowInTaskbar = false;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AdvancedMessageBox_FormClosing);
			this.Load += new System.EventHandler(this.AdvancedMessageBox_Load);
			this.Click += new System.EventHandler(this.AdvancedMessageBox_Click);
			this.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.AdvancedMessageBox_HelpRequested);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AdvancedMessageBox_KeyDown);
			this.pnlForm.ResumeLayout(false);
			this.pnlForm.PerformLayout();
			this.pnlMessage.ResumeLayout(false);
			this.pnlMessage.PerformLayout();
			this.pnlButtons.ResumeLayout(false);
			this.pnlButtons.PerformLayout();
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion


	private TableLayoutPanel pnlForm;
	private Panel pnlIcon;
	private TableLayoutPanel pnlMessage;
	private LinkLabel lblTopMessage;
	private Label lblAdditionalInfo;
	private TableLayoutPanel pnlAdditional;
	private BlackbirdSql.Sys.Controls.Widgets.WrappingCheckBox chkDontShow;
	private GroupBox grpSeparator;
	private ImageList imgIcons;
	private Button button1;
	private Button button2;
	private Button button3;
	private Button button4;
	private Button button5;
	private TableLayoutPanel pnlButtons;
	private ToolStrip toolStrip1;
	private ToolStripDropDownButton tbBtnHelp;
	private ToolStripButton tbBtnCopy;
	private ToolStripButton tbBtnAdvanced;
	private ToolStripButton tbBtnHelpSingle;


}
