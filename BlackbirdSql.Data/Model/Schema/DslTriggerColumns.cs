//
// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)
//

using System.Data;
using BlackbirdSql.Sys.Interfaces;



namespace BlackbirdSql.Data.Model.Schema;



internal class DslTriggerColumns : DslColumns
{
	public DslTriggerColumns() : base()
	{
		// Evs.Trace(GetType(), "DslTriggerColumns.DslTriggerColumns");

		_ParentType = "Trigger";
		_ObjectType = "TriggerColumn";
		_ParentColumn = "dep.rdb$dependent_name";
		_OrderingField = "dep.rdb$dependent_name";
		_FromClause = @"rdb$dependencies dep
                INNER JOIN rdb$triggers trg
                    ON trg.rdb$trigger_name = dep.rdb$dependent_name AND trg.rdb$relation_name = dep.rdb$depended_on_name
				INNER JOIN rdb$relation_fields r
					ON r.rdb$relation_name = dep.rdb$depended_on_name AND r.rdb$field_name = dep.rdb$field_name";
		_ConditionClause = "dep.rdb$field_name IS NOT NULL";

		_RequiredColumns["TRIGGER_NAME"] = "dep.rdb$dependent_name";

	}





	#region Protected Methods


	#endregion
}
