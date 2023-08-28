// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Common.GlobalServices

using System;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;



namespace BlackbirdSql.Common.Ctl;

public static class GlobalServices
{
	private static IVsExtensibility3 _Extensibility;

	public static IVsExtensibility3 Extensibility
	{
		get
		{
			return _Extensibility ??= GetGlobalService<IVsExtensibility3>(typeof(IVsExtensibility3));
		}
	}

	private static T GetGlobalService<T>(Type t)
	{
		return (T)Package.GetGlobalService(t);
	}
}
