using ExtendableCustomerApi.ViewModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;

namespace ExtendableCustomerApi.Model
{
    public class Company
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("NumberOfEmployees")]
        public int NumberOfEmployees { get; set; }

        [BsonElement("DynamicFieldList")]
        public List<DynamicAttributeViewModel> DynamicFieldList { get; set; }

        [BsonElement("Deleted")]
        public bool Deleted { get; set; }

    }
}
