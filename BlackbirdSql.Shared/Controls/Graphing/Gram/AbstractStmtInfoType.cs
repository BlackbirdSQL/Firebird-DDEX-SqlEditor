// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.BaseStmtInfoType
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using BlackbirdSql.Shared.Controls.Graphing.Enums;

namespace BlackbirdSql.Shared.Controls.Graphing.Gram;

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
internal class AbstractStmtInfoType
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

	internal SetOptionsType StatementSetOptions
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
	internal int StatementCompId
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
	internal bool StatementCompIdSpecified
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
	internal double StatementEstRows
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
	internal bool StatementEstRowsSpecified
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
	internal int StatementId
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
	internal bool StatementIdSpecified
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
	internal int QueryCompilationReplay
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
	internal bool QueryCompilationReplaySpecified
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
	internal string StatementOptmLevel
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
	internal EnStmtInfoTypeStatementOptmEarlyAbortReason StatementOptmEarlyAbortReason
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
	internal bool StatementOptmEarlyAbortReasonSpecified
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
	internal string CardinalityEstimationModelVersion
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
	internal double StatementSubTreeCost
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
	internal bool StatementSubTreeCostSpecified
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
	internal string StatementText
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
	internal string StatementType
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
	internal string TemplatePlanGuideDB
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
	internal string TemplatePlanGuideName
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
	internal string PlanGuideDB
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
	internal string PlanGuideName
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
	internal string ParameterizedText
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
	internal string ParameterizedPlanHandle
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
	internal string QueryHash
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
	internal string QueryPlanHash
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
	internal string RetrievedFromCache
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
	internal string StatementSqlHandle
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
	internal ulong DatabaseContextSettingsId
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
	internal bool DatabaseContextSettingsIdSpecified
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
	internal ulong ParentObjectId
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
	internal bool ParentObjectIdSpecified
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
	internal string BatchSqlHandle
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
	internal int StatementParameterizationType
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
	internal bool StatementParameterizationTypeSpecified
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
	internal bool SecurityPolicyApplied
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
	internal bool SecurityPolicyAppliedSpecified
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
	internal bool BatchModeOnRowStoreUsed
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
	internal bool BatchModeOnRowStoreUsedSpecified
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
	internal ulong QueryStoreStatementHintId
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
	internal bool QueryStoreStatementHintIdSpecified
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
	internal string QueryStoreStatementHintText
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
	internal string QueryStoreStatementHintSource
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
	internal bool ContainsLedgerTables
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
	internal bool ContainsLedgerTablesSpecified
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
