// Microsoft.VisualStudio.Data.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Package.DataProvider
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.VisualStudio.Ddex.Ctl.Interfaces;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services.SupportEntities;


namespace BlackbirdSql.VisualStudio.Ddex.Ctl.DataTools;

public class TVsDataProvider : IVsDataProvider // , IVsDataInternalProvider, IVsDataInternalDataProviderDescriptionEx
{
	private delegate void GetEditionSupportDelegate();

	private class TiClientProviderObjectFactory : DataProviderObjectFactory, IVsDataSourceSpecializer, IVsDataSiteableObject<IVsDataSourceSpecializer>
	{
		private class TiiTypeActivator
		{
			private readonly TiClientProviderObjectFactory _Factory;

			private readonly Type _ObjType;

			private readonly string _CodeBase;

			private readonly string _Assembly;

			private readonly string _TypeName;

			private readonly string _XmlPath;

			private readonly string _XmlFileName;

			private readonly string _XmlCodeBase;

			private readonly string _XmlAssembly;

			private readonly string _XmlResourceName;

			public TiiTypeActivator(TiClientProviderObjectFactory factory, Type objType, string codeBase, string assembly, string typeName, string xmlPath, string xmlFileName, string xmlCodeBase, string xmlAssembly, string xmlResourceName)
			{
				_Factory = factory;
				_ObjType = objType;
				_CodeBase = codeBase;
				_Assembly = assembly;
				_TypeName = typeName;
				_XmlPath = xmlPath;
				_XmlFileName = xmlFileName;
				_XmlCodeBase = xmlCodeBase;
				_XmlAssembly = xmlAssembly;
				_XmlResourceName = xmlResourceName;
			}

			public object CreateInstance()
			{
				if (_TypeName == typeof(TConnectionUIProperties).FullName)
					return new TConnectionUIProperties();

				Assembly assembly = null;
				if (_CodeBase != null)
				{
					assembly = Hostess.LoadAssemblyFrom(_CodeBase);
				}
				else if (_Assembly != null)
				{
					assembly = _Factory.Site.GetAssembly(_Assembly);
				}
				Type type = !(assembly == null) ? Hostess.GetTypeFromAssembly(assembly, _TypeName) : _Factory.Site.GetType(_TypeName);
				if (type == null)
				{
					TypeAccessException ex = new($"Could not get Type for Factory Site {_Factory.Site.Name}, Object type {_ObjType.Name}, Type name {_TypeName}.");
					Diag.Dug(ex);
					throw ex;
				}
				object[] array = new object[0];
				if (_XmlFileName != null || _XmlResourceName != null)
				{
					if (_XmlFileName != null)
					{
						array = new object[2] { _XmlFileName, _XmlPath };
					}
					else
					{
						array = new object[2] { _XmlResourceName, null };
						Assembly assembly2 = ((_XmlCodeBase != null) ? Hostess.LoadAssemblyFrom(_XmlCodeBase) : ((_XmlAssembly == null) ? _Factory.Site.GetMainAssembly() : _Factory.Site.GetAssembly(_XmlAssembly)));
						if (assembly2 == null)
						{
							TypeAccessException ex = new($"Could not get XmlAssembly for Factory Site {_Factory.Site.Name}, Object type {_ObjType.Name}.");
							Diag.Dug(ex);
							throw ex;
						}
						array[1] = assembly2;
					}
				}
				object obj;
				try
				{
					obj = Hostess.CreateInstance(type, array);
				}
				catch (Exception innerException)
				{
					TypeAccessException ex = new($"Could create instance for Factory Site {_Factory.Site.Name}, Object type {_ObjType.Name}.");
					Diag.Dug(ex, innerException.Message);
					throw ex;
				}

				if (!_ObjType.IsInstanceOfType(obj))
				{
					Exception ex = new($"Object does not implement type for Factory site: {_Factory.Site.Name}, Object type: {_ObjType.Name}.");
					Diag.Dug(ex);
					throw ex;
				}
				return obj;
			}
		}

		private bool _GotAssociatedSource;

		private Guid _AssociatedSource;

		private IVsDataSourceSpecializer _SourceSpecializer;

		private readonly Microsoft.VisualStudio.Data.Core.IVsDataProviderObjectFactory _PackageProviderObjectFactory;

		private IDictionary<Guid, IDictionary<Type, TiiTypeActivator>> _TypeActivators;

		private bool _GotMainAssembly;

		private Assembly _MainAssembly;

		private readonly object _LockObject = new object();

		IVsDataSourceSpecializer IVsDataSiteableObject<IVsDataSourceSpecializer>.Site
		{
			get
			{
				return _SourceSpecializer;

			}
			set
			{
				lock (_LockObject)
				{
					_SourceSpecializer = value;
				}
			}
		}


		private Assembly MainAssembly
		{
			get
			{
				if (!_GotMainAssembly)
				{
					Assembly mainAssembly = GetMainAssembly();
					lock (_LockObject)
					{
						if (!_GotMainAssembly)
						{
							_MainAssembly = mainAssembly;
							_GotMainAssembly = true;
						}
					}
				}
				return _MainAssembly;
			}
		}

