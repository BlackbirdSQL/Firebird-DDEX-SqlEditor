// Microsoft.VisualStudio.Data.Framework, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Framework.NativeMethods
using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.OLE.Interop;

internal static class NativeMethods
{
	private const string Kernel32 = "kernel32.dll";

	private const string Ole32 = "ole32.dll";

	public static Guid IID_IUnknown = new Guid("00000000-0000-0000-c000-000000000046");

	public static Guid GUID_AdoDotNetTechnology = new Guid("77AB9A9D-78B9-4ba7-91AC-873F5338F1D2");

	public static Guid CLSID_DSRef = new Guid("E09EE6AC-FEF0-41ae-9F77-3C394DA49849");

	public static Guid GUID_DSRefProperty_Provider = new Guid("B30985D6-6BBB-45f2-9AB8-371664F03270");

	public static Guid GUID_DSRefProperty_PreciseType = new Guid("39A5A7E7-513F-44a4-B79D-7652CD8962D9");

	public const int GMEM_MOVEABLE = 2;

	public const int GMEM_SHARE = 8192;

	public const int S_OK = 0;

	public const int DATA_S_SAMEFORMATETC = 262448;

	public const int E_NOTIMPL = -2147467263;

	public const int E_NOINTERFACE = -2147467262;

	public const int E_POINTER = -2147467261;

	public const int E_FAIL = -2147467259;

	public const int SVC_E_UNKNOWNSERVICE = -2147467259;

	public const int OLE_E_ADVISENOTSUPPORTED = -2147221501;

	public const int E_INVALIDARG = -2147024809;

	public const int VSITEMID_NIL = -1;

	public const int VSITEMID_ROOT = -2;

	public const int VSITEMID_SELECTION = -3;

	public const int VSDOCCOOKIE_NIL = 0;

	public static readonly IntPtr DSREFNODEID_NIL = (IntPtr)0;

	public static readonly IntPtr DSREFNODEID_ROOT = (IntPtr)0;

	public const int DSREF_PRECISE_TYPE_DATABASE = 1;

	public static bool SUCCEEDED(int Status)
	{
		return Status >= 0;
	}

	public static bool FAILED(int Status)
	{
		return Status < 0;
	}

	[DllImport("kernel32.dll")]
	public static extern IntPtr GlobalAlloc(int uFlags, IntPtr dwBytes);

	[DllImport("kernel32.dll")]
	public static extern IntPtr GlobalSize(IntPtr hMem);

	[DllImport("kernel32.dll")]
	public static extern IntPtr GlobalLock(IntPtr hMem);

	[DllImport("kernel32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GlobalUnlock(IntPtr hMem);

	[DllImport("kernel32.dll")]
	public static extern IntPtr GlobalFree(IntPtr hMem);

	[DllImport("ole32.dll", PreserveSig = false)]
	public static extern IStream CreateStreamOnHGlobal(IntPtr hGlobal, [MarshalAs(UnmanagedType.Bool)] bool fDeleteOnRelease);

	[DllImport("ole32.dll", PreserveSig = false)]
	public static extern void OleSaveToStream(IPersistStream pPStm, IStream pStm);
}
