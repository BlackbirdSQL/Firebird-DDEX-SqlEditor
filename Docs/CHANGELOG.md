# BlackbirdSQL DDEX 2.0 with SqlEditor Provider for Firebird

## Change log


### v14.5.3.1002 Fixed Connection Dialog database dropdown .

#### New / Enhancements
- Added __User Option__ `BlackbirdSQL Server Tools > Ddex Provider > Debug > Expected exceptions`. Enabling this option causes expected exceptions to be output to the __BlackbirdSql__ section of the VS output window for Release builds. 
#### Fixes
- Fixed anomaly where defining a property's `ReadOnlyAttribute` as false causes the property to attach to the Class `ReadonlyAttribute`. This caused a programmatic  change to property's `ReadOnlyAttribute` to modify the Class level `ReadOnlyAttribute`, and by default to modify all properties without a `ReadOnlyAttribute`.
- Fixed issue in Connection Dialog where a dummy blank database was not inserted if no Server Explorer or FlameRobin database existed for the selected DataSource/Server.


#### Tips
- None.



### v14.5.3.1001 Patch to fix ReadOnlyAttribute anomaly.

#### New / Enhancements
- Added __User Option__ `BlackbirdSQL Server Tools > Ddex Provider > Debug > Expected exceptions`. Enabling this option causes expected exceptions to be output to the __BlackbirdSql__ section of the VS output window for Release builds. 
#### Fixes
- Fixed anomaly where defining a property's `ReadOnlyAttribute` as false causes the property to attach to the Class `ReadonlyAttribute`. This caused a programmatic  change to property's `ReadOnlyAttribute` to modify the Class level `ReadOnlyAttribute`, and by default to modify all properties without a `ReadOnlyAttribute`.


#### Tips
- None.



### v14.5.3.1 Added User Option to output Expected Exceptions.

#### New / Enhancements
- Added __User Option__ `BlackbirdSQL Server Tools > Ddex Provider > Debug > Expected exceptions`. Enabling this option causes expected exceptions to be output to the __BlackbirdSql__ section of the VS output window for Release builds. 
#### Fixes
- Fixed possible bug where an attempt is made to open an open connection for a second time in the connection dialog.



### v14.5.3.0 Major update including Selective Intellisense.

#### New / Enhancements
- Replaced the `Enable Intellisense` SqlEditor user option with `Intellisense Policy`, which provides a radio button choice of `Active Query Only`, `All Queries` and `Disabled`. The default policy is `Active Query Only` which displays only the current active query's `Intellisense` messages. The option applies to only __BlackbirdSql__ SqlEditor queries. This eliminates the ambiguity caused by Intellisense messages when multiple query windows with the same or similar names are open.
- Implemented first phase of `EventSource` tracing and `EventCounter` telemetry. This replaces the legacy MS SqlServer `SqlTracer`, `TraceUtils` and Etw tracing and telemetry from the original Firebird port. Several User Options settings have been added to the diagnostics settings to cater for additional diagnostics filtering, which will be improved over time. All assemblies use the same BlackbirdSql EventSource provider base class, but have distinct static instances (all named `Evs`) by late binding to the base class using type parameters. This is simply to have only class declarative code in the final `Evs` types. 
- Added an n/nn (Statement n of Total nn) grid identifier to the upper left cell of grids for queries with multiple result sets.
- Streamlined the reindexing of project type resolution services so that only the extension's shipped EntityFramework assembly version and the applicable project's version are checked for reindexing.
- Added wrappers for all `Path` class methods that operate on file paths. The wrappers remove common invalid and non-printable characters from the path before calling the native `Path` method.
- Added the `ConnectionSource DataSource` to differentiate between DataSource and edmx Connection Dialogs.
- Code cleanup: Changed the prefix of all Ddex IVs interface implementations from the previous Borland `T` class type prefix convention to `Vxb` (VS Extension BlackbirdSql) because it conflicted with the Microsoft `T` prefix convention for __Type Parameter/Specifier__ names.
- Code cleanup: Removed the `GuidId` class because the built-in `CommandID` class is functionally equivalent.
- Code cleanup: Moved the __SplitNext__ and __SplitPrev__ built-in `VSStandardCommandSet97` commands into the `CommandMapper` as `IBsCommandHandler` type interfaces. This provides for a uniform system of handling global commands that is in line with the handling of internal extension commands.
- Code cleanup: Separated __Action__ `QueryManager` states as a subset of the new __Operation__ state in the state syncronicity stack, due to subtle differences in their behavior during push/pop stack operations. __Virtual__ states are cleared when the last __Action__ state is popped. When the __Operation__ state is popped, __Virtual__ states are only cleared if no __Action__ states exist in the stack. __Virtual__ states become volatile whenever an __Operation__ or __Action__ state is pushed onto the stack, as was their behavior previously. The __Operation__ state marker is set as the second to last enum marker in the `EnQueryState` enum.
- Code cleanup: Renamed the `Csb DatasetId` property to the more descriptive name `DatasetName`. The `DatasetName` is the unique short name of a `DbConnectionStringBuilder Database`.
#### Fixes
- Fixed bug where the FileDialog selection was not being correctly assigned in the Connection Dialog. Under certain conditions this also affected the ability to add the path manually.
- Fixed `IDSRefBuilder` functionality for database `ScalarFunctions`.
- Fixed issue where creating a new session connection within a query, with the __"Add new connections to Server Explorer"__ checkbox checked, was not creating a new Server Explorer connection.
- Fixed bug where a Server Explorer connection's events were not handled when the connection was added internally from a query window or Entity Data Model connection dialog.
- Fixed a bug where the ReadOnlyAttribute of `Csb` descriptors was not being correctly updated. The proposed `ConnectionName` and `DatasetName` advanced properties descriptors must be readonly within the Application settings connection dialog.

