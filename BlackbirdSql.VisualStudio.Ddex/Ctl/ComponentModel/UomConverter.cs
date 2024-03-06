// System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// System.Windows.Forms.OpacityConverter
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.VisualStudio.Ddex.Properties;


namespace BlackbirdSql.VisualStudio.Ddex.Ctl.ComponentModel;

// =========================================================================================================
//  									UomConverter TypeConverter Class
//
/// <summary>
/// Formats a value dependent on culture and minimum / maximum / default values.
/// Also provides user input synonyms for each of these values.
/// Uses LiteralRangeAttribute to identify min, max and unit of measure key.
/// Uses any attribute derived from or equal to DefaultValueAttribute for the default.
/// Refer to the RegisterModel() method for the resource naming conventions for
/// accessing globalized strings in AttributeResources.resx.
/// </summary>
// =========================================================================================================
public class UomConverter : AbstractUomConverter
{
	protected override System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;
}
