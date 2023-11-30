// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlResultsEditorFactory

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using BlackbirdSql.Common;
using BlackbirdSql.Common.Controls;
using BlackbirdSql.Common.Properties;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Utilities;

using Cmd = BlackbirdSql.Common.Cmd;
using Tracer = BlackbirdSql.Core.Ctl.Diagnostics.Tracer;

namespace BlackbirdSql.EditorExtension.Ctl;


[Guid(LibraryData.SqlResultsEditorFactoryGuid)]
[ProvideMenuResource("Menus.ctmenu", 1)]

[Name("BlackbirdSql Results")]
public sealed class SqlResultsEditorFactory : AbstruseEditorFactory
{
	public override Guid ClsidEditorFactory => new(LibraryData.SqlResultsEditorFactoryGuid);

	public SqlResultsEditorFactory()
		: base(withEncoding: false)
	{
	}

	public override int CreateEditorInstance(uint createFlags, string moniker, string physicalView, IVsHierarchy hierarchy, uint itemId, IntPtr existingDocData, out IntPtr intPtrDocView, out IntPtr intPtrDocData, out string caption, out Guid cmdUIGuid, out int result)
	{
		intPtrDocView = IntPtr.Zero;
		intPtrDocData = IntPtr.Zero;
		caption = "";
		cmdUIGuid = Guid.Empty;
		result = 1;
		Cursor current = Cursor.Current;
		try
		{
			// Tracer.Trace(GetType(), "IVsEditorFactory.CreateEditorInstance", "nCreateFlags = {0}, strMoniker = {1}, strPhysicalView = {2}, itemid = {3}", createFlags, moniker, physicalView, itemId);
			if (!string.IsNullOrEmpty(physicalView))
			{
				// Tracer.Trace(GetType(), "IVsEditorFactory.CreateEditorInstance", "physicalView is null - returning E_INVALIDARG");
				return VSConstants.E_INVALIDARG;
			}

			if ((createFlags & 6) == 0)
			{
				// Tracer.Trace(GetType(), "IVsEditorFactory.CreateEditorInstance", "invalid create flags - returning E_INVALIDARG");
				return VSConstants.E_INVALIDARG;
			}

			if (existingDocData != IntPtr.Zero)
			{
				return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
			}

			result = 0;
			Cursor.Current = Cursors.WaitCursor;
			ResultWindowPane resultWindowPane = CreateResultsWindowPane();
			caption = SharedResx.SqlResultsEditorFactory_Caption;
			intPtrDocView = Marshal.GetIUnknownForObject(resultWindowPane);
			intPtrDocData = Marshal.GetIUnknownForObject(resultWindowPane);
			cmdUIGuid = VSConstants.GUID_TextEditorFactory;
			return VSConstants.S_OK;
		}
		catch (Exception ex)
		{
			if (ex is NullReferenceException || ex is ApplicationException || ex is ArgumentException || ex is InvalidOperationException)
			{
				Cmd.ShowExceptionInDialog(SharedResx.BaseEditorFactory_FailedToCreateEditor, ex);
				return VSConstants.E_FAIL;
			}

			throw;
		}
		finally
		{
			Cursor.Current = current;
		}
	}

	public ResultWindowPane CreateResultsWindowPane()
	{
		return new ResultWindowPane();
	}
}
