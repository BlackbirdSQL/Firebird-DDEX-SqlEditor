// Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.Core.IConnectionMruManager

using System.Collections.Generic;
using BlackbirdSql.Core.Ctl.Enums;


namespace BlackbirdSql.Common.Ctl.Interfaces;

public interface IBConnectionMruManager // : IBExportable
{
	void AddConnection(string connectionString, bool isFavorite, EnEngineType serverEngine);

	void RemoveConnection(string connectionString);

	void UpdateFavorite(string connectionString, bool isFavorite, EnEngineType serverEngine);

	List<MruInfo> GetMruList();
}
