using System;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.VisualStudio.Shell;



namespace BlackbirdSql.VisualStudio.Ddex.Configuration
{
	[ComVisible(true)]
	[Guid("7E2D7C87-1FAD-42D1-AE67-4EEA3281E52C")]


	internal class UIDebugOptionDialogPage : UIElementDialogPage
	{
		protected override UIElement Child
		{
			get
			{
				UIDebugOptionControl control = new UIDebugOptionControl
				{
					DebugOptionDialogPage = this
				};
				control.Initialize();

				return control;
			}
		}
	}
}
