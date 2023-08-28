#region Assembly Microsoft.VisualStudio.Data.Tools.SqlLanguageServices, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System.Collections.Generic;
using Babel;
// using Microsoft.VisualStudio.Package;


// using Microsoft.VisualStudio.Data.Tools.SqlLanguageServices;
// using Ns = Microsoft.VisualStudio.Data.Tools.SqlLanguageServices;


// namespace Microsoft.VisualStudio.Data.Tools.SqlLanguageServices
namespace BlackbirdSql.LanguageExtension
{
	internal class Methods : Microsoft.VisualStudio.Package.Methods
	{
		private readonly IList<MethodHelpText> methods;

		public override string OpenBracket
		{
			get
			{
				if (!methods[0].ShouldShowParentheses)
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
				if (methods[0].ShouldShowParentheses)
				{
					result = ((!methods[0].IsVarArg) ? ")" : ((methods[0].Parameters.Count > 0) ? ", ...)" : "...)"));
				}

				return result;
			}
		}

		public Methods(IList<MethodHelpText> methods)
		{
			this.methods = methods;
		}

		public override int GetCount()
		{
			return methods.Count;
		}

		public override string GetName(int index)
		{
			return methods[index].Name;
		}

		public override string GetDescription(int index)
		{
			return methods[index].Description;
		}

		public override string GetType(int index)
		{
			return methods[index].Type;
		}

		public override int GetParameterCount(int index)
		{
			if (methods[index].Parameters != null)
			{
				return methods[index].Parameters.Count;
			}

			return 0;
		}

		public override void GetParameterInfo(int index, int paramIndex, out string name, out string display, out string description)
		{
			Parameter parameter = methods[index].Parameters[paramIndex];
			name = parameter.Name;
			display = parameter.Display;
			description = parameter.Description;
		}
	}
}
