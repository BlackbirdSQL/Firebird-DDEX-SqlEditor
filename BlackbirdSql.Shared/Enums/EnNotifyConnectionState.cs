// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.ButtonCellState

namespace BlackbirdSql.Shared.Enums;

public enum EnNotifyConnectionState
{
	NotifyAutoClosed,
	NotifyBroken,
	NotifyDead,
	NotifyReset,
	NotifyOpen,
	NotifyClosed,
	NotifyEnumEndMarker,

	ConfirmedOpen,
	ConfirmedClosed,
	RequestIsUnlocked
}