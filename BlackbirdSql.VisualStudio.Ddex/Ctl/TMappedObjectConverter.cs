// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.Data;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;



namespace BlackbirdSql.VisualStudio.Ddex.Ctl;


// =========================================================================================================
//										TMappedObjectConverter Class
//
/// <summary>
/// Implementation of <see cref="IVsDataMappedObjectConverter"/> interface
/// </summary>
/// <remarks>
/// Implements mapping requests initiated by TObjectSupport.xml Property/Conversion/CallMapper
/// </remarks>
// =========================================================================================================
public class TMappedObjectConverter : AdoDotNetMappedObjectConverter
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - TMappedObjectConverter
	// ---------------------------------------------------------------------------------


	public TMappedObjectConverter() : base()
	{
		// Tracer.Trace(typeof(TMappedObjectConverter), ".ctor");
	}

	public TMappedObjectConverter(IVsDataConnection connection) : base(connection)
	{
		// Tracer.Trace(typeof(TMappedObjectConverter), ".ctor(IVsDataConnection)");
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Method Implementations - TMappedObjectConverter
	// =========================================================================================================


	protected override object ConvertToMappedMember(string typeName, string mappedMemberName, object[] underlyingValues, object[] parameters)
	{
		// Tracer.Trace(GetType(), "ConvertToMappedMember()", "typeName: {0}, mappedMemberName: {1}", typeName, mappedMemberName);
		object result = base.ConvertToMappedMember(typeName, mappedMemberName, underlyingValues, parameters);
		// Tracer.Trace(GetType(), "ConvertToMappedMember()", "result: {0}", result);
		return result;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Maps the NativeDataType property to the AdoDotNetDbType property for calls
	/// initiated by TObjectSupport.xml Property/Conversion/CallMapper
	/// </summary>
	/// <param name="nativeType"></param>
	/// <returns>The <see cref="DbType"/> of the AdoDotNetDbType</returns>
	// ---------------------------------------------------------------------------------
	protected override DbType GetDbTypeFromNativeType(string nativeType)
	{
		// Tracer.Trace(GetType(), "GetDbTypeFromNativeType()", "nativeType: {0}.", nativeType);

		DbType result;

		DataRow[] rows = DataTypes.Select(String.Format("TypeName = '{0}'", nativeType));

		if (rows != null && rows.Length > 0)
		{
			result = (DbType)Convert.ToInt32(rows[0]["DbType"]);
			// Tracer.Trace(GetType(), "GetDbTypeFromNativeType()", "Result DbType: {0}.", result);
			return result;
		}

		result = base.GetDbTypeFromNativeType(nativeType);
		// Tracer.Trace(GetType(), "GetDbTypeFromNativeType()", "Result DbType: {0}.", result);
		return result;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Maps the NativeDataType property to the AdoDotNetDataType property for calls
	/// initiated by TObjectSupport.xml Property/Conversion/CallMapper
	/// </summary>
	/// <param name="nativeType"></param>
	/// <returns>The integer value of the AdoDotNetDataType</returns>
	// ---------------------------------------------------------------------------------
	protected override int GetProviderTypeFromNativeType(string nativeType)
	{
		// Tracer.Trace(GetType(), "GetProviderTypeFromNativeType()", "nativeType: {0}.", nativeType);

		DataRow[] rows = DataTypes.Select(String.Format("TypeName = '{0}'", nativeType));

		int result;

		if (rows != null && rows.Length > 0)
		{
			result = Convert.ToInt32(rows[0]["ProviderDbType"]);
			// Tracer.Trace(GetType(), "GetProviderTypeFromNativeType()", "Result ProviderType: {0}.", result);
			return result;
		}

		result = base.GetProviderTypeFromNativeType(nativeType);
		// Tracer.Trace(GetType(), "GetProviderTypeFromNativeType()", "Result ProviderType: {0}.", result);
		return result;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Maps the NativeDataType property to the FrameworkDataType property for calls
	/// initiated by TObjectSupport.xml Property/Conversion/CallMapper
	/// </summary>
	/// <param name="nativeType"></param>
	/// <returns>The <see cref="Type"/> of the FrameworkDataType</returns>
	// ---------------------------------------------------------------------------------
	protected override Type GetFrameworkTypeFromNativeType(string nativeType)
	{
		// Tracer.Trace(GetType(), "GetFrameworkTypeFromNativeType()", "nativeType: {0}.", nativeType);

		DataRow[] rows = DataTypes.Select(String.Format("TypeName = '{0}'", nativeType));

		Type result;

		if (rows != null && rows.Length > 0)
		{
			result = Type.GetType(rows[0]["DataType"].ToString());
			// Tracer.Trace(GetType(), "GetFrameworkTypeFromNativeType()", "Result FrameworkType: {0}.", result);
			return result;
		}

		result = base.GetFrameworkTypeFromNativeType(nativeType);
		// Tracer.Trace(GetType(), "GetFrameworkTypeFromNativeType()", "Result FrameworkType: {0}.", result);
		return result;
	}


	#endregion Method Implementations

}
