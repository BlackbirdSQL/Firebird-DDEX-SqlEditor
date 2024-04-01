// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices.AuthoringScope
using System;
using System.Collections.Generic;
using Babel;
using BlackbirdSql.LanguageExtension.Model.Interfaces;
using BlackbirdSql.LanguageExtension.Services;
using Microsoft.SqlServer.Management.SqlParser.Intellisense;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;
using Microsoft.SqlServer.Management.SqlParser.Parser;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;



namespace BlackbirdSql.LanguageExtension.Ctl;


internal class LsbAuthoringScope : Microsoft.VisualStudio.Package.AuthoringScope
{

	public LsbAuthoringScope(ParseResult parseResult, MetadataDisplayInfoProvider displayInfoProvider, LsbSource source)
	{
		_ParseResult = parseResult;
		_DisplayInfoProvider = displayInfoProvider;
		_Source = source;
	}




	private readonly ParseResult _ParseResult;

	private readonly MetadataDisplayInfoProvider _DisplayInfoProvider;

	private readonly LsbSource _Source;


	public override string GetDataTipText(int line, int col, out TextSpan span)
	{
		string result = null;

		span = default;

		CodeObjectQuickInfo codeObjectQuickInfo = null;
		object localfunc() => Resolver.GetQuickInfo(_ParseResult, line + 1, col + 1, _DisplayInfoProvider);
		IBMetadataProviderProvider metadataProviderProvider = _Source.GetMetadataProviderProvider();

		if (metadataProviderProvider != null)
		{
			IAsyncResult asyncResult = metadataProviderProvider.BinderQueue.EnqueueUIThreadAction(localfunc);

			if (asyncResult.AsyncWaitHandle.WaitOne(LsbLanguageService.C_UIThreadWaitMilliseconds) && asyncResult.IsCompleted)
			{
				codeObjectQuickInfo = asyncResult.AsyncState as CodeObjectQuickInfo;
			}
		}
		if (codeObjectQuickInfo != null)
		{
			span.iStartLine = codeObjectQuickInfo.StartLocation.LineNumber - 1;
			span.iStartIndex = codeObjectQuickInfo.StartLocation.ColumnNumber - 1;
			span.iEndLine = codeObjectQuickInfo.EndLocation.LineNumber - 1;
			span.iEndIndex = codeObjectQuickInfo.EndLocation.ColumnNumber - 1;
			result = codeObjectQuickInfo.Text;
		}

		return result;
	}

	public override Microsoft.VisualStudio.Package.Declarations GetDeclarations(IVsTextView view,
		int line, int col, Microsoft.VisualStudio.Package.TokenInfo info, ParseReason reason)
	{
		switch (reason)
		{
			case ParseReason.MemberSelect:
			case ParseReason.MemberSelectAndHighlightBraces:
			case ParseReason.CompleteWord:
			case ParseReason.DisplayMemberList:
			case ParseReason.MethodTip:
				LsbSource source = LanguageExtensionPackage.Instance.LanguageService.GetSource(view) as LsbSource;

				// TraceUtils.Trace(GetType(), "GetDeclarations()", "GetDeclarations() started... (ThreadName = " + Thread.CurrentThread.Name + ")");
				LsbDeclarations declarations;
				if (source.CompletionSet.IsDisplayed)
				{
					declarations = source.CompletionSet.Declarations as LsbDeclarations;
				}
				else
				{
					IList<Declaration> list = null;

					IBMetadataProviderProvider metadataProviderProvider = source.GetMetadataProviderProvider();
					if (metadataProviderProvider != null)
					{
						object localfunc() => Resolver.FindCompletions(_ParseResult, line + 1, col + 1, _DisplayInfoProvider);
						IAsyncResult asyncResult = metadataProviderProvider.BinderQueue.EnqueueUIThreadAction(localfunc);

						if (asyncResult.AsyncWaitHandle.WaitOne(LsbLanguageService.C_UIThreadWaitMilliseconds) && asyncResult.IsCompleted)
						{
							list = asyncResult.AsyncState as IList<Declaration>;
						}
					}
					declarations = ((list != null) ? new(list, source) : new(new List<Declaration>(0), source));
				}
				if (LsbLanguageServiceTestEvents.Instance.EnableTestEvents)
				{
					// TraceUtils.Trace(GetType(), "GetDeclarations()", "raising DeclarationsRequestedEvent (ThreadName = " + Thread.CurrentThread.Name + ")");
					LsbLanguageServiceTestEvents.Instance.RaiseDeclarationsRequestedEvent(declarations, view, line, col, info, reason);
					// TraceUtils.Trace(GetType(), "GetDeclarations()", "raised DeclarationsRequestedEvent (ThreadName = " + Thread.CurrentThread.Name + ")");
				}
				// TraceUtils.Trace(GetType(), "GetDeclarations()", "GetDeclarations() ending  (ThreadName = " + Thread.CurrentThread.Name + ")");
				return declarations;
			default:
				throw new ArgumentOutOfRangeException("reason");
		}
	}

	public override Microsoft.VisualStudio.Package.Methods GetMethods(int line, int col, string name)
	{
		return new LsbMethods(Resolver.FindMethods(_ParseResult, line + 1, col + 2, _DisplayInfoProvider));
	}

	public override string Goto(VSConstants.VSStd97CmdID cmd, IVsTextView textView, int line, int col, out TextSpan span)
	{
		span = default;
		return null;
	}
}
