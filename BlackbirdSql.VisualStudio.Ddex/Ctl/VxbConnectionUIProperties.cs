// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Sys.Ctl;
using BlackbirdSql.Sys.Enums;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using static BlackbirdSql.SysConstants;



namespace BlackbirdSql.VisualStudio.Ddex.Ctl;


// =========================================================================================================
//										VxbConnectionUIProperties Class
//
/// <summary>
/// Implementation of <see cref="IVsDataConnectionUIProperties"/> interface
/// </summary>
// =========================================================================================================
public class VxbConnectionUIProperties : VxbConnectionProperties
{

	public VxbConnectionUIProperties() : base()
	{
		// Evs.Trace(typeof(VxbConnectionUIProperties), ".ctor");
	}

	// private static int _StaticCardinal = -1;
	// private int _InstanceCardinal = -1;


	/// <summary>
	/// Determines if the connection properties object is sufficiently complete,
	/// inclusive of password for connections other than Properties settings
	/// connection strings, in order to establish a database connection.
	/// </summary>
	public override bool IsComplete
	{
		get
		{
			try
			{
				IEnumerable<Describer> describers = (ConnectionSource == EnConnectionSource.Application)
					? Csb.PublicMandatoryKeys : Csb.MandatoryKeys;

				foreach (Describer describer in describers)
				{
					if (!base.TryGetValue(describer.Key, out object value) || string.IsNullOrEmpty((string)value))
					{
						return false;
					}
				}
			}
			catch (Exception ex)
			{
				Diag.Ex(ex);
				throw;
			}

			return true;
		}
	}



	/// <summary>
	/// Overloads <see cref="ICustomTypeDescriptor.GetProperties"/> to change readonly on
	/// dataset key properties for application connection sources.
	/// </summary>
	protected override PropertyDescriptorCollection GetCsbProperties(Csb csb, Attribute[] attributes)
	{
		try
		{
			PropertyDescriptorCollection descriptors = TypeDescriptor.GetProperties(csb, attributes);

			// Csb.UpdateDescriptorReadOnlyAttribute(ref descriptors, [C_KeyExConnectionName, C_KeyExDatasetName],
			//	ConnectionSource == EnConnectionSource.Application);

			return descriptors;
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			throw;
		}
	}



	/// <summary>
	/// Overloads <see cref="ICustomTypeDescriptor.GetProperties"/> to change readonly on
	/// dataset key properties for application connection sources.
	/// </summary>
	protected override PropertyDescriptorCollection GetCsbProperties(Csb csb)
	{
		try
		{
			PropertyDescriptorCollection descriptors = TypeDescriptor.GetProperties(csb);

			// Csb.UpdateDescriptorReadOnlyAttribute(ref descriptors, [C_KeyExConnectionName, C_KeyExDatasetName],
			//	ConnectionSource == EnConnectionSource.Application);

			return descriptors;
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			throw;
		}
	}



	public override void Reset()
	{
		// Evs.Trace(GetType(), nameof(Reset));

		base.Reset();
	}



	public override void Parse(string connectionString)
	{
		try
		{
			Evs.Trace(GetType(), nameof(Parse));

			lock (_LockObject)
			{
				ConnectionStringBuilder.ConnectionString = connectionString;

				// Connection dialogs spawned from a UIHierarchyMarshaler can misbehave and
				// corrupt our connection nodes, which we repair. So the
				// IVsDataConnectionUIProperties implementation of this class is identified
				// with an "edmu" property.
				// The ancestor IVsDataConnectionProperties implementation is identified
				// with an "edmx" property.
				if (ConnectionSource == EnConnectionSource.EntityDataModel || ConnectionSource == EnConnectionSource.DataSource)
				{
					ConnectionStringBuilder.Remove(CoreConstants.C_KeyExEdmx);
					ConnectionStringBuilder[CoreConstants.C_KeyExEdmu] = true;

					// Evs.Debug(GetType(), nameof(Parse), "Calling ReindexEntityFrameworkAssemblies.");

					NativeDb.ReindexEntityFrameworkAssembliesAsyui(ApcManager.ActiveProject);
				}


				// Evs.Debug(GetType(), nameof(Parse), "ConnectionSource: {0}", ConnectionSource);
			}

			OnPropertyChanged(new PropertyChangedEventArgs(""));
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			throw;
		}
	}



	protected override void OnSiteChanged(EventArgs e)
	{
		// Evs.Trace(GetType(), nameof(OnSiteChanged), "Site type or null: {0}", Site != null ? Site.GetType().FullName : "Null");

		base.OnSiteChanged(e);
	}

}
