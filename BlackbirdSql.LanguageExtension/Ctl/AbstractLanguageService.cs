#region Assembly Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion


using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using Babel;

using BlackbirdSql.Core;
using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Common.Interfaces;
using BlackbirdSql.LanguageExtension.ColorService;
using BlackbirdSql.LanguageExtension.Properties;

using Microsoft.SqlServer.Management.SqlParser.Intellisense;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;
using Microsoft.SqlServer.Management.SqlParser.Parser;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

using Ns = Microsoft.VisualStudio.Data.Tools.SqlLanguageServices;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;



namespace BlackbirdSql.LanguageExtension;


public class AbstractLanguageService : Microsoft.VisualStudio.Package.LanguageService, IBLanguageService, IVsLanguageBlock
{
	// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices.Package

	internal class CustomViewFilter : ViewFilter
	{
		// private readonly AbstractLanguageService _SqlLanguageService;

		internal CustomViewFilter(AbstractLanguageService service, CodeWindowManager mgr, IVsTextView view)
			: base(mgr, view)
		{
			// _SqlLanguageService = service;
			_ = service;
		}

		protected override int QueryCommandStatus(ref Guid guidCmdGroup, uint nCmdId)
		{

			if (guidCmdGroup == VSConstants.CMDSETID.StandardCommandSet2K_guid && (nCmdId == (uint)VSConstants.VSStd2KCmdID.FORMATSELECTION || nCmdId == (uint)VSConstants.VSStd2KCmdID.INSERTSNIPPET || nCmdId == (uint)VSConstants.VSStd2KCmdID.SURROUNDWITH))
			{
				return VSConstants.E_FAIL;
			}

			if (guidCmdGroup == VSConstants.CMDSETID.StandardCommandSet97_guid && (nCmdId - (int)VSConstants.VSStd97CmdID.ExtToolsSlnFileName <= 0 || nCmdId == (uint)VSConstants.VSStd97CmdID.GotoRef || nCmdId == (uint)VSConstants.VSStd97CmdID.FindReferences))
			{
				return VSConstants.E_FAIL;
			}

			if (guidCmdGroup == VSConstants.CMDSETID.StandardCommandSet2K_guid)
			{
				switch ((VSConstants.VSStd2KCmdID)nCmdId)
				{
					case VSConstants.VSStd2KCmdID.OUTLN_COLLAPSE_TO_DEF:
						return 16;
					case VSConstants.VSStd2KCmdID.OUTLN_TOGGLE_CURRENT:
					case VSConstants.VSStd2KCmdID.OUTLN_TOGGLE_ALL:
					case VSConstants.VSStd2KCmdID.OUTLN_STOP_HIDING_ALL:
						if (base.Source.OutliningEnabled)
						{
							return (int)(OleConstants.MSOCMDF_SUPPORTED | OleConstants.MSOCMDF_ENABLED);
						}

						return 16;
					case VSConstants.VSStd2KCmdID.OUTLN_START_AUTOHIDING:
						if (base.Source.OutliningEnabled)
						{
							return 16;
						}

						return (int)(OleConstants.MSOCMDF_SUPPORTED | OleConstants.MSOCMDF_ENABLED);
					case VSConstants.VSStd2KCmdID.ToggleConsumeFirstCompletionMode:
						{
							SqlCompletionSet obj = base.Source.CompletionSet as SqlCompletionSet;
							int num = (int)(OleConstants.MSOCMDF_SUPPORTED | OleConstants.MSOCMDF_ENABLED);
							if (obj.InPreviewMode)
							{
								num |= (int)OleConstants.MSOCMDF_LATCHED;
							}

							return num;
						}
				}
			}

			return base.QueryCommandStatus(ref guidCmdGroup, nCmdId);
		}

		private void InvokeSnippetBrowser(string prompt, string[] snippetTypes)
		{
			ExpansionProvider expansionProvider = GetExpansionProvider();
			if (expansionProvider != null && base.TextView != null)
			{
				expansionProvider.DisplayExpansionBrowser(base.TextView, prompt, snippetTypes, includeNullType: true, null, includeNullKind: true);
			}
		}

