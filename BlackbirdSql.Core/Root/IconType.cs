
using BlackbirdSql.Core.Interfaces;




namespace BlackbirdSql.Core;


public class IconType : IBIconType
{
	protected static int _Seed = -1;

	private readonly int _Id;
	private readonly string _Name;
	private readonly string _Prefix;


	public int Id => _Id;
	public string Name => _Name;


	public IconType(string name, string prefix)
	{
		_Id = ++_Seed;
		_Name = name;
		_Prefix = prefix;
	}

	public override string ToString() => $"{_Prefix}{Name}x";
}
