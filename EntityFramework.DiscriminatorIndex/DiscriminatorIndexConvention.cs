using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;

namespace EntityFramework.DiscriminatorIndex
{
    /// <summary>
    ///     Convenção que configura um índice para todas as tabelas que possuam 
    ///     a coluna 'Discriminator'
    /// </summary>
    public class DiscriminatorIndexConvention
        : IStoreModelConvention<EntitySet>
    {
        private readonly string _discriminatorColumnName;

        #region ' Constructor '

        public DiscriminatorIndexConvention() : this(@"Discriminator") { }

        public DiscriminatorIndexConvention(string discriminatorColumnName)
        {
            _discriminatorColumnName = discriminatorColumnName;
        }

        #endregion

        #region ' IStoreModelConvention<EntitySet> '

        public void Apply(EntitySet item, DbModel dbModel)
        {
            // Somente continua o processamento se o Set conter discriminator.
            var elementType = item.ElementType;
            if (!elementType.Members.Contains(_discriminatorColumnName)) return;

            // Verifica se a anotação de criação de indice já existe.
            // Caso exista, não adiciona novamente.
            if (IndiceJahCriado(elementType.Name, dbModel))
                return;

            // Cria a anotação para criação de índice.
            {
                var annotationName = $"http://schemas.microsoft.com/ado/2013/11/edm/customannotation:{ DiscriminatorIndexAnnotation.AnnotationName }";
                var newIndexName = $"IX_{elementType.Name}_{_discriminatorColumnName}";
                //
                elementType.AddAnnotation(
                    name: annotationName, 
                    value: new DiscriminatorIndexAnnotation(newIndexName, _discriminatorColumnName));
            }
        }

        #endregion

        /// <summary>
        ///     Verifica se já existe um índice sobre a coluna discriminator.
        /// </summary>
        private bool IndiceJahCriado(string elementTypeName, DbModel dbModel)
        {
            var entityType = dbModel.ConceptualModel.EntityTypes.SingleOrDefault(e => e.Name.Equals(elementTypeName));
            var entityTypeConfiguration = entityType?.MetadataProperties.Contains("Configuration") == true
                ? entityType.MetadataProperties["Configuration"].Value
                : null;
            var annotations = entityTypeConfiguration?.GetPrivateFieldValue(@"Annotations") as Dictionary<string, object>;
            var indiceJahCriado = annotations != null && annotations.ContainsKey(DiscriminatorIndexAnnotation.AnnotationName);
            //
            return indiceJahCriado;
        }
    }
}
