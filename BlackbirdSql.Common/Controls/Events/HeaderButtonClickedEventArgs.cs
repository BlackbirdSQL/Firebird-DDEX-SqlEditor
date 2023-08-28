#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Windows.Forms;
using BlackbirdSql.Common.Controls.Enums;

// namespace Microsoft.SqlServer.Management.UI.Grid
namespace BlackbirdSql.Common.Controls.Events
{
	public class HeaderButtonClickedEventArgs : EventArgs
	{
		private readonly int m_ColumnIndex;

		private readonly MouseButtons m_Button;

		private bool m_RepaintWholeGrid;

		private readonly EnGridButtonArea m_headerArea;

		public int ColumnIndex => m_ColumnIndex;

		public MouseButtons Button => m_Button;

		public EnGridButtonArea HeaderArea => m_headerArea;

		public bool RepaintWholeGrid
		{
			get
			{
				return m_RepaintWholeGrid;
			}
			set
			{
				m_RepaintWholeGrid = value;
			}
		}

		public HeaderButtonClickedEventArgs(int nColIndex, MouseButtons btn, EnGridButtonArea headerArea)
		{
			m_ColumnIndex = nColIndex;
			m_Button = btn;
			m_headerArea = headerArea;
		}

		protected HeaderButtonClickedEventArgs()
		{
		}
	}
}
