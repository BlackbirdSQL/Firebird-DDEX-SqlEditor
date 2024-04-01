
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Model.Enums;
using EnvDTE;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;



namespace BlackbirdSql.Core;

[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Using Diag.ThrowIfNotOnUIThread()")]


// =========================================================================================================
//												UnsafeCmd Class
//
/// <summary>
/// Central location for implementation of unsafe utility static methods. 
/// </summary>
// =========================================================================================================
public abstract class UnsafeCmd
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


	#endregion Property Accessors





	// =========================================================================================================
	#region Static Methods - Cmd
	// =========================================================================================================


	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	protected static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);



	/// <summary>
	/// <see cref="ErrorHandler.Succeeded"/> token.
	/// </summary>
	protected static bool __(int hr) => ErrorHandler.Succeeded(hr);



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the app.config <see cref="ProjectItem"/> of a <see cref="Project"/>
	/// </summary>
	/// <param name="project"></param>
	/// <returns>true if app.config was updated else false</returns>
	// ---------------------------------------------------------------------------------
	public static ProjectItem GetAppConfigProjectItem(Project project, bool createIfNotFound = false)
	{
		Diag.ThrowIfNotOnUIThread();

		ProjectItem config = null;

		try
		{
			foreach (ProjectItem item in project.ProjectItems)
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
			Diag.Dug(ex);
			return null;
		}

		if (createIfNotFound && config == null)
		{
			FileInfo info = new FileInfo(project.FullName);
			string filename = info.Directory.FullName + "\\App.config";
			StreamWriter sw = File.CreateText(filename);

			string xml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
</configuration>";

			sw.Write(xml);
			sw.Close();

			config = project.ProjectItems.AddFromFile(filename);
		}

		return config;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Identifies whether or not a project is a CPS project
	/// </summary>
	/// <param name="hierarchy"></param>
	/// <returns>true if project is CPS</returns>
	// ---------------------------------------------------------------------------------
	public static bool IsCpsProject(IVsHierarchy hierarchy)
	{
		Requires.NotNull(hierarchy, "hierarchy");
		return hierarchy.IsCapabilityMatch("CPS");
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks wether the project is a valid executable output type that requires
	/// configuration of the app.config
	/// </summary>
	/// <param name="project"></param>
	/// <returns>
	/// True if the project is a valid C#/VB executable project else false.
	/// </returns>
	/// <remarks>
	/// We're not going to worry about anything but C# and VB non=CSP projects
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static bool IsValidExecutableProjectType(Project project, bool validateCps = true)
	{
		Diag.ThrowIfNotOnUIThread();

		// We're only supporting C# and VB projects for this - a dict list is at the end of this class
		if (Kind(project.Kind) != "VbProject" && Kind(project.Kind) != "C#Project")
		{
			return false;
		}

		bool result = false;


		if (validateCps)
		{
			// Don't process CPS projects

			ApcManager.Instance.VsSolution.GetProjectOfUniqueName(project.UniqueName, out IVsHierarchy hierarchy);


			if (UnsafeCmd.IsCpsProject(hierarchy))
				return false;
		}

		int outputType = int.MaxValue;

		Property property = null;

		if (project.Properties != null && project.Properties.Count > 0)
		{
			bool invalidOutputType = true;

			try
			{
				property = project.Properties.Item("OutputType");

				if (property != null && property.Value != null)
				{
					if (property.Value is int ival)
					{
						invalidOutputType = false;
						outputType = ival;
					}
					else if (property.Value is long lval)
					{
						outputType = (int)lval;
					}
					else if (property.Value is string sval)
					{
						if (int.TryParse(sval, out int value))
							outputType = value;
					}
				}
			}
			catch { }

			if (invalidOutputType)
			{
				try
				{
					// Temporary debug message.
					Tracer.Warning(typeof(UnsafeCmd), "IsValidExecutableProjectType()", "Project: {0}, OutputType property {1}, Type: {2}, Value: {3}.",
						project.Name, property == null ? "does not exist" : "exists",
						property != null && property.Value != null ? property.Value.GetType().Name : "NULL Type",
						property?.Value);
				}
				catch (Exception ex)
				{
					Diag.Dug(ex);
				}
			}

		}


		if (outputType < 3)
			result = true;


		return result;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the descriptive name for a DTE object Kind guid string
	/// </summary>
	/// <param name="kind"></param>
	/// <returns>The descriptive name from <see cref="ProjectGuids"/></returns>
	// ---------------------------------------------------------------------------------
	public static string Kind(string kind)
	{
		if (!ProjectGuids.TryGetValue(kind, out string name))
			name = kind;

		return name;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns True if project is external files project.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool IsVirtualProjectKind(IVsHierarchy hierarchy)
	{
		int hresult = hierarchy.GetGuidProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_TypeGuid, out Guid guid);

		if (!__(hresult))
			return false;

		return IsVirtualProjectKind(guid);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns True if project is external files project.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool IsVirtualProjectKind(string kind)
	{
		return VSConstants.ItemTypeGuid.VirtualFolder_string.Trim(['{', '}']).Equals(kind.Trim(['{', '}']), StringComparison.OrdinalIgnoreCase);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns True if project is external files project.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool IsVirtualProjectKind(Guid clsid)
	{
		return clsid == VSConstants.GUID_ItemType_VirtualFolder;
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns True if project is misc files project.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool IsMiscFilesProject(IVsHierarchy hierarchy)
	{
		int hresult = hierarchy.GetGuidProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ProjectIDGuid, out Guid clsid);

		if (!__(hresult))
			return false;

		return IsMiscFilesProject(clsid);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns True if project is misc files project.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool IsMiscFilesProject(string kind)
	{
		return kind.Trim(['{', '}']).Equals(VSConstants.CLSID.MiscellaneousFilesProject_string.Trim(['{', '}']),
			StringComparison.OrdinalIgnoreCase);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns True if project is misc files project.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool IsMiscFilesProject(Guid clsid)
	{
		return clsid == VSConstants.CLSID.MiscellaneousFilesProject_guid;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns True if project is VB or C#.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool IsProjectKind(string kind)
	{
		if (!ProjectGuids.TryGetValue(kind, out string name))
			return false;

		return name.EndsWith("Project");
	}

	public static string GetProjectKind(string name)
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
	/// DTE object Kind guid string descriptive name dictionary
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static readonly Dictionary<string, string> ProjectGuids =
		new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
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
