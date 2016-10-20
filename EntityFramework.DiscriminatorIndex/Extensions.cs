using System;
using System.Data.Entity.Migrations;
using System.Data.Entity.ModelConfiguration;
using System.Reflection;

namespace EntityFramework.DiscriminatorIndex
{
    public static class Extensions
    {

        #region ' Bootstrap '

        public static void AddSupportToDiscriminatorIndex(
            this DbMigrationsConfiguration migrationConfig)
        {
            if (migrationConfig == null) throw new ArgumentNullException(nameof(migrationConfig));

            migrationConfig.CodeGenerator = new MigrationCodeGeneratorWithDiscriminatorIndexSupport(migrationConfig.CodeGenerator);
            migrationConfig.ConfigureSqlGeneratorForAllProviders();
        }

        #endregion

        public static void ConfigureSqlGeneratorForAllProviders(
            this DbMigrationsConfiguration migrationConfiguration)
        {
            var providers = migrationConfiguration.GetAvailableProviders();
            foreach (var providerName in providers)
            {
                var currentSqlGenerator = migrationConfiguration.GetSqlGenerator(providerName);
                var newSqlGenerator = new MigrationSqlGeneratorWithDiscriminatorIndexSupport(currentSqlGenerator);
                migrationConfiguration.SetSqlGenerator(providerName, newSqlGenerator);
            }
        }


        /// <summary />
        /// <typeparam name="TEntityType" />
        /// <param name="configuration" />
        /// <param name="columnName" />
        /// <param name="indexName">
        ///     o nome do índice. 
        ///     Caso não seja informado, um novo nome será gerado pelo sistema.
        /// </param>
        public static void HasIndexOnDiscriminator<TEntityType>(
            this EntityTypeConfiguration<TEntityType> configuration,
            string columnName = "Discriminator",
            string indexName = null) 
            where TEntityType : class
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            // se o nome do index não foi informado, gera-o.
            if (indexName == null)
            {
                var tableName = typeof(TEntityType).Name;
                indexName = $@"IX_{tableName}_{columnName}";
            }
            

            configuration.HasTableAnnotation(
                DiscriminatorIndexAnnotation.AnnotationName,
                new DiscriminatorIndexAnnotation(indexName, columnName));
        }


        public static TField GetPrivateField<TField>(
            this object @source,
            string fieldName)
        {
            return (TField)@source.GetPrivateFieldValue(fieldName);
        }

        /// <summary>
        /// Returns a private Property Value from a given Object. Uses Reflection.
        /// Throws a ArgumentOutOfRangeException if the Property is not found.
        /// </summary>
        /// <param name="obj">Object from where the Property Value is returned</param>
        /// <param name="propName">Propertyname as string.</param>
        /// <returns>PropertyValue</returns>
        public static object GetPrivateFieldValue(this object obj, string propName)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            Type t = obj.GetType();
            FieldInfo fieldInfo = null;
            PropertyInfo propertyInfo = null;
            while (fieldInfo == null && propertyInfo == null && t != null)
            {
                fieldInfo = t.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo == null)
                {
                    propertyInfo = t.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                }

                t = t.BaseType;
            }
            if (fieldInfo == null && propertyInfo == null)
                throw new ArgumentOutOfRangeException(nameof(propName),
                    $"Field {propName} was not found in Type {obj.GetType().FullName}");

            if (fieldInfo != null)
                return fieldInfo.GetValue(obj);

            return propertyInfo.GetValue(obj, null);
        }

    }
}
