#region Assembly Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System.Windows.Input;


namespace BlackbirdSql.Common.Ctl.Interfaces;

public interface IBOwnedCommand : ICommand
{
	bool IsExecuting { get; }

	void RaiseCanExecuteChanged();
}
