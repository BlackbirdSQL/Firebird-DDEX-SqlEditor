// Microsoft.SqlServer.DlgGrid, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.IDlgStorage

using BlackbirdSql.Common.Controls.Events;
using BlackbirdSql.Common.Model.Interfaces;

// using Microsoft.SqlServer.Management.UI.Grid;




namespace BlackbirdSql.Common.Controls.Interfaces;


// [CLSCompliant(false)]
public interface IBDlgStorage : IBGridStorage
{
	IBMemDataStorage Storage { get; }

	IBStorageView StorageView { get; }

	event FillControlWithDataEventHandler FillControlWithDataEvent;

	event SetCellDataFromControlEventHandler SetCellDataFromControlEvent;
}
