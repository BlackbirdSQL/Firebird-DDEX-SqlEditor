// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.Data;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;



namespace BlackbirdSql.VisualStudio.Ddex.Ctl;


// =========================================================================================================
//										VxbMappedObjectConverter Class
//
/// <summary>
/// Implementation of <see cref="IVsDataMappedObjectConverter"/> interface
/// </summary>
/// <remarks>
/// Implements mapping requests initiated by VxbObjectSupport.xml Property/Conversion/CallMapper
/// </remarks>
// =========================================================================================================
public class VxbMappedObjectConverter : AdoDotNetMappedObjectConverter
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - VxbMappedObjectConverter
	// ---------------------------------------------------------------------------------


	public VxbMappedObjectConverter() : base()
	{
		// Evs.Trace(typeof(VxbMappedObjectConverter), ".ctor");
	}

	public VxbMappedObjectConverter(IVsDataConnection connection) : base(connection)
	{
		// Evs.Trace(typeof(VxbMappedObjectConverter), ".ctor(IVsDataConnection)");
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Method Implementations - VxbMappedObjectConverter
	// =========================================================================================================


	protected override object ConvertToMappedMember(string typeName, string mappedMemberName, object[] underlyingValues, object[] parameters)
	{
		Evs.Trace(GetType(), nameof(ConvertToMappedMember));

		object result = base.ConvertToMappedMember(typeName, mappedMemberName, underlyingValues, parameters);
		// Evs.Trace(GetType(), nameof(ConvertToMappedMember), "result: {0}", result);
		return result;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Maps the NativeDataType property to the AdoDotNetDbType property for calls
	/// initiated by VxbObjectSupport.xml Property/Conversion/CallMapper
	/// </summary>
	/// <param name="nativeType"></param>
	/// <returns>The <see cref="DbType"/> of the AdoDotNetDbType</returns>
	// ---------------------------------------------------------------------------------
	protected override DbType GetDbTypeFromNativeType(string nativeType)
	{
		Evs.Trace(GetType(), nameof(GetDbTypeFromNativeType));

		DbType result;

		DataRow[] rows = DataTypes.Select(String.Format("TypeName = '{0}'", nativeType));

		if (rows != null && rows.Length > 0)
		{
			result = (DbType)Convert.ToInt32(rows[0]["DbType"]);
			// Evs.Trace(GetType(), nameof(GetDbTypeFromNativeType), "Result DbType: {0}.", result);
			return result;
		}

		result = base.GetDbTypeFromNativeType(nativeType);
		// Evs.Trace(GetType(), nameof(GetDbTypeFromNativeType), "Result DbType: {0}.", result);
		return result;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Maps the NativeDataType property to the AdoDotNetDataType property for calls
	/// initiated by VxbObjectSupport.xml Property/Conversion/CallMapper
	/// </summary>
	/// <param name="nativeType"></param>
	/// <returns>The integer value of the AdoDotNetDataType</returns>
	// ---------------------------------------------------------------------------------
	protected override int GetProviderTypeFromNativeType(string nativeType)
	{
		Evs.Trace(GetType(), nameof(GetProviderTypeFromNativeType));

		DataRow[] rows = DataTypes.Select(String.Format("TypeName = '{0}'", nativeType));

		int result;

		if (rows != null && rows.Length > 0)
		{
			result = Convert.ToInt32(rows[0]["ProviderDbType"]);
			// Evs.Trace(GetType(), nameof(GetProviderTypeFromNativeType), "Result ProviderType: {0}.", result);
			return result;
		}

		result = base.GetProviderTypeFromNativeType(nativeType);
		// Evs.Trace(GetType(), nameof(GetProviderTypeFromNativeType), "Result ProviderType: {0}.", result);
		return result;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Maps the NativeDataType property to the FrameworkDataType property for calls
	/// initiated by VxbObjectSupport.xml Property/Conversion/CallMapper
	/// </summary>
	/// <param name="nativeType"></param>
	/// <returns>The <see cref="Type"/> of the FrameworkDataType</returns>
	// ---------------------------------------------------------------------------------
	protected override Type GetFrameworkTypeFromNativeType(string nativeType)
	{
		Evs.Trace(GetType(), nameof(GetFrameworkTypeFromNativeType));

		DataRow[] rows = DataTypes.Select(String.Format("TypeName = '{0}'", nativeType));

		Type result;

		if (rows != null && rows.Length > 0)
		{
			result = Type.GetType(rows[0]["DataType"].ToString());
			// Evs.Trace(GetType(), nameof(GetFrameworkTypeFromNativeType), "Result FrameworkType: {0}.", result);
			return result;
		}

		result = base.GetFrameworkTypeFromNativeType(nativeType);
		// Evs.Trace(GetType(), nameof(GetFrameworkTypeFromNativeType), "Result FrameworkType: {0}.", result);
		return result;
	}


	#endregion Method Implementations

}
