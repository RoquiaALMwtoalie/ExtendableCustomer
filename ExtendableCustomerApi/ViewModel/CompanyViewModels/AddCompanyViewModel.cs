using MongoDB.Bson.Serialization.Attributes;

namespace ExtendableCustomerApi.ViewModel.CompanyViewModel
{
    public class AddCompanyViewModel
    {


        public string Name { get; set; }

        public int NumberOfEmployees { get; set; }


        public List<DynamicAttributeViewModel> DynamicFieldList { get; set; }

     

    }
}
