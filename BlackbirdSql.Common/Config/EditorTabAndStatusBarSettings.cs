// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.EditorTabAndStatusBarSettings

using System;
using System.Drawing;
using BlackbirdSql.Common.Config.Enums;
using BlackbirdSql.Common.Interfaces;



namespace BlackbirdSql.Common.Config;

public sealed class EditorTabAndStatusBarSettings : IEditorTabAndStatusBarSettings, ICloneable
{
	public static class Defaults
	{
		public static readonly EnDisplayTimeOptions ShowTimeOption = EnDisplayTimeOptions.Elapsed;

		public static readonly bool StatusBarIncludeServerName = true;

		public static readonly bool StatusBarIncludeDatabaseName = true;

		public static readonly bool StatusBarIncludeLoginName = true;

		public static readonly bool StatusBarIncludeRowCount = true;

		public static Color StatusBarColor = SystemColors.Control;

		public static readonly bool TabTextIncludeServerName = false;

		public static readonly bool TabTextIncludeDatabaseName = false;

		public static readonly bool TabTextIncludeLoginName = false;

		public static readonly bool TabTextIncludeFileName = true;
	}

	private EnDisplayTimeOptions? _statusBarShowTimeOption;

	private bool? _statusBarIncludeServerName;

	private bool? _statusBarIncludeDatabaseName;

	private bool? _statusBarIncludeLoginName;

	private bool? _statusBarIncludeRowCount;

	private Color? _statusBarColor;

	private bool _layoutPropertyChanged;

	private bool? _tabTextIncludeServerName;

	private bool? _tabTextIncludeDatabaseName;

	private bool? _tabTextIncludeLoginName;

	private bool? _tabTextIncludeFileName;

	public EnDisplayTimeOptions ShowTimeOption
	{
		get
		{
			if (!_statusBarShowTimeOption.HasValue)
			{
				return Defaults.ShowTimeOption;
			}

			return _statusBarShowTimeOption.Value;
		}
		set
		{
			_statusBarShowTimeOption = value;
		}
	}

	public bool StatusBarIncludeServerName
	{
		get
		{
			if (!_statusBarIncludeServerName.HasValue)
			{
				return Defaults.StatusBarIncludeServerName;
			}

			return _statusBarIncludeServerName.Value;
		}
		set
		{
			_statusBarIncludeServerName = value;
			_layoutPropertyChanged = true;
		}
	}

	public bool StatusBarIncludeDatabaseName
	{
		get
		{
			if (!_statusBarIncludeDatabaseName.HasValue)
			{
				return Defaults.StatusBarIncludeDatabaseName;
			}

			return _statusBarIncludeDatabaseName.Value;
		}
		set
		{
			_statusBarIncludeDatabaseName = value;
			_layoutPropertyChanged = true;
		}
	}

	public bool StatusBarIncludeLoginName
	{
		get
		{
			if (!_statusBarIncludeLoginName.HasValue)
			{
				return Defaults.StatusBarIncludeLoginName;
			}

			return _statusBarIncludeLoginName.Value;
		}
		set
		{
			_statusBarIncludeLoginName = value;
			_layoutPropertyChanged = true;
		}
	}

	public bool StatusBarIncludeRowCount
	{
		get
		{
			if (!_statusBarIncludeRowCount.HasValue)
			{
				return Defaults.StatusBarIncludeRowCount;
			}

			return _statusBarIncludeRowCount.Value;
		}
		set
		{
			_statusBarIncludeRowCount = value;
			_layoutPropertyChanged = true;
		}
	}

	public Color StatusBarColor
	{
		get
		{
			if (!_statusBarColor.HasValue)
			{
				return Defaults.StatusBarColor;
			}

			return _statusBarColor.Value;
		}
		set
		{
			_statusBarColor = value;
		}
	}

	public bool LayoutPropertyChanged
	{
		get
		{
			return _layoutPropertyChanged;
		}
		set
		{
			_layoutPropertyChanged = value;
		}
	}

	public bool TabTextIncludeServerName
	{
		get
		{
			if (!_tabTextIncludeServerName.HasValue)
			{
				return Defaults.TabTextIncludeServerName;
			}

			return _tabTextIncludeServerName.Value;
		}
		set
		{
			_tabTextIncludeServerName = value;
		}
	}

	public bool TabTextIncludeDatabaseName
	{
		get
		{
			if (!_tabTextIncludeDatabaseName.HasValue)
			{
				return Defaults.TabTextIncludeDatabaseName;
			}

			return _tabTextIncludeDatabaseName.Value;
		}
		set
		{
			_tabTextIncludeDatabaseName = value;
		}
	}

	public bool TabTextIncludeLoginName
	{
		get
		{
			if (!_tabTextIncludeLoginName.HasValue)
			{
				return Defaults.TabTextIncludeLoginName;
			}

			return _tabTextIncludeLoginName.Value;
		}
		set
		{
			_tabTextIncludeLoginName = value;
		}
	}

	public bool TabTextIncludeFileName
	{
		get
		{
			if (!_tabTextIncludeFileName.HasValue)
			{
				return Defaults.TabTextIncludeFileName;
			}

			return _tabTextIncludeFileName.Value;
		}
		set
		{
			_tabTextIncludeFileName = value;
		}
	}

	public object Clone()
	{
		return MemberwiseClone();
	}

	public void ResetToDefault()
	{
		_statusBarShowTimeOption = Defaults.ShowTimeOption;
		_statusBarIncludeServerName = Defaults.StatusBarIncludeServerName;
		_statusBarIncludeDatabaseName = Defaults.StatusBarIncludeDatabaseName;
		_statusBarIncludeLoginName = Defaults.StatusBarIncludeLoginName;
		_statusBarIncludeRowCount = Defaults.StatusBarIncludeRowCount;
		_statusBarColor = Defaults.StatusBarColor;
		_tabTextIncludeServerName = Defaults.TabTextIncludeServerName;
		_tabTextIncludeDatabaseName = Defaults.TabTextIncludeDatabaseName;
		_tabTextIncludeFileName = Defaults.TabTextIncludeFileName;
		_tabTextIncludeLoginName = Defaults.TabTextIncludeLoginName;
		_layoutPropertyChanged = true;
	}
}
