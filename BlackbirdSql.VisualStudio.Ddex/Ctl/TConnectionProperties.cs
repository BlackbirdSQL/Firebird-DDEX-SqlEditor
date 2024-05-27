// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Sys;
using Microsoft.VisualStudio.Data.Services.SupportEntities;


namespace BlackbirdSql.VisualStudio.Ddex.Ctl;

// =========================================================================================================
//										TConnectionProperties Class
//
/// <summary>
/// Implementation of <see cref="IVsDataConnectionProperties"/> interface.
/// </summary>
// =========================================================================================================
public class TConnectionProperties : TAbstractConnectionProperties, IBDataConnectionProperties
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - TConnectionProperties
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// TConnectionProperties .ctor
	/// </summary>
	public TConnectionProperties() : base()
	{
		// Tracer.Trace(GetType(), "TConnectionProperties.TConnectionProperties()");
	}


	#endregion Constructors / Destructors





	// =================================================================================
	#region Fields - TConnectionProperties
	// =================================================================================


	#endregion Fields





	// =================================================================================
	#region Property Accessors - TConnectionProperties
	// =================================================================================


	public Csb Csa => ConnectionStringBuilder;


	/// <summary>
	/// Determines if the connection properties object is sufficiently complete (inclusive of password)
	/// to establish a database connection
	/// </summary>
	public override bool IsComplete
	{
		get
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


			return true;
		}
	}


	#endregion Property Accessors




	// =================================================================================
	#region Methods - TConnectionProperties
	// =================================================================================

	protected override PropertyDescriptorCollection GetCsbProperties(DbConnectionStringBuilder csb, Attribute[] attributes)
	{
		throw new NotImplementedException("TConnectionProperties.GetCsbProperties(DbConnectionStringBuilder, Attribute[])");
	}

	protected override PropertyDescriptorCollection GetCsbProperties(DbConnectionStringBuilder csb)
	{
		throw new NotImplementedException("TConnectionProperties.GetCsbProperties(DbConnectionStringBuilder)");
	}



	#endregion Methods

}
