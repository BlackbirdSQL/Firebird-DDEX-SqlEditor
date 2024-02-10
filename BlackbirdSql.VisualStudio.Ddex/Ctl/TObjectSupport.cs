// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.Globalization;
using System.IO;
using System.Reflection;

using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Extensions;
using BlackbirdSql.VisualStudio.Ddex.Properties;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;




namespace BlackbirdSql.VisualStudio.Ddex.Ctl;


// =========================================================================================================
//										TObjectSupport Class
//
/// <summary>
/// Implementation of <see cref="IVsDataObjectSupport"/> and <see cref="IVsDataSupportImportResolver"/>
/// interfaces.
/// </summary>
// =========================================================================================================
public class TObjectSupport : DataObjectSupport, IVsDataSupportImportResolver
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - TObjectSupport
	// ---------------------------------------------------------------------------------


	public TObjectSupport(string fileName, string path) : base(fileName, path)
	{
		// Tracer.Trace(GetType(), "TObjectSupport.TObjectSupport()", "fileName: {0}, path: {1}", fileName, path);
	}


	public TObjectSupport(string resourceName, Assembly assembly) : base(resourceName, assembly)
	{
		// Tracer.Trace(GetType(), "TObjectSupport.TObjectSupport()", "resourceName: {0}", resourceName);
	}


	public TObjectSupport(IVsDataConnection connection) : base(typeof(TObjectSupport).FullName, typeof(TObjectSupport).Assembly)
	{
		// Tracer.Trace(GetType(), "TObjectSupport.TObjectSupport(IVsDataConnection)");
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
	/// <remarks>
	/// According to xsd 
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public override Stream OpenSupportStream() => OpenSupportStream(CultureInfo.InvariantCulture);



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Opens a stream of bytes representing the XML content TObjectSupport for a
	/// specified culture.
	/// </summary>
	/// <param name="culture">
	/// The geographical culture (as System.Globalization.CultureInfo object) for which
	/// to retrieve the Stream object instance.</param>
	/// <returns>
	/// Returns the extrapolated stream for TObjectSupport.xml.
	/// </returns>
	/// <remarks>
	/// For whatever reason Microsoft.VisualStudio.Data.Package.DataObjectSupportBuilder
	/// is failing to utilize our implementation of <see cref="IVsDataSupportImportResolver"/>,
	/// even though decompiling shows it is utilized in the builder and that
	/// <see cref="ImportSupportStream"/> is called, yet it never is.
	/// Works fine in <see cref="TViewSupport"/>.
	/// Microsoft.VisualStudio.Data.Package.DataViewSupportBuilder uses the exact same code in
	/// the ancestor Microsoft.VisualStudio.Data.Package.DataSupportBuilder and it works for
	/// <see cref="TViewSupport"/>, so yeah... dunno.
	/// It's a glitch in DataObjectSupportBuilder.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	protected override Stream OpenSupportStream(CultureInfo culture)
	{
		Stream stream = base.OpenSupportStream(culture);

		return XmlParser.ExtrapolateXmlImports(GetType().Name, stream, this);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Imports and returns a stream of data support XML that is identified with a specified
	/// pseudo name.
	/// </summary>
	/// <param name="name">The pseudo name of a stream to import.</param>
	/// <returns>
	/// An open stream containing the data support XML to be imported, or null if there
	/// is no stream found with this pseudo name.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public Stream ImportSupportStream(string name)
	{
		if (name == null)
		{
			ArgumentNullException ex = new("name");
			Diag.Dug(ex);
			throw ex;
		}

		if (!name.EndsWith("Definitions"))
		{
			Diag.StackException(Resources.ExceptionImportResourceNotFound.FmtRes(name));
			return null;
		}


		Type type = GetType();
		string resource = type.FullName + name[..^11] + ".xml";

		// Tracer.Trace("Importing resource: " + resource);


		return type.Assembly.GetManifestResourceStream(resource);
	}


	#endregion Method implementations

}
