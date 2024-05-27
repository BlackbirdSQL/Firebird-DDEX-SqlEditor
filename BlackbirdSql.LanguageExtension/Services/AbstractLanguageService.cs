// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices.LanguageService
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Babel;
using BlackbirdSql.LanguageExtension.Ctl;
using BlackbirdSql.LanguageExtension.Ctl.Config;
using BlackbirdSql.LanguageExtension.Properties;
using Microsoft.SqlServer.Management.SqlParser.Intellisense;
using Microsoft.SqlServer.Management.SqlParser.Parser;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

using CasingStyle = Microsoft.SqlServer.Management.SqlParser.MetadataProvider.CasingStyle;
using MetadataDisplayInfoProvider = Microsoft.SqlServer.Management.SqlParser.MetadataProvider.MetadataDisplayInfoProvider;



namespace BlackbirdSql.LanguageExtension.Services;


// =========================================================================================================
//										AbstractLanguageService Class 
//
/// <summary>
/// BlackbirdSql Language Service base class. This class abstraction handles all legacy SSDT functionality.
/// </summary>
// =========================================================================================================
public abstract class AbstractLanguageService : LanguageService, IVsLanguageBlock
{

	// ---------------------------------------------------------
	#region Constructors / Destructors - AbstractLanguageService
	// ----------------------------------------------------------


