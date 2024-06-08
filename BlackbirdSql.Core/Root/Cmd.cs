
using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using BlackbirdSql.Core.Properties;
using BlackbirdSql.Sys;
using BlackbirdSql.Sys.Ctl;
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
public abstract class Cmd : Sys.Cmd
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





	// GetDetailedException
	public static string GetDetailedException(Exception e)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (e != null)
		{
			if (e is DbException ex)
			{
				stringBuilder.AppendLine(HashLog.Format(CultureInfo.CurrentCulture, Resources.ClassNumber, ex.GetProcedure(), ex.GetErrorCode()));
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
