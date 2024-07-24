// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.QueryExecutor.StatusType


using System;

namespace BlackbirdSql.Shared.Enums;

[Flags]
public enum EnQueryStatusFlags
{
	None = 0,
	Connected = 0x1,
	Executing = 0x2,
	Connection = 0x4,
	Connecting = 0x8,
	Cancelling = 0x10,
	DatabaseChanged = 0x40
}
