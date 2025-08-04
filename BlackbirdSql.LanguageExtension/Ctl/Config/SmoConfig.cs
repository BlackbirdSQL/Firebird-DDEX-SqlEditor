// Microsoft.SqlServer.Management.SmoMetadataProvider, Version=17.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SmoMetadataProvider.Config

using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Management.Smo;
// using Microsoft.SqlServer.Management.SmoMetadataProvider;



namespace BlackbirdSql.LanguageExtension.Ctl.Config;


internal static class SmoConfig
{
	internal sealed class SmoInitFields
	{
		private static readonly Dictionary<Type, SmoInitFields> smoInitFields;

		private static readonly SmoInitFields database;

		public readonly Type Type;

		public readonly string[] Optimized;

		public readonly string[] Safe;

		public static SmoInitFields Database => database;

		private SmoInitFields(Type type, string[] optimized, string[] safe)
		{
			Type = type;
			Optimized = optimized;
			Safe = safe;
		}

		static SmoInitFields()
		{
			smoInitFields = [];
			database = new SmoInitFields(typeof(Microsoft.SqlServer.Management.Smo.Database), ["Name", "ID", "IsSystemObject", "Collation", "IsAccessible"], ["Name", "ID", "IsAccessible"]);
			AddSmoInitFields(database);
			AddSmoInitFields(new SmoInitFields(typeof(Microsoft.SqlServer.Management.Smo.AsymmetricKey), ["Name", "ID"], ["Name", "ID"]));
			AddSmoInitFields(new SmoInitFields(typeof(Microsoft.SqlServer.Management.Smo.Column), ["Name", "ID", "Nullable", "InPrimaryKey", "Identity", "DataType", "Computed"], ["Name", "ID", "Nullable", "InPrimaryKey", "Identity", "Computed"]));
			AddSmoInitFields(new SmoInitFields(typeof(Microsoft.SqlServer.Management.Smo.Certificate), ["Name", "ID"], ["Name", "ID"]));
			AddSmoInitFields(new SmoInitFields(typeof(Microsoft.SqlServer.Management.Smo.Credential), ["Name", "ID"], ["Name", "ID"]));
			AddSmoInitFields(new SmoInitFields(typeof(Check), ["Name"], ["Name"]));
			AddSmoInitFields(new SmoInitFields(typeof(Microsoft.SqlServer.Management.Smo.DatabaseDdlTrigger), ["Name", "ID", "IsSystemObject", "ImplementationType", "IsEncrypted"], ["Name", "ID"]));
			AddSmoInitFields(new SmoInitFields(typeof(Microsoft.SqlServer.Management.Smo.ExtendedStoredProcedure), ["Name", "ID", "Schema", "IsSystemObject"], ["Name", "ID"]));
			AddSmoInitFields(new SmoInitFields(typeof(ForeignKey), ["Name"], ["Name"]));
			AddSmoInitFields(new SmoInitFields(typeof(Microsoft.SqlServer.Management.Smo.Index), ["Name", "IndexKeyType", "IsSpatialIndex", "IsXmlIndex"], ["Name"]));
			AddSmoInitFields(new SmoInitFields(typeof(Microsoft.SqlServer.Management.Smo.Login), ["Name", "ID", "AsymmetricKey", "Certificate", "Credential", "LoginType"], ["Name", "ID"]));
			AddSmoInitFields(new SmoInitFields(typeof(Microsoft.SqlServer.Management.Smo.ServerDdlTrigger), ["Name", "ID", "IsSystemObject", "ImplementationType", "IsEncrypted"], ["Name", "ID"]));
			AddSmoInitFields(new SmoInitFields(typeof(Microsoft.SqlServer.Management.Smo.StoredProcedure), ["Name", "ID", "Schema", "IsSystemObject", "ImplementationType", "IsEncrypted"], ["Name", "ID", "Schema"]));
			AddSmoInitFields(new SmoInitFields(typeof(Microsoft.SqlServer.Management.Smo.Synonym), ["Name", "ID", "Schema", "BaseDatabase", "BaseObject", "BaseSchema", "BaseServer", "BaseType"], ["Name", "ID", "Schema"]));
			AddSmoInitFields(new SmoInitFields(typeof(StoredProcedureParameter), ["Name", "ID", "DefaultValue", "IsOutputParameter", "IsReadOnly", "DataType"], ["Name", "ID"]));
			AddSmoInitFields(new SmoInitFields(typeof(Trigger), ["Name", "ID", "ImplementationType", "IsSystemObject", "IsEncrypted", "Insert", "Delete", "Update"], ["Name", "ID"]));
			AddSmoInitFields(new SmoInitFields(typeof(Microsoft.SqlServer.Management.Smo.User), ["Name", "ID", "AsymmetricKey", "Certificate", "UserType"], ["Name", "ID"]));
			AddSmoInitFields(new SmoInitFields(typeof(Microsoft.SqlServer.Management.Smo.UserDefinedAggregate), ["Name", "ID", "Schema", "DataType"], ["Name", "ID", "Schema"]));
			AddSmoInitFields(new SmoInitFields(typeof(Microsoft.SqlServer.Management.Smo.UserDefinedFunction), ["Name", "ID", "Schema", "FunctionType", "ImplementationType", "IsSystemObject", "IsEncrypted", "IsSchemaBound", "DataType"], ["Name", "ID", "Schema"]));
			AddSmoInitFields(new SmoInitFields(typeof(UserDefinedFunctionParameter), ["Name", "ID", "DataType"], ["Name", "ID"]));
		}

		public static IEnumerable<SmoInitFields> GetAllInitFields()
		{
			return smoInitFields.Values;
		}

		public static SmoInitFields GetInitFields(Type type)
		{
			Diag.ThrowIfInstanceNull(type, typeof(Type));
			// TraceHelper.TraceContext.Assert(type != null, "SmoMetadataProvider Assert", "type != null");
			smoInitFields.TryGetValue(type, out var value);
			return value;
		}

		private static void AddSmoInitFields(SmoInitFields initFields)
		{
			Diag.ThrowIfInstanceNull(initFields, typeof(SmoInitFields));
			// TraceHelper.TraceContext.Assert(initFields != null, "SmoMetadataProvider Assert", "initFields != null");
			if (smoInitFields.ContainsKey(initFields.Type))
				Diag.ThrowException(new ArgumentException($"smoInitiFields already contains {initFields.Type}."));
			// TraceHelper.TraceContext.Assert(!smoInitFields.ContainsKey(initFields.Type), "SmoMetadataProvider Assert", "!smoInitFields.ContainsKey(initFields.Type)");
			smoInitFields.Add(initFields.Type, initFields);
		}
	}
}
