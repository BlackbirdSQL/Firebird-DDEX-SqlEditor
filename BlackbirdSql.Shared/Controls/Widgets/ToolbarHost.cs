// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.ToolbarHost

using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;



namespace BlackbirdSql.Shared.Controls.Widgets;


public class ToolbarHost : Panel, IVsToolWindowToolbar
{
	private IVsToolWindowToolbarHost _VsToolbarHost;

	private Guid _ClsidCmdSet;

	private uint _ToolbarMenuId;

	private IVsUIShell4 _UiShell;

	private IOleCommandTarget _CommandTarget;

	private bool _AssociateToolbarOnHandleCreate;


	public IVsToolWindowToolbarHost VsToolbarHost => _VsToolbarHost;



	public ToolbarHost()
	{
		Margin = new Padding(0);
		Dock = DockStyle.Top;
	}

	public void Activate(IntPtr h)
	{
		if (_VsToolbarHost != null && h != IntPtr.Zero)
		{
			Diag.ThrowIfNotOnUIThread();

			___(_VsToolbarHost.ProcessMouseActivation(h, 7u, IntPtr.Zero, IntPtr.Zero));
		}
	}

	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	private static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);

	public void SetToolbar(IVsUIShell4 uiShell, Guid clsidCmdSet, uint menuId, IOleCommandTarget commandTarget)
	{
		_ClsidCmdSet = clsidCmdSet;
		_ToolbarMenuId = menuId;

		try
		{
			_UiShell = uiShell ?? throw new ArgumentNullException("SetToolbar:uiShell");
			_CommandTarget = commandTarget ?? throw new ArgumentNullException("SetToolbar:commandTarget");
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		if (IsHandleCreated)
		{
			InternalAssociateToolbarWithHandle();
		}
		else
		{
			_AssociateToolbarOnHandleCreate = true;
		}
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		if (_AssociateToolbarOnHandleCreate)
		{
			InternalAssociateToolbarWithHandle();
		}
	}

	protected override void OnHandleDestroyed(EventArgs e)
	{
		base.OnHandleDestroyed(e);
		_AssociateToolbarOnHandleCreate = true;
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);
		if (_VsToolbarHost != null)
		{
			Diag.ThrowIfNotOnUIThread();

			___(_VsToolbarHost.BorderChanged());
		}
	}

	protected override void WndProc(ref Message m)
	{
		if (m.Msg == Sys.Native.WM_SETFOCUS && Parent != null)
		{
			Control control = Parent;
			while (control != null && control is not ContainerControl)
			{
				control = control.Parent;
			}
			control ??= Parent;
			if (control != null)
			{
				Sys.Native.SetFocus(control.Handle);
				return;
			}
		}
		base.WndProc(ref m);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_VsToolbarHost = null;
			_CommandTarget = null;
			_UiShell = null;
		}
		base.Dispose(disposing);
	}

	public int GetBorder(RECT[] borders)
	{
		borders[0].left = ClientRectangle.Left;
		borders[0].top = ClientRectangle.Top;
		borders[0].right = ClientRectangle.Right;
		borders[0].bottom = ClientRectangle.Bottom;
		return 0;
	}

	public int SetBorderSpace(RECT[] borders)
	{
		int num2 = Height = borders[0].top - borders[0].bottom;
		MinimumSize = new Size(0, num2);
		return 0;
	}

	private void InternalAssociateToolbarWithHandle()
	{
		try
		{
			if (_UiShell == null)
				throw new NullReferenceException("UiShell is null");

			Diag.ThrowIfNotOnUIThread();

			___(_UiShell.SetupToolbar2(Handle, this, _CommandTarget, out _VsToolbarHost));

			___(_VsToolbarHost.AddToolbar(VSTWT_LOCATION.VSTWT_TOP, ref _ClsidCmdSet, _ToolbarMenuId));

			___(_VsToolbarHost.ShowHideToolbar(ref _ClsidCmdSet, _ToolbarMenuId, 1));

			_AssociateToolbarOnHandleCreate = false;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, $"TOOL Handle: {Handle}   CommandTarget: {_CommandTarget}   ToolbarHost: {_VsToolbarHost}   ClsidCmdSet: {_ClsidCmdSet}   MenuId: {_ToolbarMenuId}");

			throw ex;
		}
	}
}
