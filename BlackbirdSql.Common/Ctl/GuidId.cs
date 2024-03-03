// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.GuidId
using System;
using System.Globalization;



namespace BlackbirdSql.Common.Ctl;

public sealed class GuidId(Guid guid, uint id)
{
	private Guid _Clsid = guid;
	private readonly uint _Id = id;


	public Guid Clsid => _Clsid;

	public uint Id => _Id;

	public override string ToString()
	{
		return _Clsid.ToString() + "/" + _Id.ToString(CultureInfo.InvariantCulture);
	}

	public override bool Equals(object o)
	{
		if (o is not GuidId guidId)
		{
			return false;
		}
		if (Clsid.Equals(guidId.Clsid))
		{
			return Id == guidId.Id;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Clsid.GetHashCode() ^ Id.GetHashCode();
	}
}
