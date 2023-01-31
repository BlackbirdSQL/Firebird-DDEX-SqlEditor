using System.ComponentModel;
using System.Runtime.InteropServices;

using BlackbirdSql.VisualStudio.Ddex.Extensions;


namespace BlackbirdSql.VisualStudio.Ddex.Configuration
{
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
}
