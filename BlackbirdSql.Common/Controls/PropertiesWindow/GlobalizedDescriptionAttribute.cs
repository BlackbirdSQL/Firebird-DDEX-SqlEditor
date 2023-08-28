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
	public sealed class GlobalizedDescriptionAttribute : DescriptionAttribute
	{
		private readonly string resourceName;

		public override string Description
		{
			get
			{
				try
				{
					return ControlsResources.ResourceManager.GetString(resourceName);
				}
				catch (Exception)
				{
					return string.Empty;
				}
			}
		}

		public GlobalizedDescriptionAttribute(string resourceName)
		{
			this.resourceName = resourceName;
		}
	}
}
