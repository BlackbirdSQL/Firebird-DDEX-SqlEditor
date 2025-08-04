using BlackbirdSql.Core.Model;
using BlackbirdSql.Sys.Enums;


namespace BlackbirdSql.Core.Interfaces;

internal interface IBsDataConnectionProperties
{
	EnConnectionSource ConnectionSource { get; set; }
	Csb Csa { get; }
}
