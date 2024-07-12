#region Assembly FirebirdSql.Data.FirebirdClient, Version=10.0.0.0, Culture=neutral, PublicKeyToken=3750abcc3150b00c
// C:\Users\GregChristos\.nuget\packages\firebirdsql.data.firebirdclient\10.0.0\lib\net48\FirebirdSql.Data.FirebirdClient.dll
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464
#endregion

using System;



namespace BlackbirdSql.Sys.Enums;


[Serializable]
public enum EnSqlStatementType
{
	AlterCharacterSet,
	AlterDatabase,
	AlterDomain,
	AlterException,
	AlterExternalFunction,
	AlterFunction,
	AlterIndex,
	AlterPackage,
	AlterProcedure,
	AlterRole,
	AlterSequence,
	AlterTable,
	AlterTrigger,
	AlterView,
	Close,
	CommentOn,
	Commit,
	Connect,
	CreateCollation,
	CreateDatabase,
	CreateDomain,
	CreateException,
	CreateFunction,
	CreateGenerator,
	CreateIndex,
	CreatePackage,
	CreatePackageBody,
	CreateProcedure,
	CreateRole,
	CreateSequence,
	CreateShadow,
	CreateTable,
	CreateTrigger,
	CreateView,
	DeclareCursor,
	DeclareExternalFunction,
	DeclareFilter,
	DeclareStatement,
	DeclareTable,
	Delete,
	Describe,
	Disconnect,
	DropCollation,
	DropDatabase,
	DropDomain,
	DropException,
	DropExternalFunction,
	DropFunction,
	DropFilter,
	DropGenerator,
	DropIndex,
	DropPackage,
	DropPackageBody,
	DropProcedure,
	DropSequence,
	DropRole,
	DropShadow,
	DropTable,
	DropTrigger,
	DropView,
	EndDeclareSection,
	EventInit,
	EventWait,
	Execute,
	ExecuteBlock,
	ExecuteImmediate,
	ExecuteProcedure,
	Fetch,
	Grant,
	Insert,
	InsertCursor,
	Merge,
	Open,
	Prepare,
	RecreateFunction,
	RecreatePackage,
	RecreatePackageBody,
	RecreateProcedure,
	RecreateTable,
	RecreateTrigger,
	RecreateView,
	Revoke,
	Rollback,
	Select,
	SetAutoDDL,
	SetDatabase,
	SetGenerator,
	SetNames,
	SetSQLDialect,
	SetStatistics,
	SetTransaction,
	ShowSQLDialect,
	Update,
	Whenever
}
#if false // Decompilation log
'145' items in cache
------------------
Resolve: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\mscorlib.dll'
------------------
Resolve: 'System.Numerics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System.Numerics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.Numerics.dll'
------------------
Resolve: 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.dll'
------------------
Resolve: 'System.Threading.Tasks.Extensions, Version=4.2.0.1, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Found single assembly: 'System.Threading.Tasks.Extensions, Version=4.2.0.1, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Load from: 'C:\Users\GregChristos\.nuget\packages\system.threading.tasks.extensions\4.5.4\lib\net461\System.Threading.Tasks.Extensions.dll'
------------------
Resolve: 'System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.Data.dll'
------------------
Resolve: 'System.Transactions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System.Transactions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.Transactions.dll'
------------------
Resolve: 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.Core.dll'
#endif
