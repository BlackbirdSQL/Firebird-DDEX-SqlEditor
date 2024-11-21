// Microsoft.Data.ConnectionUI.Dialog, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.ConnectionUI.DataSource
using System;
using System.Collections;
using System.Collections.Generic;
using BlackbirdSql.Core;
using BlackbirdSql.VisualStudio.Ddex.Controls.DataTools;
using BlackbirdSql.VisualStudio.Ddex.Properties;


namespace BlackbirdSql.VisualStudio.Ddex.Ctl.DataTools;

public class VxbDataSource
{
	private class VxiDataProviderCollection(VxbDataSource owningDataSource) : ICollection<VxbDataProvider>, IEnumerable<VxbDataProvider>, IEnumerable
	{
		private readonly VxbDataSource _OwningDataSource = owningDataSource;


		private readonly ICollection<VxbDataProvider> _DataProviders = [];

		public int Count => _DataProviders.Count;

		public bool IsReadOnly => false;

		public void Add(VxbDataProvider item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			if (!_DataProviders.Contains(item))
			{
				_DataProviders.Add(item);
			}
		}

		public bool Contains(VxbDataProvider item)
		{
			return _DataProviders.Contains(item);
		}

		public bool Remove(VxbDataProvider item)
		{
			bool result = _DataProviders.Remove(item);
			if (item == _OwningDataSource._DefaultProvider)
			{
				_OwningDataSource._DefaultProvider = null;
			}
			return result;
		}

		public void Clear()
		{
			_DataProviders.Clear();
			_OwningDataSource._DefaultProvider = null;
		}

		public void CopyTo(VxbDataProvider[] array, int arrayIndex)
		{
			_DataProviders.CopyTo(array, arrayIndex);
		}

		public IEnumerator<VxbDataProvider> GetEnumerator()
		{
			return _DataProviders.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _DataProviders.GetEnumerator();
		}
	}
	private static VxbDataSource _FbDataSource;
	private readonly string _Name;
	private readonly string _DisplayName;
	private VxbDataProvider _DefaultProvider;
	private readonly ICollection<VxbDataProvider> _Providers;



	public static VxbDataSource FbDataSource
	{
		get
		{
			if (_FbDataSource == null)
			{
				_FbDataSource = new ("FirebirdServer", Resources.DataSource_FirebirdServer);
				_FbDataSource.Providers.Add(VxbDataProvider.BlackbirdSqlDataProvider);
			}
			return _FbDataSource;
		}
	}


	public string Name => _Name;

	public Guid NameClsid => string.IsNullOrWhiteSpace(_Name) ? Guid.Empty : (new Guid(_Name == "FirebirdServer" ? SystemData.C_DataSourceGuid : _Name));

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


	public VxbDataProvider DefaultProvider
	{
		get
		{
			switch (_Providers.Count)
			{
			case 0:
				return null;
			case 1:
			{
				IEnumerator<VxbDataProvider> enumerator = _Providers.GetEnumerator();
				enumerator.MoveNext();
				return enumerator.Current;
			}
			default:
				if (_Name == null)
				{
					return null;
				}
				return _DefaultProvider;
			}
		}
		set
		{
			if (_Providers.Count == 1 && _DefaultProvider != value)
			{
				throw new InvalidOperationException(ControlsResources.TDataSource_CannotChangeSingleDataProvider);
			}
			if (value != null && !_Providers.Contains(value))
			{
				throw new InvalidOperationException(ControlsResources.TDataSource_DataProviderNotFound);
			}
			_DefaultProvider = value;
		}
	}

	public ICollection<VxbDataProvider> Providers => _Providers;

	private VxbDataSource()
	{
		_DisplayName = "<other>";
		_Providers = new VxiDataProviderCollection(this);
	}

	public VxbDataSource(string name, string displayName)
	{
		_Name = name ?? throw new ArgumentNullException("name");
		_DisplayName = displayName;
		_Providers = new VxiDataProviderCollection(this);
	}

	public static void AddStandardDataSources(VxbConnectionDialog dialog)
	{
		dialog.DataSources.Add(FbDataSource);
		dialog.UnspecifiedDataSource.Providers.Add(VxbDataProvider.BlackbirdSqlDataProvider);
		dialog.DataSources.Add(dialog.UnspecifiedDataSource);
	}

	internal static VxbDataSource CreateUnspecified()
	{
		return new VxbDataSource();
	}
}
