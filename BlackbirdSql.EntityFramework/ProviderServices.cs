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

//$OriginalAuthors = Jiri Cincura (jiri@cincura.net)

using System;
using System.Data;
using System.Data.Common;

#if EF6 || NET
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Migrations.Sql;
#else
using System.Data.Common.CommandTrees;
using System.Data.Metadata.Edm;
#endif

using System.Diagnostics;
using System.Linq;

using BlackbirdSql.Common;
using BlackbirdSql.Data.DslClient;
using BlackbirdSql.Data.Isql;
using BlackbirdSql.Data.Services;

#if EF6 || NET
using BlackbirdSql.Data.Entity.Configuration;
using BlackbirdSql.Data.Entity.Helpers;
using BlackbirdSql.Data.Entity.Sql;
#else
using BlackbirdSql.Data.Configuration;
using BlackbirdSql.Data.Helpers;
using BlackbirdSql.Data.Sql;
#endif

#if EF6
using BlackbirdSql.Data.Entity.Migrations;
#endif




#if EF6 || NET
namespace BlackbirdSql.Data.Entity;
#else
namespace BlackbirdSql.Data;
#endif


public class ProviderServices : DbProviderServices
{
	public const string Invariant = PackageData.Invariant;
	public static readonly ProviderServices Instance = new ProviderServices();

	public ProviderServices()
	{

#if EF6
		Diag.Trace("In EF6");
		AddDependencyResolver(new SingletonDependencyResolver<IDbConnectionFactory>(new ConnectionFactory()));
		AddDependencyResolver(new SingletonDependencyResolver<Func<MigrationSqlGenerator>>(() => new EfbMigrationSqlGenerator(), PackageData.Invariant));
		DbInterception.Add(new EfbMigrationsTransactionsInterceptor());
#else
		Diag.Trace("In DslClient");
#endif
	}

	public string SayHello()
	{
		return "I'm saying hello";
	}

	protected override DbCommandDefinition CreateDbCommandDefinition(DbProviderManifest manifest, DbCommandTree commandTree)
	{
		Diag.Trace();
		var prototype = CreateCommand(manifest, commandTree);
		var result = CreateCommandDefinition(prototype);
		return result;
	}

	private DbCommand CreateCommand(DbProviderManifest manifest, DbCommandTree commandTree)
	{
		Diag.Trace();
		if (manifest == null)
			throw new ArgumentNullException(nameof(manifest));

		if (commandTree == null)
			throw new ArgumentNullException(nameof(commandTree));

		var expectedTypes = PrepareTypeCoercions(commandTree);

		var command = DslCommand.CreateWithTypeCoercions(expectedTypes);

		command.CommandText = SqlGenerator.GenerateSql(commandTree, out var parameters, out var commandType);
		command.CommandType = commandType;

		// Get the function (if any) implemented by the command tree since this influences our interpretation of parameters
		EdmFunction function = null;
#pragma warning disable IDE0038 // Use pattern matching
		if (commandTree is DbFunctionCommandTree)
		{
			function = ((DbFunctionCommandTree)commandTree).EdmFunction;
		}
#pragma warning restore IDE0038 // Use pattern matching

		// Now make sure we populate the command's parameters from the CQT's parameters:
		foreach (var queryParameter in commandTree.Parameters)
		{
			DslParameter parameter;

			// Use the corresponding function parameter TypeUsage where available (currently, the SSDL facets and
			// type trump user-defined facets and type in the EntityCommand).
			if (null != function && function.Parameters.TryGetValue(queryParameter.Key, false, out var functionParameter))
			{
				parameter = CreateSqlParameter(functionParameter.Name, functionParameter.TypeUsage, functionParameter.Mode, DBNull.Value);
			}
			else
			{
				parameter = CreateSqlParameter(queryParameter.Key, queryParameter.Value, ParameterMode.In, DBNull.Value);
			}

			command.Parameters.Add(parameter);
		}

		// Now add parameters added as part of SQL gen (note: this feature is only safe for DML SQL gen which
		// does not support user parameters, where there is no risk of name collision)
		if (null != parameters && 0 < parameters.Count)
		{
			if (commandTree is not DbInsertCommandTree and
				not DbUpdateCommandTree and
				not DbDeleteCommandTree)
			{
				Diag.Dug(true, "SqlGenParametersNotPermitted");
				throw new InvalidOperationException("SqlGenParametersNotPermitted");
			}

			foreach (var parameter in parameters)
			{
				command.Parameters.Add(parameter);
			}
		}

		return command;
	}

	protected override string GetDbProviderManifestToken(DbConnection connection)
	{
		Diag.Trace();

		try
		{
			var serverVersion = default(Version);
			if (connection.State == ConnectionState.Open)
			{
				serverVersion = FbServerProperties.ParseServerVersion(connection.ServerVersion);
			}
			else
			{
				var serverProperties = new FbServerProperties() { ConnectionString = connection.ConnectionString };
				serverVersion = FbServerProperties.ParseServerVersion(serverProperties.GetServerVersion());
			}
			return serverVersion.ToString(2);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw new InvalidOperationException("Could not retrieve storage version.", ex);
		}
	}