		public TiClientProviderObjectFactory()
			: this(null)
		{
		}

		public TiClientProviderObjectFactory(Microsoft.VisualStudio.Data.Core.IVsDataProviderObjectFactory packageProviderObjectFactory)
		{
			_PackageProviderObjectFactory = packageProviderObjectFactory;
		}

		public override object CreateObject(Type objType)
		{
			if (objType == null)
			{
				throw new ArgumentNullException("objType");
			}
			object obj = null;
			TiiTypeActivator typeActivator = GetTypeActivator(Guid.Empty, objType);
			if (typeActivator != null)
			{
				obj = typeActivator.CreateInstance();
			}
			if (obj == null && _PackageProviderObjectFactory != null)
			{
				try
				{
					obj = _PackageProviderObjectFactory.CreateObject(objType);
				}
				catch (NotImplementedException)
				{
				}
			}
			return obj;
		}

		public override Type GetType(string typeName)
		{
			if (typeName == null)
			{
				throw new ArgumentNullException("typeName");
			}
			Type type = null;
			if (typeName.IndexOf(',') == -1 && MainAssembly != null)
			{
				type = Hostess.GetTypeFromAssembly(MainAssembly, typeName);
			}
			if (type == null && _PackageProviderObjectFactory != null)
			{
				type = _PackageProviderObjectFactory.GetType(typeName);
			}
			if (type == null && typeName.IndexOf(',') != -1)
			{
				type = base.GetType(typeName);
			}
			if (type == null)
			{
				try
				{
					Guid classId = new Guid(typeName);
					type = Hostess.GetManagedTypeFromCLSID(classId);
				}
				catch (FormatException)
				{
				}
			}
			return type;
		}

		public override Assembly GetAssembly(string assemblyString)
		{
			if (assemblyString == null)
			{
				throw new ArgumentNullException("assemblyString");
			}
			Assembly assembly = null;
			if (assemblyString.Length == 0 || (MainAssembly != null && MainAssembly.GetName().ToString().Equals(new AssemblyName(assemblyString).ToString(), StringComparison.Ordinal)))
			{
				assembly = MainAssembly;
			}
			if (assembly == null && _PackageProviderObjectFactory != null)
			{
				assembly = _PackageProviderObjectFactory.GetAssembly(assemblyString);
			}
			if (assembly == null && assemblyString.Length > 0)
			{
				assembly = base.GetAssembly(assemblyString);
			}
			return assembly;
		}

		Guid IVsDataSourceSpecializer.DeriveSource(string connectionString)
		{
			if (_SourceSpecializer != null)
			{
				return _SourceSpecializer.DeriveSource(connectionString);
			}
			if (!_GotAssociatedSource)
			{
				Guid associatedSource = Guid.Empty;
				if (base.Site.GetProperty("AssociatedSource") is string g)
				{
					try
					{
						associatedSource = new Guid(g);
					}
					catch (FormatException)
					{
					}
				}
				lock (_LockObject)
				{
					if (!_GotAssociatedSource)
					{
						_AssociatedSource = associatedSource;
						_GotAssociatedSource = true;
					}
				}
			}
			return _AssociatedSource;
		}

		object IVsDataSourceSpecializer.CreateObject(Guid clsid, Type objType)
		{
			if (objType == null)
			{
				throw new ArgumentNullException("objType");
			}
			object obj = null;
			TiiTypeActivator typeActivator = GetTypeActivator(clsid, objType);
			if (typeActivator != null)
			{
				obj = typeActivator.CreateInstance();
			}
			if (obj == null && _SourceSpecializer != null)
			{
				obj = _SourceSpecializer.CreateObject(clsid, objType);
			}
			return obj;
		}

		Type IVsDataSourceSpecializer.GetType(Guid clsid, string typeName)
		{
			if (_SourceSpecializer != null)
			{
				return _SourceSpecializer.GetType(clsid, typeName);
			}
			return null;
		}

		Assembly IVsDataSourceSpecializer.GetAssembly(Guid clsid, string assemblyString)
		{
			if (_SourceSpecializer != null)
			{
				return _SourceSpecializer.GetAssembly(clsid, assemblyString);
			}
			return null;
		}

		private Assembly GetMainAssembly()
		{
			Assembly assembly = null;
			Exception ex = null;
			if (base.Site.GetProperty("CodeBase") is string fileName)
			{
				try
				{
					throw new Exception($"Unable to load assembly: {fileName}.");
					// assembly = Host.System.LoadAssemblyFrom(fileName);
				}
				catch (Exception ex2)
				{
					ex = ex2;
				}
			}
			if (assembly == null && base.Site.GetProperty("Assembly") is string assemblyString)
			{
				try
				{
					throw new Exception($"Unable to load assembly: {assemblyString}.");
					// assembly = Hostess.LoadAssembly(assemblyString);
				}
				catch (Exception ex3)
				{
					ex = ex3;
				}
			}
			if (ex != null && _PackageProviderObjectFactory == null)
			{
				Diag.Dug(ex);
				throw ex;
				// (DataPackage.ProviderManager as IVsDataInternalProviderManager).RemoveProvider(base.Site);
				// throw new Microsoft.VisualStudio.Data.Package.DataProviderException(base.Site.Name, string.Format(null, Resources.DataProvider_FailedToLoad, base.Site.Name, ex.Message), ex);
			}
			return assembly;
		}

