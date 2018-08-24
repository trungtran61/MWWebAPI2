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
    [Route("api")]   
    public class AuthController : BaseApiController
    {
        private static string conn;    
        private static AppSettings appSettings;
        DBSecurityRepository securityInventoryRepo = null;
        public AuthController(AppSettings _appSettings)
        {
            appSettings = _appSettings;
            conn = appSettings.SecurityConnectionString;            
            securityInventoryRepo = new DBSecurityRepository(appSettings);
        }        
        
        [Route("ValidateUser")]
        [HttpPost]         
        public IActionResult ValidateUser([FromBody]SecurityModels.UserAuthRequest userAuthRequest)
        {
            SecurityModels.UserAuth userAuth = new SecurityModels.UserAuth();
            userAuth = securityInventoryRepo.ValidateUser(userAuthRequest);

            if (userAuth.isAuthenticated)
                return StatusCode(StatusCodes.Status200OK, userAuth);
            else
                return StatusCode(StatusCodes.Status404NotFound, "Invalid Username/Password.");

        }        
    }
}