// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.CommonIcons

using System.Collections.Generic;
using System.ComponentModel;
using BlackbirdSql.Core.Ctl.Interfaces;

namespace BlackbirdSql.Core.Ctl;

[EditorBrowsable(EditorBrowsableState.Never)]
public class CoreIconsCollection : AbstractIconsCollection
{
	private const string C_Prefix = "BlackbirdSql.Core.CoreResources.";

	private static CoreIconsCollection _Instance;
	private IList<IBIconType> _Icons = null;
	private int _Seed = -1;


	public IBIconType CollapseChevronLeft_16 => Icons[_Seed];
	public IBIconType Error_16 => Icons[_Seed + 1];
	public IBIconType ExpandChevronRight_16 => Icons[_Seed + 2];
	public IBIconType FirewallRule => Icons[_Seed + 3];
	public IBIconType Property_16 => Icons[_Seed + 4];
	public IBIconType Pushpin_16 => Icons[_Seed + 5];
	public IBIconType PushpinUnpin_16 => Icons[_Seed + 6];
	public IBIconType Report_16 => Icons[_Seed + 7];
	public IBIconType Report_32 => Icons[_Seed + 8];
	public IBIconType Search_16 => Icons[_Seed + 9];
	public IBIconType SortAscending_16 => Icons[_Seed + 10];
	public IBIconType Spinner_16 => Icons[_Seed + 11];
	public IBIconType Warning_16 => Icons[_Seed + 12];

	public IBIconType ServerError_16 => Icons[_Seed + 13];
	public IBIconType ServerError_32 => Icons[_Seed + 14];
	public IBIconType DataSourceTarget_16 => Icons[_Seed + 15];
	public IBIconType DataSourceTarget_32 => Icons[_Seed + 16];
	public IBIconType EmbeddedDatabase_16 => Icons[_Seed + 17];
	public IBIconType EmbeddedDatabase_32 => Icons[_Seed + 18];
	public IBIconType ClassicServer_16 => Icons[_Seed + 19];
	public IBIconType ClassicServer_32 => Icons[_Seed + 20];


	public override IList<IBIconType> Icons
	{
		get
		{
			if (_Icons != null)
				return _Icons;

			// Ensure icons will be sequencially indexed for this ide instance.
			lock (_LockObject)
			{
				_Icons = new List<IBIconType>(15)
				{
					new IconType("CollapseChevronLeft",  C_Prefix),
					new IconType("Error_16",  C_Prefix),
					new IconType("ExpandChevronRight_16",  C_Prefix),
					new IconType("FirewallRule",  C_Prefix),
					new IconType("Property_16",  C_Prefix),
					new IconType("Pushpin_16",  C_Prefix),
					new IconType("PushpinUnpin_16",  C_Prefix),
					new IconType("Report_16",  C_Prefix),
					new IconType("Report_32",  C_Prefix),
					new IconType("Search_16",  C_Prefix),
					new IconType("SortAscending_16",  C_Prefix),
					new IconType("Spinner_10BlueDots_16",  C_Prefix),
					new IconType("Warning_16",  C_Prefix),

					new IconType("ServerError_16",  C_Prefix),
					new IconType("ServerError_32",  C_Prefix),
					new IconType("DataSourceTarget_16",  C_Prefix),
					new IconType("DataSourceTarget_32",  C_Prefix),
					new IconType("EmbeddedDatabase_16",  C_Prefix),
					new IconType("EmbeddedDatabase_32",  C_Prefix),
					new IconType("ClassicServer_16",  C_Prefix),
					new IconType("ClassicServer_32",  C_Prefix)
				};

				_Seed = _Icons[0].Id;
			};

			return _Icons;
		}
	}




	public static CoreIconsCollection Instance => _Instance ??= new CoreIconsCollection();




	public override void LoadIconResourceList(IList<IBIconType> iconResourceList)
	{
		base.LoadIconResourceList(iconResourceList);

		foreach (IBIconType icon in Icons)
			iconResourceList.Add(icon);
	}

}
