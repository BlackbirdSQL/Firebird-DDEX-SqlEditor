using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;


namespace BlackbirdSql.Common.Providers;


[Guid(NativeMethods.PackageControllerGuid)]
public interface IPackageController
{
	void RegisterMiscHierarchy(IVsUIHierarchy hierarchy);
}
