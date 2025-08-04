using BlackbirdSql.Sys.Enums;


namespace BlackbirdSql.Core.Interfaces;


internal interface IBsDataConnectionSupport
{
	EnConnectionSource ConnectionSource { get; set; }
}
