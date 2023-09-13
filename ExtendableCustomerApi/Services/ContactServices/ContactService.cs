using ExtendableCustomerApi.IServices.IContactServices;
using ExtendableCustomerApi.Model;
using ExtendableCustomerApi.Model.Filter;
using ExtendableCustomerApi.ViewModel;
using ExtendableCustomerApi.ViewModel.CompanyViewModels;
using ExtendableCustomerApi.ViewModel.ContactViewModels;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.ComponentModel.Design;
using System.Reflection;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ExtendableCustomerApi.Services.ContactServices
{
    public class ContactService :IContactService
    {

        private readonly IMongoCollection<Contact> _Contact;
        private readonly IMongoCollection<Company> _Company;
        public ContactService(IOptions<ExtendableCustomerDatabaseSettings> settings)
        {
            var mongoClient = new MongoClient(
                 settings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                settings.Value.DatabaseName);

            _Contact = mongoDatabase.GetCollection<Contact>(
                settings.Value.ExtendableCustomerCollectionName2);

            _Company = mongoDatabase.GetCollection<Company>(
              settings.Value.ExtendableCustomerCollectionName);

        }
        public ApiResponse AddContactWithDynamicField(AddContactViewModel addContactViewModel)
        {
            try
            {
                foreach (var item in addContactViewModel.CompaniesId)
                {
                    var company = _Company.Find(x => x.Id == item && !x.Deleted).FirstOrDefault();
                    if (company == null)
                    {
                        return new ApiResponse(false, $"The CompanyId {item} Is Not Found");
                    }
                }
                foreach (var item in addContactViewModel.DynamicFieldList)
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
                foreach (var item in addContactViewModel.CompaniesId)
                {
                    var company = _Company.Find(x => x.Id == item).FirstOrDefault();
                    
                }
               
                Contact contact = new Contact();
                contact = new Contact()
                {
                    Name = addContactViewModel.Name,
                    Companies = addContactViewModel.CompaniesId,
                    Deleted = false,
                    DynamicFieldList = addContactViewModel.DynamicFieldList

                };

                _Contact.InsertOne(contact);

                return new ApiResponse(true, null);
            }
            catch (Exception ex)
            {
                if(ex.Message.Contains("is not a valid 24 digit hex string"))
                {
                    return new ApiResponse(false, "The CompanyId Is Not Found");
                }
                return new ApiResponse(false, ex.Message);
            }

        }
        public ApiResponse GetAllContact(ComplexFilter Filter)
        {
            try
            {
                List<Contact> contacts = new List<Contact>();
                List<ContactViewModel> contact = new List<ContactViewModel>();
                var res = _Contact.Find(x => !x.Deleted).ToList();
               
                foreach (var item in res)
                {
                    List<string> CompanyNames = new List<string>();
                    foreach (var Companyvalue in item.Companies)
                    {
                        var CompanyName = _Company.Find(x => x.Id == Companyvalue).FirstOrDefault().Name;
                        CompanyNames.Add(CompanyName);
                    }

                    contact.Add(new ContactViewModel()
                    {
                        Id = item.Id,
                        Name = item.Name,
                        CompaniesId=item.Companies,
                        CompaniesName = CompanyNames,
                        Deleted = item.Deleted,
                        DynamicFieldList = item.DynamicFieldList

                    });
                    
                }
               


                if (!string.IsNullOrEmpty(Filter.SearchQuery))
                {
                    contact = contact.Where(x => x.Name.ToLower().StartsWith(Filter.SearchQuery.ToLower())).ToList();
                }

                if (Filter.Filters != null ? Filter.Filters.Count != 0 : false)
                {
                    foreach (SimpleFilter SimpleFilter in Filter.Filters)
                    {
                        string? attributeName = typeof(ContactViewModel)
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
                            PropertyInfo? FilterKeyProperty = typeof(ContactViewModel).GetProperty(attributeName);
                            if (FilterKeyProperty.PropertyType == typeof(string) || FilterKeyProperty.PropertyType == typeof(bool))
                            {
                                foreach (string FilterValue in SimpleFilter.Values)
                                {
                                    contact = contact.Where(x => FilterKeyProperty.GetValue(x).ToString().ToLower().Contains(FilterValue.ToLower())).ToList();
                                }
                            }
                            else if (FilterKeyProperty.PropertyType == typeof(DateTime))
                            {
                                contact = contact.Where(x => DateTime.Parse(FilterKeyProperty.GetValue(x, null).ToString()).ToString("MM/dd/yyyy")
                                   == DateTime.Parse(SimpleFilter.Values.FirstOrDefault().ToString()).ToString("MM/dd/yyyy")).ToList();

                            }

                            else
                            {
                                contact = contact.Where(x => SimpleFilter.Values.Contains(FilterKeyProperty.GetValue(x).ToString())).ToList();
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


                                    var allvalue = _Contact.Find(x => true).ToList();
                                    var value = allvalue.Select(x => x.DynamicFieldList);

                                    foreach (var itemvalue in value)
                                    {
                                        var itemvaluee = itemvalue.Where(x => x.Value.ToLower().Contains( itemvalues.ToLower())).FirstOrDefault();


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


                                    var Contact = _Contact.Find(x => true).ToList();
                                    foreach (var item in Contact)
                                    {
                                        if (item.DynamicFieldList[0].Value == dynamicAttributeViewModels[0].Value)
                                        {
                                            contacts.Add(item);
                                        }
                                    }
                                    return new ApiResponse(contacts, null, Count2);
                                }

                                else
                                {
                                    dynamicAttributeViewModels = dynamicAttributeViewModels.Skip((Filter.PageIndex - 1) * Filter.PageSize)
                                        .Take(Filter.PageSize).ToList();

                                    var Contact2 = _Contact.Find(x => true).ToList();
                                    foreach (var item in Contact2)
                                    {
                                        if (item.DynamicFieldList[0].Value == dynamicAttributeViewModels[0].Value)
                                        {
                                            contacts.Add(item);
                                        }
                                    }

                                    return new ApiResponse(contacts, null, Count2);
                                }
                                var Contact3 = _Contact.Find(x => true).ToList();
                                foreach (var item in Contact3)
                                {
                                    if (item.DynamicFieldList[0].Value == dynamicAttributeViewModels[0].Value)
                                    {
                                        contacts.Add(item);
                                    }
                                }
                                return new ApiResponse(contacts, null, Count2);
                            }
                            if (SimpleFilter.Key.ToLower() == "type")
                            {
                                foreach (var itemvalues in SimpleFilter.Values)
                                {


                                    var allvalue = _Contact.Find(x => true).ToList();
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


                                    var Contact = _Contact.Find(x => true).ToList();
                                    foreach (var item in Contact)
                                    {
                                        if (item.DynamicFieldList[0].Type == dynamicAttributeViewModels[0].Type)
                                        {
                                            contacts.Add(item);
                                        }
                                    }
                                    return new ApiResponse(contacts, null, Count2);
                                }

                                else
                                {
                                    dynamicAttributeViewModels = dynamicAttributeViewModels.Skip((Filter.PageIndex - 1) * Filter.PageSize)
                                        .Take(Filter.PageSize).ToList();

                                    var Contact2 = _Contact.Find(x => true).ToList();
                                    foreach (var item in Contact2)
                                    {
                                        if (item.DynamicFieldList[0].Type == dynamicAttributeViewModels[0].Type)
                                        {
                                            contacts.Add(item);
                                        }
                                    }

                                    return new ApiResponse(contacts, null, Count2);
                                }
                                var Contact3 = _Contact.Find(x => true).ToList();
                                foreach (var item in Contact3)
                                {
                                    if (item.DynamicFieldList[0].Type == dynamicAttributeViewModels[0].Type)
                                    {
                                        contacts.Add(item);
                                    }
                                }
                                return new ApiResponse(contacts, null, Count2);
                            }
                            if (SimpleFilter.Key.ToLower() == "label")
                            {
                                foreach (var itemvalues in SimpleFilter.Values)
                                {


                                    var allvalue = _Contact.Find(x => true).ToList();
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


                                    var Contact = _Contact.Find(x => true).ToList();
                                    foreach (var item in Contact)
                                    {
                                        if (item.DynamicFieldList[0].Label == dynamicAttributeViewModels[0].Label)
                                        {
                                            contacts.Add(item);
                                        }
                                    }
                                    return new ApiResponse(contacts, null, Count2);
                                }

                                else
                                {
                                    dynamicAttributeViewModels = dynamicAttributeViewModels.Skip((Filter.PageIndex - 1) * Filter.PageSize)
                                        .Take(Filter.PageSize).ToList();

                                    var Contact2 = _Contact.Find(x => true).ToList();
                                    foreach (var item in Contact2)
                                    {
                                        if (item.DynamicFieldList[0].Label == dynamicAttributeViewModels[0].Label)
                                        {
                                            contacts.Add(item);
                                        }
                                    }

                                    return new ApiResponse(contacts, null, Count2);
                                }
                                var Contact3 = _Contact.Find(x => true).ToList();
                                foreach (var item in Contact3)
                                {
                                    if (item.DynamicFieldList[0].Label == dynamicAttributeViewModels[0].Label)
                                    {
                                        contacts.Add(item);
                                    }
                                }
                                return new ApiResponse(contacts, null, Count2);
                            }
                      
                        }


                    }
                   
                }
                int Count = contact.Count();

                if (!string.IsNullOrEmpty(Filter.Sort))
                {
                    PropertyInfo? SortProperty = typeof(ContactViewModel).GetProperty(Filter.Sort);

                    if (SortProperty != null && Filter.Order == "asc")
                        contact = contact.OrderBy(x => SortProperty.GetValue(x)).ToList();

                    else if (SortProperty != null && Filter.Order == "desc")
                        contact = contact.OrderByDescending(x => SortProperty.GetValue(x)).ToList();

                    contact = contact.Skip((Filter.PageIndex - 1) * Filter.PageSize)
                        .Take(Filter.PageSize).ToList();

                    return new ApiResponse(res, null, Count);
                }

                else
                {
                    contact = contact.Skip((Filter.PageIndex - 1) * Filter.PageSize)
                        .Take(Filter.PageSize).ToList();

                    return new ApiResponse(contact, null, Count);
                }

                return new ApiResponse(contact, null, Count);
            }


            catch (Exception ex)
            {

                return new ApiResponse(null, ex.Message);
            }

        }
        public ApiResponse EditContact(EditContactViewModel editContactViewModel)
        {
            try
            {
                var ContactId = _Contact.Find(x => x.Id == editContactViewModel.Id).FirstOrDefault();
                var filter = Builders<Contact>.Filter.Eq(x => x.Id, editContactViewModel.Id);

                foreach (var item in editContactViewModel.CompaniesId)
                {
                    var company = _Company.Find(x => x.Id == item).FirstOrDefault();

                }
                foreach (var item in editContactViewModel.DynamicFieldList)
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
                var update = Builders<Contact>.Update
                    .Set(x => x.Name, editContactViewModel.Name)
                    .Set(x => x.Companies, editContactViewModel.CompaniesId)
                     .Set(x => x.DynamicFieldList, editContactViewModel.DynamicFieldList);

                var result = _Contact.UpdateOneAsync(filter, update);
                return new ApiResponse(true, null);
            }
            catch (Exception ex)
            {

             
                return new ApiResponse(false, ex.Message);
            }
         
          
        }
        public ApiResponse GetContactById(string id)
        {
            List<String> Contact = new List<string>();
            var contact = _Contact.Find(x => x.Id == id).FirstOrDefault();
            foreach (var item in contact.Companies)
            {
                var companyname = _Company.Find(x => x.Id == item).FirstOrDefault().Name;
                Contact.Add(companyname);
            }
            if (contact != null)
            {
                ContactViewModel contactViewModel = new ContactViewModel();

                contactViewModel = new ContactViewModel()
                {
                  Id= contact.Id,
                  Name= contact.Name,
                  CompaniesName= Contact,
                  DynamicFieldList= contact.DynamicFieldList,
                  Deleted=contact.Deleted
                };

                return new ApiResponse(contactViewModel, null);

            }

            else
            {
                return new ApiResponse(null, "Not find this Id");

            }


        }
        public ApiResponse DeleteContact(string id)
        {
            var contact = _Contact.Find(x => x.Id == id && x.Deleted==false).FirstOrDefault();
            if (contact != null)
            {
                var filter = Builders<Contact>.Filter.Eq(x => x.Id, id);

                var update = Builders<Contact>.Update
                    .Set(x => x.Deleted, true);
           
                var result = _Contact.UpdateOneAsync(filter, update);
                return new ApiResponse(true, null);

            }
            return new ApiResponse(false, "This Id is not valid");

        }
    }
}
