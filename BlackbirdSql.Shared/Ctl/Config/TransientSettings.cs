// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;



namespace BlackbirdSql.Shared.Ctl.Config;


// =============================================================================================================
//										TransientSettings Class
//
/// <summary>
/// A Transient Options Model that inherits as a copy from the Persistent models but can be used as a volatile
/// AutomationObject.
/// </summary>
// =============================================================================================================
public class TransientSettings : PersistentSettings, IBsEditorTransientSettings, IBsSettingsProvider, ICloneable
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

	protected TransientSettings(IDictionary<string, object> transientStore, BitArray execOptions) : base(transientStore)
	{
		_ExecOptions = execOptions.Clone() as BitArray;
	}



	public static TransientSettings CreateInstance()
	{
		return new();
	}



	public static TransientSettings CreateInstance(IDictionary<string, object> transientStore, BitArray execOptions)
	{
		return new(transientStore, execOptions);
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields
	// =========================================================================================================


	private readonly BitArray _ExecOptions;


	#endregion Fields




	// =========================================================================================================
	#region Property Accessors - TransientSettings
	// =========================================================================================================


	// Editor GeneralSettingsModel
	public new EnIntellisensePolicy EditorIntellisensePolicy
	{
		get { return (EnIntellisensePolicy)this["EditorGeneralIntellisensePolicy"]; }
		set { this["EditorGeneralIntellisensePolicy"] = value; }
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
	public int EditorExecutionSetRowCount
	{
		get { return (int)this["EditorExecutionGeneralSetRowCount"]; }
		set { this["EditorExecutionGeneralSetRowCount"] = value; }
	}

	public EnBlobSubType EditorExecutionSetBlobDisplay
	{
		get { return (EnBlobSubType)this["EditorExecutionGeneralSetBlobDisplay"]; }
		set { this["EditorExecutionGeneralSetBlobDisplay"] = value; }
	}

	public int EditorExecutionTimeout
	{
		get { return (int)this["EditorExecutionGeneralExecutionTimeout"]; }
		set { this["EditorExecutionGeneralExecutionTimeout"] = value; }
	}

	public string EditorExecutionBatchSeparator
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
	public bool EditorExecutionSetCount
	{
		get { return (bool)this["EditorExecutionAdvancedSetCount"]; }
		set { this["EditorExecutionAdvancedSetCount"] = value; }
	}
	public bool EditorExecutionSetNoExec
	{
		get { return (bool)this["EditorExecutionAdvancedSetNoExec"]; }
		set { this["EditorExecutionAdvancedSetNoExec"] = value; }
	}
	public bool EditorExecutionSetShowplanText
	{
		get { return (bool)this["EditorExecutionAdvancedSetShowplanText"]; }
		set { this["EditorExecutionAdvancedSetShowplanText"] = value; }
	}
	public bool EditorExecutionSetPlanXml
	{
		get { return (bool)this["EditorExecutionAdvancedSetPlanXml"]; }
		set { this["EditorExecutionAdvancedSetPlanXml"] = value; }
	}
	public bool EditorExecutionSetParseOnly
	{
		get { return (bool)this["EditorExecutionAdvancedSetParseOnly"]; }
		set { this["EditorExecutionAdvancedSetParseOnly"] = value; }
	}
	public bool EditorExecutionSetConcatenationNull
	{
		get { return (bool)this["EditorExecutionAdvancedSetConcatenationNull"]; }
		set { this["EditorExecutionAdvancedSetConcatenationNull"] = value; }
	}
	public bool EditorExecutionSetBail
	{
		get { return (bool)this["EditorExecutionAdvancedSetBail"]; }
		set { this["EditorExecutionAdvancedSetBail"] = value; }
	}
	public bool EditorExecutionSetPlanText
	{
		get { return (bool)this["EditorExecutionAdvancedSetPlanText"]; }
		set { this["EditorExecutionAdvancedSetPlanText"] = value; }
	}
	public bool EditorExecutionSetStats
	{
		get { return (bool)this["EditorExecutionAdvancedSetStats"]; }
		set { this["EditorExecutionAdvancedSetStats"] = value; }
	}
	public bool EditorExecutionSetStatisticsIO
	{
		get { return (bool)this["EditorExecutionAdvancedSetStatisticsIO"]; }
		set { this["EditorExecutionAdvancedSetStatisticsIO"] = value; }
	}
	public bool EditorExecutionSetWarnings
	{
		get { return (bool)this["EditorExecutionAdvancedSetWarnings"]; }
		set { this["EditorExecutionAdvancedSetSetWarnings"] = value; }
	}
	public IsolationLevel EditorExecutionIsolationLevel
	{
		get { return (IsolationLevel)this["EditorExecutionAdvancedIsolationLevel"]; }
		set { this["EditorExecutionAdvancedIsolationLevel"] = value; }
	}
	public EnDeadlockPriority EditorExecutionDeadlockPriority
	{
		get { return (EnDeadlockPriority)this["EditorExecutionAdvancedDeadlockPriority"]; }
		set { this["EditorExecutionAdvancedDeadlockPriority"] = value; }
	}
	public bool EditorExecutionDeadlockPriorityLow
	{
		get { return EditorExecutionDeadlockPriority == EnDeadlockPriority.Low; }
		set { EditorExecutionDeadlockPriority = value ? EnDeadlockPriority.Low : EnDeadlockPriority.Normal; }
	}
	public int EditorExecutionLockTimeout
	{
		get { return (int)this["EditorExecutionAdvancedLockTimeout"]; }
		set { this["EditorExecutionAdvancedLockTimeout"] = value; }
	}
	public int EditorExecutionCostLimit
	{
		get { return (int)this["EditorExecutionAdvancedCostLimit"]; }
		set { this["EditorExecutionAdvancedCostLimit"] = value; }
	}
	public bool EditorExecutionSuppressHeaders
	{
		get { return (bool)this["EditorExecutionAdvancedSuppressHeaders"]; }
		set { this["EditorExecutionAdvancedSuppressHeaders"] = value; }
	}
	public bool EditorExecutionDisconnectOnCompletion
	{
		get { return (bool)this["EditorExecutionAdvancedDisconnectOnCompletion"]; }
		set { this["EditorExecutionAdvancedDisconnectOnCompletion"] = value; }
	}


	// Editor ResultsSettingsModel
	public EnSqlOutputMode EditorResultsOutputMode
	{
		get { return (EnSqlOutputMode)this["EditorResultsGeneralOutputMode"]; }
		set { this["EditorResultsGeneralOutputMode"] = value; }
	}
	public string EditorResultsDirectory
	{
		get { return (string)this["EditorResultsGeneralDirectory"]; }
		set { this["EditorResultsGeneralDirectory"] = value; }
	}
	public bool EditorResultsPlaySounds
	{
		get { return (bool)this["EditorResultsGeneralPlaySounds"]; }
		set { this["EditorResultsGeneralPlaySounds"] = value; }
	}

	// Grid results options.
	public bool EditorResultsGridOutputQuery
	{
		get { return (bool)this["EditorResultsGridOutputQuery"]; }
		set { this["EditorResultsGridOutputQuery"] = value; }
	}

	public bool EditorResultsGridSingleTab
	{
		get { return (bool)this["EditorResultsGridSingleTab"]; }
		set { this["EditorResultsGridSingleTab"] = value; }
	}

	public bool EditorResultsGridSaveIncludeHeaders
	{
		get { return (bool)this["EditorResultsGridSaveIncludeHeaders"]; }
		set { this["EditorResultsGridSaveIncludeHeaders"] = value; }
	}
	public bool EditorResultsGridCsvQuoteStringsCommas
	{
		get { return (bool)this["EditorResultsGridCsvQuoteStringsCommas"]; }
		set { this["EditorResultsGridCsvQuoteStringsCommas"] = value; }
	}
	public bool EditorResultsGridDiscardResults
	{
		get { return (bool)this["EditorResultsGridDiscardResults"]; }
		set { this["EditorResultsGridDiscardResults"] = value; }
	}
	public bool EditorResultsGridSeparateTabs
	{
		get { return (bool)this["EditorResultsGridSeparateTabs"]; }
		set { this["EditorResultsGridSeparateTabs"] = value; }
	}
	public bool EditorResultsGridSwitchToResults
	{
		get { return (bool)this["EditorResultsGridSwitchToResults"]; }
		set { this["EditorResultsGridSwitchToResults"] = value; }
	}
	public int EditorResultsGridMaxCharsPerColumnStd
	{
		get { return (int)this["EditorResultsGridMaxCharsPerColumnStd"]; }
		set { this["EditorResultsGridMaxCharsPerColumnStd"] = value; }
	}
	public int EditorResultsGridMaxCharsPerColumnXml
	{
		get { return (int)this["EditorResultsGridMaxCharsPerColumnXml"]; }
		set { this["EditorResultsGridMaxCharsPerColumnXml"] = value; }
	}

	// Text results options.
	public bool EditorResultsTextIncludeHeaders
	{
		get { return (bool)this["EditorResultsTextIncludeHeaders"]; }
		set { this["EditorResultsTextIncludeHeaders"] = value; }
	}
	public bool EditorResultsTextOutputQuery
	{
		get { return (bool)this["EditorResultsTextOutputQuery"]; }
		set { this["EditorResultsTextOutputQuery"] = value; }
	}
	public bool EditorResultsTextScrollingResults
	{
		get { return (bool)this["EditorResultsTextScrollingResults"]; }
		set { this["EditorResultsTextScrollingResults"] = value; }
	}
	public bool EditorResultsTextAlignRightNumerics
	{
		get { return (bool)this["EditorResultsTextAlignRightNumerics"]; }
		set { this["EditorResultsTextAlignRightNumerics"] = value; }
	}
	public bool EditorResultsTextDiscardResults
	{
		get { return (bool)this["EditorResultsTextDiscardResults"]; }
		set { this["EditorResultsTextDiscardResults"] = value; }
	}
	public int EditorResultsTextMaxCharsPerColumnStd
	{
		get { return (int)this["EditorResultsTextMaxCharsPerColumnStd"]; }
		set { this["EditorResultsTextMaxCharsPerColumnStd"] = value; }
	}
	public bool EditorResultsTextSeparateTabs
	{
		get { return (bool)this["EditorResultsTextSeparateTabs"]; }
		set { this["EditorResultsTextSeparateTabs"] = value; }
	}
	public bool EditorResultsTextSwitchToResults
	{
		get { return (bool)this["EditorResultsTextSwitchToResults"]; }
		set { this["EditorResultsTextSwitchToResults"] = value; }
	}
	public EnSqlOutputFormat EditorResultsTextOutputFormat
	{
		get { return (EnSqlOutputFormat)this["EditorResultsTextOutputFormat"]; }
		set { this["EditorResultsTextOutputFormat"] = value; }
	}
	public char EditorResultsTextDelimiter
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
			return CreateInstance(_TransientStore, _ExecOptions);
	}



	public static bool IsSupported(object property)
	{
		Type type = property.GetType();

		string cmd = SqlCmdResources.ResourceManager.GetString(type.Name);

		return cmd == null;
	}


	#endregion Methods




	// =========================================================================================================
	#region Event handlers - TransientSettings
	// =========================================================================================================


	#endregion Event handlers

}