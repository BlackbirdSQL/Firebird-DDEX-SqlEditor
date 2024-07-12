#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using BlackbirdSql.Core.Ctl.CommandProviders;
using BlackbirdSql.Shared.Controls.Dialogs;
using BlackbirdSql.Shared.Controls.Grid;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using Point = System.Drawing.Point;



namespace BlackbirdSql.Shared;


public abstract class VS : BlackbirdSql.VS
{

	// ---------------------------------------------------------------------------------
	#region DataTools Members - VS
	// ---------------------------------------------------------------------------------


	public const string IVsDataConnectionInteropGuid = "902A17C6-B166-485F-A49F-9029549442DD";
	public const string IVsDataConnectionManagerInteropGuid = "E7A0D4E0-D0E4-4AFA-A8A1-DD4636073D98";


	#endregion DataTools Members




	// ---------------------------------------------------------------------------------
	#region SqlEditor Members - VS
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// Visual Studio built-in Sql Editor Guid
	/// </summary>
	public const string SqlEditorFactoryGuid = "cc5d8df0-88f4-4bb2-9dbb-b48cee65c30a";

	/// <summary>
	/// Visual Studio built-in Encoded Sql Editor Guid
	/// </summary>
	public const string SqlEditorEncodedFactoryGuid = "F9D1E5B1-8A59-439C-9BB9-D5598B830ECB";


	public const string VSDebugCommandGuid = "C9DD4A59-47FB-11d2-83E7-00C04F9902C1";

	public static readonly Guid ClsidVSDebugCommand = new Guid(VSDebugCommandGuid);


	#endregion SqlEditor Members




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
		Diag.ThrowIfNotOnUIThread();

		IVsUIShell obj = Package.GetGlobalService(typeof(IVsUIShell)) as IVsUIShell;
		Guid rclsidActive = CommandProperties.ClsidCommandSet;

		_ = Control.MousePosition;
		POINTS pOINTS = new POINTS
		{
			x = (short)xPos,
			y = (short)yPos
		};
		obj.ShowContextMenu(dwReserved, ref rclsidActive, menuId, [pOINTS], commandTarget);
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



	public static IVsWindowFrame GetWindowFrameForDocData(object docData, System.IServiceProvider serviceProvider)
	{
		Diag.ThrowIfNotOnUIThread();

		foreach (IVsWindowFrame frame in GetWindowFramesForDocData(docData, serviceProvider))
			return frame;

		return null;
	}



	public static IEnumerable<IVsWindowFrame> GetWindowFramesForDocData(object docData, System.IServiceProvider serviceProvider)
	{
		Diag.ThrowIfNotOnUIThread();

		if (docData == null)
		{
			ArgumentException ex = new("docData");
			Diag.Dug(ex);
			throw ex;
		}

		___((serviceProvider.GetService(typeof(SVsUIShell)) as IVsUIShell).GetDocumentWindowEnum(out var windowFramesEnum));
		IVsWindowFrame[] windowFrames = new IVsWindowFrame[1];

		while (windowFramesEnum.Next(1u, windowFrames, out uint pceltFetched) == 0 && pceltFetched == 1)
		{
			IVsWindowFrame vsWindowFrame = windowFrames[0];
			___(vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out var pvar));
			if (pvar == docData)
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
		// Tracer.Trace(typeof(CommonUtils), "CommonUtils.GetFileNameUsingSaveDialog", "strFilterString = {0}, strCaption = {1}", strFilterString, strCaption);

		Diag.ThrowIfNotOnUIThread();

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
				___(vsUIShell.GetDialogOwnerHwnd(out array[0].hwndOwner));
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
					// Tracer.Trace(typeof(CommonUtils), Tracer.EnLevel.Information, "CommonUtils.GetFileNameUsingSaveDialog", "file name is {0}", empty);
					return empty;
				}
				catch (Exception e)
				{
					Diag.Dug(e);
					return null;
				}
			}
			catch (Exception e2)
			{
				Diag.Dug(e2);
				MessageCtl.ShowEx(string.Empty, e2);
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

		// Tracer.Trace(typeof(CommonUtils), Tracer.EnLevel.Verbose, "CommonUtils.GetFileNameUsingSaveDialog", "cannot get IVsUIShell!!");
		return null;
	}

	public static StreamWriter GetTextWriterForQueryResultsToFile(bool xmlResults, ref string intialDirectory)
	{
		// Tracer.Trace(typeof(VS), "GetTextWriterForQueryResultsToFile()");

		FileEncodingDialog fileEncodingDlg = new FileEncodingDialog();
		string text = Properties.ControlsResources.SqlExportFromGridFilterTabDelimitted;

		if (xmlResults)
			text = Properties.ControlsResources.SqlXMLFileFilter;


		text = text + "|" + Properties.ControlsResources.SqlExportFromGridFilterAllFiles;
		string fileNameUsingSaveDialog = GetFileNameUsingSaveDialog(MakeVsFilterString(text), Properties.ControlsResources.SaveResults, intialDirectory, fileEncodingDlg);

		if (fileNameUsingSaveDialog != null)
		{
			intialDirectory = Path.GetDirectoryName(fileNameUsingSaveDialog);
			return new StreamWriter(fileNameUsingSaveDialog, append: false, fileEncodingDlg.Encoding, 8192)
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
