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
using BlackbirdSql.Data.Entity.Core;
using BlackbirdSql.Data.Entity.Core.Diagnostics.Internal;
using BlackbirdSql.Data.Entity.Core.Infrastructure;
using BlackbirdSql.Data.Entity.Core.Infrastructure.Internal;
using BlackbirdSql.Data.Entity.Core.Internal;
using BlackbirdSql.Data.Entity.Core.Metadata.Conventions;
using BlackbirdSql.Data.Entity.Core.Metadata.Internal;
using BlackbirdSql.Data.Entity.Core.Migrations;
using BlackbirdSql.Data.Entity.Core.Migrations.Internal;
using BlackbirdSql.Data.Entity.Core.Query.ExpressionTranslators.Internal;
using BlackbirdSql.Data.Entity.Core.Query.Internal;
using BlackbirdSql.Data.Entity.Core.Storage.Internal;
using BlackbirdSql.Data.Entity.Core.Update.Internal;
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
