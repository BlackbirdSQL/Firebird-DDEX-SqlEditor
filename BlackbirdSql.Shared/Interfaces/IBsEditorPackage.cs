using System.Collections.Generic;
using System.Windows.Forms;
using BlackbirdSql.Shared.Model;



namespace BlackbirdSql.Shared.Interfaces;


public interface IBsEditorPackage
{
	Dictionary<object, AuxilliaryDocData> AuxDocDataTable { get; }

	IBsTabbedEditorPane CurrentTabbedEditor { get; set; }

	AuxilliaryDocData GetAuxilliaryDocData(object docData);


	internal DialogResult ShowExecutionSettingsDialog(AuxilliaryDocData auxDocData,
		FormStartPosition startPosition);
}