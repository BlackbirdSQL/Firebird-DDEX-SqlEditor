// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices.LanguageServiceTestEvents
using System;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;



namespace BlackbirdSql.LanguageExtension.Ctl;


public class LsbLanguageServiceTestEvents
{

	private LsbLanguageServiceTestEvents()
	{
		EnableTestEvents = false;
	}



	public class DeclarationsRequestedEventArgs : EventArgs
	{
		public LsbDeclarations Declarations { get; private set; }

		public IVsTextView TextView { get; private set; }

		public int Line { get; private set; }

		public int Column { get; private set; }

		public TokenInfo TokenInfo { get; private set; }

		public ParseReason ParseReason { get; private set; }

		public DeclarationsRequestedEventArgs(LsbDeclarations declarations, IVsTextView textView,
			int line, int column, TokenInfo tokenInfo, ParseReason parseReason)
		{
			Declarations = declarations;
			TextView = textView;
			Line = line;
			Column = column;
			TokenInfo = tokenInfo;
			ParseReason = parseReason;
		}
	}

	private static LsbLanguageServiceTestEvents _Instance;

	public static LsbLanguageServiceTestEvents Instance => _Instance ??= new LsbLanguageServiceTestEvents();


	public bool EnableTestEvents { get; set; }

	public event EventHandler<DeclarationsRequestedEventArgs> DeclarationsRequestedEvent;

	public event EventHandler<EventArgs> SqlCompletionSetDismissedEvent;


	public void RaiseDeclarationsRequestedEvent(LsbDeclarations declarations, IVsTextView textView,
		int line, int column, TokenInfo tokenInfo, ParseReason parseReason)
	{
		if (EnableTestEvents)
		{
			DeclarationsRequestedEventArgs e = new DeclarationsRequestedEventArgs(declarations, textView, line, column, tokenInfo, parseReason);

			DeclarationsRequestedEvent?.Invoke(this, e);
		}
	}

	public void RaiseSqlCompletionSetDismissedEvent()
	{
		if (EnableTestEvents && SqlCompletionSetDismissedEvent != null)
		{
			SqlCompletionSetDismissedEvent(this, null);
		}
	}
}
