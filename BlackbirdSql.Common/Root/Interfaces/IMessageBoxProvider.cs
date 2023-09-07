// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.IMessageBoxProvider
using System;
using System.Windows.Forms;
using BlackbirdSql.Common.Enums;

namespace BlackbirdSql.Common.Interfaces;

public interface IMessageBoxProvider
{
	DialogResult ShowMessage(Exception e, string caption, EnExceptionMessageBoxButtons buttons, EnExceptionMessageBoxSymbol symbol, IWin32Window owner);

	DialogResult ShowMessage(string text, string caption, EnExceptionMessageBoxButtons buttons, EnExceptionMessageBoxSymbol symbol, IWin32Window owner);
}
