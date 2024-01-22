// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Package.DesignerServices.DatabaseChangesManager


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using BlackbirdSql.Common.Ctl.Commands;
using BlackbirdSql.Common.Ctl.Config;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Model.Enums;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;


namespace BlackbirdSql.Common.Ctl;

public class DesignerExplorerServices : AbstractDesignerServices, IBDesignerExplorerServices
{

	public DesignerExplorerServices() : base()
	{
	}



	// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
	// Microsoft.VisualStudio.Data.Tools.Package.Explorers.SqlServerObjectExplorer.SqlServerObjectExplorerServiceHelper.OpenOnlineEditor
	// combined with local methods
	public static void OpenExplorerEditor(IVsDataExplorerNode node, EnModelObjectType objectType,
		IList<string> identifierList, EnModelTargetType targetType, Guid editorFactory,
		Action<DatabaseLocation, bool> documentLoadedCallback, string physicalViewName = null)
	{
		bool result = false;

		if (editorFactory == Guid.Empty)
			editorFactory = new(SystemData.MandatedSqlEditorFactoryGuid);

		string mkDocument = null;
		EnModelObjectType elementType = objectType;
		DatabaseLocation dbl = new(node, targetType);
		HashSet<NodeElementDescriptor> originalObjects = null;
		bool flag = false;
		IList<string> identifierArray = null;


		if (identifierList != null)
		{
			identifierArray = new List<string>(identifierList);
			mkDocument = LookupObjectMoniker(dbl, elementType, identifierArray);
			if (objectType != EnModelObjectType.Unknown)
			{
				originalObjects = 
				[
					new (objectType, identifierArray)
				];
			}
		}
		if (string.IsNullOrEmpty(mkDocument))
		{
			mkDocument = MonikerAgent.BuildMiscDocumentMonikerPath(node, ref identifierArray, targetType, false);


			flag = true;
			AddInflightOpen(dbl, new NodeElementDescriptor(elementType, identifierArray), mkDocument);
		}

		RaiseBeforeOpenDocument(mkDocument, dbl, identifierArray, objectType, targetType, S_BeforeOpenDocumentHandler);

		if (RctManager.ShutdownState)
			return;

		CsbAgent csa = RctManager.CloneRegistered(node);

		// Tracer.Trace(typeof(DesignerExplorerServices), "OpenExplorerEditor()", "csa ConnectionString: {0}.", csa.ConnectionString);

		OpenMiscDocument(mkDocument, csa, true, false, editorFactory, out uint docCookie, out IVsWindowFrame frame,
			out bool editorAlreadyOpened, out bool documentAlreadyLoaded, physicalViewName);

		
		_ = frame; // Suppression
		result = true;
		bool flag2 = false;

		if (editorAlreadyOpened)
		{
			ExecuteDocumentLoadedCallback(documentLoadedCallback, dbl, true);
		}
		else if (docCookie == 0)
		{
			ApplicationException ex = new($"Failed to open or create the document: {mkDocument}.");
			Diag.Dug(ex);
			SqlTracer.TraceException(EnSqlTraceId.CoreServices, ex);
		}
		else if (documentAlreadyLoaded)
		{
			ExecuteDocumentLoadedCallback(documentLoadedCallback, dbl, true);
		}
		else
		{
			flag2 = true;
			SuppressChangeTracking(mkDocument, suppress: true);
			SetTextIntoTextBuffer(docCookie, string.Format(CultureInfo.CurrentCulture, "/*\n\r{0}\n\r*/",
				ControlsResources.PowerBuffer_RetrievingDefinitionFromServer));

			try
			{
				string script = MonikerAgent.GetDecoratedDdlSource(node, targetType);

				PopulateEditorWithObject(false, mkDocument, docCookie, script, originalObjects);
				ExecuteDocumentLoadedCallback(documentLoadedCallback, dbl, false);
				RemoveInflightOpen(dbl, new NodeElementDescriptor(elementType, identifierArray));
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				SqlTracer.TraceException(TraceEventType.Critical, EnSqlTraceId.CoreServices, new InvalidDataException("Could not get script"), "DesignerExplorerServices:OpenExplorerEditor");
				string text = ex.ToString();
				SetTextIntoTextBuffer(docCookie, text);
			}
		}

		if (flag && !flag2)
		{
			RemoveInflightOpen(dbl, new NodeElementDescriptor(elementType, identifierArray));
		}

		if (!result)
		{

			InvalidOperationException ex = new(ControlsResources.OpenOnlineEditorException);
			Diag.Dug(ex);
			throw ex;
		}

	}



