// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices.NoOpAuthoringScope

using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;


namespace BlackbirdSql.LanguageExtension.Ctl;


// =========================================================================================================
//
//										LsbAuthoringScope Class
//
/// <summary>
/// Language service no operation AuthoringScope implementation.
/// </summary>
// =========================================================================================================
internal class LsbNoOpAuthoringScope : AuthoringScope
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbNoOpAuthoringScope
	// ---------------------------------------------------------------------------------


	public LsbNoOpAuthoringScope(MetadataDisplayInfoProvider displayInfoProvider)
	{
		_Declarations = new ();
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - LsbNoOpAuthoringScope
	// =========================================================================================================


	private readonly LsbDeclarations _Declarations;


	#endregion Fields





	// =========================================================================================================
	#region Methods - LsbNoOpAuthoringScope
	// =========================================================================================================


	public override string GetDataTipText(int line, int col, out TextSpan span)
	{
		span = default;
		return null;
	}



	public override Declarations GetDeclarations(IVsTextView view, int line, int col, TokenInfo info, ParseReason reason)
	{
		return _Declarations;
	}



	public override Methods GetMethods(int line, int col, string name)
	{
		return null;
	}



	public override string Goto(VSConstants.VSStd97CmdID cmd, IVsTextView textView, int line, int col, out TextSpan span)
	{
		span = default;
		return null;
	}


	#endregion Methods

}
