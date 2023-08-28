#region Assembly Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;


// using Microsoft.VisualStudio.Data.Tools.SqlLanguageServices;
// using Ns = Microsoft.VisualStudio.Data.Tools.SqlLanguageServices;

// namespace Microsoft.VisualStudio.Data.Tools.SqlLanguageServices
namespace BlackbirdSql.LanguageExtension
{
	internal class NoOpAuthoringScope : Microsoft.VisualStudio.Package.AuthoringScope
	{
		private readonly Declarations declarations;

		public NoOpAuthoringScope(MetadataDisplayInfoProvider displayInfoProvider)
		{
			declarations = new Declarations();
		}

		public override string GetDataTipText(int line, int col, out TextSpan span)
		{
			span = default;
			return null;
		}

		public override Microsoft.VisualStudio.Package.Declarations GetDeclarations(IVsTextView view, int line, int col, TokenInfo info, ParseReason reason)
		{
			return declarations;
		}

		public override Microsoft.VisualStudio.Package.Methods GetMethods(int line, int col, string name)
		{
			return null;
		}

		public override string Goto(VSConstants.VSStd97CmdID cmd, IVsTextView textView, int line, int col, out TextSpan span)
		{
			span = default;
			return null;
		}
	}
}
