//
// Plagiarized from Community.VisualStudio.Toolkit extension
//

using System;
using System.Reflection;



namespace BlackbirdSql.Sys;


public interface IBModelPropertyWrapper
{
	PropertyInfo PropInfo { get; }
	string PropertyName { get; }
	string Automator { get; }
	bool IsAutomator { get; }
	bool InvertAutomator { get; }
	int AutomatorEnableValue { get; }


	int DisplayOrder { get; }

	object DefaultValue { get; }

	Func<object, object> WrappedPropertyGetMethod { get; }

}