		private static string[] GetExpansionTypes(uint cmd)
		{
			string text = ((cmd == 323) ? "Expansion" : "SurroundsWith");
			return new string[1] { text };
		}

		public override bool HandlePreExec(ref Guid guidCmdGroup, uint nCmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
		{
			if (guidCmdGroup == VSConstants.CMDSETID.StandardCommandSet2K_guid)
			{
				switch ((VSConstants.VSStd2KCmdID)nCmdId)
				{
					case VSConstants.VSStd2KCmdID.INSERTSNIPPET:
						InvokeSnippetBrowser(Resources.InsertSnippet, GetExpansionTypes(nCmdId));
						return true;
					case VSConstants.VSStd2KCmdID.SURROUNDWITH:
						InvokeSnippetBrowser(Resources.SurroundWith, GetExpansionTypes(nCmdId));
						return true;
					case VSConstants.VSStd2KCmdID.ToggleConsumeFirstCompletionMode:
						{
							SqlCompletionSet sqlCompletionSet = (base.Source as Source).CompletionSet as SqlCompletionSet;
							sqlCompletionSet.InPreviewMode = !sqlCompletionSet.InPreviewMode;
							sqlCompletionSet.InPreviewModeOutline = sqlCompletionSet.InPreviewMode;
							if (sqlCompletionSet.IsDisplayed)
							{
								Native.ThrowOnFailure(base.TextView.UpdateCompletionStatus(sqlCompletionSet, (uint)UpdateCompletionFlags.UCS_NAMESCHANGED));
							}

							return true;
						}
				}
			}

			if (guidCmdGroup == VSConstants.CMDSETID.StandardCommandSet97_guid && nCmdId == 1915)
			{
				HandleGotoReference();
				return true;
			}

			if (guidCmdGroup == (new Guid(VS.SqlEditorCommandsGuid)) && nCmdId == 70)
			{
				return true;
			}

			return base.HandlePreExec(ref guidCmdGroup, nCmdId, nCmdexecopt, pvaIn, pvaOut);
		}

		private void HandleGotoReference()
		{
			// ServiceProviderUtil.GetService<ISqlEditorServices>(base.TextView)?.HandleGotoReference();
		}

		public override void HandleGoto(VSConstants.VSStd97CmdID cmd)
		{
			base.HandleGoto(cmd);
			/*
			ISqlEditorServices sqlEditorServices = ServiceProviderUtil.GetService<ISqlEditorServices>(base.TextView);
			if (sqlEditorServices != null && cmd == VSConstants.VSStd97CmdID.GotoDefn)
			{
				sqlEditorServices.HandleGotoDefinition();
			}
			*/
		}
	}

	private static readonly Guid _ExpressionEvaluatorClsid = new Guid(SystemData.MandatedExpressionEvaluatorGuid);

	private static IBPackageController _PackageController = null;


	private Ns.SqlLanguagePreferences _SqlPreferences;

	private readonly MetadataDisplayInfoProvider displayInfoProvider;

	private readonly NoOpAuthoringScope _NoOpAuthoringScope;

	private const string C_DefaultPrefix = "SQL80001: ";

	internal static readonly int UIThreadWaitMilliseconds = 500;

	internal static readonly int BinderWaitMilliseconds = 2000;

	public override string Name => "SQL Server Tools";



	internal IBPackageController PackageController
	{
		get
		{
			_PackageController ??= GetService(typeof(IBPackageController)) as IBPackageController;
			return _PackageController;
		}
	}

	internal Ns.SqlLanguagePreferences SqlLanguagePreferences => GetLanguagePreferences() as Ns.SqlLanguagePreferences;

	public override int GetColorableItem(int index, out IVsColorableItem item)
	{
		if (ColorConfiguration.ColorableItems.TryGetValue(index, out item))
		{
			return VSConstants.S_OK;
		}

		item = ColorConfiguration.ColorableItems.Values.FirstOrDefault();
		return 1;
	}

