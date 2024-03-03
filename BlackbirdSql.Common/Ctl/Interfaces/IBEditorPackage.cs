using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BlackbirdSql.Common.Controls.Interfaces;
using BlackbirdSql.Common.Ctl.Events;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Core.Model;


namespace BlackbirdSql.Common.Ctl.Interfaces;

public interface IBEditorPackage
{
	Dictionary<object, AuxiliaryDocData> AuxiliaryDocDataTable { get; }

	IBSqlEditorWindowPane LastFocusedSqlEditor { get; set; }

	AuxiliaryDocData GetAuxiliaryDocData(object docData);


	public DialogResult ShowExecutionSettingsDialogFrame(AuxiliaryDocData auxDocData,
		FormStartPosition startPosition);

	bool? ShowConnectionDialogFrame(IntPtr parent, EventsChannel channel,
		ConnectionPropertyAgent ci, VerifyConnectionDelegate verifierDelegate,
		ConnectionDialogConfiguration config, ref ConnectionPropertyAgent connectionInfo);

	bool TryGetTabbedEditorService(uint docCookie, bool activateIfOpen, out IBTabbedEditorService tabbedEditorService);
}