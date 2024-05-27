
using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Security;
using System.Text;
using BlackbirdSql.Sys;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;



namespace BlackbirdSql.Core;

[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification="Using Diag.ThrowIfNotOnUIThread()")]


// =========================================================================================================
//												Cmd Class
//
/// <summary>
/// Central location for implementation of utility static methods. 
/// </summary>
// =========================================================================================================
public abstract class Cmd : BlackbirdSql.Sys.Cmd
{


	// ---------------------------------------------------------------------------------
	#region Constants - Cmd
	// ---------------------------------------------------------------------------------


	#endregion Constants





	// =========================================================================================================
	#region Fields - Cmd
	// =========================================================================================================


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - Cmd
	// =========================================================================================================


	#endregion Property accessors





	// =========================================================================================================
	#region Static Methods - Cmd
	// =========================================================================================================




	// CanonicalizeFileNameOrDirectoryImpl
	private static string CanonicalizeFileNameOrDirectoryImpl(string path, bool pathIsDir)
	{
		if (path.StartsWith("FBSQL::", StringComparison.OrdinalIgnoreCase) || path.StartsWith("FBSQLCLR::", StringComparison.OrdinalIgnoreCase))
		{
			return path;
		}

		path = Path.GetFullPath(path);
		path = path.ToUpperInvariant();
		if (pathIsDir)
		{
			return EnsureNoBackslash(path);
		}

		return path;
	}



	// GetDetailedException
	public static string GetDetailedException(Exception e)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (e != null)
		{
			if (e is DbException ex)
			{
				stringBuilder.AppendLine(HashLog.Format(CultureInfo.CurrentCulture, Properties.Resources.ClassNumber, ex.GetProcedure(), ex.GetErrorCode()));
			}

			stringBuilder.AppendLine(e.Message);
		}

		return stringBuilder.ToString();
	}


	public static IVsProject3 GetMiscellaneousProject(IServiceProvider provider = null)
	{
		Diag.ThrowIfNotOnUIThread();

		provider ??= new ServiceProvider(ApcManager.OleServiceProvider);

		IVsExternalFilesManager vsExternalFilesManager = provider.GetService(typeof(SVsExternalFilesManager)) as IVsExternalFilesManager
			?? throw Diag.ExceptionService(typeof(IVsExternalFilesManager));

		Native.WrapComCall(vsExternalFilesManager.GetExternalFilesProject(out IVsProject ppProject), []);

		return (IVsProject3)ppProject;
	}


	public static IVsProject3 GetMiscellaneousProject(IVsExternalFilesManager vsExternalFilesManager)
	{
		Diag.ThrowIfNotOnUIThread();

		Native.WrapComCall(vsExternalFilesManager.GetExternalFilesProject(out IVsProject ppProject), []);

		return (IVsProject3)ppProject;
	}


	#endregion Static Methods

}