	public override int GetItemCount(out int count)
	{
		count = ColorConfiguration.ColorableItems.Count;
		return VSConstants.S_OK;
	}


	public override Microsoft.VisualStudio.Package.Source CreateSource(IVsTextLines buffer)
	{
		return new SourceAgent(this, buffer, GetColorizer(buffer));
	}

	public override IScanner GetScanner(IVsTextLines buffer)
	{
		return new LineScanner();
	}

	public override void OnIdle(bool periodic)
	{
		/*
		// We don't need this
		if (base.LastActiveTextView != null)
		{
			SourceAgent source = (SourceAgent)GetSource(base.LastActiveTextView);
			if (source != null)
			{
				if (!source.CompletedFirstParse)
				{
					source.LastParseTime = 0;
				}

				IMetadataProviderProvider metadataProviderProvider = source.GetMetadataProviderProvider();
				if (metadataProviderProvider != null)
				{
					AuxiliaryDocData auxillaryDocData = EditorExtensionAsyncPackage.Instance.GetAuxiliaryDocData(source.GetTextLines());
					if (auxillaryDocData.QueryExecutor.IsConnected)
					{
						Ns.SmoMetadataProviderProvider smoMetadataProviderProvider = metadataProviderProvider as Ns.SmoMetadataProviderProvider;
						if (smoMetadataProviderProvider != null)
						{
							IDbConnection connection = auxillaryDocData.QueryExecutor.ConnectionStrategy.Connection;
							if (connection != null && !string.IsNullOrEmpty(connection.Database))
							{
								smoMetadataProviderProvider.AsyncAddDatabaseToDriftDetectionSet(connection.Database);
							}
						}
					}
				}
			}
		}
		*/

		base.OnIdle(periodic);
	}

	public override int GetLanguageID(IVsTextBuffer buffer, int line, int col, out Guid langId)
	{
		Tracer.Trace(GetType(), "LanguageService.GetLanguageID", "buffer: {0}, line: {1}, col: {2}", buffer, line, col);
		langId = _ExpressionEvaluatorClsid;

		return VSConstants.S_OK;
	}

	public override int GetLocationOfName(string name, out string pbstrMkDoc, TextSpan[] spans)
	{
		Tracer.Trace(GetType(), "LanguageService.GetLocationOfName", "name: {0}, spans: {1}", name, spans);
		return base.GetLocationOfName(name, out pbstrMkDoc, spans);
	}

	public override int GetNameOfLocation(IVsTextBuffer buffer, int line, int col, out string name, out int lineOffset)
	{
		Tracer.Trace(GetType(), "LanguageService.GetNameOfLocation", "buffer: {0}, line: {1}, col: {2}", buffer, line, col);
		return base.GetNameOfLocation(buffer, line, col, out name, out lineOffset);
	}

	public override int GetProximityExpressions(IVsTextBuffer buffer, int line, int col, int cLines, out Microsoft.VisualStudio.TextManager.Interop.IVsEnumBSTR ppEnum)
	{
		Tracer.Trace(GetType(), "LanguageService.GetProximityExpressions", "buffer: {0}, line: {1}, col: {2}, cLines: {3}", buffer, line, col, cLines);
		return base.GetProximityExpressions(buffer, line, col, cLines, out ppEnum);
	}

	public override int IsMappedLocation(IVsTextBuffer buffer, int line, int col)
	{
	 Tracer.Trace(GetType(), "LanguageService.IsMappedLocation", "buffer: {0}, line: {1}, col: {2}", buffer, line, col);
		return base.IsMappedLocation(buffer, line, col);
	}

	public override int ResolveName(string name, uint flags, out IVsEnumDebugName ppNames)
	{
	 Tracer.Trace(GetType(), "LanguageService.ResolveName", "name: {0}, flags: {1}", name, flags);
		return base.ResolveName(name, flags, out ppNames);
	}

