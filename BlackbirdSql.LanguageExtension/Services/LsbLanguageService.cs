// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices.LanguageService
using System.Runtime.InteropServices;
using Microsoft.SqlServer.Management.SqlParser.Parser;



namespace BlackbirdSql.LanguageExtension.Services;


// =========================================================================================================
//											LsbLanguageService Class 
//
/// <summary>
/// BlackbirdSql Language Service final class. This class level will handle all Firebird specific grammar.
/// </summary>
// =========================================================================================================
[Guid(PackageData.C_LanguageServiceGuid)]
public class LsbLanguageService : AbstractLanguageService
{

	// ----------------------------------------------------
	#region Constructors / Destructors - LsbLanguageService
	// ----------------------------------------------------


	public LsbLanguageService(object site) : base(site)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Constants - LsbLanguageService
	// =========================================================================================================


	public const int C_UIThreadWaitMilliseconds = 500;
	public const int C_BinderWaitMilliseconds = 2000;


	#endregion Constants





	// =========================================================================================================
	#region Methods - LsbLanguageService
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Hardcoded brute force checks for errors that are not Firebird errors.
	/// This will gradually be added to. It may not be ultra fast but we've already
	/// pumped up performance elsewhere.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override bool OverrideError(string script, Error error)
	{
		int startOffset = error.Start.Offset;
		int endOffset = error.End.Offset - 1;

		int len;
		bool ignore = false;


		// Check for '||' errors.
		if (startOffset > 2 && startOffset == endOffset && script[startOffset] == '|')
		{
			len = script.Length;

			// || concatenation.
			if (startOffset < len - 3 && script[startOffset - 1] == '|'
				&& script[startOffset - 2] != '|' && script[startOffset + 1] != '|')
			{
				return true;
			}
		}

		// Evs.Trace(GetType(), nameof(OverrideError), "StartOffset: {0}, EndOffset: {1}, Ignore: {2}.", startOffset, endOffset, ignore);

		return ignore;
	}


	#endregion Methods

}
