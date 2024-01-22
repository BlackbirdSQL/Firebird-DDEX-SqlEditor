// Microsoft.VisualStudio.Data.Interop, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Interop.IVsDataConnection
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BlackbirdSql.Common.Ctl.DataTools.Events.Interop;


namespace BlackbirdSql.Common.Ctl.DataTools.Interfaces.Interop;

[ComImport]
[Guid(VS.IVsDataConnectionInteropGuid)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IVsDataConnection
{
	// Microsoft.VisualStudio.Data.Interop.__VSDATACONNECTIONSTATE
	internal enum EN__VSDATACONNECTIONSTATE
	{
		VSDCS_CLOSED = 0,
		VSDCS_OPEN = 1,
		VSDCS_BROKEN = int.MinValue
	}

	[DispId(1610678272)]
	Guid Provider
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		get;
	}

	[DispId(1610678273)]
	string DisplayConnectionString
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[param: In]
		[param: MarshalAs(UnmanagedType.BStr)]
		set;
	}

	[DispId(1610678275)]
	string EncryptedConnectionString
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[param: In]
		[param: MarshalAs(UnmanagedType.BStr)]
		set;
	}

	[DispId(1610678277)]
	bool IsLockedForExclusiveAccess
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		get;
	}

	[DispId(1610678278)]
	bool ProviderObjectIsLocked
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		get;
	}

	[DispId(1610678279)]
	int ConnectionTimeout
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[param: In]
		set;
	}

	[DispId(1610678281)]
	IVsDataConnection.EN__VSDATACONNECTIONSTATE State
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[return: MarshalAs(UnmanagedType.Interface)]
	IVsDataConnection GetExclusiveAccessProxy([In] int iLockTimeout);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void Open();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[return: MarshalAs(UnmanagedType.IUnknown)]
	object GetLockedProviderObject([In] int iLockTimeout);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void UnlockProviderObject();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void Close();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void ReleaseExclusiveAccessProxy();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	uint AdviseEvents([In][MarshalAs(UnmanagedType.Interface)] IVsDataConnectionEvents pDataConnectionEvents);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void UnadviseEvents([In] uint dwCookie);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	bool EquivalentTo([In] Guid guidProvider, [In][MarshalAs(UnmanagedType.BStr)] string bstrConnectionString, [In] bool fEncryptedString);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[return: MarshalAs(UnmanagedType.Interface)]
	IVsDataConnection Clone();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	void Dispose();
}
