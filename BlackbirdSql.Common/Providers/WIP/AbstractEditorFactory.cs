/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation.
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A
 * copy of the license can be found in the License.html file at the root of this distribution. If
 * you cannot locate the Apache License, Version 2.0, please send an email to
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * This source code has been modified from its original form.
 *
 * ***************************************************************************/

using System;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

using BlackbirdSql.Common.Commands;

using IServiceProvider = System.IServiceProvider;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;




namespace BlackbirdSql.Common.Providers
{



	/// <summary>
	/// A base class for custom editor factories that implements the core
	/// functionality of <see cref="IVsEditorFactory"/>.
	/// The EditorFactory class extends the core for Firebird expressions
	/// in the SE.
	/// </summary>
	public abstract class AbstractEditorFactory : IVsEditorFactory
	{
		private readonly Guid _ServiceId;
		private readonly Package _Package;
		private readonly Guid _LanguageGuid;

		private ServiceProvider _ServiceProvider = null;


		/// <inheritdoc/>

		/// <summary>
		/// An array of file extensions associated with this language.
		/// </summary>
		public string[] FileExtensions
		{
			get
			{
				return new string[] { "sql" };
			}
		}


		protected Guid LanguageGuid { get { return _LanguageGuid; } }


		protected Package Package { get { return _Package; } }



		/// <inheritdoc/>
		protected virtual bool PromptEncodingOnLoad => false;


		protected ServiceProvider ServiceProvider
		{
			get
			{
				ThreadHelper.ThrowIfNotOnUIThread();
				if (_ServiceProvider == null)
				{
					SetSite(_Package);
				}
				return _ServiceProvider;
			}
		}

		#region IVsEditorFactory




		/// <summary>
		/// Default .ctor 
		/// </summary>
		public AbstractEditorFactory(Package package, Guid languageGuid)
		{
			_Package = package;
			_ServiceId = GetType().GUID;
			_LanguageGuid = languageGuid;

		}


		/// <inheritdoc/>
		public virtual int SetSite(IOleServiceProvider psp)
		{
			_ServiceProvider = new ServiceProvider(psp);
			return VSConstants.S_OK;
		}

		// This method is called by the Environment (inside IVsUIShellOpenDocument::
		// OpenStandardEditor and OpenSpecificEditor) to map a LOGICAL view to a
		// PHYSICAL view. A LOGICAL view identifies the purpose of the view that is
		// desired (e.g. a view appropriate for Debugging [LOGVIEWID_Debugging], or a
		// view appropriate for text view manipulation as by navigating to a find
		// result [LOGVIEWID_TextView]). A PHYSICAL view identifies an actual type
		// of view implementation that an IVsEditorFactory can create.
		//
		// NOTE: Physical views are identified by a string of your choice with the
		// one constraint that the default/primary physical view for an editor
		// *MUST* use a NULL string as its physical view name (*pbstrPhysicalView = NULL).
		//
		// NOTE: It is essential that the implementation of MapLogicalView properly
		// validates that the LogicalView desired is actually supported by the editor.
		// If an unsupported LogicalView is requested then E_NOTIMPL must be returned.
		//
		// NOTE: The special Logical Views supported by an Editor Factory must also
		// be registered in the local registry hive. LOGVIEWID_Primary is implicitly
		// supported by all editor types and does not need to be registered.
		// For example, an editor that supports a ViewCode/ViewDesigner scenario
		// might register something like the following:
		//        HKLM\Software\Microsoft\VisualStudio\9.0\Editors\
		//            {...guidEditor...}\
		//                LogicalViews\
		//                    {...LOGVIEWID_TextView...} = s ''
		//                    {...LOGVIEWID_Code...} = s ''
		//                    {...LOGVIEWID_Debugging...} = s ''
		//                    {...LOGVIEWID_Designer...} = s 'Form'
		//
		/// <inheritdoc/>
		public virtual int MapLogicalView(ref Guid logicalView, out string physicalView)
		{
			// initialize out parameter
			physicalView = null;

			bool isSupportedView = false;
			// Determine the physical view
			if (VSConstants.LOGVIEWID_Primary == logicalView ||
				VSConstants.LOGVIEWID_Debugging == logicalView ||
				VSConstants.LOGVIEWID_Code == logicalView ||
				VSConstants.LOGVIEWID_UserChooseView == logicalView ||
				VSConstants.LOGVIEWID_TextView == logicalView)
			{
				// primary view uses NULL as pbstrPhysicalView
				isSupportedView = true;
			}
			else if (VSConstants.LOGVIEWID_Designer == logicalView)
			{
				physicalView = "Design";
				isSupportedView = true;
			}

			if (isSupportedView)
			{
				return VSConstants.S_OK;
			}
			else
			{
				// E_NOTIMPL must be returned for any unrecognized rguidLogicalView values
				return VSConstants.E_NOTIMPL;
			}
		}

