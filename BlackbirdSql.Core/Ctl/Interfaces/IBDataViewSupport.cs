using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell.Interop;

namespace BlackbirdSql.Core.Ctl.Interfaces;

public interface IBDataViewSupport
{
	void EditNodeLabel(IVsDataExplorerNode node);
	void StartEditNodeLabel(IVsDataExplorerNode node);
	void CancelEditNodeLabel(IVsDataExplorerNode node);
	void CommitEditNodeLabel(IVsDataExplorerNode node);
}
