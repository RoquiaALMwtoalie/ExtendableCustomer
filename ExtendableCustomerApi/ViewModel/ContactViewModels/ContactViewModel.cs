using ExtendableCustomerApi.Model;
using ExtendableCustomerApi.ViewModel.CompanyViewModels;
using MongoDB.Bson;

namespace ExtendableCustomerApi.ViewModel.ContactViewModels
{
    public class ContactViewModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public List<string> CompaniesId { get; set; }

        public List<string> CompaniesName { get; set; }


        public List<DynamicAttributeViewModel> DynamicFieldList { get; set; }


        public bool Deleted { get; set; }
    }
    
}
