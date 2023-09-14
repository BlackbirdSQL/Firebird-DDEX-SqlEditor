// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System.ComponentModel;
using BlackbirdSql.Core.Ctl.Config;
using BlackbirdSql.VisualStudio.Ddex.Ctl.ComponentModel;

namespace BlackbirdSql.VisualStudio.Ddex.Ctl.Config;

// =========================================================================================================
//										VsGeneralOptionModel Class
//
/// <summary>
/// Option Model for General options
/// </summary>
// =========================================================================================================
public class GeneralOptionModel : AbstractOptionModel<GeneralOptionModel>
{

	[GlobalizedCategory("OptionCategoryDiagnostics")]
	[GlobalizedDisplayName("OptionDisplayEnableDiagnostics")]
	[GlobalizedDescription("OptionDescriptionEnableDiagnostics")]
	[DefaultValue(true)]
	public bool EnableDiagnostics { get; set; } = true;

	[GlobalizedCategory("OptionCategoryDiagnostics")]
	[GlobalizedDisplayName("OptionDisplayEnableTaskLog")]
	[GlobalizedDescription("OptionDescriptionEnableTaskLog")]
	[DefaultValue(true)]
	public bool EnableTaskLog { get; set; } = true;

	[GlobalizedCategory("OptionCategoryEntityFramework")]
	[GlobalizedDisplayName("OptionDisplayValidateConfig")]
	[GlobalizedDescription("OptionDescriptionValidateConfig")]
	[DefaultValue(true)]
	public bool ValidateConfig { get; set; } = true;

	[GlobalizedCategory("OptionCategoryEntityFramework")]
	[GlobalizedDisplayName("OptionDisplayValidateEdmx")]
	[GlobalizedDescription("OptionDescriptionValidateEdmx")]
	[DefaultValue(true)]
	public bool ValidateEdmx { get; set; } = true;


	public GeneralOptionModel() : base("General")
	{
	}

}
