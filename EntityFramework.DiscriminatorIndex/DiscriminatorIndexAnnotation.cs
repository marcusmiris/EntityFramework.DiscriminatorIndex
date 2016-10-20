namespace EntityFramework.DiscriminatorIndex
{
    /// <summary>
    ///     Adds an annotation to create an index on the discriminator column. 
    /// </summary>
    public class DiscriminatorIndexAnnotation
    {
        public const string AnnotationName = @"DiscriminatorIndexAnnotation";

        #region ' Members '

        /// <summary>
        ///     The name of the column to create the index on.
        /// </summary>
        public string ColumnName { get; }

        /// <summary>
        ///     The name to use for the index in the database. If no value is supplied a unique
        ///     name will be generated.
        /// </summary>
        public string IndexName { get; }

        #endregion

        #region ' Constructor '

        /// <summary />
        /// <param name="columnName">
        ///     The name of the column to create the index on.
        /// </param>
        /// <param name="indexName">
        ///     The name to use for the index in the database. If no value is supplied a unique
        ///     name will be generated
        /// </param>
        public DiscriminatorIndexAnnotation(string indexName, string columnName)
        {
            ColumnName = columnName;
            IndexName = indexName;
        }

        #endregion

        #region ' Equals(...) '

        public bool Equals(DiscriminatorIndexAnnotation annotation)
        {
            if (annotation == null) return false;

            return (ColumnName == annotation.ColumnName) && (IndexName == annotation.IndexName);
        }

        public override bool Equals(object obj) => Equals(obj as DiscriminatorIndexAnnotation);
        public override int GetHashCode() => (ColumnName ?? "").GetHashCode() ^ (IndexName ?? "").GetHashCode();

        #endregion

    }
}
