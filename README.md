# EntityFramework.DiscriminatorIndex
Help us to create index on discriminator columns by generating migration index operations (create, rename and drop).

## Setup

Migration Configuration file:

```c#
internal sealed class Configuration : DbMigrationsConfiguration<DbContext>
{
    public Configuration()
    {
        this.AddSupportToDiscriminatorIndex();
    }
}
```

DbConfiguration: 

```c#
public class DbConfiguration : System.Data.Entity.DbConfiguration
{
    public DbConfiguration()
    {
        SetMetadataAnnotationSerializer(
            DiscriminatorIndexAnnotation.AnnotationName,
            () => new DiscriminatorIndexAnnotationSerializer());
    }
}
```

##How To Use?

### Entity Type Configuration 

To configure the index for a type, use the extention method `HasIndexOnDiscriminator()`  of the `EntityTypeConfiguration`:

```c#
public class DbContext: System.Data.Entity.DbContext
{
    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MyEntity>().HasIndexOnDiscriminator();
        base.OnModelCreating(modelBuilder);
    }
}
```

Optionally, we can customize inform the column name, and also set the index name.

```c#
modelBuilder.Entity<MyEntity>().HasIndexOnDiscriminator(
    columnName: "MyDiscriminator", 
    indexName: "IX_Entity_Discriminator");
```

### Convention

Using the convention, all columns named as "Discriminator" in the database will be indexed.

```c#
public class DbContext: System.Data.Entity.DbContext
{
    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        modelBuilder.Conventions.Add<DiscriminatorIndexConvention>();
        base.OnModelCreating(modelBuilder);
    }
}
```

Optionally, we can provide an alternative column name to search for:

```c#
modelBuilder.Conventions.Add(new DiscriminatorIndexConvention(discriminatorColumnName: "CustoDiscriminator"));
```

When using the convention, we can also ignore some entity:
```
modelBuilder.Entity<MyEntity>()
    .HasTableAnnotation(DiscriminatorIndexAnnotation.AnnotationName, null);
```

... or yet replace the default convention using the ```HasIndexOnDiscriminator``` extension method.
