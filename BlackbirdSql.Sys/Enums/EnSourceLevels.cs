
using System;



namespace BlackbirdSql.Sys.Enums;


/// <summary>
/// A SourceLevels enum that includes a TraceOnly, DebugOnly and TraceDebug value.
/// </summary>
[Flags]
public enum EnSourceLevels
{
	/// <summary>
	/// Does not allow any events through.
	/// </summary>
	Off = 0,

	/// <summary>
	/// Allows Critical events through.
	/// </summary>
	Critical = 1,

	/// <summary>
	/// Allows Critical and Error events through.
	/// </summary>
	Error = 3,

	/// <summary>
	/// Allows Critical, Error and Warning events through.
	/// </summary>
	Warning = 7,

	/// <summary>
	/// Allows Critical, Error, Warning and Information events through.
	/// </summary>
	Information = 0xF,

	TraceOnly = 0x10,
	DebugOnly = 0x20,

	/// <summary>
	/// Allows Critical, Error, Warning, Information and Trace events through.
	/// </summary>
	Verbose = 0x1F,

	/// <summary>
	/// Allows Trace and Debug events through.
	/// </summary>
	TraceDebug = 0x30,

	/// <summary>
	/// Allows the Stop, Start, Suspend, Transfer and Resume events through.
	/// </summary>
	ActivityTracing = 0xFF00,

	/// <summary>
	/// Allows all events through.
	/// </summary>
	All = 0xFFFF
}
