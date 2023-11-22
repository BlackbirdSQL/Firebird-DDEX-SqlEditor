// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Package.DesignerServices.DatabaseChangesManager


using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Extensions;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using MonikerAgent = BlackbirdSql.Common.Model.MonikerAgent;

namespace BlackbirdSql.Common.Ctl;

public class DesignerExplorerServices : AbstractDesignerServices, IBDesignerExplorerServices
{

	public DesignerExplorerServices() : base()
	{
	}



	// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
	// Microsoft.VisualStudio.Data.Tools.Package.Explorers.SqlServerObjectExplorer.SqlServerObjectExplorerServiceHelper.OpenOnlineEditor
	// combined with local methods
	public static void OpenExplorerEditor(IVsDataExplorerNode node, EnModelObjectType objectType, bool alternate,
		IList<string> identifierList, Guid editorFactory, Action<IServiceProvider> documentLoadedCallback,
		string physicalViewName = null)
	{
		bool result = false;

		if (editorFactory == Guid.Empty)
			editorFactory = new(SystemData.MandatedSqlEditorFactoryGuid);

		string mkDocument = null;
		objectType += (alternate ? 20 : 0);
		EnModelObjectType elementType = objectType;
		DatabaseLocation dbl = new(node, alternate);
		HashSet<NodeElementDescriptor> originalObjects = null;
		bool flag = false;
		IList<string> identifierArray = null;


		if (identifierList != null)
		{
			identifierArray = new List<string>(identifierList);
			mkDocument = LookupObjectMoniker(dbl, elementType, identifierArray);
			if (objectType != EnModelObjectType.Unknown)
			{
				originalObjects = new HashSet<NodeElementDescriptor>
				{
					new NodeElementDescriptor(objectType, identifierArray)
				};
			}
		}
		if (string.IsNullOrEmpty(mkDocument))
		{
			mkDocument = MonikerAgent.BuildMiscDocumentMoniker(node, ref identifierArray, false, alternate);


			flag = true;
			AddInflightOpen(dbl, new NodeElementDescriptor(elementType, identifierArray), mkDocument);
		}

		RaiseBeforeOpenDocument(mkDocument, dbl, identifierArray, objectType, S_BeforeOpenDocumentHandler);

		CsbAgent csa = new(node);

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
				string script = MonikerAgent.GetDecoratedDdlSource(node, alternate);

				PopulateEditorWithObject(false, mkDocument, docCookie, script, originalObjects);
				ExecuteDocumentLoadedCallback(documentLoadedCallback, dbl);
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



	public static void OpenNewQueryEditor(string datasetKey, Guid editorFactory, Action<IServiceProvider> documentLoadedCallback,
		string physicalViewName = null)
	{
		bool result = false;

		if (editorFactory == Guid.Empty)
			editorFactory = new(SystemData.MandatedSqlEditorFactoryGuid);

		bool alternate = false;
		string mkDocument = null;
		EnModelObjectType objectType = EnModelObjectType.NewQuery;
		EnModelObjectType elementType = objectType;
		CsbAgent csa = new(CsbAgent.GetDatasetConnectionString(datasetKey));
		DatabaseLocation dbl = new(csa, alternate);
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
				originalObjects = new HashSet<NodeElementDescriptor>
				{
					new NodeElementDescriptor(objectType, identifierArray)
				};
			}
		}
		if (string.IsNullOrEmpty(mkDocument))
		{
			mkDocument = MonikerAgent.BuildMiscDocumentMoniker(csa.DataSource, csa.Database, elementType, ref identifierArray, true, alternate);


			flag = true;
			AddInflightOpen(dbl, new NodeElementDescriptor(elementType, identifierArray), mkDocument);
		}

		RaiseBeforeOpenDocument(mkDocument, dbl, identifierArray, objectType, S_BeforeOpenDocumentHandler);

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


	public void NewQuery(string datasetKey)
	{
		// Sanity check.
		// Currently our only entry point to AbstractDesignerServices whose warnings are suppressed.
		if (!ThreadHelper.CheckAccess())
		{
			COMException ex = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(ex);
			throw ex;
		}

		Guid clsidEditorFactory = new Guid(SystemData.DslEditorFactoryGuid);

		OpenNewQueryEditor(datasetKey, clsidEditorFactory, null);
	}




	// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
	// Microsoft.VisualStudio.Data.Tools.Package.Explorers.SqlServerObjectExplorer.SqlServerObjectExplorerService:ViewCode()
	public void ViewCode(IVsDataExplorerNode node, bool alternate)
	{
		// Sanity check.
		// Currently our only entry point to AbstractDesignerServices whose warnings are suppressed.
		if (!ThreadHelper.CheckAccess())
		{
			COMException ex = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(ex);
			throw ex;
		}

		MonikerAgent moniker = new(node);

		IList<string> identifierList = moniker.Identifier.ToArray();
		EnModelObjectType objectType = moniker.ObjectType;
		Guid clsidEditorFactory = new Guid(SystemData.DslEditorFactoryGuid);

		OpenExplorerEditor(node, objectType, alternate, identifierList, clsidEditorFactory, null, null);
	}


}