	protected override DbProviderManifest GetDbProviderManifest(string versionHint)
	{
		Diag.Trace();

		if (string.IsNullOrEmpty(versionHint))
		{
			Diag.Dug(true, "Could not determine store version; a valid store connection or a version hint is required.");
			throw new ArgumentException("Could not determine store version; a valid store connection or a version hint is required.");
		}
		return new ProviderManifest(versionHint);
	}

	internal static DslParameter CreateSqlParameter(string name, TypeUsage type, ParameterMode mode, object value)
	{
		Diag.Trace();

		var result = new DslParameter(name, value);

		var direction = MetadataHelpers.ParameterModeToParameterDirection(mode);
		if (result.Direction != direction)
		{
			result.Direction = direction;
		}

		// output parameters are handled differently (we need to ensure there is space for return
		// values where the user has not given a specific Size/MaxLength)
		var isOutParam = mode != ParameterMode.In;
		var sqlDbType = GetSqlDbType(type, isOutParam, out var size);

		if (result.FbDbType != sqlDbType)
		{
			result.FbDbType = sqlDbType;
		}

		// Note that we overwrite 'facet' parameters where either the value is different or
		// there is an output parameter.
		if (size.HasValue && (isOutParam || result.Size != size.Value))
		{
			result.Size = size.Value;
		}

		var isNullable = MetadataHelpers.IsNullable(type);
		if (isOutParam || isNullable != result.IsNullable)
		{
			result.IsNullable = isNullable;
		}

		return result;
	}

	private static DslDbType GetSqlDbType(TypeUsage type, bool isOutParam, out int? size)
	{
		Diag.Trace();

		// only supported for primitive type
		var primitiveTypeKind = MetadataHelpers.GetPrimitiveTypeKind(type);

		size = default;

		switch (primitiveTypeKind)
		{
			case PrimitiveTypeKind.Boolean:
				return DslDbType.SmallInt;

			case PrimitiveTypeKind.Int16:
				return DslDbType.SmallInt;

			case PrimitiveTypeKind.Int32:
				return DslDbType.Integer;

			case PrimitiveTypeKind.Int64:
				return DslDbType.BigInt;

			case PrimitiveTypeKind.Double:
				return DslDbType.Double;

			case PrimitiveTypeKind.Single:
				return DslDbType.Float;

			case PrimitiveTypeKind.Decimal:
				return DslDbType.Decimal;

			case PrimitiveTypeKind.Binary:
				// for output parameters, ensure there is space...
				size = GetParameterSize(type, isOutParam);
				return GetBinaryDbType(type);

			case PrimitiveTypeKind.String:
				size = GetParameterSize(type, isOutParam);
				return GetStringDbType(type);

			case PrimitiveTypeKind.DateTime:
				return DslDbType.TimeStamp;

			case PrimitiveTypeKind.Time:
				return DslDbType.Time;

			case PrimitiveTypeKind.Guid:
				return DslDbType.Guid;

			default:
				Debug.Fail("unknown PrimitiveTypeKind " + primitiveTypeKind);
				Diag.Dug(true, "unknown PrimitiveTypeKind " + primitiveTypeKind);
				throw new InvalidOperationException("unknown PrimitiveTypeKind " + primitiveTypeKind);
		}
	}

	private static int? GetParameterSize(TypeUsage type, bool isOutParam)
	{
		Diag.Trace();
		if (MetadataHelpers.TryGetMaxLength(type, out var maxLength))
		{
			// if the MaxLength facet has a specific value use it
			return maxLength;
		}
		else if (isOutParam)
		{
			// if the parameter is a return/out/inout parameter, ensure there
			// is space for any value
			return int.MaxValue;
		}
		else
		{
			// no value
			return default;
		}
	}

	private static DslDbType GetStringDbType(TypeUsage type)
	{
		Diag.Trace();

		Debug.Assert(type.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType && PrimitiveTypeKind.String == ((PrimitiveType)type.EdmType).PrimitiveTypeKind, "only valid for string type");

		DslDbType dbType;
		// Specific type depends on whether the string is a unicode string and whether it is a fixed length string.
		// By default, assume widest type (unicode) and most common type (variable length)
		if (!MetadataHelpers.TryGetIsFixedLength(type, out var fixedLength))
		{
			fixedLength = false;
		}

		if (!MetadataHelpers.TryGetIsUnicode(type, out var unicode))
		{
			unicode = true;
		}

		if (fixedLength)
		{
			dbType = unicode ? DslDbType.Char : DslDbType.Char;
		}
		else
		{
			if (!MetadataHelpers.TryGetMaxLength(type, out var maxLength))
			{
				maxLength = unicode ? ProviderManifest.UnicodeVarcharMaxSize : ProviderManifest.AsciiVarcharMaxSize;
			}
			if (maxLength == default || maxLength > (unicode ? ProviderManifest.UnicodeVarcharMaxSize : ProviderManifest.AsciiVarcharMaxSize))
			{
				dbType = DslDbType.Text;
			}
			else
			{
				dbType = unicode ? DslDbType.VarChar : DslDbType.VarChar;
			}
		}

		return dbType;
	}

