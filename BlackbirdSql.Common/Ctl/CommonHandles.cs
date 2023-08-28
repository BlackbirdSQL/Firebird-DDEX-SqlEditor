// Microsoft.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Common.Win32.CommonHandles


namespace BlackbirdSql.Common.Ctl;

public sealed class CommonHandles
{
	public static readonly int Accelerator = HandleCollector.RegisterType("Accelerator", 80, 50);

	public static readonly int Cursor = HandleCollector.RegisterType("Cursor", 20, 500);

	public static readonly int EMF = HandleCollector.RegisterType("EnhancedMetaFile", 20, 500);

	public static readonly int Find = HandleCollector.RegisterType("Find", 0, 1000);

	public static readonly int GDI = HandleCollector.RegisterType("GDI", 50, 500);

	public static readonly int HDC = HandleCollector.RegisterType("HDC", 100, 2);

	public static readonly int CompatibleHDC = HandleCollector.RegisterType("ComptibleHDC", 50, 50);

	public static readonly int Icon = HandleCollector.RegisterType("Icon", 20, 500);

	public static readonly int Kernel = HandleCollector.RegisterType("Kernel", 0, 1000);

	public static readonly int Menu = HandleCollector.RegisterType("Menu", 30, 1000);

	public static readonly int Window = HandleCollector.RegisterType("Window", 5, 1000);
}
