// Microsoft.Data.ConnectionUI.Dialog, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.ConnectionUI.AddPropertyDlg
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.VisualStudio.Ddex.Controls.Enums;
using BlackbirdSql.VisualStudio.Ddex.Controls.Events;


namespace BlackbirdSql.VisualStudio.Ddex.Controls.DataTools;

internal class TAddPropertyDlg : Form
{
	private readonly TDataConnectionDlg _mainDialog;

	private readonly IContainer components;

	private Label propertyLabel;

	private TextBox propertyTextBox;

	private TableLayoutPanel buttonsTableLayoutPanel;

	private Button okButton;

	private Button cancelButton;

	public string PropertyName => propertyTextBox.Text;

	public TAddPropertyDlg()
	{
		InitializeComponent();
		components ??= new Container();
		components.Add(new UserPreferenceChangedHandler(this));
	}

	public TAddPropertyDlg(TDataConnectionDlg mainDialog)
		: this()
	{
		_mainDialog = mainDialog;
		_ = _mainDialog;
	}

	protected override void OnFontChanged(EventArgs e)
	{
		base.OnFontChanged(e);
		propertyTextBox.Width = buttonsTableLayoutPanel.Right - propertyTextBox.Left;
		int num = base.Padding.Left + buttonsTableLayoutPanel.Margin.Left + buttonsTableLayoutPanel.Width + buttonsTableLayoutPanel.Margin.Right + base.Padding.Right;
		if (base.ClientSize.Width < num)
		{
			base.ClientSize = new Size(num, base.ClientSize.Height);
		}
	}

	protected override void OnHelpRequested(HelpEventArgs hevent)
	{
		Control control = null; //  HelpUtils.GetActiveControl(this);
		EnDataConnectionDlgContext context = EnDataConnectionDlgContext.AddProperty;
		if (control == propertyTextBox)
		{
			context = EnDataConnectionDlgContext.AddPropertyTextBox;
		}
		if (control == okButton)
		{
			context = EnDataConnectionDlgContext.AddPropertyOkButton;
		}
		if (control == cancelButton)
		{
			context = EnDataConnectionDlgContext.AddPropertyCancelButton;
		}
		ContextHelpEventArgs contextHelpEventArgs = new ContextHelpEventArgs(context, hevent.MousePos);
		_mainDialog.OnContextHelpRequested(contextHelpEventArgs);
		hevent.Handled = contextHelpEventArgs.Handled;
		if (!contextHelpEventArgs.Handled)
		{
			base.OnHelpRequested(hevent);
		}
	}


	protected override void WndProc(ref Message m)
	{
		/*
		if (_mainDialog.TranslateHelpButton && HelpUtils.IsContextHelpMessage(ref m))
		{
			HelpUtils.TranslateContextHelpMessage(this, ref m);
		}
		*/
		base.WndProc(ref m);
	}

	private void SetOkButtonStatus(object sender, EventArgs e)
	{
		okButton.Enabled = propertyTextBox.Text.Trim().Length > 0;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BlackbirdSql.VisualStudio.Ddex.Controls.DataTools.TAddPropertyDlg));
		this.propertyLabel = new System.Windows.Forms.Label();
		this.propertyTextBox = new System.Windows.Forms.TextBox();
		this.buttonsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
		this.okButton = new System.Windows.Forms.Button();
		this.cancelButton = new System.Windows.Forms.Button();
		this.buttonsTableLayoutPanel.SuspendLayout();
		base.SuspendLayout();
		resources.ApplyResources(this.propertyLabel, "propertyLabel");
		this.propertyLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.propertyLabel.Name = "propertyLabel";
		resources.ApplyResources(this.propertyTextBox, "propertyTextBox");
		this.propertyTextBox.Name = "propertyTextBox";
		this.propertyTextBox.TextChanged += new System.EventHandler(SetOkButtonStatus);
		resources.ApplyResources(this.buttonsTableLayoutPanel, "buttonsTableLayoutPanel");
		this.buttonsTableLayoutPanel.Controls.Add(this.okButton, 0, 0);
		this.buttonsTableLayoutPanel.Controls.Add(this.cancelButton, 1, 0);
		this.buttonsTableLayoutPanel.Name = "buttonsTableLayoutPanel";
		resources.ApplyResources(this.okButton, "okButton");
		this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.okButton.MinimumSize = new System.Drawing.Size(75, 23);
		this.okButton.Name = "okButton";
		resources.ApplyResources(this.cancelButton, "cancelButton");
		this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.cancelButton.MinimumSize = new System.Drawing.Size(75, 23);
		this.cancelButton.Name = "cancelButton";
		base.AcceptButton = this.okButton;
		resources.ApplyResources(this, "$this");
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.cancelButton;
		base.Controls.Add(this.buttonsTableLayoutPanel);
		base.Controls.Add(this.propertyTextBox);
		base.Controls.Add(this.propertyLabel);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.HelpButton = true;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "AddPropertyDlg";
		base.ShowInTaskbar = false;
		this.buttonsTableLayoutPanel.ResumeLayout(false);
		this.buttonsTableLayoutPanel.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
