#region Assembly Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLDB\Microsoft.SqlServer.ConnectionDlg.Core.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using BlackbirdSql.Core.Interfaces;

namespace BlackbirdSql.Common.Ctl;


public class MruInfo
{
	public IBPropertyAgent ConnectionInfo { get; private set; }

	public bool IsFavorite { get; private set; }

	public IBServerDefinition ServerDefinition { get; private set; }

	public string PropertyString => ConnectionInfo != null ? ConnectionInfo.PropertyString : "";

	public MruInfo(IBPropertyAgent ci, bool isFavorite, IBServerDefinition serverDefinition)
	{
		ConnectionInfo = ci.Copy();
		IsFavorite = isFavorite;
		ServerDefinition = serverDefinition;
	}
}
