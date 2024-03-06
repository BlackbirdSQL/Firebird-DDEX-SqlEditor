// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.DefaultMessageBoxProvider
using System;
using System.Windows.Forms;
using BlackbirdSql.Common.Ctl.Enums;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Properties;

namespace BlackbirdSql.Common.Ctl.Dialogs;

public class DefaultMessageBoxProvider : IBMessageBoxProvider
{
	private delegate DialogResult PopupExceptionMessageDelegate(Exception e, string caption, EnExceptionMessageBoxButtons buttons, EnExceptionMessageBoxSymbol symbol, IWin32Window owner);

	private delegate DialogResult PopupMessageDelegate(string text, string caption, EnExceptionMessageBoxButtons buttons, EnExceptionMessageBoxSymbol symbol, IWin32Window owner);

	private Control marshallingControl;

	public Control MarshallingControl
	{
		get
		{
			return marshallingControl;
		}
		set
		{
			marshallingControl = value ?? throw new ArgumentNullException("value");
		}
	}

	protected DefaultMessageBoxProvider()
	{
	}

	public DefaultMessageBoxProvider(Control marshallingControl)
	{
		this.marshallingControl = marshallingControl ?? throw new ArgumentNullException("marshallingControl");
	}

	private IWin32Window RealMessageBoxOwner(IWin32Window userSpecifiedOwner)
	{
		if (userSpecifiedOwner is Control control && control.IsHandleCreated)
		{
			return userSpecifiedOwner;
		}
		if (marshallingControl.IsHandleCreated)
		{
			return marshallingControl;
		}
		return userSpecifiedOwner;
	}

	protected virtual DialogResult ShowExceptionBasedMessageInternal(Exception e, string caption, EnExceptionMessageBoxButtons buttons, EnExceptionMessageBoxSymbol symbol, IWin32Window owner)
	{
		ExceptionMessageBoxCtl exceptionMessageBox = new ExceptionMessageBoxCtl(e);
		if (caption != null && caption.Length > 0)
		{
			exceptionMessageBox.Caption = caption;
		}
		else
		{
			exceptionMessageBox.Caption = ControlsResources.DefaultMessageBoxCaption;
		}
		exceptionMessageBox.Symbol = symbol;
		exceptionMessageBox.Options = EnExceptionMessageBoxOptions.RightAlign;
		exceptionMessageBox.Buttons = buttons;
		if (buttons == EnExceptionMessageBoxButtons.YesNo)
		{
			exceptionMessageBox.DefaultButton = EnExceptionMessageBoxDefaultButton.Button2;
		}
		return exceptionMessageBox.Show(RealMessageBoxOwner(owner));
	}

	protected virtual DialogResult ShowMessageInternal(string text, string caption, EnExceptionMessageBoxButtons buttons, EnExceptionMessageBoxSymbol symbol, IWin32Window owner)
	{
		ExceptionMessageBoxCtl exceptionMessageBox = new()
		{
			Message = new(text)
		};
		if (caption != null && caption.Length > 0)
		{
			exceptionMessageBox.Caption = caption;
		}
		else
		{
			exceptionMessageBox.Caption = ControlsResources.DefaultMessageBoxCaption;
		}
		exceptionMessageBox.Symbol = symbol;
		exceptionMessageBox.Options = EnExceptionMessageBoxOptions.RightAlign;
		exceptionMessageBox.Buttons = buttons;
		if (buttons == EnExceptionMessageBoxButtons.YesNo)
		{
			exceptionMessageBox.DefaultButton = EnExceptionMessageBoxDefaultButton.Button2;
		}
		return exceptionMessageBox.Show(RealMessageBoxOwner(owner));
	}

	DialogResult IBMessageBoxProvider.ShowMessage(Exception e, string caption, EnExceptionMessageBoxButtons buttons, EnExceptionMessageBoxSymbol symbol, IWin32Window owner)
	{
		if (marshallingControl.InvokeRequired)
		{
			return (DialogResult)marshallingControl.Invoke(new PopupExceptionMessageDelegate(ShowExceptionBasedMessageInternal), e, caption, buttons, symbol, owner);
		}
		return ShowExceptionBasedMessageInternal(e, caption, buttons, symbol, owner);
	}

	DialogResult IBMessageBoxProvider.ShowMessage(string text, string caption, EnExceptionMessageBoxButtons buttons, EnExceptionMessageBoxSymbol symbol, IWin32Window owner)
	{
		if (marshallingControl.InvokeRequired)
		{
			return (DialogResult)marshallingControl.Invoke(new PopupMessageDelegate(ShowMessageInternal), text, caption, buttons, symbol, owner);
		}
		return ShowMessageInternal(text, caption, buttons, symbol, owner);
	}
}
