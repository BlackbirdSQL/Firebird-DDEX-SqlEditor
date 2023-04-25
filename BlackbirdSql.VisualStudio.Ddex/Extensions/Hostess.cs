using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft;
using Microsoft.VisualStudio.Data.Core;

using BlackbirdSql.Common;



namespace BlackbirdSql.VisualStudio.Ddex.Extensions;

internal class Hostess : IDisposable
{

	private IVsDataHostService _hostService;

	protected IVsDataHostService HostService => _hostService;

	public Hostess(System.IServiceProvider serviceProvider)
	{
		Diag.Dug();
		_hostService = serviceProvider.GetService(typeof(IVsDataHostService)) as IVsDataHostService;
		Assumes.Present(_hostService);
	}

	~Hostess()
	{
		Dispose(disposing: false);
	}



	public T TryGetService<T>()
	{
		Diag.Dug();
		return _hostService.TryGetService<T>();
	}

	public TInterface TryGetService<TService, TInterface>()
	{
		Diag.Dug();
		return _hostService.TryGetService<TService, TInterface>();
	}

	public T TryGetService<T>(Guid serviceGuid)
	{
		Diag.Dug();
		return _hostService.TryGetService<T>(serviceGuid);
	}

	public T GetService<T>()
	{
		Diag.Dug();
		return _hostService.GetService<T>();
	}

	public TInterface GetService<TService, TInterface>()
	{
		Diag.Dug();
		return _hostService.GetService<TService, TInterface>();
	}

	public T GetService<T>(Guid serviceGuid)
	{
		Diag.Dug();
		return _hostService.GetService<T>(serviceGuid);
	}

	void IDisposable.Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (disposing && _hostService != null)
		{
			/*
			if (_environment != null)
			{
				((IDisposable)_environment).Dispose();
				_environment = null;
			}
			*/

			_hostService = null;
			// _serviceProvider = null;
		}
	}

}
