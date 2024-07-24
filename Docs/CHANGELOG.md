# BlackbirdSQL DDEX 2.0 with SqlEditor Provider for Firebird

## Change log


### v13.1.0.1 Overhaul of connection strategy.

#### New / Enhancements
- Extension load time statistics: Introduced a new user option that will send a notification to the output window of the load time statistics of the BlackbirdSql extension. Enabling the option has no performance overhead. The total load time averages under 15ms.
- Improved efficiency of `IOleCommandTarget` `QueryStatus` and `Exec` message responses. Toolbar commands for a query window now access common non-volatile status variables instead of rebuilding their environments for each successive message on each command. A global drift detection stamp in the Running Connection Table determines if the environments for each query window are invalidated. This dramatically reduces the processing and memory requirements of the SQL Editor and Language modules which previously used the same mechanism as the Microsoft SqlServer extensions.
- Implemented the `Clone To New Query` toolbar command as is available in the SqlServer SqlEditor extension. The command button is placed immediately to the right of the `New Query` command.
- Enabled the `New Query` toolbar command for Queries that have no active connection selected.
- Session connections now display the Large circle glyph (unicode `\u25EF`). Session connections are non-presistent connections that have been created during a session or are modified persistent connections that are no longer in their original persistent form.
- Session connections that use Entity Data Model or Project Settings connections, and that have no stored password, will invoke the `IVsDataConnectionPromptDialog` password prompt before attempting to connect. After entering a password the connection converts to a Session connection.
- Introduced a low overhead keepalive task. Connections with active transactions will remain active irrelevant of the ConnectionLifetime setting and the FirebirdClient internal timeouts. Connections with no active transactions and that have their ConnectionLifetime set will be closed if there is no activity after the specified timeout. Broken connections are force closed and disposed and any active transactions discarded. A status change notification will be sent to the Output window in all these cases.
- Before the detailed error message window is displayed, the IDE will now automatically activate the window of a longer running query that was previously hidden if it produces an SQL error.
- Enhanced the SqlEditor status bar Query Status label information.
- Removed unused types that were a hangover of the SqlServer SqlEditor port. The graphics types/classes for the ExecutionPlan visualizer remain for possible future implementation but are set to a `Build Action` of `None`.
- General code and type naming cleanup.
#### Fixes
- Improved the handling of database connections that are frozen due to a broken database network connection. Opening a connection is now performed asynchronously. The connection strategy service will attempt to rollback active transactions and close the connection, then attempt to dispose of the connection and any active transaction objects, and finally release transaction and connection objects. This should prevent lockouts of a query window after a network failure.
- Removed Debug commands from query context menus.
- Resolved bug that intermittently caused a query to freeze on completion when tab captions were incprrectly updated without switching to the main thread.
- Resolved issue where tab captions were not updated after an SQL batch script was cancelled or had failed.
- Fixed bug where a project event was was allowed to be entered before project event handlers had been initialized.
- Fixed bug where Path.GetExtension() threw an exception if the ActiveDocument name could not be parsed.


### v13.1.0.0 Overhaul of connection strategy.
__New / Enhancements__</br>
-- Extension load time statistics: Introduced a new user option that will send a notification to the output window of the load time statistics of the BlackbirdSql extension. Enabling the option has no performance overhead. The total load time averages under 15ms.</br>
-- Improved efficiency of `IOleCommandTarget` `QueryStatus` and `Exec` message responses. Toolbar commands for a query window now access common non-volatile status variables instead of rebuilding their environments for each successive message on each command. A global drift detection stamp in the Running Connection Table determines if the environments for each query window are invalidated. This dramatically reduces the processing and memory requirements of the SQL Editor and Language modules which previously used the same mechanism as the Microsoft SqlServer extensions.</br>
-- Implemented the `Clone To New Query` toolbar command as is available in the SqlServer SqlEditor extension. The command button is placed immediately to the right of the `New Query` command.</br>
-- Enabled the `New Query` toolbar command for Queries that have no active connection selected.</br>
-- Session connections now display the Large circle glyph (unicode `\u25EF`). Session connections are non-presistent connections that have been created during a session or are modified persistent connections that are no longer in their original persistent form.</br>
-- Session connections that use Entity Data Model or Project Settings connections, and that have no stored password, will invoke the `IVsDataConnectionPromptDialog` password prompt before attempting to connect. After entering a password the connection converts to a Session connection.</br>
-- Introduced a low overhead keepalive task. Connections with active transactions will remain active irrelevant of the ConnectionLifetime setting and the FirebirdClient internal timeouts. Connections with no active transactions and that have their ConnectionLifetime set will be closed if there is no activity after the specified timeout. Broken connections are force closed and disposed and any active transactions discarded. A status change notification will be sent to the Output window in all these cases.</br>
-- Before the detailed error message window is displayed, the IDE will now automatically activate the window of a longer running query that was previously hidden if it produces an SQL error.</br>
-- Enhanced the SqlEditor status bar Query Status label information.</br>
-- Removed unused types that were a hangover of the SqlServer SqlEditor port. The graphics types/classes for the ExecutionPlan visualizer remain for possible future implementation but are set to a `Build Action` of `None`.</br>
-- General code and type naming cleanup.</br>
__Fixes__</br>
-- Improved the handling of database connections that are frozen due to a broken database network connection. Opening a connection is now performed asynchronously. The connection strategy service will attempt to rollback active transactions and close the connection, then attempt to dispose of the connection and any active transaction objects, and finally release transaction and connection objects. This should prevent lockouts of a query window after a network failure.</br>
-- Removed Debug commands from query context menus.</br>
-- Resolved bug that intermittently caused a query to freeze on completion when tab captions were incprrectly updated without switching to the main thread.</br>
-- Resolved issue where tab captions were not updated after an SQL batch script was cancelled or had failed.</br>
-- Fixed bug where a project event was was allowed to be entered before project event handlers had been initialized.</br>
-- Fixed bug where Path.GetExtension() threw an exception if the ActiveDocument name could not be parsed.

