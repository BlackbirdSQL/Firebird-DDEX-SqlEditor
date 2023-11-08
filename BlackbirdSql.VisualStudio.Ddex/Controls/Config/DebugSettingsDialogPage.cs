// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.VisualStudio.Shell;


namespace BlackbirdSql.VisualStudio.Ddex.Controls.Config
{

	// =========================================================================================================
	//										UIDebugOptionsDialogPage Class
	//
	/// <summary>
	/// WPF <see cref="UIElementDialogPage"/> for Debug options.
	/// This is depracated for now because we have implemented check boxes, and every
	/// other control we are using atm, in the standard DialogPage PropertyGrid.
	/// </summary>
	// =========================================================================================================

	[ComVisible(true)]
	[Guid(PackageData.DebugSettingsGuid)]

	internal class DebugSettingsDialogPage : UIElementDialogPage
	{

		/// <summary>
		/// Gets the WPF child element to be hosted inside the dialog page.
		/// </summary>
		protected override UIElement Child
		{
			get
			{
				DebugSettingsControl control = new DebugSettingsControl
				{
					SettingsDialogPage = this
				};
				control.Initialize();

				return control;
			}
		}
	}
}
