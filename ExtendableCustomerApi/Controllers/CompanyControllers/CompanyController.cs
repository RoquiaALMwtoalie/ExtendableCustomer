using ExtendableCustomerApi.IServices;
using ExtendableCustomerApi.IServices.ICompanyServices;
using ExtendableCustomerApi.Model.Filter;
using ExtendableCustomerApi.Model;
using ExtendableCustomerApi.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ExtendableCustomerApi.ViewModel.CompanyViewModel;
using MongoDB.Bson;
using ExtendableCustomerApi.ViewModel.ContactViewModels;
using ExtendableCustomerApi.ViewModel.CompanyViewModels;

namespace ExtendableCustomerApi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {

        private readonly ICompanyService companyService;
        public CompanyController(ICompanyService _companyService)
        {
            companyService = _companyService;
        }

        [HttpPost("AddCompanyWithDynamicField")]
        public ActionResult AddCompanyWithDynamicField(AddCompanyViewModel addCompanyViewModel)
        {
            var EndResult = companyService.AddCompanyWithDynamicField(addCompanyViewModel);
            if (string.IsNullOrEmpty(EndResult.ErrorMessage))
            {
                return Ok(EndResult.Result);

            }
            else
            {
                return BadRequest(new { ErrorMessage = EndResult.ErrorMessage });
            }
        }

        [HttpPost("GetAllCompany")]
        public ActionResult GetAllCompany(ComplexFilter ComplexFilter)
        {
            ApiResponse? Result = companyService.GetALLCompany(ComplexFilter);

            if (string.IsNullOrEmpty(Result.ErrorMessage))
                return Ok(Result);

            else
                return BadRequest(new { ErrorMessage = Result.ErrorMessage });
        }
        [HttpGet("GetCompanyId")]
        public ActionResult GetCompanyId([FromQuery]string id)
        {
            var res = companyService.GetCompanyById(id);
            if (string.IsNullOrEmpty(res.ErrorMessage))
            {
                return Ok(res.Result);
            }
            else
            {
                return BadRequest(new { ErrorMessage = res.ErrorMessage });
            }

        }
        [HttpPut("UpdateCompany/{Id}")]
        public ActionResult UpdateCompany( string Id, [FromBody]EditCompanyViewModel editEmployeeBinding)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            if (Id != editEmployeeBinding.Id)
            {
                return BadRequest();
            }
            var res = companyService.EditCompany(editEmployeeBinding);

            if (string.IsNullOrEmpty(res.ErrorMessage))
            {
                return Ok(res.Result);

            }
            else
            {

                return BadRequest(new { ErrorMessage = res.ErrorMessage });

            }
        }
        [HttpDelete("DeleteCompany")]
 
        public ActionResult DeleteCompany(string id)
        {
            var res = companyService.DeleteCompany(id);
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
