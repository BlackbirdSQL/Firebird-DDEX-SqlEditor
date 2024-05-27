# BlackbirdSQL DDEX 2.0 with SqlEditor Provider for Firebird

## Change log

### v12.0.0.0 Multi-statement (batch) SQL and iSql support.
__New/ Enhancements__</br>
-- Implemented multi-statement SQL support. Note: For block sql statements the statement terminator will need to be changed to 'GO' or similar if it contains a semi-colon and should be reset back to the default semi-colon to continue processing subsequent statements that are terminated using the default.</br>
-- First phase of database independence from the extension to a service hub. All data access now takes place in BlackbirdSql.Data.</br>
-- Added additional statistics to the SqlEditor statistics output. </br>
__Fixes__</br>
-- Bypassed FirebirdClient statistics bugs. SQL statstics are now correctly reported. 

### v11.3.0.2 Updated snippets.
__New/ Enhancements__</br>
-- Updated Language service snippets to Firebird syntax.</br>
__Fixes__</br>
-- None. 

### v11.3.0.1 Patch to address Snippet insertion error.
__New/ Enhancements__</br>
-- None.</br>
__Fixes__</br>
-- Insertion of snippets into scripts caused a lock requiring the escape key to be pressed. Note that the majority of snippets are not yet updated to Firebird syntax. 

### v11.3.0.0 Firebird Intellisense and Language Service phase 1 implementation.
__New/ Enhancements__</br>
-- Implemented Phase 1 of Firebird Language Service.</br>
-- Improved RunningConnectionTable drift detection.</br>
-- Created Firebird Intellisense MetaDataFactory skeleton classes for predictive text.</br>
-- Added a user option for selecting the mandated language service for the`SqlEditor`. The available language services are `Firebird-SQL`, `Transact-SQL SSDT`, `Transact-SQL90` and `Unified-SQL Azure Data Lake`.</br>
-- Added language service preferences for `Firebird-SQL`.</br>
-- Implemented several performance enhancements.</br>
__Fixes__</br>
-- Intellisense no longer reports the Firebird string concatenation operator `||` as an error. This is part of phase 1 of the Firebird language service implementation. 

### v11.2.0.3
__New/ Enhancements__</br>
-- None.</br>
__Fixes__</br>
-- Fixed bug where multiple unique new queries were not being registered in the Rdt.

### v11.2.0.2
__New/ Enhancements__</br>
-- None.</br>
__Fixes__</br>
-- Fixed bug introduced in V11.2.0.0 where a query without a connection crashed the IDE when a connection was selected from the dropdown.

### v11.2.0.0
__New/ Enhancements__</br>
-- Implemented advanced handling of lost connections and recovery.</br>
-- Forced final extension package assembly to use Nuget packages installed in referenced projects. This circumvents the mess ToolsVersion 15.0 projects cause with package referencing.</br>
-- Added selection and print commands to grid results and statistics context menus.</br>
-- Linkage tables are no longer rebuilt if modifications to a connection will have no affect on them.</br> 
-- Equivalent connections with shorter names in project app.configs are given precedence when loading connections into the running connection table. This is to try and avoid equivalent connections with long names allocated by the edmx wizard.</br>
-- For the SqlEditor, implemented several shortcut key combinations that correspond with the SqlServer SqlEditor shortcut keys.</br>
__Fixes__</br>
-- Fixed issue where a lost connection did not correctly dispose of a LinkageParser.</br>
-- Removed debug commands from Editor context menu.</br>
-- Fixed issue where the `New Query` command was appearing in non-Firebird Server Explorer nodes and the `Reset to defaults` user options command was appearing in the VS Tools menu.</br>
-- Fixed issue where active connections in SqlEditor documents were not being updated if the connection was modified outside of an editor document. __Warning__: Any modifications to connections will cause the connection to be renewed in all documents where it is active. This includes modifying the connection through Server Explorer, an SqlEditor window and the `New connection` option in the edmx wizard, but does not include connections modified within Project Settings. Any active TTS transactions where the connection is active will be cleared on renewal. 

### v11.1.1.1
__New__</br>
-- Implemented a user option under `DDEX Provider > General > Validation` to Enable/Disable the recovery procedure for provider factories that have been placed into an error state by the IDE after loading late (Defaults to `Enabled`. This will revert to `Disabled` once Npgsql is fixed.).</br>
__Fixes__</br>
-- Implemented autoload DesignMode UIContext for edmx models and xsd datasets that have been left open when a project is closed. The design interfaces should no longer require reopening due to an early load error.