		/// <inheritdoc/>
		public virtual int Close()
		{
			return VSConstants.S_OK;
		}



		public virtual int CreateEditorInstance(uint createEditorFlags, string documentMoniker,
			string physicalView, IVsHierarchy hierarchy, uint itemid, IntPtr docDataExisting,
			out IntPtr docView, out IntPtr docData, out string editorCaption, out Guid commandUIGuid,
			out int createDocumentWindowFlags)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			// Initialize output parameters
			docView = IntPtr.Zero;
			docData = IntPtr.Zero;
			commandUIGuid = Guid.Empty;
			createDocumentWindowFlags = 0;
			editorCaption = null;

			// Validate inputs
			if ((createEditorFlags & (uint)(VSConstants.CEF.OpenFile | VSConstants.CEF.Silent)) == 0)
			{
				return VSConstants.E_INVALIDARG;
			}

			// Get a text buffer
			IVsTextLines textLines = GetTextBuffer(docDataExisting, documentMoniker);

			// Assign docData IntPtr to either existing docData or the new text buffer
			if (docDataExisting != IntPtr.Zero)
			{
				docData = docDataExisting;
				Marshal.AddRef(docData);
			}
			else
			{
				docData = Marshal.GetIUnknownForObject(textLines);
			}

			object docViewObject;

			try
			{
				docViewObject = CreateDocumentView(documentMoniker, physicalView, hierarchy, itemid, textLines,
					docDataExisting == IntPtr.Zero, out editorCaption, out commandUIGuid);
			}
			catch (Exception ex)
			{
				if (docDataExisting != docData && docData != IntPtr.Zero)
				{
					// Cleanup the instance of the docData that we have addref'ed
					Marshal.Release(docData);
					docData = IntPtr.Zero;
				}

				Diag.Dug(ex);
				throw ex;
			}


			try
			{
				docView = Marshal.GetIUnknownForObject(docViewObject);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw ex;
			}
			finally
			{
				if (docView == IntPtr.Zero)
				{
					if (docDataExisting != docData && docData != IntPtr.Zero)
					{
						// Cleanup the instance of the docData that we have addref'ed
						Marshal.Release(docData);
						docData = IntPtr.Zero;
					}
				}
			}

			return VSConstants.S_OK;
		}


		/// <summary>
		/// Gets the text view lines from the doc data.
		/// </summary>
		protected IVsTextLines GetTextBuffer(IntPtr docDataExisting, string filename)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			IVsTextLines textLines;

			if (docDataExisting == IntPtr.Zero)
			{
				// Create a new IVsTextLines buffer.
				Type textLinesType = typeof(IVsTextLines);
				Guid riid = textLinesType.GUID;
				Guid clsid = typeof(VsTextBufferClass).GUID;
				textLines = _Package.CreateInstance(ref clsid, ref riid, textLinesType) as IVsTextLines;

				// set the buffer's site
				IObjectWithSite objectWithSite = (IObjectWithSite)textLines!;
				objectWithSite.SetSite(ServiceProvider.GetService(typeof(IOleServiceProvider)));
			}
			else
			{
				// Use the existing text buffer
				object dataObject = Marshal.GetObjectForIUnknown(docDataExisting);
				textLines = dataObject as IVsTextLines;
				if (textLines == null)
				{
					// Try get the text buffer from textbuffer provider
					if (dataObject is IVsTextBufferProvider textBufferProvider)
					{
						textBufferProvider.GetTextBuffer(out textLines);
					}
				}
				if (textLines == null)
				{
					// Unknown docData type then, so we have to force VS to close the other editor.
					throw Marshal.GetExceptionForHR(VSConstants.VS_E_INCOMPATIBLEDOCDATA);
				}

			}

