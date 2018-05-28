using MWWebAPI2.DBRepository;
using MWWebAPI2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MWWebAPI2.Controllers
{
    [Route("api/organizations")]
    //[Authorize]
    public class OrganizationsController : BaseApiController
    {
        private static string conn;    
        public OrganizationsController(AppSettings _appSettings)
        {
            appSettings = _appSettings;
            conn = appSettings.SecurityConnectionString;            
            organizationsRepo = new DBOrganizationsRepository(appSettings);
        }
        private static AppSettings appSettings;
        DBOrganizationsRepository organizationsRepo = null;

        [Route("GetOrganizations")]
        [HttpGet]
        public IActionResult GetOrganizations([FromQuery]OrganizationModels.GetOrganizationsRequest getOrganizationsRequest)
        {
            OrganizationModels.GetOrganizationsResponse getOrganizationResponse = organizationsRepo.GetOrganizations(getOrganizationsRequest);
            return StatusCode(StatusCodes.Status200OK, getOrganizationResponse);
        }

        [Route("GetOrganization")]
        [HttpGet]
        public IActionResult GetOrganization(int id, string type)
        {
            OrganizationModels.Organization org = organizationsRepo.GetOrganization(id, type);
            return StatusCode(StatusCodes.Status200OK, org);
        }
        
        [Route("UpdateOrganizationStatus")]
        [HttpPost]
        public IActionResult UpdateOrganizationStatus([FromBody]OrganizationModels.Organization organization)
        {
            organizationsRepo.UpdateOrganizationStatus(organization);
            return StatusCode(StatusCodes.Status200OK, organization);
        }        
        
    }
}