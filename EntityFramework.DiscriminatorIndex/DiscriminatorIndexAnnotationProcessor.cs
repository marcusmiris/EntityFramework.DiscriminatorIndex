using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Migrations.Model;
using System.Linq;

namespace EntityFramework.DiscriminatorIndex
{
    /// <summary>
    ///     Classe responsável por converter um <see cref="DiscriminatorIndexAnnotation"/>
    ///     em <see cref="CreateIndexOperation"/>.
    /// </summary>
    public static class DiscriminatorIndexAnnotationProcessor
    {
        /// <summary>
        ///     Revisa as operações, substituindo a <see cref="DiscriminatorIndexAnnotation"/> pelo
        ///     <see cref="CreateIndexOperation"/>.
        /// </summary>
        /// <param name="operations"></param>
        public static IEnumerable<MigrationOperation> Process(IEnumerable<MigrationOperation> operations)
        {
            if (operations == null) throw new ArgumentNullException(nameof(operations));

            return operations
                .SelectMany(operation => (IEnumerable<MigrationOperation>)InternalProcess((dynamic)operation))
                .ToList();
        }

        #region ' private Methods '

        #region ' InternalProces(...) '

        private static IEnumerable<MigrationOperation> InternalProcess(AlterTableOperation operation)
        {
            return InternalProcess(
                operation.Annotations,
                operation.Name);
        }

        private static IEnumerable<MigrationOperation> InternalProcess(MigrationOperation operation)
        {
            yield return operation; // catch all.
        }

        private static IEnumerable<MigrationOperation> InternalProcess(
            IDictionary<string, AnnotationValues> annotations,
            string targetTableName)
        {

            // recupera a anotação
            var indexAnnotation = ((Func<AnnotationValues>) (() =>
            {
                AnnotationValues value;
                annotations.TryGetValue(DiscriminatorIndexAnnotation.AnnotationName, out value);
                return value;
            }))();
            if (indexAnnotation == null) yield break;

            // identifica anotações novas/antigas.
            var oldValue = indexAnnotation.OldValue as DiscriminatorIndexAnnotation;
            var newValue = indexAnnotation.NewValue as DiscriminatorIndexAnnotation;

            // identifica operação (CUD).
            var criou = (oldValue == null) && (newValue != null);
            var excluiu = (oldValue != null) && (newValue == null);
            var alterou = (oldValue != null) && (newValue != null) && !(oldValue.Equals(newValue));


            if (criou)
                yield return CreateIndexOperationFactory(
                    newValue, 
                    targetTableName);

            else if (alterou)
                yield return RenameIndexOperationFactory(
                    oldValue, 
                    newValue, 
                    targetTableName);

            else if (excluiu)
                yield return DropIndexOperationFactory(
                    oldValue, 
                    targetTableName);

            // remove a anotação
            annotations.Remove(DiscriminatorIndexAnnotation.AnnotationName);
        }

        #endregion

        #region ' Index Operation Factories '

        private static RenameIndexOperation RenameIndexOperationFactory(
            DiscriminatorIndexAnnotation oldValue,
            DiscriminatorIndexAnnotation newValue,
            string targetTableName)
        {
            return new RenameIndexOperation(
                table: targetTableName,
                name: oldValue.IndexName,
                newName: newValue.IndexName);
        }

        private static CreateIndexOperation CreateIndexOperationFactory(
            DiscriminatorIndexAnnotation annotation,
            string targetTableName)
        {
            return new CreateIndexOperation()
            {
                Table = targetTableName,
                Columns = {annotation.ColumnName},
                IsUnique = false,
                Name = annotation.IndexName,
            };
        }

        private static DropIndexOperation DropIndexOperationFactory(
            DiscriminatorIndexAnnotation annotation,
            string targetTableName)
        {
            var inverseOperation = CreateIndexOperationFactory(annotation, targetTableName);

            return new DropIndexOperation(inverseOperation)
            {
                Table = targetTableName,
                Columns = { annotation.ColumnName },
                Name = annotation.IndexName,
            };
        }

        #endregion

        #endregion


    }
}
