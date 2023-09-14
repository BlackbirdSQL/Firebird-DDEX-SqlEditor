// Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.Core.ServiceManager<T>

using System;
using System.Collections.Generic;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;


namespace BlackbirdSql.Common.Ctl;

public class ServiceManager<T> where T : IBExportable
{
	private readonly IEnumerable<ExportableDescriptor<T>> _Services;

	private readonly IBDependencyManager _DependencyManager;

	public ServiceManager(IBDependencyManager dependencyManager)
	{
		_DependencyManager = dependencyManager ?? throw new ArgumentNullException("dependencyManager");
		_Services = dependencyManager.GetServiceDescriptors<T>();
	}

	public bool RequiresUserAccount(IBServerDefinition serverDefinition)
	{
		ExportableDescriptor<T> serviceDescriptor = GetServiceDescriptor(serverDefinition);
		if (serviceDescriptor != null)
		{
			return IsSecureService(serviceDescriptor);
		}
		return false;
	}

	/*
	public IAccountManager GetAccountManager(IBServerDefinition serverDefinition)
	{
		ExportableDescriptor<T> serviceDescriptor = GetServiceDescriptor(serverDefinition);
		if (serviceDescriptor != null)
		{
			if (!((object)serviceDescriptor.Exportable is ISecureService secureService))
			{
				return null;
			}
			return secureService.AccountManager;
		}
		return null;
	}
	*/

	public T GetService(IBServerDefinition serverDefinition)
	{
		ExportableDescriptor<T> serviceDescriptor = GetServiceDescriptor(serverDefinition);
		if (serviceDescriptor != null)
		{
			T exportable = serviceDescriptor.Exportable;
			exportable.DependencyManager = _DependencyManager;
			return serviceDescriptor.Exportable;
		}
		return default;
	}

	private ExportableDescriptor<T> GetServiceDescriptor(IBServerDefinition serverDefinition)
	{
		if (_Services == null)
		{
			return null;
		}
		return _Services.FindMatchedDescriptor(serverDefinition);
	}

	private bool IsSecureService<TService>(ExportableDescriptor<TService> exportableDescriptor) where TService : IBExportable
	{
		return false;
		// return exportableDescriptor.Exportable is ISecureService;
	}
}
