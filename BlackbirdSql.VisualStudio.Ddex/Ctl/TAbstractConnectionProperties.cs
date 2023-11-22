// Microsoft.VisualStudio.Data.Framework, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Framework.AdoDotNet.AdoDotNetConnectionProperties
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Model;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services.SupportEntities;


namespace BlackbirdSql.VisualStudio.Ddex.Ctl;

/// <summary>
/// Replacement for Framework.AdoDotNet.AdoDotNetConnectionProperties implementation of the
/// IVsDataConnectionProperties and IVsDataConnectionUIProperties interfaces using CsbAgent
/// instead of FbConnectionStringBuilder.
/// </summary>
public class TAbstractConnectionProperties : DataSiteableObject<IVsDataProvider>, IVsDataConnectionProperties, IDictionary<string, object>, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable, IVsDataConnectionUIProperties, ICustomTypeDescriptor, INotifyPropertyChanged
{
	private DbConnectionStringBuilder _ConnectionStringBuilder;

	private readonly object _LockObject = new object();

	public virtual object this[string key]
	{
		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		get
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			if (!ConnectionStringBuilder.TryGetValue(key, out object value))
			{
				throw new KeyNotFoundException(string.Format(null, "Resources.DataConnectionProperties_PropertyNotFound: {0}", key));
			}

			return value;
		}
		set
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			bool flag = false;
			lock (_LockObject)
			{
				if (!ContainsKey(key))
				{
					if (!IsExtensible)
					{
						throw new KeyNotFoundException(string.Format(null, "Resources.DataConnectionProperties_PropertyNotFound: {0}", key));
					}

					((CsbAgent)ConnectionStringBuilder).Add(key, value);
					flag = true;
				}

				object objB = ConnectionStringBuilder[key];
				ConnectionStringBuilder.Remove(key);
				ConnectionStringBuilder.TryGetValue(key, out object value2);
				ConnectionStringBuilder[key] = value;
				if (Equals(value2, value))
				{
					ConnectionStringBuilder.Remove(key);
				}

				if (!Equals(value, objB))
				{
					flag = true;
				}
			}

