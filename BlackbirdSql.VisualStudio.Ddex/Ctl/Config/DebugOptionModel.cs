// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System.ComponentModel;
using BlackbirdSql.Core.Ctl.Config;
using BlackbirdSql.VisualStudio.Ddex.Ctl.ComponentModel;

namespace BlackbirdSql.VisualStudio.Ddex.Ctl.Config;

// =========================================================================================================
//										VsDebugOptionModel Class
//
/// <summary>
/// Option Model for Debug options
/// </summary>
// =========================================================================================================
public class DebugOptionModel : AbstractOptionModel<DebugOptionModel>
{
	[GlobalizedCategory("OptionCategoryDebug")]
	[GlobalizedDisplayName("OptionDisplayEnableTrace")]
	[GlobalizedDescription("OptionDescriptionEnableTrace")]
	[DefaultValue(true)]
	public bool EnableTrace { get; set; } = true;

	[GlobalizedCategory("OptionCategoryDebug")]
	[GlobalizedDisplayName("OptionDisplayEnableTracer")]
	[GlobalizedDescription("OptionDescriptionEnableTracer")]
	[DefaultValue(false)]
	public bool EnableTracer { get; set; } = false;

	[GlobalizedCategory("OptionCategoryDebug")]
	[GlobalizedDisplayName("OptionDisplayPersistentValidation")]
	[GlobalizedDescription("OptionDescriptionPersistentValidation")]
	[DefaultValue(true)]
	public bool PersistentValidation { get; set; } = true;

	[GlobalizedCategory("OptionCategoryDebug")]
	[GlobalizedDisplayName("OptionDisplayEnableDiagnosticsLog")]
	[GlobalizedDescription("OptionDescriptionEnableDiagnosticsLog")]
	[DefaultValue(true)]
	public bool EnableDiagnosticsLog { get; set; } = false;

	[GlobalizedCategory("OptionCategoryDebug")]
	[GlobalizedDisplayName("OptionDisplayLogFile")]
	[GlobalizedDescription("OptionDescriptionLogFile")]
	[DefaultValue("/temp/vsdiag.log")]
	public string LogFile { get; set; } = "/temp/vsdiag.log";

	[GlobalizedCategory("OptionCategoryDebug")]
	[GlobalizedDisplayName("OptionDisplayEnableFbDiagnostics")]
	[GlobalizedDescription("OptionDescriptionEnableFbDiagnostics")]
	[DefaultValue(true)]
	public bool EnableFbDiagnostics { get; set; } = true;

	[GlobalizedCategory("OptionCategoryDebug")]
	[GlobalizedDisplayName("OptionDisplayFbLogFile")]
	[GlobalizedDescription("OptionDescriptionFbLogFile")]
	[DefaultValue("/temp/vsdiagfb.log")]
	public string FbLogFile { get; set; } = "/temp/vsdiagfb.log";

	public DebugOptionModel() : base("Debug")
	{
	}

}
