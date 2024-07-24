
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Xml;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Sys.Ctl;
using Microsoft.Data.ConnectionUI;


namespace BlackbirdSql.Core.Interfaces;

// =========================================================================================================
//
//									IBsPropertyAgent Interface
//
// =========================================================================================================
public interface IBsPropertyAgent : IDisposable, ICustomTypeDescriptor, IDataConnectionProperties,
	IComparable<IBsPropertyAgent>, IEquatable<IBsPropertyAgent>, INotifyPropertyChanged,
	INotifyDataErrorInfo, IWeakEventListener

{

	// ---------------------------------------------------------------------------------
	#region Constructors & Destructors - IBsPropertyAgent
	// ---------------------------------------------------------------------------------


	IBsPropertyAgent Copy();

	void CopyTo(IBsPropertyAgent lhs);


	#endregion Constructors & Destructors





	// =========================================================================================================
	#region Property Accessors - IBsPropertyAgent
	// =========================================================================================================


	string DatasetKey { get; }
	string DataSource { get; set; }
	string Database { get; set; }

	/// <summary>
	/// The unique id of an IBsPropertyAgent base class.
	/// </summary>
	long Id { get; }

	bool IsCompletePublic { get; }

	string UserID { get; set; }
	IDictionary<string, string> ValidationErrors { get; }


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - IBsPropertyAgent
	// =========================================================================================================


	void Add(string name, string parameter, Type propertyType, object defaultValue = null,
		bool isParameter = false, bool isAdvanced = true, bool isPublic = true, bool isMandatory = false);



	public void Add(string name, Type propertyType, object defaultValue = null,
		bool isParameter = false, bool isAdvanced = true, bool isPublic = true, bool isMandatory = false);


	bool AreEquivalent(IBsPropertyAgent other);
	void Clear();
	void ClearAllErrors();
	bool CloseConnection();
	object GetProperty(string name);
	Describer GetParameterDescriber(string name);
	Type GetPropertyType(string name);
	string GetPropertyTypeName(string name);
	Describer GetSynonymDescriber(string synonym);
	bool IsParameter(string propertyNameName);
	bool Isset(string property);
	void LoadFromStream(XmlReader reader);
	void Parse(Csb csb);
	void PopulateConnectionStringBuilder(Csb csb, bool secure);
	void ResetConnectionInfo();
	void SaveToStream(XmlWriter writer, bool unsecured);


	#endregion Methods


}
