// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System.ComponentModel;

using BlackbirdSql.Common.Extensions.Options;



namespace BlackbirdSql.VisualStudio.Ddex.Configuration
{


	// =========================================================================================================
	//										VsGeneralOptionModel Class
	//
	/// <summary>
	/// Option Model for General options
	/// </summary>
	// =========================================================================================================
	public class VsGeneralOptionModel : AbstractOptionModel<VsGeneralOptionModel>
	{
		[Category("General")]
		[DisplayName("Enable logging")]
		[Description("Enables diagnostics logging to a log file. Note: For release version only exceptions will be logged.")]
		[DefaultValue(true)]
		public bool EnableWriteLog { get; set; } = true;

		[Category("General")]
		[DisplayName("Log file")]
		[Description("Location of diagnostics log file.")]
		[DefaultValue("C:\\bin\\vsdiag.log")]
		public string LogFile { get; set; } = "C:\\bin\\vsdiag.log";

		[Category("General")]
		[DisplayName("Firebird Log file")]
		[Description("Location of Firebird diagnostics log file.")]
		[DefaultValue("C:\\bin\\vsdiagfb.log")]
		public string FbLogFile { get; set; } = "C:\\bin\\vsdiagfb.log";

		[Category("EntityFramework settings")]
		[DisplayName("Validate App.config")]
		[Description("Enable this option to allow BlackbirdSql to ensure that Firebird EntityFramework is configured in your non-CPS projects' App.config.")]
		[DefaultValue(true)]
		public bool ValidateConfig { get; set; } = true;

		[Category("EntityFramework settings")]
		[DisplayName("Update legacy edmx models")]
		[Description("Enable this option to allow BlackbirdSql to update legacy edmx models to use EntityFramework 6.")]
		[DefaultValue(true)]
		public bool ValidateEdmx { get; set; } = true;

	}
}