	public override int ValidateBreakpointLocation(IVsTextBuffer buffer, int line, int col, TextSpan[] pCodeSpan)
	{
	 Tracer.Trace(GetType(), "LanguageService.ValidateBreakpointLocation", "buffer: {0}, line: {1}, col: {2}, pCodeSpan: {3}", buffer, line, col, pCodeSpan);
		IVsTextLines obj = buffer as IVsTextLines;
		Native.ThrowOnFailure(obj.GetLastLineIndex(out var piLine, out var piIndex));
		Native.ThrowOnFailure(obj.GetLineText(0, 0, piLine, piIndex, out var pbstrBuf));
		Resolver.BlockInformation blockInformation = Resolver.FindBreakPointInformation(Parser.Parse(pbstrBuf), line + 1, col + 1);
		if (pCodeSpan != null && blockInformation != null)
		{
			pCodeSpan[0].iStartLine = blockInformation.Start.LineNumber - 1;
			pCodeSpan[0].iEndLine = blockInformation.End.LineNumber - 1;
			pCodeSpan[0].iStartIndex = blockInformation.Start.ColumnNumber - 1;
			pCodeSpan[0].iEndIndex = blockInformation.End.ColumnNumber - 1;
			return VSConstants.S_OK;
		}

		return 1;
	}

	public AbstractLanguageService()
	{
		displayInfoProvider = new MetadataDisplayInfoProvider();
		_NoOpAuthoringScope = new NoOpAuthoringScope(displayInfoProvider);
	}

	public override LanguagePreferences GetLanguagePreferences()
	{
		_SqlPreferences ??= ((LanguageExtensionAsyncPackage)PackageController.DdexPackage).GetLanguagePreferences();

		return _SqlPreferences;
	}

	public override ViewFilter CreateViewFilter(CodeWindowManager mgr, IVsTextView newView)
	{
		return new CustomViewFilter(this, mgr, newView);
	}

	internal void RefreshIntellisense(bool currentWindowOnly)
	{
		IVsTextView lastActiveTextView = base.LastActiveTextView;
		SourceAgent source = GetSource(lastActiveTextView) as SourceAgent;
		foreach (SourceAgent source2 in GetSources())
		{
			source2.IsDirty = true;
			source2.Reset();
			if (!currentWindowOnly && source2 != source)
			{
				source2.GetTaskProvider().Tasks.Clear();
			}
		}
	}

	public override ImageList GetImageList()
	{
		return new ImageList
		{
			Images =
			{
			 	(Icon)Resources.ResourceManager.GetObject("AsymmetricKey"),
				(Icon)Resources.ResourceManager.GetObject("Certificate"),
				(Icon)Resources.ResourceManager.GetObject("Column"),
				(Icon)Resources.ResourceManager.GetObject("Credential"),
				(Icon)Resources.ResourceManager.GetObject("Database"),
				(Icon)Resources.ResourceManager.GetObject("Login"),
				(Icon)Resources.ResourceManager.GetObject("ScalarValuedFunction"),
				(Icon)Resources.ResourceManager.GetObject("Schema"),
				(Icon)Resources.ResourceManager.GetObject("StoredProcedure"),
				(Icon)Resources.ResourceManager.GetObject("Table"),
				(Icon)Resources.ResourceManager.GetObject("TableValuedFunction"),
				(Icon)Resources.ResourceManager.GetObject("User"),
				(Icon)Resources.ResourceManager.GetObject("Variable"),
				(Icon)Resources.ResourceManager.GetObject("View")
			}
		};
	}

	public override Microsoft.VisualStudio.Package.AuthoringScope ParseSource(ParseRequest req)
	{
		SourceAgent source = ((req.FileName != null) ? ((SourceAgent)GetSource(req.FileName)) : null);
		Microsoft.VisualStudio.Package.AuthoringScope authoringScope = null;
		if (source != null && source.TryEnterDisposeLock())
		{
			try
			{
				if (AbleToParseSource(req, source))
				{
					bool hasChanged = source.ExecuteParseRequest(req.Text);
					ParseResult parseResult = source.ParseResult;
					if (parseResult != null)
					{
						authoringScope = ProcessParseResult(req, source, hasChanged, parseResult);
					}
				}
			}
			finally
			{
				source.ExitDisposeLock();
			}
		}

		if (authoringScope == null)
		{
			if (req.Sink.HiddenRegions)
			{
				req.Sink.ProcessHiddenRegions = true;
			}

			authoringScope = _NoOpAuthoringScope;
		}

		return authoringScope;
	}

