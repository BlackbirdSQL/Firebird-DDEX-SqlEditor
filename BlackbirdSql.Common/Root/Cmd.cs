
using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Controls;
using Microsoft.VisualStudio.Shell.Interop;



namespace BlackbirdSql.Common;


// =========================================================================================================
//												Cmd Class
//
/// <summary>
/// Central location for implementation of utility static methods for Controls. 
/// </summary>
// =========================================================================================================

public abstract class Cmd : BlackbirdSql.Core.Cmd
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




	public static System.Windows.Media.Color CombineColors(System.Windows.Media.Color c1, int a1, System.Windows.Media.Color c2, int a2)
	{
		return System.Windows.Media.Color.FromArgb((byte)((c1.A * a1 + c2.A * a2) / 100), (byte)((c1.R * a1 + c2.R * a2) / 100), (byte)((c1.G * a1 + c2.G * a2) / 100), (byte)((c1.B * a1 + c2.B * a2) / 100));
	}



	public static System.Drawing.Color ConvertColor(System.Windows.Media.Color wpfColor)
	{
		return System.Drawing.Color.FromArgb(255, wpfColor.R, wpfColor.G, wpfColor.B);
	}


	public static UIElement FindVisualParent(Type typeOfParent, UIElement element, Type stopOnType = null)
	{
		for (UIElement uIElement = element; uIElement != null; uIElement = VisualTreeHelper.GetParent(uIElement) as UIElement)
		{
			if (typeOfParent.IsAssignableFrom(uIElement.GetType()))
			{
				return uIElement;
			}
			if (stopOnType != null && stopOnType.IsAssignableFrom(uIElement.GetType()))
			{
				return null;
			}
		}
		return null;
	}

	public static T FindVisualParent<T>(UIElement element, Type stopOnType = null) where T : UIElement
	{
		for (UIElement uIElement = element; uIElement != null; uIElement = VisualTreeHelper.GetParent(uIElement) as UIElement)
		{
			if (uIElement is T result)
			{
				return result;
			}
			if (stopOnType != null && stopOnType.IsAssignableFrom(uIElement.GetType()))
			{
				return null;
			}
		}
		return null;
	}




	// IsInAutomationFunction
	public static bool IsInAutomationFunction()
	{
		Diag.ThrowIfNotOnUIThread();

		IVsExtensibility3 extensibility = VS.GetExtensibility(); 

		if (extensibility == null)
			return true;

		___(extensibility.IsInAutomationFunction(out int pfInAutoFunc));

		return pfInAutoFunc != 0;
	}



	public static bool ShouldStopCloseDialog(AuxiliaryDocData add, Type type)
	{
		DialogResult dialogResult = DialogResult.Yes;
		bool flag = Cmd.IsInAutomationFunction();
		if (add.QryMgr.IsExecuting)
		{
			if (!flag)
				dialogResult = MessageCtl.ShowEx(ControlsResources.ScriptIsStillBeingExecuted, "", "", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

			if (dialogResult == DialogResult.No)
				return true;

			add.QryMgr.Cancel(bSync: true);
		}
		else if (add.QryMgr.ConnectionStrategy.HasTransactions)
		{
			if (!flag)
			{
				dialogResult = MessageCtl.ShowEx(ControlsResources.UncommittedTransactionsWarning, "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
			}

			switch (dialogResult)
			{
				case DialogResult.Cancel:
					return true;
				case DialogResult.Yes:
					try
					{
						add.CommitTransactions();
					}
					catch (Exception ex)
					{
						MessageCtl.ShowEx("", ex.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}

					break;
			}
		}

		return false;
	}


	#endregion Static Methods
}
