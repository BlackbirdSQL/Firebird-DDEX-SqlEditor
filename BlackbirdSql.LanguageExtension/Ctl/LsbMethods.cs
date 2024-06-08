// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlLanguageServices.Methods
using System.Collections.Generic;
using Babel;
using Microsoft.VisualStudio.Package;



namespace BlackbirdSql.LanguageExtension.Ctl;


public class LsbMethods : Methods
{

	public LsbMethods(IList<MethodHelpText> methods)
	{
		_Methods = methods;
	}




	private readonly IList<MethodHelpText> _Methods;

	public override string OpenBracket
	{
		get
		{
			if (!_Methods[0].ShouldShowParentheses)
			{
				return "";
			}
			return "(";
		}
	}

	public override string CloseBracket
	{
		get
		{
			string result = string.Empty;
			if (_Methods[0].ShouldShowParentheses)
			{
				result = ((!_Methods[0].IsVarArg) ? ")" : ((_Methods[0].Parameters.Count > 0) ? ", ...)" : "...)"));
			}
			return result;
		}
	}


	public override int GetCount()
	{
		return _Methods.Count;
	}

	public override string GetName(int index)
	{
		return _Methods[index].Name;
	}

	public override string GetDescription(int index)
	{
		return _Methods[index].Description;
	}

	public override string GetType(int index)
	{
		return _Methods[index].Type;
	}

	public override int GetParameterCount(int index)
	{
		if (_Methods[index].Parameters != null)
		{
			return _Methods[index].Parameters.Count;
		}
		return 0;
	}

	public override void GetParameterInfo(int index, int paramIndex, out string name, out string display, out string description)
	{
		Parameter parameter = _Methods[index].Parameters[paramIndex];
		name = parameter.Name;
		display = parameter.Display;
		description = parameter.Description;
	}
}