		private TiiTypeActivator GetTypeActivator(Guid clsid, Type objType)
		{
			if (_TypeActivators == null)
			{
				lock (_LockObject)
				{
					_TypeActivators ??= new Dictionary<Guid, IDictionary<Type, TiiTypeActivator>>();
				}
			}
			if (!_TypeActivators.ContainsKey(clsid))
			{
				lock (_LockObject)
				{
					if (!_TypeActivators.ContainsKey(clsid))
					{
						_TypeActivators.Add(clsid, new Dictionary<Type, TiiTypeActivator>());
					}
				}
			}
			if (!_TypeActivators[clsid].ContainsKey(objType))
			{
				TiiTypeActivator typeActivatorImpl = GetTypeActivatorImpl(clsid, objType);
				lock (_LockObject)
				{
					if (!_TypeActivators[clsid].ContainsKey(objType))
					{
						_TypeActivators[clsid].Add(objType, typeActivatorImpl);
					}
				}
			}
			return _TypeActivators[clsid][objType];
		}

		private TiiTypeActivator GetTypeActivatorImpl(Guid clsid, Type objType)
		{
			if (clsid == TDataSource.FbDataSource.NameClsid)
			{
				if (objType == typeof(IVsDataConnectionUIProperties)
					|| objType == typeof(Microsoft.Data.ConnectionUI.IDataConnectionProperties))
				{
					return new TiiTypeActivator(this, objType, GetType().Assembly.CodeBase,
						GetType().Assembly.FullName, typeof(TConnectionUIProperties).FullName,
						null, null, null, null, null);
				}
			}

			// Tracer.Trace(GetType(), "GetTypeActivatorImpl()", "Activator not implemented for guid: {0}, object type: {1}.", clsid, objType.FullName);

			return null;
		}
	}


	private Guid _Guid;

	private string _Name;

	private string _DisplayName;

	private string _ShortDisplayName;

	private string _Description;

	// private readonly string _DescriptionEx;

	private bool _GotTechnology;

	private Guid _Technology;

	// private Guid _PackageGuid;

	private IDictionary<string, object> _Properties;

	private IDictionary<Guid, List<string>> _SupportedObjectTypes;

	private bool _GotSourceSpecializer;

	private bool _GotProviderDynamicSupport;

	private IVsDataProviderDynamicSupport _ProviderDynamicSupport;

	private Microsoft.VisualStudio.Data.Core.IVsDataProviderObjectFactory _ProviderObjectFactory;

	// private static NativeMethods.IVsDataEditionSupport _EditionSupport;

	private readonly object _LockObject = new object();


	private Guid CLSID_CompatibleProviderObjectFactory => new Guid("9DBACAC7-8589-4c46-AE40-4B4FDE40DCDA");


	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public Guid Guid => _Guid;


	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public string Name
	{
		get
		{
			if (_Name == null)
			{
				string text = GetProperty(string.Empty) as string;
				text ??= Guid.ToString("B", CultureInfo.InvariantCulture);
				lock (_LockObject)
				{
					_Name ??= text;
				}
			}
			return _Name;
		}
	}


	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public string DisplayName
	{
		get
		{
			if (_DisplayName == null)
			{
				string text = null;
				if (GetProperty("DisplayName") is string resourceId)
				{
					text = resourceId;
				}
				text ??= Name;
				lock (_LockObject)
				{
					_DisplayName ??= text;
				}
			}
			return _DisplayName;
		}
	}


	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public string ShortDisplayName
	{
		get
		{
			if (_ShortDisplayName == null)
			{
				string text = null;
				if (GetProperty("ShortDisplayName") is string resourceId)
				{
					text = resourceId;
				}
				text ??= DisplayName;
				lock (_LockObject)
				{
					_ShortDisplayName ??= text;
				}
			}
			return _ShortDisplayName;
		}
	}

	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public string Description
	{
		get
		{
			if (_Description == null)
			{
				string text = null;
				if (GetProperty("Description") is string resourceId)
				{
					text = resourceId;
				}
				text ??= string.Empty;
				lock (_LockObject)
				{
					_Description ??= text;
				}
			}
			if (_Description.Length <= 0)
			{
				return null;
			}
			return _Description;
		}
	}


	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public Guid Technology
	{
		get
		{
			if (!_GotTechnology)
			{
				Guid technology = Guid.Empty;
				if (GetProperty("Technology") is string g)
				{
					try
					{
						technology = new Guid(g);
					}
					catch (FormatException)
					{
						technology = Guid.Empty;
					}
				}
				lock (_LockObject)
				{
					if (!_GotTechnology)
					{
						_Technology = technology;
						_GotTechnology = true;
					}
				}
			}
			return _Technology;
		}
	}


	private bool IsLegacyProvider
	{
		get
		{
			if (GetProperty("PlatformVersion") is string text)
			{
				return text.Trim().Equals("1.0", StringComparison.Ordinal);
			}
			return true;
		}
	}

