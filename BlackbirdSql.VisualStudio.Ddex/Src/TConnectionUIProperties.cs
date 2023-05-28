// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;

using BlackbirdSql.Common;

namespace BlackbirdSql.VisualStudio.Ddex;


// =========================================================================================================
//										TConnectionUIProperties Class
//
/// <summary>
/// Implementation of IVsDataConnectionProperties interface
/// </summary>
// =========================================================================================================
public class TConnectionUIProperties : AdoDotNetConnectionProperties
{

	// ---------------------------------------------------------------------------------
	#region Properties accessors
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// Checks the connection string and activates PromptDialog if incomplete
	/// </summary>
	public override bool IsComplete
	{
		get
		{
			// Diag.Trace("ProtectedMandatoryProperties required");
			// This has to be ProtectedMandatoryProperties for password PromptDialog to be activated
			try
			{
				foreach (string property in Schema.DslProperties.ProtectedMandatoryProperties)
				{
					try
					{
						if (!TryGetValue(property, out object value) || string.IsNullOrEmpty((string)value))
						{
							return false;
						}
					}
					catch (Exception ex)
					{
						Diag.Dug(ex, $"Property: {property}");
						throw;
					}

				}
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}

			return true;
		}
	}


	#endregion Properties accessors



	// ---------------------------------------------------------------------------------
	#region Constructors - TConnectionUIProperties
	// ---------------------------------------------------------------------------------


	public TConnectionUIProperties() : base()
	{
		// Diag.Trace();
	}


	#endregion Constructors

}
