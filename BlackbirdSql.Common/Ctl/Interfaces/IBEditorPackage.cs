using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BlackbirdSql.Common.Ctl.Events;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;


namespace BlackbirdSql.Common.Ctl.Interfaces;

public interface IBEditorPackage
{
	Dictionary<object, AuxiliaryDocData> DocDataEditors { get; }

	IBSqlEditorWindowPane LastFocusedSqlEditor { get; set; }

	AuxiliaryDocData GetAuxiliaryDocData(object docData);

	public DialogResult ShowExecutionSettingsDialogFrame(AuxiliaryDocData auxDocData,
		FormStartPosition startPosition);

	bool? ShowConnectionDialogFrame(IntPtr parent, EventsChannel channel,
		UIConnectionInfo ci, VerifyConnectionDelegate verifierDelegate,
		ConnectionDialogConfiguration config, ref UIConnectionInfo uIConnectionInfo);
}