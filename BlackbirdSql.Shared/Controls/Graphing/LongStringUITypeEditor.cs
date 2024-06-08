// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.LongStringUITypeEditor
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Security.Permissions;


namespace BlackbirdSql.Shared.Controls.Graphing;

[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
public class LongStringUITypeEditor : DiffImageUITypeEditor
{
	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return UITypeEditorEditStyle.Modal;
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		using PropertyViewDlg propertyViewForm = new PropertyViewDlg();
		propertyViewForm.Text = context.PropertyDescriptor.DisplayName;
		propertyViewForm.DisplayText = value.ToString();
		propertyViewForm.ShowDialog();
		return value;
	}
}
