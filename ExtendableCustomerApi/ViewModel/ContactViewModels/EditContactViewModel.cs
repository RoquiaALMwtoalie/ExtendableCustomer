using ExtendableCustomerApi.ViewModel.DynamicAttributeViewModels;
using MongoDB.Bson;

namespace ExtendableCustomerApi.ViewModel.ContactViewModels
{
    public class EditContactViewModel
    {
        public string Id { get; set; }

        public string Name { get; set; }


        public List<string> CompaniesId{ get; set; }


        public List<DynamicAttributeViewModel> DynamicFieldList { get; set; }


    }
}
