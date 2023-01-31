
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;



namespace BlackbirdSql.Common;



// ---------------------------------------------------------------------------------------------------
//
//										DbXmlUpdater Class
//
// ---------------------------------------------------------------------------------------------------


/// <summary>
/// Updates project db xml items
/// </summary>
internal static class DbXmlUpdater
{

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

			if (!xmlNs.HasNamespace("confBlackbird"))
			{
				xmlNs.AddNamespace("confBlackbird", xmlRoot.NamespaceURI);
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

			if (!xmlNs.HasNamespace("confBlackbird"))
			{
				xmlNs.AddNamespace("confBlackbird", xmlRoot.NamespaceURI);
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

			xmlNode = xmlRoot.SelectSingleNode("//confBlackbird:entityFramework", xmlNs);

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


			xmlNode = xmlParent.SelectSingleNode("//confBlackbird:defaultConnectionFactory", xmlNs);

			if (xmlNode == null)
			{
				modified = true;

				xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, "defaultConnectionFactory", "");
				xmlAttr = xmlDoc.CreateAttribute("type");
				xmlAttr.Value = SystemData.EFConnectionFactory + ", " + SystemData.EFProvider;
				xmlNode.Attributes.Append(xmlAttr);
				xmlParent.AppendChild(xmlNode);
			}


			xmlNode = xmlParent.SelectSingleNode("//confBlackbird:providers", xmlNs);

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

			xmlNode = xmlParent.SelectSingleNode("//confBlackbird:provider[@invariantName='" + SystemData.Invariant + "']", xmlNs);

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

			xmlNode = xmlRoot.SelectSingleNode("//confBlackbird:system.data", xmlNs);

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


			xmlNode = xmlParent.SelectSingleNode("//confBlackbird:DbProviderFactories", xmlNs);

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

			xmlNode = xmlParent.SelectSingleNode("//confBlackbird:add[@invariant='" + SystemData.Invariant + "']", xmlNs);

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