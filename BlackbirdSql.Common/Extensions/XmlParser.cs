
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Xml;



namespace BlackbirdSql.Common.Extensions;



// ---------------------------------------------------------------------------------------------------
//
//										XmlParser Class
//
// ---------------------------------------------------------------------------------------------------


/// <summary>
/// Updates project db xml items
/// </summary>
internal static class XmlParser
{
	static DataTable _DataSources = null, _Databases = null;



	public static DataTable DataSources
	{
		get
		{
			if (_DataSources != null)
				return _DataSources;

			_DataSources = Databases.DefaultView.ToTable(true, "Orderer", "DataSourceName", "DataSource", "DataSourceLc", "PortNumber");


			return _DataSources;
		}
	}



	public static DataTable Databases
	{
		get
		{
			if (_Databases != null)
				return _Databases;

			DataTable databases = new DataTable();

			databases.Columns.Add("Id", typeof(int));
			databases.Columns.Add("Orderer", typeof(int));
			databases.Columns.Add("DataSourceName", typeof(string));
			databases.Columns.Add("DataSource", typeof(string));
			databases.Columns.Add("DataSourceLc", typeof(string));
			databases.Columns.Add("Name", typeof(string));
			databases.Columns.Add("InitialCatalog", typeof(string));
			databases.Columns.Add("InitialCatalogLc", typeof(string));
			databases.Columns.Add("PortNumber", typeof(int));
			databases.Columns.Add("Charset", typeof(string));
			databases.Columns.Add("UserName", typeof(string));
			databases.Columns.Add("Password", typeof(string));
			databases.Columns.Add("RoleName", typeof(string));


			bool hasLocal = false;
			DataRow row;

			string xmlPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\flamerobin\\fr_databases.conf";

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
					string datasourceName, datasource, authentication;

					xmlServers = xmlRoot.SelectNodes("//server");


					foreach (XmlNode xmlServer in xmlServers)
					{
						if ((xmlNode = xmlServer.SelectSingleNode("name")) == null)
							continue;
						datasourceName = xmlNode.InnerText;


						if ((xmlNode = xmlServer.SelectSingleNode("host")) == null)
							continue;
						datasource = xmlNode.InnerText;


						if ((xmlNode = xmlServer.SelectSingleNode("port")) == null)
							continue;
						port = Convert.ToUInt32(xmlNode.InnerText);

						if (port == 0)
							continue;

						xmlDatabases = xmlServer.SelectNodes("database");



						foreach (XmlNode xmlDatabase in xmlDatabases)
						{
							if ((xmlNode = xmlDatabase.SelectSingleNode("name")) == null)
							continue;


							row = databases.NewRow();

							row["Id"] = databases.Rows.Count;
							row["DataSourceName"] = datasourceName;
							row["DataSource"] = datasource;
							row["DataSourceLc"] = datasource.ToLower();
							row["Name"] = xmlNode.InnerText;
							row["PortNumber"] = (int)port;

							if ((xmlNode = xmlDatabase.SelectSingleNode("path")) == null)
							continue;
							row["InitialCatalog"] = xmlNode.InnerText;
							row["InitialCatalogLc"] = xmlNode.InnerText.ToLower();

							if ((xmlNode = xmlDatabase.SelectSingleNode("charset")) == null)
							continue;
							row["Charset"] = xmlNode.InnerText;

							row["UserName"] = "";
							row["Password"] = "";

							if ((xmlNode = xmlDatabase.SelectSingleNode("authentication")) == null)
								authentication = "trusted";
							else
								authentication = xmlNode.InnerText;

							if (authentication != "trusted")
							{
								if ((xmlNode = xmlDatabase.SelectSingleNode("username")) != null)
								{
									row["UserName"] = xmlNode.InnerText;

									if (authentication == "pwd"
										&& (xmlNode = xmlDatabase.SelectSingleNode("password")) != null)
									{
										row["Password"] = xmlNode.InnerText;
									}
								}

							}

							if (datasource == "localhost")
							{
								hasLocal = true;
								row["Orderer"] = 1;
							}
							else
							{
								row["Orderer"] = 2;
							}

							databases.Rows.Add(row);
						}
					}

				}
				catch (Exception ex)
				{
					Diag.Dug(ex);
				}

				row = databases.NewRow();

				row["Id"] = databases.Rows.Count;
				row["Orderer"] = 0;
				row["DataSourceName"] = "Clear";
				row["DataSource"] = "";
				row["DataSourceLc"] = "";
				row["Name"] = "";
				row["PortNumber"] = 0;
				row["InitialCatalog"] = "";
				row["InitialCatalogLc"] = "";
				row["Charset"] = "";
				row["UserName"] = "";
				row["Password"] = "";
				databases.Rows.Add(row);

				if (!hasLocal)
				{
					row = databases.NewRow();

					row["Id"] = databases.Rows.Count;
					row["Orderer"] = 1;
					row["DataSourceName"] = "Localhost";
					row["DataSource"] = "localhost";
					row["DataSourceLc"] = "localhost";
					row["Name"] = "";
					row["PortNumber"] = 0;
					row["InitialCatalog"] = "";
					row["InitialCatalogLc"] = "";
					row["Charset"] = "";
					row["UserName"] = "";
					row["Password"] = "";

					databases.Rows.Add(row);
				}

				databases.DefaultView.Sort = "Orderer,DataSourceName,Name ASC";


			}