	private bool AbleToParseSource(ParseRequest req, SourceAgent source)
	{
		bool result = true;
		int maxScriptSize = _SqlPreferences.MaxScriptSize;
		bool flag = req.Text.Length > maxScriptSize;
		// IMetadataProviderProvider metadataProviderProvider;
		if (!source.IsDisconnectedMode && !source.IsServerSupported)
		{
			result = false;
			req.Sink.AddError(req.FileName, Resources.IntellisenseAvailableForKatmaiOnly, default, Severity.Hint);
		}
		/*
		else if (!Ns.Package.Instance.LanguageService.Ns.SqlLanguagePreferences.EnableAzureIntellisense && (metadataProviderProvider = source.GetMetadataProviderProvider()) != null && metadataProviderProvider is Ns.SmoMetadataProviderProvider && ((Ns.SmoMetadataProviderProvider)metadataProviderProvider).IsCloudConnection)
		{
			result = false;
			req.Sink.AddError(req.FileName, Resources.IntellisenseAvailableForKatmaiOnly, default(TextSpan), Severity.Hint);
		}
		*/
		else if (!source.IntelliSenseEnabled)
		{
			result = false;
		}
		else if (source.IsSqlCmdModeEnabled)
		{
			result = false;
			req.Sink.AddError(req.FileName, Resources.IntellisenseDisabledForSqlCmdMode, default, Severity.Hint);
		}
		else if (flag)
		{
			result = false;
			req.Sink.AddError(req.FileName, Resources.FileTooBig, default, Severity.Hint);
		}

		return result;
	}

	private AuthoringScope ProcessParseResult(ParseRequest req, SourceAgent source, bool hasChanged, ParseResult parseResult)
	{
		bool isReallyDirty = source.IsReallyDirty;
		if (req.Reason == ParseReason.Check && _SqlPreferences.UnderlineErrors)
		{
			foreach (Error error in source.Errors)
			{
				if (error.Type != ErrorType.BindError)
				{
					req.Sink.AddError(req.FileName, C_DefaultPrefix + error.Message, GetTextSpan(error.Start, error.End), error.IsWarning ? Severity.Warning : Severity.Error);
				}
			}
		}

		if (req.Sink.HiddenRegions && (hasChanged || source.HasPendingRegions))
		{
			source.HasPendingRegions = true;
			req.Sink.ProcessHiddenRegions = false;
			if (source.OutliningEnabled)
			{
                Babel.Region region = null;
				if (source.HiddenRegions != null)
				{
					foreach (Babel.Region hiddenRegion in source.HiddenRegions)
					{
						TextSpan textSpan = GetTextSpan(hiddenRegion.StartLocation, hiddenRegion.EndLocation);
						if (region == null || IsDifferentRegion(hiddenRegion, region))
						{
							req.Sink.AddHiddenRegion(textSpan);
							region = hiddenRegion;
						}
					}
				}
			}

			req.Sink.ProcessHiddenRegions = true;
			source.HasPendingRegions = false;
		}

		switch (req.Reason)
		{
			case ParseReason.MatchBraces:
				MatchPair(source, req.Sink, isReallyDirty, highlight: false);
				break;
			case ParseReason.HighlightBraces:
			case ParseReason.MemberSelectAndHighlightBraces:
				MatchPair(source, req.Sink, isReallyDirty, highlight: true);
				break;
			case ParseReason.MethodTip:
				PopulateNamesAndParams(req, parseResult);
				break;
		}

		if (_SqlPreferences.TextCasing == 0)
		{
			displayInfoProvider.BuiltInCasing = CasingStyle.Uppercase;
		}
		else
		{
			displayInfoProvider.BuiltInCasing = CasingStyle.Lowercase;
		}

		return new AuthoringScope(parseResult, displayInfoProvider, source);
	}

