// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.BaseStmtInfoType
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using BlackbirdSql.Common.Controls.Graphing.Enums;

namespace BlackbirdSql.Common.Controls.Graphing.Gram;

[Serializable]
[XmlInclude(typeof(StmtReceiveType))]
[XmlInclude(typeof(StmtCursorType))]
[XmlInclude(typeof(StmtCondType))]
[XmlInclude(typeof(StmtUseDbType))]
[XmlInclude(typeof(StmtSimpleType))]
[GeneratedCode("xsd", "4.8.3928.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(Namespace = LibraryData.C_ShowPlanNamespace)]
public class AbstractStmtInfoType
{
	private SetOptionsType statementSetOptionsField;

	private int statementCompIdField;

	private bool statementCompIdFieldSpecified;

	private double statementEstRowsField;

	private bool statementEstRowsFieldSpecified;

	private int statementIdField;

	private bool statementIdFieldSpecified;

	private int queryCompilationReplayField;

	private bool queryCompilationReplayFieldSpecified;

	private string statementOptmLevelField;

	private EnStmtInfoTypeStatementOptmEarlyAbortReason statementOptmEarlyAbortReasonField;

	private bool statementOptmEarlyAbortReasonFieldSpecified;

	private string cardinalityEstimationModelVersionField;

	private double statementSubTreeCostField;

	private bool statementSubTreeCostFieldSpecified;

	private string statementTextField;

	private string statementTypeField;

	private string templatePlanGuideDBField;

	private string templatePlanGuideNameField;

	private string planGuideDBField;

	private string planGuideNameField;

	private string parameterizedTextField;

	private string parameterizedPlanHandleField;

	private string queryHashField;

	private string queryPlanHashField;

	private string retrievedFromCacheField;

	private string statementSqlHandleField;

	private ulong databaseContextSettingsIdField;

	private bool databaseContextSettingsIdFieldSpecified;

	private ulong parentObjectIdField;

	private bool parentObjectIdFieldSpecified;

	private string batchSqlHandleField;

	private int statementParameterizationTypeField;

	private bool statementParameterizationTypeFieldSpecified;

	private bool securityPolicyAppliedField;

	private bool securityPolicyAppliedFieldSpecified;

	private bool batchModeOnRowStoreUsedField;

	private bool batchModeOnRowStoreUsedFieldSpecified;

	private ulong queryStoreStatementHintIdField;

	private bool queryStoreStatementHintIdFieldSpecified;

	private string queryStoreStatementHintTextField;

	private string queryStoreStatementHintSourceField;

	private bool containsLedgerTablesField;

	private bool containsLedgerTablesFieldSpecified;

	public SetOptionsType StatementSetOptions
	{
		get
		{
			return statementSetOptionsField;
		}
		set
		{
			statementSetOptionsField = value;
		}
	}

	[XmlAttribute]
	public int StatementCompId
	{
		get
		{
			return statementCompIdField;
		}
		set
		{
			statementCompIdField = value;
		}
	}

	[XmlIgnore]
	public bool StatementCompIdSpecified
	{
		get
		{
			return statementCompIdFieldSpecified;
		}
		set
		{
			statementCompIdFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public double StatementEstRows
	{
		get
		{
			return statementEstRowsField;
		}
		set
		{
			statementEstRowsField = value;
		}
	}

	[XmlIgnore]
	public bool StatementEstRowsSpecified
	{
		get
		{
			return statementEstRowsFieldSpecified;
		}
		set
		{
			statementEstRowsFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public int StatementId
	{
		get
		{
			return statementIdField;
		}
		set
		{
			statementIdField = value;
		}
	}

	[XmlIgnore]
	public bool StatementIdSpecified
	{
		get
		{
			return statementIdFieldSpecified;
		}
		set
		{
			statementIdFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public int QueryCompilationReplay
	{
		get
		{
			return queryCompilationReplayField;
		}
		set
		{
			queryCompilationReplayField = value;
		}
	}

	[XmlIgnore]
	public bool QueryCompilationReplaySpecified
	{
		get
		{
			return queryCompilationReplayFieldSpecified;
		}
		set
		{
			queryCompilationReplayFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public string StatementOptmLevel
	{
		get
		{
			return statementOptmLevelField;
		}
		set
		{
			statementOptmLevelField = value;
		}
	}

	[XmlAttribute]
	public EnStmtInfoTypeStatementOptmEarlyAbortReason StatementOptmEarlyAbortReason
	{
		get
		{
			return statementOptmEarlyAbortReasonField;
		}
		set
		{
			statementOptmEarlyAbortReasonField = value;
		}
	}

	[XmlIgnore]
	public bool StatementOptmEarlyAbortReasonSpecified
	{
		get
		{
			return statementOptmEarlyAbortReasonFieldSpecified;
		}
		set
		{
			statementOptmEarlyAbortReasonFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public string CardinalityEstimationModelVersion
	{
		get
		{
			return cardinalityEstimationModelVersionField;
		}
		set
		{
			cardinalityEstimationModelVersionField = value;
		}
	}

	[XmlAttribute]
	public double StatementSubTreeCost
	{
		get
		{
			return statementSubTreeCostField;
		}
		set
		{
			statementSubTreeCostField = value;
		}
	}

	[XmlIgnore]
	public bool StatementSubTreeCostSpecified
	{
		get
		{
			return statementSubTreeCostFieldSpecified;
		}
		set
		{
			statementSubTreeCostFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public string StatementText
	{
		get
		{
			return statementTextField;
		}
		set
		{
			statementTextField = value;
		}
	}

	[XmlAttribute]
	public string StatementType
	{
		get
		{
			return statementTypeField;
		}
		set
		{
			statementTypeField = value;
		}
	}

	[XmlAttribute]
	public string TemplatePlanGuideDB
	{
		get
		{
			return templatePlanGuideDBField;
		}
		set
		{
			templatePlanGuideDBField = value;
		}
	}

	[XmlAttribute]
	public string TemplatePlanGuideName
	{
		get
		{
			return templatePlanGuideNameField;
		}
		set
		{
			templatePlanGuideNameField = value;
		}
	}

	[XmlAttribute]
	public string PlanGuideDB
	{
		get
		{
			return planGuideDBField;
		}
		set
		{
			planGuideDBField = value;
		}
	}

	[XmlAttribute]
	public string PlanGuideName
	{
		get
		{
			return planGuideNameField;
		}
		set
		{
			planGuideNameField = value;
		}
	}

	[XmlAttribute]
	public string ParameterizedText
	{
		get
		{
			return parameterizedTextField;
		}
		set
		{
			parameterizedTextField = value;
		}
	}

	[XmlAttribute]
	public string ParameterizedPlanHandle
	{
		get
		{
			return parameterizedPlanHandleField;
		}
		set
		{
			parameterizedPlanHandleField = value;
		}
	}

	[XmlAttribute]
	public string QueryHash
	{
		get
		{
			return queryHashField;
		}
		set
		{
			queryHashField = value;
		}
	}

	[XmlAttribute]
	public string QueryPlanHash
	{
		get
		{
			return queryPlanHashField;
		}
		set
		{
			queryPlanHashField = value;
		}
	}

	[XmlAttribute]
	public string RetrievedFromCache
	{
		get
		{
			return retrievedFromCacheField;
		}
		set
		{
			retrievedFromCacheField = value;
		}
	}

	[XmlAttribute]
	public string StatementSqlHandle
	{
		get
		{
			return statementSqlHandleField;
		}
		set
		{
			statementSqlHandleField = value;
		}
	}

	[XmlAttribute]
	public ulong DatabaseContextSettingsId
	{
		get
		{
			return databaseContextSettingsIdField;
		}
		set
		{
			databaseContextSettingsIdField = value;
		}
	}

	[XmlIgnore]
	public bool DatabaseContextSettingsIdSpecified
	{
		get
		{
			return databaseContextSettingsIdFieldSpecified;
		}
		set
		{
			databaseContextSettingsIdFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public ulong ParentObjectId
	{
		get
		{
			return parentObjectIdField;
		}
		set
		{
			parentObjectIdField = value;
		}
	}

	[XmlIgnore]
	public bool ParentObjectIdSpecified
	{
		get
		{
			return parentObjectIdFieldSpecified;
		}
		set
		{
			parentObjectIdFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public string BatchSqlHandle
	{
		get
		{
			return batchSqlHandleField;
		}
		set
		{
			batchSqlHandleField = value;
		}
	}

	[XmlAttribute]
	public int StatementParameterizationType
	{
		get
		{
			return statementParameterizationTypeField;
		}
		set
		{
			statementParameterizationTypeField = value;
		}
	}

	[XmlIgnore]
	public bool StatementParameterizationTypeSpecified
	{
		get
		{
			return statementParameterizationTypeFieldSpecified;
		}
		set
		{
			statementParameterizationTypeFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public bool SecurityPolicyApplied
	{
		get
		{
			return securityPolicyAppliedField;
		}
		set
		{
			securityPolicyAppliedField = value;
		}
	}

	[XmlIgnore]
	public bool SecurityPolicyAppliedSpecified
	{
		get
		{
			return securityPolicyAppliedFieldSpecified;
		}
		set
		{
			securityPolicyAppliedFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public bool BatchModeOnRowStoreUsed
	{
		get
		{
			return batchModeOnRowStoreUsedField;
		}
		set
		{
			batchModeOnRowStoreUsedField = value;
		}
	}

	[XmlIgnore]
	public bool BatchModeOnRowStoreUsedSpecified
	{
		get
		{
			return batchModeOnRowStoreUsedFieldSpecified;
		}
		set
		{
			batchModeOnRowStoreUsedFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public ulong QueryStoreStatementHintId
	{
		get
		{
			return queryStoreStatementHintIdField;
		}
		set
		{
			queryStoreStatementHintIdField = value;
		}
	}

	[XmlIgnore]
	public bool QueryStoreStatementHintIdSpecified
	{
		get
		{
			return queryStoreStatementHintIdFieldSpecified;
		}
		set
		{
			queryStoreStatementHintIdFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public string QueryStoreStatementHintText
	{
		get
		{
			return queryStoreStatementHintTextField;
		}
		set
		{
			queryStoreStatementHintTextField = value;
		}
	}

	[XmlAttribute]
	public string QueryStoreStatementHintSource
	{
		get
		{
			return queryStoreStatementHintSourceField;
		}
		set
		{
			queryStoreStatementHintSourceField = value;
		}
	}

	[XmlAttribute]
	public bool ContainsLedgerTables
	{
		get
		{
			return containsLedgerTablesField;
		}
		set
		{
			containsLedgerTablesField = value;
		}
	}

	[XmlIgnore]
	public bool ContainsLedgerTablesSpecified
	{
		get
		{
			return containsLedgerTablesFieldSpecified;
		}
		set
		{
			containsLedgerTablesFieldSpecified = value;
		}
	}
}
