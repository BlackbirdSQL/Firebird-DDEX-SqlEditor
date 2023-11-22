// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;

using BlackbirdSql.Core.Ctl.Config;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Extensions;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Properties;

using EnvDTE;

using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;


namespace BlackbirdSql.Core.Ctl;

[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread",
	Justification = "Class is UIThread compliant.")]

// =========================================================================================================
//											ConnectionLocator Class
//
/// <summary>
/// Loads and registers FlameRobin configured connections and, if enabled in user settings, recursively
/// locates configured connection strings in a solution's projects and registers them if they are
/// equivalency unique.
/// </summary>
// =========================================================================================================
public abstract class ConnectionLocator
{
	// A static class lock
	private static readonly object _LockClass = new();
	private static bool _HasLocal = false;
	private static IDictionary<string, string> _TempServerNames = null;
	private static DataTable _DataSources = null, _Databases = null;

	// =========================================================================================================
	#region Property accessors - ConnectionLocator
	// =========================================================================================================


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
			lock (_LockClass)
			{
				if (_Databases != null)
					return _Databases;

				LoadConfiguredConnections();

				return _Databases;
			}
		}
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
			lock (_LockClass)
			{
				if (_DataSources != null)
					return _DataSources;

				_DataSources = Databases.DefaultView.ToTable(true, "Orderer", "DataSource", "DataSourceLc", "Port");


				return _DataSources;
			}
		}
	}


	private static IVsSolution DteSolution => Controller.Instance.DteSolution;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - ConnectionLocator
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates an initialized connection node row.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static DataRow CreateConnectionNodeRow(DataTable table, CsbAgent csa = null)
	{
		object value;
		DataRow row = table.NewRow();

		foreach (Describer describer in CsbAgent.Describers.Values)
		{
			if (csa != null)
			{
				value = csa.ContainsKey(describer.Name) ? csa[describer.Name] : null;
				if (value != null)
				{
					row[describer.Name] = value;
					continue;
				}
			}

			if (describer.DefaultValue != null)
				row[describer.Name] = describer.DefaultValue;
			else
				row[describer.Name] = DBNull.Value;
		}

		return row;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the app.config <see cref="ProjectItem"/> of a <see cref="Project"/>
	/// </summary>
	/// <param name="project"></param>
	/// <returns>true if app.config was updated else false</returns>
	// ---------------------------------------------------------------------------------
	private static ProjectItem GetAppConfigProjectItem(Project project, bool createIfNotFound = false)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		ProjectItem config = null;

		try
		{
			foreach (ProjectItem item in project.ProjectItems)
			{
				if (item.Name.ToLower() == "app.config")
				{
					config = item;
					break;
				}
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return null;
		}

		return config;

	}



	public static DbConnectionStringBuilder GetCsbFromDatabases(string datasetKey)
	{
		lock (_LockClass)
		{
			foreach (DataRow row in Databases.Rows)
			{
				if ((string)row["Name"] == "")
					continue;

				if (datasetKey.Equals((string)row["DatasetKey"]))
				{
					DbConnectionStringBuilder csb = new();

					foreach (Describer describer in CsbAgent.Describers.Values)
					{
						if (row[describer.Name] == null || row[describer.Name] == DBNull.Value
							|| describer.DefaultEquals(row[describer.Name]))
						{
							continue;
						}

						csb[describer.Name] = row[describer.Name];
					}

					return csb;
				}
			}
		}

		return null;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Identifies whether or not a project is a CPS project
	/// </summary>
	/// <param name="hierarchy"></param>
	/// <returns>true if project is CPS</returns>
	// ---------------------------------------------------------------------------------
	private static bool IsCpsProject(IVsHierarchy hierarchy)
	{
		Requires.NotNull(hierarchy, "hierarchy");
		return hierarchy.IsCapabilityMatch("CPS");
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks wether the project is a valid executable output type that requires
	/// configuration of the app.config
	/// </summary>
	/// <param name="project"></param>
	/// <returns>
	/// True if the project is a valid C#/VB executable project else false.
	/// </returns>
	/// <remarks>
	/// We're not going to worry about anything but C# and VB non=CSP projects
	/// </remarks>
	// ---------------------------------------------------------------------------------
	private static bool IsValidExecutableProjectType(IVsSolution solution, Project project)
	{

		// We're only supporting C# and VB projects for this - a dict list is at the end of this class
		if (project.Kind != "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}"
			&& project.Kind != "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}")
		{
			return false;
		}

		bool result = false;


		// Don't process CPS projects
		solution.GetProjectOfUniqueName(project.UniqueName, out IVsHierarchy hierarchy);


		if (!IsCpsProject(hierarchy))
		{
			int outputType = int.MaxValue;

			if (project.Properties != null && project.Properties.Count > 0)
			{
				Property property = project.Properties.Item("OutputType");
				if (property != null)
					outputType = (int)property.Value;
			}


			if (outputType < 2)
				result = true;
		}

		return result;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Searches for application configured connections and performs registration.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void LoadConfiguredConnections()
	{
		lock (_LockClass)
		{
			if (_Databases != null)
				return;

			if (!ThreadHelper.CheckAccess())
			{
				COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
				Diag.Dug(exc);
				throw exc;
			}

			if (Controller.Instance.Dte == null || Controller.Instance.Dte.Solution == null
				|| Controller.Instance.Dte.Solution.Projects == null)
			{
				COMException exc = new("DTE.Solution.Projects is not available", VSConstants.RPC_E_INVALID_DATA);
				Diag.Dug(exc);
				throw exc;
			}

			_TempServerNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			LoadConfiguredConnectionsImpl();

			_HasLocal = false;
			_TempServerNames = null;
		}

	}



	private static void LoadConfiguredConnectionsImpl()
	{
		string str;

		DataTable databases = new DataTable();


		databases.Columns.Add("Id", typeof(int));
		databases.Columns.Add("Orderer", typeof(int));
		databases.Columns.Add("DataSourceLc", typeof(string));
		databases.Columns.Add("Name", typeof(string));
		databases.Columns.Add("DatabaseLc", typeof(string));

		foreach (Describer describer in CsbAgent.Describers.Values)
			databases.Columns.Add(describer.Name, describer.DataType);

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
				int port;
				string serverName, datasource, authentication, user, password, path, charset;
				string datasetId;

				xmlServers = xmlRoot.SelectNodes("//server");


				foreach (XmlNode xmlServer in xmlServers)
				{
					if ((xmlNode = xmlServer.SelectSingleNode("name")) == null)
						continue;
					serverName = xmlNode.InnerText.Trim();

					if ((xmlNode = xmlServer.SelectSingleNode("host")) == null)
						continue;
					datasource = xmlNode.InnerText.Trim();


					if ((xmlNode = xmlServer.SelectSingleNode("port")) == null)
						continue;
					port = Convert.ToInt32(xmlNode.InnerText.Trim());

					if (port == 0)
						port = CoreConstants.C_DefaultPort;


					if (datasource.ToLowerInvariant() == "localhost")
					{
						datasource = "localhost";
						_HasLocal = true;
					}

					// To keep uniformity of server names, the case of the first connection
					// discovered for a server name is the case that will be used for all
					// connections for that server.
					if (!_TempServerNames.TryGetValue(datasource.ToLowerInvariant(), out serverName))
					{
						_TempServerNames.Add(datasource.ToLowerInvariant(), datasource);
						serverName = datasource;
					}


					row = CreateConnectionNodeRow(databases);

					row["Id"] = databases.Rows.Count;
					row["DataSource"] = serverName;
					row["DataSourceLc"] = serverName.ToLower();
					row["Port"] = port;

					row["Name"] = "";
					row["DatabaseLc"] = "";

					if (serverName == "localhost")
						row["Orderer"] = 2;
					else
						row["Orderer"] = 3;

					str = "AddServerRow: ";

					foreach (DataColumn col in databases.Columns)
						str += col.ColumnName + ": " + row[col.ColumnName] + ", ";

					// Tracer.Trace(typeof(ConnectionLocator), "LoadConfiguredConnectionsImpl()", str);

					databases.Rows.Add(row);


					xmlDatabases = xmlServer.SelectNodes("database");



					foreach (XmlNode xmlDatabase in xmlDatabases)
					{
						if ((xmlNode = xmlDatabase.SelectSingleNode("name")) == null)
							continue;


						// Add a ghost row to each database
						// A binding source cannot have an invalidated state. ie. Position == -1 and Current == null,
						// if it's List Count > 0. The ghost row is a placeholder for that state.

						row = CreateConnectionNodeRow(databases);

						row["Id"] = databases.Rows.Count;
						row["DataSource"] = serverName;
						row["DataSourceLc"] = serverName.ToLower();
						row["Port"] = port;

						datasetId = xmlNode.InnerText.Trim();
						row["Name"] = datasetId;

						if ((xmlNode = xmlDatabase.SelectSingleNode("path")) == null)
							continue;

						path = xmlNode.InnerText.Trim();
						row["Database"] = path;
						row["DatabaseLc"] = path.ToLower();

						if ((xmlNode = xmlDatabase.SelectSingleNode("charset")) == null)
							continue;


						charset = xmlNode.InnerText.Trim();
						row["Charset"] = charset;


						user = "";
						password = "";

						if ((xmlNode = xmlDatabase.SelectSingleNode("authentication")) == null)
							authentication = "trusted";
						else
							authentication = xmlNode.InnerText.Trim();

						if (authentication != "trusted")
						{
							if ((xmlNode = xmlDatabase.SelectSingleNode("username")) != null)
							{
								user = xmlNode.InnerText.Trim();

								if (authentication == "pwd"
									&& (xmlNode = xmlDatabase.SelectSingleNode("password")) != null)
								{
									password = xmlNode.InnerText.Trim();
								}
							}

						}

						row["UserID"] = user;
						row["Password"] = password;


						// Tracer.Trace(typeof(ConnectionLocator), "LoadConfiguredConnectionsImpl()", "Calling RegisterDatasetKey for datasetId: {0}.", datasetId);

						// The datasetId may not be unique at this juncture.
						CsbAgent csa = CsbAgent.CreateRegisteredDataset(datasetId, serverName, port, path,
							user, password, charset);

						if (csa == null)
							continue;

						// Tracer.Trace(typeof(ConnectionLocator), "LoadConfiguredConnectionsImpl()", "Database path: {0}.", path);


						// CsbAgent will return a new DatasetId if the supplied one was not unique.
						// to the server.
						row["DatasetKey"] = csa.DatasetKey;
						row["DatasetId"] = csa.DatasetId;
						row["Dataset"] = csa.Dataset;

						if (serverName == "localhost")
							row["Orderer"] = 2;
						else
							row["Orderer"] = 3;


						str = "AddDbRow: ";

						foreach (DataColumn col in databases.Columns)
							str += col.ColumnName + ": " + row[col.ColumnName] + ", ";

						// Tracer.Trace(typeof(ConnectionLocator), "LoadConfiguredConnectionsImpl()", str);

						databases.Rows.Add(row);
					}
				}

			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}

			if (UserSettings.IncludeAppConnections)
				LoadSolutionConfiguredConnections(databases);



			// Add a ghost row to the datasources list
			// This will be the default datasource row so that anything else
			// selected will generate a CurrentChanged event.
			row = CreateConnectionNodeRow(databases);

			row["Id"] = databases.Rows.Count;
			row["DataSourceLc"] = "";
			row["Port"] = 0;

			row["Name"] = "";
			row["DatabaseLc"] = "";

			row["Orderer"] = 0;

			str = "AddGhostRow: ";

			foreach (DataColumn col in databases.Columns)
				str += col.ColumnName + ": " + row[col.ColumnName] + ", ";

			// Tracer.Trace(typeof(ConnectionLocator), "LoadConfiguredConnectionsImpl()", str);

			databases.Rows.Add(row);


			// Add a Clear/Reset dummy row for the datasources list
			// If selected will invoke a form reset the move the cursor back to the ghost row.
			row = CreateConnectionNodeRow(databases);

			row["Id"] = databases.Rows.Count;
			row["Orderer"] = 1;
			row["DataSource"] = "Reset";
			row["DataSourceLc"] = "reset";
			row["Name"] = "";
			row["Port"] = 0;
			row["DatabaseLc"] = "";

			str = "AddResetRow: ";

			foreach (DataColumn col in databases.Columns)
				str += col.ColumnName + ": " + row[col.ColumnName] + ", ";

			// Tracer.Trace(typeof(ConnectionLocator), "LoadConfiguredConnectionsImpl()", str);

			databases.Rows.Add(row);


			// Add at least one row, that will be the ghost row, for localhost. 
			if (!_HasLocal)
			{
				row = CreateConnectionNodeRow(databases);

				row["Id"] = databases.Rows.Count;
				row["Orderer"] = 2;
				row["DataSource"] = "localhost";
				row["DataSourceLc"] = "localhost";
				row["Name"] = "";
				row["DatabaseLc"] = "";

				str = "AddLocalHostGhostRow: ";

				foreach (DataColumn col in databases.Columns)
					str += col.ColumnName + ": " + row[col.ColumnName] + ", ";

				// Tracer.Trace(typeof(ConnectionLocator), "LoadConfiguredConnectionsImpl()", str);

				databases.Rows.Add(row);
			}

			databases.DefaultView.Sort = "Orderer,DataSource,DatasetId ASC";


		}

		_Databases = databases.DefaultView.ToTable(false);

	}



	private static void LoadSolutionConfiguredConnections(DataTable databases)
	{ 

		if (Controller.Instance.Dte.Solution.Projects.Count == 0)
			return;

		int projectCount = Controller.Instance.Dte.Solution.Projects.Count;


		for (int i = 0; i < projectCount; i++)
		{
			if (!RecursiveScanSolutionProject(databases, i))
				i = projectCount - 1;
		}

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Recursively validates a project already opened before our package was sited
	/// </summary>
	/// <param name="projects"></param>
	/// <remarks>
	/// If the project is valid and has EntityFramework referenced, the app.config is checked.
	/// If it doesn't an <see cref="OnGlobalReferenceAdded"/> event is attached.
	/// Updates legacy edmxs in the project
	/// If it's a folder project is checked for child projects
	/// </remarks>
	// ---------------------------------------------------------------------------------
	private static void RecursiveScanProject(DataTable databases, Project project)
	{
		ProjectItem config = null;

		// There's a dict list of these at the end of the class
		if (Kind(project.Kind) == "ProjectFolder")
		{
			if (project.ProjectItems != null && project.ProjectItems.Count > 0)
			{
				// Diag.Trace("Recursing ProjectFolder: " + project.Name);
				RecursiveScanProject(databases, project.ProjectItems);
			}
			/*
			else
			{
				// Diag.Trace("No items in ProjectFolder: " + project.Name);
			}
			*/
		}
		else
		{
			// Diag.Trace("Recursive validate project: " + project.Name);

			if (IsValidExecutableProjectType(DteSolution, project))
			{

				// VSProject projectObject = project.Object as VSProject;


				config ??= GetAppConfigProjectItem(project);
				if (config != null)
					ScanAppConfig(databases, config);

				try
				{
					foreach (ProjectItem item in project.ProjectItems)
						RecursiveScanProjectItem(databases, item);
				}
				catch (Exception ex)
				{
					Diag.Dug(ex);
				}

			}

		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Recursively validates projects already opened before our package was sited
	/// This list is tertiary level projects from parent projects (solution folders)
	/// </summary>
	/// <param name="projects"></param>
	// ---------------------------------------------------------------------------------
	private static void RecursiveScanProject(DataTable databases, ProjectItems projectItems)
	{
		foreach (ProjectItem projectItem in projectItems)
		{
			if (projectItem.SubProject != null)
			{
				RecursiveScanProject(databases, projectItem.SubProject);
			}
			else
			{
				// Diag.Trace(projectItem.Name + " projectItem.SubProject is null (Possible Unloaded project or document)");
			}
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if a project item is an edmx and calls <see cref="UpdateEdmx"/> if it is.
	/// If it's a project folder it recursively calls itself.
	/// </summary>
	/// <param name="item"></param>
	/// <returns>true if completed successfully else false if there were errors.</returns>
	// ---------------------------------------------------------------------------------
	private static bool RecursiveScanProjectItem(DataTable databases, ProjectItem item)
	{
		if (Kind(item.Kind) == "PhysicalFolder")
		{
			bool success = true;

			foreach (ProjectItem subitem in item.ProjectItems)
			{
				if (!RecursiveScanProjectItem(databases, subitem))
					success = false;
			}

			return success;
		}

		return true;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Moves back onto the UI thread and validates the next top-level project.
	/// </summary>
	/// <param name="index">Index of the next project</param>
	// ---------------------------------------------------------------------------------
	private static bool RecursiveScanSolutionProject(DataTable databases, int index)
	{

		int i = 0;
		Project project = null;

		// The enumerator is not accessable thru GetEnumerator()
		foreach (Project proj in Controller.Instance.Dte.Solution.Projects)
		{
			if (i == index)
			{
				project = proj;
				break;
			}
			i++;
		}
		if (project == null)
			return false;


		RecursiveScanProject(databases, project);

		return true;
	}



	private static void RegisterAppConnectionStrings(DataTable databases, string projectName, string xmlPath)
	{
		string str;

		XmlDocument xmlDoc = new XmlDocument();

		try
		{
			xmlDoc.Load(xmlPath);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return;
		}


		try
		{


			XmlNode xmlRoot = xmlDoc.DocumentElement;
			XmlNamespaceManager xmlNs = new XmlNamespaceManager(xmlDoc.NameTable);

			if (!xmlNs.HasNamespace("confBlackbirdNs"))
			{
				xmlNs.AddNamespace("confBlackbirdNs", xmlRoot.NamespaceURI);
			}


			// For anyone watching, you have to denote your private namespace after every forwardslash in
			// the markup tree.
			// Q? Does this mean you can use different namespaces within the selection string?
			XmlNode xmlNode = null, xmlParent;

			xmlNode = xmlRoot.SelectSingleNode("//confBlackbirdNs:connectionStrings", xmlNs);

			if (xmlNode == null)
				return;

			xmlParent = xmlNode;


			XmlNodeList xmlNodes = xmlParent.SelectNodes("confBlackbirdNs:add[@providerName='" + SystemData.Invariant + "']", xmlNs);

			if (xmlNodes.Count == 0)
				return;

			string datasetId, datasource;
			string[] arr;
			CsbAgent csa;
			DataRow row;

			foreach (XmlNode connectionNode in xmlNodes)
			{
				arr = connectionNode.Attributes["name"].Value.Split('.');
				datasetId = Resources.ConnectionLocatorProjectDatasetId.FmtRes(projectName, arr[^1]);

				// Tracer.Trace(typeof(ConnectionLocator), "RegisterAppConnectionStrings()", "connectionString: {0}.", connectionNode.Attributes["connectionString"].Value);

				csa = new(connectionNode.Attributes["connectionString"].Value);

				datasource = csa.DataSource;

				if (datasource.ToLowerInvariant() == "localhost")
				{
					datasource = "localhost";
					_HasLocal = true;
				}

				// To keep uniformity of server names, the case of the first connection
				// discovered for a server name is the case that will be used for all
				// connections for that server.
				if (!_TempServerNames.TryGetValue(datasource.ToLowerInvariant(), out string serverName))
				{
					// No server row previously added so add one.

					_TempServerNames.Add(datasource.ToLowerInvariant(), datasource);
					serverName = datasource;


					row = CreateConnectionNodeRow(databases);

					row["Id"] = databases.Rows.Count;
					row["DataSource"] = serverName;
					row["DataSourceLc"] = serverName.ToLower();
					row["Port"] = csa.Port;
					row["ServerType"] = (int)csa.ServerType;

					row["Name"] = "";
					row["DatabaseLc"] = "";

					if (serverName == "localhost")
						row["Orderer"] = 2;
					else
						row["Orderer"] = 3;

					str = "AddAppServerRow: ";

					foreach (DataColumn col in databases.Columns)
						str += col.ColumnName + ": " + row[col.ColumnName] + ", ";

					// Tracer.Trace(typeof(ConnectionLocator), "RegisterAppConnectionStrings()", str);

					databases.Rows.Add(row);
				}

				csa.DataSource = serverName;

				// Tracer.Trace(typeof(ConnectionLocator), "RegisterAppConnectionStrings()", "Updated csb datasource: {0}, serverName: {1}, connectionstring: {2}.", datasource, serverName, csa.ConnectionString);

				csa = CsbAgent.CreateRegisteredDataset(datasetId, csa.ConnectionString);

				if (csa == null)
					continue;


				row = CreateConnectionNodeRow(databases, csa);

				row["Id"] = databases.Rows.Count;
				row["DataSourceLc"] = csa.DataSource.ToLower();
				row["Name"] = csa.DatasetId;
				row["DatabaseLc"] = csa.Database.ToLower();

				if (csa.DataSource.ToLowerInvariant() == "localhost")
					row["Orderer"] = 2;
				else
					row["Orderer"] = 3;

				str = "AddAppDbRow: ";

				foreach (DataColumn col in databases.Columns)
					str += col.ColumnName + ": " + row[col.ColumnName] + ", ";

				// Tracer.Trace(typeof(ConnectionLocator), "RegisterAppConnectionStrings()", str);

				databases.Rows.Add(row);

			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

	}


	public static void Reset()
	{
		lock (_LockClass)
		{
			_DataSources = null;
			_Databases = null;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if the app.config exeists and then executes a scan.
	/// </summary>
	/// <param name="project"></param>
	/// <returns>true if completed successfully else false if there were errors.</returns>
	// ---------------------------------------------------------------------------------
	private static bool ScanAppConfig(DataTable databases, ProjectItem appConfig)
	{
		try
		{
			if (appConfig.FileCount == 0)
				return false;

			string configFile = appConfig.FileNames[0];

			RegisterAppConnectionStrings(databases, appConfig.ContainingProject.Name, configFile);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}


		return true;
	}


	#endregion Methods





	// =========================================================================================================
	#region Utility Methods and Dictionaries - ConnectionLocator
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the descriptive name for a DTE object Kind guid string
	/// </summary>
	/// <param name="kind"></param>
	/// <returns>The descriptive name from <see cref="ProjectGuids"/></returns>
	// ---------------------------------------------------------------------------------
	protected static string Kind(string kind)
	{
		if (!ProjectGuids.TryGetValue(kind, out string name))
			name = kind;

		return name;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// DTE object Kind guid string descriptive name dictionary
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected static readonly Dictionary<string, string> ProjectGuids = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		{ "{2150E333-8FDC-42A3-9474-1A3956D46DE8}", "SolutionFolder" },
		{ "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}", "ProjectFolder" },
		{ "{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}", "PhysicalFolder" },
		{ "{66A2671F-8FB5-11D2-AA7E-00C04F688DDE}", "MiscItem" },
		{ "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}", "VbProject" },
		{ "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}", "C#Project" }
		/* The above the only one's we care about ATM
		{ "{06A35CCD-C46D-44D5-987B-CF40FF872267}", "DeploymentMergeModule" },
		{ "{14822709-B5A1-4724-98CA-57A101D1B079}", "Workflow(C#)" },
		{ "{20D4826A-C6FA-45DB-90F4-C717570B9F32}", "Legacy(2003)SmartDevice(C#)" },
		{ "{262852C6-CD72-467D-83FE-5EEB1973A190}", "JsProject" },
		{ "{2DF5C3F4-5A5F-47a9-8E94-23B4456F55E2}", "XNA(XBox)" },
		{ "{32F31D43-81CC-4C15-9DE6-3FC5453562B6}", "WorkflowFoundation" },
		{ "{349C5851-65DF-11DA-9384-00065B846F21}", "WebApplicationProject" },
		{ "{3AC096D0-A1C2-E12C-1390-A8335801FDAB}", "Test" },
		{ "{3AE79031-E1BC-11D0-8F78-00A0C9110057}", "SolutionExplorer"},
		{ "{3D9AD99F-2412-4246-B90B-4EAA41C64699}", "WCF" },
		{ "{3EA9E505-35AC-4774-B492-AD1749C4943A}", "DeploymentCab" },
		{ "{4B160523-D178-4405-B438-79FB67C8D499}", "NomadVSProject" },
		{ "{4D628B5B-2FBC-4AA6-8C16-197242AEB884}", "SmartDevice(C#)" },
		{ "{4F174C21-8C12-11D0-8340-0000F80270F8}", "Db(other" },
		{ "{54435603-DBB4-11D2-8724-00A0C9A8B90C}", "VS2015InstallerProjectExtension" },
		{ "{593B0543-81F6-4436-BA1E-4747859CAAE2}", "SharePoint(C#)" },
		{ "{603C0E0B-DB56-11DC-BE95-000D561079B0}", "ASP.NET(MVC 1.0)" },
		{ "{60DC8134-EBA5-43B8-BCC9-BB4BC16C2548}", "WPF" },
		{ "{68B1623D-7FB9-47D8-8664-7ECEA3297D4F}", "SmartDevice(VB.NET)" },
		{ "{66A2671D-8FB5-11D2-AA7E-00C04F688DDE}", "MiscProject" },
		{ "{66A26722-8FB5-11D2-AA7E-00C04F688DDE}", "SolutionItem" },
		{ "{67294A52-A4F0-11D2-AA88-00C04F688DDE}", "UnloadedProject" },
		{ "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}", "ProjectFile" },
		{ "{6BC8ED88-2882-458C-8E55-DFD12B67127B}", "MonoTouch" },
		{ "{6D335F3A-9D43-41b4-9D22-F6F17C4BE596}", "XNA(Win)" },
		{ "{76F1466A-8B6D-4E39-A767-685A06062A39}", "WinPhone8/8.1(Blank/Hub/Webview)" },
		{ "{786C830F-07A1-408B-BD7F-6EE04809D6DB}", "PortableClassLibrary" },
		{ "{8BB2217D-0F2D-49D1-97BC-3654ED321F3B}", "ASP.NET5" },
		{ "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}", "C++Project" },
		{ "{930C7802-8A8C-48F9-8165-68863BCCD9DD}", "WixProject" },
		{ "{978C614F-708E-4E1A-B201-565925725DBA}", "DeploymentSetup" },
		{ "{A1591282-1198-4647-A2B1-27E5FF5F6F3B}", "Silverlight" },
		{ "{A5A43C5B-DE2A-4C0C-9213-0A381AF9435A}", "UniversalWinClassLib" },
		{ "{A860303F-1F3F-4691-B57E-529FC101A107}", "VSToolsApplications(VSTA)" },
		{ "{A9ACE9BB-CECE-4E62-9AA4-C7E7C5BD2124}", "Database" },
		{ "{AB322303-2255-48EF-A496-5904EB18DA55}", "DeploymentSmartDeviceCab" },
		{ "{B69E3092-B931-443C-ABE7-7E7B65F2A37F}", "MicroFW" },
		{ "{BAA0C2D2-18E2-41B9-852F-F413020CAA33}", "VSToolsOffice(VSTO)" },
		{ "{BBD0F5D1-1CC4-42fd-BA4C-A96779C64378}", "SynergexProject" },
		{ "{BC8A1FFA-BEE3-4634-8014-F334798102B3}", "WinStoreProject" },
		{ "{BF6F8E12-879D-49E7-ADF0-5503146B24B8}", "C#Dynamics2012AX-AOT" },
		{ "{C089C8C0-30E0-4E22-80C0-CE093F111A43}", "WinPhone8/8.1App(C#)" },
		{ "{C252FEB5-A946-4202-B1D4-9916A0590387}", "VisualDbTools" },
		{ "{CB4CE8C6-1BDB-4DC7-A4D3-65A1999772F8}", "Legacy(2003)SmartDevice(VB.NET)" },
		{ "{D399B71A-8929-442a-A9AC-8BEC78BB2433}", "XNA(Zune)" },
		{ "{D59BE175-2ED0-4C54-BE3D-CDAA9F3214C8}", "Workflow(VB.NET)" },
		{ "{DB03555F-0C8B-43BE-9FF9-57896B3C5E56}", "WinPhone8/8.1App(VB.NET)" },
		{ "{E24C65DC-7377-472B-9ABA-BC803B73C61A}", "WebSiteProject" },
		{ "{E3E379DF-F4C6-4180-9B81-6769533ABE47}", "ASP.NET(MVC4.0)" },
		{ "{E53F8FEA-EAE0-44A6-8774-FFD645390401}", "ASP.NET(MVC3.0)" },
		{ "{E6FDF86B-F3D1-11D4-8576-0002A516ECE8}", "J#" },
		{ "{EC05E597-79D4-47f3-ADA0-324C4F7C7484}", "SharePoint(VB.NET)" },
		{ "{ECD6D718-D1CF-4119-97F3-97C25A0DFBF9}", "LightSwitchProject" },
		{ "{edcc3b85-0bad-11db-bc1a-00112fde8b61}", "NemerleProject" },
		{ "{EFBA0AD7-5A72-4C68-AF49-83D382785DCF}", "Xamarin.Android/MonoAndroid" },
		{ "{F135691A-BF7E-435D-8960-F99683D2D49C}", "DistributedSystem" },
		{ "{F2A71F9B-5D33-465A-A702-920D77279786}", "F#Project" },
		{ "{F5B4F3BC-B597-4E2B-B552-EF5D8A32436F}", "MonoTouchBinding" },
		{ "{F85E285D-A4E0-4152-9332-AB1D724D3325}", "ASP.NET(MVC2.0)" },
		{ "{F8810EC1-6754-47FC-A15F-DFABD2E3FA90}", "SharePointWorkflow" },
		{ "{FBB4BD86-BF63-432a-A6FB-6CF3A1288F83}", "InstallShieldLimitedEdition" },
		{ ".nuget", "SettingsNuget" }
		*/
	};


	#endregion Utility Methods and Dictionaries

}