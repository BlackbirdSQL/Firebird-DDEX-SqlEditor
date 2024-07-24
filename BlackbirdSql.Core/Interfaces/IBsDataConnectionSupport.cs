using BlackbirdSql.Sys.Enums;


namespace BlackbirdSql.Core.Interfaces;


public interface IBsDataConnectionSupport
{
	EnConnectionSource ConnectionSource { get; set; }
}
