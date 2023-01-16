using System.ComponentModel;
using System.Runtime.InteropServices;

using BlackbirdSql.VisualStudio.Ddex.Extensions;


namespace BlackbirdSql.VisualStudio.Ddex.Configuration
{
	internal partial class VsOptionsProvider
	{
		public const string OptionPageCategory = "BlackbirdSql DDEX 2.0";
		public const string GeneralOptionPageName = "General";
		public const string DebugOptionPageName = "Debugging";

		[ComVisible(true)]
		public class GeneralOptionPage : AbstractOptionPage<VsGeneralOptionModel> { }
		public class DebugOptionPage : AbstractOptionPage<VsDebugOptionModel> { }
	}
}