	private void MatchPair(SourceAgent source, AuthoringSink sink, bool isDirty, bool highlight)
	{
		ParseResult parseResult = source.ParseResult;
		int num = sink.Line + 1;
		int num2 = sink.Column + 1;
		PairMatch pairMatch = Resolver.FindPairMatch(parseResult, num, num2);
		if (pairMatch == null)
		{
			return;
		}

		bool flag = true;
		bool flag2 = pairMatch.StartToken.Id == 40;
		if (highlight)
		{
			if (flag2)
			{
				flag = pairMatch.StartToken.EndLocation.ColumnNumber != num2 || pairMatch.StartToken.EndLocation.LineNumber != num;
			}
			else
			{
				flag = false;
				if (isDirty)
				{
					flag = sink.Line >= source.DirtySpan.iStartLine && sink.Line <= source.DirtySpan.iEndLine && sink.Column >= source.DirtySpan.iStartIndex && sink.Column <= source.DirtySpan.iEndIndex;
				}
			}
		}

		if (flag)
		{
			Token startToken = pairMatch.StartToken;
			Token endToken = pairMatch.EndToken;
			TextSpan textSpan = GetTextSpan(startToken.StartLocation, startToken.EndLocation);
			TextSpan endContext = ((!(!highlight && flag2) || !pairMatch.IsStartEnd) ? GetTextSpan(endToken.StartLocation, endToken.EndLocation) : GetTextSpan(endToken.EndLocation));
			sink.MatchPair(textSpan, endContext, 1);
		}
	}

	private void PopulateNamesAndParams(ParseRequest request, ParseResult parseResult)
	{
		MethodNameAndParamLocations methodNameAndParams = Resolver.GetMethodNameAndParams(parseResult, request.Line + 1, request.Col + 1, displayInfoProvider);
		if (methodNameAndParams == null)
		{
			return;
		}

		AuthoringSink sink = request.Sink;
		sink.StartName(GetTextSpan(methodNameAndParams.NameStartLocation, methodNameAndParams.NameEndLocation), methodNameAndParams.Name);
		if (methodNameAndParams.ParamStartLocation.HasValue)
		{
			sink.StartParameters(GetTextSpan(methodNameAndParams.ParamStartLocation.Value));
		}

		foreach (Location paramSeperatorLocation in methodNameAndParams.ParamSeperatorLocations)
		{
			sink.NextParameter(GetTextSpan(paramSeperatorLocation));
		}

		if (methodNameAndParams.ParamEndLocation.HasValue)
		{
			sink.EndParameters(GetTextSpan(methodNameAndParams.ParamEndLocation.Value));
		}
	}

	private static TextSpan GetTextSpan(Location start, Location end)
	{
		TextSpan result = default;
		result.iStartLine = start.LineNumber - 1;
		result.iEndLine = end.LineNumber - 1;
		result.iStartIndex = start.ColumnNumber - 1;
		result.iEndIndex = end.ColumnNumber - 1;
		return result;
	}

	private static TextSpan GetTextSpan(Location location)
	{
		TextSpan result = default;
		result.iStartLine = location.LineNumber - 1;
		result.iEndLine = location.LineNumber - 1;
		result.iStartIndex = location.ColumnNumber - 1;
		result.iEndIndex = location.ColumnNumber;
		return result;
	}

	private bool IsDifferentRegion(Babel.Region currentRegion, Babel.Region prevRegion)
	{
		if (currentRegion.StartLocation.CompareTo(prevRegion.StartLocation) == 0 && currentRegion.EndLocation.CompareTo(prevRegion.EndLocation) == 0)
		{
			return false;
		}

		return true;
	}

	public override string GetFormatFilterList()
	{
		return string.Format(Resources.FormatFilterList, MonikerAgent.C_SqlExtension) + $"\n*{MonikerAgent.C_SqlExtension}\n";
	}

