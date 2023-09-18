// Microsoft.VisualStudio.Data.Framework, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Framework.DSRefBuilder
using System;
using System.Collections;
using System.Globalization;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Data.Services.SupportEntities.Interop;


namespace BlackbirdSql.Common.Ctl.DataTools;

/// <summary>
/// Replacement for Microsoft.VisualStudio.Data.Framework.DSRefBuilder for tracing
/// and future improvements. Disabled
/// </summary>
public class DSRefBuilder : DataSiteableObject<IVsDataConnection>, IDSRefBuilder, IVsDataSupportObject<IDSRefBuilder>
{
	public DSRefBuilder()
	{
		Tracer.Trace(GetType(), "DSRefBuilder.DSRefBuilder");
	}

	public DSRefBuilder(IVsDataConnection connection)
		: base(connection)
	{
		Tracer.Trace(GetType(), "DSRefBuilder.DSRefBuilder(IVsDataConnection)");
	}

	public void AppendToDSRef(object dsRef, string typeName, object[] identifier)
	{
		Tracer.Trace(GetType(), "DSRefBuilder.AppendToDSRef");
		AppendToDSRef(dsRef, typeName, identifier, null);
	}

	object IVsDataSupportObject<IDSRefBuilder>.Invoke(string name, object[] args, object[] parameters)
	{
		Tracer.Trace(GetType(), "DSRefBuilder.Invoke");
		try
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (name.Equals("AppendToDSRef", StringComparison.Ordinal))
			{
				if (args == null || args.Length != 3)
				{
					throw new ArgumentException(Resources.DataSupportObject_InvalidInvokeArguments, "args");
				}
				AppendToDSRef(args[0], args[1] as string, args[2] as object[], parameters);
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
		Tracer.Trace(GetType(), "IDSRefBuilder.AppendToDSRef");
		try
		{
			if (dsRef == null)
			{
				throw new ArgumentNullException("dsRef");
			}
			if (typeName == null)
			{
				throw new ArgumentNullException("typeName");
			}
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (base.Site == null)
			{
				throw new InvalidOperationException(Resources.DSRefBuilder_MissingSite);
			}
			IDSRefConsumer iDSRefConsumer = dsRef as IDSRefConsumer;
			IntPtr intPtr = iDSRefConsumer.GetFirstChildNode(VS.DSREFNODEID_ROOT);
			IntPtr intPtr2 = VS.DSREFNODEID_NIL;
			while (intPtr != VS.DSREFNODEID_NIL
				&& (!(new Guid(iDSRefConsumer.GetProperty(intPtr, ref VS.CLSID_DSRefProperty_Provider) as string) == base.Site.Provider)
				|| !NodeEquals(iDSRefConsumer, intPtr, base.Site.SafeConnectionString, "dbo", (__DSREFTYPE)18448)))
			{
				intPtr2 = intPtr;
				intPtr = iDSRefConsumer.GetNextSiblingNode(intPtr);
			}
			if (intPtr == VS.DSREFNODEID_NIL)
			{
				IDSRefProvider iDSRefProvider = dsRef as IDSRefProvider;
				intPtr = ((!(intPtr2 == VS.DSREFNODEID_NIL)) ? iDSRefProvider.CreateNextSiblingNode(intPtr2) : iDSRefProvider.CreateFirstChildNode(VS.DSREFNODEID_ROOT));
				iDSRefProvider.SetProperty(intPtr, ref VS.CLSID_DSRefProperty_Provider, base.Site.Provider.ToString());

				iDSRefProvider.SetName(intPtr, base.Site.SafeConnectionString);
				iDSRefProvider.SetOwner(intPtr, "dbo");
				iDSRefProvider.SetType(intPtr, (__DSREFTYPE)18448);
				Guid pguidType = Guid.Empty;
				iDSRefProvider.SetExtendedType(intPtr, ref pguidType);
				iDSRefProvider.SetProperty(intPtr, ref VS.CLSID_DSRefProperty_PreciseType, 1);
			}
			if (parameters == null)
			{
				return;
			}
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
			__DSREFTYPE _DSREFTYPE = __DSREFTYPE.DSREFTYPE_NULL;
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
						_DSREFTYPE = (__DSREFTYPE)Enum.Parse(typeof(__DSREFTYPE), "DSREFTYPE_" + (parameters[2] as string), ignoreCase: true);
					}
				}
				catch (ArgumentException innerException)
				{
					throw new ArgumentException(Resources.DSRefBuilder_InvalidParameters, innerException);
				}
			}
			IntPtr intPtr = CreateOrMatchNode(dsRef, text, text2, _DSREFTYPE, parentNode, out bool isNew);
			if (isNew)
			{
				IDSRefProvider iDSRefProvider = dsRef as IDSRefProvider;
				if (text != null)
				{
					iDSRefProvider.SetName(intPtr, text);
				}
				if (text2 != null)
				{
					iDSRefProvider.SetOwner(intPtr, text2);
				}
				if (_DSREFTYPE != 0)
				{
					iDSRefProvider.SetType(intPtr, _DSREFTYPE);
				}
				if (parameters.Length > 3)
				{
					if (parameters[3] != null && parameters[3] is not Guid && parameters[3] is not string)
					{
						throw new ArgumentException(Resources.DSRefBuilder_InvalidParameters);
					}
					Guid pguidType = Guid.Empty;
					if (parameters[3] is Guid guid)
					{
						pguidType = guid;
					}
					else if (parameters[3] != null)
					{
						pguidType = new Guid(parameters[3] as string);
					}
					if (pguidType != Guid.Empty)
					{
						iDSRefProvider.SetExtendedType(intPtr, ref pguidType);
					}
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
						if (obj is DictionaryEntry dictionaryEntry2 && dictionaryEntry2.Value is object[] array3 && array3.Length != 0)
						{
							obj3 = array3[0];
							if (obj3 is string text4 && text4.StartsWith("{", StringComparison.Ordinal))
							{
								obj3 = string.Format(CultureInfo.CurrentCulture, text4, identifier);
							}
						}
						iDSRefProvider.SetProperty(intPtr, ref empty, obj3);
					}
				}
			}
			if (parameters.Length <= 5 || parameters[5] is not DictionaryEntry param || param.Value is not object[] array4)
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

	private static IntPtr CreateOrMatchNode(object dsRef, string name, string owner, __DSREFTYPE type, IntPtr parentNode, out bool isNew)
	{
		try
		{
			IDSRefConsumer iDSRefConsumer = dsRef as IDSRefConsumer;
			IntPtr intPtr = iDSRefConsumer.GetFirstChildNode(parentNode);
			IntPtr intPtr2 = VS.DSREFNODEID_NIL;
			while (intPtr != VS.DSREFNODEID_NIL && !NodeEquals(iDSRefConsumer, intPtr, name, owner, type))
			{
				intPtr2 = intPtr;
				intPtr = iDSRefConsumer.GetNextSiblingNode(intPtr);
			}
			if (intPtr == VS.DSREFNODEID_NIL)
			{
				IDSRefProvider iDSRefProvider = dsRef as IDSRefProvider;
				intPtr = ((!(intPtr2 == VS.DSREFNODEID_NIL)) ? iDSRefProvider.CreateNextSiblingNode(intPtr2) : iDSRefProvider.CreateFirstChildNode(parentNode));
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

	private static bool NodeEquals(IDSRefConsumer dsRefConsumer, IntPtr node, string name, string owner, __DSREFTYPE type)
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
			if (type2 != type)
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
