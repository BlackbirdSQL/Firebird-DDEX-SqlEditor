#region Assembly Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;

using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Interfaces;


namespace BlackbirdSql.Common.Model;

public abstract class ViewModelBase : AbstractDispatcherConnection
{

	public const string C_KeyParentViewModel = "ParentViewModel";

	protected static new DescriberDictionary _Describers;

	private List<object> _Services;

	private readonly Dictionary<string, List<Lazy<IBOwnedCommand>>> _CommandDependencies;

	public override DescriberDictionary Describers
	{
		get
		{
			if (_Describers == null)
				CreateAndPopulatePropertySet(null);

			return _Describers;
		}
	}


	public ViewModelBase ParentViewModel
	{
		get { return (ViewModelBase)GetProperty(C_KeyParentViewModel); }
		set { SetProperty(C_KeyParentViewModel, value); }
	}



	public ViewModelBase(Dispatcher dispatcher, IBEventsChannel channel, ViewModelBase parentViewModel, ViewModelBase rhs)
		: base(dispatcher, channel, rhs, true)
	{
		ParentViewModel = parentViewModel;
		_CommandDependencies = null;
	}
	// Dispatcher.CurrentDispatcher

	public ViewModelBase(Dispatcher dispatcher, IBEventsChannel channel, ViewModelBase parentViewModel = null)
		: this(dispatcher, channel, parentViewModel, null)
	{
	}

	public ViewModelBase(IBEventsChannel channel, ViewModelBase parentViewModel = null)
		: this(Dispatcher.CurrentDispatcher, channel, parentViewModel, null)
	{
	}

	public ViewModelBase(ViewModelBase parentViewModel, ViewModelBase rhs) : this(Dispatcher.CurrentDispatcher, null, parentViewModel, rhs)
	{
	}

	public ViewModelBase(ViewModelBase parentViewModel = null) : this(Dispatcher.CurrentDispatcher, null, parentViewModel, null)
	{
	}

	protected static new void CreateAndPopulatePropertySet(DescriberDictionary describers = null)
	{
		if (_Describers == null)
		{
			_Describers = new();

			// Initializers for property sets are held externally for this class
			AbstractDispatcherConnection.CreateAndPopulatePropertySet(_Describers);

			_Describers.Add(C_KeyParentViewModel, typeof(object), false);
		}

		// If null then this was a call from our own .ctor so no need to pass anything back
		describers?.AddRange(_Describers);

	}


	public void RegisterService(object service)
	{
		if (service == null)
		{
			ArgumentNullException ex = new("service");
			Diag.Dug(ex);
			throw ex;
		}

		if (service.GetType().IsValueType)
		{
			ArgumentException ex = new("Service must be implemented by reference type.", "service");
			Diag.Dug(ex);
			throw ex;
		}

		lock (_LockObject)
		{
			_Services ??= new List<object>();

			_Services.Add(service);
		}
	}

	public TService ResolveService<TService>() where TService : class
	{
		TService val = TryResolveService<TService>();
		if (val == null)
		{
			string message = string.Format("Service not found: {0}.\nMake sure that '{1}' is in your .xaml file.", typeof(TService).FullName, "mvvm:MVVMSupport.ViewModel=\"{Binding}\"");
			UiTracer.TraceSource.AssertTraceEvent(condition: false, TraceEventType.Error, EnUiTraceId.UiInfra, message);
			Exception ex = new(message);
			Diag.Dug(ex);
			throw ex;
		}

		return val;
	}



	public TService TryResolveService<TService>() where TService : class
	{
		return (TService)TryResolveService(typeof(TService));
	}



	public object TryResolveService(Type serviceType)
	{
		lock (_LockObject)
		{
			if (_Services != null)
			{
				for (int num = _Services.Count - 1; num >= 0; num--)
				{
					object obj = _Services[num];
					if (obj is IServiceProvider serviceProvider)
					{
						object service = serviceProvider.GetService(serviceType);
						if (service != null)
						{
							return service;
						}
					}

					Type type = obj.GetType();
					if (serviceType.IsAssignableFrom(type))
					{
						return obj;
					}
				}
			}
		}

		return ParentViewModel?.TryResolveService(serviceType);
	}

	public bool UnRegisterService(object service)
	{
		if (service == null)
		{
			ArgumentNullException ex = new("service");
			Diag.Dug(ex);
			throw ex;
		}

		if (service.GetType().IsValueType)
		{
			ArgumentException ex = new("Service must be implemented by reference type.", "service");
			Diag.Dug(ex);
			throw ex;
		}

		lock (_LockObject)
		{
			if (_Services == null)
				return false;

			return _Services.Remove(service);
		}
	}

	public void UnRegisterServices(IEnumerable services)
	{
		if (services == null)
		{
			ArgumentNullException ex = new("services");
			Diag.Dug(ex);
			throw ex;
		}

		foreach (object service in services)
		{
			UnRegisterService(service);
		}
	}

	public void RegisterServices(IEnumerable services)
	{
		if (services == null)
		{
			ArgumentNullException ex = new("services");
			Diag.Dug(ex);
			throw ex;
		}

		foreach (object service in services)
		{
			RegisterService(service);
		}
	}




	public override void RaisePropertyChanged(string propertyName)
	{
		base.RaisePropertyChanged(propertyName);
		if (_CommandDependencies == null || !_CommandDependencies.TryGetValue(propertyName, out var value))
		{
			return;
		}

		foreach (IBOwnedCommand item in value.Select((command) => command.Value))
		{
			UiTracer.TraceSource.AssertTraceEvent(item != null, TraceEventType.Error, EnUiTraceId.UiInfra, "dependentCommand != null");
			item?.RaiseCanExecuteChanged();
		}
	}


}
