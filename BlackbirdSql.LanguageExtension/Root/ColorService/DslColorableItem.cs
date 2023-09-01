
using System.Drawing;

using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;



namespace BlackbirdSql.LanguageExtension.ColorService
{
	internal class DslColorableItem : ColorableItem
	{
		public Color HiForeColor { get; private set; }

		public Color HiBackColor { get; private set; }

		public DslColorableItem(string name, string displayName, COLORINDEX foreColor, COLORINDEX backColor, Color hiForeColor, Color hiBackColor, bool bold, bool strikethrough)
			: base(name, displayName, foreColor, backColor, hiForeColor, hiBackColor, FONTFLAGS.FF_DEFAULT | (bold ? FONTFLAGS.FF_BOLD : FONTFLAGS.FF_DEFAULT) | (strikethrough ? FONTFLAGS.FF_STRIKETHROUGH : FONTFLAGS.FF_DEFAULT))
		{
			HiForeColor = hiForeColor;
			HiBackColor = hiBackColor;
		}
	}
}
