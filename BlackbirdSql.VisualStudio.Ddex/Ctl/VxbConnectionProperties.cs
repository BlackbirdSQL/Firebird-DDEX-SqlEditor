// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Sys.Ctl;
using Microsoft.VisualStudio.Data.Services.SupportEntities;



namespace BlackbirdSql.VisualStudio.Ddex.Ctl;


// =========================================================================================================
//										VxbConnectionProperties Class
//
/// <summary>
/// Implementation of <see cref="IVsDataConnectionProperties"/> interface.
/// </summary>
// =========================================================================================================
public class VxbConnectionProperties : AbstractConnectionProperties
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - VxbConnectionProperties
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// VxbConnectionProperties .ctor
	/// </summary>
	public VxbConnectionProperties() : base()
	{
		// Evs.Trace(typeof(VxbConnectionProperties), ".ctor");
	}


	#endregion Constructors / Destructors





	// =================================================================================
	#region Fields - VxbConnectionProperties
	// =================================================================================


	#endregion Fields





	// =================================================================================
	#region Property Accessors - VxbConnectionProperties
	// =================================================================================


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
				// IEnumerable<Describer> describers = (ConnectionSource == EnConnectionSource.Application)
				//	? Csb.PublicMandatoryKeys : Csb.MandatoryKeys;

				IEnumerable<Describer> describers = Csb.MandatoryKeys;

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
				Diag.Dug(ex);
				throw;
			}

			return true;
		}
	}


	#endregion Property Accessors




	// =================================================================================
	#region Methods - VxbConnectionProperties
	// =================================================================================

	protected override PropertyDescriptorCollection GetCsbProperties(Csb csb, Attribute[] attributes)
	{
		return (PropertyDescriptorCollection)Diag.ThrowException(new NotImplementedException("VxbConnectionProperties.GetCsbProperties(DbConnectionStringBuilder, Attribute[])"));
	}

	protected override PropertyDescriptorCollection GetCsbProperties(Csb csb)
	{
		return (PropertyDescriptorCollection)Diag.ThrowException(new NotImplementedException("VxbConnectionProperties.GetCsbProperties(DbConnectionStringBuilder)"));
	}


	protected override void OnSiteChanged(EventArgs e)
	{
		Evs.Trace(GetType(), nameof(OnSiteChanged));

		base.OnSiteChanged(e);

		if (Site != null)
			NativeDb.ReindexVersioningFacade(ApcManager.ActiveProject);
	}


	#endregion Methods

}
