// Microsoft.Data.ConnectionUI.Dialog, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.ConnectionUI.DataSource
using System;
using System.Collections;
using System.Collections.Generic;
using BlackbirdSql.Core;
using BlackbirdSql.VisualStudio.Ddex.Controls.DataTools;
using BlackbirdSql.VisualStudio.Ddex.Properties;


namespace BlackbirdSql.VisualStudio.Ddex.Ctl.DataTools;

public class TDataSource
{
	private class TiDataProviderCollection(TDataSource owningDataSource) : ICollection<TDataProvider>, IEnumerable<TDataProvider>, IEnumerable
	{
		private readonly TDataSource _OwningDataSource = owningDataSource;


		private readonly ICollection<TDataProvider> _DataProviders = [];

		public int Count => _DataProviders.Count;

		public bool IsReadOnly => false;

		public void Add(TDataProvider item)
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

		public bool Contains(TDataProvider item)
		{
			return _DataProviders.Contains(item);
		}

		public bool Remove(TDataProvider item)
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

		public void CopyTo(TDataProvider[] array, int arrayIndex)
		{
			_DataProviders.CopyTo(array, arrayIndex);
		}

		public IEnumerator<TDataProvider> GetEnumerator()
		{
			return _DataProviders.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _DataProviders.GetEnumerator();
		}
	}
	private static TDataSource _FbDataSource;
	private readonly string _Name;
	private readonly string _DisplayName;
	private TDataProvider _DefaultProvider;
	private readonly ICollection<TDataProvider> _Providers;



	public static TDataSource FbDataSource
	{
		get
		{
			if (_FbDataSource == null)
			{
				_FbDataSource = new ("FirebirdServer", Resources.DataSource_FirebirdServer);
				_FbDataSource.Providers.Add(TDataProvider.BlackbirdSqlDataProvider);
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


	public TDataProvider DefaultProvider
	{
		get
		{
			switch (_Providers.Count)
			{
			case 0:
				return null;
			case 1:
			{
				IEnumerator<TDataProvider> enumerator = _Providers.GetEnumerator();
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

	public ICollection<TDataProvider> Providers => _Providers;

	private TDataSource()
	{
		_DisplayName = "<other>";
		_Providers = new TiDataProviderCollection(this);
	}

	public TDataSource(string name, string displayName)
	{
		_Name = name ?? throw new ArgumentNullException("name");
		_DisplayName = displayName;
		_Providers = new TiDataProviderCollection(this);
	}

	public static void AddStandardDataSources(TDataConnectionDlg dialog)
	{
		dialog.DataSources.Add(FbDataSource);
		dialog.UnspecifiedDataSource.Providers.Add(TDataProvider.BlackbirdSqlDataProvider);
		dialog.DataSources.Add(dialog.UnspecifiedDataSource);
	}

	internal static TDataSource CreateUnspecified()
	{
		return new TDataSource();
	}
}
