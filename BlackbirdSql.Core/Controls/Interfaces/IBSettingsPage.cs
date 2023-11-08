using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using BlackbirdSql.Core.Controls.Events;
using BlackbirdSql.Core.Ctl.Config;
using BlackbirdSql.Core.Ctl.Events;
using BlackbirdSql.Core.Ctl.Interfaces;

namespace BlackbirdSql.Core.Controls.Interfaces;

public interface IBSettingsPage
{
	PropertyGrid Grid { get; }

	public void LoadSettings();
	public void SaveSettings();
}
