// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.IRenderable
using System.Drawing;


namespace BlackbirdSql.Common.Controls.Graphing.Interfaces;

internal interface IRenderable
{
	void Render(Graphics graphics);
}