#### Tips
- Whenever a new Entity Data Model or DataSource connection is added using `Add Connection`, it must exist as a Server Explorer connection. You will be prompted to add it if it does not exist.
- Modifications to an existing Server Explorer connection from within the Query Session, DataSource or edmx connection dialog will permanently update the Server Explorer connection. You will be prompted before any modifications are applied.



### v14.5.1.1 Minor enhancements and code cleanup.

#### New / Enhancements
- Enabled the IDE SplitNext (F6) and SpitPrev (Shift-F6) global commands with AutoScroll. Grids in multiple resultset queries will automatically scroll into view when they receive focus.
- Moved the initial statement terminator (batch separator) user option to `SqlEditor > Query Execution > General` so that it now appears in the __Live Settings__ of a query. Setting the option within a query affects multi-statement script parsing only. 
- Enabled the `Commit` and `Rollback` SQL statements in SqlEditor scripts. These statements were previously disabled.
- Code cleanup: Converted calls to the `RdtManager` `ShowWindowFrame()` and `InvalidateToolbar()` methods to direct calls instead of using an event driven mechanism. This includes calls to the Async versions of these methods.
- Code cleanup: Converted all references to the `this` object in extension members to the name __@this__.
- Code cleanup: Converted all references to `string.Empty` to it's functionally equivalent constant `""`.
- Code cleanup: Moved all native db script parsing of server explorer nodes into a separate `IBsNativeDbServerExplorerService` service. This is to unclutter the `IBsNativeDatabaseEngine` service. Also moved the handling of native db error types into the `IBsNativeDbException` service.
- Code cleanup: Dropped the `IBsNativeDbCommand`, `IBsNativeDbConnection` and `IBsNativeDbConnectionWrapper` services, as equivalent functionality was already available in the remaining native database services.
#### Fixes
- Fixed issues with the IBsDbExceptionService. Database exceptions were themselves throwing exceptions when the exception were of type `IscException`. This caused error information loss when a critical Firebird connection error was displayed in the __Advanced Message Box__ detailed information popup. Isc exceptions are typically raised when when a connection fails due to a broken network connection or server shutdown. Also cofigured the service to recognize an `FbException` with error code `isc_net_write_err` as a critical error and not an SQL error.
- Removed second attempt to prevent a disconnect of connections that have been idle for 90+ minutes and have active transactions. Executing a mutating low-overhead `SELECT` statement on the database every 25 minutes fails to prevent the connection shutdown. The shutdown can be prevented by giving the connection a large `ConnectionLifetime`. The property has an upper limit of `int.MaxValue` seconds.
- Fixed issue where first statistics snapshot was being dropped.

#### Tips
- To manage your __BlackbirdSql SqlEditor__ Text Editor and Intellisense settings, navigate to the __Text Editor__ section in __User Options__. There you will find the settings for all text editors, including the BlackbirdSql editor, which is located under the __FB-SQL__ language sub-menu. All other settings are located under the __BlackbirdSQL Server Tools__ User Options section.
- You can change the terminator as many times as you require within a multi-statement SQL script. It is not necessary to reset it to it's original initial value at the end of your script. As soon as you re-execute the query the terminator is reset to it's value in __User Options__ or the query's __Live (Transient) Settings__.
- The Visual Studio **_F6_** (`SplitNext`) and **_Shift-F6_** (`SplitPrev`) global commands navigate between tabbed panes within a document window pane. If you have a batch query with multiple result sets, the results tabbed pane will contain multiple result grids. The **_F6_** and **_Shift-F6_** commands can be used to navigate to each grid within the results pane.



