using BlackbirdSql.Sys.Enums;


namespace BlackbirdSql.Core.Interfaces;


public interface IBDataConnectionSupport
{
	EnConnectionSource ConnectionSource { get; set; }
}