	private IVsDataSourceSpecializer SourceSpecializer
	{
		get
		{
			if (!_GotSourceSpecializer)
			{
				IVsDataSourceSpecializer vsDataSourceSpecializer = null;
				if (SupportsObject(typeof(IVsDataSourceSpecializer)))
				{
					vsDataSourceSpecializer = TryCreateObject<IVsDataSourceSpecializer>();
				}
				lock (_LockObject)
				{
					if (!_GotSourceSpecializer)
					{
						if (vsDataSourceSpecializer != null)
						{
							(ProviderObjectFactory as IVsDataSiteableObject<IVsDataSourceSpecializer>).Site = vsDataSourceSpecializer;
						}
						_GotSourceSpecializer = true;
					}
				}
			}
			return ProviderObjectFactory as IVsDataSourceSpecializer;
		}
	}

	private IVsDataProviderDynamicSupport ProviderDynamicSupport
	{
		get
		{
			if (!_GotProviderDynamicSupport)
			{
				IVsDataProviderDynamicSupport providerDynamicSupport = null;
				if (SupportsObject(typeof(IVsDataProviderDynamicSupport)))
				{
					providerDynamicSupport = TryCreateObject<IVsDataProviderDynamicSupport>();
				}
				lock (_LockObject)
				{
					if (!_GotProviderDynamicSupport)
					{
						_ProviderDynamicSupport = providerDynamicSupport;
						_GotProviderDynamicSupport = true;
					}
				}
			}
			return _ProviderDynamicSupport;
		}
	}

	private Microsoft.VisualStudio.Data.Core.IVsDataProviderObjectFactory ProviderObjectFactory
	{
		get
		{
			if (_ProviderObjectFactory == null)
			{
				Core.Controller.GetService<IVsDataProviderManager>();
				IVsDataProviderObjectFactory vsDataProviderObjectFactory;
				if (IsLegacyProvider)
				{
					vsDataProviderObjectFactory = Hostess.CreateManagedInstance(CLSID_CompatibleProviderObjectFactory) as Microsoft.VisualStudio.Data.Core.IVsDataProviderObjectFactory;
					(vsDataProviderObjectFactory as IVsDataSiteableObject<Microsoft.VisualStudio.Data.Core.IVsDataProvider>).Site = this;
					lock (_LockObject)
					{
						_ProviderObjectFactory ??= vsDataProviderObjectFactory;
					}
					return _ProviderObjectFactory;
				}
				if (GetProperty("FactoryService") is not string g)
				{
					vsDataProviderObjectFactory = Hostess.CreateInstance(typeof(TiClientProviderObjectFactory)) as Microsoft.VisualStudio.Data.Core.IVsDataProviderObjectFactory;
				}
				else
				{
					try
					{
						Guid empty = new Guid(g);
					}
					catch (FormatException ex)
					{
						Diag.Dug(ex, $"Failed to load: {Name}.");
						throw ex;
						// (DataPackage.ProviderManager as IVsDataInternalProviderManager).RemoveProvider(this);
						// throw new Microsoft.VisualStudio.Data.Package.DataProviderException(Name, string.Format(null, Resources.DataProvider_FailedToLoad, Name, ex.Message), ex);
					}
					try
					{
						vsDataProviderObjectFactory = Core.Controller.GetService<IVsDataProviderObjectFactory>();
					}
					catch (Exception ex2)
					{
						Diag.Dug(ex2, $"Failed to load: {Name}.");
						throw ex2;
						// (DataPackage.ProviderManager as IVsDataInternalProviderManager).RemoveProvider(this);
						// throw new Microsoft.VisualStudio.Data.Package.DataProviderException(Name, string.Format(null, Resources.DataProvider_FailedToLoad, Name, ex2.Message), ex2);
					}
					vsDataProviderObjectFactory = Hostess.CreateInstance(typeof(TiClientProviderObjectFactory), vsDataProviderObjectFactory)
						as IVsDataProviderObjectFactory;
				}
				(vsDataProviderObjectFactory as IVsDataSiteableObject<IVsDataProvider>).Site = this;
				lock (_LockObject)
				{
					_ProviderObjectFactory ??= vsDataProviderObjectFactory;
				}
			}
			return _ProviderObjectFactory;
		}
	}

	/*
	private NativeMethods.IVsDataEditionSupport EditionSupport
	{
		get
		{
			if (_EditionSupport == null)
			{
				Host.Environment.DangerousInvokeOnUIThread(new GetEditionSupportDelegate(GetEditionSupport));
			}
			return _EditionSupport;
		}
	}
	*/

	public TVsDataProvider(Guid clsid)
	{
		_Guid = clsid;
	}


