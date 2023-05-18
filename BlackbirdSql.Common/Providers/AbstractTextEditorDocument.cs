
using System;
using System.Runtime.InteropServices;


using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.DataTools.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

using BlackbirdSql.Common.Properties;



namespace BlackbirdSql.Common.Providers;


// [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "<Pending>")]


/// <summary>
/// Plagiarized off of Microsoft.VisualStudio.Data.Providers.Common.TextEditorDocument
/// </summary>
internal abstract class AbstractTextEditorDocument : AbstractDataToolsDocument
{
	private string _Text;

	public string Text
	{
		get
		{
			if (_Text == null)
			{
				_Text = LoadText();
				if (_Text == null)
				{
					InvalidOperationException ex = new(Resources.TextEditorDocument_FailedToLoadText);
					Diag.Dug(ex);
					throw ex;
				}
			}

			return _Text;
		}
	}


	
	
	public AbstractTextEditorDocument(int owningItemId, IVsDataViewHierarchy owningHierarchy)
		: base(Guid.Empty, NativeMethods.CMDUIGUID_TextEditor, typeof(IDTSqlTextEditorFactory).GUID, owningItemId, owningHierarchy)
	{
	}

	public AbstractTextEditorDocument(IVsDataObject obj, int owningItemId, IVsDataViewHierarchy owningHierarchy)
		: base(obj, Guid.Empty, NativeMethods.CMDUIGUID_TextEditor, typeof(IDTSqlTextEditorFactory).GUID, owningItemId, owningHierarchy)
	{
	}

	public static bool IsExecuting(Hostess host)
	{
		NativeMethods.IDTInternalRunManager service = host.GetService<NativeMethods.IDTInternalRunManager>(NativeMethods.SID_SDTInternalRunManager);
		return service.IsExecuting;
	}


	protected static void ExecuteOrCancel(Hostess host, IVsDataExplorerNode node, bool debug, int providerType, int objectType, string objectSchema, string objectName, IVsDataViewHierarchy owningHierarchy)
	{
		NativeMethods.IDTInternalRunManager service = host.GetService<NativeMethods.IDTInternalRunManager>(NativeMethods.SID_SDTInternalRunManager);
		IVsDataConnectionManager service2 = host.GetService<IVsDataConnectionManager>();
		Guid provider = node.ExplorerConnection.Provider;
		IVsDataConnection val = service2.GetConnection(provider, node.ExplorerConnection.EncryptedConnectionString, true);
		string objectTypeDisplayName = AbstractDataToolsDocument.GetObjectTypeDisplayName(node.Object, node.ItemId, owningHierarchy);
		service.RunProcedure(val, providerType | objectType, objectTypeDisplayName, objectName, objectSchema, debug);
	}

	protected abstract string LoadText();

	protected override IVsWindowFrame CreateWindowFrame()
	{
		Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
		IVsWindowFrame result = base.CreateWindowFrame();
		if (IsReadOnly)
		{
			IVsTextLines ppTextBuffer = null;
			if (UnderlyingDocument.DocData is IVsTextBufferProvider vsTextBufferProvider)
			{
				NativeMethods.WrapComCall(vsTextBufferProvider.GetTextBuffer(out ppTextBuffer));
			}

			if (ppTextBuffer != null)
			{
				NativeMethods.WrapComCall(ppTextBuffer.GetStateFlags(out uint pdwReadOnlyFlags));
				pdwReadOnlyFlags |= 1u;
				NativeMethods.WrapComCall(ppTextBuffer.SetStateFlags(pdwReadOnlyFlags));
			}
		}

		return result;
	}

	protected override IDTDocTool CreateUnderlyingDocument(object factory)
	{
		IDTDocTool result = null;
		if (factory is IDTSqlTextEditorFactory iDTSqlTextEditorFactory)
		{
			result = iDTSqlTextEditorFactory.EditSqlText(this, ServiceProvider) as IDTDocTool;
		}

		return result;
	}
}