			return textLines;

		}




		/// <summary>
		/// Creates the document view
		/// </summary>
		protected object CreateDocumentView(string documentMoniker, string physicalView, IVsHierarchy hierarchy, uint itemid,
			IVsTextLines textLines, bool createdDocData, out string editorCaption, out Guid cmdUI)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			//Init out params
			editorCaption = string.Empty;
			cmdUI = Guid.Empty;

			if (string.IsNullOrEmpty(physicalView))
			{
				// create code window as default physical view
				return CreateCodeView(documentMoniker, textLines, createdDocData, ref editorCaption, ref cmdUI);
			}

			// We couldn't create the view
			// Return special error code so VS can try another editor factory.
			Exception ex = Marshal.GetExceptionForHR(VSConstants.VS_E_UNSUPPORTEDFORMAT);
			Diag.Dug(ex);
			throw ex;
		}



		/// <summary>
		/// Creates the code view.
		/// </summary>
		protected IVsCodeWindow CreateCodeView(string documentMoniker, IVsTextLines textLines, bool createdDocData,
			ref string editorCaption, ref Guid cmdUI)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (ServiceProvider == null)
			{
				Exception ex = new("ServiceProvider can't be null");
				Diag.Dug(ex);
				throw ex;
			}


			IComponentModel2 compService = (IComponentModel2)ServiceProvider.GlobalProvider.GetService(typeof(SComponentModel));
			if (compService == null)
			{
				Exception ex = new("GetService SComponentModel failed");
				Diag.Dug(ex);
				throw ex;
			}

			IVsEditorAdaptersFactoryService adapterService = compService.GetService<IVsEditorAdaptersFactoryService>();
			if (adapterService == null)
			{
				Exception ex = new("GetService IVsEditorAdaptersFactoryService failed");
				Diag.Dug(ex);
				throw ex;
			}

			IVsCodeWindow codeWindow;

			try
			{
				codeWindow = adapterService.CreateVsCodeWindowAdapter(Package);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw ex;
			}

			if (codeWindow == null)
			{
				Exception ex = new("IVsEditorAdaptersFactoryService.CreateVsCodeWindowAdapter failed");
				Diag.Dug(ex);
				throw ex;
			}



			try
			{
				// ErrorHandler.ThrowOnFailure(codeWindow.SetBuffer(textLines));
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw ex;
			}
			try
			{
				ErrorHandler.ThrowOnFailure(codeWindow.SetBaseEditorCaption(null));
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw ex;
			}
			try
			{
				// ErrorHandler.ThrowOnFailure(codeWindow.GetEditorCaption(READONLYSTATUS.ROSTATUS_Unknown, out editorCaption));
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw ex;
			}


			cmdUI = VSConstants.GUID_TextEditorFactory;

			if (!createdDocData && textLines != null)
			{
				// we have a pre-created buffer, go ahead and initialize now as the buffer already
				// exists and is initialized
				TextBufferEventListener bufferEventListener = new(textLines, _ServiceId);
				bufferEventListener.OnLoadCompleted(0);
			}

			return codeWindow;
		}


		private sealed class TextBufferEventListener : IVsTextBufferDataEvents
		{
			private readonly IVsTextLines _textLines;

			private readonly IConnectionPoint _connectionPoint;
			private readonly uint _cookie;
			private Guid _languageServiceId;

			public TextBufferEventListener(IVsTextLines textLines, Guid languageServiceId)
			{
				ThreadHelper.ThrowIfNotOnUIThread();
				_textLines = textLines;
				_languageServiceId = languageServiceId;

				IConnectionPointContainer connectionPointContainer = textLines as IConnectionPointContainer;
				Guid bufferEventsGuid = typeof(IVsTextBufferDataEvents).GUID;
				connectionPointContainer?.FindConnectionPoint(ref bufferEventsGuid, out _connectionPoint);
				_connectionPoint!.Advise(this, out _cookie);
			}

			public void OnFileChanged(uint grfChange, uint dwFileAttrs)
			{
			}

			public int OnLoadCompleted(int fReload)
			{
				ThreadHelper.ThrowIfNotOnUIThread();
				_connectionPoint.Unadvise(_cookie);
				_textLines.SetLanguageServiceID(ref _languageServiceId);

				return VSConstants.S_OK;
			}
		}

		#endregion
	}
}