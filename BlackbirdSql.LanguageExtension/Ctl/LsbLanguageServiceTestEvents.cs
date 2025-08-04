// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices.LanguageServiceTestEvents

using System;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;



namespace BlackbirdSql.LanguageExtension.Ctl;


// =========================================================================================================
//
//										LsbLanguageServiceTestEvents Class
//
// =========================================================================================================
internal class LsbLanguageServiceTestEvents
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - LsbLanguageServiceTestEvents
	// ---------------------------------------------------------------------------------


	private LsbLanguageServiceTestEvents()
	{
		EnableTestEvents = false;
	}



	internal static LsbLanguageServiceTestEvents Instance => _Instance ??= new LsbLanguageServiceTestEvents();


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - LsbLanguageServiceTestEvents
	// =========================================================================================================


	private static LsbLanguageServiceTestEvents _Instance;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - LsbLanguageServiceTestEvents
	// =========================================================================================================


	internal bool EnableTestEvents { get; set; }

	internal event EventHandler<DeclarationsRequestedEventArgsI> DeclarationsRequestedEvent;
	internal event EventHandler<EventArgs> SqlCompletionSetDismissedEvent;


	#endregion Property accessors





	// =========================================================================================================
	#region Event handling - LsbLanguageServiceTestEvents
	// =========================================================================================================


	internal void RaiseDeclarationsRequestedEvent(LsbDeclarations declarations, IVsTextView textView,
		int line, int column, TokenInfo tokenInfo, ParseReason parseReason)
	{
		if (EnableTestEvents)
		{
			DeclarationsRequestedEventArgsI e = new (declarations, textView, line, column, tokenInfo, parseReason);

			DeclarationsRequestedEvent?.Invoke(this, e);
		}
	}



	internal void RaiseSqlCompletionSetDismissedEvent()
	{
		if (EnableTestEvents && SqlCompletionSetDismissedEvent != null)
		{
			SqlCompletionSetDismissedEvent(this, null);
		}
	}


	#endregion Event handling





	// =========================================================================================================
	#region							Nested types - LsbLanguageServiceTestEvents
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// DeclarationsRequestedEventArgs Sub-class
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal class DeclarationsRequestedEventArgsI : EventArgs
	{
		public DeclarationsRequestedEventArgsI(LsbDeclarations declarations, IVsTextView textView,
			int line, int column, TokenInfo tokenInfo, ParseReason parseReason)
		{
			Declarations = declarations;
			TextView = textView;
			Line = line;
			Column = column;
			TokenInfo = tokenInfo;
			ParseReason = parseReason;
		}


		internal LsbDeclarations Declarations { get; private set; }

		internal IVsTextView TextView { get; private set; }

		internal int Line { get; private set; }

		internal int Column { get; private set; }

		internal TokenInfo TokenInfo { get; private set; }

		internal ParseReason ParseReason { get; private set; }
	}


	#endregion Nested types

}
