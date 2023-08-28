#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System.Windows.Forms;

// namespace Microsoft.SqlServer.Management.UI.Grid
namespace BlackbirdSql.Common.Controls.Interfaces
{
	public interface IGridEmbeddedControlManagement2 : IGridEmbeddedControlManagement
	{
		void ReceiveKeyboardEvent(KeyEventArgs ke);

		void ReceiveChar(char c);

		void PostProcessFocusFromKeyboard(Keys keyStroke, Keys modifiers);
	}
}
