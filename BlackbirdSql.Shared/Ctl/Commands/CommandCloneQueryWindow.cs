// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorCloneQueryWindowCommand

using System;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Shared.Interfaces;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Utilities;



namespace BlackbirdSql.Shared.Ctl.Commands;


public class CommandCloneQueryWindow : AbstractCommand
{
	public CommandCloneQueryWindow()
	{
	}

	public CommandCloneQueryWindow(IBsTabbedEditorPane tabbedEditor) : base(tabbedEditor)
	{
	}



	protected override int OnQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);

		return VSConstants.S_OK;
	}

	protected override int OnExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
		{

			IBsDesignerExplorerServices service = ApcManager.EnsureService<IBsDesignerExplorerServices>();

			string baseName = TabbedEditor.WindowBaseName;

			TextSpanEx textSpanInfo = TabbedEditor.GetSelectedCodeEditorTextSpan();
			if (textSpanInfo.Text == null || textSpanInfo.Text.Length == 0)
			{
				textSpanInfo = TabbedEditor.GetAllCodeEditorTextSpan();
			}

			service.CloneQuery(CachedStrategy?.CurrentDatasetKey, baseName, textSpanInfo.Text);
		}

		return VSConstants.S_OK;
	}
}
