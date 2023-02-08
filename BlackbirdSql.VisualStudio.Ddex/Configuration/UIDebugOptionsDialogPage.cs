//
// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)
//

using System;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.VisualStudio.Shell;



namespace BlackbirdSql.VisualStudio.Ddex.Configuration
{


	// =========================================================================================================
	//										UIDebugOptionsDialogPage Class
	//
	/// <summary>
	/// WPF <see cref="UIElementDialogPage"/> for Debug options
	/// </summary>
	// =========================================================================================================

	[ComVisible(true)]
	[Guid(PackageData.WpfDebugOptionsGiud)]

	internal class UIDebugOptionsDialogPage : UIElementDialogPage
	{

		/// <summary>
		/// Gets the WPF child element to be hosted inside the dialog page.
		/// </summary>
		protected override UIElement Child
		{
			get
			{
				UIDebugOptionsControl control = new UIDebugOptionsControl
				{
					DebugOptionDialogPage = this
				};
				control.Initialize();

				return control;
			}
		}
	}
}
