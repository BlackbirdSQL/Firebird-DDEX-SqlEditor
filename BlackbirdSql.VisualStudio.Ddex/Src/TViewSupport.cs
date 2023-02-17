//
// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)
//

using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using BlackbirdSql.Common;



namespace BlackbirdSql.VisualStudio.Ddex;


// =========================================================================================================
//										TViewSupport Class
//
/// <summary>
/// Implementation of <see cref="IVsDataViewSupport"/> and <see cref="IVsDataSupportImportResolver"/> interfaces
/// </summary>
// =========================================================================================================
internal class TViewSupport : DataViewSupport, IVsDataSupportImportResolver
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
		// Diag.Trace();
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

		Diag.Dug(true, serviceType.FullName + " is not directly supported");

		return base.CreateService(serviceType);
	}



	/// <summary>
	/// Imports and returns a stream of data support XML that is identified with a specified
	/// pseudo name.
	/// </summary>
	/// <param name="name">The pseudo name of a stream to import.</param>
	/// <returns>
	/// An open stream containing the data support XML to be imported, or null if there
	/// is no stream found with this pseudo name.
	/// </returns>
	public Stream ImportSupportStream(string name)
	{
		if (name == null)
		{
			ArgumentNullException ex = new("name");
			Diag.Dug(ex);
			throw ex;
		}

		if (!name.EndsWith("Definitions"))
			return null;

		Type type = GetType();
		string resource = name[..^11] + "s.xml";

		Diag.Trace(type.Namespace + "." + resource);
		return type.Assembly.GetManifestResourceStream(type.FullName + resource);
	}


	#endregion Method Implementations

}
