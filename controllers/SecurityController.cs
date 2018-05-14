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
    [Route("api/security")]
    [Authorize]
    public class SecurityController : BaseApiController
    {
        private static string conn;    
        public SecurityController(AppSettings _appSettings)
        {
            appSettings = _appSettings;
            conn = appSettings.SecurityConnectionString;            
            securityInventoryRepo = new DBSecurityRepository(appSettings);
        }
        private static AppSettings appSettings;
        DBSecurityRepository securityInventoryRepo = null;

        [Route("GetUsers")]
        [HttpGet]
        public IActionResult GetUsers([FromQuery]SecurityModels.GetListRequest getUsersRequest)
        {
            SecurityModels.GetUsersResponse getUserResponse = securityInventoryRepo.GetUsers(getUsersRequest);
            return StatusCode(StatusCodes.Status200OK, getUserResponse);
        }

        [Route("GetUser")]
        [HttpGet]
        public IActionResult GetUser(int id)
        {
            SecurityModels.User user = securityInventoryRepo.GetUser(id);
            return StatusCode(StatusCodes.Status200OK, user);
        }

        [Route("GetRoles")]
        [HttpGet]
        public IActionResult GetRoles([FromQuery]SecurityModels.GetListRequest getRolesRequest)
        {
            SecurityModels.GetRolesResponse getRolesResponse = securityInventoryRepo.GetRoles(getRolesRequest);
            return StatusCode(StatusCodes.Status200OK, getRolesResponse);
        }

        [Route("GetRole")]
        [HttpGet]
        public IActionResult GetRole(int id)
        {
            SecurityModels.Role role = securityInventoryRepo.GetRole(id);
            return StatusCode(StatusCodes.Status200OK, role);
        }

        [Route("GetPermission")]
        [HttpGet]
        public IActionResult GetPermission(int id)
        {
            SecurityModels.Permission permission = securityInventoryRepo.GetPermission(id);
            return StatusCode(StatusCodes.Status200OK, permission);
        }

        [Route("GetPermissions")]
        [HttpGet]
        public IActionResult GetPermissions([FromQuery]SecurityModels.GetListRequest getPermissionsRequest)
        {
            SecurityModels.GetPermissionsResponse getPermissionsResponse = securityInventoryRepo.GetPermissions(getPermissionsRequest);
            return StatusCode(StatusCodes.Status200OK, getPermissionsResponse);
        }

        [Route("UpdateUserRoles")]
        [HttpPost]
        public IActionResult UpdateUserRoles([FromBody]SecurityModels.UpdateUserRolesRequest updateUserRolesRequest)
        {
            securityInventoryRepo.UpdateUserRoles(updateUserRolesRequest);
            return StatusCode(StatusCodes.Status200OK, "");
        }

        [Route("UpdateUserStatus")]
        [HttpPost]
        public IActionResult UpdateUserStatus([FromBody]SecurityModels.UpdateUserStatusRequest updateUserStatusRequest)
        {
            securityInventoryRepo.UpdateUserStatus(updateUserStatusRequest);
            return StatusCode(StatusCodes.Status200OK, "");
        }

        [Route("UpdateUserProfile")]
        [HttpPost]
        public IActionResult UpdateUserProfile([FromBody]SecurityModels.User user)
        {
            user.id = securityInventoryRepo.UpdateUserProfile(user);
            return StatusCode(StatusCodes.Status200OK, user.id);
        }

        [Route("UpdateRole")]
        [HttpPost]
        public IActionResult UpdateRole([FromBody]SecurityModels.Role role)
        {
            try
            {
                role.id = securityInventoryRepo.UpdateRole(role);
                return StatusCode(StatusCodes.Status200OK, role.id);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
        }

        [Route("UpdateRoleStatus")]
        [HttpPost]
        public IActionResult UpdateRoleStatus([FromBody]SecurityModels.Role permission)
        {
            securityInventoryRepo.UpdateRoleStatus(permission);
            return StatusCode(StatusCodes.Status200OK, permission);
        }

        [Route("UpdatePermission")]
        [HttpPost]
        public IActionResult UpdatePermission([FromBody]SecurityModels.Permission permission)
        {
            permission.id = securityInventoryRepo.UpdatePermission(permission);
            return StatusCode(StatusCodes.Status200OK, permission.id);
        }

        [Route("UpdatePermissionStatus")]
        [HttpPost]
        public IActionResult UpdatePermissionStatus([FromBody]SecurityModels.Permission permission)
        {
            securityInventoryRepo.UpdatePermissionStatus(permission);
            return StatusCode(StatusCodes.Status200OK, permission);
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