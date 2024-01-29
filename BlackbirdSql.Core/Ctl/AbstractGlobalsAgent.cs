using System;
using System.IO;
using System.Text;
using System.Xml;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Interfaces;


namespace BlackbirdSql.Core.Ctl;

// =========================================================================================================
//											AbstractGlobalsAgent Class
//
/// <summary>
/// Replacement for solution and project globals using .sou and .user storage
/// for a single integer value. The abstract definition deals with reading and writing to the solution
/// stream and .user xml files.
/// </summary>
// =========================================================================================================
public abstract class AbstractGlobalsAgent : IBGlobalsAgent
{

	// ---------------------------------------------------------------------------------
	#region Constants - AbstractGlobalsAgent
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// This key is the globals persistent key for the solution stream and .user xml
	/// UserProperties node attribute.
	/// </summary>
	public const string C_PersistentKey = "GlobalBlackbirdPersistent";


	#endregion Constants




	// =========================================================================================================
	#region Fields - AbstractGlobalsAgent
	// =========================================================================================================


	protected static IBGlobalsAgent _SolutionGlobals = null;
	protected static bool _ValidateSolution;
	protected static bool _PersistentValidation;
	protected static bool _ValidateConfig;
	protected static bool _ValidateEdmx;

	private readonly string _ProjectPath = null;
	private int? _Value;
	private int? _StoredValue;


	#endregion Fields




	// =========================================================================================================
	#region Property Accessors - AbstractGlobalsAgent
	// =========================================================================================================


	public abstract bool IsValidateFailedStatus { get; set; }
	public abstract bool IsConfiguredDbProviderStatus { get; }
	public abstract bool IsConfiguredEFStatus { get; }
	public abstract bool IsScannedStatus { get; set; }
	public abstract bool IsUpdatedEdmxsStatus { get; set; }
	public abstract bool IsValidatedStatus { get; }
	public abstract bool IsValidStatus { get; set; }
	public abstract bool IsValidatedDbProviderStatus { set; }
	public abstract bool IsValidatedEFStatus { set; }


	/// <summary>
	/// The current validation Globals flags status of a Solution or Project
	/// as defined under Constants in GlobalsAgent.
	/// </summary>
	public int Value
	{
		get
		{
			if (!_Value.HasValue)
				return 0;
			return _Value.Value;
		}
		set
		{
			_Value = value;
		}

	}


	/// <summary>
	/// Shortcut for checking if the global has a value.
	/// </summary>
	public bool VariableExists => _Value.HasValue;


	#endregion Property accessors




