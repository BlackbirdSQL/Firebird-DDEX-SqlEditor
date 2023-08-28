#region Assembly Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections.Generic;
using System.Threading;

using Babel;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Common.Interfaces;

using Microsoft.SqlServer.Management.SqlParser.Intellisense;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;
using Microsoft.SqlServer.Management.SqlParser.Parser;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;




namespace BlackbirdSql.LanguageExtension;


internal class AuthoringScope : Microsoft.VisualStudio.Package.AuthoringScope
{
	private readonly ParseResult parseResult;

	private readonly MetadataDisplayInfoProvider displayInfoProvider;

	private readonly SourceAgent _source;

	public AuthoringScope(ParseResult parseResult, MetadataDisplayInfoProvider displayInfoProvider, SourceAgent source)
	{
		_source = source;
		this.parseResult = parseResult;
		this.displayInfoProvider = displayInfoProvider;
	}

	public override string GetDataTipText(int line, int col, out TextSpan span)
	{
		string result = null;
		span = default;
		CodeObjectQuickInfo codeObjectQuickInfo = null;

		object f() => Resolver.GetQuickInfo(parseResult, line + 1, col + 1, displayInfoProvider);

		IMetadataProviderProvider metadataProviderProvider = _source.GetMetadataProviderProvider();
		if (metadataProviderProvider != null)
		{
			IAsyncResult asyncResult = metadataProviderProvider.BinderQueue.EnqueueUIThreadAction(f);
			if (asyncResult.AsyncWaitHandle.WaitOne(AbstractLanguageService.UIThreadWaitMilliseconds) && asyncResult.IsCompleted)
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

	public override Microsoft.VisualStudio.Package.Declarations GetDeclarations(IVsTextView view, int line, int col, Microsoft.VisualStudio.Package.TokenInfo info, ParseReason reason)
	{
		Tracer.Trace(GetType(), "GetDeclarations()", "GetDeclarations() started... (ThreadName = " + Thread.CurrentThread.Name + ")");
		Declarations declarations = null;
		switch (reason)
		{
			case ParseReason.MemberSelect:
			case ParseReason.MemberSelectAndHighlightBraces:
			case ParseReason.CompleteWord:
			case ParseReason.DisplayMemberList:
			case ParseReason.MethodTip:
				{
					IBPackageController controller = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(IBPackageController)) as IBPackageController;

					SourceAgent source = ((IBLanguageExtensionAsyncPackage)controller.DdexPackage).LanguageService.GetSource(view) as SourceAgent;
					if (source.CompletionSet.IsDisplayed)
					{
						declarations = source.CompletionSet.Declarations as Declarations;
					}
					else
					{
						IList<Declaration> list = null;
						IMetadataProviderProvider metadataProviderProvider = source.GetMetadataProviderProvider();
						if (metadataProviderProvider != null)
						{
							object f() => Resolver.FindCompletions(parseResult, line + 1, col + 1, displayInfoProvider);

							IAsyncResult asyncResult = metadataProviderProvider.BinderQueue.EnqueueUIThreadAction(f);
							if (asyncResult.AsyncWaitHandle.WaitOne(AbstractLanguageService.UIThreadWaitMilliseconds) && asyncResult.IsCompleted)
							{
								list = asyncResult.AsyncState as IList<Declaration>;
							}
						}

						declarations = ((list != null) ? new Declarations(list, source) : new Declarations(new List<Declaration>(0), source));
					}

					/*
					if (AbstractLanguageService.EnableTestEvents)
					{
						Tracer.Trace(GetType(), "GetDeclarations()", "raising DeclarationsRequestedEvent (ThreadName = " + Thread.CurrentThread.Name + ")");
						Ns.LanguageServiceTestEvents.Instance.RaiseDeclarationsRequestedEvent(declarations, view, line, col, info, reason);
						Tracer.Trace(GetType(), "GetDeclarations()", "raised DeclarationsRequestedEvent (ThreadName = " + Thread.CurrentThread.Name + ")");
					}
					*/

					Tracer.Trace(GetType(), "GetDeclarations()", "GetDeclarations() ending  (ThreadName = " + Thread.CurrentThread.Name + ")");

					return declarations;
				}
			default:
				ArgumentOutOfRangeException ex = new("reason");
				Diag.Dug(ex);
				throw ex;
		}
	}

	public override Microsoft.VisualStudio.Package.Methods GetMethods(int line, int col, string name)
	{
		return new Methods(Resolver.FindMethods(parseResult, line + 1, col + 2, displayInfoProvider));
	}

	public override string Goto(VSConstants.VSStd97CmdID cmd, IVsTextView textView, int line, int col, out TextSpan span)
	{
		span = default;
		return null;
	}
}
