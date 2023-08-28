// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using BlackbirdSql.Core.Options;

namespace BlackbirdSql.VisualStudio.Ddex.Config;


// =========================================================================================================
//										VsOptionsProvider Class
//
/// <summary>
/// Provider for <see cref="ProvideOptionPageAttribute"/> in <see cref="BlackbirdSqlDdexExtension"/>
/// </summary>
// =========================================================================================================
internal partial class VsOptionsProvider
{
	public const string OptionPageCategory = "BlackbirdSql DDEX 2.0";
	public const string GeneralOptionPageName = "General";
	public const string DebugOptionPageName = "Debugging";

	[ComVisible(true)]
	public class GeneralOptionPage : AbstractOptionPage<VsGeneralOptionModel> { }
	public class DebugOptionPage : AbstractOptionPage<VsDebugOptionModel> { }
}
