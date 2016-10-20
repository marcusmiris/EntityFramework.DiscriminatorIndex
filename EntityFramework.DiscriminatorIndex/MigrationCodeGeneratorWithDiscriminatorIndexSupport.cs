using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Migrations.Design;
using System.Data.Entity.Migrations.Model;

namespace EntityFramework.DiscriminatorIndex
{
    /// <summary>
    ///     Generic code-based migration generator that adds
    ///     support to add index on TPH discriminator column.
    /// </summary>
    public class MigrationCodeGeneratorWithDiscriminatorIndexSupport
        : MigrationCodeGenerator
    {
        private readonly MigrationCodeGenerator _wrapped;

        #region ' Constructor '

        public MigrationCodeGeneratorWithDiscriminatorIndexSupport(MigrationCodeGenerator wrapped)
        {
            _wrapped = wrapped;
            AnnotationGenerators = new Dictionary<string, Func<AnnotationCodeGenerator>>(_wrapped.AnnotationGenerators)
            {
                [DiscriminatorIndexAnnotation.AnnotationName] = () => new DiscriminatorIndexAnnotationCodeGenerator()
            };
        }

        #endregion

        #region 'MigrationCodeGenerator '

        public override ScaffoldedMigration Generate(
            string migrationId,
            IEnumerable<MigrationOperation> operations,
            string sourceModel,
            string targetModel,
            string @namespace,
            string className)
        {
            var operacoesProcessadas = DiscriminatorIndexAnnotationProcessor.Process(operations);

            return _wrapped.Generate(
                migrationId,
                operacoesProcessadas,
                sourceModel,
                targetModel,
                @namespace,
                className);
        }

        public override IDictionary<string, Func<AnnotationCodeGenerator>> AnnotationGenerators { get; }

        #endregion

    }


}
