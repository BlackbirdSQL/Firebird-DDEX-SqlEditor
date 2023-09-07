// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.EdgeDisplay
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using BlackbirdSql.Common.Controls.Graphing.Interfaces;
using BlackbirdSql.Common.Properties;
using Microsoft.AnalysisServices.Graphing;


namespace BlackbirdSql.Common.Controls.Graphing;

internal class EdgeDisplay : Microsoft.AnalysisServices.Graphing.EdgeDisplay, IRenderable, ICustomTypeDescriptor
{
	private const int C_MinWidth = 1;

	public const int C_MaxWidth = 12;

	private Size connectionOffset;

	protected int width;

	private int minimalMidpointOffsetFromTheEnd;

	private const int C_ExtendedWidthExtra = 5;

	public new BlackbirdSql.Common.Controls.Graphing.Edge EdgeOriginal => (BlackbirdSql.Common.Controls.Graphing.Edge)base.EdgeOriginal;

	public virtual int Width
	{
		get
		{
			if (width == 0)
			{
				double rowCount = EdgeOriginal.RowCount;
				if (rowCount >= 1.0)
				{
					width = (int)Math.Max(MinWidth, Math.Min(1.0 + 1.5 * Math.Log10(rowCount), 12.0));
				}
				else
				{
					width = MinWidth;
				}
			}
			return width;
		}
	}

	protected virtual int MinWidth => C_MinWidth;

	public int ExtendedWidth => Width + C_ExtendedWidthExtra;

	public new BlackbirdSql.Common.Controls.Graphing.NodeDisplay NodeFrom => base.NodeFrom as BlackbirdSql.Common.Controls.Graphing.NodeDisplay;

	public new BlackbirdSql.Common.Controls.Graphing.NodeDisplay NodeTo => base.NodeTo as BlackbirdSql.Common.Controls.Graphing.NodeDisplay;

	public override string Name
	{
		get
		{
			return string.Format(ControlsResources.EdgeDisplayPropertiesLabel, EdgeOriginal.EstimatedRowCount, EdgeOriginal.RowSize, NodeFrom.ID, NodeTo.ID);
		}
		set
		{
		}
	}

	internal Size ConnectionOffset
	{
		get
		{
			return connectionOffset;
		}
		set
		{
			connectionOffset = value;
		}
	}

	internal int MinimalMidpointOffsetFromTheEnd
	{
		get
		{
			return minimalMidpointOffsetFromTheEnd;
		}
		set
		{
			minimalMidpointOffsetFromTheEnd = value;
		}
	}

	public float ArrowWidth => (float)(Width - C_MinWidth) / 2f + 2f;

	public EdgeDisplay(GraphCtrl graphctrl, IGraphModify igraphmodify, IEdge iedgeOriginal, IEdgeDisplayProperties iedgedisplaypropertiesDefault, IList ilistDisplayable, INode inodeFrom, INode inodeTo)
		: base(graphctrl, igraphmodify, iedgeOriginal, iedgedisplaypropertiesDefault, ilistDisplayable, inodeFrom, inodeTo)
	{
		base.EdgeDisplayProperties.Selectable = true;
	}

	void IRenderable.Render(Graphics graphics)
	{
		DoRender(graphics);
	}

	protected virtual void DoRender(Graphics graphics)
	{
		PointF[] points = GetPoints();
		graphics.FillPolygon(base.Selected ? SystemBrushes.Highlight : GetFillColor(), points);
		graphics.DrawPolygon(base.Selected ? SystemPens.Highlight : GetLineColor(), points);
	}

	protected virtual Brush GetFillColor()
	{
		return SystemBrushes.ControlLight;
	}

	protected virtual Pen GetLineColor()
	{
		return SystemPens.ControlDark;
	}

	AttributeCollection ICustomTypeDescriptor.GetAttributes()
	{
		return TypeDescriptor.GetAttributes(GetType());
	}

	EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
	{
		return TypeDescriptor.GetDefaultEvent(GetType());
	}

	PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
	{
		return TypeDescriptor.GetDefaultProperty(GetType());
	}

	object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
	{
		return TypeDescriptor.GetEditor(GetType(), editorBaseType);
	}

	EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
	{
		return TypeDescriptor.GetEvents(GetType());
	}

	EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
	{
		return TypeDescriptor.GetEvents(GetType(), attributes);
	}

	object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
	{
		return this;
	}

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
	{
		return EdgeOriginal.Properties;
	}

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
	{
		return EdgeOriginal.Properties;
	}

	string ICustomTypeDescriptor.GetComponentName()
	{
		return ControlsResources.Edge;
	}

	TypeConverter ICustomTypeDescriptor.GetConverter()
	{
		return TypeDescriptor.GetConverter(GetType());
	}

	string ICustomTypeDescriptor.GetClassName()
	{
		return string.Empty;
	}

	public override bool HitTest(Point point)
	{
		RectangleF[] rectangles = GetRectangles();
		foreach (RectangleF rectangleF in rectangles)
		{
			if (rectangleF.Contains(point))
			{
				return true;
			}
		}
		return false;
	}

	public int GetMidpoint()
	{
		int num = Math.Min((NodeFrom.BoundingRect.Right + NodeTo.BoundingRect.Left) / 2, Math.Max(NodeTo.BoundingRect.Left - MinimalMidpointOffsetFromTheEnd, NodeTo.BoundingRect.Right + MinimalMidpointOffsetFromTheEnd));
		foreach (NodeDisplay child in NodeFrom.Children)
		{
			if (child == NodeTo)
			{
				break;
			}
			num = Math.Min(num, child.BoundingRect.Left - 6);
		}
		return num - ConnectionOffset.Height;
	}

	protected PointF[] GetPoints()
	{
		PointF pointF = NodeFrom.ConnectionPoint(NodeTo.Position);
		PointF pointF2 = NodeTo.ConnectionPoint(NodeFrom.Position);
		float num = (float)(Width - C_MinWidth) / 2f;
		float arrowWidth = ArrowWidth;
		if (pointF.Y == pointF2.Y)
		{
			pointF += ConnectionOffset;
			pointF2 += ConnectionOffset;
			return new PointF[8]
			{
				pointF,
				pointF + new SizeF(arrowWidth, 0f - arrowWidth),
				pointF + new SizeF(arrowWidth, 0f - num),
				pointF2 + new SizeF(0f, 0f - num),
				pointF2 + new SizeF(0f, num),
				pointF + new SizeF(arrowWidth, num),
				pointF + new SizeF(arrowWidth, arrowWidth),
				pointF
			};
		}
		int midpoint = GetMidpoint();
		pointF += ConnectionOffset;
		return new PointF[12]
		{
			pointF,
			pointF + new SizeF(arrowWidth, 0f - arrowWidth),
			pointF + new SizeF(arrowWidth, 0f - num),
			new PointF((float)midpoint + num, pointF.Y - num),
			new PointF((float)midpoint + num, pointF2.Y - num),
			pointF2 + new SizeF(0f, 0f - num),
			pointF2 + new SizeF(0f, num),
			new PointF((float)midpoint - num, pointF2.Y + num),
			new PointF((float)midpoint - num, pointF.Y + num),
			pointF + new SizeF(arrowWidth, num),
			pointF + new SizeF(arrowWidth, arrowWidth),
			pointF
		};
	}

	private RectangleF[] GetRectangles()
	{
		Point point = NodeFrom.ConnectionPoint(NodeTo.Position);
		Point point2 = NodeTo.ConnectionPoint(NodeFrom.Position);
		int num = (Width - C_MinWidth) / 2 + 3;
		if (point.Y == point2.Y)
		{
			point += ConnectionOffset;
			point2 += ConnectionOffset;
			return new RectangleF[1] { RectangleF.FromLTRB(point.X, point.Y - num, point2.X, point.Y + num + 1) };
		}
		int midpoint = GetMidpoint();
		point += ConnectionOffset;
		return new RectangleF[3]
		{
			Rectangle.FromLTRB(point.X, point.Y - num, midpoint + num + 1, point.Y + num + 1),
			Rectangle.FromLTRB(midpoint - num, point.Y - num, midpoint + num + 1, point2.Y + num + 1),
			Rectangle.FromLTRB(midpoint - num, point2.Y - num, point2.X + 1, point2.Y + num + 1)
		};
	}
}
