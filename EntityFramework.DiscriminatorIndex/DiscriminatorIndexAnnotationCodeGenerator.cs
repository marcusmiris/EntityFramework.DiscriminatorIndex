using System;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Migrations.Utilities;

namespace EntityFramework.DiscriminatorIndex
{
    /// <summary>
    ///     Componente que, quando configurado no Entity Framework, processa o 
    ///     valor dos <see cref="AnnotationValues"/> dos <see cref="DiscriminatorIndexAnnotation"/>.
    ///     ao gerar o código dos migrations (e.g. Add-Migration).
    /// </summary>
    /// <remarks>
    ///     Esta classe somente é necessária porque o <see cref="DiscriminatorIndexAnnotation"/>
    ///     é uma anotação de tipo complexto (a.k.a não string).
    /// </remarks>
    public class DiscriminatorIndexAnnotationCodeGenerator
        : AnnotationCodeGenerator
    {

        #region ' abstract base implementation '

        public override void Generate(
            string annotationName,
            object annotation,
            IndentedTextWriter writer)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            // recupera anotação
            var discriminatorIndexAnnotation = annotation as DiscriminatorIndexAnnotation;
            if (discriminatorIndexAnnotation == null) return;

            // serializa
            var serialized = DiscriminatorIndexAnnotationSerializer.Serialize(discriminatorIndexAnnotation);

            writer.Write(Quote(serialized));
        }

        #endregion

        private static string Quote(string @string) => $"@\"{@string.Replace("\"", "\"\"")}\"";
    }
}
