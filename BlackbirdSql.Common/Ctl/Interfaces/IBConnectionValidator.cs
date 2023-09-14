// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.Core.IConnectionValidator

using System.Data;
using BlackbirdSql.Core.Ctl.Interfaces;


namespace BlackbirdSql.Common.Ctl.Interfaces;

public interface IBConnectionValidator : IBExportable
{
	void CheckConnection(IDbConnection conn);
}
