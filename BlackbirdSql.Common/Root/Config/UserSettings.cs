// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.UserSettings

using System;
using BlackbirdSql.Common.Interfaces;
using BlackbirdSql.Core;


namespace BlackbirdSql.Common.Config;

public sealed class UserSettings
{
	public delegate void PersistenceChangedEventHandler(object sender, EventArgs e);

	private static readonly UserSettings _Instance = new UserSettings();

	private IUserSettingsPersistence _persistenceModel;

	private IUserSettings _settings;

	public static UserSettings Instance => _Instance;

	public IUserSettings Current
	{
		get
		{
			if (_settings == null)
			{
				lock (this)
				{
					_settings ??= _persistenceModel.Load();
				}
			}

			return _settings;
		}
	}

	public IUserSettings Default
	{
		get
		{
			IUserSettings obj = Current.Clone() as IUserSettings;
			obj.ResetToDefault();
			return obj;
		}
	}

	public event PersistenceChangedEventHandler PersistenceModelChanged;

	private UserSettings()
	{
		_persistenceModel = new UserSettingsPersistence();
	}

	public void Load()
	{
		lock (this)
		{
			_settings = _persistenceModel.Load();
		}
	}

	public void Save()
	{
		lock (this)
		{
			_persistenceModel.Save(_settings);
		}
	}

	public void SetPersistenceModel(IUserSettingsPersistence model)
	{
		if (model == null)
		{
			ArgumentNullException ex = new("model", "Invalid persistence model");
			Diag.Dug(ex);
			throw ex;
		}

		lock (this)
		{
			if (_persistenceModel == model)
			{
				return;
			}

			_persistenceModel?.Save(_settings);

			_persistenceModel = model;
			_settings = _persistenceModel.Load();
		}

		PersistenceModelChanged?.Invoke(this, new EventArgs());
	}
}