	public override void UpdateLanguageContext(LanguageContextHint hint, IVsTextLines buffer, TextSpan[] ptsSelection, IVsUserContext context)
	{
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
		context.RemoveAttribute(null, null);
		context.AddAttribute(VSUSERCONTEXTATTRIBUTEUSAGE.VSUC_Usage_Filter, "DevLang", "TSQL");
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
		TextSpan textSpan = ptsSelection[0];
		buffer.GetLineText(textSpan.iStartLine, textSpan.iStartIndex, textSpan.iEndLine, textSpan.iEndIndex, out var pbstrBuf);
		if (string.IsNullOrEmpty(pbstrBuf))
		{
			textSpan = ComputeWordExtent(buffer, textSpan.iStartLine, textSpan.iStartIndex);
			if (textSpan.iStartIndex == textSpan.iEndIndex && textSpan.iStartLine == textSpan.iEndLine)
			{
				return;
			}

			buffer.GetLineText(textSpan.iStartLine, textSpan.iStartIndex, textSpan.iEndLine, textSpan.iEndIndex, out pbstrBuf);
		}

		if (pbstrBuf.Trim().Length > 0)
		{
			string szValue = Regex.Replace(pbstrBuf.Trim(), "\\s+", "_") + "_TSQL";
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
			context.AddAttribute(VSUSERCONTEXTATTRIBUTEUSAGE.VSUC_Usage_LookupF1, "keyword", szValue);
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
		}
	}

	private static bool CanBePartOfSqlIdentifier(char ch)
	{
		if ((ch < 'a' || ch > 'z') && (ch < 'A' || ch > 'Z') && (ch < '0' || ch > '9') && ch != '_' && ch != '$' && ch != '@' && ch != '#')
		{
			return ch >= 'À';
		}

		return true;
	}

	private TextSpan ComputeWordExtent(IVsTextLines buffer, int line, int index)
	{
		TextSpan result = default;
		buffer.GetLengthOfLine(line, out var piLength);
		buffer.GetLineText(line, 0, line, piLength, out var pbstrBuf);
		if (pbstrBuf.Length == 0)
		{
			return result;
		}

		if (index > 0 && CanBePartOfSqlIdentifier(pbstrBuf[index - 1]))
		{
			index--;
		}

		if (index == piLength)
		{
			return result;
		}

		if (!CanBePartOfSqlIdentifier(pbstrBuf[index]))
		{
			result.iStartIndex = index;
			result.iStartLine = line;
			result.iEndIndex = index + 1;
			result.iEndLine = line;
			return result;
		}

		int num = index;
		while (num > 0 && CanBePartOfSqlIdentifier(pbstrBuf[num - 1]))
		{
			num--;
		}

		int i;
		for (i = index; i < piLength && CanBePartOfSqlIdentifier(pbstrBuf[i]); i++)
		{
		}

		if (num > 0 && i < pbstrBuf.Length && pbstrBuf[num] != '@' && pbstrBuf[num] != '#' && pbstrBuf[num - 1] == '[' && pbstrBuf[i] == ']')
		{
			num--;
			i++;
		}

		result.iStartIndex = num;
		result.iStartLine = line;
		result.iEndIndex = i;
		result.iEndLine = line;
		return result;
	}

	public int GetCurrentBlock(IVsTextLines pTextLines, int iCurrentLine, int iCurrentChar, TextSpan[] ptsBlockSpan, out string pbstrDescription, out int pfBlockAvailable)
	{
		SourceAgent source = (SourceAgent)GetSource(pTextLines);
		if (source == null || source.ParseResult == null)
		{
			pbstrDescription = null;
			pfBlockAvailable = 0;
			return VSConstants.S_OK;
		}

		Resolver.BlockInformation blockInformation = Resolver.GetBlockInformation(source.ParseResult, iCurrentLine + 1, iCurrentChar + 1);
		if (ptsBlockSpan != null)
		{
			ptsBlockSpan[0] = GetTextSpan(blockInformation.Start, blockInformation.End);
		}

		pbstrDescription = blockInformation.Description;
		pfBlockAvailable = -1;
		return VSConstants.S_OK;
	}
}
