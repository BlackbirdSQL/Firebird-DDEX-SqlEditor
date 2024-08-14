using BlackbirdSql.Shared.Enums;



namespace BlackbirdSql;


// =========================================================================================================
//											SharedConstants Class
//
/// <summary>
/// Shared db constants class.
/// </summary>
// =========================================================================================================
public static class SharedConstants
{
	// ---------------------------------------------------------------------------------
	#region DbConnectionString Property Names - SharedConstants
	// ---------------------------------------------------------------------------------


	// External (non-paramameter) property descriptor
	public const string C_KeyExCreationFlags = "CreationFlags";


	#endregion DbConnectionString Property Names




	// ---------------------------------------------------------------------------------
	#region DbConnectionString Property Default Values - SharedConstants
	// ---------------------------------------------------------------------------------


	// External (non-paramameter) property defaults 
	public const EnCreationFlags C_DefaultExCreationFlags = EnCreationFlags.None;


	#endregion DbConnectionString Property Default Values

}
