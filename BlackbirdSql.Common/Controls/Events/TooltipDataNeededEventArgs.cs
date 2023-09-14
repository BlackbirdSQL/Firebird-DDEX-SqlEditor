﻿#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Common.Controls.Enums;


// namespace Microsoft.SqlServer.Management.UI.Grid
namespace BlackbirdSql.Common.Controls.Events
{
	public class TooltipDataNeededEventArgs : EventArgs
	{
		private readonly EnHitTestResult m_htResult;

		private readonly long m_rowIndex;

		private readonly int m_colIndex;

		private string m_toolTip;

		public EnHitTestResult HitTest => m_htResult;

		public long RowIndex => m_rowIndex;

		public int ColumnIndex => m_colIndex;

		public string TooltipText
		{
			get
			{
				return m_toolTip;
			}
			set
			{
				m_toolTip = value;
			}
		}

		public TooltipDataNeededEventArgs(EnHitTestResult ht, long rowIndex, int colIndex)
		{
			m_htResult = ht;
			m_rowIndex = rowIndex;
			m_colIndex = colIndex;
		}
	}
}