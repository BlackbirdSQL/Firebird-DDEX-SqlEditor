//
// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)
//

using System.Globalization;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using BlackbirdSql.Common;

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
		Diag.Trace();
	}


	public TObjectSupport(string resourceName, Assembly assembly) : base(resourceName, assembly)
	{
		Diag.Trace();
	}


	public TObjectSupport(IVsDataConnection connection) : base(typeof(TObjectSupport).FullName, typeof(TObjectSupport).Assembly)
	{
		Diag.Trace();
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Method implementations - TObjectSupport
	// ---------------------------------------------------------------------------------


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Opens a stream of bytes representing the XML content.
	/// </summary>
	/// <returns>
	/// Returns a System.IO.Stream object.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public override Stream OpenSupportStream()
	{
		Diag.Trace();
		return base.OpenSupportStream();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Opens a stream of bytes representing the XML content for a specified culture.
	/// </summary>
	/// <param name="culture">
	/// The geographical culture (as System.Globalization.CultureInfo object) for which
	/// to retrieve the Stream object instance.</param>
	/// <returns>
	/// Returns a System.IO.Stream object for the specified culture.
	/// </returns>
	// ---------------------------------------------------------------------------------
	protected override Stream OpenSupportStream(CultureInfo culture)
	{
		Diag.Trace();
		return base.OpenSupportStream(culture);
	}


	#endregion Method implementations

}
