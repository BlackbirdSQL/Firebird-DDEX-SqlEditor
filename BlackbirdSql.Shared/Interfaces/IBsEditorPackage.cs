using System.Collections.Generic;
using System.Windows.Forms;
using BlackbirdSql.Shared.Model;



namespace BlackbirdSql.Shared.Interfaces;


public interface IBsEditorPackage
{
	Dictionary<object, AuxilliaryDocData> AuxilliaryDocDataTable { get; }

	IBsTabbedEditorWindowPane LastFocusedSqlEditor { get; set; }

	AuxilliaryDocData GetAuxilliaryDocData(object docData);


	public DialogResult ShowExecutionSettingsDialogFrame(AuxilliaryDocData auxDocData,
		FormStartPosition startPosition);


	bool TryGetTabbedEditorService(uint docCookie, bool activateIfOpen, out IBsWindowPaneServiceProvider tabbedEditorService);
}