# ADO.NET

### Setup procedure

* Install the DDEX extension <span style="color:#d7ba7d">BlackbirdSql.VisualStudio.Ddex.vsix</span>. </br>*We're still under development so install the debug version located in the output folder <span style="color:#d7ba7d">BlackbirdSql.VisualStudio.Ddex\bin\Debug</span>.*
* Create a dataset throught the xsd wizard or an entity data model through the edmx wizard.
<p style="font-size:1.1em;color:#4080d0;margin-bottom:-8px">For an xsd dataset:</p>

* You can add your Firebird database to the server explorer (SE) (*or add a Settings connection string*) using the <span style="color:#d7ba7d">Firebird SQL Server</span> data source and selecting the <span style="color:#d7ba7d">BlackbirdSql DDEX 2.0</span> provider.</br>Using the SE is the easiest. From there you can just drag and drop your Firebird database entities onto the xsd.</br>
*(A reference to the <span style="color:#d7ba7d">BlackbirdSql.Data.DslClient.dll</span> assembly should automatically be added whenever you create a connection using the BlackbirdSql DDEX 2.0 provider.*
<p style="font-size:1.1em;color:#4080d0;margin-bottom:-8px">For an ADO.NET edmx:</p>

* Go through the steps in [Entity Framework 6 provider](entity-framework-6.md) to set up EntityFramework 6 for your application
* Add the edmx through the wizard using the <span style="color:#d7ba7d">EF designer from database</span> option as you normally would, again connecting through the <span style="color:#d7ba7d">BlackbirdSql DDEX 2.0</span> provider.
* With your edmx open select `Project > Add new data source > Object` to complete the setup of your model.