#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;

// namespace Microsoft.SqlServer.Management.UI.Grid
namespace BlackbirdSql.Common.Controls.Events
{
	public class MouseButtonClickedEventArgs : EventArgs
	{
		private readonly long m_RowIndex;

		private readonly int m_ColumnIndex;

		private Rectangle m_CellRect;

		private readonly MouseButtons m_Button;

		private bool m_ShouldRedraw = true;

		public long RowIndex => m_RowIndex;

		public int ColumnIndex => m_ColumnIndex;

		public Rectangle CellRect => m_CellRect;

		public MouseButtons Button => m_Button;

		public bool ShouldRedraw
		{
			get
			{
				return m_ShouldRedraw;
			}
			set
			{
				m_ShouldRedraw = value;
			}
		}

		public MouseButtonClickedEventArgs(long nRowIndex, int nColIndex, Rectangle rCellRect, MouseButtons btn)
		{
			m_RowIndex = nRowIndex;
			m_ColumnIndex = nColIndex;
			m_CellRect = rCellRect;
			m_Button = btn;
		}

		protected MouseButtonClickedEventArgs()
		{
		}
	}
}