	// =========================================================================================================
	#region Constructors / Destructors - AbstractGlobalsAgent
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Project Globals .ctor.
	/// </summary>
	/// <param name="projectFilePath">Full file path including project type extension</param>
	// ---------------------------------------------------------------------------------
	public AbstractGlobalsAgent(string projectFilePath)
	{
		// project.Properties.Item("FullPath").Value, project.Properties.Item("FileName").Value
		_ProjectPath = projectFilePath != null ? (projectFilePath + ".user") : null;
		try
		{
			ReadProjectGlobal();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Solution Globals .ctor.
	/// </summary>
	/// <param name="solution">The <see cref="EnvDTE.DTE.Solution"/> object</param>
	/// <param name="stream">The stream from <see cref="Package.OnLoadOptions" event./></param>
	// ---------------------------------------------------------------------------------
	public AbstractGlobalsAgent(Stream stream)
	{
		_ValidateSolution = true;
		_PersistentValidation = false;
		_ValidateConfig = true;
		_ValidateEdmx = true;

		if (!_ValidateSolution || !_PersistentValidation)
			return;

		// Deprecated.

		try
		{
			long len = stream.Length;

			if (len > 10)
				len = 20;

			byte[] buffer = new byte[len];

			if (len > 0)
			{
				len = stream.Read(buffer, 0, (int)len);

				if (len > 0)
				{
					if (!_PersistentValidation)
					{
						Tracer.Warning(GetType(), "AbstractGlobalsAgent(Stream)", "A validation status Global was found for the Solution but was not used because Persistent validation is disabled. Value: {0}", Encoding.Default.GetString(buffer));
						_StoredValue = Convert.ToInt32(Encoding.Default.GetString(buffer));
					}
					else
					{
						_Value = _StoredValue = Convert.ToInt32(Encoding.Default.GetString(buffer));
					}
				}
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Destructor.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public abstract void Dispose();


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Methods - AbstractGlobalsAgent
	// =========================================================================================================


	public abstract bool ClearValidateStatus();



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Deprecated.
	/// Flushes a project Globals to the .user file.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public bool Flush()
	{
		return WriteProjectGlobal();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Deprecated.
	/// Flushes a solution Globals to the solution stream.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public bool Flush(Stream stream)
	{
		if (!_ValidateSolution)
			return false;

		// Deprecated. Exit.
		if (!_PersistentValidation)
			return true;

		int? runningValue = _Value;

		if (IsValidateFailedStatus)
			ClearValidateStatus();
		else if (!IsValidatedStatus)
			IsValidStatus = true;


		try
		{
			if (_PersistentValidation && _Value.HasValue && _Value.Value != 0)
			{
				byte[] buffer = Encoding.Default.GetBytes(_Value.Value.ToString());

				stream.Write(buffer, 0, buffer.Length);
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}
		finally
		{
			_Value = runningValue;
		}

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Deprecated.
	/// Reads and updates a project Globals from the .user file.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private bool ReadProjectGlobal()
	{

		// Deprecated.
		if (!_PersistentValidation)
		{
			_Value = _StoredValue = 0;
			return true;
		}

		if (_ProjectPath == null)
		{
			// Tracer.Trace(GetType(), "ReadProjectGlobal()", "Exiting. Project path is null.");
			return false;
		}


		if (!File.Exists(_ProjectPath))
		{
			// Tracer.Trace(GetType(), "ReadProjectGlobal()", "Exiting. Project file not found: {0}.", _ProjectPath);
			return false;
		}


		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.Load(_ProjectPath);

		XmlNode xmlRoot = xmlDoc.DocumentElement;
		XmlNamespaceManager xmlNs = new XmlNamespaceManager(xmlDoc.NameTable);

		if (!xmlNs.HasNamespace("bbxmlns"))
			xmlNs.AddNamespace("bbxmlns", xmlRoot.NamespaceURI);


		XmlNode xmlNode;
		XmlNode xmlParent = xmlRoot.SelectSingleNode("//bbxmlns:ProjectExtensions", xmlNs);

		if (xmlParent == null)
		{
			// Tracer.Trace(GetType(), "ReadProjectGlobal()", "Exiting. Node ProjectExtensions not found for project: {0}.", _ProjectPath);
			return false;
		}


		xmlNode = xmlParent.SelectSingleNode("bbxmlns:VisualStudio", xmlNs);

		if (xmlNode == null)
		{
			// Tracer.Trace(GetType(), "ReadProjectGlobal()", "Exiting. Node VisualStudio not found for project: {0}.", _ProjectPath);
			return false;
		}

		xmlParent = xmlNode;


		xmlNode = xmlParent.SelectSingleNode("bbxmlns:UserProperties", xmlNs);

		if (xmlNode == null)
		{
			// Tracer.Trace(GetType(), "ReadProjectGlobal()", "Exiting. Node UserProperties not found for project: {0}.", _ProjectPath);
			return false;
		}


		XmlAttribute xmlAttr = xmlNode.Attributes[C_PersistentKey];

		if (xmlAttr == null)
		{
			// Tracer.Trace(GetType(), "ReadProjectGlobal()", "Exiting. Attribute {0} not found for project: {1}.", C_PersistentKey, _ProjectPath);
			return false;
		}

		int value = Convert.ToInt32(xmlAttr.Value);



		if (!_PersistentValidation)
		{
			// Tracer.Warning(GetType(), "ReadProjectGlobal()", "A validation status Global was found for project {0} but was not used because Persistent validation is disabled. Value: {1}", _ProjectPath, value);
			_StoredValue = value;
			return true;
		}

		_Value = _StoredValue = value;

		return true;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Deperecated.
	/// Writes a project Globals to the .user file.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private bool WriteProjectGlobal()
	{
		// Deprecated.
		if (!_PersistentValidation)
			return true;

		if (_ProjectPath == null)
			return false;

		bool save = false;
		bool remove = false;
		int value = _Value ?? 0;
		int storedValue = _StoredValue ?? 0;

		// For settings...
		// _PersistentValidation applies here.
		// if disabled and the value exists we must remove it
		// else exit.


		if (!_PersistentValidation)
		{
			// If not persistent
			remove = _StoredValue.HasValue;
		}
		else if (value == 0)
		{
			// We don't store 0.
			remove = _StoredValue.HasValue;
		}
		else if (value != storedValue)
		{
			save = true;
		}


		// No action so exit.
		if (!save && !remove)
		{
			// Tracer.Trace(GetType(), "WriteProjectGlobal()", "No action for Project: {0}.", _ProjectPath);
			return false;
		}

		if (remove)
		{
			// Tracer.Trace(GetType(), "WriteProjectGlobal()", "Clearing value {0} from Project: {1}.", _StoredValue.Value, _ProjectPath);
		}
		else
		{
			// Tracer.Trace(GetType(), "WriteProjectGlobal()", "Writing value {0} to Project: {1}.", _Value.Value, _ProjectPath);
		}

		if (!File.Exists(_ProjectPath))
		{
			// Not saving and nothing to remove so no need to create, so just exit.
			if (!save || remove)
				return false;

			// If we're here the only action left is save, so we create the full content and exit.
			string text = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""Current"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <ProjectExtensions>
    <VisualStudio>
      <UserProperties GlobalBlackbirdPersistent=""{0}"" />
    </VisualStudio>
  </ProjectExtensions>
</Project>";


			StreamWriter sw = File.CreateText(_ProjectPath);
			sw.Write(string.Format(text, _Value));
			sw.Close();
			return true;
		}

		XmlDocument xmlDoc = new XmlDocument();

		xmlDoc.Load(_ProjectPath);

		XmlNode xmlRoot = xmlDoc.DocumentElement;
		XmlNamespaceManager xmlNs = new XmlNamespaceManager(xmlDoc.NameTable);


		if (!xmlNs.HasNamespace("bbxmlns"))
			xmlNs.AddNamespace("bbxmlns", xmlRoot.NamespaceURI);

		XmlNode xmlNode;
		XmlNode xmlParent = xmlRoot.SelectSingleNode("//bbxmlns:ProjectExtensions", xmlNs);

		if (xmlParent == null)
		{
			// Not saving and nothing to remove so no need to create, so just exit.
			if (!save || remove)
				return false;

			// If we're here the only action left is save, so we create the node.
			xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, "ProjectExtensions", xmlRoot.NamespaceURI);
			xmlParent = xmlRoot.AppendChild(xmlNode);
		}

		xmlNode = xmlParent.SelectSingleNode("bbxmlns:VisualStudio", xmlNs);

		if (xmlNode == null)
		{
			// Not saving and nothing to remove so no need to create, so just exit.
			if (!save || remove)
				return false;

			// If we're here the only action left is save, so we create the node.
			xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, "VisualStudio", xmlRoot.NamespaceURI);
			xmlNode = xmlParent.AppendChild(xmlNode);
		}

		xmlParent = xmlNode;

		xmlNode = xmlParent.SelectSingleNode("bbxmlns:UserProperties", xmlNs);

		if (xmlNode == null)
		{
			// Not saving and nothing to remove so no need to create, so just exit.
			if (!save || remove)
				return false;

			// If we're here the only action left is save, so we create the node.
			xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, "UserProperties", xmlRoot.NamespaceURI);
			xmlNode = xmlParent.AppendChild(xmlNode);
		}


		XmlAttribute xmlAttr = xmlNode.Attributes[C_PersistentKey];

		if (xmlAttr == null)
		{
			// Not saving and nothing to remove so no need to create, so just exit.
			if (!save || remove)
				return false;
		}

		if (remove)
		{
			// If we're here the attribute must exist, so remove.
			xmlNode.Attributes.Remove(xmlAttr);
		}
		else
		{
			// remove and save are mutually exclusive so we must be saving.
			if (xmlAttr == null)
			{
				// Create the attribute
				xmlAttr = xmlDoc.CreateAttribute(C_PersistentKey);
				xmlAttr.Value = _Value.ToString();
				xmlNode.Attributes.Append(xmlAttr);
			}
			else
			{
				// Update the existing attribute.
				xmlAttr.Value = _Value.ToString();
			}
		}

		xmlDoc.Save(_ProjectPath);

		return true;

	}


	#endregion Methods

}