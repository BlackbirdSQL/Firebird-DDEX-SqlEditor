// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.ColorChangedEventHandler
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.ColorChangedEventArgs

using System;
using System.Drawing;



namespace BlackbirdSql.Shared.Events;


internal delegate void ColorChangedEventHandler(object sender, ColorChangedEventArgs e);


internal class ColorChangedEventArgs : EventArgs
{
	internal string ItemName { get; private set; }

	internal Color? BkColor { get; private set; }

	internal Color? FgColor { get; private set; }

	public ColorChangedEventArgs(string itemName, Color? bkColor, Color? fgColor)
	{
		ItemName = itemName;
		BkColor = bkColor;
		FgColor = fgColor;
	}
}
