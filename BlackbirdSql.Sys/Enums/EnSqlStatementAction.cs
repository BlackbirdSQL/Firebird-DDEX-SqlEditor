
namespace BlackbirdSql.Sys;


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
