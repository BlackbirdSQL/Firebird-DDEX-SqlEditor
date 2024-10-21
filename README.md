# BlackbirdSQL DDEX 2.0 with SqlEditor Provider for Firebird

The Ultimate Firebird DDEX 2.0 Provider and SqlEditor with the *"look and feel"* of Microsoft's SqlServer DDEX and SqlEditor extensions.

[Download BlackbirdSql DDEX with SqlEditor Extension (Release v14.5.3.0)](https://raw.githubusercontent.com/BlackbirdSQL/Firebird-DDEX-SqlEditor/master/BlackbirdSql.VisualStudio.Ddex/bin/Release/BlackbirdSql.VisualStudio.Ddex.vsix) ([Change Log](https://github.com/BlackbirdSQL/Firebird-DDEX-SqlEditor/blob/master/Docs/CHANGELOG.md))

#### Screenshots
![ReadMe](https://github.com/BlackbirdSQL/Firebird-DDEX-SqlEditor/assets/120905720/e22c80d4-56d9-4982-ac17-15a7a73eef76)
</br>`Click on image to view fullscreen`
</br></br>


### Features
* Firebird DDEX provider support for Visual Studio's DDEX 2.0 IVs DML interfaces utilizing FirebirdSql.Data.FirebirdClient version 10.3.1.0. 
* Microsoft SqlServer SqlEditor port for Firebird for editing Computed columns, Triggers, Views, Procedures, Functions, SQL scripts and `.fbsql` files.
</br>__Note:__ The Firebird Language Service is a phased implementation of Microsoft's Transaction-SQL SSDT language service. Intellisense may report Firebird specific grammar as errors or warnings but still successfully execute.
</br>__New:__ Full support for multi-statement SQL scripts and iSql.
* Trigger/Generator auto-increment linkage. Linkage can be set to *On Demand* in user options. If enabled linkage will only take place when actually required. 
* Full integration of Server Explorer with asynchronous loading. Connection dialogs include Host and Database drop-down selection; derived from Server Explorer, FlameRobin and the current solution projects' settings and EDM connection strings.
* SqlEditor text-based execution plans and statistics snapshot comparer.
* Configurable connection equivalency keys under `BlackbirdSql Server Tools` DDEX user options.
* Within Server Explorer, top level folders for Tables, Views, Stored procedures, Functions, Sequence Generators, Triggers and Domains.
* Within tables, drilldowns for indexes, foreign keys and triggers, and table columns, index columns, foreign key columns and trigger columns.
* Identification of Identity fields, Primary keys, Unique keys and Computed columns.
* System table, system index and system trigger enumeration within the SE and support for system tables within the xsd and edmx models.
* Edmx identity (auto-increment primary key) column and foreign key support.
* Procedures, functions, views, triggers and computed columns display the decoded blr if no source exists.
* Display of initial value (seed), increment and next value within column primary key, trigger and sequence generator property windows.
* New query and data retrieval for both user and system tables.
* Local database connection drift detection across all connections in use within an IDE session. This applies to `local` drift and does not detect remote database drift.
* Automated DbProviderFactories and EDM validation and update of a Solution's App.Config, and upgrade of EDMX models to DDEX 2.0. This procedure can be launched from the context menu of any Firebird node in Server Explorer.
* Plug and play. No configuration of the .csproj, app.config or machine.config files and no GAC registration.
* BlackbirdSql background and UI thread tasks compliant with the IDE TaskHandler and implements the user cancel feature for background tasks from the TaskHandler window.
* All exception, task progress and task status reporting logged to the output window accessible under *BlackbirdSql* in the dropdown (Enabled by default under Options).
* The connection node `Refresh` command option in the SE will successfully recover from a connection timeout shutdown exception, to the node's previous state.


### AutoIncrement Identity Fields
There is a simple parser coded in C++/Cli which parses the Trigger source for linkage to the auto-increment sequence generator. The original parser code was ported from the pgsql LISP con-cell parser but then scrapped in favor of the [greenlion/PHP-SQL-Parser](https://github.com/greenlion/PHP-SQL-Parser) PHP parser, which meant adapting the BlackbirdSql Cell class library so that it could imitate PHP style arrays/variables. This library is fully functional but the port of the parser itself was not completed because the partial port satisfied the needs for parsing the Trigger DDL.</br>
The parser itself is reasonably fast (+- 0.1 milliseconds per trigger), but SQL SELECT commands on a large number of rows, in this case for triggers and generators, may take some time, so building of the Trigger/Generator linkage tables for a connection is initiated asynchronously as soon as the connection is established.
</br></br>

__Disclaimer/Warning__ regarding exposing members with hidden access modifiers. The private edit field of the VS User Options PropertyGrid GridView is accessed to overcome a long-time irritant and implement radio buttons and check boxes into the grid by utilizing type converters and attributes. There are several other locations where hidden members are also exposed.</br>
All access of this nature takes place in the BlackbirdSql.Sys.Reflect class. As a rule no other code within the extension may expose hidden members. The code performing this access is 100% stable, using standard calls included in Visual Studio's Reflection. If you object to this practice, do not install this extension.</br></br>
*The first tenet of this package is `small footprint, low overhead`, and to be as unobtrusive as possible. It is installed as a standard VSIX extension. If you uninstall it is is gone. It does not leave it's fingerprints in either your computer system or your Visual Studio installation.*
</br></br>

### Deconstructing connection naming, equivalency and SE integration
For a clearer understanding of BlackbirdSql's RunningConnectionTable management of connections, insight into it's basic rules of operation can be found [here...](GettingStartedGuide.md)
</br></br>

## Known issues and limitations
* Operations within the EDMX UI can take some time. For even a single table the wizard executes over 100 SELECT statements with the primary SELECT statement having 20+ JOINS and 5+ UNIONS. Even a Cancel request can lock up the IDE for some time. Be patient.
* Intellisense and Firebird grammar: Development of the Firebird Language Service is ongoing and support for Firebird grammar will be progressively extended over time. The Firebird Language Service uses the Visual Studio built-in T-SQL SSDT Language service as it's basis. This means that Intellisense may mark incompatible DML and DDL as errors. The scripts will still successfully execute.
* Support for embedded databases: BlackbirdSql uses the FirebirdSql.Data.FirebirdClient client, so embedded databases 'should' work, however no testing has been performed on embedded databases as of this writing.
* The SqlEditor port does not currently support script parameter loading.
* The BlackbirdSql Editor settings in Visual Studio Options have been ported as is from the Microsoft SqlServer SqlEditor settings. This means that several options are not currently being used or are not applicable.
* If you have a huge number of triggers then rendering of the triggers in the SE, or any other collection for that matter, may take some time. This has nothing to do with the parser but is simply down to network and database server performance. To minimize the effect of this, Trigger/Generator linkage is built asynchronously as soon as a connection is established.
</br>

## Documentation

* [ADO.NET provider](ado-net.md)
* [Using ADO.NET edmx with .NET](edmx-NET.md)
</br>

## FirebirdSQL Packages
BlackbirdSql utilizes the [FirebirdSQL/NETProvider](https://github.com/FirebirdSQL/NETProvider) FirebirdSql.Data.FirebirdClient package for client access to Firebird. The package source is included in the BlackbirdSql source for debug and tracing purposes, however BlackbirdSql is not associated with FirebirdSql in any form and the Firebird source will be removed once testing is complete. Links to the FirebirdSql nuget packages are provided below.

| NuGet | Version | Downloads |
|-------|---------|-----------|
| [FirebirdSql.Data.FireClient](https://www.nuget.org/packages/FirebirdSql.Data.FirebirdClient) | ![FirebirdSql.Data.FireClient](https://img.shields.io/nuget/v/FirebirdSql.Data.FirebirdClient.svg) | ![FirebirdSql.Data.FireClient](https://img.shields.io/nuget/dt/FirebirdSql.Data.FirebirdClient.svg) |
| [EntityFramework.Firebird](https://www.nuget.org/packages/EntityFramework.Firebird) | ![EntityFramework.Firebird](https://img.shields.io/nuget/v/EntityFramework.Firebird.svg) | ![EntityFramework.Firebird](https://img.shields.io/nuget/dt/EntityFramework.Firebird.svg) |
| [FirebirdSql.EntityFrameworkCore.Firebird](https://www.nuget.org/packages/FirebirdSql.EntityFrameworkCore.Firebird) | ![FirebirdSql.EntityFrameworkCore.Firebird](https://img.shields.io/nuget/v/FirebirdSql.EntityFrameworkCore.Firebird.svg) | ![FirebirdSql.EntityFrameworkCore.Firebird](https://img.shields.io/nuget/dt/FirebirdSql.EntityFrameworkCore.Firebird.svg) |
</br>

## Resources

* [Downloads](https://github.com/BlackbirdSQL/NETProvider-DDEX/releases)
* [Issue tracker](https://github.com/BlackbirdSQL/NETProvider-DDEX/issues)
</br>

## Builds

If you're planning on testing this solution (Blackbird.sln) preferably DO NOT use test projects within Blackbird.sln.</br>
Rather fire up the experimental instance of Visual Studio with Blackbird.sln remaining open and test within the experimental instance. If you have successfully built Blackbird.sln you won't need to install the vsix. VS will automatically detect it in the experimental instance.</br>
To fire up an experimental instance of Visual Studio outside of the IDE, create a shortcut of Visual Studio with the experimental suffix. eg. `"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\devenv.exe" /RootSuffix Exp`
</br></br>

## Extended Description

BlackbirdSql.VisualStudio.Ddex is DDEX 2.0 compliant and is click and go using VSIX and autoload, and requires no additional setup either in the app.config, csproj or machine.config.</br>

There is an option available in the context menu of any sited node of Server Explorer to perform a validation of a solution's projects' app.config and edmx models. Legacy edmx models won't work with DDEX 2.0 so an update is required. An edmx model can also be manually updated by clicking on an open model and setting `Use Legacy Provider' to false in the properties window.</br>
The goal is that you don't have to do any configuring of the .csproj, app.config, machine.config or any legacy edmx models, and eliminate using the GAC.</br>
Each individual task in the validation process is spawned asynchronously so the overhead is miniscual.</br>

`In performance tests on a 64-bit Windows 10 I7-4500U (1.80GHz) machine with 16GB RAM and a 1TB SSD, a solution with 40MB of source code, and that included 6 projects with 3 EDMX models and 3 XSD models, validated in under 300ms on a low-priority background task.`

The validation process will not validate any open app.config or edmx models. You will need to close them first and then rerun the validation process.</br>

The intention is to maintain a small footprint. With the exception of the IDE Options menu, we are not going to begin altering VS menus and taking over your Visual Studio IDE workspace. It is a data source UI provider for Firebird and the benchmark is the SqlServer provider, so whatever UI functionality is available for SqlServer is on the todo list for Firebird provided it does not directly interfere with the developer's active UI.


If you're planning on using EF Core and/or .NET, VS does not have wizard support for edmx which makes no sense to me.
This roadblock is easily overcome by creating a separate project using .NET Framework for your data models and then linking your .NET / EF Core projects to those edmx models.

If there's any magic you feel should be included here, you're welcome to pop me a mail at greg@blackbirdsql.com.</br>
On the priority list are DDL commands... Create, Alter etc.
