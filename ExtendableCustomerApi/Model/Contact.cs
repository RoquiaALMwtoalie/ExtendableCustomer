using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using ExtendableCustomerApi.ViewModel;

namespace ExtendableCustomerApi.Model
{
    public class Contact
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("Companies")]

        public List<string> Companies { get; set; }

        [BsonElement("DynamicFieldList")]
        public List<DynamicAttributeViewModel> DynamicFieldList { get; set; }

        [BsonElement("Deleted")]
        public bool Deleted { get; set; }

    }
}
