#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.ComponentModel;
using BlackbirdSql.Common.Properties;


// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.PropertyGridUtilities
namespace BlackbirdSql.Common.Controls.PropertiesWindow
{
	public sealed class GlobalizedCategoryAttribute : CategoryAttribute
	{
		private readonly string resourceName;

		public GlobalizedCategoryAttribute(string resourceName)
		{
			this.resourceName = resourceName;
		}

		protected override string GetLocalizedString(string value)
		{
			try
			{
				return ControlsResources.ResourceManager.GetString(resourceName);
			}
			catch (Exception)
			{
				return "ControlsResources: " + value;
			}
		}
	}
}
