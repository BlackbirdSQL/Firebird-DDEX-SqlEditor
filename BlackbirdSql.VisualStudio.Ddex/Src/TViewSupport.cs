//
// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)
//

using System;
using System.Reflection;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using BlackbirdSql.Common;



namespace BlackbirdSql.VisualStudio.Ddex;


// =========================================================================================================
//										TViewSupport Class
//
/// <summary>
/// Implementation of <see cref="IVsDataViewSupport"/> interface
/// </summary>
// =========================================================================================================
internal class TViewSupport : DataViewSupport
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors
	// ---------------------------------------------------------------------------------


	public TViewSupport(string fileName, string path) : base(fileName, path)
	{
		// Diag.Trace();
	}
	public TViewSupport(string resourceName, Assembly assembly) : base(resourceName, assembly)
	{
		// Diag.Trace();
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Method Implementations - TViewSupport
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates a service for the specified type.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override object CreateService(Type serviceType)
	{
		// TBC
		/*
		if (serviceType == typeof(IVsDataViewCommandProvider))
		{
			return new ViewDatabaseCommandProvider();
		}

		if (serviceType == typeof(IVsDataViewDocumentProvider))
		{
			return new ViewDocumentProvider();
		}
		*/
		return base.CreateService(serviceType);
	}


	#endregion Method Implementations

}
