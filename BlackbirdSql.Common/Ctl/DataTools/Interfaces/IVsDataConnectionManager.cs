// Microsoft.VisualStudio.Data.Interop, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Interop.IVsDataConnectionManager
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BlackbirdSql.Core;


//namespace Microsoft.VisualStudio.Data.Interop;
namespace BlackbirdSql.Common.Ctl.DataTools.Interfaces.Interop;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid(VS.IVsDataConnectionManagerInteropGuid)]
internal interface IVsDataConnectionManager
{
	[DispId(1610678272)]
	int Count
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void CopyTo([In][Out][MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_UNKNOWN)] IVsDataConnection[] saDataConnections, [In] int iIndex);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[return: MarshalAs(UnmanagedType.Interface)]
	IVsDataConnection GetDataConnection([In] ref Guid guidProvider, [In][MarshalAs(UnmanagedType.BStr)] string bstrConnectionString, [In] bool fEncryptedString);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void InvalidateDataConnection([In] ref Guid guidProvider, [In][MarshalAs(UnmanagedType.BStr)] string bstrConnectionString, [In] bool fEncryptedString);
}
