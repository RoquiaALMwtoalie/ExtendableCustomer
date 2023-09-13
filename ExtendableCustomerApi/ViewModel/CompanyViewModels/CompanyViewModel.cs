using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ExtendableCustomerApi.ViewModel.CompanyViewModels
{
    public class CompanyViewModel
    {

        public string Id { get; set; }

        public string Name { get; set; }

        public int NumberOfEmployees { get; set; }

        public bool Deleted { get; set; }
        public List<DynamicAttributeViewModel> DynamicFieldList { get; set; }

    }
   

}
