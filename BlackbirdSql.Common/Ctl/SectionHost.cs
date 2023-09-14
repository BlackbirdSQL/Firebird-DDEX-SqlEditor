// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.Framework.SectionHost

#define TRACE

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using BlackbirdSql.Common.Ctl.Events;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;


namespace BlackbirdSql.Common.Ctl;

[EditorBrowsable(EditorBrowsableState.Never)]
[DebuggerDisplay("Id = {Id}, Priority = {Priority}")]
public class SectionHost : IDisposable, IServiceProvider
{
	public class ErrorSection : IBSection, IDisposable, INotifyPropertyChanged, IBExportable
	{
		public static string _error = "Error";

		public Exception Error { get; private set; }

		public string Title
		{
			get
			{
				return _error;
			}
			set
			{
			}
		}

		public object SectionContent => new TextBox
		{
			BorderBrush = null,
			Background = new SolidColorBrush(Colors.Transparent),
			Foreground = Cmd.SharedResources["BodyTextBrushKey"] as SolidColorBrush,
			IsReadOnly = true,
			BorderThickness = new Thickness(0.0),
			Text = Error.ToString()
		};

		public bool IsVisible
		{
			get
			{
				return true;
			}
			set
			{
			}
		}

		public bool IsExpanded { get; set; }

		public bool IsBusy => false;

		public IBExportableMetadata Metadata { get; set; }

		public IBDependencyManager DependencyManager { get; set; }

		public ExportableStatus Status => new ExportableStatus();

#pragma warning disable CS0067 // The event 'SectionHost.ErrorSection.PropertyChanged' is never used
		public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067 // The event 'SectionHost.ErrorSection.PropertyChanged' is never used

		public ErrorSection(Exception ex)
		{
			Error = ex;
			IsExpanded = true;
		}

		public void Initialize(object sender, SectionInitializeEventArgs e)
		{
		}

		public void Loaded(object sender, SectionLoadedEventArgs e)
		{
		}

		public void SaveContext(object sender, SectionSaveContextEventArgs e)
		{
		}

		public void Refresh()
		{
		}

		public void Cancel()
		{
		}

		public virtual object GetExtensibilityService(Type serviceType)
		{
			return null;
		}

		public void Dispose()
		{
		}
	}

	public SectionRegInfo SectionRegInfo { get; private set; }

	public Guid Id => SectionRegInfo.Id;

	public IBSection Section { get; private set; }

	public int Priority => SectionRegInfo.Priority;

	public object Tag { get; set; }

	public bool IsCreated => Section != null;

	public bool IsInitialized { get; private set; }

	public event EventHandler<SectionEventArgs> SectionCreatedEvent;

	public event EventHandler<SectionEventArgs> SectionInitializedEvent;

	public event EventHandler<SectionEventArgs> SectionClosingEvent;

	public SectionHost(SectionRegInfo sectionRegInfo)
	{
		SectionRegInfo = sectionRegInfo;
	}

	public void RaiseSectionClosing()
	{
		SectionClosingEvent?.Invoke(this, new SectionEventArgs(this));
	}

	private void ReplaceWithErrorSection(Exception ex)
	{
		IBSection section = Section;
		Section = new ErrorSection(ex);
		if (section != null)
		{
			try
			{
				section.Dispose();
			}
			catch (Exception ex2)
			{
				Trace.TraceError("SectionHost.ReplaceWithErrorSection: {0}", ex2.Message);
			}
		}
	}

	private void ShowException(Exception ex)
	{
		Cmd.ShowException(ex);
	}

	public void Create()
	{
		UiTracer.TraceSource.AssertTraceEvent(Section == null, TraceEventType.Error, EnUiTraceId.UiInfra, "Section == null");
		Trace.TraceInformation("SectionHost.Create section={0} pri={1}", Id, Priority);
		try
		{
			if (SectionRegInfo.SectionTypeInfo != null)
			{
				Section = SectionRegInfo.SectionTypeInfo.Value;
				if (Section != null)
				{
					Trace.TraceInformation("SectionHost.Create section={0} created as type {1}", Id, Section.GetType().ToString());
				}
			}
		}
		catch (Exception ex)
		{
			Trace.TraceError("SectionHost.Create: {0}", ex.Message);
			ReplaceWithErrorSection(ex);
		}
		SectionCreatedEvent?.Invoke(this, new SectionEventArgs(this));
	}

