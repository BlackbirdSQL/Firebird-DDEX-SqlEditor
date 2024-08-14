// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Package.DesignerServices.DatabaseChangesManager
// Split into two for brevity.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Shared.Ctl.Commands;
using BlackbirdSql.Shared.Ctl.Config;
using BlackbirdSql.Shared.Enums;
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
public class DesignerExplorerServices : AbstractDesignerServices, IBsDesignerExplorerServices
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
			IVsProject3 miscellaneousProject = UnsafeCmd.MiscellaneousProjectHierarchy;

			VSADDRESULT[] array = new VSADDRESULT[1];
			VSADDITEMOPERATION addItemOperation = VSADDITEMOPERATION.VSADDITEMOP_CLONEFILE;

			uint flags = (uint)__VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_DoOpen;

			flags |= (uint)(!(editor == Guid.Empty)
				? __VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_UseEditor
				: __VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_UseView);

			___(miscellaneousProject.AddItemWithSpecific(grfEditorFlags: flags,
				itemidLoc: uint.MaxValue, dwAddItemOperation: addItemOperation, pszItemName: caption, cFilesToOpen: 1u,
				rgpszFilesToOpen: [path], hwndDlgOwner: IntPtr.Zero, rguidEditorType: ref editor,
				pszPhysicalView: physicalView, rguidLogicalView: ref logicalView, pResult: array));

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
		bool autoExecute, string physicalViewName = null)
	{
		if (editorFactory == Guid.Empty)
			editorFactory = new(SystemData.C_MandatedSqlEditorFactoryGuid);

		EnModelObjectType elementType = objectType;

		IList<string> identifierArray = null;


		if (identifierList != null)
			identifierArray = new List<string>(identifierList);


		string mkDocument = Moniker.BuildDocumentMoniker(node, ref identifierArray, targetType);


		if (RctManager.ShutdownState)
			return;

		Csb csb = RctManager.CloneRegistered(node);
		ModelCsb csa = null;

		if (csb != null)
		{
			EnCreationFlags creationFlags = autoExecute ? EnCreationFlags.CreateAndExecute : EnCreationFlags.CreateConnection;

			csa = new(csb)
			{
				CreationFlags = creationFlags
			};
		}

		RaiseBeforeOpenDocument(mkDocument, csa, identifierArray, objectType, targetType, S_BeforeOpenDocumentHandler);

		OpenMiscellaneousVirtualFile(mkDocument, node, targetType, csa, null);

		// bool editorAlreadyOpened = !OpenMiscellaneousSqlFile(mkDocument, node, targetType, csa);

		// ExecuteDocumentLoadedCallback(documentLoadedCallback, csa, editorAlreadyOpened);

		return;


	}



	private static bool OpenMiscellaneousVirtualFile(string moniker, IVsDataExplorerNode node,
		EnModelTargetType targetType, Csb csa, string initialScript)
	{
		// Tracer.Trace(typeof(DesignerExplorerServices), "OpenMiscellaneousSqlFile()", "ExplorerMoniker: {0}.", explorerMoniker);

		Diag.ThrowIfNotOnUIThread();

		uint documentCookie = 0;

		if (RdtManager.InflightMonikerCsbTable.ContainsKey(moniker))
		{
			foreach (KeyValuePair<object, AuxilliaryDocData> pair in ((IBsEditorPackage)ApcManager.PackageInstance).AuxDocDataTable)
			{
				if (!pair.Value.IsVirtualWindow)
					continue;

				if (moniker.Equals(pair.Value.InflightMoniker))
				{
					documentCookie = pair.Value.DocCookie;
					break;
				}
			}

			if (documentCookie == 0)
				RdtManager.InflightMonikerCsbTable.Remove(moniker);
		}

		if (documentCookie != 0)
		{
			string documentMoniker = RdtManager.GetDocumentMoniker(documentCookie);


			IVsUIShellOpenDocument vsUIShellOpenDocument = ApcManager.EnsureService<SVsUIShellOpenDocument, IVsUIShellOpenDocument>();
			Guid rguidEditorType = new(SystemData.C_MandatedSqlEditorFactoryGuid); 
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


		string filename = Path.GetFileName(moniker);

		if (filename.EndsWith(NativeDb.Extension, StringComparison.OrdinalIgnoreCase))
			filename = filename.TrimSuffix(NativeDb.Extension, StringComparison.OrdinalIgnoreCase);


		string tempFilename = Path.Combine(tempDirectory, filename + NativeDb.Extension);

		// Tracer.Trace(typeof(DesignerExplorerServices), "OpenMiscellaneousVirtialFile()", "filename: {0}, tempFilename: {1}, moniker: {2}.",
		//	filename, tempFilename, moniker);


		if (tempFilename == null)
		{
			MessageCtl.ShowEx(string.Empty, Resources.ExCannotCreateTempFile, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return false;
		}

		StreamWriter streamWriter = null;
		try
		{
			initialScript ??= node != null
				? node.GetDecoratedDdlSource(targetType)
				: string.Empty;

			streamWriter = new StreamWriter(tempFilename);
			streamWriter.Write(initialScript);
			streamWriter.Flush();
			streamWriter.Close();
			streamWriter = null;

			RdtManager.InflightMonikerStack = moniker;
			RdtManager.InflightMonikerCsbTable.Add(moniker, csa);

			OpenAsMiscellaneousFile(tempFilename, filename + NativeDb.Extension, new Guid(SystemData.C_EditorFactoryGuid),
				string.Empty, VSConstants.LOGVIEWID_Primary);
		}
		catch
		{
			_ = RdtManager.InflightMonikerStack;
			RdtManager.InflightMonikerCsbTable.Remove(moniker);
		}
		finally
		{
			streamWriter?.Close();
			File.Delete(tempFilename);
			Directory.Delete(tempDirectory);
		}

		return true;
	}



	public static void OpenNewMiscellaneousSqlFile(string baseName, string initialContent)
	{
		// Tracer.Trace(typeof(Cmd), "OpenAsMiscellaneousFile()");

		Diag.ThrowIfNotOnUIThread();

		IVsProject3 miscellaneousProject = UnsafeCmd.MiscellaneousProjectHierarchy;

		miscellaneousProject.GenerateUniqueItemName(VSConstants.VSITEMID_ROOT, NativeDb.Extension, baseName, out string pbstrItemName);
		string tempFileName = Path.GetTempFileName();
		if (tempFileName == null)
		{
			MessageCtl.ShowEx(string.Empty, Resources.ExCannotCreateTempFile, MessageBoxButtons.OK, MessageBoxIcon.Hand);
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
			OpenAsMiscellaneousFile(tempFileName, pbstrItemName, new Guid(SystemData.C_EditorFactoryGuid),
				string.Empty, VSConstants.LOGVIEWID_Primary);
		}
		finally
		{
			streamWriter?.Close();
			File.Delete(tempFileName);
		}
	}



	private static void OpenNewQueryEditor(string datasetKey, string baseName, Guid editorFactory,
		string initialScript, bool executeQuery, bool isClone, string physicalViewName = null)
	{
		if (editorFactory == Guid.Empty)
			editorFactory = new(SystemData.C_MandatedSqlEditorFactoryGuid);

		EnModelTargetType targetType = EnModelTargetType.QueryScript;
		EnModelObjectType objectType = EnModelObjectType.NewQuery;
		EnModelObjectType elementType = objectType;

		if (RctManager.ShutdownState)
			return;

		Csb csb = datasetKey != null ? RctManager.CloneRegistered(datasetKey, EnRctKeyType.DatasetKey) : null;

		IList<string> identifierList = [baseName];
		IList<string> identifierArray = new List<string>(identifierList); 

		// Tracer.Trace(typeof(DesignerExplorerServices), "OpenNewQueryEditor()", "csa.DataSource: {0}, csa.Database: {1}", csa.DataSource, csa.Database);

		string mkDocument = Moniker.BuildDocumentMoniker(csb?.DataSource, csb?.Database, elementType, ref identifierArray, targetType, true, isClone);

		ModelCsb csa = null;

		if (csb != null)
		{
			csa = new(csb)
			{
				CreationFlags = EnCreationFlags.CreateConnection
			};
		}

		RaiseBeforeOpenDocument(mkDocument, csa, identifierArray, objectType, targetType, S_BeforeOpenDocumentHandler);

		OpenMiscellaneousVirtualFile(mkDocument, null, targetType, csa, initialScript);

		return;

	}



	public void NewQuery(string datasetKey, string baseName, string initialScript)
	{
		// Sanity check.
		// Currently our only entry point to AbstractDesignerServices whose warnings are suppressed.

		Diag.ThrowIfNotOnUIThread();


		Guid clsidEditorFactory = new Guid(SystemData.C_EditorFactoryGuid);

		OpenNewQueryEditor(datasetKey, baseName, clsidEditorFactory, initialScript, false, false);
		
	}

	public void CloneQuery(string datasetKey, string baseName, string initialScript)
	{
		// Sanity check.
		// Currently our only entry point to AbstractDesignerServices whose warnings are suppressed.

		Diag.ThrowIfNotOnUIThread();


		Guid clsidEditorFactory = new Guid(SystemData.C_EditorFactoryGuid);

		OpenNewQueryEditor(datasetKey, baseName, clsidEditorFactory, initialScript, false, true);

	}



	// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
	// Microsoft.VisualStudio.Data.Tools.Package.Explorers.SqlServerObjectExplorer.SqlServerObjectExplorerService:ViewCode()
	public void ViewCode(IVsDataExplorerNode node, EnModelTargetType targetType)
	{
		// Sanity check.
		// Currently our only entry point to AbstractDesignerServices whose warnings are suppressed.
		Diag.ThrowIfNotOnUIThread();

		// Tracer.Trace(GetType(), "ViewCode()");

		Moniker moniker;
		IList<string> identifierList;
		EnModelObjectType objectType;
		bool autoExecute = false;

		Guid clsidEditorFactory = new Guid(SystemData.C_EditorFactoryGuid);

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
				autoExecute = true;
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

		try
		{
			OpenExplorerEditor(node, objectType, identifierList, targetType, clsidEditorFactory, autoExecute, null);
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

		IBsTabbedEditorPane lastFocusedSqlEditor = ((IBsEditorPackage)ApcManager.PackageInstance).LastFocusedSqlEditor;

		// Tracer.Trace(GetType(), "OnSqlQueryLoaded()", "lastFocusedSqlEditor != null: {0}.", lastFocusedSqlEditor != null);

		if (lastFocusedSqlEditor != null)
		{
			CommandExecuteQuery command = new (lastFocusedSqlEditor);

			_ = Task.Run(() => OnSqlQueryLoadedAsync(command, 50));
		}
	}



	private async Task<bool> OnSqlQueryLoadedAsync(AbstractCommand command, int delay)
	{
		// Tracer.Trace(GetType(), "OnSqlQueryLoadedAsync()");

		// Give editor time to breath.
		if (delay > 0)
			System.Threading.Thread.Sleep(delay);

		await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

		command.Exec((uint)OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, IntPtr.Zero, IntPtr.Zero);

		return true;
	}


	#endregion Event Handling


}
