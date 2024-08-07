// Microsoft.Data.ConnectionUI.Dialog, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.ConnectionUI.UserPreferenceChangedHandler
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.Win32;



namespace BlackbirdSql.VisualStudio.Ddex.Events;


public sealed class UserPreferenceChangedHandler : IComponent, IDisposable
{
	private readonly Form _Frm;

	public ISite Site
	{
		get
		{
			return _Frm.Site;
		}
		set
		{
		}
	}

	public event EventHandler Disposed;

	public UserPreferenceChangedHandler(Form form)
	{
		SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
		_Frm = form;
	}

	~UserPreferenceChangedHandler()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
	{
		IUIService iUIService = ((_Frm.Site != null)
			? (_Frm.Site.GetService(typeof(IUIService)) as IUIService)
			: Package.GetGlobalService(typeof(IUIService)) as IUIService);

		if (iUIService != null && iUIService.Styles["DialogFont"] is Font font)
		{
			_Frm.Font = font;
		}
	}

	private void Dispose(bool disposing)
	{
		if (disposing)
		{
			SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
			Disposed?.Invoke(this, EventArgs.Empty);
		}
	}
}
