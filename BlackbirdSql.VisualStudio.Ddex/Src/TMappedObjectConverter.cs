//
// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)
//

using System;
using System.Data;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using BlackbirdSql.Common;



namespace BlackbirdSql.VisualStudio.Ddex;


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
		Diag.Trace();
	}

	public TMappedObjectConverter(IVsDataConnection connection) : base(connection)
	{
		Diag.Trace();
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Method Implementations - TMappedObjectConverter
	// =========================================================================================================


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
		Diag.Trace();
		DataRow[] rows = this.DataTypes.Select(String.Format("TypeName = '{0}'", nativeType));

		if (rows != null && rows.Length > 0)
		{
			return (DbType)Convert.ToInt32(rows[0]["DbType"]);
		}

		return base.GetDbTypeFromNativeType(nativeType);
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
		Diag.Trace();
		DataRow[] rows = this.DataTypes.Select(String.Format("TypeName = '{0}'", nativeType));

		if (rows != null && rows.Length > 0)
		{
			return Convert.ToInt32(rows[0]["ProviderDbType"]);
		}

		return base.GetProviderTypeFromNativeType(nativeType);
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
		Diag.Trace();
		DataRow[] rows = this.DataTypes.Select(String.Format("TypeName = '{0}'", nativeType));

		if (rows != null && rows.Length > 0)
		{
			return Type.GetType(rows[0]["DataType"].ToString());
		}

		return base.GetFrameworkTypeFromNativeType(nativeType);
	}


	#endregion Method Implementations

}
