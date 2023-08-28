#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Windows.Forms;

// namespace Microsoft.SqlServer.Management.UI.Grid
namespace BlackbirdSql.Common.Controls.Events
{
	public class KeyPressedOnCellEventArgs : EventArgs
	{
		private readonly long m_RowIndex;

		private readonly int m_ColumnIndex;

		private readonly Keys m_Key;

		private readonly Keys m_Modifiers;

		public long RowIndex => m_RowIndex;

		public int ColumnIndex => m_ColumnIndex;

		public Keys Key => m_Key;

		public Keys Modifiers => m_Modifiers;

		public KeyPressedOnCellEventArgs(long nCurRow, int nCurCol, Keys k, Keys m)
		{
			m_RowIndex = nCurRow;
			m_ColumnIndex = nCurCol;
			m_Key = k;
			m_Modifiers = m;
		}

		protected KeyPressedOnCellEventArgs()
		{
		}
	}
}
