

using System.Threading.Tasks;

namespace BlackbirdSql.Shared.Interfaces;


public interface IBsLanguageService
{
	void RefreshIntellisense(bool currentWindowOnly);
	Task RefreshIntellisenseAsync(bool currentWindowOnly);
}
