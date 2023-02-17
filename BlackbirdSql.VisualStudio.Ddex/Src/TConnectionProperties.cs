//
// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)
//

using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using BlackbirdSql.Common;



namespace BlackbirdSql.VisualStudio.Ddex;


// =========================================================================================================
//										TConnectionProperties Class
//
/// <summary>
/// Implementation of <see cref="IVsDataConnectionProperties"/> interface
/// </summary>
// =========================================================================================================
internal class TConnectionProperties : AdoDotNetConnectionProperties
{

	// ---------------------------------------------------------------------------------
	#region Property Accessors - TConnectionProperties
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// Determines if the connection properties object is sufficiently complete (inclusive of password)
	/// to establish a database connection
	/// </summary>
	public override bool IsComplete
	{
		get
		{
			// Diag.Trace("ProtectedMandatoryProperties required");
			foreach (string property in Schema.DslConnectionString.ProtectedMandatoryProperties)
			{
				if (!TryGetValue(property, out object value) || string.IsNullOrEmpty((string)value))
				{
					return false;
				}
			}

			return true;
		}
	}


	#endregion Property Accessors





	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - TConnectionProperties
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// TConnectionProperties .ctor
	/// </summary>
	public TConnectionProperties() : base()
	{
		// Diag.Trace();
	}

	#endregion Constructors / Destructors


}
