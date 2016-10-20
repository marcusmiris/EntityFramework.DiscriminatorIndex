using System.Data.Entity.Infrastructure;
using Newtonsoft.Json;

namespace EntityFramework.DiscriminatorIndex
{
    public class DiscriminatorIndexAnnotationSerializer
        : IMetadataAnnotationSerializer
    {

        public static string Serialize(DiscriminatorIndexAnnotation annotation) 
            => JsonConvert.SerializeObject(
                annotation,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

        public static DiscriminatorIndexAnnotation Deserialize(string json)
            => JsonConvert.DeserializeObject<DiscriminatorIndexAnnotation>(json);

        #region ' IMetadataAnnotationSerializer '

        public string Serialize(string name, object value) => Serialize(value as DiscriminatorIndexAnnotation);

        public object Deserialize(string name, string value) => Deserialize(value);

        #endregion
    }
}
