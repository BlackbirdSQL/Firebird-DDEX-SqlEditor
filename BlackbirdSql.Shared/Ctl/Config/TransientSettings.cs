﻿// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys.Interfaces;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Events;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Interfaces;



namespace BlackbirdSql.Shared.Ctl.Config;


// =============================================================================================================
//										TransientSettings Class
//
/// <summary>
/// A Transient Options Model that inherits as a copy from the Persistent models but can be used as a volatile
/// AutomationObject.
/// </summary>
// =============================================================================================================
public class TransientSettings : PersistentSettings, IBsEditorTransientSettings, IBsTransientSettings, ICloneable
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - TransientSettings
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// Protected singleton .ctor
	/// </summary>
	protected TransientSettings() : base(true)
	{
		_ExecOptions = new BitArray(16);
	}

	protected TransientSettings(BitArray execOptions) : base(true)
	{
		_ExecOptions = execOptions.Clone() as BitArray;
	}



	public static TransientSettings CreateInstance()
	{
		return new()
		{
			_LiveStore = new(SettingsStore)
		};
	}



	public static TransientSettings CreateInstance(IDictionary<string, object> liveStore, BitArray execOptions)
	{
		return new(execOptions)
		{
			_LiveStore = new(liveStore)
		};
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields
	// =========================================================================================================


	private readonly BitArray _ExecOptions;
	protected Dictionary<string, object> _LiveStore;
	

	#endregion Fields




	// =========================================================================================================
	#region Property Accessors - TransientSettings
	// =========================================================================================================


	public override object this[string name]
	{
		get
		{
			lock (_LockObject)
			{
				if (!_LiveStore.TryGetValue(name, out object value))
					return null;

				return value;
			}
		}

		set
		{
			lock (_LockObject)
				_LiveStore[name] = value;
		}
	}





	// Editor GeneralSettingsModel
	public new bool EditorEnableIntellisense
	{
		get { return (bool)this["EditorGeneralEnableIntellisense"]; }
		set { this["EditorGeneralEnableIntellisense"] = value; }
	}

	public new bool EditorPromptSave
	{
		get { return (bool)this["EditorGeneralPromptSave"]; }
		set { this["EditorGeneralPromptSave"] = value; }
	}

	public new bool EditorExecuteQueryOnOpen
	{
		get { return (bool)this["EditorGeneralExecuteQueryOnOpen"]; }
		set { this["EditorGeneralExecuteQueryOnOpen"] = value; }
	}

	public new EnLanguageService EditorLanguageService
	{
		get { return (EnLanguageService)this["EditorGeneralLanguageService"]; }
		set { this["EditorGeneralLanguageService"] = value; }
	}
	public new bool EditorTtsDefault
	{
		get { return (bool)this["EditorGeneralTtsDefault"]; }
		set { this["EditorGeneralTtsDefault"] = value; }
	}


	// Editor ContextSettingsModel
	public new EnStatusBarPosition EditorContextStatusBarPosition
	{
		get { return (EnStatusBarPosition)this["EditorContextStatusBarPosition"]; }
		set { this["EditorContextStatusBarPosition"] = value; }
	}


	// Editor TabAndStatusBarSettingsModel
	public new EnExecutionTimeMethod EditorStatusBarExecutionTimeMethod
	{
		get { return (EnExecutionTimeMethod)this["EditorStatusBarExecutionTimeMethod"]; }
		set { this["EditorStatusBarExecutionTimeMethod"] = value; }
	}
	public new bool EditorStatusBarIncludeDatabaseName
	{
		get { return (bool)this["EditorStatusBarIncludeDatabaseName"]; }
		set { this["EditorStatusBarIncludeDatabaseName"] = value; }
	}
	public new bool EditorStatusBarIncludeLoginName
	{
		get { return (bool)this["EditorStatusBarIncludeLoginName"]; }
		set { this["EditorStatusBarIncludeLoginName"] = value; }
	}
	public new bool EditorStatusBarIncludeRowCount
	{
		get { return (bool)this["EditorStatusBarIncludeRowCount"]; }
		set { this["EditorStatusBarIncludeRowCount"] = value; }
	}
	public new bool EditorStatusBarIncludeServerName
	{
		get { return (bool)this["EditorStatusBarIncludeServerName"]; }
		set { this["EditorStatusBarIncludeServerName"] = value; }
	}
	public new Color EditorStatusBarBackgroundColor
	{
		get { return (Color)this["EditorStatusBarBackgroundColor"]; }
		set { this["EditorStatusBarBackgroundColor"] = value; }
	}
	public new bool EditorStatusTabTextIncludeDatabaseName
	{
		get { return (bool)this["EditorStatusTabTextIncludeDatabaseName"]; }
		set { this["EditorStatusTabTextIncludeDatabaseName"] = value; }
	}
	public new bool EditorStatusTabTextIncludeLoginName
	{
		get { return (bool)this["EditorStatusTabTextIncludeLoginName"]; }
		set { this["EditorStatusTabTextIncludeLoginName"] = value; }
	}
	public new bool EditorStatusTabTextIncludeFileName
	{
		get { return (bool)this["EditorStatusTabTextIncludeFileName"]; }
		set { this["EditorStatusTabTextIncludeFileName"] = value; }
	}
	public new bool EditorStatusTabTextIncludeServerName
	{
		get { return (bool)this["EditorStatusTabTextIncludeServerName"]; }
		set { this["EditorStatusTabTextIncludeServerName"] = value; }
	}


	// Editor ExecutionSettingsModel
	public new int EditorExecutionSetRowCount
	{
		get { return (int)this["EditorExecutionGeneralSetRowCount"]; }
		set { this["EditorExecutionGeneralSetRowCount"] = value; }
	}

	public new EnBlobSubType EditorExecutionSetBlobDisplay
	{
		get { return (EnBlobSubType)this["EditorExecutionGeneralSetBlobDisplay"]; }
		set { this["EditorExecutionGeneralSetBlobDisplay"] = value; }
	}

	public new int EditorExecutionTimeout
	{
		get { return (int)this["EditorExecutionGeneralExecutionTimeout"]; }
		set { this["EditorExecutionGeneralExecutionTimeout"] = value; }
	}

	public new string EditorExecutionBatchSeparator
	{
		get
		{
			string value = (string)this["EditorExecutionGeneralBatchSeparator"];
			value = value.Replace(" ", "");

			if (value == "")
				value = ";";

			return value;
		}
		set
		{
			this["EditorExecutionGeneralBatchSeparator"] = value;
		}
	}


	// Editor ExecutionAdvancedSettingsModel
	public new bool EditorExecutionSetCount
	{
		get { return (bool)this["EditorExecutionAdvancedSetCount"]; }
		set { this["EditorExecutionAdvancedSetCount"] = value; }
	}
	public new bool EditorExecutionSetNoExec
	{
		get { return (bool)this["EditorExecutionAdvancedSetNoExec"]; }
		set { this["EditorExecutionAdvancedSetNoExec"] = value; }
	}
	public new bool EditorExecutionSetShowplanText
	{
		get { return (bool)this["EditorExecutionAdvancedSetShowplanText"]; }
		set { this["EditorExecutionAdvancedSetShowplanText"] = value; }
	}
	public new bool EditorExecutionSetPlanXml
	{
		get { return (bool)this["EditorExecutionAdvancedSetPlanXml"]; }
		set { this["EditorExecutionAdvancedSetPlanXml"] = value; }
	}
	public new bool EditorExecutionSetParseOnly
	{
		get { return (bool)this["EditorExecutionAdvancedSetParseOnly"]; }
		set { this["EditorExecutionAdvancedSetParseOnly"] = value; }
	}
	public new bool EditorExecutionSetConcatenationNull
	{
		get { return (bool)this["EditorExecutionAdvancedSetConcatenationNull"]; }
		set { this["EditorExecutionAdvancedSetConcatenationNull"] = value; }
	}
	public new bool EditorExecutionSetBail
	{
		get { return (bool)this["EditorExecutionAdvancedSetBail"]; }
		set { this["EditorExecutionAdvancedSetBail"] = value; }
	}
	public new bool EditorExecutionSetPlanText
	{
		get { return (bool)this["EditorExecutionAdvancedSetPlanText"]; }
		set { this["EditorExecutionAdvancedSetPlanText"] = value; }
	}
	public new bool EditorExecutionSetStats
	{
		get { return (bool)this["EditorExecutionAdvancedSetStats"]; }
		set { this["EditorExecutionAdvancedSetStats"] = value; }
	}
	public new bool EditorExecutionSetStatisticsIO
	{
		get { return (bool)this["EditorExecutionAdvancedSetStatisticsIO"]; }
		set { this["EditorExecutionAdvancedSetStatisticsIO"] = value; }
	}
	public new bool EditorExecutionSetWarnings
	{
		get { return (bool)this["EditorExecutionAdvancedSetWarnings"]; }
		set { this["EditorExecutionAdvancedSetSetWarnings"] = value; }
	}
	public new IsolationLevel EditorExecutionIsolationLevel
	{
		get { return (IsolationLevel)this["EditorExecutionAdvancedIsolationLevel"]; }
		set { this["EditorExecutionAdvancedIsolationLevel"] = value; }
	}
	public new EnDeadlockPriority EditorExecutionDeadlockPriority
	{
		get { return (EnDeadlockPriority)this["EditorExecutionAdvancedDeadlockPriority"]; }
		set { this["EditorExecutionAdvancedDeadlockPriority"] = value; }
	}
	public new bool EditorExecutionDeadlockPriorityLow
	{
		get { return EditorExecutionDeadlockPriority == EnDeadlockPriority.Low; }
		set { EditorExecutionDeadlockPriority = value ? EnDeadlockPriority.Low : EnDeadlockPriority.Normal; }
	}
	public new int EditorExecutionLockTimeout
	{
		get { return (int)this["EditorExecutionAdvancedLockTimeout"]; }
		set { this["EditorExecutionAdvancedLockTimeout"] = value; }
	}
	public new int EditorExecutionCostLimit
	{
		get { return (int)this["EditorExecutionAdvancedCostLimit"]; }
		set { this["EditorExecutionAdvancedCostLimit"] = value; }
	}
	public new bool EditorExecutionSuppressHeaders
	{
		get { return (bool)this["EditorExecutionAdvancedSuppressHeaders"]; }
		set { this["EditorExecutionAdvancedSuppressHeaders"] = value; }
	}
	public new bool EditorExecutionDisconnectOnCompletion
	{
		get { return (bool)this["EditorExecutionAdvancedDisconnectOnCompletion"]; }
		set { this["EditorExecutionAdvancedDisconnectOnCompletion"] = value; }
	}


	// Editor ResultsSettingsModel
	public new EnSqlOutputMode EditorResultsOutputMode
	{
		get { return (EnSqlOutputMode)this["EditorResultsGeneralOutputMode"]; }
		set { this["EditorResultsGeneralOutputMode"] = value; }
	}
	public new string EditorResultsDirectory
	{
		get { return (string)this["EditorResultsGeneralDirectory"]; }
		set { this["EditorResultsGeneralDirectory"] = value; }
	}
	public new bool EditorResultsPlaySounds
	{
		get { return (bool)this["EditorResultsGeneralPlaySounds"]; }
		set { this["EditorResultsGeneralPlaySounds"] = value; }
	}

	// Grid results options.
	public new bool EditorResultsGridOutputQuery
	{
		get { return (bool)this["EditorResultsGridOutputQuery"]; }
		set { this["EditorResultsGridOutputQuery"] = value; }
	}

	public new bool EditorResultsGridSingleTab
	{
		get { return (bool)this["EditorResultsGridSingleTab"]; }
		set { this["EditorResultsGridSingleTab"] = value; }
	}

	public new bool EditorResultsGridSaveIncludeHeaders
	{
		get { return (bool)this["EditorResultsGridSaveIncludeHeaders"]; }
		set { this["EditorResultsGridSaveIncludeHeaders"] = value; }
	}
	public new bool EditorResultsGridCsvQuoteStringsCommas
	{
		get { return (bool)this["EditorResultsGridCsvQuoteStringsCommas"]; }
		set { this["EditorResultsGridCsvQuoteStringsCommas"] = value; }
	}
	public new bool EditorResultsGridDiscardResults
	{
		get { return (bool)this["EditorResultsGridDiscardResults"]; }
		set { this["EditorResultsGridDiscardResults"] = value; }
	}
	public new bool EditorResultsGridSeparateTabs
	{
		get { return (bool)this["EditorResultsGridSeparateTabs"]; }
		set { this["EditorResultsGridSeparateTabs"] = value; }
	}
	public new bool EditorResultsGridSwitchToResults
	{
		get { return (bool)this["EditorResultsGridSwitchToResults"]; }
		set { this["EditorResultsGridSwitchToResults"] = value; }
	}
	public new int EditorResultsGridMaxCharsPerColumnStd
	{
		get { return (int)this["EditorResultsGridMaxCharsPerColumnStd"]; }
		set { this["EditorResultsGridMaxCharsPerColumnStd"] = value; }
	}
	public new int EditorResultsGridMaxCharsPerColumnXml
	{
		get { return (int)this["EditorResultsGridMaxCharsPerColumnXml"]; }
		set { this["EditorResultsGridMaxCharsPerColumnXml"] = value; }
	}

	// Text results options.
	public new bool EditorResultsTextIncludeHeaders
	{
		get { return (bool)this["EditorResultsTextIncludeHeaders"]; }
		set { this["EditorResultsTextIncludeHeaders"] = value; }
	}
	public new bool EditorResultsTextOutputQuery
	{
		get { return (bool)this["EditorResultsTextOutputQuery"]; }
		set { this["EditorResultsTextOutputQuery"] = value; }
	}
	public new bool EditorResultsTextScrollingResults
	{
		get { return (bool)this["EditorResultsTextScrollingResults"]; }
		set { this["EditorResultsTextScrollingResults"] = value; }
	}
	public new bool EditorResultsTextAlignRightNumerics
	{
		get { return (bool)this["EditorResultsTextAlignRightNumerics"]; }
		set { this["EditorResultsTextAlignRightNumerics"] = value; }
	}
	public new bool EditorResultsTextDiscardResults
	{
		get { return (bool)this["EditorResultsTextDiscardResults"]; }
		set { this["EditorResultsTextDiscardResults"] = value; }
	}
	public new int EditorResultsTextMaxCharsPerColumnStd
	{
		get { return (int)this["EditorResultsTextMaxCharsPerColumnStd"]; }
		set { this["EditorResultsTextMaxCharsPerColumnStd"] = value; }
	}
	public new bool EditorResultsTextSeparateTabs
	{
		get { return (bool)this["EditorResultsTextSeparateTabs"]; }
		set { this["EditorResultsTextSeparateTabs"] = value; }
	}
	public new bool EditorResultsTextSwitchToResults
	{
		get { return (bool)this["EditorResultsTextSwitchToResults"]; }
		set { this["EditorResultsTextSwitchToResults"] = value; }
	}
	public new EnSqlOutputFormat EditorResultsTextOutputFormat
	{
		get { return (EnSqlOutputFormat)this["EditorResultsTextOutputFormat"]; }
		set { this["EditorResultsTextOutputFormat"] = value; }
	}
	public new char EditorResultsTextDelimiter
	{
		get
		{
			return GetTextDelimiter(EditorResultsTextOutputFormat, this["EditorResultsTextDelimiter"]);
		}
		set
		{
			switch (value)
			{
				case '\0':
					EditorResultsTextOutputFormat = EnSqlOutputFormat.ColAligned;
					break;
				case ',':
					EditorResultsTextOutputFormat = EnSqlOutputFormat.CommaDelim;
					break;
				case '\t':
					EditorResultsTextOutputFormat = EnSqlOutputFormat.TabDelim;
					break;
				case ' ':
					EditorResultsTextOutputFormat = EnSqlOutputFormat.SpaceDelim;
					break;
				default:
					EditorResultsTextOutputFormat = EnSqlOutputFormat.Custom;
					this["EditorResultsTextDelimiter"] = value.ToString();
					break;
			}
		}
	}



	public EnSqlExecutionType ExecutionType
	{
		get
		{
			if (!_ExecOptions[0] && !_ExecOptions[1])
				return EnSqlExecutionType.QueryOnly;

			if (_ExecOptions[0] && !_ExecOptions[1])
				return EnSqlExecutionType.QueryWithPlan;

			return EnSqlExecutionType.PlanOnly;

		}
		set
		{
			if (value == EnSqlExecutionType.QueryOnly)
			{
				_ExecOptions[0] = false;
				_ExecOptions[1] = false;
			}
			else if (value == EnSqlExecutionType.QueryWithPlan)
			{
				_ExecOptions[0] = true;
				_ExecOptions[1] = false;
			}
			else
			{
				_ExecOptions[0] = false;
				_ExecOptions[1] = true;
			}
		}
	}


	public bool WithActualPlan
	{
		get { return _ExecOptions[3]; }
		set { _ExecOptions[3] = value; }
	}

	public bool WithClientStats
	{
		get { return _ExecOptions[4]; }
		set { _ExecOptions[4] = value; }
	}

	public bool WithProfiling
	{
		get { return _ExecOptions[5]; }
		set { _ExecOptions[5] = value; }
	}


	public bool WithNoExec
	{
		get { return _ExecOptions[6]; }
		set { _ExecOptions[6] = value; }
	}


	public bool WithStatisticsTime
	{
		get { return _ExecOptions[7]; }
		set { _ExecOptions[7] = value; }
	}

	public bool WithStatisticsIO
	{
		get { return _ExecOptions[8]; }
		set { _ExecOptions[8] = value; }
	}

	public bool WithStatisticsProfile
	{
		get { return _ExecOptions[9]; }
		set { _ExecOptions[9] = value; }
	}


	public bool TtsEnabled
	{
		get { return _ExecOptions[10]; }
		set { _ExecOptions[10] = value; }
	}


	public bool SuppressProviderMessageHeaders
	{
		get { return _ExecOptions[12]; }
		set { _ExecOptions[12] = value; }
	}


	public bool HasExecutionPlan =>
		ExecutionType == EnSqlExecutionType.PlanOnly || ExecutionType == EnSqlExecutionType.QueryWithPlan;


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - TransientSettings
	// =========================================================================================================


	public virtual object Clone()
	{
		lock (_LockObject)
			return CreateInstance(_LiveStore, _ExecOptions);
	}



	public static bool IsSupported(object property)
	{
		Type type = property.GetType();

		string cmd = SqlCmdResources.ResourceManager.GetString(type.Name);

		return cmd == null;
	}



	public bool PropertyExists(string name)
	{
		lock (_LockObject)
			return _LiveStore.ContainsKey(name);
	}


	protected override void Initialize()
	{
	}

	/// <summary>
	/// Adds the extension's SettingsSavedDelegate to a package settings models SettingsSavedEvents.
	/// Only implemented by packages that have settings models, ie. are options providers.
	/// </summary>
	public override void RegisterSettingsEventHandlers(IBsPersistentSettings.SettingsSavedDelegate onSettingsSavedDelegate)
	{
	}


	/// <summary>
	/// Only implemented by packages that have settings models. Whenever a package
	/// settings model is saved it fires the extension's OnSettingsSaved event.
	/// That event handler then requests each package to populate SettingsEventArgs
	/// if it has settings relevant to the model.
	/// PopulateSettingsEventArgs is also called on loading by the extension without
	/// a specific model specified for a universal request for settings.
	/// </summary>
	public override bool PopulateSettingsEventArgs(ref PropagateSettingsEventArgs args)
	{
		return false;
	}


	#endregion Methods




	// =========================================================================================================
	#region Event handlers - TransientSettings
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Settings saved event handler - only the final descendent class implements this.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public override void OnSettingsSaved(object sender)
	{
	}


	#endregion Event handlers

}