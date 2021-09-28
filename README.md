# FluentMigrator.MigrationGenerator

[FluentMigrator](https://github.com/schambers/fluentmigrator) is a SQL migration framework designed to help version an application's database. This package allows a developer to quickly create a new migration from within Visual Studio's Tool Window. 

A few notable features:

- Timestamp generation
- Migration file named correctly with timestamp
- Migration added to `Migrations` project under current active solution

## Getting Started

Build the project then install DatabaseMigration.vsix as an extension.

Once installed, open the `View > Other Windows > Database Migration Window` in Visual Studio.

In the new window, choose migration project and operation.

You should see the following structure in the `Migration` project.

```
Migration Project
|- /Year
    |- /Month
        |- Mig20160219141436_{Operation}{Db ObjectName}.cs
```

The migration file contents should look like the following.

```csharp
using FluentMigrator;

namespace Database.Migrations._2021._09
{
    [Migration(202109281448)]
    public class Mig202109281448_UpdateTableTest : Migration
    {
        public override void Up()
        {
        }

        public override void Down()
        {
        }
    }
}
```

Fill in the migration appropriately.
