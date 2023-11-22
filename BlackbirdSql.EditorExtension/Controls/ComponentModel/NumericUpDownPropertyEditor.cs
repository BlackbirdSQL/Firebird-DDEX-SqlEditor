using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Ctl.Extensions;
using BlackbirdSql.EditorExtension.Properties;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Utilities;


namespace BlackbirdSql.EditorExtension.Controls.ComponentModel
{
	/// <summary>
	/// This is currently a fail.
	/// </summary>
	public class NumericUpDownPropertyEditor : UITypeEditor
	{
		// private NumericUpDown _UpDownControl;

		private int _Min = int.MinValue;
		private int _Max = int.MaxValue;
		private int _Increment = 1;

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			// Diag.Trace("Getting editor style");
			return UITypeEditorEditStyle.DropDown;
		}


		/// <summary>
		///  Edits the specified object's value using the editor style indicated by the
		///  System.Drawing.Design.UITypeEditor.GetEditStyle method.
		/// </summary>
		/// <param name="context">
		/// The Forms.PropertyGridInternal.PropertyDescriptorGridEntry grid row entry.
		/// </param>
		/// <remarks>
		/// The SettingsModel is referenced in context.Instance.
		/// The SettingsModel property attributes are in context.PropertyDescriptor.Attributes.
		/// The PropertyGridInternal.PropertyGridView is accessible as service IWindowsFormsEditorService.
		/// </remarks>
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			// Get the System.Windows.Forms.PropertyGridInternal.PropertyGridView.
			if (provider.GetService(typeof(IWindowsFormsEditorService)) is not IWindowsFormsEditorService editorService)
			{
				throw Diag.ServiceUnavailable(typeof(IWindowsFormsEditorService));
			}

			if (context.PropertyDescriptor.Attributes[typeof(MinMaxIncrementAttribute)] is MinMaxIncrementAttribute minmaxAttr)
			{
				_Min = minmaxAttr.Min;
				_Max = minmaxAttr.Max;
				_Increment = minmaxAttr.Increment;
			}

			using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
			{

				NumericUpDown udControl = new NumericUpDown
				{
					DecimalPlaces = 0,
					Minimum = _Min,
					Maximum = _Max,
					Increment = _Increment,

					Value = (int)value
				};

				editorService.DropDownControl(udControl);

				value = (int)udControl.Value;
			}

			return value;
		}

		public override bool GetPaintValueSupported(ITypeDescriptorContext context)
		{
			return true;
		}


		public override void PaintValue(PaintValueEventArgs e)
		{
			if (e.Value is not int value)
				return;


			if (e.Context.PropertyDescriptor.Attributes[typeof(MinMaxIncrementAttribute)]
				is MinMaxIncrementAttribute minmaxAttr)
			{
				_Min = minmaxAttr.Min;
				_Max = minmaxAttr.Max;
				_Increment = minmaxAttr.Increment;
			}

			using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
			{

				NumericUpDown udControl = new NumericUpDown
				{
					DecimalPlaces = 0,
					Minimum = _Min,
					Maximum = _Max,
					Increment = _Increment,
					Width = e.Bounds.Width,
					Height = e.Bounds.Height,
					Value = value
				};

				// Diag.Trace("Paint bounds: " + e.Bounds);

				Bitmap bitmap = new(e.Bounds.Width, e.Bounds.Height);

				udControl.DrawToBitmap(bitmap, e.Bounds);
				e.Graphics.DrawImage(bitmap, e.Bounds);

				udControl.Dispose();
				bitmap.Dispose();
			}


			/*
			Color color = (Color)e.Value;
			SolidBrush solidBrush = new SolidBrush(color);
			e.Graphics.FillRectangle(solidBrush, e.Bounds);
			solidBrush.Dispose();
			*/

		}

	}
}
