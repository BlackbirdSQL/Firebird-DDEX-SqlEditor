# BlackbirdSQL DDEX 2.0 .NET Data Provider for Firebird

(*This package is essentially complete but is still in pre-release until fully tested.*)

The BlackbirdSQL DDEX 2.0 .NET Data Provider tool, `BlackbirdSql.VisualStudio.Ddex`, implements all the core DDEX 2.0 interfaces prevalent in the SqlServer DDEX provider and more, but currently excludes DDL functionality. 

*If you're noticing a degradation in performance in the IDE after installing an extension then it's advantages are lost, so the first tenet of this package is `small footprint, low overhead`.*
See [Extended Description](#extended-description) below.

### Features
* Firebird DDEX provider support for most of the DDEX 2.0 IVs DML interfaces utilizing FirebirdSql.Data.FirebirdClient and EntityFramework.Firebird versions 9.1.1.
* Trigger/Generator auto-increment linkage.
* Plug and play. No configuration of the .csproj, app.config or machine.config files and no GAC registration.
* FlameRobin host and database selection within connection dialogs.
* Within Server Explorer, top level folders for Tables, Views, Stored procedures, Functions, Sequence Generators, Triggers and Domains.
* Within tables, drilldowns for indexes, foreign keys and triggers, and table columns, index columns, foreign key columns and trigger columns.
* Identification of Identity fields, Primary keys, Unique keys and Computed columns.
* System table, system index and system trigger enumeration within the SE and support for system tables within the xsd and edmx models.
* Edmx identity (auto-increment primary key) column and foreign key support.
* Procedures, functions, views, triggers and computed columns display the decoded blr if no source exists.
* Display of initial value (seed), increment and next value within sequence generator display.
* New query and data retrieval for both user and system tables.

### A Note on AutoIncrement
There is a simple parser coded in C++/Cli which parses the Trigger source for linkage to the auto-increment sequence generator. The original parser code was ported from the pgsql LISP con-cell parser but then scrapped in favor of the [greenlion/PHP-SQL-Parser](https://github.com/greenlion/PHP-SQL-Parser) PHP parser, which meant writing a class library, the Cell class, which could imitate PHP style arrays. The port was not completed because the partial port satisfied the needs for parsing the Trigger DDL. The parser itself is reasonable fast but SELECT commands for a large number of triggers and generators may take some time, so building of the Trigger/Generator linkage tables for a connection is initiated asynchronously as soon as the connection is established.


## Known issues
* The 'Open Script' command on Server Explorer nodes with an expression or dsl/sql source code is still in development and not functional yet. If you select the command from the SE context menu the IDE will shut down.
* If on startup of the Visual Studio IDE, and only on startup, an attempt is made to access a data object before the BlackbirdSql DDEX provider has been given the IDE shell context, Visual Studio will flag the provider as unavailable for the duration of the session. As it stands Visual Studio will have to be restarted to clear the flag.</br>
This is true for both the SE and any other data objects in a solution. In particular this issue can consistently be replicated in Server Explorer if a connection node for Firebird is open and the SE is pinned on startup of the IDE within a solution context.</br> 
Unless we're missing a trick here this seems to be unavoidable. Loading is asynchronous, so the provider needs time to register and load.</br>
We want to be as unobtrusive as possible so load delays are just a reality if we go the asynchronous route. (*Loading is initiated at the earliest possible, which is as soon as the IDE shell context is available.*)
* If you have a huge number of triggers then rendering of the triggers in the SE, or any other collection for that matter, may take some time. This may be normal behavior for the SE.
* If you use a Server Explorer Database Connection as the connection for an EDMX model and choose to exclude sensitive data there may be instances where the password will also be stripped from the SE connection and you will be prompted for a password through the BlackbirdSql IVsDataConnectionPromptDialog implementation the next time you access it in the SE.</br>
I have been unable to replicate this bug for some time now so it may have been resolved since updating the IVsDataConnectionPromptDialog logic.
* There seems to be an issue with drag and drop on procedures and functions which I haven't looked at. It's likely something trivial but not sure if this functionality is available to SqlServer so may be another rabbit hole.</br>
The same applies to drag and drop from the SE directly into the edmx, not available on SqlServer but I don't see why it cannot be done.
* The enhanced localized new query command is working but has not yet been placed in the views or functions/procedures nodes of the SE, which still use the built-in new query command.</br>
The BlackbirdSql new query command simply filters the table selection list (based on whether you initiated it from a System Table node context or User Table node context) and then passes it on to the native Visual Studio command.</br>
Refreshing the table selection list will include both System and User tables.


## Documentation

* [ADO.NET provider](ado-net.md)
* [Using ADO.NET edmx with .NET](edmx-NET.md)


## Packages

| NuGet | Version | Downloads |
|-------|---------|-----------|
| [FirebirdSql.Data.FireClient](https://www.nuget.org/packages/FirebirdSql.Data.FireClient) | ![FirebirdSql.Data.FireClient](https://img.shields.io/nuget/v/FirebirdSql.Data.FireClient.svg) | ![FirebirdSql.Data.FireClient](https://img.shields.io/nuget/dt/FirebirdSql.Data.FireClient.svg) |
| [EntityFramework.Firebird](https://www.nuget.org/packages/EntityFramework.Firebird) | ![EntityFramework.Firebird](https://img.shields.io/nuget/v/EntityFramework.Firebird.svg) | ![EntityFramework.Firebird](https://img.shields.io/nuget/dt/EntityFramework.Firebird.svg) |
| [FirebirdSql.EntityFrameworkCore.Firebird](https://www.nuget.org/packages/FirebirdSql.EntityFrameworkCore.Firebird) | ![FirebirdSql.EntityFrameworkCore.Firebird](https://img.shields.io/nuget/v/FirebirdSql.EntityFrameworkCore.Firebird.svg) | ![FirebirdSql.EntityFrameworkCore.Firebird](https://img.shields.io/nuget/dt/FirebirdSql.EntityFrameworkCore.Firebird.svg) |


## Resources

* [Downloads](https://github.com/BlackbirdSQL/NETProvider-DDEX/releases)
* [Issue tracker](https://github.com/BlackbirdSQL/NETProvider-DDEX/issues)


## Builds

If you're planning on testing this solution (Blackbird.sln) preferably DO NOT use test projects within Blackbird.sln.</br>
Rather fire up the experimental instance of Visual Studio with Blackbird.sln remaining open and test within the experimental instance. If you have successfully built Blackbird.sln you won't need to install the vsix. VS will automatically detect it in the experimental instance.</br>
To fire up an experimental instance of Visual Studio create a shortcut of Visual Studio with the experimental suffix. eg. `"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\devenv.exe" /RootSuffix Exp`

## Misc

BlackbirdSql.Data.DslClient.dll, EntityFramework.BlackbirdSql.dll and BlackbirdSql.EntityFrameworkCore.dll (Blackbird[Experimental]) were for initial debugging purposes only in order to have full control over the client and modify it where necessary.


## Extended Description

Although the original DDEX 1.0 DataTools package, *BlackbirdSql.VisualStudio.DataTools* was revived, the DDEX 2.0 package, BlackbirdSql.VisualStudio.Ddex is click and go using VSIX and autoload, and requires no additional setup either in the app.config, csproj or machine.config.</br>
The  prupose of *BlackbirdSql.VisualStudio.DataTools* was only to get DDEX functioning for Firebird before implementing DDEX 2.0, so this tool can be considered obsolete.

If the option is enabled there will be a once-off validation of a solution's projects' app.config's and edmx models. Legacy edmx models won't work with Firebird's latest EntityFramework version so an update is required.</br>
This is a once off validation on each `existing` solution the first time it is opened after installing the VSIX. If the app.config is open or any edmx models are open you will need to close them first and then reopen your solution for the once-off validation to complete.

If the option is enabled and you add Firebird.Data.FirebirdClient or EntityFramework.Firebird to a project it will be validated and the app.config updated correctly if required. If the app.config is open the update will be skipped and you will need to reopen your solution for the validation to complete.

__Note:__ For the debug build the once-off validation flags are not persistent between loads of solutions and are repeated.

The intention is to maintain a small footprint. We're not going to start altering VS menus and taking over your Visual Studio IDE workspace. It is a data source UI provider for Firebird and the benchmark is the SqlServer provider, so whatever UI functionality is available for SqlServer is on the todo list for Firebird provided it does not directly interfere with the developer's active UI.

The goal is that you don't have to do any configuring of the .csproj, app.config, machine.config or any legacy edmx models, and eliminate using the GAC.</br>
The once-off validation features can be disabled in the Visual Studio options but each individual task in the validation process is spawned asynchronously so the overhead is miniscual.

If you're planning on using EF Core and/or .NET, VS does not have wizard support for edmx which makes no sense to me.
This roadblock is easily overcome by creating a separate project using .NET Framework for your data models and then linking your .NET / EF Core projects to those edmx models.

If there's any magic you feel should be included here, you're welcome to pop me a mail.</br>
On the priority list are DDL commands... Create, Alter etc.
