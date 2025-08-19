//
// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)
//

using System.Data;
using BlackbirdSql.Sys.Interfaces;
using FirebirdSql.Data.FirebirdClient;



namespace BlackbirdSql.Data.Model.Schema;



internal class DslTriggerColumns : DslColumns
{
	public DslTriggerColumns() : base()
	{
	}


	protected override void InitializeParameters(IDbConnection connection)
	{
		base.InitializeParameters(connection);

		_ParentType = "Trigger";
		_ObjectType = "TriggerColumn";
		_ParentColName = "dep.RDB$DEPENDENT_NAME";
		_OrderingColumn = "dep.RDB$DEPENDENT_NAME";
		_FromClause = @"RDB$DEPENDENCIES dep
                INNER JOIN RDB$TRIGGERS trg
                    ON trg.RDB$TRIGGER_NAME = dep.RDB$DEPENDENT_NAME AND trg.RDB$RELATION_NAME = dep.RDB$DEPENDED_ON_NAME
				INNER JOIN RDB$RELATION_FIELDS r
					ON r.RDB$RELATION_NAME = dep.RDB$DEPENDED_ON_NAME AND r.RDB$FIELD_NAME = dep.RDB$FIELD_NAME";
		_ConditionClause = "dep.RDB$FIELD_NAME IS NOT NULL";

		_RequiredColumns["TRIGGER_NAME"] = "dep.RDB$DEPENDENT_NAME";
	}





	#region Protected Methods


	#endregion
}
