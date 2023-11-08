#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Data;
using System.Windows;
using System.Windows.Forms;

using BlackbirdSql.Common.Controls;
using BlackbirdSql.Common.Controls.Grid;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using Point = System.Drawing.Point;


namespace BlackbirdSql.Common.Ctl;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread",
	Justification = "UIThread compliance is performed by applicable methods.")]

public static class CommonUtils
{

	public static void BindProperty(FrameworkElement element, DependencyProperty property, string bindToProperty, BindingMode mode)
	{
		System.Windows.Data.Binding binding = new(bindToProperty)
		{
			Mode = mode
		};
		element.SetBinding(property, binding);
	}



	public static void ShowContextMenuEvent(int menuId, int xPos, int yPos, IOleCommandTarget commandTarget)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException ex = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(ex);
			throw ex;
		}

		IVsUIShell obj = Package.GetGlobalService(typeof(IVsUIShell)) as IVsUIShell;
		Guid rclsidActive = LibraryData.CLSID_CommandSet;

		_ = Control.MousePosition;
		POINTS pOINTS = new POINTS
		{
			x = (short)xPos,
			y = (short)yPos
		};
		obj.ShowContextMenu(VS.dwReserved, ref rclsidActive, menuId, new POINTS[1] { pOINTS }, commandTarget);
	}



	public static int DefaultMaxCharsPerColumnForGrid => 43679;

	public static int DefaultInitialMaxCharsPerColumnForGrid => 50;

	public static int DefaultInitialMinNumberOfVisibleRows => 8;

	public static bool GetCoordinatesForPopupMenuFromWM_Context(ref Message m, out int x, out int y, Control c)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		x = (short)(int)m.LParam;
		y = (int)m.LParam >> 16;
		if (c == null)
		{
			return false;
		}

		Point pt = c.PointToClient(new Point(x, y));
		if ((int)m.LParam == -1)
		{
			if (c is GridControl val)
			{
				val.GetCurrentCell(out long num, out int num2);
				Rectangle visibleCellRectangle = val.GetVisibleCellRectangle(num, num2);
				pt = visibleCellRectangle.IsEmpty || !c.ClientRectangle.Contains(visibleCellRectangle.Left, visibleCellRectangle.Top) ? c.PointToScreen(new Point(0, val.HeaderHeight)) : c.PointToScreen(new Point(visibleCellRectangle.Left, visibleCellRectangle.Top));
				x = pt.X;
				y = pt.Y;
			}
			else
			{
				pt = c.PointToScreen(new Point(c.ClientSize.Width / 2, c.ClientSize.Height / 2));
				x = pt.X;
				y = pt.Y;
			}

			pt = c.PointToClient(new Point(x, y));
		}

		return c.ClientRectangle.Contains(pt);
	}

	public static IEnumerable<IVsWindowFrame> GetWindowFramesForDocData(object existingDocData, System.IServiceProvider serviceProvider)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException ex = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(ex);
			throw ex;
		}

		if (existingDocData == null)
		{
			ArgumentException ex = new("docData");
			Diag.Dug(ex);
			throw ex;
		}

		ErrorHandler.ThrowOnFailure((serviceProvider.GetService(typeof(SVsUIShell)) as IVsUIShell).GetDocumentWindowEnum(out var windowFramesEnum));
		IVsWindowFrame[] windowFrames = new IVsWindowFrame[1];

		while (windowFramesEnum.Next(1u, windowFrames, out uint pceltFetched) == 0 && pceltFetched == 1)
		{
			IVsWindowFrame vsWindowFrame = windowFrames[0];
			ErrorHandler.ThrowOnFailure(vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out var pvar));
			if (pvar == existingDocData)
			{
				yield return vsWindowFrame;
			}
		}
	}

	public static string MakeVsFilterString(string s)
	{
		if (s == null)
		{
			return null;
		}

		int length = s.Length;
		char[] array = new char[length + 1];
		s.CopyTo(0, array, 0, length);
		for (int i = 0; i < length; i++)
		{
			if (array[i] == '|')
			{
				array[i] = '\0';
			}
		}

		return new string(array);
	}

	public static string GetFileNameUsingSaveDialog(string strFilterString, string strCaption, string initialDir, IVsSaveOptionsDlg optionsDlg)
	{
		return GetFileNameUsingSaveDialog(strFilterString, strCaption, initialDir, optionsDlg, out _);
	}

	public static string GetFileNameUsingSaveDialog(string strFilterString, string strCaption, string initialDir, IVsSaveOptionsDlg optionsDlg, out int filterIndex)
	{
		Tracer.Trace(typeof(CommonUtils), "CommonUtils.GetFileNameUsingSaveDialog", "strFilterString = {0}, strCaption = {1}", strFilterString, strCaption);

		if (!ThreadHelper.CheckAccess())
		{
			COMException ex = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(ex);
			throw ex;
		}

		filterIndex = 0;
		if (Package.GetGlobalService(typeof(SVsUIShell)) is IVsUIShell vsUIShell)
		{
			int num = 512;
			IntPtr intPtr = IntPtr.Zero;
			VSSAVEFILENAMEW[] array = new VSSAVEFILENAMEW[1];
			try
			{
				string empty = string.Empty;
				char[] array2 = new char[num];
				intPtr = Marshal.AllocCoTaskMem(array2.Length * 2);
				Marshal.Copy(array2, 0, intPtr, array2.Length);
				array[0].lStructSize = (uint)Marshal.SizeOf(typeof(VSSAVEFILENAMEW));
				Native.ThrowOnFailure(vsUIShell.GetDialogOwnerHwnd(out array[0].hwndOwner), (string)null);
				array[0].pwzFilter = strFilterString;
				array[0].pwzFileName = intPtr;
				array[0].nMaxFileName = (uint)num;
				array[0].pwzDlgTitle = strCaption;
				array[0].dwFlags = (uint)__VSRDTSAVEOPTIONS.RDTSAVEOPT_ForceSave;
				array[0].pSaveOpts = optionsDlg;
				if (initialDir != null && initialDir.Length != 0)
				{
					array[0].pwzInitialDir = initialDir;
				}
				else
				{
					try
					{
						array[0].pwzInitialDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
					}
					catch
					{
					}
				}

				try
				{
					if (vsUIShell.GetSaveFileNameViaDlg(array) != 0)
					{
						return null;
					}

					filterIndex = (int)array[0].nFilterIndex;
					Marshal.Copy(intPtr, array2, 0, array2.Length);
					int i;
					for (i = 0; i < array2.Length && array2[i] != 0; i++)
					{
					}

					empty = new string(array2, 0, i);
					Tracer.Trace(typeof(CommonUtils), Tracer.EnLevel.Information, "CommonUtils.GetFileNameUsingSaveDialog", "file name is {0}", empty);
					return empty;
				}
				catch (Exception e)
				{
					Tracer.LogExCatch(typeof(CommonUtils), e);
					return null;
				}
			}
			catch (Exception e2)
			{
				Tracer.LogExCatch(typeof(CommonUtils), e2);
				Cmd.ShowExceptionInDialog(string.Empty, e2);
				return null;
			}
			finally
			{
				if (intPtr != IntPtr.Zero)
				{
					Marshal.FreeCoTaskMem(intPtr);
				}
			}
		}

		Tracer.Trace(typeof(CommonUtils), Tracer.EnLevel.Verbose, "CommonUtils.GetFileNameUsingSaveDialog", "cannot get IVsUIShell!!");
		return null;
	}

	public static StreamWriter GetTextWriterForQueryResultsToFile(bool xmlResults, ref string intialDirectory)
	{
		Tracer.Trace(typeof(CommonUtils), "CommonUtils.GetTextWriterForQueryResultsToFile", "", null);
		FileEncodingDlg fileEncodingDialog = new FileEncodingDlg();
		string text = Properties.ControlsResources.SqlExportFromGridFilterTabDelimitted;
		if (xmlResults)
		{
			text = Properties.ControlsResources.SqlXMLFileFilter;
		}

		text = text + "|" + Properties.ControlsResources.SqlExportFromGridFilterAllFiles;
		string fileNameUsingSaveDialog = GetFileNameUsingSaveDialog(MakeVsFilterString(text), Properties.ControlsResources.SaveResults, intialDirectory, fileEncodingDialog);
		if (fileNameUsingSaveDialog != null)
		{
			intialDirectory = Path.GetDirectoryName(fileNameUsingSaveDialog);
			return new StreamWriter(fileNameUsingSaveDialog, append: false, fileEncodingDialog.Encoding, 8192)
			{
				AutoFlush = false
			};
		}

		return null;
	}

	public static int GetExtraSizeForBorderStyle(BorderStyle border)
	{
		return border switch
		{
			BorderStyle.Fixed3D => SystemInformation.Border3DSize.Height * 2,
			BorderStyle.FixedSingle => SystemInformation.BorderSize.Height * 2,
			BorderStyle.None => 0,
			_ => 0,
		};
	}
}
