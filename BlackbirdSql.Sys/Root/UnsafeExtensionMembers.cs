// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj;



namespace BlackbirdSql;


// =========================================================================================================
//											UnsafeExtensionMembers Class
//
/// <summary>
/// Central class for package external class extension methods. 
/// </summary>
// =========================================================================================================
internal static class UnsafeExtensionMembers
{

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the app.config <see cref="ProjectItem"/> of a <see cref="Project"/>
	/// </summary>
	/// <param name="project"></param>
	/// <returns>true if app.config was updated else false</returns>
	// ---------------------------------------------------------------------------------
	internal static ProjectItem GetAppConfig(this Project @this, bool createIfNotFound = false)
	{
		Diag.ThrowIfNotOnUIThread();

		ProjectItem config = null;

		try
		{
			foreach (ProjectItem item in @this.ProjectItems)
			{
				if (item.Name.ToLower() == "app.config")
				{
					config = item;
					break;
				}
			}
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			return null;
		}

		if (createIfNotFound && config == null)
		{
			FileInfo info = new FileInfo(@this.FullName);
			string filename = info.Directory.FullName + "\\App.config";
			StreamWriter sw = File.CreateText(filename);

			string xml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
</configuration>";

			sw.Write(xml);
			sw.Close();

			config = @this.ProjectItems.AddFromFile(filename);
		}

		return config;

	}



	/// <summary>
	/// Gets the EnvDTE.Project for this hierarchy if it exists else null.
	/// </summary>
	internal static ProjectItem GetProjectItem(this IVsHierarchy @this, uint itemId)
	{
		object objProj;

		try
		{
			if (!ErrorHandler.Succeeded(@this.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out objProj)))
				return null;
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			throw ex;
		}

		if (objProj == null || objProj is not ProjectItem projectItem)
			return null;


		return projectItem;
	}



	internal static bool IsFolder(this Project @this)
	{
		return UnsafeCmd.IsProjectFolderKind(@this.Kind);
	}



	internal static bool IsMiscellaneous(this Project @this)
	{
		return UnsafeCmd.IsMiscProjectKind(@this.Kind);
	}


	internal static bool IsMiscellaneous(this IVsHierarchy @this)
	{
		string kind = @this.Kind();

		if (kind == null)
			return false;

		return UnsafeCmd.IsMiscProjectKind(kind);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks whether the project is a valid visible or misc type.
	/// </summary>
	/// <param name="project"></param>
	/// <returns>
	/// True if the project is valid else false.
	/// </returns>
	// ---------------------------------------------------------------------------------
	internal static bool IsDesignTimeProject(this IVsHierarchy @this)
	{
		Guid clsid = @this.Clsid();

		if (clsid == Guid.Empty)
			return false;

		string kind = clsid.ToString("D");

		if (UnsafeCmd.IsMiscProjectKind(kind))
			return true;

		if (UnsafeCmd.IsProjectFolderKind(kind))
			return false;

		Project project = @this.ToProject();

		return (project != null && project.Object is VSProject);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks whether the project is a visible or misc type.
	/// </summary>
	/// <param name="project"></param>
	/// <returns>
	/// True if the project is valid else false.
	/// </returns>
	// ---------------------------------------------------------------------------------
	internal static bool IsDesignTime(this Project @this)
	{
		// We're just peeking.
		// Diag.ThrowIfNotOnUIThread();

		if (@this.IsMiscellaneous())
			return true;

		if (@this.Object is not VSProject)
			return false;

		return !UnsafeCmd.IsProjectFolderKind(@this.Kind);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks whether the project is an editable design time project.
	/// </summary>
	/// <param name="project"></param>
	/// <returns>
	/// The VSProject object if the project is valid else null.
	/// </returns>
	// ---------------------------------------------------------------------------------
	internal static VSProject EditableObject(this Project @this)
	{
		// We're just peeking.
		// Diag.ThrowIfNotOnUIThread();

		VSProject projectObject = null;

		try
		{
			if (@this.Object == null)
				return null;

			projectObject = @this.Object as VSProject;
		}
		catch { };

		if (projectObject == null)
			return null;


		if (UnsafeCmd.IsMiscProjectKind(@this.Kind) || UnsafeCmd.IsProjectFolderKind(@this.Kind))
			return null;

		return projectObject;
	}



	internal static Guid Clsid(this IVsHierarchy @this)
	{

		uint itemid = VSConstants.VSITEMID_ROOT;
		Guid clsid = default;

		// Get the hierarchy root node.
		try
		{
			@this.GetGuidProperty(itemid, (int)__VSHPROPID.VSHPROPID_ProjectIDGuid, out clsid);
		}
		catch (Exception ex)
		{
			Diag.ThrowException(ex);
		}

		return clsid;
	}


	internal static string Kind(this IVsHierarchy @this)
	{
		return @this.Clsid().ToString("D");
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets an editable or miscellaneous project from a hierarchy object if it exists
	/// else null.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static Project ToDesignTimeProject(this IVsHierarchy @this)
	{
		Guid clsid = @this.Clsid();

		if (clsid == Guid.Empty)
			return null;

		string kind = clsid.ToString("D");

		if (UnsafeCmd.IsProjectFolderKind(kind))
			return null;

		return @this.ToProject();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets an editable project from a hierarchy object if it exists
	/// else null.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static Project ToEditableProject(this IVsHierarchy @this)
	{
		Project project = ToDesignTimeProject(@this);

		if (project == null || project.Object is not VSProject)
			return null;

		return project;
	}



	/// <summary>
	/// Gets the EnvDTE.Project for this hierarchy if it exists else null.
	/// </summary>
	internal static Project ToProject(this IVsUIHierarchy @this)
	{
		return (@this as IVsHierarchy).ToProject();
	}



	/// <summary>
	/// Gets the EnvDTE.Project for this hierarchy if it exists else null.
	/// </summary>
	internal static Project ToProject(this IVsProject3 @this)
	{
		return (@this as IVsHierarchy).ToProject();
	}



	/// <summary>
	/// Gets the EnvDTE.Project for this hierarchy if it exists else null.
	/// </summary>
	internal static Project ToProject(this IVsHierarchy @this)
	{
		uint itemid = VSConstants.VSITEMID_ROOT;
		object objProj;

		// Get the hierarchy root node.
		try
		{
			@this.GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_ExtObject, out objProj);
		}
		catch
		{
			return null;
		}

		if (objProj == null || objProj is not Project project)
			return null;

		return project;
	}

}
