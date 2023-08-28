// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)



using System;
using System.Windows.Forms;

using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Providers;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Common.Model;

namespace BlackbirdSql.Common.Ctl;


// =========================================================================================================
//										EventsManager Class
//
/// <summary>
/// Manages Solution, RDT and Selection events for the editor extension.
/// </summary>
// =========================================================================================================
public abstract class AbstractEditorEventsManager : AbstractEventsManager
{



	// =========================================================================================================
	#region Constructors / Destructors - EventsManager
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Private singleton .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	public AbstractEditorEventsManager(IBPackageController controller) : base(controller)
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Methods - EventsManager
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// AbstractEventsManager disposal
	/// </summary>
	/// <remarks>
	/// Example
	/// <code>_Controller.OnExampleEvent -= OnExample;</code>
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public override abstract void Dispose();


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Initializes the event manager
	/// validation.
	/// </summary>
	/// Example
	/// <code>_Controller.OnExampleEvent += OnExample;</code>
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public override abstract void Initialize();


	public static bool ShouldStopClose(AuxiliaryDocData add, Type type)
	{
		DialogResult dialogResult = DialogResult.Yes;
		bool flag = Cmd.IsInAutomationFunction();
		if (add.QueryExecutor.IsExecuting)
		{
			if (!flag)
			{
				dialogResult = Cmd.ShowMessageBoxEx("", SharedResx.ScriptIsStillBeingExecuted, "", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
			}

			if (dialogResult == DialogResult.No)
			{
				return true;
			}

			add.QueryExecutor.Cancel(bSync: true);
		}
		else if (add.QueryExecutor.ConnectionStrategy.IsTransactionOpen())
		{
			if (!flag)
			{
				dialogResult = Cmd.ShowMessageBoxEx("", SharedResx.UncommittedTransactionsWarning, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
			}

			switch (dialogResult)
			{
				case DialogResult.Cancel:
					return true;
				case DialogResult.Yes:
					try
					{
						add.QueryExecutor.ConnectionStrategy.CommitOpenTransactions();
					}
					catch (Exception ex)
					{
						Tracer.Trace(type, "HandleCloseEditorOrDesigner", ex.ToString());
						Cmd.ShowMessageBoxEx("", ex.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}

					break;
			}
		}

		return false;
	}


	#endregion Methods

}