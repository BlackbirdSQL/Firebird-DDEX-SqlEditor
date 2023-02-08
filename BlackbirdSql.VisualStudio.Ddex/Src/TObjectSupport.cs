//
// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)
//

using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using BlackbirdSql.Common;
using System.Reflection;



namespace BlackbirdSql.VisualStudio.Ddex;


// =========================================================================================================
//										TObjectSupport Class
//
/// <summary>
/// Implementation of <see cref="IVsDataObjectSupport"/> interface
/// </summary>
// =========================================================================================================
internal class TObjectSupport : DataObjectSupport
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - TObjectSupport
	// ---------------------------------------------------------------------------------


	public TObjectSupport(string fileName, string path) : base(fileName, path)
	{
		// Diag.Trace();
	}


	public TObjectSupport(string resourceName, Assembly assembly) : base(resourceName, assembly)
	{
		// Diag.Trace();
	}


	public TObjectSupport(IVsDataConnection connection) : base(typeof(TObjectSupport).FullName, typeof(TObjectSupport).Assembly)
	{
		// Diag.Trace();
	}


	#endregion Constructors / Destructors

}
