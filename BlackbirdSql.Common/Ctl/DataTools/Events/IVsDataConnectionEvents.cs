// Microsoft.VisualStudio.Data.Interop, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Interop.IVsDataConnectionEvents
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BlackbirdSql.Common.Ctl.DataTools.Interfaces.Interop;

namespace BlackbirdSql.Common.Ctl.DataTools.Events.Interop;

[ComImport]
[Guid("398E5329-4D33-4534-AB2C-D8EA177B59F2")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IVsDataConnectionEvents
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void OnStateChanged([In] IVsDataConnection.EN__VSDATACONNECTIONSTATE vsdcsOldState, [In] IVsDataConnection.EN__VSDATACONNECTIONSTATE vsdcsNewState);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void OnMessageReceived([In][MarshalAs(UnmanagedType.BStr)] string bstrMessage);
}
