#region Assembly Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections.Generic;

using BlackbirdSql.Core;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;

namespace BlackbirdSql.Common.Ctl;



public class DependencyManager : IBDependencyManager
{
	// private IBTrace _trace;

	// private readonly ConcurrentDictionary<Type, object> _extensionLoaders = new ConcurrentDictionary<Type, object>();

	// private readonly ExtensionProperties _serviceProperties;

	public DependencyManager(ExtensionProperties serviceProperties)
	{
		if (serviceProperties == null)
		{
			ArgumentNullException ex = new("serviceProperties");
			Diag.Dug(ex);
			throw ex;
		}

		// _serviceProperties = serviceProperties;
		Initialize();
	}

	/*
	public ExtensionLoader<T> Register<T>() where T : IBExportable
	{
		return _extensionLoaders.GetOrAdd(typeof(T), (Type key) => CreateServiceLoader<T>(_trace)) as ExtensionLoader<T>;
	}
	*/
	public IEnumerable<ExportableDescriptor<T>> GetServiceDescriptors<T>(IBServerDefinition serviceMetadata = null) where T : IBExportable
	{
		return null;
		// return GetAllServiceDescriptors<T>()?.FilterDescriptors(serviceMetadata);
	}


	private void Initialize()
	{
		/*
		ExtensionLoader<IBTrace> extensionLoader = Register<IBTrace>();
		_trace = GetService<IBTrace>();
		if (_trace != null)
		{
			_trace.DependencyManager = this;
		}

		extensionLoader.Trace = _trace;
		Register<IServerDiscoveryProvider>();
		Register<IAccountManager>();
		Register<IDatabaseDiscoveryProvider>();
		Register<IAzureResourceManagerWrapper>();
		*/
	}

	/*
	private ExtensionLoader<T> CreateServiceLoader<T>(IBTrace trace) where T : IBExportable
	{
		return new ExtensionLoader<T>(_serviceProperties)
		{
			// Trace = trace
		};
	}
	*/

	/*
	public T GetService<T>() where T : IBExportable
	{
		return GetAllServiceDescriptors<T>().ToList().Filter();
	}
	*/

	/*
	private IEnumerable<ExportableDescriptor<T>> GetAllServiceDescriptors<T>() where T : IBExportable
	{
		IList<ExtensibilityError> typeLoadErrors;
		return GetLoader<T>()?.GetAllServices(out typeLoadErrors).Select(delegate (ExportableDescriptor<T> x)
		{
			T exportable = x.Exportable;
			exportable.DependencyManager = this;
			return x;
		});
	}
	*/

	/*
	private ExtensionLoader<T> GetLoader<T>() where T : IBExportable
	{
		Type typeFromHandle = typeof(T);
		_extensionLoaders.TryGetValue(typeFromHandle, out var value);
		if (value == null)
		{
			value = Register<T>();
		}

		return value as ExtensionLoader<T>;
	}
	*/
}
