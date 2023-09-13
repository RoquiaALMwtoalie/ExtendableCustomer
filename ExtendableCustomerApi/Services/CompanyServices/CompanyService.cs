using ExtendableCustomerApi.IServices;
using ExtendableCustomerApi.IServices.ICompanyServices;
using ExtendableCustomerApi.Model;
using ExtendableCustomerApi.Model.Filter;
using ExtendableCustomerApi.ViewModel;
using ExtendableCustomerApi.ViewModel.CompanyViewModel;
using ExtendableCustomerApi.ViewModel.CompanyViewModels;
using ExtendableCustomerApi.ViewModel.ContactViewModels;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Transactions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Net.Mime.MediaTypeNames;

namespace ExtendableCustomerApi.Services
{
    public class CompanyService : ICompanyService
    {

        private readonly IMongoCollection<Company> _Company;
        private readonly IMongoCollection<Contact> _Contact;

        public CompanyService(IOptions<ExtendableCustomerDatabaseSettings> settings)
        {
            var mongoClient = new MongoClient(
                 settings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                settings.Value.DatabaseName);

            _Company = mongoDatabase.GetCollection<Company>(
                settings.Value.ExtendableCustomerCollectionName);

            _Contact = mongoDatabase.GetCollection<Contact>(
                settings.Value.ExtendableCustomerCollectionName2);

        }
        public ApiResponse AddCompanyWithDynamicField(AddCompanyViewModel addCompanyViewModel)
        {
            try
            {
                foreach (var item in addCompanyViewModel.DynamicFieldList)
                {
                    if (item.Type.ToLower() == "int")
                    {
                        int Type;
                        var TypeInt = int.TryParse(item.Value, out Type);
                        if (TypeInt == true)
                        {
                            item.Value = Type.ToString();
                        }
                        else
                        {
                            return new ApiResponse(false, $" Wrong Input Value {item.Label} Must Be Type {item.Type}");
                        }
                    }
                    else if (item.Type.ToLower() == "datetime")
                    {
                        DateTime Type;
                        var TypeInt = DateTime.TryParse(item.Value, out Type);
                        if (TypeInt == true)
                        {
                            item.Value = Type.ToString();
                        }
                        else
                        {
                            return new ApiResponse(false, $" Wrong Input Value {item.Label} Must Be Type {item.Type}");
                        }
                    }
                }

                Company company = new Company();
                company = new Company()
                {
                    Name = addCompanyViewModel.Name,
                    NumberOfEmployees = addCompanyViewModel.NumberOfEmployees,
                    Deleted = false,
                    DynamicFieldList = addCompanyViewModel.DynamicFieldList

                };

                _Company.InsertOne(company);

                return new ApiResponse(true, null);
            }
            catch (Exception ex)
            {

                return new ApiResponse(false, ex.Message);
            }

        }
        public ApiResponse GetALLCompany(ComplexFilter Filter)
        {
            try
            {
                List<Company> companies = new List<Company>();
                List<CompanyViewModel> company = new List<CompanyViewModel>();
                var res = _Company.Find(x => !x.Deleted).ToList();
                foreach (var item in res)
                {
                    company.Add(new CompanyViewModel()
                    {
                        Id = item.Id,
                        Name=item.Name,
                        NumberOfEmployees=item.NumberOfEmployees,
                        Deleted=item.Deleted,
                        DynamicFieldList=item.DynamicFieldList
                    });
                }
                


                if (!string.IsNullOrEmpty(Filter.SearchQuery))
                {
                    company = company.Where(x => x.Name.ToLower().StartsWith(Filter.SearchQuery.ToLower())).ToList();
                }

                if (Filter.Filters != null ? Filter.Filters.Count != 0 : false)
                {
                    foreach (SimpleFilter SimpleFilter in Filter.Filters)
                    {
                        string? attributeName = typeof(CompanyViewModel)
                                .GetProperties()
                                .Where(x => x.Name.ToLower() == SimpleFilter.Key.ToLower())
                                .FirstOrDefault()?
                                .Name;

                        string? attributeNames = typeof(DynamicAttributeViewModel)
                               .GetProperties()
                               .Where(x => x.Name.ToLower() == SimpleFilter.Key.ToLower())
                               .FirstOrDefault()?
                               .Name;
                        if (attributeName != null)
                        {
                            PropertyInfo? FilterKeyProperty = typeof(CompanyViewModel).GetProperty(attributeName);
                            if (FilterKeyProperty.PropertyType == typeof(string)|| FilterKeyProperty.PropertyType == typeof(bool))
                            {
                                foreach (string FilterValue in SimpleFilter.Values)
                                {
                                    company = company.Where(x => FilterKeyProperty.GetValue(x).ToString().ToLower().Contains(FilterValue.ToLower())).ToList();
                                }
                            }
                            else if (FilterKeyProperty.PropertyType == typeof(DateTime))
                            {
                                company = company.Where(x => DateTime.Parse(FilterKeyProperty.GetValue(x, null).ToString()).ToString("MM/dd/yyyy")
                                   == DateTime.Parse(SimpleFilter.Values.FirstOrDefault().ToString()).ToString("MM/dd/yyyy")).ToList();

                            }

                            else
                            {
                                company = company.Where(x => SimpleFilter.Values.Contains(FilterKeyProperty.GetValue(x).ToString())).ToList();
                            }

                        }
                        if (attributeNames != null)
                        {
                            PropertyInfo? FilterKeyPropertys = typeof(DynamicAttributeViewModel).GetProperty(attributeNames);
                            List<DynamicAttributeViewModel> dynamicAttributeViewModels = new List<DynamicAttributeViewModel>();
                            if (SimpleFilter.Key.ToLower() == "value")
                            {
                                foreach (var itemvalues in SimpleFilter.Values)
                                {


                                    var allvalue = _Company.Find(x => true).ToList();
                                    var value = allvalue.Select(x => x.DynamicFieldList);

                                    foreach (var itemvalue in value)
                                    {
                                        var itemvaluee = itemvalue.Where(x => x.Value.ToLower().Contains(itemvalues.ToLower())).FirstOrDefault();


                                        if (itemvaluee != null)
                                        {

                                            foreach (var item in itemvalue)
                                            {

                                                dynamicAttributeViewModels.Add(new DynamicAttributeViewModel()
                                                {
                                                    Label = item.Label,
                                                    Type = item.Type,
                                                    Value = item.Value

                                                });
                                            }


                                            var DynamicValue = itemvalue.Where(x => x.Value.ToLower().Contains(itemvalues.ToLower())).FirstOrDefault().Type;

                                            if (DynamicValue != null)
                                            {
                                                if (DynamicValue.ToLower() == "string" || DynamicValue.ToLower() == "bool")
                                                {
                                                    foreach (string FilterValues in SimpleFilter.Values)
                                                    {

                                                        dynamicAttributeViewModels = dynamicAttributeViewModels.Where(x => FilterKeyPropertys.GetValue(x).ToString().ToLower().Contains(FilterValues.ToLower())).ToList();


                                                    }
                                                }
                                                else if (DynamicValue.ToLower() == ("datetime"))
                                                {

                                                    dynamicAttributeViewModels = dynamicAttributeViewModels.Where(x => DateTime.Parse(FilterKeyPropertys.GetValue(x, null).ToString()).ToString("MM/dd/yyyy")
                                                  == DateTime.Parse(SimpleFilter.Values.FirstOrDefault().ToString()).ToString("MM/dd/yyyy")).ToList();



                                                }

                                                else
                                                {

                                                    dynamicAttributeViewModels = dynamicAttributeViewModels.Where(x => SimpleFilter.Values.Contains(FilterKeyPropertys.GetValue(x).ToString())).ToList();


                                                }
                                            }

                                        }
                                    }
                                }
                                int Count2 = dynamicAttributeViewModels.Count();

                                if (!string.IsNullOrEmpty(Filter.Sort))
                                {
                                    PropertyInfo? SortProperty = typeof(DynamicAttributeViewModel).GetProperty(Filter.Sort);

                                    if (SortProperty != null && Filter.Order == "asc")
                                        dynamicAttributeViewModels = dynamicAttributeViewModels.OrderBy(x => SortProperty.GetValue(x)).ToList();

                                    else if (SortProperty != null && Filter.Order == "desc")
                                        dynamicAttributeViewModels = dynamicAttributeViewModels.OrderByDescending(x => SortProperty.GetValue(x)).ToList();

                                    dynamicAttributeViewModels = dynamicAttributeViewModels.Skip((Filter.PageIndex - 1) * Filter.PageSize)
                                        .Take(Filter.PageSize).ToList();


                                    var companys = _Company.Find(x => true).ToList();
                                    foreach (var item in companys)
                                    {
                                        if (item.DynamicFieldList[0].Value == dynamicAttributeViewModels[0].Value)
                                        {
                                            companies.Add(item);
                                        }
                                    }
                                    return new ApiResponse(companies, null, Count2);
                                }

                                else
                                {
                                    dynamicAttributeViewModels = dynamicAttributeViewModels.Skip((Filter.PageIndex - 1) * Filter.PageSize)
                                        .Take(Filter.PageSize).ToList();

                                    var companys2 = _Company.Find(x => true).ToList();
                                    foreach (var item in companys2)
                                    {
                                        if (item.DynamicFieldList[0].Value == dynamicAttributeViewModels[0].Value)
                                        {
                                            companies.Add(item);
                                        }
                                    }
                                    return new ApiResponse(companies, null, Count2);
                 
                                }
                                var companys3 = _Company.Find(x => true).ToList();
                                foreach (var item in companys3)
                                {
                                    if (item.DynamicFieldList[0].Value == dynamicAttributeViewModels[0].Value)
                                    {
                                        companies.Add(item);
                                    }
                                }
                                return new ApiResponse(companies, null, Count2);
                            }
                            if (SimpleFilter.Key.ToLower() == "type")
                            {
                                foreach (var itemvalues in SimpleFilter.Values)
                                {


                                    var allvalue = _Company.Find(x => true).ToList();
                                    var value = allvalue.Select(x => x.DynamicFieldList);

                                    foreach (var itemvalue in value)
                                    {
                                        var itemvaluee = itemvalue.Where(x => x.Type.ToLower().Contains(itemvalues.ToLower())).FirstOrDefault();


                                        if (itemvaluee != null)
                                        {

                                            foreach (var item in itemvalue)
                                            {

                                                dynamicAttributeViewModels.Add(new DynamicAttributeViewModel()
                                                {
                                                    Label = item.Label,
                                                    Type = item.Type,
                                                    Value = item.Value

                                                });
                                            }

                                            foreach (string FilterValues in SimpleFilter.Values)
                                            {

                                                dynamicAttributeViewModels = dynamicAttributeViewModels.Where(x => FilterKeyPropertys.GetValue(x).ToString().ToLower().Contains(FilterValues.ToLower())).ToList();


                                            }

                                        }
                                    }
                                }
                                int Count2 = dynamicAttributeViewModels.Count();

                                if (!string.IsNullOrEmpty(Filter.Sort))
                                {
                                    PropertyInfo? SortProperty = typeof(DynamicAttributeViewModel).GetProperty(Filter.Sort);

                                    if (SortProperty != null && Filter.Order == "asc")
                                        dynamicAttributeViewModels = dynamicAttributeViewModels.OrderBy(x => SortProperty.GetValue(x)).ToList();

                                    else if (SortProperty != null && Filter.Order == "desc")
                                        dynamicAttributeViewModels = dynamicAttributeViewModels.OrderByDescending(x => SortProperty.GetValue(x)).ToList();

                                    dynamicAttributeViewModels = dynamicAttributeViewModels.Skip((Filter.PageIndex - 1) * Filter.PageSize)
                                        .Take(Filter.PageSize).ToList();


                                    var companys = _Company.Find(x => true).ToList();
                                    foreach (var item in companys)
                                    {
                                        if (item.DynamicFieldList[0].Type == dynamicAttributeViewModels[0].Type)
                                        {
                                            companies.Add(item);
                                        }
                                    }
                                    return new ApiResponse(companies, null, Count2);
                                }

                                else
                                {
                                    dynamicAttributeViewModels = dynamicAttributeViewModels.Skip((Filter.PageIndex - 1) * Filter.PageSize)
                                        .Take(Filter.PageSize).ToList();

                                    var companys2 = _Company.Find(x => true).ToList();
                                    foreach (var item in companys2)
                                    {
                                        if (item.DynamicFieldList[0].Type == dynamicAttributeViewModels[0].Type)
                                        {
                                            companies.Add(item);
                                        }
                                    }
                                    return new ApiResponse(companies, null, Count2);
                                }
                                var companys3 = _Company.Find(x => true).ToList();
                                foreach (var item in companys3)
                                {
                                    if (item.DynamicFieldList[0].Type == dynamicAttributeViewModels[0].Type)
                                    {
                                        companies.Add(item);
                                    }
                                }
                                return new ApiResponse(companies, null, Count2);
                            }
                            if (SimpleFilter.Key.ToLower() == "label")
                            {
                                foreach (var itemvalues in SimpleFilter.Values)
                                {


                                    var allvalue = _Company.Find(x => true).ToList();
                                    var value = allvalue.Select(x => x.DynamicFieldList);

                                    foreach (var itemvalue in value)
                                    {
                                        var itemvaluee = itemvalue.Where(x => x.Label.ToLower().Contains(itemvalues.ToLower())).FirstOrDefault();


                                        if (itemvaluee != null)
                                        {

                                            foreach (var item in itemvalue)
                                            {

                                                dynamicAttributeViewModels.Add(new DynamicAttributeViewModel()
                                                {
                                                    Label = item.Label,
                                                    Type = item.Type,
                                                    Value = item.Value

                                                });
                                            }

                                            foreach (string FilterValues in SimpleFilter.Values)
                                            {

                                                dynamicAttributeViewModels = dynamicAttributeViewModels.Where(x => FilterKeyPropertys.GetValue(x).ToString().ToLower().Contains(FilterValues.ToLower())).ToList();


                                            }


                                        }
                                    }
                                }
                                int Count2 = dynamicAttributeViewModels.Count();

                                if (!string.IsNullOrEmpty(Filter.Sort))
                                {
                                    PropertyInfo? SortProperty = typeof(DynamicAttributeViewModel).GetProperty(Filter.Sort);

                                    if (SortProperty != null && Filter.Order == "asc")
                                        dynamicAttributeViewModels = dynamicAttributeViewModels.OrderBy(x => SortProperty.GetValue(x)).ToList();

                                    else if (SortProperty != null && Filter.Order == "desc")
                                        dynamicAttributeViewModels = dynamicAttributeViewModels.OrderByDescending(x => SortProperty.GetValue(x)).ToList();

                                    dynamicAttributeViewModels = dynamicAttributeViewModels.Skip((Filter.PageIndex - 1) * Filter.PageSize)
                                        .Take(Filter.PageSize).ToList();


                                    var companys = _Company.Find(x => true).ToList();
                                    foreach (var item in companys)
                                    {
                                        if (item.DynamicFieldList[0].Label == dynamicAttributeViewModels[0].Label)
                                        {
                                            companies.Add(item);
                                        }
                                    }
                                    return new ApiResponse(companies, null, Count2);
                                }

                                else
                                {
                                    dynamicAttributeViewModels = dynamicAttributeViewModels.Skip((Filter.PageIndex - 1) * Filter.PageSize)
                                        .Take(Filter.PageSize).ToList();

                                    var companys2 = _Company.Find(x => true).ToList();
                                    foreach (var item in companys2)
                                    {
                                        if (item.DynamicFieldList[0].Label == dynamicAttributeViewModels[0].Label)
                                        {
                                            companies.Add(item);
                                        }
                                    }
                                    return new ApiResponse(companies, null, Count2);
                                }
                                var companys3 = _Company.Find(x => true).ToList();
                                foreach (var item in companys3)
                                {
                                    if (item.DynamicFieldList[0].Label == dynamicAttributeViewModels[0].Label)
                                    {
                                        companies.Add(item);
                                    }
                                }
                                return new ApiResponse(companies, null, Count2);
                            }

                        }

                    }


                }
                int Count = company.Count();

                if (!string.IsNullOrEmpty(Filter.Sort))
                {
                    PropertyInfo? SortProperty = typeof(CompanyViewModel).GetProperty(Filter.Sort);

                    if (SortProperty != null && Filter.Order == "asc")
                        company = company.OrderBy(x => SortProperty.GetValue(x)).ToList();

                    else if (SortProperty != null && Filter.Order == "desc")
                        company = company.OrderByDescending(x => SortProperty.GetValue(x)).ToList();

                    company = company.Skip((Filter.PageIndex - 1) * Filter.PageSize)
                        .Take(Filter.PageSize).ToList();

                    return new ApiResponse(company, null, Count);
                }

                else
                {
                    company = company.Skip((Filter.PageIndex - 1) * Filter.PageSize)
                        .Take(Filter.PageSize).ToList();

                    return new ApiResponse(company, null, Count);
                }

                return new ApiResponse(res, null, Count);
            }


            catch (Exception ex)
            {

                return new ApiResponse(null, ex.Message);
            }
        }
        public ApiResponse EditCompany(EditCompanyViewModel editCompanyViewModel)
        {
            try
            {
                foreach (var item in editCompanyViewModel.DynamicFieldList)
                {
                    if (item.Type.ToLower() == "int")
                    {
                        int Type;
                        var TypeInt = int.TryParse(item.Value, out Type);
                        if (TypeInt == true)
                        {
                            item.Value = Type.ToString();
                        }
                        else
                        {
                            return new ApiResponse(false, $" Wrong Input Value {item.Label} Must Be Type {item.Type}");
                        }
                    }
                    else if (item.Type.ToLower() == "datetime")
                    {
                        DateTime Type;
                        var TypeInt = DateTime.TryParse(item.Value, out Type);
                        if (TypeInt == true)
                        {
                            item.Value = Type.ToString();
                        }
                        else
                        {
                            return new ApiResponse(false, $" Wrong Input Value {item.Label} Must Be Type {item.Type}");
                        }
                    }
                }

                var filter = Builders<Company>.Filter.Eq(x => x.Id, editCompanyViewModel.Id);

                var update = Builders<Company>.Update
                    .Set(x => x.Name, editCompanyViewModel.Name)
                    .Set(x => x.NumberOfEmployees, editCompanyViewModel.NumberOfEmployees)
                     .Set(x => x.DynamicFieldList, editCompanyViewModel.DynamicFieldList);

                var result = _Company.UpdateOneAsync(filter, update);
                return new ApiResponse(true, null);
            }
            catch (Exception ex)
            {

                return new ApiResponse(false, ex.Message);
            }


        }
        public ApiResponse GetCompanyById(string id)
        {
            var company = _Company.Find(x => x.Id.ToString() == id).FirstOrDefault();
            if (company != null)
            {
                CompanyViewModel companyviewmodel = new CompanyViewModel();

                companyviewmodel = new CompanyViewModel()
                {
                   Id= company.Id,
                   Name= company.Name,
                   NumberOfEmployees= company.NumberOfEmployees,
                   DynamicFieldList= company.DynamicFieldList,
                   Deleted= company.Deleted

                };

                return new ApiResponse(companyviewmodel, null);

            }

            else
            {
                return new ApiResponse(null, "Not find this Id");

            }


        }
        public ApiResponse DeleteCompany(string id)
        {
            using (var scope = new TransactionScope())
            {
                try
                {
                    var company = _Company.Find(x => x.Id.ToString() == id && x.Deleted == false).FirstOrDefault();
                    if (company != null)
                    {
                        var filter = Builders<Company>.Filter.Eq(x => x.Id, id);

                        var update = Builders<Company>.Update
                            .Set(x => x.Deleted, true);

                        var result = _Company.UpdateOneAsync(filter, update);

                        var contact = _Contact.Find(x => true).ToList();
                        var indexesToRemove = new List<int>();

                        foreach (var item in contact)
                        {
                          
                                for (int i = 0; i < item.Companies.Count; i++)
                                {
                                    if (item.Companies[i]==id && !string.IsNullOrEmpty(item.Companies[i]))
                                    {
                                        indexesToRemove.Add(i);
                                    }
                                        
                                    foreach (var item3 in indexesToRemove)
                                    {
                                        item.Companies.RemoveAt(item3);
                                    }
                                    
                                }
                                var filter2 = Builders<Contact>.Filter.Eq(x => x.Id, item.Id);
                                var update2 = Builders<Contact>.Update.Set(x => x.Companies, item.Companies);

                                var result2 = _Contact.UpdateOneAsync(filter2, update2);
                            
                        }

                        scope.Complete();
                        return new ApiResponse(true, null);
                    }
                    return new ApiResponse(false, "This Id is not valid");
                }
                catch (Exception ex)
                {                   
                    return new ApiResponse(true, ex.Message);
                }

            }
        }
    }
}