// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorToolbarCommandHandler<T>

using System;

using BlackbirdSql.Common.Commands;
using BlackbirdSql.Common.Controls;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Interfaces;

using Microsoft.VisualStudio.OLE.Interop;

// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration
namespace BlackbirdSql.EditorExtension
{
	public sealed class SqlEditorToolbarCommandHandler<T> : ITabbedEditorToolbarCommandHandler where T : AbstractSqlEditorCommand, new()
	{
		private readonly GuidId _GuidId;

		public GuidId GuidId => _GuidId;

		public SqlEditorToolbarCommandHandler(Guid clsidCmdSet, uint cmdId)
		{
			_GuidId = new GuidId(clsidCmdSet, cmdId);
		}

		public int HandleQueryStatus(AbstractTabbedEditorPane editorPane, ref OLECMD prgCmd, IntPtr pCmdText)
		{
			if (editorPane is SqlEditorTabbedEditorPane sqlEditorTabbedEditorPane)
			{
				return new T
				{
					Editor = sqlEditorTabbedEditorPane
				}.QueryStatus(ref prgCmd, pCmdText);
			}

			return (int)Constants.MSOCMDERR_E_NOTSUPPORTED;
		}

		public int HandleExec(AbstractTabbedEditorPane editorPane, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
		{
			if (editorPane is SqlEditorTabbedEditorPane sqlEditorTabbedEditorPane)
			{
				return new T
				{
					Editor = sqlEditorTabbedEditorPane
				}.Exec(nCmdexecopt, pvaIn, pvaOut);
			}

			return (int)Constants.MSOCMDERR_E_NOTSUPPORTED;
		}
	}
}
