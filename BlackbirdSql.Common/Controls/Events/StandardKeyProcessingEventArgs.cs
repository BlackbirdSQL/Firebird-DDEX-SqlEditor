#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Windows.Forms;

// namespace Microsoft.SqlServer.Management.UI.Grid
namespace BlackbirdSql.Common.Controls.Events
{
	public class StandardKeyProcessingEventArgs : EventArgs
	{
		private readonly Keys m_Key;

		private readonly Keys m_Modifiers;

		private bool m_ShouldHandle = true;

		public Keys Key => m_Key;

		public Keys Modifiers => m_Modifiers;

		public bool ShouldHandle
		{
			get
			{
				return m_ShouldHandle;
			}
			set
			{
				m_ShouldHandle = value;
			}
		}

		public StandardKeyProcessingEventArgs(KeyEventArgs ke)
		{
			m_Key = ke.KeyCode;
			m_Modifiers = ke.Modifiers;
		}

		protected StandardKeyProcessingEventArgs()
		{
		}
	}
}
