// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.UserSettings

using System;

using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Core;


namespace BlackbirdSql.Common.Ctl.Config;

public sealed class UserSettings
{
	public delegate void PersistenceChangedEventHandler(object sender, EventArgs e);

	private static readonly UserSettings _Instance = new UserSettings();

	private IBUserSettingsPersistence _persistenceModel;

	private IBUserSettings _settings;

	public static UserSettings Instance => _Instance;

	public IBUserSettings Current
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

	public IBUserSettings Default
	{
		get
		{
			IBUserSettings obj = Current.Clone() as IBUserSettings;
			obj.ResetToDefault();
			return obj;
		}
	}

	public event PersistenceChangedEventHandler PersistenceModelChangedEvent;

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

	public void SetPersistenceModel(IBUserSettingsPersistence model)
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

		PersistenceModelChangedEvent?.Invoke(this, new EventArgs());
	}
}