	public object CreateRawObject<TSite>(Guid clsid, Type objType, TSite site)
	{
		object obj = null;
		if (IsLegacyProvider && objType == typeof(IVsDataViewSupport))
		{
			if ((object)site is IVsDataViewHierarchy vsDataViewHierarchy)
			{
				obj = vsDataViewHierarchy.ExplorerConnection.Connection.GetService(typeof(IVsDataViewSupport));
			}
			if (obj != null)
			{
				return obj;
			}
		}
		if (clsid != Guid.Empty)
		{
			obj = SourceSpecializer.CreateObject(clsid, objType);
			if (!objType.IsInstanceOfType(obj))
			{
				obj = null;
			}
		}
		if (obj == null)
		{
			obj = ProviderObjectFactory.CreateObject(objType);
			if (!objType.IsInstanceOfType(obj))
			{
				obj = null;
			}
		}
		if (!IsLegacyProvider && obj == null)
		{
			try
			{
				obj = typeof(DataDefaultObject).GetMethod("Create").MakeGenericMethod(objType).Invoke(null, new object[1] { Core.Controller.ServiceProvider });
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
		}
		if (obj != null && obj is IVsDataSiteableObject<IServiceProvider> vsDataSiteableObject)
		{
			vsDataSiteableObject.Site = Core.Controller.ServiceProvider;
		}
		if (obj != null && obj is IVsDataSiteableObject<IVsDataProvider> vsDataSiteableObject2)
		{
			vsDataSiteableObject2.Site = this;
		}
		if (obj != null && clsid != Guid.Empty && obj is IVsDataSiteableObject<IVsDataSource> vsDataSiteableObject3)
		{

			IVsDataSource value = null;
			// DataPackage.SourceManager.Sources.TryGetValue(clsid, out value);
			if (value != null)
			{
				vsDataSiteableObject3.Site = value;
			}
			TargetException ex = new($"Could not get Site from DataPackage.SourceManager.Sources for type {obj.GetType().FullName}.");
			Diag.Dug(ex);
		}
		if (site != null && obj is IVsDataSiteableObject<TSite> vsDataSiteableObject4)
		{
			vsDataSiteableObject4.Site = site;
		}
		return obj;
	}



	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public object GetProperty(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (_Properties == null)
		{
			lock (_LockObject)
			{
				_Properties ??= new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			}
		}
		if (!_Properties.ContainsKey(name))
		{
			object propertyFromRegistry = GetPropertyFromRegistry(name, _Guid);
			lock (_LockObject)
			{
				if (!_Properties.ContainsKey(name))
				{
					_Properties[name] = propertyFromRegistry;
				}
			}
		}
		if (_Properties[name] == DBNull.Value)
		{
			return null;
		}
		return _Properties[name];
	}

	private static object GetPropertyFromRegistry(string name, Guid guid)
	{
		if (guid == TDataProvider.BlackbirdSqlDataProvider.NameClsid)
		{
			switch (name)
			{
				case null:
				case "":
				case "DisplayName":
					return TDataProvider.BlackbirdSqlDataProvider.DisplayName;
				case "ShortDisplayName":
					return TDataProvider.BlackbirdSqlDataProvider.ShortDisplayName;
				case "PlatformVersion":
					return "2.0";
				case "FactoryService":
					return $"{{{(Attribute.GetCustomAttribute(typeof(IBProviderObjectFactory), typeof(GuidAttribute)) as GuidAttribute).Value}}}";
				default:
					break;


			}
		}
		Exception ex = new($"Failed to get property {name} from registry for guid {guid}.");
		Diag.Dug(ex);
		return DBNull.Value;
		/*
		object obj = null;
		Registry.Key key = Host.Environment.LocalRegistry.OpenKey("DataProviders\\" + guid.ToString("B", CultureInfo.InvariantCulture));
		if (key != null)
		{
			using (key)
			{
				obj = key.GetValue(name);
			}
		}
		if (obj == null)
		{
			obj = DBNull.Value;
		}
		return obj;
		*/
	}



	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public string GetString(string resourceId)
	{
		if (resourceId == null)
		{
			throw new ArgumentNullException("resourceId");
		}
		if (resourceId.Length == 0)
		{
			return null;
		}

		// Tracer.Trace(GetType(), "GetString(resourceId)", "resourceId: {0}", resourceId);

		return $"rescourceId: {resourceId}.";

		/*
		if (!resourceId.Trim().StartsWith("#", StringComparison.Ordinal))
		{
			string name = resourceId;
			string text = string.Empty;
			string assemblyString = string.Empty;
			int num = 0;
			int num2 = resourceId.IndexOf(',', num);
			if (num2 > 0)
			{
				name = resourceId.Substring(num, num2 - num).Trim();
				num = num2 + 1;
				num2 = resourceId.IndexOf(',', num);
				if (num2 == -1)
				{
					text = resourceId.Substring(num).Trim();
					assemblyString = string.Empty;
				}
				else
				{
					text = resourceId.Substring(num, num2 - num).Trim();
					num = num2 + 1;
					assemblyString = resourceId.Substring(num).Trim();
				}
			}
			if (text.Length == 0)
			{
				return null;
			}
			Assembly assembly = GetAssembly(assemblyString);
			if (assembly == null)
			{
				return null;
			}
			return Host.System.GetString(name, text, assembly);
		}
		uint result = 0u;
		if (!uint.TryParse(resourceId.TrimStart('#'), NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
		{
			return null;
		}
		if (_PackageGuid == Guid.Empty)
		{
			if (!(GetProperty("FactoryService") is string text2))
			{
				lock (_LockObject)
				{
					_PackageGuid = NativeMethods.IID_IUnknown;
				}
				return null;
			}
			Registry.Key key = Host.Environment.LocalRegistry.OpenKey("Services\\" + text2);
			if (key == null)
			{
				lock (_LockObject)
				{
					_PackageGuid = NativeMethods.IID_IUnknown;
				}
				return null;
			}
			string text3 = null;
			using (key)
			{
				text3 = key.GetValue(null) as string;
			}
			if (text3 == null)
			{
				lock (_LockObject)
				{
					_PackageGuid = NativeMethods.IID_IUnknown;
				}
				return null;
			}
			Guid empty = Guid.Empty;
			try
			{
				empty = new Guid(text3);
			}
			catch (FormatException)
			{
				lock (_LockObject)
				{
					_PackageGuid = NativeMethods.IID_IUnknown;
				}
				return null;
			}
			lock (_LockObject)
			{
				if (_PackageGuid == Guid.Empty)
				{
					_PackageGuid = empty;
				}
			}
		}
		if (_PackageGuid == NativeMethods.IID_IUnknown)
		{
			return null;
		}
		return Host.Environment.DangerousGetString(_PackageGuid, (int)result);
		*/
	}



	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public Guid DeriveSource(string connectionString)
	{
		return SourceSpecializer.DeriveSource(connectionString);
	}



	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public bool SupportsObject(Type objType)
	{
		return SupportsObject(Guid.Empty, objType);
	}


	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public bool SupportsObject(Guid clsid, Type objType)
	{
		if (objType == null)
		{
			throw new ArgumentNullException("objType");
		}
		if (_SupportedObjectTypes == null)
		{
			IDictionary<Guid, List<string>> supportedObjectTypes = GetSupportedObjectTypes(_Guid);
			lock (_LockObject)
			{
				_SupportedObjectTypes ??= supportedObjectTypes;
			}
		}
		lock (_LockObject)
		{
			return SupportsObject(clsid, objType, ref _SupportedObjectTypes, IsLegacyProvider);
		}
	}


