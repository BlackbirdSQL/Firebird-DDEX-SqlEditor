
using System;
using System.Runtime.InteropServices;
using System.Security;




namespace BlackbirdSql.Core;


// =========================================================================================================
//											Native Class
//
/// <summary>
/// Central location for accessing of native members. 
/// </summary>
// =========================================================================================================

public abstract class Native
{


	// ---------------------------------------------------------------------------------
	#region Constants and Static Variables - Native
	// ---------------------------------------------------------------------------------



	#endregion Constants and Static Variables





	// =========================================================================================================
	#region Static Methods - Native
	// =========================================================================================================


	[DllImport("user32.dll")]
	public static extern bool EnableWindow(IntPtr hWnd, bool enable);


	// Failed
	public static bool Failed(int hr)
	{
		return hr < 0;
	}


	/*
	[DllImport("QCall", CharSet = CharSet.Unicode)]
	[SecurityCritical]
	[SuppressUnmanagedCodeSecurity]
	public static extern bool InternalUseRandomizedHashing();
	*/


	[DllImport("User32.dll", CharSet = CharSet.Unicode)]
	public static extern int SetProp(HandleRef hWnd, string propName, HandleRef data);


	// Succeeded
	public static bool Succeeded(int hr)
	{
		return hr >= 0;
	}



	// ThrowOnFailure
	public static int ThrowOnFailure(int hr)
	{
		return ThrowOnFailure(hr, (string)null);
	}

	// ThrowOnFailure
	public static int ThrowOnFailure(int hr, string context = null)
	{
		if (Failed(hr))
			Marshal.ThrowExceptionForHR(hr);

		return hr;
	}

	// ThrowOnFailure
	public static int ThrowOnFailure(int hr, params int[] expectedFailures)
	{
		if (Failed(hr) && (expectedFailures == null || Array.IndexOf(expectedFailures, hr) < 0))
		{
			Exception ex = Marshal.GetExceptionForHR(hr);
			Diag.Dug(ex);
			throw ex;
		}

		return hr;
	}



	// WrapComCall
	public static int WrapComCall(int hr)
	{
		if (Cmd.Failed(hr))
			throw Marshal.GetExceptionForHR(hr);

		return hr;
	}

	// WrapComCall
	public static int WrapComCall(int hr, params int[] expectedFailures)
	{
		if (Cmd.Failed(hr) && (expectedFailures == null || Array.IndexOf(expectedFailures, hr) < 0))
		{
			throw Marshal.GetExceptionForHR(hr);
		}

		return hr;
	}



	#endregion Static Methods


}
