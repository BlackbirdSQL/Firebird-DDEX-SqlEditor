# BlackbirdSQL DDEX 2.0 with SqlEditor Provider for Firebird 9.1.0.91 Beta

#### Screenshots
![ReadMe](https://github.com/BlackbirdSQL/Firebird-DDEX-SqlEditor/assets/120905720/0b52cc20-c957-451d-93c0-31255abb0ada)
</br>`Click on image to view fullscreen`
</br></br>


The BlackbirdSQL DDEX 2.0 .NET Data with SqlEditor Provider extension, `BlackbirdSql.VisualStudio.Ddex`, implements most DDEX 2.0 interfaces prevalent in the SqlServer DDEX and SqlEditor extensions.

[Download BlackbirdSql DDEX with SqlEditor Extension (Pre-release v9.1.0.84-beta)](https://github.com/BlackbirdSQL/Firebird-DDEX-SqlEditor/releases/download/v9.1.0.84-beta/BlackbirdSql.VisualStudio.Ddex.vsix)

*The first tenet of this package is `small footprint, low overhead`, and to be as unobtrusive as possible. It is installed as a standard VSIX extension. If you uninstall it is is gone. It does not leave it's fingerprints in either your computer system or your Visual Studio installation.*

### Features
* Firebird DDEX provider support for most of the DDEX 2.0 IVs DML interfaces utilizing FirebirdSql.Data.FirebirdClient version 9.1.1.
* SqlServer SqlEditor port for Firebird for editing Computed columns, Triggers, Views, Procedures, Functions and SQL scripts.</br>__Note:__ The editor service execution plan visualizer is not currently functional. Execution plans are text based.
* Trigger/Generator auto-increment linkage.
* FlameRobin host and database selection within connection dialogs.
* SqlEditor text-based execution plans and statistics snapshot comparer.
* Within Server Explorer, top level folders for Tables, Views, Stored procedures, Functions, Sequence Generators, Triggers and Domains.
* Within tables, drilldowns for indexes, foreign keys and triggers, and table columns, index columns, foreign key columns and trigger columns.
* Identification of Identity fields, Primary keys, Unique keys and Computed columns.
* System table, system index and system trigger enumeration within the SE and support for system tables within the xsd and edmx models.
* Edmx identity (auto-increment primary key) column and foreign key support.
* Procedures, functions, views, triggers and computed columns display the decoded blr if no source exists.
* Display of initial value (seed), increment and next value within sequence generator display.
* New query and data retrieval for both user and system tables.
* Plug and play. No configuration of the .csproj, app.config or machine.config files and no GAC registration.
* BlackbirdSql background and UI thread tasks compliant with the IDE TaskHandler and implements the user cancel feature for background tasks from the TaskHandler window.
* All exception, task progress and task status reporting logged to the output window accessible under *BlackbirdSql* in the dropdown (Enabled by default under Options).
* The connection node `Refresh` command option in the SE will successfully recover from a connection timeout shutdown exception, to the node's previous state

### AutoIncrement Identity Fields
There is a simple parser coded in C++/Cli which parses the Trigger source for linkage to the auto-increment sequence generator. The original parser code was ported from the pgsql LISP con-cell parser but then scrapped in favor of the [greenlion/PHP-SQL-Parser](https://github.com/greenlion/PHP-SQL-Parser) PHP parser, which meant adapting the BlackbirdSql Cell class library so that it could imitate PHP style arrays/variables. This library is fully functional but the port of the parser itself was not completed because the partial port satisfied the needs for parsing the Trigger DDL.</br>
The parser itself is reasonably fast (+- 0.1 milliseconds per trigger), but SQL SELECT commands on a large number of rows, in this case for triggers and generators, may take some time, so building of the Trigger/Generator linkage tables for a connection is initiated asynchronously as soon as the connection is established.


## Known issues
* The Language service for the SqlEditor service is still under development and has not been linked into the extension. When opening scripts for Triggers, Views, Procedures, Funtions, Computed columns or SQL statements, the SqlEditor uses the Visual Studio built-in T-SQL Language service. This means that Intellisense may mark incompatible SQL and DDL as errors. The scripts will still successfully execute.
* The SqlEditor port does not currently support script parameter loading feature.
* The BlackbirdSql Editor settings in Visual Studio Options has been ported as is from the Microsoft SqlServer SqlEditor settings. This means that many of the options are not currently being used or are not applicable.
* If on startup of the Visual Studio IDE, and only on startup, an attempt is made to access an EDMX model or a Database in the SE before the associated DDEX provider has been given the IDE shell context, Visual Studio will flag the provider as unavailable for the duration of the session. This is true for both the SqlServer and BlackbirdSql providers, and is likely the case for any other DDEX provider that loads asynchronously.</br>
As it stands Visual Studio will have to be restarted to clear the flag.</br>
Unless we're missing a trick here this seems to be unavoidable. Loading is asynchronous, so the provider needs time to register and load.</br>
We want to be as unobtrusive as possible so load delays are just a reality if we go the asynchronous route. (*Loading is initiated at the earliest possible, which is as soon as the IDE shell context is available.*)
* If you have a huge number of triggers then rendering of the triggers in the SE, or any other collection for that matter, may take some time. This has nothing to do with the parser but is simply down to network and database server performance. To minimize the effect of this, Trigger/Generator linkage is built asynchronously as soon as a connection is established.
* There seems to be an issue with drag and drop on procedures and functions which I haven't looked at. It's likely something trivial but not sure if this functionality is available to SqlServer so may be another rabbit hole.</br>
The same applies to drag and drop from the SE directly into the edmx, not available on SqlServer but I don't see why it cannot be done.
* The enhanced localized new query command is working but has not yet been placed in the views or functions/procedures nodes of the SE, which still use the built-in new query command.</br>
The BlackbirdSql new query command simply filters the table selection list (based on whether you initiated it from a System Table node context or User Table node context) and then passes it on to the native Visual Studio command.</br>
Refreshing the table selection list will include both System and User tables.


## Documentation

* [ADO.NET provider](ado-net.md)
* [Using ADO.NET edmx with .NET](edmx-NET.md)


## FirebirdSQL Packages
BlackbirdSql utilizes the [FirebirdSQL/NETProvider](https://github.com/FirebirdSQL/NETProvider) FirebirdSql.Data.FirebirdClient and EntityFramework.Firebird packages for client access to Firebird. The package source is included in the BlackbirdSql source for debug and tracing purposes, however BlackbirdSql is not associated with FirebirdSql in any form and the Firebird source will be removed once testing is complete. Links to the FirebirdSql nuget packages are provided below.

| NuGet | Version | Downloads |
|-------|---------|-----------|
| [FirebirdSql.Data.FireClient](https://www.nuget.org/packages/FirebirdSql.Data.FirebirdClient) | ![FirebirdSql.Data.FireClient](https://img.shields.io/nuget/v/FirebirdSql.Data.FirebirdClient.svg) | ![FirebirdSql.Data.FireClient](https://img.shields.io/nuget/dt/FirebirdSql.Data.FirebirdClient.svg) |
| [EntityFramework.Firebird](https://www.nuget.org/packages/EntityFramework.Firebird) | ![EntityFramework.Firebird](https://img.shields.io/nuget/v/EntityFramework.Firebird.svg) | ![EntityFramework.Firebird](https://img.shields.io/nuget/dt/EntityFramework.Firebird.svg) |
| [FirebirdSql.EntityFrameworkCore.Firebird](https://www.nuget.org/packages/FirebirdSql.EntityFrameworkCore.Firebird) | ![FirebirdSql.EntityFrameworkCore.Firebird](https://img.shields.io/nuget/v/FirebirdSql.EntityFrameworkCore.Firebird.svg) | ![FirebirdSql.EntityFrameworkCore.Firebird](https://img.shields.io/nuget/dt/FirebirdSql.EntityFrameworkCore.Firebird.svg) |


## Resources

* [Downloads](https://github.com/BlackbirdSQL/NETProvider-DDEX/releases)
* [Issue tracker](https://github.com/BlackbirdSQL/NETProvider-DDEX/issues)


## Builds

If you're planning on testing this solution (Blackbird.sln) preferably DO NOT use test projects within Blackbird.sln.</br>
Rather fire up the experimental instance of Visual Studio with Blackbird.sln remaining open and test within the experimental instance. If you have successfully built Blackbird.sln you won't need to install the vsix. VS will automatically detect it in the experimental instance.</br>
To fire up an experimental instance of Visual Studio create a shortcut of Visual Studio with the experimental suffix. eg. `"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\devenv.exe" /RootSuffix Exp`

## Extended Description

BlackbirdSql.VisualStudio.Ddex is DDEX 2.0 compliant and is click and go using VSIX and autoload, and requires no additional setup either in the app.config, csproj or machine.config.</br>

If enabled (the default), there will be a once-off validation of a solution's projects' app.config's and edmx models. Legacy edmx models won't work with Firebird's latest EntityFramework version so an update is required.</br>
This is a once off validation on each `existing` solution the first time it is opened after installing the VSIX.
The goal is that you don't have to do any configuring of the .csproj, app.config, machine.config or any legacy edmx models, and eliminate using the GAC.</br>
This feature can be disabled in the Visual Studio options, but each individual task in the validation process is spawned asynchronously so the overhead is miniscual.</br>

`In performance tests on a 64-bit Windows 10 I7-4500U (1.80GHz) machine with 16GB RAM and a 1TB SSD, a solution with 40MB of source code, and that included 6 projects with 3 EDMX models and 3 XSD models, validated in under 300ms on a low-priority background task.`

The validation process will not validate any open app.config or edmx models. You will need to close them first and then reopen your solution for the once-off validation to complete.</br>

If the validation option is enabled (the default) and you add Firebird.Data.FirebirdClient or EntityFramework.Firebird to a project, the project will be validated and the app.config updated correctly if required. If the app.config is open the update will be skipped and you will need to reopen your solution for the validation to complete.

__Note:__ If you wish to clear the peristent flag for a solution, you can set `Persistent Flags` to false under the IDE BlackbirdSql Debug options.

The intention is to maintain a small footprint. We're not going to begin altering VS menus and taking over your Visual Studio IDE workspace. It is a data source UI provider for Firebird and the benchmark is the SqlServer provider, so whatever UI functionality is available for SqlServer is on the todo list for Firebird provided it does not directly interfere with the developer's active UI.


If you're planning on using EF Core and/or .NET, VS does not have wizard support for edmx which makes no sense to me.
This roadblock is easily overcome by creating a separate project using .NET Framework for your data models and then linking your .NET / EF Core projects to those edmx models.

If there's any magic you feel should be included here, you're welcome to pop me a mail at greg@blackbirdsql.com.</br>
On the priority list are DDL commands... Create, Alter etc.
