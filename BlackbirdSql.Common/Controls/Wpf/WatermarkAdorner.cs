// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.WatermarkAdorner

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using BlackbirdSql.Core;
using BlackbirdSql.Common.Model.Wpf;

namespace BlackbirdSql.Common.Controls.Wpf;


public class WatermarkAdorner : Adorner
{
	protected override int VisualChildrenCount => 1;

	public ContentPresenter ContentPresenter { get; set; }

	protected WatermarkState State { get; set; }

	private TextBlock TextBlock { get; set; }

	public WatermarkAdorner(WatermarkState state)
		: base(state.Control)
	{
		State = state;
		ClipToBounds = true;
		SetContent(state);
	}

	public void Attach()
	{
		AdornerLayer.GetAdornerLayer(AdornedElement)?.Add(this);
	}

	public void Detach()
	{
		AdornerLayer.GetAdornerLayer(AdornedElement)?.Remove(this);
	}

	public void ShowText()
	{
		if (TextBlock != null)
		{
			Detach();
			TextBlock.Visibility = Visibility.Visible;
			Attach();
		}
	}

	public void HideText()
	{
		if (TextBlock != null)
		{
			Detach();
			TextBlock.Visibility = Visibility.Collapsed;
			Attach();
		}
	}

	protected override Visual GetVisualChild(int index)
	{
		return ContentPresenter;
	}

	protected override Size MeasureOverride(Size constraint)
	{
		ContentPresenter.Measure(constraint);
		return ContentPresenter.DesiredSize;
	}

	protected override Size ArrangeOverride(Size finalSize)
	{
		if (State.Control.IsVisible)
		{
			ContentPresenter.Arrange(new Rect(finalSize));
			return finalSize;
		}
		return new Size(0.0, 0.0);
	}

	private void SetContent(WatermarkState state)
	{
		GridLength width = GridLength.Auto;
		GridLength width2 = new GridLength(1.0, GridUnitType.Star);
		UIElement uIElement = null;
		UIElement uIElement2 = GetTextContent(state);
		TextBlock = uIElement2 as TextBlock;
		Thickness originalPadding = state.OriginalPadding;
		Thickness margin = new Thickness(originalPadding.Left, originalPadding.Top, originalPadding.Right, originalPadding.Bottom);
		double num = 0.0;
		Thickness margin2 = new Thickness(0.0);
		if (state.Image != null)
		{
			num = 16.0;
			if (state.ShowImageOnRight)
			{
				width2 = GridLength.Auto;
				width = new GridLength(1.0, GridUnitType.Star);
				uIElement = uIElement2;
				margin2 = new Thickness(state.OriginalPadding.Right, 0.0, 0.0, 0.0);
				uIElement2 = GetImageContent(state, margin2);
			}
			else
			{
				margin2 = new Thickness(0.0, 0.0, state.OriginalPadding.Left, 0.0);
				uIElement = GetImageContent(state, margin2);
			}
		}
		uIElement2.SetValue(System.Windows.Controls.Grid.ColumnProperty, 1);
		Thickness borderThickness = State.Control.BorderThickness;
		double actualHeight = State.Control.ActualHeight;
		double width3;
		if (State.TextBox != null)
		{
			width3 = State.TextBox.ActualWidth + margin2.Left + margin2.Right + num;
		}
		else
		{
			width3 = State.Control.ActualWidth - originalPadding.Left - originalPadding.Right - borderThickness.Left - borderThickness.Right;
			if (State.Control is ComboBox)
			{
				width3 -= SystemParameters.VerticalScrollBarWidth;
			}
			width3 = Math.Max(0.0, width3);
		}
		System.Windows.Controls.Grid grid = new System.Windows.Controls.Grid
		{
			Height = actualHeight,
			Margin = margin,
			Width = width3,
			VerticalAlignment = VerticalAlignment.Top,
			HorizontalAlignment = HorizontalAlignment.Left,
			ColumnDefinitions =
			{
				new ColumnDefinition
				{
					Width = width
				},
				new ColumnDefinition
				{
					Width = width2
				}
			},
			RowDefinitions =
			{
				new RowDefinition
				{
					Height = GridLength.Auto
				},
				new RowDefinition
				{
					Height = new GridLength(1.0, GridUnitType.Star)
				}
			}
		};
		if (uIElement != null)
		{
			grid.Children.Add(uIElement);
		}
		if (uIElement2 != null)
		{
			grid.Children.Add(uIElement2);
		}
		ContentPresenter = new ContentPresenter
		{
			Content = grid
		};
	}

	private UIElement GetImageContent(WatermarkState state, Thickness margin)
	{
		if (state.Image is Brush)
		{
			return new Rectangle
			{
				Margin = margin,
				Width = 16.0,
				Height = 16.0,
				Fill = state.Image as Brush
			};
		}
		if (state.Image is ImageSource)
		{
			return new Image
			{
				Margin = margin,
				Source = state.Image as ImageSource,
				Stretch = Stretch.None
			};
		}
		if (state.Image == null)
		{
			return null;
		}
		ArgumentException ex = new("Image is not supported");
		Diag.Dug(ex);
		throw ex;
	}

	private UIElement GetTextContent(WatermarkState state)
	{
		TextBlock obj = new TextBlock
		{
			Margin = new Thickness(2.0, 0.0, 0.0, 0.0),
			ClipToBounds = true,
			Text = state.HintText,
			VerticalAlignment = VerticalAlignment.Center,
			FontFamily = state.Control.FontFamily,
			FontSize = state.Control.FontSize,
			Foreground = new SolidColorBrush()
		};
		BindingOperations.SetBinding(obj.Foreground, SolidColorBrush.ColorProperty, new Binding("TextBoxHintTextColor")
		{
			Source = DialogColors.Instance,
			Mode = BindingMode.OneWay
		});
		return obj;
	}
}
