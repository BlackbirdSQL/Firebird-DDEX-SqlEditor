// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System.ComponentModel;

using BlackbirdSql.Common.Extensions.Options;



namespace BlackbirdSql.VisualStudio.Ddex.Configuration;


// =========================================================================================================
//										VsDebugOptionModel Class
//
/// <summary>
/// Option Model for Debug options
/// </summary>
// =========================================================================================================
public class VsDebugOptionModel : AbstractOptionModel<VsDebugOptionModel>
{
	// [Category("General")]
	[DisplayName("Enable trace")]
	[Description("Enables the execution of Debug.Trace() calls. Note: Only available on DEBUG build.")]
	[DefaultValue(true)]
	public bool EnableTrace { get; set; } = true;

	// [Category("General")]
	[DisplayName("Enable diagnostics")]
	[Description("Enables the execution of Debug.Dug()/Debug.Trace() calls. Note: Does not disable Exceptions.")]
	[DefaultValue(true)]
	public bool EnableDiagnostics { get; set; } = true;

	// [Category("General")]
	[DisplayName("Enable Firebird diagnostics")]
	[Description("Enables the execution of Firebird Debug.Dug()/Debug.Trace() calls. Note: Does not disable Exceptions.")]
	[DefaultValue(true)]
	public bool EnableFbDiagnostics { get; set; } = true;

}
