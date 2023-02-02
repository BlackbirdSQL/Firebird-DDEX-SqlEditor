using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.OLE.Interop;

using BlackbirdSql.Common;



namespace BlackbirdSql.Common.Extensions
{
	internal static class NativeMethods
	{
		[ComImport]
		[Guid(DataToolsCommands.NativeMethodsGuid)]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		internal interface IDTInternalRunManager
		{
			bool IsExecuting { get; }

			bool IsDebugging { get; }

			IVsDataConnection DebuggingConnection { get; }

			void RunProcedure([In] IVsDataConnection pDataConnection, [In] int exeObjType, [In] string bstrType, [In] string bstrName, [In] string bstrOwner, [In][MarshalAs(UnmanagedType.Bool)] bool fDebug);

			void RunScript([In] IVsDataConnection pDataConnection, [In] string bstrScript, [In] string bstrDebugFileName);

			void Cancel();
		}

		[StructLayout(LayoutKind.Sequential)]
		public class HELPINFO
		{
			public int cbSize = Marshal.SizeOf(typeof(HELPINFO));

			public int iContextType;

			public int iCtrlId;

			public IntPtr hItemHandle;

			public int dwContextId;

			public POINT MousePos;
		}

		public static Guid IID_IUnknown = new Guid("00000000-0000-0000-c000-000000000046");

		public static Guid SID_SVsLog = new Guid("4AAEA0BD-4327-44E0-B958-365EBCBEF679");

		public static Guid GUID_MicrosoftSqlServerDataSource = new Guid("067EA0D9-BA62-43f7-9106-34930C60C528");

		public static Guid GUID_MicrosoftSqlServerFileDataSource = new Guid("485C80D5-BC85-46db-9E6D-4238A0AD7B6B");

		public static Guid GUID_SqlServerDataProvider = new Guid("91510608-8809-4020-8897-FBA057E22D54");

		public static Guid CLSID_DSRef = new Guid("E09EE6AC-FEF0-41ae-9F77-3C394DA49849");

		public static Guid GUID_Mode_QueryDesigner = new Guid("B2C40B32-3A37-4ca9-97B9-FA44248B69FF");

		public static Guid CMDUIGUID_TextEditor = new Guid("8B382828-6202-11d1-8870-0000F87579D2");

		public static Guid SID_SDTInternalRunManager = new Guid("7884CCA5-4B77-427b-8124-9D1C3CA7A0C0");

		public const int S_OK = 0;

		public const int E_NOTIMPL = -2147467263;

		public const int E_FAIL = -2147467259;

		public const int STG_E_FILEALREADYEXISTS = -2147286960;

		public const int STG_E_NOTCURRENT = -2147286783;

		public const int OLE_E_PROMPTSAVECANCELLED = -2147221492;

		public const int DB_E_CANCELED = -2147217842;

		public const int WM_HELP = 83;

		public const int WM_SYSCOMMAND = 274;

		public const int SC_CONTEXTHELP = 61824;

		public const int HELPINFO_WINDOW = 1;

		public const uint VSITEMID_NIL = uint.MaxValue;

		public static readonly IntPtr HIERARCHY_DONTCHANGE = (IntPtr)(-1);

		public const uint VSDOCCOOKIE_NIL = 0u;

		public static readonly IntPtr SELCONTAINER_DONTCHANGE = (IntPtr)(-1);

		public const int ExeObjType_NotExecutable = 0;

		public const int ExeObjType_SSV = 1;

		public const int ExeObjType_Oracle = 2;

		public const int ExeObjType_Other = 4;

		public const int ExeObjType_SProc = 65536;

		public const int ExeObjType_Trigger = 131072;

		public const int ExeObjType_ScalarFunc = 262144;

		public const int ExeObjType_TableFunc = 524288;

		public const int ExeObjType_InlineFunc = 1048576;

		public static int WrapComCall(int hr)
		{
			Diag.Dug();
			if (FAILED(hr))
			{
				throw Marshal.GetExceptionForHR(hr);
			}

			return hr;
		}

		public static bool FAILED(int Status)
		{
			Diag.Dug();
			return Status < 0;
		}

		public static short LOWORD(uint dwValue)
		{
			Diag.Dug();
			return (short)(dwValue & 0xFFFF);
		}

		public static short HIWORD(uint dwValue)
		{
			Diag.Dug();
			return (short)((dwValue >> 16) & 0xFFFF);
		}

		[DllImport("ole32.dll", PreserveSig = false)]
		public static extern IStream CreateStreamOnHGlobal(IntPtr hGlobal, [MarshalAs(UnmanagedType.Bool)] bool fDeleteOnRelease);
	}
}
