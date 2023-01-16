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
using System.Threading;
using System.Threading.Tasks;

using BlackbirdSql.Common;
using BlackbirdSql.Data.Client.Managed;
using BlackbirdSql.Data.Common;
using WireCryptOption = BlackbirdSql.Data.Client.Managed.Version13.WireCryptOption;

namespace BlackbirdSql.Data.Client;

internal static class ClientFactory
{
	public static DatabaseBase CreateDatabase(ConnectionString options)
	{
		Diag.Trace();

		return options.ServerType switch
		{
			ServerType.Default => CreateManagedDatabase(options),
			ServerType.Embedded => CreateNativeDatabase(options),
			_ => throw IncorrectServerTypeException(),
		};
	}
	public static ValueTask<DatabaseBase> CreateDatabaseAsync(ConnectionString options, CancellationToken cancellationToken = default)
	{
		Diag.Trace();

		return options.ServerType switch
		{
			ServerType.Default => CreateManagedDatabaseAsync(options, cancellationToken),
			ServerType.Embedded => CreateNativeDatabaseAsync(options),
			_ => throw IncorrectServerTypeException(),
		};
	}

	public static ServiceManagerBase CreateServiceManager(ConnectionString options)
	{
		Diag.Trace();

		return options.ServerType switch
		{
			ServerType.Default => CreateManagedServiceManager(options),
			ServerType.Embedded => CreateNativeServiceManager(options),
			_ => throw IncorrectServerTypeException(),
		};
	}
	public static ValueTask<ServiceManagerBase> CreateServiceManagerAsync(ConnectionString options, CancellationToken cancellationToken = default)
	{
		Diag.Trace();

		return options.ServerType switch
		{
			ServerType.Default => CreateManagedServiceManagerAsync(options, cancellationToken),
			ServerType.Embedded => CreateNativeServiceManagerAsync(options),
			_ => throw IncorrectServerTypeException(),
		};
	}