### v13.0.0.0 Ring-fenced the Firebird assemblies in the IDE Extension context.
__New/ Enhancements__</br>
-- Accessing the Firebird assemblies at design time will now always use the extension's shipped assemblies. This means design time wizards are guaranteed to work even if a project's referenced versions differ. Your projects' referenced versions will still be used at runtime, however if you're receiving an HRESULT design time error it may be necessary to perform a once-off deletion of the .vs folder and a rebuild for the new system to work correctly at design time.</br>
-- Optimistic trigger linkage. Schema requests where only a subset of triggers are required no longer wait for the full trigger linkage of a database to complete. The full linkage task will still be launched but a satellite task will be launched to satisfy the request in the interim. Typically this will take place when a table's columns, indexes, foreign keys or associated triggers are requested and the database trigger linkage tables are not yet available.</br>
-- Query execution now uses the TaskScheduler instead of creating threads. All database access is now performed asynchronously. This also improves query execution cancellation using a common CancellationToken which is carried through to the disk storage readers and native database engine functions.</br>
-- Any open edmx models are now auto-closed by default if at least one of the models is not on-screen when a project is closed. This is to circumvent a Visual Studio bug that causes the EDM sub-system to fail when switching to hidden models, requiring an IDE session restart. The feature can be disabled in user options.</br>
-- The tab tooltips for saved .fbsql queries now display the full path to the saved query.</br>
-- Results tabs now provide additional information. The SQL result tab displays the number of output rows and number of SQL statements. Statistics results tabs display the number of trials. The messages tab displays the number of messages.</br>
-- Connection display names now always display the glyph in all instances where the connection is either an Entity Data Model connection, Application (Project settings / App.config) connection or a FlameRobin connection.</br>
__Fixes__</br>
-- Resolved bug where the connection source was not being correctly identified. This resulted in some Entity Data Model connections getting past the validation checks that are intended to prevent EDM wizards from corrupting Server Explorer connections.</br>
-- Resolved several issues where connection naming was not being uniformly replicated across the extension.</br>
-- Resolved issue where closed connection nodes called the running connection table for updates when a connection was about to be modified. This caused proposed DatasetId's to be converted to proposed ConnectionNames and also initiated an unecessary trigger linkage that was then discarded.</br>
-- Resolved several instances where database connections were left dangling. Server Explorer may still leave connections dangling where multiple nodes are expanded on a refresh. This is a minor internal Server Explorer bug which causes each consecutive refresh to sporadically increase the active connections by one.</br>
-- Resolved a bug where selecting another connection for a query caused all queries using that same connection to change.</br>
-- Removed lazy loading of the SettingsManager from the User Options Model, which is already lazy and which caused VS to hang when a Model was requested early. Sections of the Model were cribbed from VisualStudio.Community which contains this bug.

### v12.0.0.1 Improved "Query is Executing" animation and addressed `Execution Plan` TTS bug.
__New/ Enhancements__</br>
-- Implemented a flowing progress bar for the "Query is Executing" animation.</br>
__Fixes__</br>
-- Addressed issue where a conflict occurs on an active transaction and an `Estimated Execution Plan`" has been requested. 

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