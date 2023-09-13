using ExtendableCustomerApi.Model;
using ExtendableCustomerApi.Model.Filter;
using ExtendableCustomerApi.ViewModel;
using ExtendableCustomerApi.ViewModel.ContactViewModels;
using MongoDB.Bson;

namespace ExtendableCustomerApi.IServices.IContactServices
{
    public interface IContactService
    {
        ApiResponse AddContactWithDynamicField(AddContactViewModel addContactViewModel);
        ApiResponse GetAllContact(ComplexFilter Filter);
        ApiResponse EditContact(EditContactViewModel editContactViewModel);
        ApiResponse GetContactById(string id);
        ApiResponse DeleteContact(string id);




    }
}
