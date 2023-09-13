using ExtendableCustomerApi.ViewModel.DynamicAttributeViewModels;
using MongoDB.Bson;

namespace ExtendableCustomerApi.ViewModel.CompanyViewModels
{
    public class EditCompanyViewModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public int NumberOfEmployees { get; set; }

        public List<DynamicAttributeViewModel> DynamicFieldList { get; set; }

    }
}

    
