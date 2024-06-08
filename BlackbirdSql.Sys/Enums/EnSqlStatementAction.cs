namespace BlackbirdSql.Sys.Enums;


public enum EnSqlStatementAction
{
	Inactive,
	ProcessQuery,
	SpecialActions,
	SpecialWithActualPlan,
	SpecialWithEstimatedPlan,
	Completed,
	Cancelled
}
