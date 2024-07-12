# Getting Started Guide

### Configuring the App.Config
* __Using BlackbirdSql's Solution Validation Utility__: On older versions of the `FirebirdSql.Data.FirebirdClient` and `EntityFramework.Firebird` packages your `App.config` will not be updated with the correct settings after adding the packages.</br>
You can run the `BlackbirdSql Solution Validation` utility from the context menu of any Firebird node in Server Explorer to update your solution projects' App.config files to the correct settings applicable to each project.
* __HRESULT reference error:__ As of BlackbirdSql release 13.0.0.0 the Firebird and EntityFramework6 assemblies are shipped with the extension. This is to ensure that design time models and wizards are guaranteed to work even if your projects' referenced versions of the EntityFramework versions differ.</br>
Your projects' referenced versions will still be used at runtime, however you may receive an `HRESULT` error after the upgrade.</br>
If rebuilding your projects and then restarting the IDE does not resolve the issue it may be necessary to perform a once-off deletion of the `.vs` folder and then a rebuild for the new system to work correctly at design time.
</br>

## Deconstructing connection naming, equivalency and SE integration

If you're finding BlackbirdSql's RunningConnectionTable management of connections confusing, this document will go some way towards explaining some of the behaviour.
</br></br>

#### Connection Equivalency
Server Explorer (SE) uniquely identifies connections based on their equivalency. By that we mean connections that will produce the same results are considered equivalent, based on the defined equivalency keys. No further distinction takes place.</br>
BlackbirdSql adopts the same method of uniquely identifying distinct connections.</br>
BlackbirdSql lists these equivalency connection properties under `Mandatory equivalency keys` in it's user settings.</br>
For all other connection properties BlackbirdSql allows the definition of additional equivalency keys under `Optional equivalency keys`. By default `Application Name` is set as an additional equivalency key, so if you need an equivalent connection to be distinct, you can enter your own custom Application Name. Also, Application Name is uniquely viewed as a special Weak Equivalency Key by BlackbirdSql. In other words, although it can be used to create a distinct unique connection, it will not be considered unique by the LinkageParser, which creates trigger/sequence linkage tables for a connection. This eliminates the creation of duplicate linkage tables.</br>
Care should be taken using other connection properties as equivalency keys because having large numbers of "non-equivalent" equivalent connected databases can easily lead to confusion.</br>
</br>

#### Connection Naming
Because there is ambiguity in the naming conventions of a database's identity components we have settled on our own set of conventions:
* __Dataset__: This is the shortened/stripped filename of the database. It should be evident that this can lead to ambiguity because multiple different databases on the same server often have the same filename. Nonetheless the Dataset can serve as the basis for auto-constructing a unique database key (DatasetKey).
* __DatabaseName__: This is the unique Dataset name (DatasetId) within the scope of a Server/DataSource. If not specified it will be derived from Dataset. The DatabaseName will be suffixed with _2, _3 etc, to maintain uniqueness if necessary.
* __DatasetKey__: This is the unique global ConnectionName of a connection. If no other names are proposed DatasetKey will be auto-generated in the form `Server (DatabaseName)`.
* __ConnectionName__: If you do not want to use the DatasetKey form `Server (DatabaseName)` you can propose a name for the DatasetKey. This is the value used by the SE when you rename a connection and will override any name you may have defined in DatasetId/DatabaseName. Again this name may be suffixed with a numeric if it is not unique. Using this method to name a connection is not recommended.
* __DisplayName__: This is the displayed value of the DatabaseName. This is necessary because dropdown values for the database name (DatasetId) within the scope of a Server/DataSource may not be uniquely identifiable if a ConnectionName has been used. In these instances the DatasetId will include a ConnectionName qualifier.
* __ConnectionKey__: This is the original internal DatasetKey of a Server Explorer connection. This key does not change for the duration of an IDE session, even if a connection is renamed.
* __ConnectionSource__: This is the connection owner of a connection and may be any one of `ServerExplorer`, `Session` (a connection created by BlackbirdSql's SqlEditor), `EntityDataModel` (a connection created from the DataSources Explorer or an EDMX or other EDM wizard spawned from UIHierarchyMarshaler), `Application` (a connection created in a project's settings) or `ExternalUtility` (a connection created by FlameRobin).
</br>

