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
//										TConnectionProperties Class
//
/// <summary>
/// Implementation of <see cref="IVsDataConnectionProperties"/> interface.
/// </summary>
// =========================================================================================================
public class TConnectionProperties : TAbstractConnectionProperties
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - TConnectionProperties
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// TConnectionProperties .ctor
	/// </summary>
	public TConnectionProperties() : base()
	{
		// Tracer.Trace(typeof(TConnectionProperties), ".ctor");
	}


	#endregion Constructors / Destructors





	// =================================================================================
	#region Fields - TConnectionProperties
	// =================================================================================


	#endregion Fields





	// =================================================================================
	#region Property Accessors - TConnectionProperties
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
	#region Methods - TConnectionProperties
	// =================================================================================

	protected override PropertyDescriptorCollection GetCsbProperties(DbConnectionStringBuilder csb, Attribute[] attributes)
	{
		return (PropertyDescriptorCollection)Diag.ThrowException(new NotImplementedException("TConnectionProperties.GetCsbProperties(DbConnectionStringBuilder, Attribute[])"));
	}

	protected override PropertyDescriptorCollection GetCsbProperties(DbConnectionStringBuilder csb)
	{
		return (PropertyDescriptorCollection)Diag.ThrowException(new NotImplementedException("TConnectionProperties.GetCsbProperties(DbConnectionStringBuilder)"));
	}



	#endregion Methods

}