### v14.5.1.0 Minor enhancements and code cleanup.

#### New / Enhancements
- Enabled the IDE SplitNext (F6) and SpitPrev (Shift-F6) global commands with AutoScroll. Grids in multiple resultset queries will automatically scroll into view when they receive focus.
- Moved the initial statement terminator (batch separator) user option to `SqlEditor > Query Execution > General` so that it now appears in the __Live Settings__ of a query. Setting the option within a query affects multi-statement script parsing only. 
- Enabled the `Commit` and `Rollback` SQL statements in SqlEditor scripts. These statements were previously disabled.
- Code cleanup: Converted calls to the `RdtManager` `ShowWindowFrame()` and `InvalidateToolbar()` methods to direct calls instead of using an event driven mechanism. This includes calls to the Async versions of these methods.
- Code cleanup: Converted all references to the `this` object in extension members to the name __@this__.
- Code cleanup: Converted all references to `string.Empty` to it's functionally equivalent constant `""`.
- Code cleanup: Moved all native db script parsing of server explorer nodes into a separate `IBsNativeDbServerExplorerService` service. This is to unclutter the `IBsNativeDatabaseEngine` service. Also moved the handling of native db error types into the `IBsNativeDbException` service.
- Code cleanup: Dropped the `IBsNativeDbCommand`, `IBsNativeDbConnection` and `IBsNativeDbConnectionWrapper` services, as equivalent functionality was already available in the remaining native database services.
#### Fixes
- Fixed issues with the IBsDbExceptionService. Database exceptions were themselves throwing exceptions when the exception were of type `IscException`. This caused error information loss when a critical Firebird connection error was displayed in the __Advanced Message Box__ detailed information popup. Isc exceptions are typically raised when when a connection fails due to a broken network connection or server shutdown. Also cofigured the service to recognize an `FbException` with error code `isc_net_write_err` as a critical error and not an SQL error.
- Removed second attempt to prevent a disconnect of connections that have been idle for 90+ minutes and have active transactions. Executing a mutating low-overhead `SELECT` statement on the database every 25 minutes fails to prevent the connection shutdown. The shutdown can be prevented by giving the connection a large `ConnectionLifetime`. The property has an upper limit of `int.MaxValue` seconds.

__Tip:__ To manage your __BlackbirdSql SqlEditor__ Text Editor and Intellisense settings, navigate to the __Text Editor__ section in __User Options__. There you will find the settings for all text editors, including the BlackbirdSql editor, which is located under the __FB-SQL__ language sub-menu. All other settings are located under the __BlackbirdSQL Server Tools__ User Options section.
__Tip:__ You can change the terminator as many times as you require within a multi-statement SQL script. It is not necessary to reset it to it's original initial value at the end of your script. As soon as you re-execute the query the terminator is reset to it's value in __User Options__ or the query's __Live (Transient) Settings__.


### v14.5.0.3 Patch for Type A Type B project validity exception.

#### New / Enhancements
- None.
#### Fixes
- The __KeepAlive Monitor__ has been modified to execute a dummy Select command on active connections with transactions at intervals of approximately 25 minutes. The connection verification function was not preventing a FirebirdClient shutdown of a connection after a prolonged idle period using the previous method.
- Addressed issue where a `VsProject` type check on a `Project.Object` and a find on `Project.References`, to resolve assembly version mismatches, throws an exception.


### v14.5.0.2 Patch for Type A Type B project validity exception.

#### New / Enhancements
- None.
#### Fixes
- The __KeepAlive Monitor__ has been modified to execute a dummy Select command on active connections with transactions at intervals of approximately 25 minutes. The connection verification function was not preventing a FirebirdClient shutdown of a connection after a prolonged idle period using the previous method.
- Addressed issue where a `VsProject` type check on a `Project.Object`, to establish project validity, throws an exception.



### v14.5.0.1 Patch addressing minor issues.

#### New / Enhancements
- None.
#### Fixes
- The __KeepAlive Monitor__ has been modified to execute a dummy Select command on active connections with transactions at intervals of approximately 25 minutes. The connection verification function was not preventing a FirebirdClient shutdown of a connection after a prolonged idle period using the previous method.




### v14.5.0.0 Major update addressing major and minor issues.

