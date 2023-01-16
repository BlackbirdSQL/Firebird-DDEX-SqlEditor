using System.ComponentModel;
using System.Runtime.InteropServices;

using BlackbirdSql.VisualStudio.Ddex.Extensions;


namespace BlackbirdSql.VisualStudio.Ddex.Configuration
{
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

		[Category("EntityFramework settings")]
		[DisplayName("Validate App.config")]
		[Description("Enable this option to allow BlackbirdSql to ensure that Firebird EntityFramework is configured in your App.config.")]
		[DefaultValue(true)]
		public bool ValidateConfig { get; set; } = true;

		[Category("EntityFramework settings")]
		[DisplayName("Update legacy edmx models")]
		[Description("Enable this option to allow BlackbirdSql to update legacy edmx models to use EntityFramework 6.")]
		[DefaultValue(true)]
		public bool ValidateEdmx { get; set; } = true;

	}
}
