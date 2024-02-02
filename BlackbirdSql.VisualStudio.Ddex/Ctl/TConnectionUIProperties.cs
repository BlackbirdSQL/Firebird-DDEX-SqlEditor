// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.ComponentModel;
using System.Data.Common;
using System.Reflection;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Model.Enums;
using Microsoft.VisualStudio.Data.Services.SupportEntities;


namespace BlackbirdSql.VisualStudio.Ddex.Ctl;

// =========================================================================================================
//										TConnectionUIProperties Class
//
/// <summary>
/// Implementation of <see cref="IVsDataConnectionUIProperties"/> interface
/// </summary>
// =========================================================================================================
public class TConnectionUIProperties : TConnectionProperties
{

	public TConnectionUIProperties() : base()
	{
		// Tracer.Trace(GetType(), "TConnectionUIProperties()");
	}

	// private static int _StaticCardinal = -1;
	// private int _InstanceCardinal = -1;


	/// <summary>
	/// Overloads <see cref="ICustomTypeDescriptor.GetProperties"/> to change readonly on
	/// dataset key properties for application connection sources.
	/// </summary>
	protected override PropertyDescriptorCollection GetCsbAttributesProperties(Attribute[] attributes)
	{
		PropertyDescriptorCollection descriptors = TypeDescriptor.GetProperties(ConnectionStringBuilder, attributes);

		UpdatePropertiesReadOnlyAttribute(ref descriptors);

		return descriptors;

	}


	/// <summary>
	/// Overloads <see cref="ICustomTypeDescriptor.GetProperties"/> to change readonly on
	/// dataset key properties for application connection sources.
	/// </summary>
	protected override PropertyDescriptorCollection GetCsbProperties(DbConnectionStringBuilder csb)
	{
		PropertyDescriptorCollection descriptors = TypeDescriptor.GetProperties(csb);

		UpdatePropertiesReadOnlyAttribute(ref descriptors);

		return descriptors;
	}

	public override void Reset()
	{
		// Tracer.Trace(GetType(), "Reset()");

		base.Reset();
	}



	public override void Parse(string connectionString)
	{
		lock (_LockObject)
		{
			ConnectionStringBuilder.ConnectionString = connectionString;

			if (ConnectionSource == EnConnectionSource.EntityDataModel)
			{
				ConnectionStringBuilder.Remove("edmx");
				ConnectionStringBuilder["edmu"] = true;
			}

			// Tracer.Trace(GetType(), "Parse()", "ConnectionSource: {0}, connectionString: {1}", ConnectionSource, ConnectionStringBuilder.ConnectionString);
		}

		OnPropertyChanged(new PropertyChangedEventArgs(string.Empty));
	}



	/// <summary>
	/// Updates descriptor collection readonly on dataset key properties for application
	/// connection sources.
	/// </summary>
	private void UpdatePropertiesReadOnlyAttribute(ref PropertyDescriptorCollection descriptors)
	{
		if (descriptors == null || descriptors.Count == 0)
			return;

		bool readOnly = ConnectionSource == EnConnectionSource.Application;

		try
		{
			FieldInfo fieldInfo;

			PropertyDescriptor descriptor = descriptors[CoreConstants.C_KeyExConnectionName];

			if (descriptor != null && descriptor.Attributes[typeof(ReadOnlyAttribute)] is ReadOnlyAttribute attr)
			{
				fieldInfo = Reflect.GetFieldInfo(attr, "isReadOnly", BindingFlags.NonPublic | BindingFlags.Instance);

				if ((bool)Reflect.GetFieldInfoValue(attr, fieldInfo) != readOnly)
				{
					Reflect.SetFieldInfoValue(attr, fieldInfo, readOnly);
				}
			}

			descriptor = descriptors[CoreConstants.C_KeyExDatasetId];

			if (descriptor != null && descriptor.Attributes[typeof(ReadOnlyAttribute)] is ReadOnlyAttribute attr2)
			{
				fieldInfo = Reflect.GetFieldInfo(attr2, "isReadOnly", BindingFlags.NonPublic | BindingFlags.Instance);

				if ((bool)Reflect.GetFieldInfoValue(attr2, fieldInfo) != readOnly)
				{
					Reflect.SetFieldInfoValue(attr2, fieldInfo, readOnly);
				}
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}

	}



	protected override void OnSiteChanged(EventArgs e)
	{
		// Tracer.Trace(GetType(), "OnSiteChanged()", "Site type or null: {0}", Site != null ? Site.GetType().FullName : "Null");

		base.OnSiteChanged(e);
	}

}
