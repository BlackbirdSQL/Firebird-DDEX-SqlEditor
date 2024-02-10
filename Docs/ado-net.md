# ADO.NET

### Setup procedure

* Install the DDEX extension `BlackbirdSql.VisualStudio.Ddex.vsix`. </br>*We're still under development so install the debug version located in the output folder `BlackbirdSql.VisualStudio.Ddex\bin\Debug`.*
* On opening a solution for the first time after installation ensure none of your app.config files or edmx models are open in the IDE. If any are the DDEX tool will skip over them during validation, so close them and then reopen your solution.
* Add the applicable Nuget package if you haven't already.
* Rebuild your Project. It should now be able to access any existing client connections, xsd datasets and edmx models you have.
* Create a dataset throught the xsd wizard or an entity data model through the edmx wizard.
<p style="font-size:1.1em;margin-bottom:-8px">For an xsd dataset:</p>

* Install the Nuget FirebirdSql.Data.FirebirdClient package if you haven't already and `rebuild` your project.
* You can add your Firebird database to the server explorer (SE) (*or add a Settings connection string*) using the `Firebird` data source and selecting the `BlackbirdSql DDEX 2.0` provider.</br>Using the SE is the easiest. From there you can just drag and drop your Firebird database entities onto the xsd.

<p style="font-size:1.1em;margin-bottom:-8px">For an ADO.NET edmx:</p>

* Install the Nuget EntityFramework.Firebird package if you haven't already and `rebuild` your project. (FirebirdSql.Data.FirebirdClient will be auto installed as a dependency if it's not already installed.)
* Open an existing edmx model. It should work right off the bat, or add a new edmx model through the wizard using the `EF designer from database` option as you normally would, again connecting through the `BlackbirdSql DDEX 2.0` provider.
* Add your Firebird database entities to the model as required.
* Rebuild your project.
* For a WPF project follow the Microsoft tutorial [WPF and Entity Framework 6](https://learn.microsoft.com/en-us/visualstudio/data-tools/create-a-simple-data-application-with-wpf-and-entity-framework-6?view=vs-2022) to complete the setup of your model entities.

__Warning__
* Operations within the edmx UI can take some time. For even a single table the wizard executes over 100 SELECT statements with the primary SELECT statement having 20+ JOINS and 5+ UNIONS. Even a Cancel request can lock up the IDE for some time. Be patient.</br>
* BlackbirdSql.VisualStudio.Ddex.dll is used by the IDE. Do not add a reference to it in your projects.
* If you add a reference or package for Firebird.Data.FirebirdClient or EntityFramework.Firebird to a project and the app.config is open it will not be configured.</br>You will need to reopen your solution for it to be updated.
