
namespace BlackbirdSql.Sys.Enums;


/// <summary>
/// EventLevel enum that includes Trace, DebugOnly and ActivityTracing.
/// </summary>
public enum EnEventLevel
{
	Off = 0,
	Critical = 0x1,
	Error = 0x2,
	Warning = 0x4,
	Information = 0x8,
	Trace = 0x10,
	Debug = 0x20,
	Verbose = 0x1F,
	ActivityTracing = 0xFF00,
}
