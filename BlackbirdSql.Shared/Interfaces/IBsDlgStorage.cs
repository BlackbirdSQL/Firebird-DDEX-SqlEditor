// Microsoft.SqlServer.DlgGrid, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.IDlgStorage

using BlackbirdSql.Shared.Events;

// using Microsoft.SqlServer.Management.UI.Grid;




namespace BlackbirdSql.Shared.Interfaces;


// [CLSCompliant(false)]
internal interface IBsDlgStorage : IBsGridStorage
{
	IBsMemDataStorage Storage { get; }

	IBsStorageView StorageView { get; }

	event FillControlWithDataEventHandler FillControlWithDataEvent;

	event SetCellDataFromControlEventHandler SetCellDataFromControlEvent;
}