			if (flag)
			{
				OnPropertyChanged(new PropertyChangedEventArgs(key));
			}
		}
	}

	public virtual bool IsExtensible => !ConnectionStringBuilder.IsFixedSize;

	public virtual bool IsComplete => true;

	public virtual int Count => ((ICustomTypeDescriptor)this).GetProperties().Count;

	public virtual ICollection<string> Keys
	{
		get
		{
			// Tracer.Trace(GetType(), "Keys");
			string[] array = new string[ConnectionStringBuilder.Keys.Count];
			ConnectionStringBuilder.Keys.CopyTo(array, 0);
			return array;
		}
	}

	public virtual ICollection<object> Values
	{
		get
		{
			object[] array = new string[ConnectionStringBuilder.Values.Count];
			object[] array2 = array;
			ConnectionStringBuilder.Values.CopyTo(array2, 0);
			return array2;
		}
	}

	bool ICollection<KeyValuePair<string, object>>.IsReadOnly => ConnectionStringBuilder.IsReadOnly;

	protected DbConnectionStringBuilder ConnectionStringBuilder
	{
		get
		{
			if (_ConnectionStringBuilder == null)
			{
				throw new InvalidOperationException();
			}

			return _ConnectionStringBuilder;
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public virtual void Reset()
	{
		lock (_LockObject)
		{
			ConnectionStringBuilder.Clear();
		}

		OnPropertyChanged(new PropertyChangedEventArgs(string.Empty));
	}

	public virtual void Parse(string connectionString)
	{
		lock (_LockObject)
		{
			ConnectionStringBuilder.ConnectionString = connectionString;
		}

		OnPropertyChanged(new PropertyChangedEventArgs(string.Empty));
	}

	public virtual string[] GetSynonyms(string key)
	{
		return new string[0];
	}

	public virtual bool IsSensitive(string key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}

		PropertyDescriptorCollection properties = ((ICustomTypeDescriptor)this).GetProperties(new Attribute[1] { PasswordPropertyTextAttribute.Yes });
		foreach (PropertyDescriptor item in properties)
		{
			if (item.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}

		return false;
	}

	public virtual bool ContainsKey(string key)
	{
		// Tracer.Trace(GetType(), "ContainsKey()", "key: {0}", key);
		return ConnectionStringBuilder.ContainsKey(key);
	}

	public virtual bool Contains(KeyValuePair<string, object> item)
	{
		if (!ContainsKey(item.Key))
		{
			return false;
		}

		return Equals(this[item.Key], item.Value);
	}

	public virtual void Add(string key, object value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}

		if (!IsExtensible)
		{
			throw new NotSupportedException("Resources.DataConnectionProperties_NotExtensible");
		}

		lock (_LockObject)
		{
			if (ContainsKey(key))
			{
				throw new ArgumentException(string.Format(null, "Resources.DataConnectionProperties_PropertyAlreadyExists: {0}", key), "key");
			}

			((CsbAgent)ConnectionStringBuilder).Add(key, value);
		}

		OnPropertyChanged(new PropertyChangedEventArgs(key));
	}

	public virtual void Add(string key, Type type, object value)
	{
		Add(key, value);
	}

	public virtual bool TryGetValue(string key, out object value)
	{
		return ConnectionStringBuilder.TryGetValue(key, out value);
	}

	public virtual bool Reset(string key)
	{
		bool flag = false;
		lock (_LockObject)
		{
			if (ConnectionStringBuilder.ContainsKey(key))
			{
				flag = ConnectionStringBuilder.Remove(key);
			}
		}

		if (flag)
		{
			OnPropertyChanged(new PropertyChangedEventArgs(key));
		}

		return flag;
	}

	public virtual bool Remove(string key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}

		if (!IsExtensible)
		{
			throw new NotSupportedException("Resources.DataConnectionProperties_NotExtensible");
		}

		return Reset(key);
	}


	public virtual string ToDisplayString()
	{
		PropertyDescriptorCollection properties = ((ICustomTypeDescriptor)this).GetProperties(new Attribute[1] { PasswordPropertyTextAttribute.Yes });
		IList<KeyValuePair<string, object>> list = new List<KeyValuePair<string, object>>();
		foreach (PropertyDescriptor item in properties)
		{
			string displayName = item.DisplayName;
			if (ConnectionStringBuilder.ShouldSerialize(displayName))
			{
				object value = ConnectionStringBuilder[displayName];
				list.Add(new KeyValuePair<string, object>(displayName, value));
				ConnectionStringBuilder[displayName] = new string('*', 11);
			}
		}

		try
		{
			return ConnectionStringBuilder.ConnectionString;
		}
		finally
		{
			foreach (KeyValuePair<string, object> item2 in list)
			{
				if (item2.Value != null)
				{
					ConnectionStringBuilder[item2.Key] = item2.Value;
				}
			}
		}
	}

	public virtual string ToSafeString()
	{
		PropertyDescriptorCollection properties = ((ICustomTypeDescriptor)this).GetProperties(new Attribute[1] { PasswordPropertyTextAttribute.Yes });
		IList<KeyValuePair<string, object>> list = new List<KeyValuePair<string, object>>();
		foreach (PropertyDescriptor item in properties)
		{
			string displayName = item.DisplayName;
			if (ConnectionStringBuilder.ShouldSerialize(displayName))
			{
				list.Add(new KeyValuePair<string, object>(displayName, ConnectionStringBuilder[displayName]));
				ConnectionStringBuilder.Remove(displayName);
			}
		}

		try
		{
			return ConnectionStringBuilder.ConnectionString;
		}
		finally
		{
			foreach (KeyValuePair<string, object> item2 in list)
			{
				if (item2.Value != null)
				{
					ConnectionStringBuilder[item2.Key] = item2.Value;
				}
			}
		}
	}



	public virtual void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
	{
		// Tracer.Trace(GetType(), "CopyTo()");
		lock (_LockObject)
		{
			KeyValuePair<string, object>[] array2 = new KeyValuePair<string, object>[Keys.Count];
			int num = 0;
			foreach (string key in Keys)
			{
				array2[num] = new KeyValuePair<string, object>(key, this[key]);
				num++;
			}

			array2.CopyTo(array, arrayIndex);
		}
	}

	public override string ToString()
	{
		// Tracer.Trace(GetType(), "ToString()");
		return ConnectionStringBuilder.ToString();
	}

	string ICustomTypeDescriptor.GetClassName()
	{
		// Tracer.Trace(GetType(), "ICustomTypeDescriptor.GetClassName()");
		return TypeDescriptor.GetClassName(ConnectionStringBuilder, noCustomTypeDesc: true);
	}

	string ICustomTypeDescriptor.GetComponentName()
	{
		//Tracer.Trace(GetType(), "ICustomTypeDescriptor.GetComponentName()");
		return TypeDescriptor.GetComponentName(ConnectionStringBuilder, noCustomTypeDesc: true);
	}

	AttributeCollection ICustomTypeDescriptor.GetAttributes()
	{
		// Tracer.Trace(GetType(), "GetAttributes()");
		return TypeDescriptor.GetAttributes(ConnectionStringBuilder, noCustomTypeDesc: true);
	}

	object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
	{
		// Tracer.Trace(GetType(), "GetEditor()");
		return TypeDescriptor.GetEditor(ConnectionStringBuilder, editorBaseType, noCustomTypeDesc: true);
	}

	TypeConverter ICustomTypeDescriptor.GetConverter()
	{
		// Tracer.Trace(GetType(), "GetConvertor()", "Conevertor: {0}", TypeDescriptor.GetConverter(ConnectionStringBuilder, noCustomTypeDesc: true));
		return new CsbAgent.CsbConverter();
	}

	PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
	{
		// Tracer.Trace(GetType(), "GetDefaultProperty()");
		return TypeDescriptor.GetDefaultProperty(ConnectionStringBuilder, noCustomTypeDesc: true);
	}

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
	{
		// Tracer.Trace(GetType(), "ICustomTypeDescriptor.GetProperties()");
		return TypeDescriptor.GetProperties(ConnectionStringBuilder);
	}

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
	{
		// Tracer.Trace(GetType(), "ICustomTypeDescriptor.GetProperties(Attribute[])");
		return TypeDescriptor.GetProperties(ConnectionStringBuilder, attributes);
	}

	EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
	{
		// Tracer.Trace(GetType(), "ICustomTypeDescriptor.GetDefaultEvent()");
		return TypeDescriptor.GetDefaultEvent(ConnectionStringBuilder, noCustomTypeDesc: true);
	}

	EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
	{
		// Tracer.Trace(GetType(), "ICustomTypeDescriptor.GetEvents()");
		return TypeDescriptor.GetEvents(ConnectionStringBuilder, noCustomTypeDesc: true);
	}

	EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
	{
		// Tracer.Trace(GetType(), "ICustomTypeDescriptor.GetEvents(Attribute[])");
		return TypeDescriptor.GetEvents(ConnectionStringBuilder, attributes, noCustomTypeDesc: true);
	}

	object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
	{
		// Tracer.Trace(GetType(), "ICustomTypeDescriptor.GetPropertyOwner()");
		return ConnectionStringBuilder;
	}

	void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
	{
		// Tracer.Trace(GetType(), "ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object>)");
		Add(item.Key, item.Value);
	}

	bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
	{
		// Tracer.Trace(GetType(), "ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object>)");
		if (!Contains(item))
		{
			return false;
		}

		return Remove(item.Key);
	}

	void ICollection<KeyValuePair<string, object>>.Clear()
	{
		// Tracer.Trace(GetType(), "ICollection<KeyValuePair<string, object>>.Clear()");
		throw new NotSupportedException();
	}

	IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
	{
		// Tracer.Trace(GetType(), "IEnumerable<KeyValuePair<string, object>>.GetEnumerator()");
		KeyValuePair<string, object>[] array = new KeyValuePair<string, object>[Keys.Count];
		CopyTo(array, 0);
		return ((IEnumerable<KeyValuePair<string, object>>)array).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		// Tracer.Trace(GetType(), "IEnumerable.GetEnumerator()");
		KeyValuePair<string, object>[] array = new KeyValuePair<string, object>[Keys.Count];
		CopyTo(array, 0);
		return array.GetEnumerator();
	}

	protected override void OnSiteChanged(EventArgs e)
	{
		if (Site != null)
		{
			Tracer.Trace(GetType(), "OnSiteChanged()", "TYPE IVsDataProvider Site type: {0}", Site.GetType().FullName);
			_ConnectionStringBuilder = new CsbAgent();

			Reset();
		}

		base.OnSiteChanged(e);
		if (Site == null)
		{
			_ConnectionStringBuilder = null;
		}
	}

	protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		PropertyChanged?.Invoke(this, e);
	}
}