	private static DatabaseBase CreateManagedDatabase(ConnectionString options)
	{
		Diag.Trace();

		var charset = GetCharset(options);

		var connection = new GdsConnection(options.UserID, options.Password, options.DataSource, options.Port, options.ConnectionTimeout, options.PacketSize, charset, options.Dialect, options.Compression, FbWireCryptToWireCryptOption(options.WireCrypt), options.CryptKey);
		connection.Connect();
		try
		{
			connection.Identify(options.Database);
		}
		catch
		{
			connection.Disconnect();
			throw;
		}
		return connection.ProtocolVersion switch
		{
			IscCodes.PROTOCOL_VERSION16 => new Managed.Version16.GdsDatabase(connection),
			IscCodes.PROTOCOL_VERSION15 => new Managed.Version15.GdsDatabase(connection),
			IscCodes.PROTOCOL_VERSION13 => new Managed.Version13.GdsDatabase(connection),
			IscCodes.PROTOCOL_VERSION12 => new Managed.Version12.GdsDatabase(connection),
			IscCodes.PROTOCOL_VERSION11 => new Managed.Version11.GdsDatabase(connection),
			IscCodes.PROTOCOL_VERSION10 => new Managed.Version10.GdsDatabase(connection),
			_ => throw UnsupportedProtocolException(),
		};
	}
	private static async ValueTask<DatabaseBase> CreateManagedDatabaseAsync(ConnectionString options, CancellationToken cancellationToken = default)
	{
		Diag.Trace();

		var charset = GetCharset(options);

		var connection = new GdsConnection(options.UserID, options.Password, options.DataSource, options.Port, options.ConnectionTimeout, options.PacketSize, charset, options.Dialect, options.Compression, FbWireCryptToWireCryptOption(options.WireCrypt), options.CryptKey);
		await connection.ConnectAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			await connection.IdentifyAsync(options.Database, cancellationToken).ConfigureAwait(false);
		}
		catch
		{
			await connection.DisconnectAsync(cancellationToken).ConfigureAwait(false);
			throw;
		}
		return connection.ProtocolVersion switch
		{
			IscCodes.PROTOCOL_VERSION16 => new Managed.Version16.GdsDatabase(connection),
			IscCodes.PROTOCOL_VERSION15 => new Managed.Version15.GdsDatabase(connection),
			IscCodes.PROTOCOL_VERSION13 => new Managed.Version13.GdsDatabase(connection),
			IscCodes.PROTOCOL_VERSION12 => new Managed.Version12.GdsDatabase(connection),
			IscCodes.PROTOCOL_VERSION11 => new Managed.Version11.GdsDatabase(connection),
			IscCodes.PROTOCOL_VERSION10 => new Managed.Version10.GdsDatabase(connection),
			_ => throw UnsupportedProtocolException(),
		};
	}

	private static DatabaseBase CreateNativeDatabase(ConnectionString options)
	{
		Diag.Trace();

		var charset = GetCharset(options);

		return new Native.FesDatabase(options.ClientLibrary, charset, options.PacketSize, options.Dialect);
	}
	private static ValueTask<DatabaseBase> CreateNativeDatabaseAsync(ConnectionString options)
	{
		Diag.Trace();

		var charset = GetCharset(options);

		return ValueTask2.FromResult<DatabaseBase>(new Native.FesDatabase(options.ClientLibrary, charset, options.PacketSize, options.Dialect));
	}

	private static ServiceManagerBase CreateManagedServiceManager(ConnectionString options)
	{
		Diag.Trace();

		var charset = GetCharset(options);

		var connection = new GdsConnection(options.UserID, options.Password, options.DataSource, options.Port, options.ConnectionTimeout, options.PacketSize, charset, options.Dialect, options.Compression, FbWireCryptToWireCryptOption(options.WireCrypt), options.CryptKey);
		connection.Connect();
		try
		{
			connection.Identify(!string.IsNullOrEmpty(options.Database) ? options.Database : string.Empty);
		}
		catch
		{
			connection.Disconnect();
			throw;
		}
		return connection.ProtocolVersion switch
		{
			IscCodes.PROTOCOL_VERSION16 => new Managed.Version16.GdsServiceManager(connection),
			IscCodes.PROTOCOL_VERSION15 => new Managed.Version15.GdsServiceManager(connection),
			IscCodes.PROTOCOL_VERSION13 => new Managed.Version13.GdsServiceManager(connection),
			IscCodes.PROTOCOL_VERSION12 => new Managed.Version12.GdsServiceManager(connection),
			IscCodes.PROTOCOL_VERSION11 => new Managed.Version11.GdsServiceManager(connection),
			IscCodes.PROTOCOL_VERSION10 => new Managed.Version10.GdsServiceManager(connection),
			_ => throw UnsupportedProtocolException(),
		};
	}
	private static async ValueTask<ServiceManagerBase> CreateManagedServiceManagerAsync(ConnectionString options, CancellationToken cancellationToken = default)
	{
		Diag.Trace();

		var charset = GetCharset(options);

		var connection = new GdsConnection(options.UserID, options.Password, options.DataSource, options.Port, options.ConnectionTimeout, options.PacketSize, charset, options.Dialect, options.Compression, FbWireCryptToWireCryptOption(options.WireCrypt), options.CryptKey);
		await connection.ConnectAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			await connection.IdentifyAsync(!string.IsNullOrEmpty(options.Database) ? options.Database : string.Empty, cancellationToken).ConfigureAwait(false);
		}
		catch
		{
			await connection.DisconnectAsync(cancellationToken).ConfigureAwait(false);
			throw;
		}
		return connection.ProtocolVersion switch
		{
			IscCodes.PROTOCOL_VERSION16 => new Managed.Version16.GdsServiceManager(connection),
			IscCodes.PROTOCOL_VERSION15 => new Managed.Version15.GdsServiceManager(connection),
			IscCodes.PROTOCOL_VERSION13 => new Managed.Version13.GdsServiceManager(connection),
			IscCodes.PROTOCOL_VERSION12 => new Managed.Version12.GdsServiceManager(connection),
			IscCodes.PROTOCOL_VERSION11 => new Managed.Version11.GdsServiceManager(connection),
			IscCodes.PROTOCOL_VERSION10 => new Managed.Version10.GdsServiceManager(connection),
			_ => throw UnsupportedProtocolException(),
		};
	}

	private static ServiceManagerBase CreateNativeServiceManager(ConnectionString options)
	{
		Diag.Trace();

		var charset = GetCharset(options);

		return new Native.FesServiceManager(options.ClientLibrary, charset);
	}
	private static ValueTask<ServiceManagerBase> CreateNativeServiceManagerAsync(ConnectionString options)
	{
		Diag.Trace();

		var charset = GetCharset(options);

		return ValueTask2.FromResult<ServiceManagerBase>(new Native.FesServiceManager(options.ClientLibrary, charset));
	}

	private static Exception UnsupportedProtocolException() => new NotSupportedException("Protocol not supported.");
	private static Exception IncorrectServerTypeException() => new NotSupportedException("Specified server type is not correct.");
	private static Exception InvalidCharsetException() => new ArgumentException("Invalid character set specified.");

	private static Charset GetCharset(ConnectionString options)
	{
		if (!Charset.TryGetByName(options.Charset, out var charset))
			throw InvalidCharsetException();
		return charset;
	}

	private static WireCryptOption FbWireCryptToWireCryptOption(WireCrypt wireCrypt)
	{
		Diag.Trace();


		switch (wireCrypt)
		{
			case WireCrypt.Disabled:
				return WireCryptOption.Disabled;
			case WireCrypt.Enabled:
				return WireCryptOption.Enabled;
			case WireCrypt.Required:
				return WireCryptOption.Required;
			default:
				ArgumentOutOfRangeException ex = new ArgumentOutOfRangeException(nameof(wireCrypt), $"{nameof(wireCrypt)}={wireCrypt}");
				Diag.Dug(ex);
				throw ex;
		};
	}
}
