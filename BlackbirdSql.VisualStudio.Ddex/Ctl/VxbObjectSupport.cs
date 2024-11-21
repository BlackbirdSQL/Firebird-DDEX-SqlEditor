// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using BlackbirdSql.Core.Extensions;
using BlackbirdSql.VisualStudio.Ddex.Ctl.Config;
using BlackbirdSql.VisualStudio.Ddex.Properties;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;



namespace BlackbirdSql.VisualStudio.Ddex.Ctl;


// =========================================================================================================
//										VxbObjectSupport Class
//
/// <summary>
/// Implementation of <see cref="IVsDataObjectSupport"/> and <see cref="IVsDataSupportImportResolver"/>
/// interfaces.
/// </summary>
// =========================================================================================================
public class VxbObjectSupport : DataObjectSupport, IVsDataSupportImportResolver
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - VxbObjectSupport
	// ---------------------------------------------------------------------------------


	public VxbObjectSupport(string fileName, string path) : base(fileName, path)
	{
		// Evs.Trace(typeof(VxbObjectSupport), ".ctor(string, string)", "fileName: {0}, path: {1}", fileName, path);
	}


	public VxbObjectSupport(string resourceName, Assembly assembly) : base(resourceName, assembly)
	{
		// Evs.Trace(typeof(VxbObjectSupport), ".ctor(string, Assenbly)", "resourceName: {0}", resourceName);
	}


	public VxbObjectSupport(IVsDataConnection connection) : base(typeof(VxbObjectSupport).FullName, typeof(VxbObjectSupport).Assembly)
	{
		// Evs.Trace(typeof(VxbObjectSupport), ".ctor(IVsDataConnection)");
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Method implementations - VxbObjectSupport
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
	/// Opens a stream of bytes representing the XML content VxbObjectSupport for a
	/// specified culture.
	/// </summary>
	/// <param name="culture">
	/// The geographical culture (as System.Globalization.CultureInfo object) for which
	/// to retrieve the Stream object instance.</param>
	/// <returns>
	/// Returns the extrapolated stream for VxbObjectSupport.xml.
	/// </returns>
	/// <remarks>
	/// For whatever reason Microsoft.VisualStudio.Data.Package.DataObjectSupportBuilder
	/// is failing to utilize our implementation of <see cref="IVsDataSupportImportResolver"/>,
	/// even though decompiling shows it is utilized in the builder and that
	/// <see cref="ImportSupportStream"/> is called, yet it never is.
	/// Works fine in <see cref="VxbViewSupport"/>.
	/// Microsoft.VisualStudio.Data.Package.DataViewSupportBuilder uses the exact same code in
	/// the ancestor Microsoft.VisualStudio.Data.Package.DataSupportBuilder and it works for
	/// <see cref="VxbViewSupport"/>, so yeah... dunno.
	/// It's a glitch in DataObjectSupportBuilder.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	protected override Stream OpenSupportStream(CultureInfo culture)
	{
		Stream stream = base.OpenSupportStream(culture);

		return ExtrapolateXmlImports(GetType().Name, stream, this);
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
			ArgumentNullException ex = new(nameof(name));
			Diag.Ex(ex);
			throw ex;
		}

		if (!name.EndsWith("Definitions"))
		{
			Diag.StackException(Resources.ExceptionImportResourceNotFound.Fmt(name));
			return null;
		}


		Type type = GetType();
		string resource = type.FullName + name[..^11] + ".xml";

		// Evs.Trace("Importing resource: " + resource);


		return type.Assembly.GetManifestResourceStream(resource);
	}


	#endregion Method implementations





	// =========================================================================================================
	#region Methods - VxbObjectSupport
	// ---------------------------------------------------------------------------------


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Single-level extrapolation of an xml stream with imports into a single stream
	/// because ObjectSupport imports are not working in VS.
	/// If <see cref="PersistentSettings.EnableSaveExtrapolatedXml"/> is set, writes a
	/// copy of the extrapolation to <paramref name="xmlName"/>.Extapolated.xml in the
	/// <see cref="PersistentSettings.LogFile"/> folder.
	/// </summary>
	/// <returns>
	/// Returns a System.IO.Stream object of the extrapolated xml.
	/// </returns>
	// ---------------------------------------------------------------------------------
	private static Stream ExtrapolateXmlImports(string xmlName, Stream stream, IVsDataSupportImportResolver resolver)
	{
		/*
		 * DataSupportBuilder recursively enumerates imported nodes and simply inserts them into it's reference dict.
		 * We're going to have to insert them into the parent xml and then stream the extrapolated xml
		 * back to Microsoft.VisualStudio.Data.Package.Connection.
		*/

		bool updated = false;

		XmlDocument xmlDoc = new XmlDocument();
		XmlDocument xmlImportDoc;

		XmlNode xmlRoot, xmlImportRoot, xmlPrev, xmlNode, xmlImportNode;

		XmlNamespaceManager xmlNs;
		XmlNamespaceManager xmlImportNs;

		XmlNodeList xmlImports, xmlDefinitions;

		Stream importStream;


		try
		{
			xmlDoc.Load(stream);
			xmlNs = new XmlNamespaceManager(xmlDoc.NameTable);

			xmlRoot = xmlDoc.DocumentElement;

			if (!xmlNs.HasNamespace("confBlackbirdNs"))
				xmlNs.AddNamespace("confBlackbirdNs", xmlRoot.NamespaceURI);


			xmlImports = xmlRoot.SelectNodes("//confBlackbirdNs:Import", xmlNs);


			foreach (XmlNode xmlImport in xmlImports)
			{
				string name = xmlImport.Attributes["name"].Value;

				importStream = resolver.ImportSupportStream(name);

				if (importStream == null)
					continue;

				xmlImportDoc = new XmlDocument();
				xmlImportDoc.Load(importStream);
				xmlImportNs = new XmlNamespaceManager(xmlImportDoc.NameTable);
				xmlImportRoot = xmlImportDoc.DocumentElement;

				if (!xmlImportNs.HasNamespace("confBlackbirdNs"))
					xmlImportNs.AddNamespace("confBlackbirdNs", xmlImportRoot.NamespaceURI);

				xmlDefinitions = xmlImportRoot.SelectNodes("//confBlackbirdNs:Define", xmlImportNs);

				xmlPrev = xmlImport;

				foreach (XmlNode xmlDefinition in xmlDefinitions)
				{
					xmlNode = xmlDoc.ImportNode(xmlDefinition, true);
					xmlPrev = xmlRoot.InsertAfter(xmlNode, xmlPrev);
				}

				xmlImportNode = xmlImportRoot.SelectSingleNode("//confBlackbirdNs:Types", xmlImportNs);

				if (xmlImportNode != null)
				{
					xmlNode = xmlDoc.ImportNode(xmlImportNode, true);
					xmlPrev = xmlRoot.InsertAfter(xmlNode, xmlPrev);
				}

				xmlImportNode = xmlImportRoot.SelectSingleNode("//confBlackbirdNs:MappedTypes", xmlImportNs);

				if (xmlImportNode != null)
				{
					xmlNode = xmlDoc.ImportNode(xmlImportNode, true);
					xmlPrev = xmlRoot.InsertAfter(xmlNode, xmlPrev);
				}


				xmlRoot.RemoveChild(xmlImport);

				updated = true;
			}

		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
		}


		if (updated)
		{
			MemoryStream memStream = new();

			xmlDoc.Save(memStream);
			memStream.Position = 0;

			byte[] buffer = new byte[memStream.Length];
			memStream.Read(buffer, 0, (int)memStream.Length);
			memStream.Dispose();

			string str = Encoding.Default.GetString(buffer).Replace("&gt;", ">");
			buffer = Encoding.Default.GetBytes(str);

			if (PersistentSettings.EnableSaveExtrapolatedXml)
			{
				FileInfo info = new FileInfo(PersistentSettings.LogFile);
				FileStream fileStream = new FileStream(info.DirectoryName + "/" + xmlName + ".Extrapolated.xml",
					FileMode.Create, FileAccess.Write);

				fileStream.Write(buffer, 0, buffer.Length);
				fileStream.Flush();
				fileStream.Close();
				fileStream.Dispose();
			}

			memStream = new(buffer)
			{
				Position = 0
			};

			return memStream;
		}

		stream.Position = 0;
		return stream;

	}


	#endregion Methods

}
