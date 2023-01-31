/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/BlackbirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net)

using System;
using BlackbirdSql.EntityFrameworkCore;
using BlackbirdSql.EntityFrameworkCore.Diagnostics.Internal;
using BlackbirdSql.EntityFrameworkCore.Infrastructure;
using BlackbirdSql.EntityFrameworkCore.Infrastructure.Internal;
using BlackbirdSql.EntityFrameworkCore.Internal;
using BlackbirdSql.EntityFrameworkCore.Metadata.Conventions;
using BlackbirdSql.EntityFrameworkCore.Metadata.Internal;
using BlackbirdSql.EntityFrameworkCore.Migrations;
using BlackbirdSql.EntityFrameworkCore.Migrations.Internal;
using BlackbirdSql.EntityFrameworkCore.Query.ExpressionTranslators.Internal;
using BlackbirdSql.EntityFrameworkCore.Query.Internal;
using BlackbirdSql.EntityFrameworkCore.Storage.Internal;
using BlackbirdSql.EntityFrameworkCore.Update.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore;

public static class FbServiceCollectionExtensions
{
	public static IServiceCollection AddBlackbird<TContext>(this IServiceCollection serviceCollection, string connectionString, Action<FbDbContextOptionsBuilder> fbOptionsAction = null, Action<DbContextOptionsBuilder> optionsAction = null)
		where TContext : DbContext
	{
		return serviceCollection.AddDbContext<TContext>(
			(serviceProvider, options) =>
			{
				optionsAction?.Invoke(options);
				options.UseBlackbird(connectionString, fbOptionsAction);
			});
	}

	public static IServiceCollection AddEntityFrameworkBlackbird(this IServiceCollection serviceCollection)
	{
		var builder = new EntityFrameworkRelationalServicesBuilder(serviceCollection)
			.TryAdd<LoggingDefinitions, FbLoggingDefinitions>()
			.TryAdd<IDatabaseProvider, DatabaseProvider<FbOptionsExtension>>()
			.TryAdd<IRelationalDatabaseCreator, FbDatabaseCreator>()
			.TryAdd<IRelationalTypeMappingSource, FbTypeMappingSource>()
			.TryAdd<ISqlGenerationHelper, FbSqlGenerationHelper>()
			.TryAdd<IRelationalAnnotationProvider, FbRelationalAnnotationProvider>()
			.TryAdd<IProviderConventionSetBuilder, FbConventionSetBuilder>()
			.TryAdd<IUpdateSqlGenerator>(p => p.GetService<IFbUpdateSqlGenerator>())
			.TryAdd<IModificationCommandBatchFactory, FbModificationCommandBatchFactory>()
			.TryAdd<IRelationalConnection>(p => p.GetService<IFbRelationalConnection>())
			.TryAdd<IRelationalTransactionFactory, FbTransactionFactory>()
			.TryAdd<IMigrationsSqlGenerator, FbMigrationsSqlGenerator>()
			.TryAdd<IHistoryRepository, FbHistoryRepository>()
			.TryAdd<IMemberTranslatorProvider, FbMemberTranslatorProvider>()
			.TryAdd<IMethodCallTranslatorProvider, FbMethodCallTranslatorProvider>()
			.TryAdd<IQuerySqlGeneratorFactory, FbQuerySqlGeneratorFactory>()
			.TryAdd<ISqlExpressionFactory, FbSqlExpressionFactory>()
			.TryAdd<ISingletonOptions, IFbOptions>(p => p.GetService<IFbOptions>())
			.TryAdd<IRelationalSqlTranslatingExpressionVisitorFactory, FbSqlTranslatingExpressionVisitorFactory>()
			.TryAddProviderSpecificServices(b => b
				.TryAddSingleton<IFbOptions, FbOptions>()
				.TryAddSingleton<IFbMigrationSqlGeneratorBehavior, FbMigrationSqlGeneratorBehavior>()
				.TryAddSingleton<IFbUpdateSqlGenerator, FbUpdateSqlGenerator>()
				.TryAddScoped<IFbRelationalConnection, FbRelationalConnection>()
				.TryAddScoped<IFbRelationalTransaction, FbRelationalTransaction>());

		builder.TryAddCoreServices();

		return serviceCollection;
	}
}
