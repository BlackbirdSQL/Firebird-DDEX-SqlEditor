

using System.Threading.Tasks;

namespace BlackbirdSql.Shared.Interfaces;


public interface IBsLanguageService
{
	void RefreshIntellisense(bool currentWindowOnly);
	Task RefreshIntellisenseEuiAsync(bool currentWindowOnly);
}
