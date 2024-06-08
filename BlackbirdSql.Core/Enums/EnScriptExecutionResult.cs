
using System;

namespace BlackbirdSql.Core.Enums;


[Flags]
public enum EnScriptExecutionResult
{
	Success = 0x1,
	Failure = 0x2,
	Cancel = 0x4,
	Timeout = 0x8,
	Halted = 0x10,
	Mask = 0x1F
}
