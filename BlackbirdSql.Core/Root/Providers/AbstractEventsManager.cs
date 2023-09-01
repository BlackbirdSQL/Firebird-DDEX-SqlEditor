// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)



using System;
using System.Collections.Generic;
using BlackbirdSql.Core.Interfaces;

using EnvDTE;

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskStatusCenter;




namespace BlackbirdSql.Core.Providers;



// =========================================================================================================
//										AbstractEventsManager Class
//
/// <summary>
/// Base class for and extension or service ide event handling manager.
/// </summary>
/// <remarks>
/// Each service or extension that handles ide events should derive an events manager from
/// AbstractEventsManager. All solution, rdt and selection events are routed through PackageController.
/// The events manager can handle an ide event by hooking onto Controller.On[event]. 
/// </remarks>
// =========================================================================================================
public abstract class AbstractEventsManager : IBEventsManager
{

	#region Variables - BlackbirdSqlDdexExtension


	private readonly IBPackageController _Controller;

	protected string _TaskHandlerTaskName = "Task";
	protected TaskProgressData _ProgressData = default;
	protected ITaskHandler _TaskHandler = null;



	#endregion Variables





	// =========================================================================================================
	#region Property accessors - AbstractEventsManager
	// =========================================================================================================


	public string UserDataDirectory => _Controller.UserDataDirectory;

	public IBPackageController Controller => _Controller;
	public IBAsyncPackage DdexPackage => _Controller.DdexPackage;
	public DTE Dte => Controller.Dte;
	public IVsSolution DteSolution => _Controller.DteSolution;
	public IVsMonitorSelection SelectionMonitor => _Controller.SelectionMonitor;
	public Globals SolutionGlobals => _Controller.SolutionGlobals;
	public IBGlobalsAgent Uig => _Controller.Uig;




	/// <summary>
	/// The name of the running task if the object is currently using the task handler.
	/// </summary>
	public string TaskHandlerTaskName => _TaskHandlerTaskName;


	#endregion Property accessors





	// =========================================================================================================
	#region Constructors / Destructors - AbstractEventsManager
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Private singleton .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	public AbstractEventsManager(IBPackageController controller)
	{
		_Controller = controller;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Placeholder for final instance descendent class CreateInstance
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static AbstractEventsManager CreateInstance(IBPackageController controller)
	{
		// Format
		// return _Instance = new(controller);

		NotImplementedException ex = new ("Static CreateInstance may only be called from the final class");
		Diag.Dug(ex);
		throw ex;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// AbstractEventsManager disposal
	/// </summary>
	/// <remarks>
	/// Example
	/// <code>_Controller.OnExampleEvent -= OnExample;</code>
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public abstract void Dispose();


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Methods - AbstractEventsManager
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Initializes the event manager
	/// validation.
	/// </summary>
	/// Example
	/// <code>_Controller.OnExampleEvent += OnExample;</code>
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public abstract void Initialize();



	#endregion Methods



	// =========================================================================================================
	#region Utility Methods - AbstractEventsManager
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Provides a callback to the client's ProgressData.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public TaskProgressData GetProgressData()
	{
		return _ProgressData;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Moves back onto the UI thread and updates the IDE task handler progress bar
	/// with updated TaskHandlerTaskName information.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public ITaskHandler GetTaskHandler()
	{
		return _TaskHandler;
	}





	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the descriptive name for a DTE object Kind guid string
	/// </summary>
	/// <param name="kind"></param>
	/// <returns>The descriptive name from <see cref="ProjectGuids"/></returns>
	// ---------------------------------------------------------------------------------
	protected string Kind(string kind)
	{
		if (!ProjectGuids.TryGetValue(kind, out string name))
			name = kind;

		return name;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// DTE object Kind guid string descriptive name dictionary
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected readonly Dictionary<string, string> ProjectGuids = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		{ "{2150E333-8FDC-42A3-9474-1A3956D46DE8}", "SolutionFolder" },
		{ "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}", "ProjectFolder" },
		{ "{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}", "PhysicalFolder" },
		{ "{66A2671F-8FB5-11D2-AA7E-00C04F688DDE}", "MiscItem" },
		{ "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}", "VbProject" },
		{ "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}", "C#Project" }
		/* The above the only one's we care about ATM
		{ "{06A35CCD-C46D-44D5-987B-CF40FF872267}", "DeploymentMergeModule" },
		{ "{14822709-B5A1-4724-98CA-57A101D1B079}", "Workflow(C#)" },
		{ "{20D4826A-C6FA-45DB-90F4-C717570B9F32}", "Legacy(2003)SmartDevice(C#)" },
		{ "{262852C6-CD72-467D-83FE-5EEB1973A190}", "JsProject" },
		{ "{2DF5C3F4-5A5F-47a9-8E94-23B4456F55E2}", "XNA(XBox)" },
		{ "{32F31D43-81CC-4C15-9DE6-3FC5453562B6}", "WorkflowFoundation" },
		{ "{349C5851-65DF-11DA-9384-00065B846F21}", "WebApplicationProject" },
		{ "{3AC096D0-A1C2-E12C-1390-A8335801FDAB}", "Test" },
		{ "{3AE79031-E1BC-11D0-8F78-00A0C9110057}", "SolutionExplorer"},
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

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Moves back onto the UI thread and updates the IDE task handler progress bar
	/// with project update information.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public virtual bool TaskHandlerProgress(string text)
	{
		if (_ProgressData.PercentComplete == null)
		{
			_ProgressData.PercentComplete = 0;
			if (_TaskHandler != null)
				Diag.TaskHandlerProgress(this, "Started");
		}

		Diag.TaskHandlerProgress(this, text);

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Moves back onto the UI thread and updates the IDE task handler progress bar.
	/// </summary>
	/// <param name="progress">The % completion of TaskHandlerTaskName.</param>
	/// <param name="elapsed">The time taken to complete the stage.</param>
	// ---------------------------------------------------------------------------------
	public virtual bool TaskHandlerProgress(int progress, int elapsed)
	{
		bool completed = false;
		string text;

		if (progress == 0)
		{
			text = "Started";
		}
		else if (progress == 100)
		{
			completed = true;
			text = $"Completed. Validation took {elapsed}ms.";
		}
		else if (_TaskHandler.UserCancellation.IsCancellationRequested)
		{
			completed = true;
			text = $"Cancelled. {progress}% completed. Validation took {elapsed}ms.";
		}
		else
		{
			text = $"{progress}% completed after {elapsed}ms.";
		}

		_ProgressData.PercentComplete = progress;

		Diag.TaskHandlerProgress(this, text, completed);

		return true;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates the VisualStudio status bar.
	/// </summary>
	/// <param name="message">Thye status bar display text.</param>
	/// <param name="complete">Tags this message as a completion message.</param>
	/// <returns></returns>
	// ---------------------------------------------------------------------------------
	public virtual bool UpdateStatusBar(string message, bool complete = false)
	{
		return Diag.UpdateStatusBar(message, complete);
	}


	#endregion Utility Methods




	// =========================================================================================================
	#region IVs Events Implementation and Event handling - AbstractEventsManager
	// =========================================================================================================


	// Example
	// public int OnExample(int arg)


	#endregion IVs Events Implementation and Event handling


}