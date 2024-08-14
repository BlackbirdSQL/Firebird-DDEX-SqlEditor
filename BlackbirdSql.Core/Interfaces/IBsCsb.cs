
using System;
using System.Collections;


namespace BlackbirdSql.Core.Interfaces;

// =========================================================================================================
//
//											IBsCsb Interface
//
// =========================================================================================================
public interface IBsCsb : IDictionary, IDisposable, ICloneable

{

	// ---------------------------------------------------------------------------------
	#region Constructors & Destructors - IBsCsb
	// ---------------------------------------------------------------------------------


	bool ContainsKey(string keyword);
	IBsCsb Copy();
	void CopyTo(IBsCsb lhs);


	#endregion Constructors & Destructors





	// =========================================================================================================
	#region Property Accessors - IBsCsb
	// =========================================================================================================

	string AdornedDisplayName { get; }
	string AdornedTitle { get; }
	string AdornedQualifiedName { get; }
	string AdornedQualifiedTitle { get; }
	int ConnectionLifeTime { get; set; }
	string ConnectionString { get; set; }
	int CommandTimeout { get; set; }
	string DatasetKey { get; }
	string DataSource { get; set; }
	string Database { get; set; }
	string DisplayName { get; }
	long Id { get; }
	bool IsCompleteMandatory { get; }
	bool IsCompletePublic { get; }
	bool IsInvalidated { get; }
	string Moniker { get; }
	string UnsafeMoniker { get; }
	string UserID { get; set; }

	// IDictionary<string, string> ValidationErrors { get; }


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - IBsCsb
	// =========================================================================================================


	/*
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
	*/
	// void PopulateConnectionStringBuilder(Csb csb, bool secure);
	// void ResetConnectionInfo();
	// void SaveToStream(XmlWriter writer, bool unsecured);

	void RefreshDriftDetectionState();

	#endregion Methods


}
