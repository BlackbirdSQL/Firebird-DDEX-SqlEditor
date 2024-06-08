using System.Collections.Generic;
using System.Windows.Forms;
using BlackbirdSql.Shared.Model;



namespace BlackbirdSql.Shared.Interfaces;


public interface IBEditorPackage
{
	Dictionary<object, AuxilliaryDocData> AuxilliaryDocDataTable { get; }

	IBSqlEditorWindowPane LastFocusedSqlEditor { get; set; }

	AuxilliaryDocData GetAuxilliaryDocData(object docData);


	public DialogResult ShowExecutionSettingsDialogFrame(AuxilliaryDocData auxDocData,
		FormStartPosition startPosition);


	bool TryGetTabbedEditorService(uint docCookie, bool activateIfOpen, out IBTabbedEditorService tabbedEditorService);
}