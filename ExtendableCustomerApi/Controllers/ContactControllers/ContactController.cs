using ExtendableCustomerApi.IServices.ICompanyServices;
using ExtendableCustomerApi.IServices.IContactServices;
using ExtendableCustomerApi.Model.Filter;
using ExtendableCustomerApi.Model;
using ExtendableCustomerApi.Services;
using ExtendableCustomerApi.ViewModel;
using ExtendableCustomerApi.ViewModel.CompanyViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ExtendableCustomerApi.ViewModel.CompanyViewModels;
using MongoDB.Bson;
using ExtendableCustomerApi.ViewModel.ContactViewModels;

namespace ExtendableCustomerApi.Controllers.ContactControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IContactService contactService;
        public ContactController(IContactService _contactService)
        {
            contactService = _contactService;
        }

        [HttpPost("AddContactWithDynamicField")]
        public ActionResult AddContactWithDynamicField(AddContactViewModel addContactViewModel)
        {
            var EndResult = contactService.AddContactWithDynamicField(addContactViewModel);
            if (string.IsNullOrEmpty(EndResult.ErrorMessage))
            {
                return Ok(EndResult.Result);

            }
            else
            {
                return BadRequest(new { ErrorMessage = EndResult.ErrorMessage });
            }
        }

        [HttpPost("GetAllContact")]
        public ActionResult GetAllContact(ComplexFilter ComplexFilter)
        {
            ApiResponse? Result = contactService.GetAllContact(ComplexFilter);

            if (string.IsNullOrEmpty(Result.ErrorMessage))
                return Ok(Result);

            else
                return BadRequest(new { ErrorMessage = Result.ErrorMessage });
        }
        [HttpGet("GetContactId")]
        public ActionResult GetCompanyId(string id)
        {
            var res = contactService.GetContactById(id);
            if (string.IsNullOrEmpty(res.ErrorMessage))
            {
                return Ok(res.Result);
            }
            else
            {
                return BadRequest(new { ErrorMessage = res.ErrorMessage });
            }

        }
        [HttpPut("UpdateContact/{Id}")]

        public ActionResult UpdateContact(string Id, [FromBody] EditContactViewModel editEmployeeBinding)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            if (Id != editEmployeeBinding.Id)
            {
                return BadRequest();
            }
            var res = contactService.EditContact(editEmployeeBinding);

            if (string.IsNullOrEmpty(res.ErrorMessage))
            {
                return Ok(res.Result);

            }
            else
            {

                return BadRequest(new { ErrorMessage = res.ErrorMessage });

            }
        }
        [HttpDelete("DeleteContact")]

        public ActionResult DeleteContact(string id)
        {
            var res = contactService.DeleteContact(id);
            if (string.IsNullOrEmpty(res.ErrorMessage))
            {
                return Ok(res.Result);
            }
            else
            {
                return BadRequest(new { ErrorMessage = res.ErrorMessage });

            }
        }

    }
}

