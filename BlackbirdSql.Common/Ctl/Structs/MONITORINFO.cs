// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.PlatformUI.MONITORINFO

namespace BlackbirdSql.Common.Ctl.Structs;

public struct MONITORINFO
{
	public uint cbSize;

	public UIRECT rcMonitor;

	public UIRECT rcWork;

	public uint dwFlags;
}
