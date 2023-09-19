// Microsoft.VisualStudio.Data.Framework, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Framework.DSRefBuilder
using System;
using System.Collections;
using System.ComponentModel.Design;
using System.Globalization;
using BlackbirdSql.Controller;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.VisualStudio.Ddex.Properties;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Data.Services.SupportEntities.Interop;



namespace BlackbirdSql.VisualStudio.Ddex.Ctl.DataTools;

/// <summary>
/// Replacement for Microsoft.VisualStudio.Data.Framework.DSRefBuilder for tracing,
/// intercepting and future improvements.
/// </summary>
public class DSRefBuilder : DataSiteableObject<IVsDataConnection>, IDSRefBuilder, IVsDataSupportObject<IDSRefBuilder>
{
	private const __DSREFTYPE C_DsRefTypeRootOrExtendedOrDb = __DSREFTYPE.DSREFTYPE_DATASOURCEROOT
		| __DSREFTYPE.DSREFTYPE_DATABASE | __DSREFTYPE.DSREFTYPE_EXTENDED; // 18448

	public DSRefBuilder()
	{
		Tracer.Trace(GetType(), "DSRefBuilder.DSRefBuilder");
	}

	public DSRefBuilder(IVsDataConnection connection)
		: base(connection)
	{
		Tracer.Trace(GetType(), "DSRefBuilder.DSRefBuilder(IVsDataConnection)");
	}



	/// <summary>
	/// Implementation of <see cref="IDSRefBuilder.AppendToDSRef(object, string, object[])"/>.
	/// </summary>
	public void AppendToDSRef(object dsRef, string typeName, object[] identifier)
	{
		Tracer.Trace(GetType(), "IDSRefBuilder.AppendToDSRef");
		AppendToDSRef(dsRef, typeName, identifier, null);
	}


	/// <summary>
	/// Implementation of <see cref="IVsDataSupportObject{T}.Invoke(string, object[], object[])"/>.
	/// </summary>
	public object Invoke(string name, object[] args, object[] parameters)
	{
		Tracer.Trace(GetType(), "IVsDataSupportObject{IDSRefBuilder}.Invoke");
		try
		{
			if (name == null)
				throw new ArgumentNullException("name");

			if (name.Equals("AppendToDSRef", StringComparison.Ordinal))
			{
				if (args == null || args.Length != 3)
					throw new ArgumentException(Resources.DataSupportObject_InvalidInvokeArguments, "args");

				AppendToDSRef(args[0], args[1] as string, args[2] as object[], parameters);

				if (PackageController.Instance.Uig.ShowDiagramPane)
				{
					Hostess host = new(Site);
					CommandID cmd = new CommandID(VSConstants.GUID_VSStandardCommandSet97, (int)VSConstants.VSStd97CmdID.ShowGraphicalPane);
					// Delay 10 ms just to give Editor WindowFrame and QueryDesignerDocument time to breath.
					host.PostExecuteCommand(cmd, 10);
				}

				return null;
			}

			throw new ArgumentException(Resources.DataSupportObject_UnknownInvokeName, "name");
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
	}

