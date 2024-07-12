
using System;
using System.Windows.Forms;
using BlackbirdSql.Shared.Model;
using BlackbirdSql.Shared.Properties;



namespace BlackbirdSql.Shared;


// =========================================================================================================
//												Cmd Class
//
/// <summary>
/// Central location for implementation of utility static methods for Controls. 
/// </summary>
// =========================================================================================================

public abstract class Cmd : BlackbirdSql.Cmd
{


	// ---------------------------------------------------------------------------------
	#region Constants - Cmd
	// ---------------------------------------------------------------------------------


	public const char OpenSquareBracket = '[';
	public const char DoubleQuote = '"';


	#endregion Constants





	// =========================================================================================================
	#region Fields - Cmd
	// =========================================================================================================


	#endregion Fields





	// =========================================================================================================
	#region Static Methods - Cmd
	// =========================================================================================================



	public static bool ShouldStopCloseDialog(AuxilliaryDocData auxDocData, Type type)
	{
		DialogResult dialogResult = DialogResult.Yes;
		bool inAutomation = UnsafeCmd.IsInAutomationFunction();

		if (auxDocData.QryMgr.IsExecuting)
		{
			if (!inAutomation)
				dialogResult = MessageCtl.ShowEx(ControlsResources.ScriptIsStillBeingExecuted, "", "", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

			if (dialogResult == DialogResult.No)
				return true;

			auxDocData.QryMgr.Cancel(synchronous: true);
		}
		else if (auxDocData.QryMgr.GetUpdateTransactionsStatus())
		{
			if (!inAutomation)
				dialogResult = MessageCtl.ShowEx(ControlsResources.UncommittedTransactionsWarning, "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);

			switch (dialogResult)
			{
				case DialogResult.Cancel:
					return true;
				case DialogResult.Yes:
					try
					{
						auxDocData.CommitTransactions();
					}
					catch (Exception ex)
					{
						MessageCtl.ShowEx(ex, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}

					break;
			}
		}

		return false;
	}


	#endregion Static Methods
}
