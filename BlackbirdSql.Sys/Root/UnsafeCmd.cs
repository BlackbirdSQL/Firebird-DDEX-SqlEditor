
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BlackbirdSql.Sys.Properties;
using EnvDTE;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj;

using IOleCommandTarget = Microsoft.VisualStudio.OLE.Interop.IOleCommandTarget ;



namespace BlackbirdSql;


// =========================================================================================================
//												UnsafeCmd Class
//
/// <summary>
/// Central location for implementation of unsafe utility static methods. These are commands that require
/// the ui thread. It's the Caller's responsibility to ensure calls to UnsafeCmd are safe.
/// </summary>
// =========================================================================================================
internal abstract class UnsafeCmd
{

	// ---------------------------------------------------------------------------------
	#region Property Accessors - UnsafeCmd
	// ---------------------------------------------------------------------------------


	internal static Project MiscellaneousProject =>
		MiscellaneousProjectHierarchy.ToProject();

	internal static IVsProject3 MiscellaneousProjectHierarchy =>
		GetMiscellaneousProjectHierarchy(null);


	#endregion Property Accessors





	// =========================================================================================================
	#region Static Methods - UnsafeCmd
	// =========================================================================================================


	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	protected static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);




	/// <summary>
	/// <see cref="ErrorHandler.Succeeded"/> token.
	/// </summary>
	protected static bool __(int hr) => ErrorHandler.Succeeded(hr);



	internal static string GetFileNameUsingSaveDialog(string strFilterString, string strCaption, string initialDir, IVsSaveOptionsDlg optionsDlg)
	{
		return GetFileNameUsingSaveDialog(strFilterString, strCaption, initialDir, optionsDlg, out _);
	}



	internal static string GetFileNameUsingSaveDialog(string strFilterString, string strCaption, string initialDir, IVsSaveOptionsDlg optionsDlg, out int filterIndex)
	{
		// Evs.Trace(typeof(CommonUtils), "CommonUtils.GetFileNameUsingSaveDialog", "strFilterString = {0}, strCaption = {1}", strFilterString, strCaption);

		Diag.ThrowIfNotOnUIThread();

		filterIndex = 0;
		if (Package.GetGlobalService(typeof(SVsUIShell)) is IVsUIShell vsUIShell)
		{
			int num = 512;
			IntPtr intPtr = IntPtr.Zero;
			VSSAVEFILENAMEW[] array = new VSSAVEFILENAMEW[1];
			try
			{
				string empty = "";
				char[] array2 = new char[num];
				intPtr = Marshal.AllocCoTaskMem(array2.Length * 2);
				Marshal.Copy(array2, 0, intPtr, array2.Length);
				array[0].lStructSize = (uint)Marshal.SizeOf(typeof(VSSAVEFILENAMEW));
				___(vsUIShell.GetDialogOwnerHwnd(out array[0].hwndOwner));
				array[0].pwzFilter = strFilterString;
				array[0].pwzFileName = intPtr;
				array[0].nMaxFileName = (uint)num;
				array[0].pwzDlgTitle = strCaption;
				array[0].dwFlags = (uint)__VSRDTSAVEOPTIONS.RDTSAVEOPT_ForceSave;
				array[0].pSaveOpts = optionsDlg;
				if (initialDir != null && initialDir.Length != 0)
				{
					array[0].pwzInitialDir = initialDir;
				}
				else
				{
					try
					{
						array[0].pwzInitialDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
					}
					catch
					{
					}
				}

				try
				{
					if (vsUIShell.GetSaveFileNameViaDlg(array) != 0)
					{
						return null;
					}

					filterIndex = (int)array[0].nFilterIndex;
					Marshal.Copy(intPtr, array2, 0, array2.Length);
					int i;
					for (i = 0; i < array2.Length && array2[i] != 0; i++)
					{
					}

					empty = new string(array2, 0, i);
					// Evs.Trace(typeof(CommonUtils), Tracer.EnLevel.Information, "CommonUtils.GetFileNameUsingSaveDialog", "file name is {0}", empty);
					return empty;
				}
				catch (Exception e)
				{
					Diag.Ex(e);
					return null;
				}
			}
			catch (Exception e2)
			{
				Diag.Ex(e2);
				MessageCtl.ShowX("", e2);
				return null;
			}
			finally
			{
				if (intPtr != IntPtr.Zero)
				{
					Marshal.FreeCoTaskMem(intPtr);
				}
			}
		}

		// Evs.Trace(typeof(CommonUtils), Tracer.EnLevel.Verbose, "CommonUtils.GetFileNameUsingSaveDialog", "cannot get IVsUIShell!!");
		return null;
	}




	internal static IVsProject3 GetMiscellaneousProjectHierarchy(IServiceProvider provider)
	{
		Diag.ThrowIfNotOnUIThread();

		provider ??= new ServiceProvider(ApcManager.OleServiceProvider);

		IVsExternalFilesManager vsExternalFilesManager = provider.GetService(typeof(SVsExternalFilesManager)) as IVsExternalFilesManager
			?? throw Diag.ExceptionService(typeof(IVsExternalFilesManager));

		___(vsExternalFilesManager.GetExternalFilesProject(out IVsProject ppProject));

		return (IVsProject3)ppProject;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets a list of all open misc project items and a Value tuple of document cookie
	/// and moniker in a project hierarchy given a list of extensions and an optional
	/// prefix else null.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static IDictionary<ProjectItem, object> GetOpenMiscProjectItems(IVsHierarchy hierarchy, string[] extensions, string prefix = null)
	{
		Dictionary<ProjectItem, object> projectItems = [];

		RunningDocumentTable rdt = new((IServiceProvider)ApcManager.PackageInstance);


		foreach (RunningDocumentInfo docInfo in rdt)
		{

			if (!(docInfo.Hierarchy == hierarchy))
				continue;

			ProjectItem projectItem = hierarchy.GetProjectItem(docInfo.ItemId);

			if (projectItem == null)
				continue;


			if (projectItem.FileCount < 1 || Kind(projectItem.Kind) != "MiscItem")
				continue;

			// FileNames is 1 based indexing - How/Why??? - A VB team did this!
			string filepath = projectItem.FileNames[1].ToLowerInvariant();

			if (prefix != null && !filepath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
				continue;

			bool found = false;

			foreach (string ext in extensions)
			{
				if (filepath.EndsWith(ext))
				{
					found = true;
					break;
				}
			}

			if (!found)
				continue;

			projectItems.Add(projectItem, docInfo.DocData);
		}


		return projectItems;
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets a list of all open project items and a Value tuple of document cookie 
	/// and moniker in a project hierarchy given a list of extensions.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static IDictionary<ProjectItem, (uint, string)> GetOpenProjectItems(IVsHierarchy hierarchy, string[] extensions)
	{
		Dictionary<ProjectItem, (uint, string)> projectItems = [];


		RunningDocumentTable rdt = new((IServiceProvider)ApcManager.PackageInstance);


		foreach (RunningDocumentInfo docInfo in rdt)
		{

			if (!(docInfo.Hierarchy == hierarchy))
				continue;

			ProjectItem projectItem = hierarchy.GetProjectItem(docInfo.ItemId);

			if (projectItem == null)
				continue;


			if (projectItem.FileCount < 1)
				continue;


			Property link = null;

			try
			{
				link = projectItem.Properties.Item("IsLink");

				if (link != null && link.Value != null)
				{
					if (link.Value is string str)
					{
						if (!string.IsNullOrWhiteSpace(str))
							continue;
					}
					else if (link.Value is bool tf && tf)
					{ 
						continue;
					}
				}
			}
			catch (Exception ex)
			{
				object linkValue = link?.Value ?? Resources.StringNullValue;
				string linkType = link?.Value?.GetType()?.FullName ?? Resources.StringNullValue;
				Diag.Debug(ex, Resources.LabelProjectItemIsLink.Fmt(linkType, linkValue));
			}


			bool found = false;
			// FileNames is 1 based indexing - How/Why??? - A VB team did this!
			string filepath = projectItem.FileNames[1].ToLowerInvariant();

			foreach (string ext in extensions)
			{
				try
				{
					if (filepath.EndsWith(ext))
					{
						found = true;
						break;
					}
				}
				catch { }
			}

			if (!found)
				continue;

			projectItems.Add(projectItem, (docInfo.DocCookie, docInfo.Moniker));
		}

		return projectItems;
	}



	internal static string GetProjectKind(string name)
	{
		foreach (KeyValuePair<string, string> pair in ProjectGuids)
		{
			if (name.Equals(pair.Value, StringComparison.OrdinalIgnoreCase))
				return pair.Key;
		}
		return null;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Identifies whether or not a project is a CPS project
	/// </summary>
	/// <param name="hierarchy"></param>
	/// <returns>true if project is CPS</returns>
	// ---------------------------------------------------------------------------------
	internal static bool IsCpsProject(IVsHierarchy hierarchy)
	{
		Requires.NotNull(hierarchy, "hierarchy");
		return hierarchy.IsCapabilityMatch("CPS");
	}



	// IsInAutomationFunction
	internal static bool IsInAutomationFunction()
	{
		Diag.ThrowIfNotOnUIThread();

		IVsExtensibility3 extensibility = VS.GetExtensibility();

		if (extensibility == null)
			return false;

		___(extensibility.IsInAutomationFunction(out int pfInAutoFunc));

		return pfInAutoFunc != 0;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns True if project is the misc project.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static bool IsMiscProjectKind(string kind)
	{
		return kind.Trim(['{', '}']).Equals(VSConstants.CLSID.MiscellaneousFilesProject_string.Trim(['{', '}']),
			StringComparison.OrdinalIgnoreCase);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns True if project is a folder project.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static bool IsProjectFolderKind(string kind)
	{
		kind = kind.Trim(['{', '}']).ToLower();

		return kind.Equals(VSConstants.GUID_ItemType_VirtualFolder.ToString("D"))
			|| kind.Equals("66a26720-8fb5-11d2-aa7e-00c04f688dde")
			|| kind.Equals("6bb5f8ef-4483-11d3-8bcf-00c04f8ec28c");
	}




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns True if project is a folder project.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static bool IsProjectFolderKind(Guid clsid)
	{
		return IsProjectFolderKind(clsid.ToString("D"));
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the descriptive name for a DTE object Kind guid string
	/// </summary>
	/// <param name="kind"></param>
	/// <returns>The descriptive name from <see cref="ProjectGuids"/></returns>
	// ---------------------------------------------------------------------------------
	internal static string Kind(string kind)
	{
		if (!ProjectGuids.TryGetValue(kind, out string name))
			name = kind;

		return name;
	}



	/// <summary>
	/// Returns a recursive list of the design time projects for the current solution. Design
	/// time projects includes all projects that have an 'Object' property of type
	/// <see cref="VSProject"/> assigned and are visible to the user.
	/// If rootProject is supplied recursion begins at that project.
	/// </summary>
	/// <returns></returns>
	internal static List<Project> RecursiveGetDesignTimeProjects(Project rootProject = null)
	{
		IEnumerable rootProjects;
		List<Project> projects = [];

		if (rootProject == null)
		{
			if (ApcManager.SolutionProjects == null || ApcManager.SolutionProjects.Count == 0)
			{
				return projects;
			}

			rootProjects = ApcManager.SolutionProjects;
		}
		else
		{
			rootProjects = new List<Project>([rootProject]);
		}

		foreach (Project project in rootProjects)
		{
			RecursiveGetDesignTimeProject(projects, project);
		}

		return projects;

	}



	private static void RecursiveGetDesignTimeProject(List<Project> projects, Project project)
	{
		// There's a dict list of these at the end of the class
		if (UnsafeCmd.Kind(project.Kind) == "ProjectFolder")
		{
			if (project.ProjectItems != null && project.ProjectItems.Count > 0)
			{
				RecursiveGetDesignTimeProject(projects, project.ProjectItems);
			}
		}
		else
		{
			if (project.EditableObject() != null)
				projects.Add(project);
		}
	}



	private static void RecursiveGetDesignTimeProject(List<Project> projects, ProjectItems projectItems)
	{
		foreach (ProjectItem projectItem in projectItems)
		{
			if (projectItem.SubProject != null)
				RecursiveGetDesignTimeProject(projects, projectItem.SubProject);
		}
	}



	internal static void ShowContextMenuEvent(Guid rclsidActive, int menuId, int xPos, int yPos, IOleCommandTarget commandTarget)
	{
		Diag.ThrowIfNotOnUIThread();

		IVsUIShell obj = Package.GetGlobalService(typeof(IVsUIShell)) as IVsUIShell;

		_ = Control.MousePosition;
		POINTS pOINTS = new POINTS
		{
			x = (short)xPos,
			y = (short)yPos
		};
		obj.ShowContextMenu(VS.dwReserved, ref rclsidActive, menuId, [pOINTS], commandTarget);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// DTE object Kind guid string descriptive name dictionary
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static readonly Dictionary<string, string> ProjectGuids =
		new (StringComparer.OrdinalIgnoreCase)
	{
		{ "{2150E333-8FDC-42A3-9474-1A3956D46DE8}", "SolutionFolder" },
		{ "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}", "ProjectFolder" },
		{ "{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}", "PhysicalFolder" },
		{ "{66A2671F-8FB5-11D2-AA7E-00C04F688DDE}", "MiscItem" },
		{ "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}", "VbProject" },
		{ "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}", "C#Project" },
		{ "{A2FE74E1-B743-11d0-AE1A-00A0C90FFFC3}", "MiscFilesProject" },
		{ "{3AE79031-E1BC-11D0-8F78-00A0C9110057}", "SolutionExplorer"}
			/* The above the only one's we care about ATM
			{ "{06A35CCD-C46D-44D5-987B-CF40FF872267}", "DeploymentMergeModule" },
			{ "{14822709-B5A1-4724-98CA-57A101D1B079}", "Workflow(C#)" },
			{ "{20D4826A-C6FA-45DB-90F4-C717570B9F32}", "Legacy(2003)SmartDevice(C#)" },
			{ "{262852C6-CD72-467D-83FE-5EEB1973A190}", "JsProject" },
			{ "{2DF5C3F4-5A5F-47a9-8E94-23B4456F55E2}", "XNA(XBox)" },
			{ "{32F31D43-81CC-4C15-9DE6-3FC5453562B6}", "WorkflowFoundation" },
			{ "{349C5851-65DF-11DA-9384-00065B846F21}", "WebApplicationProject" },
			{ "{3AC096D0-A1C2-E12C-1390-A8335801FDAB}", "Test" },
			{ "{3D9AD99F-2412-4246-B90B-4EAA41C64699}", "WCF" },
			{ "{3EA9E505-35AC-4774-B492-AD1749C4943A}", "DeploymentCab" },
			{ "{4B160523-D178-4405-B438-79FB67C8D499}", "NomadVSProject" },
			{ "{4D628B5B-2FBC-4AA6-8C16-197242AEB884}", "SmartDevice(C#)" },
			{ "{4F174C21-8C12-11D0-8340-0000F80270F8}", "Db(other" },
			{ "{54435603-DBB4-11D2-8724-00A0C9A8B90C}", "VS2015InstallerProjectExtension" },
			{ "{593B0543-81F6-4436-BA1E-4747859CAAE2}", "SharePoint(C#)" },
			{ "{603C0E0B-DB56-11DC-BE95-000D561079B0}", "ASP.NET(MVC 1.0)" },
			{ "{60DC8134-EBA5-43B8-BCC9-BB4BC16C2548}", "WPF" },
			{ "{68B1623D-7FB9-47D8-8664-7ECEA3297D4F}", "SmartDevice(VB.NET)" },
			{ "{66A2671D-8FB5-11D2-AA7E-00C04F688DDE}", "MiscProject" },
			{ "{66A26722-8FB5-11D2-AA7E-00C04F688DDE}", "SolutionItem" },
			{ "{67294A52-A4F0-11D2-AA88-00C04F688DDE}", "UnloadedProject" },
			{ "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}", "ProjectFile" },
			{ "{6BC8ED88-2882-458C-8E55-DFD12B67127B}", "MonoTouch" },
			{ "{6D335F3A-9D43-41b4-9D22-F6F17C4BE596}", "XNA(Win)" },
			{ "{76F1466A-8B6D-4E39-A767-685A06062A39}", "WinPhone8/8.1(Blank/Hub/Webview)" },
			{ "{786C830F-07A1-408B-BD7F-6EE04809D6DB}", "PortableClassLibrary" },
			{ "{8BB2217D-0F2D-49D1-97BC-3654ED321F3B}", "ASP.NET5" },
			{ "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}", "C++Project" },
			{ "{930C7802-8A8C-48F9-8165-68863BCCD9DD}", "WixProject" },
			{ "{978C614F-708E-4E1A-B201-565925725DBA}", "DeploymentSetup" },
			{ "{A1591282-1198-4647-A2B1-27E5FF5F6F3B}", "Silverlight" },
			{ "{A5A43C5B-DE2A-4C0C-9213-0A381AF9435A}", "UniversalWinClassLib" },
			{ "{A860303F-1F3F-4691-B57E-529FC101A107}", "VSToolsApplications(VSTA)" },
			{ "{A9ACE9BB-CECE-4E62-9AA4-C7E7C5BD2124}", "Database" },
			{ "{AB322303-2255-48EF-A496-5904EB18DA55}", "DeploymentSmartDeviceCab" },
			{ "{B69E3092-B931-443C-ABE7-7E7B65F2A37F}", "MicroFW" },
			{ "{BAA0C2D2-18E2-41B9-852F-F413020CAA33}", "VSToolsOffice(VSTO)" },
			{ "{BBD0F5D1-1CC4-42fd-BA4C-A96779C64378}", "SynergexProject" },
			{ "{BC8A1FFA-BEE3-4634-8014-F334798102B3}", "WinStoreProject" },
			{ "{BF6F8E12-879D-49E7-ADF0-5503146B24B8}", "C#Dynamics2012AX-AOT" },
			{ "{C089C8C0-30E0-4E22-80C0-CE093F111A43}", "WinPhone8/8.1App(C#)" },
			{ "{C252FEB5-A946-4202-B1D4-9916A0590387}", "VisualDbTools" },
			{ "{CB4CE8C6-1BDB-4DC7-A4D3-65A1999772F8}", "Legacy(2003)SmartDevice(VB.NET)" },
			{ "{D399B71A-8929-442a-A9AC-8BEC78BB2433}", "XNA(Zune)" },
			{ "{D59BE175-2ED0-4C54-BE3D-CDAA9F3214C8}", "Workflow(VB.NET)" },
			{ "{DB03555F-0C8B-43BE-9FF9-57896B3C5E56}", "WinPhone8/8.1App(VB.NET)" },
			{ "{E24C65DC-7377-472B-9ABA-BC803B73C61A}", "WebSiteProject" },
			{ "{E3E379DF-F4C6-4180-9B81-6769533ABE47}", "ASP.NET(MVC4.0)" },
			{ "{E53F8FEA-EAE0-44A6-8774-FFD645390401}", "ASP.NET(MVC3.0)" },
			{ "{E6FDF86B-F3D1-11D4-8576-0002A516ECE8}", "J#" },
			{ "{EC05E597-79D4-47f3-ADA0-324C4F7C7484}", "SharePoint(VB.NET)" },
			{ "{ECD6D718-D1CF-4119-97F3-97C25A0DFBF9}", "LightSwitchProject" },
			{ "{edcc3b85-0bad-11db-bc1a-00112fde8b61}", "NemerleProject" },
			{ "{EFBA0AD7-5A72-4C68-AF49-83D382785DCF}", "Xamarin.Android/MonoAndroid" },
			{ "{F135691A-BF7E-435D-8960-F99683D2D49C}", "DistributedSystem" },
			{ "{F2A71F9B-5D33-465A-A702-920D77279786}", "F#Project" },
			{ "{F5B4F3BC-B597-4E2B-B552-EF5D8A32436F}", "MonoTouchBinding" },
			{ "{F85E285D-A4E0-4152-9332-AB1D724D3325}", "ASP.NET(MVC2.0)" },
			{ "{F8810EC1-6754-47FC-A15F-DFABD2E3FA90}", "SharePointWorkflow" },
			{ "{FBB4BD86-BF63-432a-A6FB-6CF3A1288F83}", "InstallShieldLimitedEdition" },
			{ ".nuget", "SettingsNuget" }
			*/
	};



#endregion Static Methods

}
