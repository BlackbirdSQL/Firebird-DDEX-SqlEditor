// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Package.DesignerServices.DatabaseChangesManager

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.Common.Controls.Interfaces;
using BlackbirdSql.Common.Ctl.Commands;
using BlackbirdSql.Common.Ctl.Config;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Controls;
using BlackbirdSql.Core.Ctl.Diagnostics;
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

		CsbAgent csa = RctManager.CloneRegistered(node);

		RaiseBeforeOpenDocument(mkDocument, csa, identifierArray, objectType, targetType, S_BeforeOpenDocumentHandler);



		bool editorAlreadyOpened = !OpenMiscellaneousSqlFile(mkDocument, node, targetType, csa);

		ExecuteDocumentLoadedCallback(documentLoadedCallback, csa, editorAlreadyOpened);

		return;


	}


	public static void OpenAsMiscellaneousFile(string path, string caption,
	Guid editor, string physicalView, Guid logicalView)
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


	public static bool OpenMiscellaneousSqlFile(string explorerMoniker, IVsDataExplorerNode node, EnModelTargetType targetType, CsbAgent csa)
	{
		// Tracer.Trace(typeof(DesignerExplorerServices), "OpenMiscellaneousSqlFile()", "ExplorerMoniker: {0}.", explorerMoniker);

		Diag.ThrowIfNotOnUIThread();

		uint documentCookie = 0;

		if (RdtManager.MonikerCsaTable.ContainsKey(explorerMoniker))
		{
			foreach (KeyValuePair<object, AuxiliaryDocData> pair in ((IBEditorPackage)ApcManager.DdexPackage).AuxiliaryDocDataTable)
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
				RdtManager.MonikerCsaTable.Remove(explorerMoniker);
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

			bool editorAlreadyOpened = ErrorHandler.Succeeded(hresult) && pfOpen.AsBool();

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

		string tempFilename = tempDirectory + "\\" + filename + SystemData.Extension;


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

			RdtManager.ExplorerMonikerStack = explorerMoniker;
			RdtManager.MonikerCsaTable.Add(explorerMoniker, csa);

			OpenAsMiscellaneousFile(tempFilename, filename + SystemData.Extension, new Guid(SystemData.DslEditorFactoryGuid),
				string.Empty, VSConstants.LOGVIEWID_Primary);
		}
		catch
		{
			_ = RdtManager.ExplorerMonikerStack;
			RdtManager.MonikerCsaTable.Remove(explorerMoniker);
		}
		finally
		{
			streamWriter?.Close();
			File.Delete(tempFilename);
			Directory.Delete(tempDirectory);
		}

		foreach (KeyValuePair<object, AuxiliaryDocData> pair in ((IBEditorPackage)ApcManager.DdexPackage).AuxiliaryDocDataTable)
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

		miscellaneousProject.GenerateUniqueItemName(VSConstants.VSITEMID_ROOT, SystemData.Extension, "NewQuery", out string pbstrItemName);
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
			OpenAsMiscellaneousFile(tempFileName, pbstrItemName, new Guid(SystemData.DslEditorFactoryGuid),
				string.Empty, VSConstants.LOGVIEWID_Primary);
		}
		finally
		{
			streamWriter?.Close();
			File.Delete(tempFileName);
		}
	}


	public static void OpenNewQueryEditor(string datasetKey, Guid editorFactory, Action<DbConnectionStringBuilder> documentLoadedCallback,
		string physicalViewName = null)
	{
		if (editorFactory == Guid.Empty)
			editorFactory = new(SystemData.MandatedSqlEditorFactoryGuid);

		EnModelTargetType targetType = EnModelTargetType.QueryScript;
		EnModelObjectType objectType = EnModelObjectType.NewSqlQuery;
		EnModelObjectType elementType = objectType;

		if (RctManager.ShutdownState)
			return;

		CsbAgent csa = RctManager.CloneRegistered(datasetKey);

		IList<string> identifierList = new List<string>() { "NewQuery" };
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

		Guid clsidEditorFactory = new Guid(SystemData.DslEditorFactoryGuid);

		OpenNewQueryEditor(datasetKey, clsidEditorFactory, null);
	}



	public void OnSqlQueryLoaded(DbConnectionStringBuilder csb, bool alreadyLoaded)
	{
		if (alreadyLoaded)
			return;

		IBSqlEditorWindowPane lastFocusedSqlEditor = ((IBEditorPackage)ApcManager.DdexPackage).LastFocusedSqlEditor;

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

		// Tracer.Trace(GetType(), "ViewCode()");

		Moniker moniker = null;
		IList<string> identifierList = null;
		EnModelObjectType objectType = EnModelObjectType.Unknown;
		Action<DbConnectionStringBuilder, bool> callback = null;

		Guid clsidEditorFactory = new Guid(SystemData.DslEditorFactoryGuid);

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
			identifierList = moniker.Identifier.ToArray();
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


}