	protected virtual void AppendToDSRef(object dsRef, string typeName, object[] identifier, object[] parameters)
	{
		Tracer.Trace(GetType(), "DSRefBuilder.AppendToDSRef(object dsRef, string typeName, ...)");
		try
		{
			if (dsRef == null)
				throw new ArgumentNullException("dsRef");
			if (typeName == null)
				throw new ArgumentNullException("typeName");
			if (identifier == null)
				throw new ArgumentNullException("identifier");
			if (Site == null)
				throw new InvalidOperationException(Resources.DSRefBuilder_MissingSite);

			IDSRefConsumer dsRefConsumer = dsRef as IDSRefConsumer;
			IntPtr intPtr = dsRefConsumer.GetFirstChildNode(VS.DSREFNODEID_ROOT);
			IntPtr intPtr2 = VS.DSREFNODEID_NIL;

			while (intPtr != VS.DSREFNODEID_NIL
				&& (!(new Guid(dsRefConsumer.GetProperty(intPtr, ref VS.CLSID_DSRefProperty_Provider) as string) == Site.Provider)
				|| !NodeEquals(dsRefConsumer, intPtr, Site.SafeConnectionString, "dbo", C_DsRefTypeRootOrExtendedOrDb)))
			{
				intPtr2 = intPtr;
				intPtr = dsRefConsumer.GetNextSiblingNode(intPtr);
			}

			if (intPtr == VS.DSREFNODEID_NIL)
			{
				IDSRefProvider dsRefProvider = dsRef as IDSRefProvider;
				intPtr = ((!(intPtr2 == VS.DSREFNODEID_NIL))
					? dsRefProvider.CreateNextSiblingNode(intPtr2)
					: dsRefProvider.CreateFirstChildNode(VS.DSREFNODEID_ROOT));

				dsRefProvider.SetProperty(intPtr, ref VS.CLSID_DSRefProperty_Provider, Site.Provider.ToString());

				dsRefProvider.SetName(intPtr, Site.SafeConnectionString);
				dsRefProvider.SetOwner(intPtr, "dbo");
				dsRefProvider.SetType(intPtr, C_DsRefTypeRootOrExtendedOrDb);

				Guid pguidType = Guid.Empty;
				dsRefProvider.SetExtendedType(intPtr, ref pguidType);
				dsRefProvider.SetProperty(intPtr, ref VS.CLSID_DSRefProperty_PreciseType, 1);
			}

			if (parameters == null)
				return;

			for (int i = 0; i < parameters.Length; i++)
			{
				if (parameters[i] is DictionaryEntry entry && entry.Value is object[] parameters2)
				{
					AppendToDSRef(dsRef, intPtr, identifier, parameters2);
				}
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
	}

	private void AppendToDSRef(object dsRef, IntPtr parentNode, object[] identifier, object[] parameters)
	{
		Tracer.Trace(GetType(), "DSRefBuilder.AppendToDSRef(object dsRef, IntPtr parentNode, ...)");
		try
		{
			string text = null;
			if (parameters.Length != 0)
			{
				if (parameters[0] != null && parameters[0] is not string)
				{
					throw new ArgumentException(Resources.DSRefBuilder_InvalidParameters);
				}

				text = parameters[0] as string;
				if (text != null && text.StartsWith("{", StringComparison.Ordinal))
				{
					text = string.Format(CultureInfo.InvariantCulture, text, identifier);
				}
			}

			string text2 = null;
			if (parameters.Length > 1)
			{
				if (parameters[1] != null && parameters[1] is not string)
				{
					throw new ArgumentException(Resources.DSRefBuilder_InvalidParameters);
				}

				text2 = parameters[1] as string;
				if (text2 != null && text2.StartsWith("{", StringComparison.Ordinal))
				{
					text2 = string.Format(CultureInfo.InvariantCulture, text2, identifier);
				}
			}

			__DSREFTYPE dsRefType = __DSREFTYPE.DSREFTYPE_NULL;

			if (parameters.Length > 2)
			{
				if (parameters[2] != null && parameters[2] is not string)
				{
					throw new ArgumentException(Resources.DSRefBuilder_InvalidParameters);
				}
				try
				{
					if (parameters[2] != null)
					{
						dsRefType = (__DSREFTYPE)Enum.Parse(typeof(__DSREFTYPE),
							"DSREFTYPE_" + (parameters[2] as string), ignoreCase: true);
					}
				}
				catch (ArgumentException innerException)
				{
					throw new ArgumentException(Resources.DSRefBuilder_InvalidParameters, innerException);
				}
			}

			IntPtr intPtr = CreateOrMatchNode(dsRef, text, text2, dsRefType, parentNode, out bool isNew);
			if (isNew)
			{
				IDSRefProvider dsRefProvider = dsRef as IDSRefProvider;

				if (text != null)
					dsRefProvider.SetName(intPtr, text);

				if (text2 != null)
					dsRefProvider.SetOwner(intPtr, text2);

				if (dsRefType != 0)
					dsRefProvider.SetType(intPtr, dsRefType);

				if (parameters.Length > 3)
				{
					if (parameters[3] != null && parameters[3] is not Guid && parameters[3] is not string)
					{
						throw new ArgumentException(Resources.DSRefBuilder_InvalidParameters);
					}

					Guid pguidType = Guid.Empty;

					if (parameters[3] is Guid guid)
						pguidType = guid;
					else if (parameters[3] != null)
						pguidType = new Guid(parameters[3] as string);

					if (pguidType != Guid.Empty)
						dsRefProvider.SetExtendedType(intPtr, ref pguidType);
				}

				if (parameters.Length > 4 && parameters[4] is DictionaryEntry entry && entry.Value is object[] array)
				{
					object[] array2 = array;
					foreach (object obj in array2)
					{
						Guid empty = Guid.Empty;
						object obj2 = null;
						obj2 = ((obj is not DictionaryEntry dictionaryEntry) ? obj : dictionaryEntry.Key);
						string text3 = obj2 as string;
						if (obj2 is not Guid && text3 == null)
						{
							throw new ArgumentException(Resources.DSRefBuilder_InvalidParameters);
						}

						empty = (obj2 is not Guid guid) ? new Guid(text3) : guid;
						object obj3 = null;

						if (obj is DictionaryEntry dictionaryEntry2
							&& dictionaryEntry2.Value is object[] array3
							&& array3.Length != 0)
						{
							obj3 = array3[0];
							if (obj3 is string text4 && text4.StartsWith("{", StringComparison.Ordinal))
							{
								obj3 = string.Format(CultureInfo.CurrentCulture, text4, identifier);
							}
						}

						dsRefProvider.SetProperty(intPtr, ref empty, obj3);
					}
				}
			}

			if (parameters.Length <= 5 || parameters[5] is not DictionaryEntry param
				|| param.Value is not object[] array4)
			{
				return;
			}

			for (int j = 0; j < array4.Length; j++)
			{
				if (array4[j] is DictionaryEntry entry && entry.Value is object[] parameters2)
				{
					AppendToDSRef(dsRef, intPtr, identifier, parameters2);
				}
			}
			
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

	}

	private static IntPtr CreateOrMatchNode(object dsRef, string name, string owner,
		__DSREFTYPE dsRefType, IntPtr parentNode, out bool isNew)
	{
		try
		{
			IDSRefConsumer dsRefConsumer = dsRef as IDSRefConsumer;
			IntPtr intPtr = dsRefConsumer.GetFirstChildNode(parentNode);
			IntPtr intPtr2 = VS.DSREFNODEID_NIL;

			while (intPtr != VS.DSREFNODEID_NIL && !NodeEquals(dsRefConsumer, intPtr, name, owner, dsRefType))
			{
				intPtr2 = intPtr;
				intPtr = dsRefConsumer.GetNextSiblingNode(intPtr);
			}

			if (intPtr == VS.DSREFNODEID_NIL)
			{
				IDSRefProvider dsRefProvider = dsRef as IDSRefProvider;

				intPtr = ((!(intPtr2 == VS.DSREFNODEID_NIL))
					? dsRefProvider.CreateNextSiblingNode(intPtr2)
					: dsRefProvider.CreateFirstChildNode(parentNode));

				isNew = true;
			}
			else
			{
				isNew = false;
			}

			return intPtr;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
	}



	private static bool NodeEquals(IDSRefConsumer dsRefConsumer, IntPtr node, string name,
		string owner, __DSREFTYPE dsRefType)
	{
		try
		{
			string name2 = dsRefConsumer.GetName(node);
			if (!string.Equals(name2, name, StringComparison.Ordinal))
			{
				return false;
			}

			string owner2 = dsRefConsumer.GetOwner(node);
			if (!string.Equals(owner2, owner, StringComparison.Ordinal))
			{
				return false;
			}

			__DSREFTYPE type2 = dsRefConsumer.GetType(node);
			if (type2 != dsRefType)
			{
				return false;
			}

			return true;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
	}
}
