// Microsoft.SqlServer.ConnectionDlg.UI, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.UI.WPF.ImageListBase<TIcons>

using System.Collections.Generic;
using System.ComponentModel;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Ctl.Interfaces;

namespace BlackbirdSql.Core.Model;

[EditorBrowsable(EditorBrowsableState.Never)]
public class ModelIconsCollection : CoreIconsCollection
{
	private const string C_Prefix = "BlackbirdSql.Core.Model.ModelResources.";

	private static ModelIconsCollection _Instance;
	private IList<IBIconType> _Icons = null;
	private int _Seed = -1;


	public IBIconType LocalClassicServer_16 => Icons[_Seed];
	public IBIconType LocalClassicServer_32 => Icons[_Seed + 1];
	public IBIconType LocalSuperClassic_16 => Icons[_Seed + 2];
	public IBIconType LocalSuperClassic_32 => Icons[_Seed + 3];
	public IBIconType LocalSuperServer_16 => Icons[_Seed + 4];
	public IBIconType LocalSuperServer_32 => Icons[_Seed + 5];
	public IBIconType SuperClassic_16 => Icons[_Seed + 6];
	public IBIconType SuperClassic_32 => Icons[_Seed + 7];
	public IBIconType SuperServer_16 => Icons[_Seed + 8];
	public IBIconType SuperServer_32 => Icons[_Seed + 9];




	public static new ModelIconsCollection Instance
	{
		get
		{
			_Instance ??= new ModelIconsCollection();

			return _Instance;
		}
	}


	public new IList<IBIconType> Icons
	{
		get
		{
			if (_Icons != null)
				return _Icons;

			// Ensure icons will be sequencially indexed for this ide instance.
			lock (_LockObject)
			{
				_Icons = new List<IBIconType>(18)
				{
					new IconType("LocalClassicServer_16",  C_Prefix),
					new IconType("LocalClassicServer_32",  C_Prefix),
					new IconType("LocalSuperClassic_16",  C_Prefix),
					new IconType("LocalSuperClassic_32",  C_Prefix),
					new IconType("LocalSuperServer_16",  C_Prefix),
					new IconType("LocalSuperServer_32",  C_Prefix),
					new IconType("SuperClassic_16",  C_Prefix),
					new IconType("SuperClassic_32",  C_Prefix),
					new IconType("SuperServer_16",  C_Prefix),
					new IconType("SuperServer_32",  C_Prefix),
				};

				_Seed = _Icons[0].Id;
			};

			return _Icons;
		}
	}



	public override void LoadIconResourceList(IList<IBIconType> iconResourceList)
	{
		base.LoadIconResourceList(iconResourceList);

		foreach (IBIconType icon in Icons)
			iconResourceList.Add(icon);
	}

}
