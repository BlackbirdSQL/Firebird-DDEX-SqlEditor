// Microsoft.Data.ConnectionUI.Dialog, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.ConnectionUI.AddPropertyDlg
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.VisualStudio.Ddex.Controls.Enums;
using BlackbirdSql.VisualStudio.Ddex.Controls.Events;


namespace BlackbirdSql.VisualStudio.Ddex.Controls.DataTools;

public partial class TAddPropertyDlg : Form
{
	private readonly TDataConnectionDlg _mainDialog;


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

}
