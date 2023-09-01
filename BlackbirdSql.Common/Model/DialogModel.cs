// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.Framework.DialogModel

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Core.Diagnostics.Enums;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Events;
using BlackbirdSql.Common.Enums;
using BlackbirdSql.Common.Interfaces;

namespace BlackbirdSql.Common.Model;


[EditorBrowsable(EditorBrowsableState.Never)]
public class DialogModel : IDisposable, IServiceProvider, IServiceContainer
{
	private ServiceContainer _ServiceContainer = new ServiceContainer();

	private BackgroundWorker _DiscoveryWorker;

	// private Exception _DiscoveryError;

	private readonly object _LocalLock = new object();

	private bool _DeferredDispose;

	protected bool _Disposed;

	// private readonly bool _WindowIdsChecked;

	private readonly List<SectionRegInfo> _Sections = new List<SectionRegInfo>();

	private readonly IBDependencyManager _DependencyManager;

	// private static readonly string m_teamNavKey = "TeamFoundation\\TeamExplorer";

	// private static readonly string m_teamNavPluginDataKey = "TeamFoundation\\TeamExplorer\\PluginData";

	// private static readonly string m_teamNavWindowIdsKey = "TeamFoundation\\TeamExplorer\\WindowIds";

	// private static readonly string _WindowIdVersion = "2";

	private Traceable Trace { get; set; }

	public bool ShowSectionProgress
	{
		get
		{
			/*
			try
			{
				using RegistryKey registryKey = GetSettingsRegKey(writable: false);
				if (registryKey != null)
				{
					return (int)registryKey.GetValue("ShowSectionProgress", 0) == 1;
				}
			}
			catch (Exception ex)
			{
				Trace.TraceException(TraceEventType.Error, EnUiTraceId.UiInfra, ex, ex.Message, 572, "DialogModel.cs", "ShowSectionProgress");
			}
			*/
			return false;
		}
	}

	public bool IsDiscoveryRunning
	{
		get
		{
			if (_DiscoveryWorker != null)
			{
				return _DiscoveryWorker.IsBusy;
			}
			return false;
		}
	}

	public bool IsDiscoveryCompleted { get; private set; }

	public IEnumerable<Lazy<ISection, IBExportableMetadata>> SectionCandidates { get; set; }

	public ObservableCollection<SectionHost> Sections { get; private set; }

	public event EventHandler<EventArgs> DiscoveryCompleted;

	public event EventHandler<SectionEventArgs> SectionCreated;

	public event EventHandler<SectionEventArgs> SectionInitialized;

	public event EventHandler<SectionEventArgs> SectionClosing;

	public event EventHandler ContextInfoProvidersChanged;

	public DialogModel(IBDependencyManager dependencyManager)
	{
		Cmd.CheckForNull(dependencyManager, "dependencyManager");
		_DependencyManager = dependencyManager;
		Trace = new Traceable(dependencyManager);
		Sections = new ObservableCollection<SectionHost>();
	}

	public virtual void Dispose()
	{
		if (_DiscoveryWorker != null && _DiscoveryWorker.IsBusy)
		{
			_DeferredDispose = true;
			_DiscoveryWorker.CancelAsync();
			return;
		}
		if (_DiscoveryWorker != null)
		{
			_DiscoveryWorker.Dispose();
			_DiscoveryWorker = null;
		}
		foreach (SectionRegInfo section in _Sections)
		{
			section.Dispose();
		}
		if (_ServiceContainer != null)
		{
			_ServiceContainer.Dispose();
			_ServiceContainer = null;
		}
		GC.SuppressFinalize(this);
	}

	private void CheckForDisposed()
	{
		UiTracer.TraceSource.AssertTraceEvent(!_Disposed, TraceEventType.Error, EnUiTraceId.UiInfra, "DialogModel accessed after being disposed.");
		if (_Disposed)
		{
			ObjectDisposedException ex = new("DialogModel");
			Diag.Dug(ex);
			throw ex;
		}
	}

