using System.Collections.Generic;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Sql;

namespace EntityFramework.DiscriminatorIndex
{
    /// <summary>
    ///     Decora o SQL Generator informado, habilitando-o para processar os <see cref="DiscriminatorIndexAnnotation"/>.
    ///     Por fim, operações de Create/Rename/Drop index serão criados como fruto das anotações.
    /// </summary>
    public class MigrationSqlGeneratorWithDiscriminatorIndexSupport : MigrationSqlGenerator
    {
        private readonly MigrationSqlGenerator _migrationSqlGenerator;

        #region ' Constructor '

        /// <summary />
        /// <param name="migrationSqlGenerator">
        ///     SQL Generator que será decorado.
        /// </param>
        public MigrationSqlGeneratorWithDiscriminatorIndexSupport(MigrationSqlGenerator migrationSqlGenerator)
        {
            _migrationSqlGenerator = migrationSqlGenerator;
        }

        #endregion

        #region ' abstract base implementation '

        public override IEnumerable<MigrationStatement> Generate(
            IEnumerable<MigrationOperation> migrationOperations, 
            string providerManifestToken)
        {
            // usa o `DiscriminatorIndexAnnotationProcessor` para substituir as anotações
            // do DiscriminatorIndex pelas operações de Create/Rename/Delete index.
            migrationOperations = DiscriminatorIndexAnnotationProcessor.Process(migrationOperations);

            // continua o processamento utilizando o SQL Generator decorado.
            return _migrationSqlGenerator.Generate(migrationOperations, providerManifestToken);
        }

        #endregion

    }
         

}
