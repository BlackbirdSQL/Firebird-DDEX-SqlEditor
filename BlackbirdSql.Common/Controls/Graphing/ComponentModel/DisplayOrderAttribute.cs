// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.DisplayOrderAttribute
using System;


namespace BlackbirdSql.Common.Controls.Graphing.ComponentModel;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class DisplayOrderAttribute : Attribute
{
	private readonly int _DisplayOrder;

	public int DisplayOrder => _DisplayOrder;

	public DisplayOrderAttribute(int displayOrder)
	{
		_DisplayOrder = displayOrder;
	}
}
