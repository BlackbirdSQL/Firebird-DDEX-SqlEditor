// Microsoft.Cosmos.ClientTools.IDECommon, Version=2.6.5000.0, Culture=neutral, PublicKeyToken=f300afd708cefcd3
// Microsoft.Cosmos.ClientTools.ClientCommon.Controls.IColorService

using System;
using System.Runtime.InteropServices;

namespace BlackbirdSql.LanguageExtension.Interfaces;

[ComVisible(true)]
[Guid(ServiceData.ColorServiceGuid)]
public interface IBColorService
{
	System.Windows.Media.Color GetColor(Guid category, string key, int type);

	System.Windows.Media.Color GetColor(object key);

	System.Drawing.Color GetGDIColor(object key);

	object GetKey(Guid category, string key, int type);

	System.Windows.Media.Color GetToolWindowBackgroundColor();

	System.Windows.Media.Color GetToolWindowTextColor();
}
