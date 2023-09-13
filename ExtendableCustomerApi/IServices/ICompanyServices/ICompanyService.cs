using ExtendableCustomerApi.Model;
using ExtendableCustomerApi.Model.Filter;
using ExtendableCustomerApi.ViewModel;
using ExtendableCustomerApi.ViewModel.CompanyViewModel;
using ExtendableCustomerApi.ViewModel.CompanyViewModels;
using MongoDB.Bson;

namespace ExtendableCustomerApi.IServices.ICompanyServices
{
    public interface ICompanyService
    {
        ApiResponse AddCompanyWithDynamicField(AddCompanyViewModel addCompanyViewModel);
        ApiResponse GetALLCompany(ComplexFilter Filter);
        ApiResponse EditCompany(EditCompanyViewModel editCompanyViewModel);
        ApiResponse GetCompanyById(string id);
        ApiResponse DeleteCompany(string id);
    }
}
