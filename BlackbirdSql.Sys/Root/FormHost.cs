// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Common.FormUtilities

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;



namespace BlackbirdSql.Sys;


public static class FormHost
{

	private class WindowWithFocusRestorer : IDisposable
	{
		private readonly IntPtr _hwndFocus;

		private bool _isRestored;

		public WindowWithFocusRestorer()
		{
			_hwndFocus = Native.GetFocus();
		}

		~WindowWithFocusRestorer()
		{
			RestoreFocus();
		}

		public void Dispose()
		{
			RestoreFocus();
			GC.SuppressFinalize(this);
		}

		private void RestoreFocus()
		{
			if (!_isRestored)
			{
				_isRestored = true;
				if (_hwndFocus != IntPtr.Zero && Native.IsWindow(_hwndFocus))
				{
					Native.SetFocus(_hwndFocus);
				}
				GC.KeepAlive(this);
			}
		}
	}

	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	private static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);


	public static DialogResult ShowDialog(Form form)
	{
		return ShowDialogOrForm(form, null);
	}

	public static DialogResult ShowDialog(CommonDialog dialog)
	{
		return ShowDialogOrForm(null, dialog);
	}

	[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Using Diag.ThrowIfNotOnUIThread().")]
	private static DialogResult ShowDialogOrForm(Form form, CommonDialog dialog)
	{
		Diag.ThrowIfNotOnUIThread();

		if (form != null && dialog != null)
		{
			throw new ArgumentException("one of either form or dialog needs to be null!");
		}
		if (form == null && dialog == null)
		{
			throw new ArgumentException("one of form or dialog needs to be not null!");
		}
		IVsUIShell vsUIShell = null;
		NativeWindow nativeWindow = new NativeWindow();
		try
		{
			vsUIShell = Package.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell;
			if (vsUIShell != null)
			{
				___(vsUIShell.EnableModeless(0));
				___(vsUIShell.GetDialogOwnerHwnd(out var phwnd));
				if (phwnd != (IntPtr)0)
				{
					nativeWindow.AssignHandle(phwnd);
				}
			}
			if (form != null)
			{
				return ShowDialog(form, nativeWindow);
			}
			return ShowDialog(dialog, nativeWindow);
		}
		finally
		{
			if (vsUIShell != null)
			{
				___(vsUIShell.EnableModeless(1));
			}
		}
	}

	public static DialogResult ShowDialog(Form form, IWin32Window owner)
	{
		if (form == null)
		{
			throw new ArgumentNullException("form");
		}
		using (GetFocusWindowRestorer())
		{
			form.Font = GetDialogFont();
			return form.ShowDialog(owner);
		}
	}

	public static DialogResult ShowDialog(CommonDialog commonDialog, IWin32Window owner)
	{
		if (commonDialog == null)
		{
			throw new ArgumentNullException("commonDialog");
		}
		using (GetFocusWindowRestorer())
		{
			return commonDialog.ShowDialog(owner);
		}
	}

	public static IDisposable GetFocusWindowRestorer()
	{
		return new WindowWithFocusRestorer();
	}

	public static Font GetDialogFont()
	{
		Font result = Control.DefaultFont;
		if (Package.GetGlobalService(typeof(IUIService)) is IUIService iUIService)
		{
			result = iUIService.Styles["DialogFont"] as Font;
		}
		return result;
	}

	public static Control GetFocusControl()
	{
		Control result = null;
		IntPtr focus = Native.GetFocus();
		if (focus != IntPtr.Zero)
		{
			result = Control.FromHandle(focus);
		}
		return result;
	}

	public static void SetDataGridHeaderHeight(DataGridView dataGrid)
	{
		float num = 0f;
		using (Graphics graphics = dataGrid.CreateGraphics())
		{
			for (int i = 0; i < dataGrid.ColumnCount; i++)
			{
				float height = graphics.MeasureString(dataGrid.Columns[i].HeaderText, dataGrid.ColumnHeadersDefaultCellStyle.Font).Height;
				if (height > num)
				{
					num = height;
				}
			}
		}
		dataGrid.ColumnHeadersHeight = (int)num + 10;
	}

	public static void GetAllChildren(Control parent, List<Control> allChildren)
	{
		GetAllChildren(parent, parent, allChildren);
	}

	private static void GetAllChildren(Control toplevelParent, Control parent, List<Control> allChildren)
	{
		if (!parent.HasChildren && (parent.TabStop || parent is RadioButton))
		{
			allChildren.Add(parent);
			return;
		}
		foreach (object control in parent.Controls)
		{
			if (control is Control parent2)
			{
				GetAllChildren(toplevelParent, parent2, allChildren);
			}
		}
		if (toplevelParent != parent)
		{
			return;
		}
		allChildren.Sort(delegate (Control ctrl1, Control ctrl2)
		{
			Point relativeToParent = GetRelativeToParent(toplevelParent, ctrl1, new Point(0, 0));
			Point relativeToParent2 = GetRelativeToParent(toplevelParent, ctrl2, new Point(0, 0));
			if (relativeToParent.Y == relativeToParent2.Y)
			{
				return relativeToParent.X - relativeToParent2.X;
			}
			Rectangle rectangle = new Rectangle(new Point(0, relativeToParent.Y), ctrl1.Size);
			Rectangle rect = new Rectangle(new Point(0, relativeToParent2.Y), ctrl2.Size);
			return rectangle.IntersectsWith(rect) ? relativeToParent.X - relativeToParent2.X : relativeToParent.Y - relativeToParent2.Y;
		});
	}

	private static Point GetRelativeToParent(Control toplevelParent, Control ctrl, Point startPt)
	{
		startPt.X += ctrl.Location.X;
		startPt.Y += ctrl.Location.Y;
		if (ctrl.Parent != toplevelParent)
		{
			startPt = GetRelativeToParent(toplevelParent, ctrl.Parent, startPt);
		}
		return startPt;
	}
}
