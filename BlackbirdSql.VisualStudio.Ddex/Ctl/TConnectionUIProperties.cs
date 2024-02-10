// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.ComponentModel;
using System.Data.Common;
using System.Reflection;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Model;
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
	protected override PropertyDescriptorCollection GetCsbProperties(DbConnectionStringBuilder csb, Attribute[] attributes)
	{
		PropertyDescriptorCollection descriptors = TypeDescriptor.GetProperties(csb, attributes);

		CsbAgent.UpdateDatasetKeysReadOnlyAttribute(ref descriptors, ConnectionSource == EnConnectionSource.Application);

		return descriptors;

	}


	/// <summary>
	/// Overloads <see cref="ICustomTypeDescriptor.GetProperties"/> to change readonly on
	/// dataset key properties for application connection sources.
	/// </summary>
	protected override PropertyDescriptorCollection GetCsbProperties(DbConnectionStringBuilder csb)
	{
		PropertyDescriptorCollection descriptors = TypeDescriptor.GetProperties(csb);

		CsbAgent.UpdateDatasetKeysReadOnlyAttribute(ref descriptors, ConnectionSource == EnConnectionSource.Application);

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

			// Tracer.Trace(GetType(), "Parse()", "ConnectionSource: {0}", ConnectionSource);
		}

		OnPropertyChanged(new PropertyChangedEventArgs(string.Empty));
	}



	protected override void OnSiteChanged(EventArgs e)
	{
		// Tracer.Trace(GetType(), "OnSiteChanged()", "Site type or null: {0}", Site != null ? Site.GetType().FullName : "Null");

		base.OnSiteChanged(e);
	}

}
