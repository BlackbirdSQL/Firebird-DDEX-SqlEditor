// Microsoft.SqlServer.DlgGrid, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.ButtonInfo

using System.Drawing;
using BlackbirdSql.Shared.Enums;



namespace BlackbirdSql.Shared.Controls.Grid;


public class ButtonInfo
{
	private Bitmap m_bmp;

	private string m_label = "";

	private EnButtonCellState m_state = EnButtonCellState.Normal;

	public Bitmap Bmp
	{
		get
		{
			return m_bmp;
		}
		set
		{
			m_bmp = value;
		}
	}

	public string Label
	{
		get
		{
			return m_label;
		}
		set
		{
			m_label = value;
		}
	}

	public EnButtonCellState State
	{
		get
		{
			return m_state;
		}
		set
		{
			m_state = value;
		}
	}

	public ButtonInfo(Bitmap bmp, string label)
	{
		m_bmp = bmp;
		m_label = label;
	}
}
