#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Common.Ctl;

// namespace Microsoft.SqlServer.Management.UI.Grid
namespace BlackbirdSql.Common.Controls.Events
{
	public class SelectionChangedEventArgs : EventArgs
	{
		private readonly BlockOfCellsCollection m_SelectedBlocks;

		public BlockOfCellsCollection SelectedBlocks => m_SelectedBlocks;

		public SelectionChangedEventArgs(BlockOfCellsCollection blocks)
		{
			m_SelectedBlocks = blocks;
		}

		protected SelectionChangedEventArgs()
		{
		}
	}
}
