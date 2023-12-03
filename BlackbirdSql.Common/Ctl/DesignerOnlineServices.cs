// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Package.DesignerServices.DatabaseChangesManager


using System;

using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Model.Enums;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;


namespace BlackbirdSql.Common.Ctl;

/// <summary>
/// Do not use. To be completed.
/// </summary>
public class DesignerOnlineServices : AbstractDesignerServices, IBDesignerOnlineServices
{



	public DesignerOnlineServices() : base()
	{
	}



	public static void EnsureConnectionSpecifiesDatabase(DbConnectionStringBuilder csb)
	{
		if (csb is not CsbAgent csa || string.IsNullOrWhiteSpace(csa.Database))
		{
			ArgumentNullException ex = new("csb CsbAgent:Database");
			Diag.Dug(ex);
			throw ex;
		}
	}



	// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
	// Microsoft.VisualStudio.Data.Tools.Package.Explorers.SqlServerObjectExplorer.SqlServerObjectExplorerServiceHelper.OpenOnlineEditor
	// combined with local methods
	public static void OpenOnlineEditor(DbConnectionStringBuilder csb, EnModelObjectType objectType,
		IList<string> identifierList, EnModelTargetType targetType, string script, Guid editorFactory,
		Action<IServiceProvider> documentLoadedCallback, string physicalViewName = null)
	{
		bool result = false;

		if (editorFactory == Guid.Empty)
			editorFactory = new(SystemData.MandatedSqlEditorFactoryGuid);

		string mkDocument = null;
		EnModelObjectType elementType = objectType;
		DatabaseLocation dbl = new(csb, targetType);
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
					new NodeElementDescriptor(objectType, identifierArray)
				];
			}
		}
		if (string.IsNullOrEmpty(mkDocument))
		{
			mkDocument = MonikerAgent.BuildMiscDocumentMonikerPath(dbl.DataSource, dbl.Database,
				elementType, ref identifierArray, targetType, false);


			flag = true;
			AddInflightOpen(dbl, new NodeElementDescriptor(elementType, identifierArray), mkDocument);
		}

		RaiseBeforeOpenDocument(mkDocument, dbl, identifierArray, objectType, targetType, S_BeforeOpenDocumentHandler);


		
		OpenMiscDocument(mkDocument, csb, true, true, editorFactory, out uint docCookie, out IVsWindowFrame frame,
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
			ApplicationException ex = new("Failed to open or create the document: " + mkDocument);
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
				PopulateEditorWithObject(false, mkDocument, docCookie, script, originalObjects);
				ExecuteDocumentLoadedCallback(documentLoadedCallback, dbl);
				RemoveInflightOpen(dbl, new NodeElementDescriptor(elementType, identifierArray));
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				SqlTracer.TraceException(TraceEventType.Critical, EnSqlTraceId.CoreServices, new InvalidDataException("Could not get script"), "DesignerExplorerServices:OpenOnlineEditorImpl");
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



	// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
	// Microsoft.VisualStudio.Data.Tools.Package.Explorers.SqlServerObjectExplorer.SqlServerObjectExplorerService:ViewCode()
	public void ViewCode(DbConnectionStringBuilder csb, EnModelObjectType objectType,
		IList<string> identifierList, EnModelTargetType targetType, string script)
	{
		// Sanity check.
		// Currently our only entry point to AbstractDesignerServices whose warnings are suppressed.
		if (!ThreadHelper.CheckAccess())
		{
			COMException ex = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(ex);
			throw ex;
		}

		EnsureConnectionSpecifiesDatabase(csb);

		Guid clsidEditorFactory = new Guid(SystemData.MandatedSqlEditorFactoryGuid);

		OpenOnlineEditor(csb, objectType, identifierList, targetType, script, clsidEditorFactory, null, null);
	}


}