	public static void OpenNewQueryEditor(string datasetKey, Guid editorFactory, Action<DatabaseLocation> documentLoadedCallback,
		string physicalViewName = null)
	{
		bool result = false;

		if (editorFactory == Guid.Empty)
			editorFactory = new(SystemData.MandatedSqlEditorFactoryGuid);

		EnModelTargetType targetType = EnModelTargetType.QueryScript;
		string mkDocument = null;
		EnModelObjectType objectType = EnModelObjectType.NewSqlQuery;
		EnModelObjectType elementType = objectType;

		if (RctManager.ShutdownState)
			return;

		CsbAgent csa = RctManager.CloneRegistered(datasetKey);

		DatabaseLocation dbl = new(csa, targetType);
		HashSet<NodeElementDescriptor> originalObjects = null;
		bool flag = false;

		IList<string> identifierList = new List<string>() { "NewQuery" };
		IList<string> identifierArray = null;


		if (identifierList != null)
		{
			identifierArray = new List<string>(identifierList);
			mkDocument = LookupObjectMoniker(dbl, elementType, identifierArray);
			if (objectType != EnModelObjectType.Unknown)
			{
				originalObjects =
				[
					new (objectType, identifierArray)
				];
			}
		}
		if (string.IsNullOrEmpty(mkDocument))
		{
			mkDocument = MonikerAgent.BuildMiscDocumentMonikerPath(csa.DataSource, csa.Database, elementType, ref identifierArray, targetType, true);


			flag = true;
			AddInflightOpen(dbl, new NodeElementDescriptor(elementType, identifierArray), mkDocument);
		}

		RaiseBeforeOpenDocument(mkDocument, dbl, identifierArray, objectType, targetType, S_BeforeOpenDocumentHandler);

		OpenMiscDocument(mkDocument, csa, true, false, editorFactory, out uint docCookie, out IVsWindowFrame frame,
			out bool editorAlreadyOpened, out bool documentAlreadyLoaded, physicalViewName);


		_ = frame; // Suppression
		result = true;
		bool flag2 = false;

		if (editorAlreadyOpened)
		{
			ExecuteDocumentLoadedCallback(documentLoadedCallback, dbl);
		}
		else if (docCookie == 0)
		{
			ApplicationException ex = new($"Failed to open or create the document: {mkDocument}.");
			Diag.Dug(ex);
			SqlTracer.TraceException(EnSqlTraceId.CoreServices, ex);
		}
		else if (documentAlreadyLoaded)
		{
			ExecuteDocumentLoadedCallback(documentLoadedCallback, dbl);
		}
		else
		{
			flag2 = true;
			SuppressChangeTracking(mkDocument, suppress: true);
			SetTextIntoTextBuffer(docCookie, string.Format(CultureInfo.CurrentCulture, "/*\n\r{0}\n\r*/",
				ControlsResources.PowerBuffer_RetrievingDefinitionFromServer));

			try
			{
				string script = Resources.DesignerExplorerServices_DecoratedNewQuery + "\r\n";

				PopulateEditorWithObject(false, mkDocument, docCookie, script, originalObjects);
				ExecuteDocumentLoadedCallback(documentLoadedCallback, dbl);
				RemoveInflightOpen(dbl, new NodeElementDescriptor(elementType, identifierArray));
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				SqlTracer.TraceException(TraceEventType.Critical, EnSqlTraceId.CoreServices, new InvalidDataException("Could not get script"), "DesignerExplorerServices:OpenNewQueryEditor");
				string text = ex.ToString();
				SetTextIntoTextBuffer(docCookie, text);
			}
		}

		if (flag && !flag2)
		{
			RemoveInflightOpen(dbl, new NodeElementDescriptor(elementType, identifierArray));
		}

		if (!result)
		{

			InvalidOperationException ex = new(ControlsResources.OpenOnlineEditorException);
			Diag.Dug(ex);
			throw ex;
		}

	}


	public void NewSqlQuery(string datasetKey)
	{
		// Sanity check.
		// Currently our only entry point to AbstractDesignerServices whose warnings are suppressed.
		Diag.ThrowIfNotOnUIThread();

		Guid clsidEditorFactory = new Guid(SystemData.DslEditorFactoryGuid);

		OpenNewQueryEditor(datasetKey, clsidEditorFactory, null);
	}



	public void OnSqlQueryLoaded(DatabaseLocation dbl, bool alreadyLoaded)
	{
		if (alreadyLoaded)
			return;

		IBSqlEditorWindowPane lastFocusedSqlEditor = ((IBEditorPackage)Controller.DdexPackage).LastFocusedSqlEditor;

		// Tracer.Trace(GetType(), "OnSqlQueryLoaded()", "lastFocusedSqlEditor != null: {0}.", lastFocusedSqlEditor != null);

		if (lastFocusedSqlEditor != null)
		{
			SqlEditorExecuteQueryCommand command = new (lastFocusedSqlEditor);

			_ = Task.Run(() => OnSqlQueryLoadedAsync(command, 50));
		}
	}

	private async Task<bool> OnSqlQueryLoadedAsync(AbstractSqlEditorCommand command, int delay)
	{
		// Tracer.Trace(GetType(), "OnSqlQueryLoadedAsync()");

		// Give editor time to breath.
		if (delay > 0)
			Thread.Sleep(delay);

		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		command.Exec((uint)OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, IntPtr.Zero, IntPtr.Zero);

		return true;
	}




	// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
	// Microsoft.VisualStudio.Data.Tools.Package.Explorers.SqlServerObjectExplorer.SqlServerObjectExplorerService:ViewCode()
	public void ViewCode(IVsDataExplorerNode node, EnModelTargetType targetType)
	{
		// Sanity check.
		// Currently our only entry point to AbstractDesignerServices whose warnings are suppressed.
		Diag.ThrowIfNotOnUIThread();

		MonikerAgent moniker = new(node, targetType);

		IList<string> identifierList = moniker.Identifier.ToArray();
		EnModelObjectType objectType = moniker.ObjectType;
		Guid clsidEditorFactory = new Guid(SystemData.DslEditorFactoryGuid);

		Action<DatabaseLocation, bool> callback = null;

		if ((objectType == EnModelObjectType.Table || objectType == EnModelObjectType.View)
			&& targetType == EnModelTargetType.QueryScript && PersistentSettings.EditorExecuteQueryOnOpen)
		{
			// Tracer.Trace(GetType(), "ViewCode()", "assigning OnSqlQueryLoaded.");
			callback = OnSqlQueryLoaded;
		}


		OpenExplorerEditor(node, objectType, identifierList, targetType, clsidEditorFactory, callback, null);
	}


}
