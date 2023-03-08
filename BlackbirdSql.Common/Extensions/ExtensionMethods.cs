using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

namespace BlackbirdSql.Common.Extensions;


// =========================================================================================================
//											ExtensionMethods Class
//
/// <summary>
/// Central class for package external class extension methods. 
/// </summary>
// =========================================================================================================
static class ExtensionMethods
{

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Converts a string to title case and strips out spaces. This provides a
	/// readable/usable column name format for connection property descriptors.
	/// </summary>
	/// <param name="value"></param>
	/// <returns>
	/// The title case string without spaces else returns the original if the string
	/// is null
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static string DescriptorCase(this string value)
	{
		if (string.IsNullOrEmpty(value))
			return value;

		return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value).Replace(" ", "");
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an extended get of a connection descriptor's value
	/// if it's underlying connection properties object implements the dynamic
	/// <see cref="ICustomTypeDescriptor"/> interface.
	/// </summary>
	/// <param name="descriptor"></param>
	/// <param name="connectionProperties"></param>
	/// <returns>The descriptor's current value</returns>
	// ---------------------------------------------------------------------------------
	public static object GetValueX(this PropertyDescriptor descriptor, IVsDataConnectionProperties connectionProperties)
	{
		object component = connectionProperties;

		if (connectionProperties is ICustomTypeDescriptor customTypeDescriptor)
		{
			component = customTypeDescriptor.GetPropertyOwner(descriptor);
		}

		return descriptor.GetValue(component);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Mimicks the search performed by assignment to <see cref="ListControl.SelectedValue"/> where no <see cref="ListControl.ValueMember"/> exists.
	/// </summary>
	/// <param name="comboBox"></param>
	/// <param name="value"></param>
	/// <returns>The new found <see cref="ComboBox.Items"/> value at the new <see cref="ComboBox.SelectedIndex"/> else null</returns>
	/// <remarks>
	/// The method returns the assigned object if found so that inline assignments in series can also cast the object.
	/// eg.string str = (string)combo.SetSelectedValueX(obj);</remarks>
	// ---------------------------------------------------------------------------------
	public static object SetSelectedValueX(this ComboBox comboBox, object value)
	{
		if (value == null)
		{
			comboBox.SelectedIndex = -1;
			return null;
		}
		
		int result;

		if (value is string str)
			result = comboBox.FindStringExact(str);
		else
			result = comboBox.FindStringExact(value.ToString());

		comboBox.SelectedIndex = result;

		return result == -1 ? null : value;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Allows casting of <see cref="ComboBox.SelectedIndex"/> when doing multiple assignments in series.
	/// eg. string x = cbo.SetSelectedIndexX(index).ToString();
	/// </summary>
	/// <param name="comboBox"></param>
	/// <param name="index"></param>
	/// <returns>The new value of <see cref="ComboBox.SelectedIndex"/> else -1.</returns>
	// ---------------------------------------------------------------------------------
	public static int SetSelectedIndexX(this ComboBox comboBox, int index)
	{
		comboBox.SelectedIndex = index;

		return comboBox.SelectedIndex;
	}

}
