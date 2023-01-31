/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/BlackbirdSQL/NETProvider/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$OriginalAuthors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using BlackbirdSql.Common;
using BlackbirdSql.Data.Common;
using BlackbirdSql.Data.DslClient;

namespace BlackbirdSql.Data.Services;

public sealed class FbServerProperties : FbService
{
	public FbServerProperties(string connectionString = null) : base(connectionString)
	{
		Diag.Trace("ConnectionString: " + (connectionString == null ? "null" : connectionString) );
	}

	public int GetVersion()
	{
		Diag.Trace();

		return GetInt32(IscCodes.isc_info_svc_version);
	}
	public Task<int> GetVersionAsync(CancellationToken cancellationToken = default)
	{
		Diag.Trace();

		return GetInt32Async(IscCodes.isc_info_svc_version, cancellationToken);
	}

	public string GetServerVersion()
	{
		Diag.Trace();

		return GetString(IscCodes.isc_info_svc_server_version);
	}
	public Task<string> GetServerVersionAsync(CancellationToken cancellationToken = default)
	{
		Diag.Trace();

		return GetStringAsync(IscCodes.isc_info_svc_server_version, cancellationToken);
	}

	public string GetImplementation()
	{
		Diag.Trace();

		return GetString(IscCodes.isc_info_svc_implementation);
	}
	public Task<string> GetImplementationAsync(CancellationToken cancellationToken = default)
	{
		Diag.Trace();

		return GetStringAsync(IscCodes.isc_info_svc_implementation, cancellationToken);
	}

	public string GetRootDirectory()
	{
		Diag.Trace();

		return GetString(IscCodes.isc_info_svc_get_env);
	}
	public Task<string> GetRootDirectoryAsync(CancellationToken cancellationToken = default)
	{
		Diag.Trace();

		return GetStringAsync(IscCodes.isc_info_svc_get_env, cancellationToken);
	}

	public string GetLockManager()
	{
		Diag.Trace();

		return GetString(IscCodes.isc_info_svc_get_env_lock);
	}
	public Task<string> GetLockManagerAsync(CancellationToken cancellationToken = default)
	{
		Diag.Trace();

		return GetStringAsync(IscCodes.isc_info_svc_get_env_lock, cancellationToken);
	}

	public string GetMessageFile()
	{
		Diag.Trace();

		return GetString(IscCodes.isc_info_svc_get_env_msg);
	}
	public Task<string> GetMessageFileAsync(CancellationToken cancellationToken = default)
	{
		Diag.Trace();

		return GetStringAsync(IscCodes.isc_info_svc_get_env_msg, cancellationToken);
	}

	public FbDatabasesInfo GetDatabasesInfo()
	{
		Diag.Trace();

		return (FbDatabasesInfo)(GetInfo(IscCodes.isc_info_svc_svr_db_info)).FirstOrDefault() ?? new FbDatabasesInfo();
	}
	public async Task<FbDatabasesInfo> GetDatabasesInfoAsync(CancellationToken cancellationToken = default)
	{
		Diag.Trace();

		return (FbDatabasesInfo)(await GetInfoAsync(IscCodes.isc_info_svc_svr_db_info, cancellationToken).ConfigureAwait(false)).FirstOrDefault() ?? new FbDatabasesInfo();
	}

	public FbServerConfig GetServerConfig()
	{
		Diag.Trace();

		return (FbServerConfig)(GetInfo(IscCodes.isc_info_svc_get_config)).FirstOrDefault() ?? new FbServerConfig();
	}
	public async Task<FbServerConfig> GetServerConfigAsync(CancellationToken cancellationToken = default)
	{
		Diag.Trace();

		return (FbServerConfig)(await GetInfoAsync(IscCodes.isc_info_svc_get_config, cancellationToken).ConfigureAwait(false)).FirstOrDefault() ?? new FbServerConfig();
	}

	private string GetString(int item)
	{
		Diag.Trace();

		return (string)(GetInfo(item)).FirstOrDefault();
	}
	private async Task<string> GetStringAsync(int item, CancellationToken cancellationToken = default)
	{
		Diag.Trace();

		return (string)(await GetInfoAsync(item, cancellationToken).ConfigureAwait(false)).FirstOrDefault();
	}

	private int GetInt32(int item)
	{
		Diag.Trace();

		return (int)(GetInfo(item)).FirstOrDefault();
	}
	private async Task<int> GetInt32Async(int item, CancellationToken cancellationToken = default)
	{
		Diag.Trace();

		return (int)(await GetInfoAsync(item, cancellationToken).ConfigureAwait(false)).FirstOrDefault();
	}

	private List<object> GetInfo(int item)
	{
		Diag.Trace();

		return GetInfo(new byte[] { (byte)item });
	}
	private Task<List<object>> GetInfoAsync(int item, CancellationToken cancellationToken = default)
	{
		Diag.Trace();

		return GetInfoAsync(new byte[] { (byte)item }, cancellationToken);
	}

	private List<object> GetInfo(byte[] items)
	{
		Diag.Trace();

		try
		{
			try
			{
				Open();
				return Query(items, new ServiceParameterBuffer2(Service.ParameterBufferEncoding));
			}
			finally
			{
				Close();
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}
	}
	private async Task<List<object>> GetInfoAsync(byte[] items, CancellationToken cancellationToken = default)
	{
		Diag.Trace();

		try
		{
			try
			{
				await OpenAsync(cancellationToken).ConfigureAwait(false);
				return await QueryAsync(items, new ServiceParameterBuffer2(Service.ParameterBufferEncoding), cancellationToken).ConfigureAwait(false);
			}
			finally
			{
				await CloseAsync(cancellationToken).ConfigureAwait(false);
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw DslException.Create(ex);
		}
	}

	public static Version ParseServerVersion(string version)
	{
		Diag.Trace();

		var m = Regex.Match(version, @"\w{2}-\w(\d+\.\d+\.\d+\.\d+)");
		if (!m.Success)
			return null;
		return new Version(m.Groups[1].Value);
	}
}