#### New / Enhancements
- Added the __Query Execution Settings__ as a button on the query toolbar.
- Moved the __Initial TTS State__ user option from the `SqlEditor > Execution > General` model to the `SqlEditor > General` model. This is to prevent the setting from appearing in the Transient __Execution Settings__ options dialog, accessible from the context menu or toolbar of a query, where it would be redundant.
- Included the SqlEditor script parsing in the error handling of batch processing so that SQL script parsing errors are only reported as SQL errors in the Messages tab instead of appearing in the detailed exception message box. This applies to both single-statement and multi-statement scripts. There may still be cases where certain iSql commands will still appear in the detailed critical error message box. Connection failures will still appear in the error message box.
- Extensive ongoing code cleanup and comment documentation. Several types have also been renamed from their original MS SqlServer class names.
- Converted initial push propagation of user settings models to on-demand push propagation. Settings are now initially pushed at the latest possible when a setting is actually required.
- Ongoing removal of the default contructor syntax from classes to improve readability. 
#### Fixes
- Fixed bug introduced in version 14 that caused a new __Server Explorer__ or __Session__ connection that is based on a FlameRobin, Entity Data Model or Project settings connection, to create a duplicate connection with a numbered name, instead of taking ownership of the existing connection. 
- Resolved a longtime bug that caused __SqlEditor User Options__ pages to fail when there had been an early load of the options models. This was caused by cascaded extension/package assemblies that were intermittently not picked up in the IDE's `CurrentDomain`.
- Fixed bug that caused expansion of the full trigger list nodes to fail if full linkage was not completed. The linker was attempting to perform optimistic linking when full linkage was required.
- Reimplemented lazy loading of options models and the `SettingsManager` after clearing up the __SqlEditor User Options__ bug.
- Updated the Visual Studio extension installer manifest to the correct VS range of 17.10 - 18.0.



### v14.1.0.1 Patch to support FirebirdClient versions prior to 6.0.0.

#### New / Enhancements
- Implemented support for legacy projects using FirebirdClient and EntityFramework versions prior to 6.0.0.
#### Fixes
- None.



### v14.1.0.0 Residual VS Solution Merge peculiarities addressed.

Solution merges typically occur by default when a solution is directly opened from the `No Solution` context / environment, and you have open documents. These documents are not discarded but rather merged into the newly opened solution. There are no directly available events fired when the IDE transitions out of a `No Solution` context, so we need to ensure that the integrity of the extension's __Running Connection Table__, __Inflight Document Table__, __KeepAlive Monitor__, __Active Transactions__ and __Executing Queries__ is maintained during these transitions. 

#### New / Enhancements
- Executing queries will continue to execute during a solution merge and the subsequent opening of a solution.
- Open queries will maintain their active transactions, connection configurations and document states during a solution merge transition.
- Ensured the integrity of the `KeepAlive Monitor` is maintained when the `Server Explorer Connection Manager` is temporarily unavailable during solution merge transitions.
- Resolved issue where Visual Studio creates clone duplicates of query windows after a solution merge. The clones are removed immediately after being loaded.
- Implemented asynchronous db reader data retrieval on query `string` and `blob` grid column data types. This means that there is no syncronous blocking of the IDE during long-running multi-statement (batch) query execution.
#### Fixes
- The `CancellationToken` has been removed from all `DbDataReader.GetValueAsync<T>()` method calls. The FirebirdClient has a bug which causes it to temporarily crash on `CancellationToken.Cancelled()` when accessing blob types. 
- The database connection dropdown list in SqlEditor no longer gives the option to commit or rollback when there are active transactions. Prompting the user to commit / rollback / cancel after making a selection was susceptible to unavoidable deadlocks so it has been removed from that specific control.
- Resolved a bug where the `KeepAlive Monitor` was receiving an `Idle connection` response back from the `Query Manager` during execution, and could close a connection if it's `Lifetime` was set and was exceeded during a long running query.
- Resolved several glitches where invalidated variables were accessed during a solution closing or merging state.



### v14.0.0.3 Addressed minor issues. Version bump 2.

#### New / Enhancements
- Closing a cloned query with active transactions no longer prompts the user. The transaction status is maintained by the remaining clones. This applies to queries cloned through a solution merge or otherwise, and not cloned copies created using the `Clone to New Query` command, which creates a distinct independent copy.
- Improved user message prompt options on active transaction queries. The user is now provided with options when attempting to disconnect, modify the connection or run an estimated execution plan when transactions are still pending.
#### Fixes
- Resolved remaining instances where an Entity Data Model would throw an `InvalidCastException` with the message __An error occured while connecting to the datasbase... Type [A] cannot be cast to Type[B]__. This could occur when there were `FirebirdClient` or `EntityFramework` assembly version mismatches. The extension's versions will now be used in all cases within the design time `CurrentDomain`. This has no affect on your project builds, which will use the project's assemblies.
- Fixed a non-critical bug where a query window's keepalive monitor could abort during a solution merge if the SE is unavailable.
- Addressed issue where the extension's load statistics would be cleared from the output window if loaded too early. The extension's load statistics will only be displayed if the `Load statistics` option is enabled in `User Options`.

