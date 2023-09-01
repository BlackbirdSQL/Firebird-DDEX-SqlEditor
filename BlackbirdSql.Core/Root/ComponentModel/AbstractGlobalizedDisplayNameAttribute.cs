#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.ComponentModel;



// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.PropertyGridUtilities
namespace BlackbirdSql.Core.ComponentModel;

public abstract class AbstractGlobalizedDisplayNameAttribute : DisplayNameAttribute
{
	private readonly string _ResourceName;

	public abstract System.Resources.ResourceManager ResMgr { get; }

	public override string DisplayName
	{
		get
		{
			try
			{
				return ResMgr.GetString(_ResourceName);
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}
	}

	public AbstractGlobalizedDisplayNameAttribute(string resourceName)
	{
		_ResourceName = resourceName;
	}
}
