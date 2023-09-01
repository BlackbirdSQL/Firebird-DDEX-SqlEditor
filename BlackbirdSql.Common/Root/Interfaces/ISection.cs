// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.ISection

using System;
using System.ComponentModel;

using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Common.Events;

namespace BlackbirdSql.Common.Interfaces;


public interface ISection : IDisposable, INotifyPropertyChanged, IBExportable
{
	string Title { get; }

	object SectionContent { get; }

	bool IsVisible { get; set; }

	bool IsExpanded { get; set; }

	bool IsBusy { get; }

	void Initialize(object sender, SectionInitializeEventArgs e);

	void Loaded(object sender, SectionLoadedEventArgs e);

	void SaveContext(object sender, SectionSaveContextEventArgs e);

	void Refresh();

	void Cancel();

	object GetExtensibilityService(Type serviceType);
}
