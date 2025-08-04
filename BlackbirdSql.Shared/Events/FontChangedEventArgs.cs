// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.FontChangedEventHandler
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.FontChangedEventArgs

using System;
using System.Drawing;



namespace BlackbirdSql.Shared.Events;


internal delegate void FontChangedEventHandler(object sender, FontChangedEventArgs e);


internal class FontChangedEventArgs : EventArgs
{
	internal Font Font { get; private set; }

	public FontChangedEventArgs(Font font)
	{
		Font = font;
	}
}
