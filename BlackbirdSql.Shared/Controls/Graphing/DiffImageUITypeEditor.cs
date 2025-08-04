// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.DiffImageUITypeEditor
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Security.Permissions;


namespace BlackbirdSql.Shared.Controls.Graphing;

[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
internal class DiffImageUITypeEditor : UITypeEditor, IDisposable
{
	private Bitmap diffImage;

	public DiffImageUITypeEditor()
	{
		diffImage = new Bitmap(typeof(DiffImageUITypeEditor), "doesNotEqualYellowBack.png");
		diffImage.MakeTransparent();
	}

	internal override bool GetPaintValueSupported(ITypeDescriptorContext context)
	{
		if (context.Instance is NodeDisplay nodeDisplay && nodeDisplay.DiffMap.ContainsKey(context.PropertyDescriptor))
		{
			return nodeDisplay.DiffMap[context.PropertyDescriptor];
		}
		return false;
	}

	internal override void PaintValue(PaintValueEventArgs e)
	{
		if (diffImage != null)
		{
			Rectangle bounds = e.Bounds;
			e.Graphics.DrawImage(diffImage, bounds);
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing && diffImage != null)
		{
			diffImage.Dispose();
			diffImage = null;
		}
	}
}
