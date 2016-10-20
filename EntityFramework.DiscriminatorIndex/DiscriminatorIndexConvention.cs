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

        public void Apply(EntitySet item, DbModel model)
        {
            // Somente continua o processamento se o Set conter discriminator.
            var elementType = item.ElementType;
            if (!elementType.Members.Contains(_discriminatorColumnName)) return;

            // Verifica se a anotação de criação de indice já existe.
            // Caso exista, não adiciona novamente.
            {
                var entityType = model.ConceptualModel.EntityTypes.SingleOrDefault(e => e.Name.Equals(elementType.Name));
                var entityTypeConfiguration = entityType?.MetadataProperties["Configuration"]?.Value;
                var annotations = entityTypeConfiguration.GetPrivateFieldValue(@"Annotations") as Dictionary<string, object>;
                var indiceJahCriado = annotations == null || annotations.ContainsKey(DiscriminatorIndexAnnotation.AnnotationName);
                //
                if (indiceJahCriado) return;
            }

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
    }
}
