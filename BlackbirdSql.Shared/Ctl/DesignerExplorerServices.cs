// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Package.DesignerServices.DatabaseChangesManager

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Shared.Ctl.Commands;
using BlackbirdSql.Shared.Ctl.Config;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Model;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys.Enums;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;



namespace BlackbirdSql.Shared.Ctl;


// =========================================================================================================
//
//										DesignerServices Class
//
/// <summary>
/// Service for handling open query commands.
/// </summary>
// =========================================================================================================
public class DesignerExplorerServices : AbstractDesignerServices, IBDesignerExplorerServices
{

	// ----------------------------------------------------------
	#region Constructors / Destructors - DesignerServices
	// ----------------------------------------------------------


	public DesignerExplorerServices() : base()
	{
	}


	#endregion Constructors / Destructors





	// =====================================================================================================
	#region Methods - DesignerServices
	// =====================================================================================================


	private static void OpenAsMiscellaneousFile(string path, string caption, Guid editor,
		string physicalView, Guid logicalView)
	{
		// Tracer.Trace(typeof(Cmd), "OpenAsMiscellaneousFile()");

		Diag.ThrowIfNotOnUIThread();

		try
		{
			IVsProject3 miscellaneousProject = Cmd.GetMiscellaneousProject();

			VSADDRESULT[] array = new VSADDRESULT[1];
			VSADDITEMOPERATION addItemOperation = VSADDITEMOPERATION.VSADDITEMOP_CLONEFILE;

			uint flags = (uint)__VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_DoOpen;

			flags |= (uint)(!(editor == Guid.Empty)
				? __VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_UseEditor
				: __VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_UseView);

			Native.WrapComCall(miscellaneousProject.AddItemWithSpecific(grfEditorFlags: flags,
				itemidLoc: uint.MaxValue, dwAddItemOperation: addItemOperation, pszItemName: caption, cFilesToOpen: 1u,
				rgpszFilesToOpen: [path], hwndDlgOwner: IntPtr.Zero, rguidEditorType: ref editor,
				pszPhysicalView: physicalView, rguidLogicalView: ref logicalView, pResult: array), []);

			if (array[0] != VSADDRESULT.ADDRESULT_Success)
			{
				throw new ApplicationException(array[0].ToString());
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
	}



	// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
	// Microsoft.VisualStudio.Data.Tools.Package.Explorers.SqlServerObjectExplorer.SqlServerObjectExplorerServiceHelper.OpenOnlineEditor
	// combined with local methods
	private static void OpenExplorerEditor(IVsDataExplorerNode node, EnModelObjectType objectType,
		IList<string> identifierList, EnModelTargetType targetType, Guid editorFactory,
		Action<DbConnectionStringBuilder, bool> documentLoadedCallback, string physicalViewName = null)
	{
		if (editorFactory == Guid.Empty)
			editorFactory = new(SystemData.MandatedSqlEditorFactoryGuid);

		EnModelObjectType elementType = objectType;

		IList<string> identifierArray = null;


		if (identifierList != null)
			identifierArray = new List<string>(identifierList);


		string mkDocument = Moniker.BuildDocumentMoniker(node, ref identifierArray, targetType, false);


		if (RctManager.ShutdownState)
			return;

		Csb csa = RctManager.CloneRegistered(node);

		RaiseBeforeOpenDocument(mkDocument, csa, identifierArray, objectType, targetType, S_BeforeOpenDocumentHandler);



		bool editorAlreadyOpened = !OpenMiscellaneousSqlFile(mkDocument, node, targetType, csa);

		ExecuteDocumentLoadedCallback(documentLoadedCallback, csa, editorAlreadyOpened);

		return;


	}



	private static bool OpenMiscellaneousSqlFile(string explorerMoniker, IVsDataExplorerNode node, EnModelTargetType targetType, Csb csa)
	{
		// Tracer.Trace(typeof(DesignerExplorerServices), "OpenMiscellaneousSqlFile()", "ExplorerMoniker: {0}.", explorerMoniker);

		Diag.ThrowIfNotOnUIThread();

		uint documentCookie = 0;

		if (RdtManager.InflightMonikerCsbTable.ContainsKey(explorerMoniker))
		{
			foreach (KeyValuePair<object, AuxilliaryDocData> pair in ((IBEditorPackage)ApcManager.PackageInstance).AuxilliaryDocDataTable)
			{
				if (pair.Value.ExplorerMoniker == null)
					continue;

				if (explorerMoniker.Equals(pair.Value.ExplorerMoniker))
				{
					documentCookie = pair.Value.DocCookie;
					break;
				}
			}

			if (documentCookie == 0)
				RdtManager.InflightMonikerCsbTable.Remove(explorerMoniker);
		}

		if (documentCookie != 0)
		{
			string documentMoniker = RdtManager.GetDocumentMoniker(documentCookie);


			IVsUIShellOpenDocument vsUIShellOpenDocument = ApcManager.EnsureService<SVsUIShellOpenDocument, IVsUIShellOpenDocument>();
			Guid rguidEditorType = new(SystemData.MandatedSqlEditorFactoryGuid); 
			uint[] array = new uint[1];
			IVsUIHierarchy pHierCaller = null;

			int hresult = vsUIShellOpenDocument.IsSpecificDocumentViewOpen(pHierCaller, uint.MaxValue,
				documentMoniker, ref rguidEditorType, null, (uint)__VSIDOFLAGS.IDO_ActivateIfOpen,
				out IVsUIHierarchy ppHierOpen, out array[0], out IVsWindowFrame ppWindowFrame, out int pfOpen);

			_ = ppHierOpen; // Suppress

			bool editorAlreadyOpened = __(hresult) && pfOpen.AsBool();

			if (editorAlreadyOpened && ppWindowFrame != null)
			{
				ppWindowFrame.Show();
				return false;
			}
		}


		string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		Directory.CreateDirectory(tempDirectory);

		// Tracer.Trace(typeof(DesignerExplorerServices), "OpenMiscellaneousSqlFile()", "Created directory: {0} for explorerMoniker: {1}.", tempDirectory, explorerMoniker);

		string filename = Path.GetFileNameWithoutExtension(explorerMoniker);

		string tempFilename = tempDirectory + "\\" + filename + NativeDb.Extension;


		if (tempFilename == null)
		{
			MessageCtl.ShowEx(string.Empty, ControlsResources.ErrCannotCreateTempFile, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return false;
		}

		// Tracer.Trace(typeof(DesignerExplorerServices), "OpenMiscellaneousSqlFile()", "Writing to temp file: {0}.", tempFilename);

		StreamWriter streamWriter = null;
		try
		{
			string script = node != null
				? Moniker.GetDecoratedDdlSource(node, targetType)
				: Resources.DesignerExplorerServices_DecoratedNewQuery + "\r\n";

			streamWriter = new StreamWriter(tempFilename);
			streamWriter.Write(script);
			streamWriter.Flush();
			streamWriter.Close();
			streamWriter = null;

			RdtManager.InflightMonikerStack = explorerMoniker;
			RdtManager.InflightMonikerCsbTable.Add(explorerMoniker, csa);

			OpenAsMiscellaneousFile(tempFilename, filename + NativeDb.Extension, new Guid(SystemData.EditorFactoryGuid),
				string.Empty, VSConstants.LOGVIEWID_Primary);
		}
		catch
		{
			_ = RdtManager.InflightMonikerStack;
			RdtManager.InflightMonikerCsbTable.Remove(explorerMoniker);
		}
		finally
		{
			streamWriter?.Close();
			File.Delete(tempFilename);
			Directory.Delete(tempDirectory);
		}

		foreach (KeyValuePair<object, AuxilliaryDocData> pair in ((IBEditorPackage)ApcManager.PackageInstance).AuxilliaryDocDataTable)
		{
			if (explorerMoniker.Equals(pair.Value.ExplorerMoniker) && pair.Value.DocCookie == 0)
			{
				pair.Value.DocCookie = RdtManager.GetRdtCookie(pair.Value.OriginalDocumentMoniker);
				break;
			}
		}

		return true;
	}



	public static void OpenNewMiscellaneousSqlFile(string initialContent = "")
	{
		// Tracer.Trace(typeof(Cmd), "OpenAsMiscellaneousFile()");

		Diag.ThrowIfNotOnUIThread();

		IVsProject3 miscellaneousProject = Cmd.GetMiscellaneousProject();

		miscellaneousProject.GenerateUniqueItemName(VSConstants.VSITEMID_ROOT, NativeDb.Extension, "NewQuery", out string pbstrItemName);
		string tempFileName = Path.GetTempFileName();
		if (tempFileName == null)
		{
			MessageCtl.ShowEx(string.Empty, ControlsResources.ErrCannotCreateTempFile, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return;
		}
		StreamWriter streamWriter = null;
		try
		{
			streamWriter = new StreamWriter(tempFileName);
			streamWriter.Write(initialContent);
			streamWriter.Flush();
			streamWriter.Close();
			streamWriter = null;
			OpenAsMiscellaneousFile(tempFileName, pbstrItemName, new Guid(SystemData.EditorFactoryGuid),
				string.Empty, VSConstants.LOGVIEWID_Primary);
		}
		finally
		{
			streamWriter?.Close();
			File.Delete(tempFileName);
		}
	}



	private static void OpenNewQueryEditor(string datasetKey, Guid editorFactory, Action<DbConnectionStringBuilder> documentLoadedCallback,
		string physicalViewName = null)
	{
		if (editorFactory == Guid.Empty)
			editorFactory = new(SystemData.MandatedSqlEditorFactoryGuid);

		EnModelTargetType targetType = EnModelTargetType.QueryScript;
		EnModelObjectType objectType = EnModelObjectType.NewSqlQuery;
		EnModelObjectType elementType = objectType;

		if (RctManager.ShutdownState)
			return;

		Csb csa = RctManager.CloneRegistered(datasetKey);

		IList<string> identifierList = ["NewQuery"];
		IList<string> identifierArray = null;


		if (identifierList != null)
			identifierArray = new List<string>(identifierList);


		// Tracer.Trace(typeof(DesignerExplorerServices), "OpenNewQueryEditor()", "csa.DataSource: {0}, csa.Database: {1}", csa.DataSource, csa.Database);

		string mkDocument = Moniker.BuildDocumentMoniker(csa.DataSource, csa.Database, elementType, ref identifierArray, targetType, true);

		RaiseBeforeOpenDocument(mkDocument, csa, identifierArray, objectType, targetType, S_BeforeOpenDocumentHandler);


		bool editorAlreadyOpened = !OpenMiscellaneousSqlFile(mkDocument, null, targetType, csa);

		ExecuteDocumentLoadedCallback(documentLoadedCallback, csa, editorAlreadyOpened);

		return;

	}



	public void NewSqlQuery(string datasetKey)
	{
		// Sanity check.
		// Currently our only entry point to AbstractDesignerServices whose warnings are suppressed.
		Diag.ThrowIfNotOnUIThread();

		Guid clsidEditorFactory = new Guid(SystemData.EditorFactoryGuid);

		OpenNewQueryEditor(datasetKey, clsidEditorFactory, null);
	}



	// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
	// Microsoft.VisualStudio.Data.Tools.Package.Explorers.SqlServerObjectExplorer.SqlServerObjectExplorerService:ViewCode()
	public void ViewCode(IVsDataExplorerNode node, EnModelTargetType targetType)
	{
		// Sanity check.
		// Currently our only entry point to AbstractDesignerServices whose warnings are suppressed.
		Diag.ThrowIfNotOnUIThread();

		// Tracer.Trace(GetType(), "ViewCode()");

		Moniker moniker = null;
		IList<string> identifierList = null;
		EnModelObjectType objectType = EnModelObjectType.Unknown;
		Action<DbConnectionStringBuilder, bool> callback = null;

		Guid clsidEditorFactory = new Guid(SystemData.EditorFactoryGuid);

		try
		{
			moniker = new(node, targetType);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

		// Tracer.Trace(GetType(), "ViewCode()", "\nDocumentMoniker: {0}.", moniker.DocumentMoniker);


		try
		{
			identifierList = [.. moniker.Identifier];
			objectType = moniker.ObjectType;

			if ((objectType == EnModelObjectType.Table || objectType == EnModelObjectType.View)
				&& targetType == EnModelTargetType.QueryScript && PersistentSettings.EditorExecuteQueryOnOpen)
			{
				callback = OnSqlQueryLoaded;
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

		try
		{
			OpenExplorerEditor(node, objectType, identifierList, targetType, clsidEditorFactory, callback, null);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}
	}


	#endregion Methods





	// =====================================================================================================
	#region Event Handling - DesignerServices
	// =====================================================================================================


	public void OnSqlQueryLoaded(DbConnectionStringBuilder csb, bool alreadyLoaded)
	{
		if (alreadyLoaded)
			return;

		IBSqlEditorWindowPane lastFocusedSqlEditor = ((IBEditorPackage)ApcManager.PackageInstance).LastFocusedSqlEditor;

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


	#endregion Event Handling

}
