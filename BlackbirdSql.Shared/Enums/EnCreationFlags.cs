
using System;

namespace BlackbirdSql.Shared.Enums;


[Flags]
public enum EnCreationFlags
{
	None = 0x0,
	CreateConnection = 0x1,
	AutoExecute = 0x2,
	CreateAndExecute = 0x3
}