#### Precedence and loading of the RunningConnectionTable (Rct)
Whenever a solution is loaded the Rct is reloaded. This is a relatively fast process, usually taking only a few ms.</br>
Load precedence is as follows:
* Loading begins with the SE (Owner type ServerExplorer).
* Following that FlameRobin connections are loaded (Owner type ExternalUtility). If a FlameRobin connection is not unique by equivalency against already loaded connections, it is ignored. If it's Database name (DatasetId) is not unique it is numerically suffixed. The format of a FlameRobin connection DatasetId is `⧉ DatabaseName` and it's full DatasetKey is `Server (⧉ DatabaseName)`. The identifying glyph is the unicode `2 joined squares`, code 0x29C9.
* Next connections defined in project Entity Data Model's are loaded (Owner type EntityDataModel). Again, duplicate equivalent connections are ignored. The format of an EDM connection DatasetId/DatabaseName is `⛮ [ProjectName] Name` and it's full DatasetKey is `Server (⛮ [ProjectName] Name)`. The identifying glyph is the unicode `Gear with handles`, code 0x26EE.
* Finally connections defined in a project's settings (App.Config) are loaded (Owner type Application). As with ExternalUtility and EDM connections, equivalency duplicates are ignored. The format of an application connection DatasetId/DatabaseName is `⛭ [ProjectName] SettingName` and it's full DatasetKey is `Server (⛭ [ProjectName] SettingName)`. The identifying glyph is the unicode `Gear without hub`, code 0x26ED.</br>
If you do not want Application and EntityDataModel connections loaded into the Rct, you can disable this feature under the BlackbirdSql > Ddex Provider > General user options.
</br>

#### Configured Connections, Session Connections and Ownership
Configured connections are connections with persistence. These include connections with owner `ServerExplorer`, `ExternalUtility`, `EntityDataModel` and `Application`.</br>
Session connections are connections that are owned by SqlEditor or the SE. Whenever a connection is either an SE connection or a connection not in it's persistent state of ownership, ie. that has been modified during a solution session, it converts to a Session connection.</br>
The distinction between Configured and Session connections is unimportant and has no effect on a connection's behavior, but it does effect it's DatasetKey format.
</br></br>

#### Summary
* Whenever a FlameRobin, EDM or Application configured connection is updated by the SE or SqlEditor in a connection dialog, ownership is transferred and the connection converts to a Session connection. The glyph is dropped to denote the connection is no longer in it's original state.
* If the SE takes ownership, ownership is persistent, but if the SqlEditor/Session takes ownership, ownership is volatile, reverting back on a solution reload.
* If a unique connection is created in SqlEditor (Session), that new connection will be added to the SE unless the `Add New Connections to Server Explorer` checkbox is unchecked.
* If an attempt is made to add a connection in the SE that is equivalent to an existing connection, the SE will revert back to the existing connection.
* Any changes to connections within Application settings connection dialogs will not update the Rct. To have those connections updated to the Rct requires a solution reload. The same applies to changes made within FlameRobin.
* If a connection is deleted in the SE, ownership of the connection will revert to `Session` and still appear in dropdowns and editor windows, but will be dropped on a solution reload.
* It is always preferable to use DatsetId's for custom naming of DatasetKeys, rather than a global ConnectionName, because connection names do not differentiate by Server/DataSource.
* Renaming a connection using the SE `Rename` option creates a global ConnectionName. Rather use the `Modify Connection` option and rename the DatasetId/DatabaseName under 'Advanced', because this allows BlackbirdSql to name connections using the `Server (DatabaseName)` format.
* Whenever a connection is modified in the SE or Sqleditor, those changes will be reflected globally.</br>
Specifically, changes made in the SE will reflect in SqlEditor, but changes made in SqlEditor will reflect in the SE only if they exist in the SE.
