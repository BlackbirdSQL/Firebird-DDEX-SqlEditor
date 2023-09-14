#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLEditor\Microsoft.VisualStudio.Data.Tools.SqlEditor.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Drawing;

namespace BlackbirdSql.Common.Ctl.Events;


public class ColorChangedEventArgs : EventArgs
{
	public string ItemName { get; private set; }

	public Color? BkColor { get; private set; }

	public Color? FgColor { get; private set; }

	public ColorChangedEventArgs(string itemName, Color? bkColor, Color? fgColor)
	{
		ItemName = itemName;
		BkColor = bkColor;
		FgColor = fgColor;
	}
}
