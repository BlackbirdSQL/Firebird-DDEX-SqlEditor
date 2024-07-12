
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using BlackbirdSql.Sys.Ctl;
using BlackbirdSql.Sys.Ctl.ComponentModel;
using BlackbirdSql.Sys.Enums;

using static BlackbirdSql.Sys.SysConstants;



namespace BlackbirdSql.Sys.Model;


// =========================================================================================================
//
//									NativeDbCsbProxy Class
//
/// <summary>
/// Serves as a proxy class for the native DbConnectionStringBuilder.
/// Currently replicates FBConnectionStringBuilder.
/// </summary>
// =========================================================================================================
public class NativeDbCsbProxy : DbConnectionStringBuilder
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractCsb
	// ---------------------------------------------------------------------------------

	public NativeDbCsbProxy()
	{ }

	public NativeDbCsbProxy(string connectionString)
		: this()
	{
		ConnectionString = connectionString;
	}


	#endregion Constructors / Destructors





	// =====================================================================================================
	#region Constants - NativeDbCsbProxy
	// =====================================================================================================


	#endregion Constants





	// =====================================================================================================
	#region Fields - NativeDbCsbProxy
	// =====================================================================================================


	// A protected 'this' object lock
	protected readonly object _LockObject = new();

	private static DescriberDictionary _Describers = null;


	#endregion Fields




	// =====================================================================================================
	#region Property accessors - NativeDbCsbProxy
	// =====================================================================================================



	/// <summary>
	/// Index accessor override to get back some uniformity in connection property naming.
	/// </summary>
	[Browsable(false)]
	public override object this[string key]
	{
		get
		{
			if (key == null)
				Diag.ThrowException(new ArgumentNullException(nameof(key)));


			lock (_LockObject)
				return GetValue(key);
		}
		set
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			lock (_LockObject)
				SetValue(key, value);
		}
	}


	public static DescriberDictionary Describers => _Describers ??= NativeDb.Describers;


	[GlobalizedCategory("PropertyCategorySecurity")]
	[GlobalizedDisplayName("PropertyDisplayUserID")]
	[GlobalizedDescription("PropertyDescriptionUserID")]
	[DefaultValue(C_DefaultUserID)]
	public string UserID
	{
		get { return (string)GetValue(C_KeyUserID); }
		set { SetValue(C_KeyUserID, value); }
	}

	[GlobalizedCategory("PropertyCategorySecurity")]
	[GlobalizedDisplayName("PropertyDisplayPassword")]
	[GlobalizedDescription("PropertyDescriptionPassword")]
	[PasswordPropertyText(true)]
	[DefaultValue(C_DefaultPassword)]
	public string Password
	{
		get { return (string)GetValue(C_KeyPassword); }
		set { SetValue(C_KeyPassword, value); }
	}

	[GlobalizedCategory("PropertyCategorySource")]
	[GlobalizedDisplayName("PropertyDisplayDataSource")]
	[GlobalizedDescription("PropertyDescriptionDataSource")]
	[DefaultValue(C_DefaultDataSource)]
	public string DataSource
	{
		get { return (string)GetValue(C_KeyDataSource); }
		set { SetValue(C_KeyDataSource, value); }
	}

	[GlobalizedCategory("PropertyCategorySource")]
	[GlobalizedDisplayName("PropertyDisplayDatabase")]
	[GlobalizedDescription("PropertyDescriptionDatabase")]
	[DefaultValue(C_DefaultDatabase)]
	public string Database
	{
		get { return (string)GetValue(C_KeyDatabase); }
		set { SetValue(C_KeyDatabase, value); }
	}

	[GlobalizedCategory("PropertyCategorySource")]
	[GlobalizedDisplayName("PropertyDisplayPort")]
	[GlobalizedDescription("PropertyDescriptionPort")]
	[DefaultValue(C_DefaultPort)]
	public int Port
	{
		get { return (int)GetValue(C_KeyPort); }
		set { SetValue(C_KeyPort, value); }
	}

	[GlobalizedCategory("PropertyCategoryAdvanced")]
	[GlobalizedDisplayName("PropertyDisplayPacketSize")]
	[GlobalizedDescription("PropertyDescriptionPacketSize")]
	[DefaultValue(C_DefaultPacketSize)]
	public int PacketSize
	{
		get { return (int)GetValue(C_KeyPacketSize); }
		set { SetValue(C_KeyPacketSize, value); }
	}

	[GlobalizedCategory("PropertyCategorySecurity")]
	[GlobalizedDisplayName("PropertyDisplayRole")]
	[GlobalizedDescription("PropertyDescriptionRole")]
	[DefaultValue(C_DefaultRole)]
	public string Role
	{
		get { return (string)GetValue(C_KeyRole); }
		set { SetValue(C_KeyRole, value); }
	}

	[GlobalizedCategory("PropertyCategoryAdvanced")]
	[GlobalizedDisplayName("PropertyDisplayDialect")]
	[GlobalizedDescription("PropertyDescriptionDialect")]
	[DefaultValue(C_DefaultDialect)]
	public int Dialect
	{
		get { return (int)GetValue(C_KeyDialect); }
		set { SetValue(C_KeyDialect, value); }
	}

	[GlobalizedCategory("PropertyCategoryAdvanced")]
	[GlobalizedDisplayName("PropertyDisplayCharset")]
	[GlobalizedDescription("PropertyDescriptionCharset")]
	[DefaultValue(C_DefaultCharset)]
	public string Charset
	{
		get { return (string)GetValue(C_KeyCharset); }
		set { SetValue(C_KeyCharset, value); }
	}

	[GlobalizedCategory("PropertyCategoryConnection")]
	[GlobalizedDisplayName("PropertyDisplayConnectionTimeout")]
	[GlobalizedDescription("PropertyDescriptionConnectionTimeout")]
	[DefaultValue(C_DefaultConnectionTimeout)]
	public int ConnectionTimeout
	{
		get { return (int)GetValue(C_KeyConnectionTimeout); }
		set { SetValue(C_KeyConnectionTimeout, value); }
	}

	[GlobalizedCategory("PropertyCategoryPooling")]
	[GlobalizedDisplayName("PropertyDisplayPooling")]
	[GlobalizedDescription("PropertyDescriptionPooling")]
	[DefaultValue(C_DefaultPooling)]
	public bool Pooling
	{
		get { return (bool)GetValue(C_KeyPooling); }
		set { SetValue(C_KeyPooling, value); }
	}

	[GlobalizedCategory("PropertyCategoryConnection")]
	[GlobalizedDisplayName("PropertyDisplayConnectionLifeTime")]
	[GlobalizedDescription("PropertyDescriptionConnectionLifeTime")]
	[DefaultValue(C_DefaultConnectionLifeTime)]
	public int ConnectionLifeTime
	{
		get { return (int)GetValue(C_KeyConnectionLifeTime); }
		set { SetValue(C_KeyConnectionLifeTime, value); }
	}

	[GlobalizedCategory("PropertyCategoryPooling")]
	[GlobalizedDisplayName("PropertyDisplayMinPoolSize")]
	[GlobalizedDescription("PropertyDescriptionMinPoolSize")]
	[DefaultValue(C_DefaultMinPoolSize)]
	public int MinPoolSize
	{
		get { return (int)GetValue(C_KeyMinPoolSize); }
		set { SetValue(C_KeyMinPoolSize, value); }
	}

	[GlobalizedCategory("PropertyCategoryPooling")]
	[GlobalizedDisplayName("PropertyDisplayMaxPoolSize")]
	[GlobalizedDescription("PropertyDescriptionMaxPoolSize")]
	[DefaultValue(C_DefaultMaxPoolSize)]
	public int MaxPoolSize
	{
		get { return (int)GetValue(C_KeyMaxPoolSize); }
		set { SetValue(C_KeyMaxPoolSize, value); }
	}

	[GlobalizedCategory("PropertyCategoryAdvanced")]
	[GlobalizedDisplayName("PropertyDisplayFetchSize")]
	[GlobalizedDescription("PropertyDescriptionFetchSize")]
	[DefaultValue(C_DefaultFetchSize)]
	public int FetchSize
	{
		get { return (int)GetValue(C_KeyFetchSize); }
		set { SetValue(C_KeyFetchSize, value); }
	}

	[GlobalizedCategory("PropertyCategorySource")]
	[GlobalizedDisplayName("PropertyDisplayServerType")]
	[GlobalizedDescription("PropertyDescriptionServerType")]
	[DefaultValue(C_DefaultServerType)]
	public EnServerType ServerType
	{
		get { return (EnServerType)GetValue(C_KeyServerType); }
		set { SetValue(C_KeyServerType, value); }
	}

	[GlobalizedCategory("PropertyCategoryAdvanced")]
	[GlobalizedDisplayName("PropertyDisplayIsolationLevel")]
	[GlobalizedDescription("PropertyDescriptionIsolationLevel")]
	[DefaultValue(C_DefaultIsolationLevel)]
	public IsolationLevel IsolationLevel
	{
		get { return (IsolationLevel)GetValue(C_KeyIsolationLevel); }
		set { SetValue(C_KeyIsolationLevel, value); }
	}

	[GlobalizedCategory("PropertyCategoryAdvanced")]
	[GlobalizedDisplayName("PropertyDisplayReturnRecordsAffected")]
	[GlobalizedDescription("PropertyDescriptionReturnRecordsAffected")]
	[DefaultValue(C_DefaultReturnRecordsAffected)]
	public bool ReturnRecordsAffected
	{
		get { return (bool)GetValue(C_KeyReturnRecordsAffected); }
		set { SetValue(C_KeyReturnRecordsAffected, value); }
	}

	[GlobalizedCategory("PropertyCategoryPooling")]
	[GlobalizedDisplayName("PropertyDisplayEnlist")]
	[GlobalizedDescription("PropertyDescriptionEnlist")]
	[DefaultValue(C_DefaultPooling)]
	public bool Enlist
	{
		get { return (bool)GetValue(C_KeyEnlist); }
		set { SetValue(C_KeyEnlist, value); }
	}

	[GlobalizedCategory("PropertyCategoryAdvanced")]
	[GlobalizedDisplayName("PropertyDisplayClientLibrary")]
	[GlobalizedDescription("PropertyDescriptionClientLibrary")]
	[DefaultValue(C_DefaultClientLibrary)]
	public string ClientLibrary
	{
		get { return (string)GetValue(C_KeyClientLibrary); }
		set { SetValue(C_KeyClientLibrary, value); }
	}

	[GlobalizedCategory("PropertyCategoryAdvanced")]
	[GlobalizedDisplayName("PropertyDisplayDbCachePages")]
	[GlobalizedDescription("PropertyDescriptionDbCachePages")]
	[DefaultValue(C_DefaultDbCachePages)]
	public int DbCachePages
	{
		get { return (int)GetValue(C_KeyDbCachePages); }
		set { SetValue(C_KeyDbCachePages, value); }
	}

	[GlobalizedCategory("PropertyCategoryAdvanced")]
	[GlobalizedDisplayName("PropertyDisplayNoDatabaseTriggers")]
	[GlobalizedDescription("PropertyDescriptionNoDatabaseTriggers")]
	[DefaultValue(C_DefaultNoDatabaseTriggers)]
	public bool NoDatabaseTriggers
	{
		get { return (bool)GetValue(C_KeyNoDatabaseTriggers); }
		set { SetValue(C_KeyNoDatabaseTriggers, value); }
	}

	[GlobalizedCategory("PropertyCategoryAdvanced")]
	[GlobalizedDisplayName("PropertyDisplayNoGarbageCollect")]
	[GlobalizedDescription("PropertyDescriptionNoGarbageCollect")]
	[DefaultValue(C_DefaultNoGarbageCollect)]
	public bool NoGarbageCollect
	{
		get { return (bool)GetValue(C_KeyNoGarbageCollect); }
		set { SetValue(C_KeyNoGarbageCollect, value); }
	}

	[GlobalizedCategory("PropertyCategoryAdvanced")]
	[GlobalizedDisplayName("PropertyDisplayCompression")]
	[GlobalizedDescription("PropertyDescriptionCompression")]
	[DefaultValue(C_DefaultCompression)]
	public bool Compression
	{
		get { return (bool)GetValue(C_KeyCompression); }
		set { SetValue(C_KeyCompression, value); }
	}

	[GlobalizedCategory("PropertyCategoryAdvanced")]
	[GlobalizedDisplayName("PropertyDisplayCryptKey")]
	[GlobalizedDescription("PropertyDescriptionCryptKey")]
	[DefaultValue(C_DefaultCryptKey)]
	public byte[] CryptKey
	{
		get { return (byte[])GetValue(C_KeyCryptKey); }
		set { SetValue(C_KeyCryptKey, value); }
	}

	[GlobalizedCategory("PropertyCategoryAdvanced")]
	[GlobalizedDisplayName("PropertyDisplayWireCrypt")]
	[GlobalizedDescription("PropertyDescriptionWireCrypt")]
	[DefaultValue(C_DefaultWireCrypt)]
	public EnWireCrypt WireCrypt
	{
		get { return (EnWireCrypt)GetValue(C_KeyWireCrypt); }
		set { SetValue(C_KeyWireCrypt, value); }
	}

	[GlobalizedCategory("PropertyCategoryAdvanced")]
	[GlobalizedDisplayName("PropertyDisplayApplicationName")]
	[GlobalizedDescription("PropertyDescriptionApplicationName")]
	[DefaultValue(C_DefaultApplicationName)]
	public string ApplicationName
	{
		get { return (string)GetValue(C_KeyApplicationName); }
		set { SetValue(C_KeyApplicationName, value); }
	}

	[GlobalizedCategory("PropertyCategoryAdvanced")]
	[GlobalizedDisplayName("PropertyDisplayCommandTimeout")]
	[GlobalizedDescription("PropertyDescriptionCommandTimeout")]
	[DefaultValue(C_DefaultCommandTimeout)]
	public int CommandTimeout
	{
		get { return (int)GetValue(C_KeyCommandTimeout); }
		set { SetValue(C_KeyCommandTimeout, value); }
	}

	[GlobalizedCategory("PropertyCategoryAdvanced")]
	[GlobalizedDisplayName("PropertyDisplayParallelWorkers")]
	[GlobalizedDescription("PropertyDescriptionParallelWorkers")]
	[DefaultValue(C_DefaultParallelWorkers)]
	public int ParallelWorkers
	{
		get { return (int)GetValue(C_KeyParallelWorkers); }
		set { SetValue(C_KeyParallelWorkers, value); }
	}


	#endregion Property accessors





	// =====================================================================================================
	#region Methods - NativeDbCsbProxy
	// =====================================================================================================


	public delegate bool TryGetValueDelegate(string key, out object value);


	public new void Add(string keyword, object value)
	{
		string key = keyword;
		Describer describer = Describers[keyword];

		if (describer != null)
			key = describer.Key;

		this[key] = value;
	}



	public override bool ContainsKey(string keyword)
	{
		// Tracer.Trace(GetType(), "ContainsKey()", "key: {0}", keyword);
		if (base.ContainsKey(keyword))
			return true;

		IList<string> synonyms = Describers.GetSynonyms(keyword);

		foreach (string synonym in synonyms)
		{
			if (base.ContainsKey(synonym))
				return true;
		}

		return false;
	}


	protected object GetValue(string synonym)
	{
		Describer describer = Describers.GetSynonymDescriber(synonym);

		if (describer == null)
			Diag.ThrowException(new ArgumentException(nameof(synonym)), "Describer does not exist");

		string storageKey = describer.ConnectionStringKey;

		if (!TryGetValue(storageKey, out object value))
			return describer.DefaultValue;

		Type propertyType = describer.PropertyType;

		if (propertyType.IsSubclassOf(typeof(Enum)))
		{
			switch (value)
			{
				case Enum enumValue:
					return enumValue;
				case string stringValue:
					return Enum.Parse(propertyType, stringValue, true);
				default:
					return value;
			}
		}
		else if (propertyType == typeof(byte[]))
		{
			switch (value)
			{
				case byte[] bytesValue:
					return bytesValue;
				case string stringValue:
					return Convert.FromBase64String(stringValue);
				default:
					return value;
			}
		}
		else if (propertyType == typeof(int))
		{
			return Convert.ToInt32(value);
		}
		else if (propertyType == typeof(bool))
		{
			return Convert.ToBoolean(value);
		}
		else if (propertyType == typeof(Version))
		{
			switch (value)
			{
				case string stringValue:
					return new Version(stringValue);
				default:
					return value;
			}
		}
		else if (propertyType == typeof(string))
		{
			return Convert.ToString(value);
		}
		else
		{
			return value;
		}
	}


	public override bool Remove(string keyword)
	{
		if (base.Remove(keyword))
			return true;

		IList<string> synonyms = Describers.GetSynonyms(keyword);

		foreach (string synonym in synonyms)
		{
			if (base.Remove(synonym))
				return true;
		}

		return false;

	}




	protected void SetValue(string synonym, object value)
	{
		Describer describer = Describers.GetSynonymDescriber(synonym);

		if (describer == null)
			Diag.ThrowException(new ArgumentException(nameof(synonym)), $"Describer does not exist: {synonym}.");

		string storageKey = describer.ConnectionStringKey;
		object storedValue;

		Type propertyType = describer.PropertyType;

		if (propertyType.IsSubclassOf(typeof(Enum)))
		{
			if (value is string stringValue)
				value = Enum.Parse(propertyType, stringValue, true);

			if (value is Enum enumValue)
			{
				propertyType = Enum.GetUnderlyingType(enumValue.GetType());

				if (propertyType == typeof(int))
					storedValue = Convert.ToInt32(enumValue);
				else if (propertyType == typeof(long))
					storedValue = Convert.ToInt64(enumValue);
				else
					storedValue = enumValue;
			}
			else
			{
				storedValue = value;
			}
		}
		else if (propertyType == typeof(byte[]))
		{
			if (value is byte[] bytesValue)
				storedValue = Convert.ToBase64String(bytesValue);
			else
				storedValue = value;
		}
		else
		{
			storedValue = value;
		}

		if (storedValue == null || storedValue.Equals(describer.DefaultValue))
			Remove(storageKey);
		else
			base[storageKey] = storedValue;
	}



	public override bool TryGetValue(string keyword, out object value)
	{
		if (base.TryGetValue(keyword, out value))
			return true;

		IList<string> synonyms = Describers.GetSynonyms(keyword);

		foreach (string synonym in synonyms)
		{
			if (base.TryGetValue(synonym, out value))
				return true;
		}

		return false;
	}



	#endregion Methods


}