	private void CheckServiceContainer()
	{
		UiTracer.TraceSource.AssertTraceEvent(_ServiceContainer != null, TraceEventType.Error, EnUiTraceId.UiInfra, "DialogModel service container accessed after being disposed.");
		CheckForDisposed();
	}

	private void SectionHost_SectionCreated(object sender, SectionEventArgs e)
	{
		SectionCreated?.Invoke(this, e);
	}

	private void SectionHost_SectionInitialized(object sender, SectionEventArgs e)
	{
		SectionInitialized?.Invoke(this, e);
	}

	private void SectionHost_SectionClosing(object sender, SectionEventArgs e)
	{
		SectionClosing?.Invoke(this, e);
	}

	protected void RaiseContextInfoProvidersChanged()
	{
		ContextInfoProvidersChanged?.Invoke(this, EventArgs.Empty);
	}

	public object GetService(Type serviceType)
	{
		Cmd.CheckForNull(serviceType, "serviceType");
		if (serviceType == typeof(IServiceContainer) || serviceType == typeof(DialogModel))
		{
			return this;
		}
		return _ServiceContainer.GetService(serviceType);
	}

	void IServiceContainer.AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
	{
		CheckServiceContainer();
		_ServiceContainer.AddService(serviceType, callback, promote);
	}

	void IServiceContainer.AddService(Type serviceType, ServiceCreatorCallback callback)
	{
		CheckServiceContainer();
		_ServiceContainer.AddService(serviceType, callback);
	}

	void IServiceContainer.AddService(Type serviceType, object serviceInstance, bool promote)
	{
		CheckServiceContainer();
		_ServiceContainer.AddService(serviceType, serviceInstance, promote);
	}

	void IServiceContainer.AddService(Type serviceType, object serviceInstance)
	{
		CheckServiceContainer();
		_ServiceContainer.AddService(serviceType, serviceInstance);
	}

	void IServiceContainer.RemoveService(Type serviceType, bool promote)
	{
		CheckServiceContainer();
		_ServiceContainer.RemoveService(serviceType, promote);
	}

	void IServiceContainer.RemoveService(Type serviceType)
	{
		CheckServiceContainer();
		_ServiceContainer.RemoveService(serviceType);
	}

	object IServiceProvider.GetService(Type serviceType)
	{
		CheckServiceContainer();
		return GetService(serviceType);
	}

