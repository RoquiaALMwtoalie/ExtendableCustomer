using ExtendableCustomerApi.Model;
using ExtendableCustomerApi.ViewModel.CompanyViewModels;
using ExtendableCustomerApi.ViewModel.ContactViewModels;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Runtime.CompilerServices;

namespace ExtendableCustomerApi.ViewModel
{
    public class AddContactViewModel
    {

        public string Name { get; set; }


        public List<string> CompaniesId { get; set; }


        public List<DynamicAttributeViewModel> DynamicFieldList { get; set; }


    }
}

