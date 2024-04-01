
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml;

using Microsoft.Data.ConnectionUI;


namespace BlackbirdSql.Core.Ctl.Interfaces;

// =========================================================================================================
//
//									AbstractPropertyAgent Class - Accessors
//
// =========================================================================================================
public interface IBPropertyAgent : IDisposable, ICustomTypeDescriptor, IDataConnectionProperties,
	IComparable<IBPropertyAgent>, IEquatable<IBPropertyAgent>, INotifyPropertyChanged,
	INotifyDataErrorInfo, IWeakEventListener

{

	// ---------------------------------------------------------------------------------
	#region Property Accessors - IBPropertyAgent
	// ---------------------------------------------------------------------------------


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// List of the descriptors that fall under advanced options.
	/// </summary>
	// ---------------------------------------------------------------------------------
	// string[] AdvancedOptions { get; }


	DbConnection DataConnection { get; }


	/// <summary>
	/// Accessor to parsing and assembly of a DbConnectionStringBuilder object for
	/// <see cref="IBPropertyAgent"/>s whose properties represent <see cref="DbConnection"/>
	/// parameters.
	/// </summary>
	DbConnectionStringBuilder ConnectionStringBuilder { get; set; }

	DescriberDictionary Describers { get; }

	string DatasetKey { get; }


	/// <summary>
	/// The id of the IBPropertyAgent instance dataset.
	/// </summary>
	string DatasetId { get; set; }


	/// <summary>
	/// The string array containing the properties that must be equivalent
	/// for 2 instances of <see cref="IBPropertyAgent"/> to be consider equivalent
	/// be <see cref="AreEquivalent(IBPropertyAgent)"/>.
	/// </summary>
	// string[] EquivalencyKeys { get; }


	/// <summary>
	/// Accessor to the image of the <see cref="ConnectionIcon.IconType"/>
	/// </summary>
	BitmapImage IconImage { get; }


	/// <summary>
	/// The unique id of an IBPropertyAgent base class.
	/// </summary>
	long Id { get; }


	/// <summary>
	/// The string array containing the properties that must be set for
	/// <see cref="IDataConnectionProperties.IsComplete"/> to return true.
	/// </summary>
	// string[] MandatoryProperties { get; }


	/// <summary>
	/// Accessor to the string representation (serialized or otherwise) if the property
	/// collection. For a data based IBPropertyAgent this would be it's ConnectionString.
	/// </summary>
	string PropertyString { get; }


	IDictionary<string, string> ValidationErrors { get; }


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - IBPropertyAgent
	// =========================================================================================================


	void Add(string name, string parameter, Type propertyType, object defaultValue = null,
		bool isParameter = false, bool isAdvanced = true, bool isPublic = true, bool isMandatory = false);



	public void Add(string name, Type propertyType, object defaultValue = null,
		bool isParameter = false, bool isAdvanced = true, bool isPublic = true, bool isMandatory = false);


	bool AreEquivalent(IBPropertyAgent other);

	void Clear();

	void ClearAllErrors();

	IBPropertyAgent Copy();

	void CopyTo(IBPropertyAgent lhs);

	DbCommand CreateCommand(string cmd = null);

	void Dispose(bool disposing);

	object GetProperty(string name);


	Describer GetParameterDescriber(string name);

	Describer GetSynonymDescriber(string synonym);

	Type GetPropertyType(string name);

	string GetPropertyTypeName(string name);

	(Version, bool) GetServerVersion(IDbConnection connection);


	(IBIconType, bool) GetSet_Icon();

	bool IsParameter(string propertyNameName);

	bool Isset(string property);

	void LoadFromStream(XmlReader reader);

	void Parse(DbConnectionStringBuilder csb);

	void PopulateConnectionStringBuilder(DbConnectionStringBuilder csb, bool secure);

	void RaisePropertyChanged(string propertyName);

	void RaisePropertyChanged<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key,
		params string[] propertiesToExclude) where TValue : IEnumerable<string>;


	void ResetConnectionInfo();

	void SaveToStream(XmlWriter writer, bool unsecured);

	void Set(string connectionString);

	bool SetProperty(string name, object value);

	string ToFullString(bool secure);

	IBPropertyAgent ToConnectionInfo();

	bool TryGetDefaultValue(string name, out object value);

	bool TryGetSetDerivedProperty(string name, out object value);

	public void UpdatePropertyInfo(IBPropertyAgent rhs);

	void ValidateTextBoxField(string value, string propertyName);

	bool WillObjectPropertyChange(string name, object newValue, bool removing);


	#endregion Methods


}
