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

	[Guid(CommandProperties.SqlVirtualEditorGuid)]



	/// <summary>
	/// A base class for custom language services and editor factories
	/// </summary>
	public class EditorFactory : AbstractEditorFactory
	{




		/// <summary>
		/// Default .ctor 
		/// </summary>
		public EditorFactory(Package package, Guid languageGuid) : base(package, languageGuid)
		{
		}




		/// <inheritdoc/>
		public virtual int CreateEditorInstance(uint createEditorFlags, string documentMoniker, string physicalView,
			IVsHierarchy hierarchy, uint itemid, string text, out IntPtr docView, out IntPtr docData,
			out string editorCaption, out Guid commandUIGuid, out int createDocumentWindowFlags)
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
			CreateDocDataFromText(text, out IVsPersistDocData docDataExistingObject);
			IntPtr docDataExisting = Marshal.GetIUnknownForObject(docDataExistingObject);


			IVsTextLines textLines = (IVsTextLines)docDataExistingObject;

			docData = docDataExisting;
			Marshal.AddRef(docData);

			object docViewObject;

			try
			{
				docViewObject = CreateDocumentView(documentMoniker, physicalView, hierarchy, itemid, textLines,
					false, out editorCaption, out commandUIGuid);
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
		/// Loads a string into a docData text buffer.
		/// </summary>
		protected int CreateDocDataFromText(string text, out IVsPersistDocData docDataObject)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			// Create a new IVsTextLines buffer.
			Type persistDocDataType = typeof(IVsPersistDocData);
			Guid riid = persistDocDataType.GUID;
			Guid clsid = typeof(VsTextBufferClass).GUID;

			// Create the text buffer
			docDataObject = Package.CreateInstance(ref clsid, ref riid, persistDocDataType) as IVsPersistDocData;

			// Site the buffer
			IObjectWithSite objectWithSite = (IObjectWithSite)docDataObject!;
			objectWithSite.SetSite(Package);
			// objectWithSite.SetSite(ServiceProvider.GetService(typeof(IOleServiceProvider)));

			// Cast the buffer to a textlines buffer to load the text.
			IVsTextLines textLines = (IVsTextLines)docDataObject;
			textLines.InitializeContent(text, text.Length);

			if (LanguageGuid != Guid.Empty)
				textLines.SetLanguageServiceID(LanguageGuid);

			return VSConstants.S_OK;

		}

	}
}