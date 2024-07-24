
using System;

namespace BlackbirdSql.Sys.Enums;


[Flags]
public enum EnEditorCreationFlags
{
	None = 0x0,
	CreateConnection = 0x1,
	AutoExecute = 0x2,
	CreateAndExecute = 0x3
}