### v11.1.1.0
__New__</br>
Implemented recovery procedure for provider factories that arrived late or used ConfigurationManager registration and were invalidated by VS after BlackbirdSql loaded it's running connection table.</br>
Added `Execute with TTS` as a selectable dropdown button for the `Execute` command in SqlEditor.</br>
Added user option for auto-enabling TTS for DDL statements under `Query Execution > General` user options.</br>
__Fixes__</br>
Implemented validation and trace warning of the project property `OutputType` for types other than Int32 for [EFCorePowerTools](https://github.com/ErikEJ/EFCorePowerTools).</br>
Resolved issue where SqlEditor DDL query commands were failing.

### v11.1.0.2
Added edmx and xsd auto-close on solution close user options. Closing prevents potential load errors and drag-drop clipboard errors.</br>
Streamlined asynchronous building of trigger linkage tables.


### v11.1.0.1
Added parser linkage timeout user option. Parsers that exceed the timeout will now prompt the user for a time extension.</br>
Fixed several unhandled NotOnUIThread exceptions. In most cases the processes now switch over to the ui thread if they're not already on it, rather than throwing an exception.</br>
Fixed issue where a saved query only began displaying it's dirty state caption after a few minutes. This is a Visual Studio internal anomaly so the document is force saved a second time to kickstart dirty state notifications.</br>
Fixed the exception message dialog which had been corrupted during porting.</br>
Began a cleanup of redundant classes.

### v11.1.0.0
Added on-demand synchronous Trigger/Generator linkage user option.</br>
Resolved ussue where malformed DocumentMoniker was causing Editor Save to corrupt document locks. Monikers were deliberately malformed for Intellisense to function after a moniker name change. To resolve this issue virtual project items have been shelved in favour of passing it over to VS and maintaining an xref table to the original explorer moniker. This approach is far more efficient and faster.</br>
Renamed several classes and consolidated classes. Also began process of clearing out redundant SqlServer specific code.

### v11.0.0.1
Some convertors were going idle when switching between persistent and transient instances of the same settings model.</br>
Removed a 10 second debugging test Thread.Sleep() that was left in the AbstractRunningConnection async payload.

### v11.0.0.0
First official full release of BlackbirdSql.</br>
Improved interactive synchronization between Connection Dialog and RunningConnectionTable.
Resolved RunningConnectionTable deadlock red zone issues.</br>
Removed redundant Csb DataSource Case name mangling validations.</br>
Resolved several SE, edmx and RunningConnectionTable sync issues.</br>
Removed ConnectionSource debug messages.</br>
Prevented DatasetId glyph removal in Session connection dialogs when connection has not changed.

### v10.1.2.3
Resolved several SE, edmx and RunningConnectionTable sync issues.

### v10.1.2.2
Synchronized edmx with RunningConnectionTable connection management.</br>
Implemented recovery procedures for SE glitch that leaves a sited explorer connection corrupted and dangling after loading another solution.

### v10.1.2.1
Included handling of IDE `no solution` state and early loading of the connection dialog.</br>
Prevented LinkageParser replication when a weak equivalency has been refreshed.</br>
Handle asynchronous VS load glitch referencing incorrect invariant assembly when an edmx is open on load.</br>
Validation of Solution's configuration settings can now only be launched from an open SE node's context menu.

### v10.1.2.0
Final preview version. Implemented SqlEditor and Intellisense support for `.fbsql` disk files.

### v10.1.1.1
Final preview version. Synchronized Intellisense and RdtManager.

### v10.1.1.0
Final preview version. Implemented wait timeouts on all possible deadlock logic.

### v10.1.0.2
Prevented transient LinkageParser deletion and relink during SE connection modification.

### v10.1.0.1
Implemented asynchronous Server Explorer enumerations.</br>
Resolved early load deadlock issue.

### v10.1.0.0
Full integration of Server Explorer and RunningConnectionTable connection management.</br>
Improved asynchronous loading of trigger linkage tables.</br>
Defaulted ApplicationName as a weak equivalency connection property for loading multiple equivalent connections.</br>
Resolved several Server Explorer synchronization issues.

### v10.0.3.0
Moved validation Globals flag variable from Solution and Project Globals to user files.</br>
Resolved issues with User Options dialogs.</br>
Resolved issue where SqlEditor text output of DateTime columns threw an exception.

### v10.0.2.1
Resolved bug where the readonly attribute of dependent properties were not being updated by their automator.

### v10.0.2.0
Implemented configurable equivalency keys.</br>
Resolved bug in user options when collapsing then expanding a category.

### v10.0.1.5
Implemented connection naming and resolved centralized connection issues.

### v10.0.1.4
Resolved Generic Blob handling exceptions.

### v10.0.1.3
Resolved Blob handling exceptions and Readme update.

### v10.0.1.2-prerelease
Resolved Blob handling exceptions and name update.

### v10.0.1.1-prerelease
Resolved Blob handling exceptions.

### v10.0.1.0-prerelease
Pre-release version for publishing on Visual Sturio Markeyplace.

### v10.0.0.101
Version sync.

### v9.9.1.03-prelease
Implemented SqlEditor text output, enhanced SE context menus and resolved outstanding issues.

### v9.9.1.02-prelease
Fixed issue where DSRefBuilder used an empty Site.

### v9.9.1.01-prelease
Fixed issue with opening and then cancelling an IVsDataConnectionDialog in a custom context.

### v9.1.0.91-prelease
Upgraded the FirebirdClient to 10.0.0.0.

### v9.1.0.90-prelease
Centralized connection management and implemented outstanding connection features.

### v9.1.0.88-prelease
Implemented single click check boxes and radio button into Visual Studio user settings Property grid.</br>
Streamlined and Centralized DocumentMoniker and ConnectionMoniker management.

### v9.1.0.87-prelease
Final pre-release using a release build.

### v9.1.0.84-beta
Fixed anomaly where an attached longrunning async task had not yet been launched by a task factory launching task. This caused a deadlock when returning sync tasks attempted a wait on the launcher.

### v9.1.0.74-beta
Fixed potential deadlock on async -> sync (async cancel) -> async restart -> sync (async cancel) scenario.

### v9.1.0.6-beta
Initial beta release.

### v9.1.0.5-alpha
Extended user reporting for IDE StatusBar, TaskHandler and Output window for background and UI tasks and Exceptions.

### v0.1.03-alpha
Streamlined async to sync transitions.

### v0.1.02-alpha
Implemented VSIX installer.

### v0.1.0-alpha
Initial release.