	private static IDictionary<Guid, List<string>> GetSupportedObjectTypes(Guid providerGuid)
	{
		IDictionary<Guid, List<string>> dictionary = new Dictionary<Guid, List<string>>
		{
			{ Guid.Empty, new List<string>() }
		};

		// Only BlackbirdSql
		if (providerGuid != TDataProvider.BlackbirdSqlDataProvider.NameClsid)
		{
			NotSupportedException ex = new($"Provider not supported: {providerGuid}.");
			Diag.Dug(ex);
			throw ex;
		}

		foreach (KeyValuePair<string, int> implementation in PackageSupportedObjects.Implementations)
		{
			if (implementation.Value == 0)
				continue;
			AddSupportedObjectType(dictionary, implementation.Value, implementation.Key, IsProviderLegacy(providerGuid));
		}

		return dictionary;

		/*
		IDictionary<Guid, List<string>> dictionary = new Dictionary<Guid, List<string>>();
		dictionary.Add(Guid.Empty, new List<string>());

		Registry.Key key = Host.Environment.LocalRegistry.OpenKey("DataProviders\\" + providerGuid.ToString("B", CultureInfo.InvariantCulture) + "\\SupportedObjects");
		if (key != null)
		{
			string[] array = null;
			using (key)
			{
				array = key.GetSubKeyNames();
				string[] array2 = array;
				foreach (string typeName in array2)
				{
					AddSupportedObjectType(dictionary, key, typeName, IsProviderLegacy(providerGuid));
				}
			}
		}
		return dictionary;
		*/
	}

