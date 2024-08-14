using System.Collections.Generic;
using System.Windows.Forms;
using BlackbirdSql.Shared.Model;



namespace BlackbirdSql.Shared.Interfaces;


public interface IBsEditorPackage
{
	Dictionary<object, AuxilliaryDocData> AuxDocDataTable { get; }

	IBsTabbedEditorPane LastFocusedSqlEditor { get; set; }

	AuxilliaryDocData GetAuxilliaryDocData(object docData);


	public DialogResult ShowExecutionSettingsDialog(AuxilliaryDocData auxDocData,
		FormStartPosition startPosition);


	bool TryGetTabbedEditorService(uint docCookie, bool activateIfOpen, out IBsEditorPaneServiceProvider tabbedEditorService);
}