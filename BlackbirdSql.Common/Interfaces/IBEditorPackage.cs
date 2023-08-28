using BlackbirdSql.Core.Model;
using BlackbirdSql.Common.Ctl;
using System.Data;
using System;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Common.Events;
using System.Collections.Generic;

namespace BlackbirdSql.Common.Interfaces;


public interface IBEditorPackage
{
	Dictionary<object, AuxiliaryDocData> DocDataEditors { get; }

	IBLanguageService LanguageService { get; }

	ISqlEditorWindowPane LastFocusedSqlEditor { get; set; }

	AuxiliaryDocData GetAuxiliaryDocData(object docData);

	bool? ShowConnectionDialogFrame(IntPtr parent, IBDependencyManager dependencyManager, EventsChannel channel,
		UIConnectionInfo ci, VerifyConnectionDelegate verifierDelegate, ConnectionDialogConfiguration config, ref UIConnectionInfo uIConnectionInfo);
}