	private static bool SupportsObject(Guid clsid, Type objType, ref IDictionary<Guid, List<string>> supportedObjectTypes, bool isLegacyProvider)
	{
		if (!isLegacyProvider && !supportedObjectTypes[Guid.Empty].Contains(objType.FullName))
		{
			object[] customAttributes = objType.GetCustomAttributes(typeof(DataDefaultObjectAttribute), inherit: true);
			if (customAttributes.Length != 0)
			{
				supportedObjectTypes[Guid.Empty].Add(objType.FullName);
			}
		}
		if (isLegacyProvider && (objType == typeof(IVsDataConnectionUIProperties) || objType == typeof(IVsDataConnectionEquivalencyComparer) || objType == typeof(IVsDataConnectionUITester)))
		{
			objType = typeof(Microsoft.VisualStudio.Data.Services.SupportEntities.IVsDataConnectionProperties);
		}
		if (clsid != Guid.Empty && supportedObjectTypes.ContainsKey(clsid) && supportedObjectTypes[clsid].Contains(objType.FullName))
		{
			return true;
		}
		return supportedObjectTypes[Guid.Empty].Contains(objType.FullName);
	}


	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public TObject TryCreateObject<TObject>()
	{
		return TryCreateObject<TObject>(Guid.Empty);
	}



	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public TObject TryCreateObject<TObject>(Guid clsid)
	{
		return TryCreateObject<TObject, object>(clsid, null);
	}



	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public TObject TryCreateObject<TObject, TSite>(Guid clsid, TSite site)
	{
		object obj = TryCreateObject(clsid, typeof(TObject), site);
		if (obj is TObject @object)
		{
			return @object;
		}
		return default;
	}



	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public object TryCreateObject(Type objType)
	{
		return TryCreateObject(Guid.Empty, objType);
	}



	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public object TryCreateObject(Guid clsid, Type objType)
	{
		return TryCreateObject<object>(clsid, objType, null);
	}



	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public object TryCreateObject<TSite>(Guid clsid, Type objType, TSite site)
	{
		if (objType == null)
		{
			throw new ArgumentNullException("objType");
		}
		object obj = CreateRawObject(clsid, objType, site);
		if (obj != null)
		{
			try
			{
				object obj2 = typeof(DataClientObject<>).MakeGenericType(objType).GetMethod("Create", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[2]
				{
					Core.Controller.ServiceProvider,
					obj
				});
				if (obj2 != obj && site != null && obj is IVsDataSiteableObject<TSite> vsDataSiteableObject)
				{
					vsDataSiteableObject.Site = site;
				}
				obj = obj2;
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
		}
		return obj;
	}

	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public TObject CreateObject<TObject>()
	{
		return CreateObject<TObject>(Guid.Empty);
	}



	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public TObject CreateObject<TObject>(Guid clsid)
	{
		return CreateObject<TObject, object>(clsid, null);
	}



	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public TObject CreateObject<TObject, TSite>(Guid clsid, TSite site)
	{
		return (TObject)CreateObject(clsid, typeof(TObject), site);
	}



	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public object CreateObject(Type objType)
	{
		return CreateObject(Guid.Empty, objType);
	}



	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public object CreateObject(Guid clsid, Type objType)
	{
		return CreateObject<object>(clsid, objType, null);
	}



	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public object CreateObject<TSite>(Guid clsid, Type objType, TSite site)
	{
		object obj = TryCreateObject(clsid, objType, site);
		if (obj == null)
		{
			NotImplementedException ex = new($"{DisplayName}: Object type: {objType.Name}.");
			Diag.Dug(ex);
			throw ex;
			// throw new Microsoft.VisualStudio.Data.Package.DataProviderException(DisplayName, string.Format(null, Resources.DataProvider_NoImplementation, objType.Name));
		}
		return obj;
	}

	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public Type GetType(string typeName)
	{
		return GetType(Guid.Empty, typeName);
	}



	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public Type GetType(Guid clsid, string typeName)
	{
		if (typeName == null)
		{
			throw new ArgumentNullException("typeName");
		}
		Type type = null;
		if (clsid != Guid.Empty)
		{
			type = SourceSpecializer.GetType(clsid, typeName);
		}
		if (type == null)
		{
			type = ProviderObjectFactory.GetType(typeName);
		}
		return type;
	}



	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public Assembly GetMainAssembly()
	{
		return GetAssembly(string.Empty);
	}



	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public Assembly GetAssembly(string assemblyString)
	{
		return GetAssembly(Guid.Empty, assemblyString);
	}



	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public Assembly GetAssembly(Guid clsid, string assemblyString)
	{
		if (assemblyString == null)
		{
			throw new ArgumentNullException("assemblyString");
		}
		Assembly assembly = null;
		if (clsid != Guid.Empty)
		{
			assembly = SourceSpecializer.GetAssembly(clsid, assemblyString);
		}
		if (assembly == null)
		{
			assembly = ProviderObjectFactory.GetAssembly(assemblyString);
		}
		return assembly;
	}



	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public bool IsOperationSupported(CommandID command, object context)
	{
		return IsOperationSupported(Guid.Empty, command, context);
	}



	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public bool IsOperationSupported(Guid clsid, CommandID command, object context)
	{
		if (command == null)
		{
			ArgumentNullException ex = new("command");
			Diag.Dug(ex);
			throw ex;
		}

		InvalidOperationException ex2 = new($"Unable to complete IsOperationSupported() for Guid: {clsid}, command: {command.ID}, context: {context}.");
		Diag.Dug(ex2);

		return false;

		/*
		Guid guidCmdGroup = command.Guid;
		bool flag = EditionSupport.IsOperationSupported(ref _Guid, ref clsid, ref guidCmdGroup, (uint)command.ID, context);
		if (flag && ProviderDynamicSupport != null)
		{
			flag = ProviderDynamicSupport.IsOperationSupported(clsid, command, context);
		}
		return flag;
		*/
	}



	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public string GetUnsupportedReason(CommandID command, object context)
	{
		return $"Could not get IsOperationSupported for command {command.ID}, context: {context}.";
		// return GetUnsupportedReason(Guid.Empty, command, context);
	}



	/// <summary>
	/// IVsDataprovider implementation
	/// </summary>
	public string GetUnsupportedReason(Guid clsid, CommandID command, object context)
	{
		if (command == null)
		{
			throw new ArgumentNullException("command");
		}
		Guid guidCmdGroup = command.Guid;
		string unsupportedReason = $"EditionSupport GetUnsupportedReason() _Guid: {_Guid}, clsid Guid: {clsid}, guidCmdGroup: {guidCmdGroup}, command.ID: {command.ID}, context: {context}.";
		// string unsupportedReason = EditionSupport.GetUnsupportedReason(ref _Guid, ref clsid, ref guidCmdGroup, (uint)command.ID, context);
		if (unsupportedReason == null && ProviderDynamicSupport != null)
		{
			unsupportedReason = ProviderDynamicSupport.GetUnsupportedReason(clsid, command, context);
		}
		return unsupportedReason;
	}



	internal static bool IsProviderLegacy(Guid guid)
	{
		return false;
		/*
		if (GetPropertyFromRegistry("PlatformVersion", guid) is string text)
		{
			return text.Trim().Equals("1.0", StringComparison.Ordinal);
		}
		return true;
		*/
	}

	/*
	private void GetEditionSupport()
	{
		if (_EditionSupport != null)
		{
			return;
		}
		NativeMethods.IVsDataEditionSupport vsDataEditionSupport = Host.TryGetService<NativeMethods.IVsDataEditionSupport>(NativeMethods.SID_SVsAppId);
		vsDataEditionSupport = ((vsDataEditionSupport == null) ? DataPackage.EditionSupport : new TiDataEditionSupport(vsDataEditionSupport));
		lock (_LockObject)
		{
			if (_EditionSupport == null)
			{
				_EditionSupport = vsDataEditionSupport;
			}
		}
	}
	*/

	private static void AddSupportedObjectType(IDictionary<Guid, List<string>> supportedObjectTypes, int subKeyCount, string typeName, bool isLegacyProvider)
	{
		if (typeName.Length == 0 || typeName.Trim() != typeName)
			return;

		string item = typeName;
		if (typeName.IndexOf('.') == -1)
		{
			if (!isLegacyProvider)
			{
				item = ((typeName == typeof(IVsDataProviderDynamicSupport).Name)
					? typeof(IVsDataProviderDynamicSupport).FullName
					: ((!(typeName == typeof(IVsDataSourceSpecializer).Name))
						? (typeof(IVsDataCommand).Namespace + "." + typeName)
						: typeof(IVsDataSourceSpecializer).FullName));
			}
			else
			{
				Type type = ((!(typeName == "DataSourceSpecializer")) ? typeof(Microsoft.VisualStudio.Data.Services.SupportEntities.IVsDataCommand).Assembly.GetType(typeof(Microsoft.VisualStudio.Data.Services.SupportEntities.IVsDataCommand).Namespace + ".IVs" + typeName, throwOnError: false) : typeof(IVsDataSourceSpecializer));
				if (type != null)
				{
					item = type.FullName;
				}
			}
		}


		string[] array = null;

		if (subKeyCount > 1)
		{
			array = new string[subKeyCount-1];
			for (int i = 1; i < subKeyCount; i++)
				array[i-1] = PackageSupportedObjects.Values[$"{typeName}:{i}"].Name ?? string.Empty;
		}
		if (array == null || array.Length == 0)
		{
			supportedObjectTypes[Guid.Empty].Add(item);
			return;
		}

		string[] array2 = array;
		foreach (string g in array2)
		{
			Guid empty;
			try
			{
				empty = new Guid(g);
			}
			catch (FormatException)
			{
				continue;
			}
			if (!supportedObjectTypes.ContainsKey(empty))
			{
				supportedObjectTypes.Add(empty, []);
			}
			supportedObjectTypes[empty].Add(item);
		}

		return;



		/*

		if (typeName.Length == 0 || typeName.Trim() != typeName)
		{
			return;
		}
		string item = typeName;
		if (typeName.IndexOf('.') == -1)
		{
			if (!isLegacyProvider)
			{
				item = ((typeName == typeof(IVsDataProviderDynamicSupport).Name)
					? typeof(IVsDataProviderDynamicSupport).FullName
					: ((!(typeName == typeof(IVsDataSourceSpecializer).Name))
						? (typeof(IVsDataCommand).Namespace + "." + typeName)
						: typeof(IVsDataSourceSpecializer).FullName));
			}
			else
			{
				Type type = null;
				type = ((!(typeName == "DataSourceSpecializer")) ? typeof(IVsDataCommand).Assembly.GetType(typeof(IVsDataCommand).Namespace + ".IVs" + typeName, throwOnError: false) : typeof(IVsDataSourceSpecializer));
				if (type != null)
				{
					item = type.FullName;
				}
			}
		}

		Registry.Key key2 = key.OpenSubKey(typeName);
		string[] arrayT = null;
		using (key2)
		{
			array = key2.GetSubKeyNames();
		}
		if (array.Length == 0)
		{
			supportedObjectTypes[Guid.Empty].Add(item);
			return;
		}
		string[] array2T = array;
		foreach (string g in array2)
		{
			Guid empty = Guid.Empty;
			try
			{
				empty = new Guid(g);
			}
			catch (FormatException)
			{
				continue;
			}
			if (!supportedObjectTypes.ContainsKey(empty))
			{
				supportedObjectTypes.Add(empty, new List<string>());
			}
			supportedObjectTypes[empty].Add(item);
		}
		*/
	}
}