			_Databases = databases.DefaultView.ToTable(false);

			try
			{
				foreach (DataRow ro in _Databases.Rows)
					Diag.Trace(ro["Id"].ToString() + " : " + ro["Orderer"].ToString() + " : " + (string)ro["DataSourceName"] + " : " + (string)ro["DataSource"] + " : " + (string)ro["DataSourceLc"] + " : " + (string)ro["Name"] + " : " + ro["PortNumber"].ToString() + " : " + (string)ro["InitialCatalog"] + " : " + (string)ro["InitialCatalogLc"] + " : " + (string)ro["Charset"] + " : " + (string)ro["UserName"] + " : " + (string)ro["Password"]);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}

			return _Databases;
		}
	}



	/// <summary>
	/// Checks if a project has Firebird EntityFramework configured in the app.config and configures it if it doesn't
	/// </summary>
	/// <param name="project"></param>
	/// <exception cref="Exception">
	/// Throws an exception if the app.config could not be successfully verified/updated
	/// </exception>
	/// <returns>true if app,config was modified else false.</returns>
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
					Diag.Trace("app.config save: " + xmlPath);
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
					Diag.Trace("app.config save: " + xmlPath);
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



	/// <summary>
	/// Updates the app.config xml Firebird EntityFramework section
	/// </summary>
	/// <param name="project"></param>
	/// <exception cref="Exception">
	/// Throws an exception if the app.config could not be successfully verified/updated
	/// </exception>
	/// <returns>true if xml was modified else false</returns>
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



	/// <summary>
	/// Updates the app.config xml system.data section
	/// </summary>
	/// <param name="project"></param>
	/// <exception cref="Exception">
	/// Throws an exception if the app.config could not be successfully verified/updated
	/// </exception>
	/// <returns>true if xml was modified else false</returns>
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

			// We're using the current latest version of the client (on this build it's 9.1.0.0)
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
					// Check if it's not using the fully qualified name - must be current latest version (build is 9.1.0)
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




	/// <summary>
	/// Updates an edmx if it was using the legacy Firebird client.
	/// </summary>
	/// <param name="project"></param>
	/// <exception cref="Exception">
	/// Throws an exception if there weere errors.
	/// </exception>
	/// <returns>true if edmx was modified else false.</returns>
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
				Diag.Trace("edmx Xml saved: " + xmlPath);
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
}