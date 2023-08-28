// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.GuidId
using System;
using System.Globalization;



namespace BlackbirdSql.Common.Ctl;

public sealed class GuidId
{
	private Guid _guid = Guid.Empty;

	private readonly uint _id;

	public Guid Guid => _guid;

	public uint Id => _id;

	public GuidId(Guid guid, uint id)
	{
		_guid = guid;
		_id = id;
	}

	public override string ToString()
	{
		return _guid.ToString() + "/" + _id.ToString(CultureInfo.InvariantCulture);
	}

	public override bool Equals(object o)
	{
		if (o is not GuidId guidId)
		{
			return false;
		}
		if (Guid.Equals(guidId.Guid))
		{
			return Id == guidId.Id;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Guid.GetHashCode() ^ Id.GetHashCode();
	}
}