	public void RegisterSection(Guid sectionId, Lazy<ISection, IBExportableMetadata> sectionTypeInfo, int priority)
	{
		if (sectionId == Guid.Empty)
		{
			ArgumentOutOfRangeException ex = new("sectionId");
			Diag.Dug(ex);
			throw ex;
		}
		if (sectionTypeInfo == null)
		{
			ArgumentNullException ex = new("sectionTypeInfo");
			Diag.Dug(ex);
			throw ex;
		}
		if (priority < 0)
		{
			ArgumentOutOfRangeException ex = new("priority");
			Diag.Dug(ex);
			throw ex;
		}
		Trace.TraceEvent(TraceEventType.Information, EnUiTraceId.UiInfra, "RegisterSection section={0} pri={1}", sectionId, priority);
		SectionRegInfo sectionRegInfo = null;
		lock (_LocalLock)
		{
			sectionRegInfo = new SectionRegInfo(sectionTypeInfo);
			bool flag = false;
			for (int i = 0; i < _Sections.Count; i++)
			{
				int priority2 = _Sections[i].Priority;
				if (priority < priority2)
				{
					_Sections.Insert(i, sectionRegInfo);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				_Sections.Add(sectionRegInfo);
			}
		}
	}

	/*
	private RegistryKey GetSettingsRegKey(bool writable)
	{
		return null;
		using RegistryKey registryKey = UIHost.UserRegistryRoot;
		return writable ? registryKey.CreateSubKey(m_teamNavKey, RegistryKeyPermissionCheck.ReadWriteSubTree) : registryKey.OpenSubKey(m_teamNavKey, writable: false);
	}
	*/

	/*
	private RegistryKey GetPluginDataRegKey(bool writable)
	{
		return null;
		using RegistryKey registryKey = UIHost.UserRegistryRoot;
		return writable ? registryKey.CreateSubKey(m_teamNavPluginDataKey, RegistryKeyPermissionCheck.ReadWriteSubTree) : registryKey.OpenSubKey(m_teamNavPluginDataKey, writable: false);
	}
	*/

	/*
	private RegistryKey GetPageDataRegKey(Guid pageId, bool writable)
	{
		using (RegistryKey registryKey = GetPluginDataRegKey(writable))
		{
			if (registryKey != null)
			{
				return writable ? registryKey.CreateSubKey(pageId.ToString(), RegistryKeyPermissionCheck.ReadWriteSubTree) : registryKey.OpenSubKey(pageId.ToString(), writable: false);
			}
		}
		return null;
	}
	*/


	/*
	private void UpdateWindowIdCache()
	{
		try
		{
			if (_WindowIdsChecked)
			{
				return;
			}
			_WindowIdsChecked = true;
			using RegistryKey registryKey = UIHost.UserRegistryRoot;
			using RegistryKey registryKey2 = registryKey.CreateSubKey(m_teamNavWindowIdsKey, RegistryKeyPermissionCheck.ReadWriteSubTree);
			if (registryKey2 == null)
			{
				return;
			}
			string text = registryKey2.GetValue(null, null) as string;
			if (text == null || !string.Equals(text, _WindowIdVersion))
			{
				Trace.TraceEvent(TraceEventType.Information, EnUiTraceId.UiInfra, "DialogModel.UpdateWindowIds clearing window ID cache expectedVersion={0} foundVersion={1}", _WindowIdVersion, text ?? "null");
				string[] valueNames = registryKey2.GetValueNames();
				foreach (string name in valueNames)
				{
					registryKey2.DeleteValue(name);
				}
				registryKey2.SetValue(null, _WindowIdVersion);
			}
		}
		catch (Exception exception)
		{
			Trace.TraceException(TraceEventType.Error, EnUiTraceId.UiInfra, exception, "DialogModel.UpdateWindowIds", 430, "DialogModel.cs", "UpdateWindowIdCache");
		}
	}
	*/

	public int GetWindowIdForPage(Guid pageId)
	{
		/*
		UpdateWindowIdCache();
		using (RegistryKey registryKey = UIHost.UserRegistryRoot)
		{
			using RegistryKey registryKey2 = registryKey.CreateSubKey(m_teamNavWindowIdsKey, RegistryKeyPermissionCheck.ReadWriteSubTree);
			UiTracer.TraceSource.AssertTraceEvent(registryKey2 != null, TraceEventType.Error, EnUiTraceId.UiInfra, "windowIdsKey != null");
			if (registryKey2 != null)
			{
				int num = Math.Abs(pageId.GetHashCode());
				string text = pageId.ToString();
				num.ToString();
				string[] valueNames = registryKey2.GetValueNames();
				foreach (string text2 in valueNames)
				{
					if (int.TryParse(text2, out var result))
					{
						string text3 = registryKey2.GetValue(text2) as string;
						bool num2 = string.Equals(text3, text, StringComparison.OrdinalIgnoreCase);
						bool flag = result == num;
						if (num2 && flag)
						{
							return num;
						}
						if (flag)
						{
							Trace.TraceEvent(TraceEventType.Error, EnUiTraceId.UiInfra, "DialogModel.GetWindowIdForPage Collision detected for windowId={0} newPageId={1} existingPageId={2}", num, text, text3);
							return -1;
						}
					}
				}
				registryKey2.SetValue(num.ToString(), text);
				return num;
			}
		}
		*/
		return -1;
	}

	public Guid GetPageIdForWindowId(int windowId)
	{
		/*
		UpdateWindowIdCache();
		using (RegistryKey registryKey = UIHost.UserRegistryRoot)
		{
			using RegistryKey registryKey2 = registryKey.OpenSubKey(m_teamNavWindowIdsKey, writable: false);
			UiTracer.TraceSource.AssertTraceEvent(registryKey2 != null, TraceEventType.Error, EnUiTraceId.UiInfra, "windowIdsKey != null");
			if (registryKey2 != null)
			{
				string text = registryKey2.GetValue(windowId.ToString(), string.Empty) as string;
				if (!string.IsNullOrEmpty(text) && Guid.TryParse(text, out var result))
				{
					return result;
				}
			}
		}
		*/
		return Guid.Empty;
	}

	public EnPageWindowState GetPageWindowState(Guid pageId)
	{
		/*
		using (RegistryKey registryKey = GetPageDataRegKey(pageId, writable: false))
		{
			if (registryKey != null)
			{
				int num = (int)registryKey.GetValue("WindowState", 0);
				if (num >= 0 && num <= 1)
				{
					return (PageWindowState)num;
				}
			}
		}
		*/
		return EnPageWindowState.Default;
	}

	public void SetPageWindowState(Guid pageId, EnPageWindowState value)
	{
		/*
		using RegistryKey registryKey = GetPageDataRegKey(pageId, writable: true);
		registryKey?.SetValue("WindowState", (int)value, RegistryValueKind.DWord);
		*/
	}

	public void DiscoverPlugins()
	{
		if (_DiscoveryWorker == null)
		{
			_DiscoveryWorker = new BackgroundWorker
			{
				WorkerReportsProgress = false,
				WorkerSupportsCancellation = true
			};
			_DiscoveryWorker.DoWork += DiscoveryWorker_DoWork;
			_DiscoveryWorker.RunWorkerCompleted += DiscoveryWorker_RunWorkerCompleted;
			_DiscoveryWorker.RunWorkerAsync();
		}
	}

	public void CancelDiscoverPlugins()
	{
		if (_DiscoveryWorker != null && _DiscoveryWorker.IsBusy)
		{
			_DiscoveryWorker.CancelAsync();
		}
	}

	private bool IsCanceled(DoWorkEventArgs e)
	{
		if (_DiscoveryWorker != null && e != null && _DiscoveryWorker.CancellationPending)
		{
			e.Cancel = true;
			return true;
		}
		return false;
	}

	private void DiscoveryWorker_DoWork(object sender, DoWorkEventArgs e)
	{
		try
		{
			Trace.TraceEvent(TraceEventType.Information, EnUiTraceId.UiInfra, "Start plugin discovery");
			LoadPluginTypes(out var sections, e);
			foreach (Lazy<ISection, IBExportableMetadata> value in sections.Values)
			{
				if (IsCanceled(e))
				{
					return;
				}
				try
				{
					Guid sectionId = new Guid(value.Metadata.Id);
					RegisterSection(sectionId, value, value.Metadata.Priority);
				}
				catch (Exception exception)
				{
					Trace.TraceException(TraceEventType.Error, EnUiTraceId.UiInfra, exception, string.Format(CultureInfo.InvariantCulture, "Error registering section '{0}'", value.Metadata.Id), 683, "DialogModel.cs", "DiscoveryWorker_DoWork");
				}
			}
			Trace.TraceEvent(TraceEventType.Information, EnUiTraceId.UiInfra, "End plugin discovery");
		}
		catch (Exception ex)
		{
			Trace.TraceException(TraceEventType.Error, EnUiTraceId.UiInfra, ex, ex.Message, 692, "DialogModel.cs", "DiscoveryWorker_DoWork");
			// _DiscoveryError = ex;
		}
	}

	private void DiscoveryWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		try
		{
			if (_DeferredDispose)
			{
				Dispose();
				return;
			}
			IsDiscoveryCompleted = true;
			EventHandler<EventArgs> discoveryCompleted = DiscoveryCompleted;
			if (discoveryCompleted == null)
			{
				return;
			}
			Delegate[] invocationList = discoveryCompleted.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				EventHandler<EventArgs> eventHandler = (EventHandler<EventArgs>)invocationList[i];
				try
				{
					eventHandler(sender, EventArgs.Empty);
				}
				catch (Exception ex)
				{
					Trace.TraceException(TraceEventType.Error, EnUiTraceId.UiInfra, ex, ex.Message, 734, "DialogModel.cs", "DiscoveryWorker_RunWorkerCompleted");
				}
			}
		}
		catch (Exception exception)
		{
			Trace.TraceException(TraceEventType.Error, EnUiTraceId.UiInfra, exception, "DialogModel.DiscoveryWorker_RunWorkerCompleted", 742, "DialogModel.cs", "DiscoveryWorker_RunWorkerCompleted");
		}
	}

	public void SatisfyImportsOnce(object attributedPart)
	{
		IEnumerable<ExportableDescriptor<ISection>> serviceDescriptors = _DependencyManager.GetServiceDescriptors<ISection>();
		SectionCandidates = serviceDescriptors.Select((x) => new Lazy<ISection, IBExportableMetadata>(() => x.Exportable, x.Metadata));
	}

	private void LoadPluginTypes(out Dictionary<string, Lazy<ISection, IBExportableMetadata>> sections, DoWorkEventArgs e)
	{
		sections = new Dictionary<string, Lazy<ISection, IBExportableMetadata>>(StringComparer.OrdinalIgnoreCase);
		SatisfyImportsOnce(this);
		if (SectionCandidates == null)
		{
			return;
		}
		foreach (Lazy<ISection, IBExportableMetadata> sectionCandidate in SectionCandidates)
		{
			if (IsCanceled(e))
			{
				break;
			}
			try
			{
				LoadSectionType(sectionCandidate, sections);
			}
			catch (Exception exception)
			{
				Trace.TraceException(TraceEventType.Error, EnUiTraceId.UiInfra, exception, string.Format(CultureInfo.InvariantCulture, "Error loading section candidate via MEF: {0}", sectionCandidate.Metadata.Id), 810, "DialogModel.cs", "LoadPluginTypes");
			}
		}
	}

	private void LoadSectionType(Lazy<ISection, IBExportableMetadata> sectionCandidate, Dictionary<string, Lazy<ISection, IBExportableMetadata>> sections)
	{
		Trace.TraceEvent(TraceEventType.Information, EnUiTraceId.UiInfra, "Discovery: Section found section={0} pri={1}", sectionCandidate.Metadata.Id, sectionCandidate.Metadata.Priority);
		if (sections.TryGetValue(sectionCandidate.Metadata.Id, out var _))
		{
			InvalidOperationException ex = new($"TeamExplorer_SectionAlreadyRegistered: {sectionCandidate.Metadata.Id}");
			Diag.Dug(ex);
			throw ex;
		}
		sections[sectionCandidate.Metadata.Id] = sectionCandidate;
	}

	public void CreateSections(IBEventsChannel channel)
	{
		foreach (SectionRegInfo section in _Sections)
		{
			SectionHost sectionHost = new SectionHost(section);
			sectionHost.SectionCreated += SectionHost_SectionCreated;
			sectionHost.SectionInitialized += SectionHost_SectionInitialized;
			sectionHost.SectionClosing += SectionHost_SectionClosing;
			sectionHost.Create();
			Sections.Add(sectionHost);
			sectionHost.Initialize(channel);
		}
	}

	public void CloseSections()
	{
		foreach (SectionHost section in Sections)
		{
			section.RaiseSectionClosing();
			section.CloseSection();
		}
	}
}