	public void Initialize(object context)
	{
		if (!IsInitialized)
		{
			UiTracer.TraceSource.AssertTraceEvent(Section != null, TraceEventType.Error, EnUiTraceId.UiInfra, "Section != null");
			Trace.TraceInformation("SectionHost.Initialize section={0} pri={1}", Id, Priority);
			try
			{
				Section.Initialize(this, new SectionInitializeEventArgs(this, context));
			}
			catch (Exception ex)
			{
				Trace.TraceError("SectionHost.Initialize: {0}", ex.Message);
				ReplaceWithErrorSection(ex);
			}
			IsInitialized = true;
			SectionInitializedEvent?.Invoke(this, new SectionEventArgs(this));
		}
	}

	public void CloseSection()
	{
		if (Section != null)
		{
			Trace.TraceInformation("SectionHost.CloseSection section={0} pri={1}", Id, Priority);
			try
			{
				Section.Dispose();
			}
			catch (Exception ex)
			{
				Trace.TraceError("SectionHost.CloseSection: {0}", ex.Message);
			}
			Section = null;
			IsInitialized = false;
		}
	}

	public void Dispose()
	{
		Trace.TraceInformation("SectionHost.Dispose section={0} pri={1}", Id, Priority);
		try
		{
			Section?.Dispose();
		}
		catch (Exception ex)
		{
			Trace.TraceError("SectionHost.Dispose: {0}", ex.Message);
		}
		GC.SuppressFinalize(this);
	}

	public object GetService(Type serviceType)
	{
		return null;
	}

	public void Loaded()
	{
		UiTracer.TraceSource.AssertTraceEvent(Section != null, TraceEventType.Error, EnUiTraceId.UiInfra, "Section != null");
		if (Section == null)
		{
			return;
		}
		Trace.TraceInformation("SectionHost.Loaded page={0}", Id);
		try
		{
			Section.Loaded(this, new SectionLoadedEventArgs());
		}
		catch (Exception ex)
		{
			Trace.TraceError("SectionHost.Loaded: {0}", ex.Message);
			ShowException(ex);
		}
	}

	public object SaveContext()
	{
		UiTracer.TraceSource.AssertTraceEvent(Section != null, TraceEventType.Error, EnUiTraceId.UiInfra, "Section != null");
		if (Section == null)
		{
			return null;
		}
		Trace.TraceInformation("SectionHost.SaveContext section={0} pri={1}", Id, Priority);
		try
		{
			SectionSaveContextEventArgs sectionSaveContextEventArgs = new SectionSaveContextEventArgs();
			Section.SaveContext(this, sectionSaveContextEventArgs);
			return sectionSaveContextEventArgs.Context;
		}
		catch (Exception ex)
		{
			Trace.TraceError("SectionHost.SaveContext: {0}", ex.Message);
			ShowException(ex);
		}
		return null;
	}

	public void Refresh(object sender, RoutedEventArgs e)
	{
		Refresh();
	}

	public void Refresh()
	{
		UiTracer.TraceSource.AssertTraceEvent(Section != null, TraceEventType.Error, EnUiTraceId.UiInfra, "Section != null");
		if (Section == null)
		{
			return;
		}
		Trace.TraceInformation("SectionHost.Refresh section={0} pri={1}", Id, Priority);
		try
		{
			Section.Refresh();
		}
		catch (Exception ex)
		{
			Trace.TraceError("SectionHost.Refresh: {0}", ex.Message);
			ShowException(ex);
		}
	}

	public void Cancel(object sender, RoutedEventArgs e)
	{
		Cancel();
	}

	public void Cancel()
	{
		UiTracer.TraceSource.AssertTraceEvent(Section != null, TraceEventType.Error, EnUiTraceId.UiInfra, "Section != null");
		if (Section == null)
		{
			return;
		}
		Trace.TraceInformation("SectionHost.Cancel section={0} pri={1}", Id, Priority);
		try
		{
			Section.Cancel();
		}
		catch (Exception ex)
		{
			Trace.TraceError("SectionHost.Cancel: {0}", ex.Message);
			ShowException(ex);
		}
	}

	public object GetExtensibilityService(Type serviceType)
	{
		if (Section == null)
		{
			return null;
		}
		Trace.TraceInformation("SectionHost.GetExtensibilityService section={0} pri={1}", Id, Priority);
		try
		{
			return Section.GetExtensibilityService(serviceType);
		}
		catch (Exception ex)
		{
			Trace.TraceError("SectionHost.GetExtensibilityService: {0}", ex.Message);
			ShowException(ex);
		}
		return null;
	}
}
