// Microsoft.Data.ConnectionUI.Dialog, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.ConnectionUI.DataProvider
using System;
using System.Collections.Generic;
using BlackbirdSql.Core;
using BlackbirdSql.VisualStudio.Ddex.Controls;
using BlackbirdSql.VisualStudio.Ddex.Properties;
using Microsoft.Data.ConnectionUI;


namespace BlackbirdSql.VisualStudio.Ddex.Ctl.DataTools;

// =========================================================================================================
//
//											TDataProvider Class
//
// =========================================================================================================
public class TDataProvider
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - TDataProvider
	// ---------------------------------------------------------------------------------


	public TDataProvider(string name, string displayName, string shortDisplayName, string description,
		Type targetConnectionType)
	{
		_Name = name ?? throw new ArgumentNullException("nameGuid");

		_DisplayName = displayName;

		_ShortDisplayName = shortDisplayName;

		_Description = description;

		_TargetConnectionType = targetConnectionType;
	}



	public TDataProvider(string name, string displayName, string shortDisplayName)
	: this(name, displayName, shortDisplayName, null, null)
	{
	}


	public TDataProvider(string name, string displayName, string shortDisplayName, string description)
		: this(name, displayName, shortDisplayName, description, null)
	{
	}


	public TDataProvider(string name, string displayName, string shortDisplayName, string description, Type targetConnectionType, Type connectionPropertiesType)
		: this(name, displayName, shortDisplayName, description, targetConnectionType)
	{
		if (connectionPropertiesType == null)
		{
			throw new ArgumentNullException("connectionPropertiesType");
		}
		_ConnectionPropertiesTypes = new Dictionary<string, Type>
		{
			{ "", connectionPropertiesType }
		};
	}


	public TDataProvider(string name, string displayName, string shortDisplayName, string description, Type targetConnectionType, Type connectionUIControlType, Type connectionPropertiesType)
		: this(name, displayName, shortDisplayName, description, targetConnectionType, connectionPropertiesType)
	{
		if (connectionUIControlType == null)
		{
			throw new ArgumentNullException("connectionUIControlType");
		}
		_ConnectionUIControlTypes = new Dictionary<string, Type>
		{
			{ "", connectionUIControlType }
		};
	}


	public TDataProvider(string name, string displayName, string shortDisplayName, string description, Type targetConnectionType, IDictionary<string, Type> connectionUIControlTypes, Type connectionPropertiesType)
		: this(name, displayName, shortDisplayName, description, targetConnectionType, connectionPropertiesType)
	{
		_ConnectionUIControlTypes = connectionUIControlTypes;
	}


	public TDataProvider(string name, string displayName, string shortDisplayName, string description, Type targetConnectionType, IDictionary<string, string> dataSourceDescriptions, IDictionary<string, Type> connectionUIControlTypes, Type connectionPropertiesType)
		: this(name, displayName, shortDisplayName, description, targetConnectionType, connectionUIControlTypes, connectionPropertiesType)
	{
		_DataSourceDescriptions = dataSourceDescriptions;
	}


	public TDataProvider(string name, string displayName, string shortDisplayName, string description,
			Type targetConnectionType, IDictionary<string, string> dataSourceDescriptions,
			IDictionary<string, Type> connectionUIControlTypes, IDictionary<string, Type> connectionPropertiesTypes)
		: this(name, displayName, shortDisplayName, description, targetConnectionType)
	{
		_DataSourceDescriptions = dataSourceDescriptions;
		_ConnectionUIControlTypes = connectionUIControlTypes;
		_ConnectionPropertiesTypes = connectionPropertiesTypes;
	}


	#endregion Constructors / Destructors




	// =====================================================================================================
	#region Fields - TDataProvider
	// =====================================================================================================


	private static TDataProvider _BlackbirdSqlDataProvider;

	private readonly IDictionary<string, string> _DataSourceDescriptions;

	private readonly IDictionary<string, Type> _ConnectionUIControlTypes;

	private readonly IDictionary<string, Type> _ConnectionPropertiesTypes;

	private readonly string _Name;
	private readonly string _DisplayName;
	private readonly string _ShortDisplayName;
	private readonly string _Description;
	private readonly Type _TargetConnectionType;


	#endregion Fields




	// =====================================================================================================
	#region Property Accessors - TDataProvider
	// =====================================================================================================


	public static TDataProvider BlackbirdSqlDataProvider
	{
		get
		{
			if (_BlackbirdSqlDataProvider == null)
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>
				{
					{ TDataSource.FbDataSource.Name, Resources.DataProvider_Ddex_DataSource_Description }
			};
			Dictionary<string, Type> dictionary2 = new Dictionary<string, Type>
			{
				{ TDataSource.FbDataSource.Name, typeof(VxbConnectionUIControl) }
			};
			Dictionary<string, Type> dictionary3 = new Dictionary<string, Type>
			{
				{ "", typeof(VxbConnectionUIProperties) }
			};
				_BlackbirdSqlDataProvider = new (NativeDb.Invariant, Resources.DataProvider_Ddex, Resources.DataProvider_Ddex_Short,
				Resources.DataProvider_Ddex_Description, NativeDb.ConnectionType, dictionary, dictionary2, dictionary3);
			}
			return _BlackbirdSqlDataProvider;
		}
	}



	public string Name => _Name;

	public Guid NameClsid => _Name == NativeDb.Invariant ? new(SystemData.C_ProviderGuid) : new Guid(_Name);

	public string DisplayName
	{
		get
		{
			if (_DisplayName == null)
			{
				return _Name;
			}
			return _DisplayName;
		}
	}

	public string ShortDisplayName => _ShortDisplayName;

	public string Description => GetDescription(null);

	public string DescriptionEx => GetDescriptionEx(null);

	public Type TargetConnectionType => _TargetConnectionType;


	#endregion Property Accessors




	// =====================================================================================================
	#region Methods - TDataProvider
	// =====================================================================================================


	public virtual string GetDescription(TDataSource dataSource)
	{
		if (_DataSourceDescriptions != null && dataSource != null && _DataSourceDescriptions.ContainsKey(dataSource.Name))
		{
			return _DataSourceDescriptions[dataSource.Name];
		}
		return _Description;
	}

	public virtual string GetDescriptionEx(TDataSource dataSource)
	{
		return null;
	}

	public IDataConnectionUIControl CreateConnectionUIControl()
	{
		return CreateConnectionUIControl(null);
	}

	public virtual IDataConnectionUIControl CreateConnectionUIControl(TDataSource dataSource)
	{
		string text;
		if ((_ConnectionUIControlTypes != null && dataSource != null
			&& _ConnectionUIControlTypes.ContainsKey(text = dataSource.Name))
			|| _ConnectionUIControlTypes.ContainsKey(text = ""))
		{
			return Activator.CreateInstance(_ConnectionUIControlTypes[text]) as IDataConnectionUIControl;
		}
		return null;
	}

	public IDataConnectionProperties CreateConnectionProperties()
	{
		return CreateConnectionProperties(null);
	}

	public virtual IDataConnectionProperties CreateConnectionProperties(TDataSource dataSource)
	{
		string text;
		if (_ConnectionPropertiesTypes != null && ((dataSource != null && _ConnectionPropertiesTypes.ContainsKey(text = dataSource.Name)) || _ConnectionPropertiesTypes.ContainsKey(text = "")))
		{
			return Activator.CreateInstance(_ConnectionPropertiesTypes[text]) as IDataConnectionProperties;
		}
		return null;
	}

	#endregion Methods

}
