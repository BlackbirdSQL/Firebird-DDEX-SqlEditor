﻿
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Security;
using System.Text;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;
using FirebirdSql.Data.FirebirdClient;
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
public abstract class Cmd
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


	// CanonicalizeDirectoryName
	public static string CanonicalizeDirectoryName(string fullPathDirName)
	{
		if (string.IsNullOrEmpty(fullPathDirName))
		{
			ArgumentNullException ex = new("fullPathDirName");
			Diag.Dug(ex);
			throw ex;
		}

		return CanonicalizeFileNameOrDirectoryImpl(fullPathDirName, pathIsDir: true);
	}


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


	// CheckForEmptyString
	public static void CheckForEmptyString(string variable, string variableName)
	{
		CheckForNullReference(variable, variableName);
		CheckForNullReference(variableName, "variableName");
		if (variable.Length == 0)
		{
			ArgumentNullException ex = new("variableName");
			Diag.Dug(ex);
			throw ex;
		}
	}



	// CheckForNull
	public static void CheckForNull(object var, string varName)
	{
		if (var == null)
		{
			ArgumentNullException ex = new(varName);
			Diag.Dug(ex);
			throw ex;
		}
	}



	// CheckForNullReference
	public static void CheckForNullReference(object variable, string variableName)
	{
		if (variableName == null)
		{
			ArgumentNullException ex = new("variableName");
			Diag.Dug(ex);
			throw ex;
		}

		if (variable == null)
		{
			ArgumentNullException ex = new(variableName);
			Diag.Dug(ex);
			throw ex;
		}
	}



	// CheckStringForNullOrEmpty
	public static void CheckStringForNullOrEmpty(string stringVar, string stringVarName)
	{
		CheckStringForNullOrEmpty(stringVar, stringVarName, trim: false);
	}

	// CheckStringForNullOrEmpty
	public static void CheckStringForNullOrEmpty(string stringVar, string stringVarName, bool trim)
	{
		CheckForNull(stringVar, stringVarName);

		if (trim)
			stringVar = stringVar.Trim();

		if (stringVar.Length == 0)
		{
			ArgumentException ex = new("EmptyStringNotAllowed", stringVarName);
			Diag.Dug(ex);
			throw ex;
		}
	}


	// EnsureNoBackslash
	public static string EnsureNoBackslash(string fullPath)
	{
		string result = fullPath;
		if (!string.IsNullOrEmpty(fullPath) && fullPath.Length > 1 && (fullPath[^1] == '\\' || fullPath[^1] == '/'))
		{
			result = fullPath[..^1];
		}

		return result;
	}

	/// <summary>
	/// ThrowOnFailure token
	/// </summary>
	protected static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);

	// Failed
	public static bool Failed(int hr)
	{
		return hr < 0;
	}








	// GetDetailedException
	public static string GetDetailedException(Exception e)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (e != null)
		{
			if (e is FbException ex)
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


	/*
	public static Type GetTypeFromProject(string typeName, Project project, IServiceProvider serviceProvider)
	{
		DynamicTypeService typeSvc = serviceProvider.GetService(typeof(DynamicTypeService)) as DynamicTypeService;
		IVsHierarchy vsHierarchy = GetVsHierarchy(serviceProvider, project);

		return typeSvc.GetTypeResolutionService(vsHierarchy).GetType(typeName);
	}


	private static IVsHierarchy GetVsHierarchy(IServiceProvider provider, Project project)
	{
		IVsSolution vsSolution = (IVsSolution)provider.GetService(typeof(IVsSolution));

		if (vsSolution == null)
			return null;

		___(vsSolution.GetProjectOfUniqueName(project.UniqueName, out IVsHierarchy ppHierarchy));

		return ppHierarchy;
	}
	*/

	// IsCriticalException
	public static bool IsCriticalException(Exception ex)
	{

		if (ex is NullReferenceException || ex is OutOfMemoryException || ex is System.Threading.ThreadAbortException)
		{
			return true;
		}

		if (ex.InnerException != null)
			return IsCriticalException(ex.InnerException);

		return false;
	}


	// IsSamePath
	public static bool IsSamePath(string file1, string file2)
	{
		if (file1 == null || file1.Length == 0)
		{
			if (file2 != null)
				return file2.Length == 0;

			return true;
		}

		try
		{
			if (!Uri.TryCreate(file1, UriKind.Absolute, out var result)
				|| !Uri.TryCreate(file2, UriKind.Absolute, out var result2))
			{
				return false;
			}

			if (result != null && result.IsFile && result2 != null && result2.IsFile)
			{
				try
				{
					string strA = CanonicalizeDirectoryName(result.LocalPath);
					string strB = CanonicalizeDirectoryName(result2.LocalPath);

					return string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase) == 0;
				}
				catch (PathTooLongException)
				{
					return false;
				}
				catch (ArgumentException)
				{
					return false;
				}
				catch (SecurityException)
				{
					return false;
				}
				catch (NotSupportedException)
				{
					return false;
				}
			}

			return file1 == file2;
		}
		catch (UriFormatException ex5)
		{
			Tracer.Warning(typeof(Cmd), "IsSamePath()", "IsSamePath exception: {0}", ex5.Message);
		}

		return false;
	}


	// NullEquality
	public static EnNullEquality NullEquality(object lhs, object rhs)
	{
		if ((lhs == null || lhs == DBNull.Value) && (rhs == null || rhs == DBNull.Value)) return EnNullEquality.Equal;

		if (lhs == null || lhs == DBNull.Value || rhs == null || rhs == DBNull.Value) return EnNullEquality.UnEqual;

		return EnNullEquality.NotNulls;

	}





	/*
	// UseRandomizedHashing
	public static bool UseRandomizedHashing()
	{
		return Native.InternalUseRandomizedHashing();
	}
	*/


	#endregion Static Methods

}