__Tip:__ The solution validation option available from the context menu of any Firebird Server Explorer node can validate and update the `app.config` of all projects with `EntityFramework.Firebird` assemblies installed, with the correct settings.



### v14.0.0.0 Critical update addressing IDE freeze.

#### New / Enhancements
- Added SQL snippet for a `SET TERM GO ;` `Surround with...` wrapper.
- The `Prompt to save` user option is now enabled by default. If the option is enabled, then on closing the user will, [1] be prompted to save any dirty persistent (saved) queries, [2] be prompted to cancel any running queries, and [3] be prompted to commit, rollback or cancel the closing process if there are any active transactions pending. If the option is disabled, running queries are cancelled, any changes lost and pending transactions automatically rolled back.
- Disposed of the entire `ConnectionInfo` / `PropertyAgent` class family in favor of inheriting directly from the `Csb` class, which already incorporates 95% of the functionality. Added the `ConnectionCsb`class, which introduces data connection functionality into the Csb, and it's descendent `ModelCsb` class, which handles the connection model for BlackbirdSql SqlEditor queries. This eliminates most of the excess redundant code from the original MS SqlServer port.
#### Fixes
- Fixed multiple issues with the handling of dirty documents, active transactions and running queries when the IDE is shut down, solutions merged, and solutions or projects closed.
- Fixed a critical intermittent deadlock caused by unreleased locks during thread switching which caused the IDE to hang.
- Fixed bug where EventProjectEnter() was using _EventRdtCardinal instead of _EventProjectCardinal, causing an `Attempt to exit project event when not in a project event` exception message.
- Fixed issue where `User Options` were not being propogated after a save and were only taking affect on IDE restart. Dynamic propogation of user settings have not been taking place since the inclusion of the `Connection Equivalency Keys` page in the DDEX  section over a year ago. Changes to the connection equivalency keys must only take affect after a restart, and this conditional logic was inverted. 
- Addressed a number of issues related to the identification of the `ConnectionSource` / `Connection Owner`.
- Fixed a long-time bug that caused a Server Explorer connection node to intermittently hang when expanded. This occured because the node's LinkageParser TaskHandler was attempting to access the Running Connection Table entry of the node before it had been registered.

__Important note on query TTS behavior:__ Whenever a BlackbirdSql SqlEditor query with active transactions is closed, the user will be prompted to commit, rollback or cancel if the `Prompt to save` user option is enabled, otherwise pending active transactions will automatically be rolled back.



### v13.1.0.3 Improved query moniker naming and caption glyph adornment.

#### New / Enhancements
- Session connections are now adorned with the clearer Large Solid Circle glyph ⬤ (unicode `\u2B24`).
- Query window captions of non-persistent (Session) queries (ie. queries that are not saved disk file queries) are now also adorned with a Session glyph, the Small Solid Circle ● (unicode `\u25CF`), to distinguish them from persistent queries. Session queries are disposed of whenever a solution is closed.
- Cloned queries are now always suffixed with a numeric `_999` unique identifier and adorned with a Session glyph. If it's parent is a persistent query and also has a suffix, an attempt will first be made to use the parent's suffixed moniker file name in the moniker for the new cloned Session query.
#### Fixes
- Fixed bug introduced with the batch script processing feature where DDL, Procedure and Function scripts did not have the necessary `SET TERM` wrappers. Affected scripts now use the `GO` terminator.
- In some cases, non-persistent query moniker file names were being duplicated. This made it difficult to distinguish between non-persistent (Session) query windows. Query windows launched from a Server Explorer node, and which are by default non-persistent (Session) queries, will still maintain their original, potentially non-unique, name.



### v13.1.0.2 Overhaul of connection strategy & bug fixes

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
- Fixed bug where a connection lifetime timeout caused the IDE to hang.
- Fixed bug where modifying an EDM connection in SqlEditor did not convert it to a Session connection.
- Fixed typo of MainTread to MainThread on the extension load statistics notification.


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
-- Resolved issue where closed connection nodes called the running connection table for updates when a connection was about to be modified. This caused proposed DatasetName's to be converted to proposed ConnectionNames and also initiated an unecessary trigger linkage that was then discarded.</br>
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
Prevented DatasetName glyph removal in Session connection dialogs when connection has not changed.

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
Upgraded the FirebirdClient to v10.0.0.0.

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