	public AbstractLanguageService(object site) : base()
	{
		SetSite(site);

		_Package = (Package)site;
		_DisplayInfoProvider = new MetadataDisplayInfoProvider();
		_NoOpAuthoringScope = new LsbNoOpAuthoringScope(_DisplayInfoProvider);
	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Private classes - AbstractLanguageService
	// =========================================================================================================


	public class CustomViewFilter : ViewFilter
	{
		private static Guid _ClsidSqlLanguageServiceCommands = new(VS.SqlEditorCommandsGuid);

		private readonly AbstractLanguageService _SqlLanguageService;

		public CustomViewFilter(AbstractLanguageService service, CodeWindowManager mgr, IVsTextView view)
			: base(mgr, view)
		{
			_SqlLanguageService = service;
		}

		protected override int QueryCommandStatus(ref Guid guidCmdGroup, uint nCmdId)
		{
			VSConstants.VSStd2KCmdID cmdId = (VSConstants.VSStd2KCmdID)nCmdId;

			if (guidCmdGroup == VSConstants.VSStd2K && (nCmdId == 112 || nCmdId == 323 || nCmdId == 1561))
			{
				return VSConstants.E_FAIL;
			}
			if (guidCmdGroup == VSConstants.GUID_VSStandardCommandSet97 && (nCmdId - 935 <= 1 || nCmdId == 1107 || nCmdId == 1915))
			{
				return VSConstants.E_FAIL;
			}
			if (guidCmdGroup == VSConstants.VSStd2K)
			{
				switch (cmdId)
				{
					case VSConstants.VSStd2KCmdID.OUTLN_COLLAPSE_TO_DEF:
						return (int)OLECMDF.OLECMDF_INVISIBLE;
					case VSConstants.VSStd2KCmdID.OUTLN_TOGGLE_CURRENT:
					case VSConstants.VSStd2KCmdID.OUTLN_TOGGLE_ALL:
					case VSConstants.VSStd2KCmdID.OUTLN_STOP_HIDING_ALL:
						if (Source.OutliningEnabled)
						{
							return (int)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
						}
						return 16;
					case VSConstants.VSStd2KCmdID.OUTLN_START_AUTOHIDING:
						if (Source.OutliningEnabled)
						{
							return 16;
						}
						return 3;
					case VSConstants.VSStd2KCmdID.ToggleConsumeFirstCompletionMode:
						{
							LsbCompletionSet obj = Source.CompletionSet as LsbCompletionSet;
							int num = 3;
							if (obj.InPreviewMode)
							{
								num |= 4;
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
			if (expansionProvider != null && TextView != null)
			{
				expansionProvider.DisplayExpansionBrowser(TextView, prompt, snippetTypes, includeNullType: true, null, includeNullKind: true);
			}
		}

		private static string[] GetExpansionTypes(uint cmd)
		{
			string text = cmd == 323 ? "Expansion" : "SurroundsWith";
			return [text];
		}

		public override bool HandlePreExec(ref Guid guidCmdGroup, uint nCmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
		{
			if (guidCmdGroup == VSConstants.VSStd2K)
			{
				switch (nCmdId)
				{
					case 323u:
						InvokeSnippetBrowser(Resources.InsertSnippet, GetExpansionTypes(nCmdId));
						return true;
					case 1561u:
						InvokeSnippetBrowser(Resources.SurroundWith, GetExpansionTypes(nCmdId));
						return true;
					case 2303u:
						{
							// SqlCompletionSet sqlCompletionSet = (Source as Microsoft.VisualStudio.Data.Tools.SqlLanguageServices.Source).CompletionSet as SqlCompletionSet;
							LsbCompletionSet sqlCompletionSet = Source.CompletionSet as LsbCompletionSet;

							sqlCompletionSet.InPreviewMode = !sqlCompletionSet.InPreviewMode;
							sqlCompletionSet.InPreviewModeOutline = sqlCompletionSet.InPreviewMode;

							if (sqlCompletionSet.IsDisplayed)
								___(TextView.UpdateCompletionStatus(sqlCompletionSet, 1u));

							return true;
						}
				}
			}
			if (guidCmdGroup == VSConstants.GUID_VSStandardCommandSet97 && nCmdId == 1915)
			{
				HandleGotoReference();
				return true;
			}
			if (guidCmdGroup == _ClsidSqlLanguageServiceCommands && nCmdId == 70)
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
			/*
			ISqlEditorServices sqlEditorServices = ServiceProviderUtil.GetService<ISqlEditorServices>(base.TextView);
			if (sqlEditorServices != null && cmd == VSConstants.VSStd97CmdID.GotoDefn)
			{
				sqlEditorServices.HandleGotoDefinition();
			}
			*/
		}
	}


	#endregion Private classes





	// =========================================================================================================
	#region Fields - AbstractLanguageService
	// =========================================================================================================


	private readonly Package _Package;
	private LsbLanguagePreferences _Prefs = null;
	private readonly MetadataDisplayInfoProvider _DisplayInfoProvider;
	private readonly LsbNoOpAuthoringScope _NoOpAuthoringScope;

	private static readonly Guid _ClsidExpressionEvaluator = new(PackageData.MandatedExpressionEvaluatorGuid);


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - AbstractLanguageService
	// =========================================================================================================


	public override string Name => PackageData.LanguageLongName;

	public LsbLanguagePreferences Prefs => _Prefs ??= (LsbLanguagePreferences)Preferences;

	private string[] FileExtensions { get; } = [PackageData.Extension];


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - AbstractLanguageService
	// =========================================================================================================


	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	protected static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);



	public override int GetColorableItem(int index, out IVsColorableItem item)
	{
		IList<IVsColorableItem> colorableItems = LsbConfiguration.ColorableItems;

		if (index > 0 && index <= colorableItems.Count)
		{
			item = colorableItems[index - 1];
			return 0;
		}

		item = LsbConfiguration.TextColorableItem;

		return 1;
	}

	public override int GetItemCount(out int count)
	{
		count = LsbConfiguration.ColorableItems.Count;

		return 0;
	}

	public override Source CreateSource(IVsTextLines buffer)
	{
		return new LsbSource(this, buffer, GetColorizer(buffer));
	}


	public override string GetFormatFilterList()
	{
		IEnumerable<string> normalized = FileExtensions.Select(f => $"*{f}");
		string first = string.Join(", ", normalized);
		string second = string.Join(";", normalized);

		return Resources.FormatFilterList.FmtRes(Name, first, second);
	}



	public override IScanner GetScanner(IVsTextLines buffer)
	{
		return new LsbLineScanner();
	}



	public override int GetLanguageID(IVsTextBuffer buffer, int line, int col, out Guid langId)
	{
		// TraceUtils.Trace(GetType(), "LanguageService.GetLanguageID", "buffer: {0}, line: {1}, col: {2}", buffer, line, col);
		langId = _ClsidExpressionEvaluator;
		return 0;
	}

	public override int GetLocationOfName(string name, out string pbstrMkDoc, TextSpan[] spans)
	{
		// TraceUtils.Trace(GetType(), "LanguageService.GetLocationOfName", "name: {0}, spans: {1}", name, spans);
		return base.GetLocationOfName(name, out pbstrMkDoc, spans);
	}

	public override int GetNameOfLocation(IVsTextBuffer buffer, int line, int col, out string name, out int lineOffset)
	{
		// TraceUtils.Trace(GetType(), "LanguageService.GetNameOfLocation", "buffer: {0}, line: {1}, col: {2}", buffer, line, col);
		return base.GetNameOfLocation(buffer, line, col, out name, out lineOffset);
	}

	public override int GetProximityExpressions(IVsTextBuffer buffer, int line, int col, int cLines, out IVsEnumBSTR ppEnum)
	{
		// TraceUtils.Trace(GetType(), "LanguageService.GetProximityExpressions", "buffer: {0}, line: {1}, col: {2}, cLines: {3}", buffer, line, col, cLines);
		return base.GetProximityExpressions(buffer, line, col, cLines, out ppEnum);
	}

	public override int IsMappedLocation(IVsTextBuffer buffer, int line, int col)
	{
		// TraceUtils.Trace(GetType(), "LanguageService.IsMappedLocation", "buffer: {0}, line: {1}, col: {2}", buffer, line, col);
		return base.IsMappedLocation(buffer, line, col);
	}

	public override int ResolveName(string name, uint flags, out IVsEnumDebugName ppNames)
	{
		// TraceUtils.Trace(GetType(), "LanguageService.ResolveName", "name: {0}, flags: {1}", name, flags);
		return base.ResolveName(name, flags, out ppNames);
	}

	public override int ValidateBreakpointLocation(IVsTextBuffer buffer, int line, int col, TextSpan[] pCodeSpan)
	{
		// TraceUtils.Trace(GetType(), "LanguageService.ValidateBreakpointLocation", "buffer: {0}, line: {1}, col: {2}, pCodeSpan: {3}", buffer, line, col, pCodeSpan);
		IVsTextLines obj = buffer as IVsTextLines;

		___(obj.GetLastLineIndex(out var piLine, out var piIndex));
		___(obj.GetLineText(0, 0, piLine, piIndex, out var pbstrBuf));

		Resolver.BlockInformation blockInformation = Resolver.FindBreakPointInformation(Parser.Parse(pbstrBuf), line + 1, col + 1);

		if (pCodeSpan != null && blockInformation != null)
		{
			pCodeSpan[0].iStartLine = blockInformation.Start.LineNumber - 1;
			pCodeSpan[0].iEndLine = blockInformation.End.LineNumber - 1;
			pCodeSpan[0].iStartIndex = blockInformation.Start.ColumnNumber - 1;
			pCodeSpan[0].iEndIndex = blockInformation.End.ColumnNumber - 1;
			return 0;
		}
		return 1;
	}


	public override LanguagePreferences GetLanguagePreferences()
	{
		// TODO: Choices
		return LanguageExtensionPackage.Instance.GetUserPreferences();

	}


	public override ViewFilter CreateViewFilter(CodeWindowManager mgr, IVsTextView newView)
	{
		return new CustomViewFilter(this, mgr, newView);
	}

	public void RefreshIntellisense(bool currentWindowOnly)
	{
		IVsTextView lastActiveTextView = LastActiveTextView;
		LsbSource source = GetSource(lastActiveTextView) as LsbSource;

		foreach (LsbSource source2 in GetSources())
		{
			source2.IsDirty = true;

			source2.Reset();

			if (!currentWindowOnly && source2 != source)
				source2.GetTaskProvider().Tasks.Clear();
		}
	}

	public override ImageList GetImageList()
	{
		return new ImageList
		{
			Images =
			{
				Resources.AsymmetricKey,
				Resources.Certificate,
				Resources.Column,
				Resources.Credential,
				Resources.Database,
				Resources.Login,
				Resources.ScalarValuedFunction,
				Resources.Schema,
				Resources.StoredProcedure,
				Resources.Table,
				Resources.TableValuedFunction,
				Resources.User,
				Resources.Variable,
				Resources.View
			}
		};
	}

	public override AuthoringScope ParseSource(ParseRequest req)
	{
		LsbSource source = (LsbSource)(req.FileName != null ? GetSource(req.FileName) : null);
		AuthoringScope authoringScope = null;

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

	private bool AbleToParseSource(ParseRequest req, LsbSource source)
	{
		bool result = true;
		bool maxSizeExceeded = req.Text.Length > Prefs.MaxScriptSize;

		// IBMetadataProviderProvider metadataProviderProvider;

		/*
		if (!source.IsDisconnectedMode && !source.IsServerSupported)
		{
			result = false;
			req.Sink.AddError(req.FileName, Resources.IntellisenseAvailableForKatmaiOnly, default, Severity.Hint);
		}
		*/

		// TODO: No ProviderProvider
		/*
		else if (!EditorExtensionPackage.Instance.LanguageService.Preferences.EnableAzureIntellisense &&
			(metadataProviderProvider = source.GetMetadataProviderProvider()) != null
			&& metadataProviderProvider is SmoMetadataProviderProvider provider && provider.IsCloudConnection)
		{
			result = false;
			req.Sink.AddError(req.FileName, Resources.IntellisenseAvailableForKatmaiOnly, default, Severity.Hint);
		}
		else */
		if (!source.IntelliSenseEnabled)
		{
			result = false;
		}
		else if (source.IsSqlCmdModeEnabled)
		{
			result = false;
			req.Sink.AddError(req.FileName, Resources.IntellisenseDisabledForSqlCmdMode, default, Severity.Hint);
		}
		else if (maxSizeExceeded)
		{
			result = false;
			req.Sink.AddError(req.FileName, Resources.FileTooBig, default, Severity.Hint);
		}
		return result;
	}



	private AuthoringScope ProcessParseResult(ParseRequest req, LsbSource source, bool hasChanged, ParseResult parseResult)
	{
		bool isReallyDirty = source.IsReallyDirty;

		if (req.Reason == ParseReason.Check && Prefs.UnderlineErrors)
		{
			string script = null;

			foreach (Error error in source.Errors)
			{
				if (error.Type != ErrorType.BindError)
				{
					script ??= source.GetText();

					if (OverrideError(script, error))
						continue;

					req.Sink.AddError(req.FileName, PackageData.DefaultMessagePrefix + error.Message, GetTextSpan(error.Start, error.End), error.IsWarning ? Severity.Warning : Severity.Error);
				}
			}
		}
		if (req.Sink.HiddenRegions && (hasChanged || source.HasPendingRegions))
		{
			source.HasPendingRegions = true;
			req.Sink.ProcessHiddenRegions = false;

			if (source.OutliningEnabled)
			{
				Region region = null;

				if (source.HiddenRegions != null)
				{
					foreach (Region hiddenRegion in source.HiddenRegions)
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

		_DisplayInfoProvider.BuiltInCasing = Prefs.TextCasing == 0 ? CasingStyle.Uppercase : CasingStyle.Lowercase;



		return new LsbAuthoringScope(parseResult, _DisplayInfoProvider, source);
	}





	private void MatchPair(LsbSource source, AuthoringSink sink, bool isDirty, bool highlight)
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
			TextSpan endContext = !(!highlight && flag2) || !pairMatch.IsStartEnd ? GetTextSpan(endToken.StartLocation, endToken.EndLocation) : GetTextSpan(endToken.EndLocation);
			sink.MatchPair(textSpan, endContext, 1);
		}
	}



	protected abstract bool OverrideError(string script, Error error);



	private void PopulateNamesAndParams(ParseRequest request, ParseResult parseResult)
	{
		MethodNameAndParamLocations methodNameAndParams = Resolver.GetMethodNameAndParams(parseResult, request.Line + 1, request.Col + 1, _DisplayInfoProvider);
		if (methodNameAndParams == null)
			return;
		AuthoringSink sink = request.Sink;

		sink.StartName(GetTextSpan(methodNameAndParams.NameStartLocation, methodNameAndParams.NameEndLocation), methodNameAndParams.Name);


		if (methodNameAndParams.ParamStartLocation != null /* .HasValue */)
		{
			sink.StartParameters(GetTextSpan(methodNameAndParams.ParamStartLocation /* .Value */));
		}
		foreach (Location paramSeperatorLocation in methodNameAndParams.ParamSeperatorLocations)
		{
			sink.NextParameter(GetTextSpan(paramSeperatorLocation));
		}
		if (methodNameAndParams.ParamEndLocation != null /* .HasValue */)
		{
			sink.EndParameters(GetTextSpan(methodNameAndParams.ParamEndLocation /* .Value */));
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

	private bool IsDifferentRegion(Region currentRegion, Region prevRegion)
	{
		if (currentRegion.StartLocation.CompareTo(prevRegion.StartLocation) == 0 && currentRegion.EndLocation.CompareTo(prevRegion.EndLocation) == 0)
		{
			return false;
		}
		return true;
	}



	public override void UpdateLanguageContext(LanguageContextHint hint, IVsTextLines buffer, TextSpan[] ptsSelection, IVsUserContext context)
	{
		context.RemoveAttribute(null, null);
		context.AddAttribute(VSUSERCONTEXTATTRIBUTEUSAGE.VSUC_Usage_Filter, "DevLang", "TSQL");
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
			context.AddAttribute(VSUSERCONTEXTATTRIBUTEUSAGE.VSUC_Usage_LookupF1, "keyword", szValue);
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
		LsbSource source = (LsbSource)GetSource(pTextLines);

		if (source == null || source.ParseResult == null)
		{
			pbstrDescription = null;
			pfBlockAvailable = 0;
			return 0;
		}

		Resolver.BlockInformation blockInformation = Resolver.GetBlockInformation(source.ParseResult, iCurrentLine + 1, iCurrentChar + 1);

		if (ptsBlockSpan != null)
			ptsBlockSpan[0] = GetTextSpan(blockInformation.Start, blockInformation.End);

		pbstrDescription = blockInformation.Description;
		pfBlockAvailable = -1;
		return 0;
	}


	#endregion Methods





	// =========================================================================================================
	#region Event handling - AbstractLanguageService
	// =========================================================================================================


	public override void OnIdle(bool periodic)
	{
		if (LastActiveTextView != null)
		{
			LsbSource source = (LsbSource)GetSource(LastActiveTextView);


			if (source != null)
			{
				if (!source.CompletedFirstParse)
					source.LastParseTime = 0;

				// TODO: No ProviderProvider
				/*
				IBMetadataProviderProvider metadataProviderProvider = source.GetMetadataProviderProvider();
				if (metadataProviderProvider != null)
				{
					AuxilliaryDocData auxDocData = EditorExtensionPackage.Instance.GetAuxilliaryDocData(source.GetTextLines());
					if (auxDocData.QryMgr.IsConnected && metadataProviderProvider is SmoMetadataProviderProvider smoMetadataProviderProvider)
					{
						IDbConnection connection = auxDocData.QryMgr.ConnectionStrategy.Connection;
						if (connection != null && !string.IsNullOrEmpty(connection.Database))
						{
							smoMetadataProviderProvider.AsyncAddDatabaseToDriftDetectionSet(connection.Database);
						}
					}
				}
				*/
			}
		}
		base.OnIdle(periodic);
	}


	#endregion Event handling




}