	private static DslDbType GetBinaryDbType(TypeUsage type)
	{
		Diag.Trace();

		Debug.Assert(type.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType &&
			PrimitiveTypeKind.Binary == ((PrimitiveType)type.EdmType).PrimitiveTypeKind, "only valid for binary type");

		// Specific type depends on whether the binary value is fixed length. By default, assume variable length.
		//bool fixedLength;
		//if (!MetadataHelpers.TryGetIsFixedLength(type, out fixedLength))
		//{
		//    fixedLength = false;
		//}

		return DslDbType.Binary;
	}

	private static Type[] PrepareTypeCoercions(DbCommandTree commandTree)
	{
		Diag.Trace();

		if (commandTree is DbQueryCommandTree queryTree)
		{
			if (queryTree.Query is DbProjectExpression projectExpression)
			{
				var resultsType = projectExpression.Projection.ResultType.EdmType;
				if (resultsType is StructuralType resultsAsStructuralType)
				{
					var members = resultsAsStructuralType.Members;
					return members.Select(ExtractExpectedTypeForCoercion).ToArray();
				}
			}
		}

		if (commandTree is DbFunctionCommandTree functionTree)
		{
			if (functionTree.ResultType != null)
			{
				Debug.Assert(MetadataHelpers.IsCollectionType(functionTree.ResultType.EdmType), "Result type of a function is expected to be a collection of RowType or PrimitiveType");

				var typeUsage = MetadataHelpers.GetElementTypeUsage(functionTree.ResultType);
				var elementType = typeUsage.EdmType;
				if (MetadataHelpers.IsRowType(elementType))
				{
					var members = ((RowType)elementType).Members;
					return members.Select(ExtractExpectedTypeForCoercion).ToArray();
				}
				else if (MetadataHelpers.IsPrimitiveType(elementType))
				{
					return new[] { MakeTypeCoercion(((PrimitiveType)elementType).ClrEquivalentType, typeUsage) };
				}
				else
				{
					Debug.Fail("Result type of a function is expected to be a collection of RowType or PrimitiveType");
				}
			}
		}

		return null;
	}

	private static Type ExtractExpectedTypeForCoercion(EdmMember member)
	{
		Diag.Trace();

		var type = ((PrimitiveType)member.TypeUsage.EdmType).ClrEquivalentType;
		return MakeTypeCoercion(type, member.TypeUsage);
	}

	private static Type MakeTypeCoercion(Type type, TypeUsage typeUsage)
	{
		Diag.Trace();

		if (type.IsValueType && MetadataHelpers.IsNullable(typeUsage))
			return typeof(Nullable<>).MakeGenericType(type);
		return type;
	}

	protected override void DbCreateDatabase(DbConnection connection, int? commandTimeout,
			StoreItemCollection storeItemCollection)
	{
		Diag.Trace();

		DslConnection.CreateDatabase(connection.ConnectionString, pageSize: 16384);
		var script = DbCreateDatabaseScript(GetDbProviderManifestToken(connection), storeItemCollection);
		var fbScript = new DslScript(script);
		fbScript.Parse();
		if (fbScript.Results.Any())
		{
			using (var fbConnection = new DslConnection(connection.ConnectionString))
			{
				var execution = new DslBatchExecution(fbConnection);
				execution.AppendSqlStatements(fbScript);
				execution.Execute();
			}
		}
	}

	protected override string DbCreateDatabaseScript(string providerManifestToken,
			StoreItemCollection storeItemCollection)
	{
		Diag.Trace();

		return SsdlToFb.Transform(storeItemCollection, providerManifestToken);
	}

	protected override bool DbDatabaseExists(DbConnection connection, int? commandTimeout,
			StoreItemCollection storeItemCollection)
	{
		Diag.Trace();

		if (connection.State == ConnectionState.Open
			   || connection.State == ConnectionState.Executing
			   || connection.State == ConnectionState.Fetching)
		{
			return true;
		}
		else
		{
			try
			{
				connection.Open();
				return true;
			}
			catch
			{
				return false;
			}
			finally
			{
				try
				{
					connection.Close();
				}
				catch { }
			}
		}
	}

	protected override void DbDeleteDatabase(DbConnection connection, int? commandTimeout,
			StoreItemCollection storeItemCollection)
	{
		Diag.Trace();

		DslConnection.DropDatabase(connection.ConnectionString);
	}
}
