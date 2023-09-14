using System;
using System.Collections.Generic;

using BlackbirdSql.Common.Ctl.Events;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;


namespace BlackbirdSql.Common.Ctl.Interfaces;

public interface IBEditorPackage
{
	Dictionary<object, AuxiliaryDocData> DocDataEditors { get; }

	IBLanguageService LanguageService { get; }

	IBSqlEditorWindowPane LastFocusedSqlEditor { get; set; }

	AuxiliaryDocData GetAuxiliaryDocData(object docData);

	bool? ShowConnectionDialogFrame(IntPtr parent, IBDependencyManager dependencyManager, EventsChannel channel,
		UIConnectionInfo ci, VerifyConnectionDelegate verifierDelegate, ConnectionDialogConfiguration config, ref UIConnectionInfo uIConnectionInfo);
}