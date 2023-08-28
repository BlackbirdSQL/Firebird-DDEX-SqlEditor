// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Xml;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.LanguageServer.Client;

namespace BlackbirdSql.Core.Extensions;


// =========================================================================================================
//											XmlParser Class
//
/// <summary>
/// Xml parser utility methods
/// </summary>
// =========================================================================================================
internal static class XmlParser
{
	#region Variables


	static DataTable _DataSources = null, _Databases = null;


	#endregion Variables





	// =========================================================================================================
	#region Methods - XmlParser
	// =========================================================================================================




	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Single-level extrapolation of an xml stream with imports into a single stream.
	/// Also, in DEBUG and if <see cref="Diag.EnableDiagnosticsLog"/> is set, writes a copy of the
	/// extrapolation to <paramref name="xmlName"/>.Extapolated.xml in the
	/// <see cref="Diag.LogFile"/> folder.
	/// </summary>
	/// <returns>
	/// Returns a System.IO.Stream object of the extrapolated xml.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static Stream ExtrapolateXmlImports(string xmlName, Stream stream, IVsDataSupportImportResolver resolver)
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
			Diag.Dug(ex);
		}


		if (updated)
		{
#if DEBUG
			if (Diag.EnableDiagnosticsLog)
			{
				FileInfo info = new FileInfo(Diag.LogFile);
				xmlDoc.Save(info.DirectoryName + "/" + xmlName + ".Extrapolated.xml");
			}
#endif
			MemoryStream xmlStream = new MemoryStream();

			xmlDoc.Save(xmlStream);
			xmlStream.Position = 0;
			return xmlStream;
		}

		stream.Position = 0;
		return stream;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Populates a <see cref="DataTable"/> with the distinct Server hostnames (DataSources) of all registered
	/// servers of the current data provider (in this case FlameRobin for Firebird), using the xml located
	/// at <see cref="SystemData.ConfiguredConnectionsPath"/>.
	/// </summary>
	/// <returns>
	/// The populated <see cref="DataTable"/> that can be used together with <see cref="Databases"/> in an <see cref="ErmBindingSource"/>.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static DataTable DataSources
	{
		get
		{
			if (_DataSources != null)
				return _DataSources;

			_DataSources = Databases.DefaultView.ToTable(true, "Orderer", "DatasetName", "DataSource", "DataSourceLc", "PortNumber");


			return _DataSources;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Populates a <see cref="DataTable"/> with all registered databases of the current data provider (in this case FlameRobin for Firebird)
	/// using the xml located at <see cref="SystemData.ConfiguredConnectionsPath"/>.
	/// </summary>
	/// <returns>
	/// The populated <see cref="DataTable"/> that can be used together with <see cref="DataSources"/> in an <see cref="ErmBindingSource"/>.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static DataTable Databases
	{
		get
		{
			if (_Databases != null)
				return _Databases;

			DataTable databases = new DataTable();


			databases.Columns.Add("Id", typeof(int));
			databases.Columns.Add("Orderer", typeof(int));
			databases.Columns.Add("DatasetName", typeof(string));
			databases.Columns.Add("DataSource", typeof(string));
			databases.Columns.Add("DataSourceLc", typeof(string));
			databases.Columns.Add("Name", typeof(string));
			databases.Columns.Add("InitialCatalog", typeof(string));
			databases.Columns.Add("InitialCatalogLc", typeof(string));
			databases.Columns.Add("DatasetKey", typeof(string));
			databases.Columns.Add("PortNumber", typeof(int));
			databases.Columns.Add("Charset", typeof(string));
			databases.Columns.Add("UserName", typeof(string));
			databases.Columns.Add("Password", typeof(string));
			databases.Columns.Add("RoleName", typeof(string));


			bool hasLocal = false;
			DataRow row;

			string xmlPath = SystemData.ConfiguredConnectionsPath;

			if (File.Exists(xmlPath))
			{
				try
				{
					XmlDocument xmlDoc = new XmlDocument();

					xmlDoc.Load(xmlPath);

					XmlNode xmlRoot = xmlDoc.DocumentElement;
					/* XmlNamespaceManager xmlNs = new XmlNamespaceManager(xmlDoc.NameTable);


					if (!xmlNs.HasNamespace("confBlackbirdNs"))
					{
						xmlNs.AddNamespace("confBlackbirdNs", xmlRoot.NamespaceURI);
					}
					*/
					XmlNodeList xmlServers, xmlDatabases;
					XmlNode xmlNode = null;
					uint port;
					string datasetName, datasource, authentication, path;

					xmlServers = xmlRoot.SelectNodes("//server");


					foreach (XmlNode xmlServer in xmlServers)
					{
						if ((xmlNode = xmlServer.SelectSingleNode("name")) == null)
							continue;
						datasetName = xmlNode.InnerText.Trim();


						if ((xmlNode = xmlServer.SelectSingleNode("host")) == null)
							continue;
						datasource = xmlNode.InnerText.Trim();


						if ((xmlNode = xmlServer.SelectSingleNode("port")) == null)
							continue;
						port = Convert.ToUInt32(xmlNode.InnerText.Trim());

						if (port == 0)
							port = 3050;

						if (datasource == "localhost")
							hasLocal = true;

						row = databases.NewRow();

						row["Id"] = databases.Rows.Count;
						row["DatasetName"] = datasetName;
						row["DataSource"] = datasource;
						row["DataSourceLc"] = datasource.ToLower();
						row["PortNumber"] = (int)port;

						row["Name"] = "";
						row["InitialCatalog"] = "";
						row["InitialCatalogLc"] = "";
						row["DatasetKey"] = "";
						row["Charset"] = "";
						row["UserName"] = "";
						row["Password"] = "";
						row["RoleName"] = "";

						if (datasource == "localhost")
							row["Orderer"] = 2;
						else
							row["Orderer"] = 3;

						databases.Rows.Add(row);


						xmlDatabases = xmlServer.SelectNodes("database");



						foreach (XmlNode xmlDatabase in xmlDatabases)
						{
							if ((xmlNode = xmlDatabase.SelectSingleNode("name")) == null)
							continue;


							// Add a ghost row to each database
							// A binding source cannot have an invalidated state. ie. Position == -1 and Current == null,
							// if it's List Count > 0. The ghost row is a placeholder for that state.
							row = databases.NewRow();

							row["Id"] = databases.Rows.Count;
							row["DatasetName"] = datasetName;
							row["DataSource"] = datasource;
							row["DataSourceLc"] = datasource.ToLower();
							row["PortNumber"] = (int)port;

							row["Name"] = xmlNode.InnerText.Trim();

							if ((xmlNode = xmlDatabase.SelectSingleNode("path")) == null)
							continue;

							path =  xmlNode.InnerText.Trim();
							row["InitialCatalog"] = path;
							row["InitialCatalogLc"] = path.ToLower();

							row["DatasetKey"] = $"{datasource} ({Path.GetFileNameWithoutExtension(path)})";

							if ((xmlNode = xmlDatabase.SelectSingleNode("charset")) == null)
							continue;
							row["Charset"] = xmlNode.InnerText.Trim();

							row["UserName"] = "";
							row["Password"] = "";
							row["RoleName"] = "";

							if ((xmlNode = xmlDatabase.SelectSingleNode("authentication")) == null)
								authentication = "trusted";
							else
								authentication = xmlNode.InnerText.Trim();

							if (authentication != "trusted")
							{
								if ((xmlNode = xmlDatabase.SelectSingleNode("username")) != null)
								{
									row["UserName"] = xmlNode.InnerText.Trim();

									if (authentication == "pwd"
										&& (xmlNode = xmlDatabase.SelectSingleNode("password")) != null)
									{
										row["Password"] = xmlNode.InnerText.Trim();
									}
								}

							}

							if (datasource == "localhost")
								row["Orderer"] = 2;
							else
								row["Orderer"] = 3;

							databases.Rows.Add(row);
						}
					}

				}
				catch (Exception ex)
				{
					Diag.Dug(ex);
				}


				// Add a ghost row to the datasources list
				// This will be the default datasource row so that anything else
				// selected will generate a CurrentChanged event.
				row = databases.NewRow();

				row["Id"] = databases.Rows.Count;
				row["DatasetName"] = "";
				row["DataSource"] = "";
				row["DataSourceLc"] = "";
				row["PortNumber"] = 0;

				row["Name"] = "";
				row["InitialCatalog"] = "";
				row["InitialCatalogLc"] = "";
				row["DatasetKey"] = "";
				row["Charset"] = "";
				row["UserName"] = "";
				row["Password"] = "";
				row["RoleName"] = "";

				row["Orderer"] = 0;

				databases.Rows.Add(row);


				// Add a Clear/Reset dummy row for the datasources list
				// If selected will invoke a form reset the move the cursor back to the ghost row.
				row = databases.NewRow();

				row["Id"] = databases.Rows.Count;
				row["Orderer"] = 1;
				row["DatasetName"] = "Reset";
				row["DataSource"] = "";
				row["DataSourceLc"] = "reset";
				row["Name"] = "";
				row["PortNumber"] = 0;
				row["InitialCatalog"] = "";
				row["InitialCatalogLc"] = "";
				row["DatasetKey"] = "";
				row["Charset"] = "";
				row["UserName"] = "";
				row["Password"] = "";
				row["RoleName"] = "";
				databases.Rows.Add(row);


				// Add at least one row, that will be the ghost row, for localhost. 
				if (!hasLocal)
				{
					row = databases.NewRow();

					row["Id"] = databases.Rows.Count;
					row["Orderer"] = 2;
					row["DatasetName"] = "Localhost";
					row["DataSource"] = "localhost";
					row["DataSourceLc"] = "localhost";
					row["Name"] = "";
					row["PortNumber"] = 3050;
					row["InitialCatalog"] = "";
					row["InitialCatalogLc"] = "";
					row["DatasetKey"] = "";
					row["Charset"] = "";
					row["UserName"] = "";
					row["Password"] = "";
					row["RoleName"] = "";

					databases.Rows.Add(row);
				}

				databases.DefaultView.Sort = "Orderer,DatasetName,Name ASC";


			}

			_Databases = databases.DefaultView.ToTable(false);

			return _Databases;
		}
	}

	public static DbConnectionStringBuilder GetCsbFromDatabases(string datasetKey)
	{
		foreach (DataRow row in Databases.Rows)
		{
			if ((string)row["Name"] == "")
				continue;

			if (datasetKey.Equals((string)row["DatasetKey"]))
			{
				DbConnectionStringBuilder csb = new()
				{
					["DataSource"] = (string)row["DataSource"],
					["Port"] = (int)row["PortNumber"],
					["Database"] = (string)row["InitialCatalog"],
					["Charset"] = (string)row["Charset"],
					["UserID"] = (string)row["UserName"],
					["Password"] = (string)row["Password"],
					["Role"] = (string)row["RoleName"],
					["DatasetKey"] = (string)row["DatasetKey"]
				};
				return csb;
			}
		}

		return null;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if a project has Firebird EntityFramework configured in the app.config and configures it if it doesn't
	/// </summary>
	/// <param name="project"></param>
	/// <exception cref="Exception">
	/// Throws an exception if the app.config could not be successfully verified/updated
	/// </exception>
	/// <returns>true if app,config was modified else false.</returns>
	// ---------------------------------------------------------------------------------
	public static bool ConfigureDbProvider(string xmlPath, Type factoryClass)
	{
		bool modified;


		try
		{
			XmlDocument xmlDoc = new XmlDocument();

			try
			{
				xmlDoc.Load(xmlPath);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				return false;
			}


			XmlNode xmlRoot = xmlDoc.DocumentElement;
			XmlNamespaceManager xmlNs = new XmlNamespaceManager(xmlDoc.NameTable);

			if (!xmlNs.HasNamespace("confBlackbirdNs"))
			{
				xmlNs.AddNamespace("confBlackbirdNs", xmlRoot.NamespaceURI);
			}


			modified = ConfigureDbProviderFactory(xmlDoc, xmlNs, xmlRoot, factoryClass);


			if (modified)
			{
				try
				{
					xmlDoc.Save(xmlPath);
					// Diag.Trace("app.config save: " + xmlPath);
				}
				catch (Exception ex)
				{
					modified = false;
					Diag.Dug(ex);
					return false;
				}

			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

		return modified;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if a project has Firebird EntityFramework configured in the app.config and configures it if it doesn't
	/// </summary>
	/// <param name="project"></param>
	/// <exception cref="Exception">
	/// Throws an exception if the app.config could not be successfully verified/updated
	/// </exception>
	/// <param name="configureDbProvider">
	///	If set to true then the client DBProvider factory will also be configured
	/// </param>
	/// <returns>true if app,config was modified else false.</returns>
	// ---------------------------------------------------------------------------------
	public static bool ConfigureEntityFramework(string xmlPath, bool configureDbProvider, Type factoryClass)
	{
		bool modified = false;


		try
		{
			XmlDocument xmlDoc = new XmlDocument();

			try
			{
				xmlDoc.Load(xmlPath);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				return false;
			}


			XmlNode xmlRoot = xmlDoc.DocumentElement;
			XmlNamespaceManager xmlNs = new XmlNamespaceManager(xmlDoc.NameTable);

			if (!xmlNs.HasNamespace("confBlackbirdNs"))
			{
				xmlNs.AddNamespace("confBlackbirdNs", xmlRoot.NamespaceURI);
			}

			if (configureDbProvider)
				modified = ConfigureDbProviderFactory(xmlDoc, xmlNs, xmlRoot, factoryClass);

			modified |= ConfigureEntityFrameworkProviderServices(xmlDoc, xmlNs, xmlRoot);


			if (modified)
			{
				try
				{
					xmlDoc.Save(xmlPath);
					// Diag.Trace("app.config save: " + xmlPath);
				}
				catch (Exception ex)
				{
					modified = false;
					Diag.Dug(ex);
					return false;
				}

			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

		return modified;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates the app.config xml Firebird EntityFramework section
	/// </summary>
	/// <param name="project"></param>
	/// <exception cref="Exception">
	/// Throws an exception if the app.config could not be successfully verified/updated
	/// </exception>
	/// <returns>true if xml was modified else false</returns>
	// ---------------------------------------------------------------------------------
	private static bool ConfigureEntityFrameworkProviderServices(XmlDocument xmlDoc, XmlNamespaceManager xmlNs, XmlNode xmlRoot)
	{
		bool modified = false;


		try
		{
			// For anyone watching, you have to denote your private namespace after every forwardslash in
			// the markup tree.
			// Q? Does this mean you can use different namespaces within the selection string?
			XmlNode xmlNode = null, xmlParent;
			XmlAttribute xmlAttr;

			xmlNode = xmlRoot.SelectSingleNode("//confBlackbirdNs:entityFramework", xmlNs);

			if (xmlNode == null)
			{
				modified = true;

				xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, "entityFramework", "");
				xmlParent = xmlRoot.AppendChild(xmlNode);
			}
			else
			{
				xmlParent = xmlNode;
			}


			xmlNode = xmlParent.SelectSingleNode("confBlackbirdNs:defaultConnectionFactory", xmlNs);

			if (xmlNode == null)
			{
				modified = true;

				xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, "defaultConnectionFactory", "");
				xmlAttr = xmlDoc.CreateAttribute("type");
				xmlAttr.Value = SystemData.EFConnectionFactory + ", " + SystemData.EFProvider;
				xmlNode.Attributes.Append(xmlAttr);
				xmlParent.AppendChild(xmlNode);
			}


			xmlNode = xmlParent.SelectSingleNode("confBlackbirdNs:providers", xmlNs);

			if (xmlNode == null)
			{
				modified = true;

				xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, "providers", "");
				xmlParent = xmlParent.AppendChild(xmlNode);
			}
			else
			{
				xmlParent = xmlNode;
			}

			xmlNode = xmlParent.SelectSingleNode("confBlackbirdNs:provider[@invariantName='" + SystemData.Invariant + "']", xmlNs);

			if (xmlNode == null)
			{
				modified = true;

				xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, "provider", "");
				xmlAttr = xmlDoc.CreateAttribute("invariantName");
				xmlAttr.Value = SystemData.Invariant;
				xmlNode.Attributes.Append(xmlAttr);
				xmlAttr = xmlDoc.CreateAttribute("type");
				xmlAttr.Value = SystemData.EFProviderServices + ", " + SystemData.EFProvider;
				xmlNode.Attributes.Append(xmlAttr);
				xmlParent.AppendChild(xmlNode);
			}
			else
			{
				xmlAttr = (XmlAttribute)xmlNode.Attributes.GetNamedItem("invariantName");
				if (xmlAttr == null)
				{
					modified = true;
					xmlAttr = xmlDoc.CreateAttribute("invariantName");
					xmlAttr.Value = SystemData.Invariant;
					xmlNode.Attributes.Append(xmlAttr);
				}
				else if (xmlAttr.Value != SystemData.Invariant)
				{
					modified = true;
					xmlAttr.Value = SystemData.Invariant;
				}

				xmlAttr = (XmlAttribute)xmlNode.Attributes.GetNamedItem("type");
				if (xmlAttr == null)
				{
					modified = true;
					xmlAttr = xmlDoc.CreateAttribute("type");
					xmlAttr.Value = SystemData.EFProviderServices + ", " + SystemData.EFProvider;
					xmlNode.Attributes.Append(xmlAttr);
				}
				else if (xmlAttr.Value.ToLower().Replace(" ", "") != SystemData.EFProviderServices.ToLower() + "," + SystemData.EFProvider.ToLower())
				{
					modified = true;
					xmlAttr.Value = SystemData.EFProviderServices + ", " + SystemData.EFProvider;
				}
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}


		return modified;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates the app.config xml system.data section
	/// </summary>
	/// <param name="project"></param>
	/// <exception cref="Exception">
	/// Throws an exception if the app.config could not be successfully verified/updated
	/// </exception>
	/// <returns>true if xml was modified else false</returns>
	// ---------------------------------------------------------------------------------
	private static bool ConfigureDbProviderFactory(XmlDocument xmlDoc, XmlNamespaceManager xmlNs, XmlNode xmlRoot, Type factoryClass)
	{
		bool modified = false;


		try
		{
			// For anyone watching, you have to denote your private namespace after every forwardslash in
			// the markup tree.
			// Q? Does this mean you can use different namespaces within the selection string?
			XmlNode xmlNode = null, xmlParent;
			XmlAttribute xmlAttr;

			xmlNode = xmlRoot.SelectSingleNode("//confBlackbirdNs:system.data", xmlNs);

			if (xmlNode == null)
			{
				modified = true;

				xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, "system.data", "");
				xmlParent = xmlRoot.AppendChild(xmlNode);
			}
			else
			{
				xmlParent = xmlNode;
			}


			xmlNode = xmlParent.SelectSingleNode("confBlackbirdNs:DbProviderFactories", xmlNs);

			if (xmlNode == null)
			{
				modified = true;

				xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, "DbProviderFactories", "");
				xmlParent = xmlParent.AppendChild(xmlNode);
			}
			else
			{
				xmlParent = xmlNode;
			}

			xmlNode = xmlParent.SelectSingleNode("confBlackbirdNs:add[@invariant='" + SystemData.Invariant + "']", xmlNs);

			// We're using the current latest version of the client (on this build it's 9.1.1.0)
			string factoryQualifiedNameType;
			string factoryNameType = SystemData.ProviderFactoryType + ", " + SystemData.Invariant;

			if (xmlNode == null)
			{
				modified = true;

				xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, "add", "");

				xmlAttr = xmlDoc.CreateAttribute("invariant");
				xmlAttr.Value = SystemData.Invariant;
				xmlNode.Attributes.Append(xmlAttr);

				xmlAttr = xmlDoc.CreateAttribute("name");
				xmlAttr.Value = SystemData.ProviderFactoryName;
				xmlNode.Attributes.Append(xmlAttr);

				xmlAttr = xmlDoc.CreateAttribute("description");
				xmlAttr.Value = SystemData.ProviderFactoryDescription;
				xmlNode.Attributes.Append(xmlAttr);

				xmlAttr = xmlDoc.CreateAttribute("type");
				xmlAttr.Value = factoryNameType;
				xmlNode.Attributes.Append(xmlAttr);

				xmlParent.AppendChild(xmlNode);
			}
			else
			{
				xmlAttr = (XmlAttribute)xmlNode.Attributes.GetNamedItem("type");
				if (xmlAttr == null)
				{
					modified = true;
					xmlAttr = xmlDoc.CreateAttribute("type");
					xmlAttr.Value = factoryNameType;
					xmlNode.Attributes.Append(xmlAttr);
				}
				else if (xmlAttr.Value.Replace(" ", "") != factoryNameType.Replace(" ", ""))
				{
					// Check if it's not using the fully qualified name - must be current latest version (build is 9.1.1)
					factoryQualifiedNameType = SystemData.ProviderFactoryType + ", " + factoryClass.AssemblyQualifiedName;

					if (xmlAttr.Value.Replace(" ", "") != factoryQualifiedNameType.Replace(" ", ""))
					{
						modified = true;
						xmlAttr.Value = factoryNameType;
					}
				}
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}


		return modified;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates an edmx if it was using the legacy Firebird client.
	/// </summary>
	/// <param name="project"></param>
	/// <exception cref="Exception">
	/// Throws an exception if there weere errors.
	/// </exception>
	/// <returns>true if edmx was modified else false.</returns>
	// ---------------------------------------------------------------------------------
	public static bool UpdateEdmx(string xmlPath)
	{

		try
		{
			XmlDocument xmlDoc = new XmlDocument();

			try
			{
				xmlDoc.Load(xmlPath);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}

			XmlNode xmlRoot = xmlDoc.DocumentElement;
			XmlNamespaceManager xmlNs = new XmlNamespaceManager(xmlDoc.NameTable);


			if (!xmlNs.HasNamespace("edmxBlackbird"))
				xmlNs.AddNamespace("edmxBlackbird", xmlRoot.NamespaceURI);
			if (!xmlNs.HasNamespace("ssdlBlackbird"))
				xmlNs.AddNamespace("ssdlBlackbird", "http://schemas.microsoft.com/ado/2009/11/edm/ssdl");



			XmlNode xmlNode = null;
			XmlAttribute xmlAttr;


			// You have to denote your private namespaces after every forwardslash in the markup tree.
			try
			{
				xmlNode = xmlRoot.SelectSingleNode("edmxBlackbird:Runtime/edmxBlackbird:StorageModels/ssdlBlackbird:Schema[@Provider='" + SystemData.Invariant + "']", xmlNs);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				return false;
			}

			if (xmlNode == null)
				return false;


			xmlNode = xmlRoot.SelectSingleNode("//edmxBlackbird:Designer/edmxBlackbird:Options/edmxBlackbird:DesignerInfoPropertySet/edmxBlackbird:DesignerProperty[@Name='UseLegacyProvider']", xmlNs);

			if (xmlNode == null)
				return false;


			xmlAttr = (XmlAttribute)xmlNode.Attributes.GetNamedItem("Value");

			if (xmlAttr != null && xmlAttr.Value == "false")
				return false;

			if (xmlAttr == null)
			{
				xmlAttr = xmlDoc.CreateAttribute("Value");
				xmlAttr.Value = "false";
				xmlNode.Attributes.Append(xmlAttr);
			}
			else
			{
				xmlAttr.Value = "false";
			}

			try
			{
				xmlDoc.Save(xmlPath);
				// Diag.Trace("edmx Xml saved: " + xmlPath);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

		return true;

	}

	#endregion Methods

}