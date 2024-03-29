// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.MVVM.Commands.IAsyncCommand

using System.Threading.Tasks;
using System.Windows.Input;


namespace BlackbirdSql.Common.Ctl.Interfaces;

public interface IBAsyncCommand : IBOwnedCommand, ICommand
{
	Task ExecuteAsync(object parameter);
}
