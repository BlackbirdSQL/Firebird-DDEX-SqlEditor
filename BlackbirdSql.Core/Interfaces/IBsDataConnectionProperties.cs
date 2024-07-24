using BlackbirdSql.Core.Model;
using BlackbirdSql.Sys.Enums;


namespace BlackbirdSql.Core.Interfaces;

public interface IBsDataConnectionProperties
{
	EnConnectionSource ConnectionSource { get; set; }
	Csb Csa { get; }
}
