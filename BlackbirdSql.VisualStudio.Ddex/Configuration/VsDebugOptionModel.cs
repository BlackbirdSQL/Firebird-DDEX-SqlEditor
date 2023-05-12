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
	[Category("Debug")]
	[DisplayName("Enable trace")]
	[Description("Enables the execution of Debug.Trace() calls. Only applicable to DEBUG builds.")]
	[DefaultValue(true)]
	public bool EnableTrace { get; set; } = true;

	[Category("Debug")]
	[DisplayName("Persistent validation flags")]
	[Description("Enables persistent validation flags. If disabled the validation flag of solutions and projects will be repeated upon reopening if Validation is enabled. Only applicable to DEBUG builds.")]
	[DefaultValue(true)]
	public bool PersistentValidation { get; set; } = true;

	[Category("Debug")]
	[DisplayName("Enable diagnostics logging")]
	[Description("Enables diagnostics logging to a log file. Only applicable to DEBUG builds.")]
	[DefaultValue(true)]
	public bool EnableDiagnosticsLog { get; set; } = false;

	[Category("Debug")]
	[DisplayName("Log file")]
	[Description("Location of diagnostics log file. Only applicable to DEBUG builds.")]
	[DefaultValue("/temp/vsdiag.log")]
	public string LogFile { get; set; } = "/temp/vsdiag.log";

	[Category("Debug")]
	[DisplayName("Enable Firebird diagnostics")]
	[Description("Enables the execution of Firebird Debug.Dug()/Debug.Trace() calls. Only applicable to DEBUG builds.")]
	[DefaultValue(true)]
	public bool EnableFbDiagnostics { get; set; } = true;

	[Category("Debug")]
	[DisplayName("Firebird Log file")]
	[Description("Location of Firebird diagnostics log file. Note: Only applicable to DEBUG builds.")]
	[DefaultValue("/temp/vsdiagfb.log")]
	public string FbLogFile { get; set; } = "/temp/vsdiagfb.log";

}
