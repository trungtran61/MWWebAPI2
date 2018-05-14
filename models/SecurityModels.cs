using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MWWebAPI2.Models
{
    public class SecurityModels
    {
        public class UserAuthRequest
        {
            public string userName { get; set; }
            public string password { get; set; }
        }

        public class UserClaim
        {
            public string claimType { get; set; }
            public string claimValue { get; set; }
        }

        public class JWTSettings
        {
            public string key { get; set; }
            public string issuer { get; set; }
            public string audience { get; set; }
            public int minutesToExpiration { get; set; }
        }
        
        public class UserAuth
        {
            public UserAuth() : base()
            {
                userName = "Not Authorized";
                bearerToken = string.Empty;
            }
            public int id { get; set; }
            public string userName { get; set; }
            public string bearerToken { get; set; }
            public bool isAuthenticated { get; set; }
            public string firstName { get; set; }
            public string email { get; set; }
            public List<string> permissions { get; set; }
            public List<UserClaim> claims { get; set; }
        }

        public class User
        {
            public int id { get; set; }
            public string userName { get; set; }
            public string bearerToken { get; set; }
            public string password { get; set; }
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string email { get; set; }
            public string dateCreated { get; set; }
            public bool active { get; set; }
            public List<UserRole> roles { get; set; }
            public string permissions { get; set; }
        }

        public class UserRole
        {
            public string name { get; set; }
            public bool assigned { get; set; }
        }

        public class Role
        {
            public int id { get; set; }
            public string name { get; set; }
            public string displayName { get; set; }
            public bool active { get; set; }
            public List<RolePermission> permissions { get; set; }
        }

        public class RolePermission
        {
            public string name { get; set; }
            public bool assigned { get; set; }
        }

        public class Permission
        {
            public int id { get; set; }
            public string name { get; set; }
            public string displayName { get; set; }
            public bool active { get; set; }
        }

        public class GetListRequest
        {
            public string searchParm { get; set; } = "";
            public string sortColumn { get; set; }
            public string sortDirection { get; set; }
            public int activeOnly { get; set; }
            public int pageSize { get; set; } = 25;
            public int pageNumber { get; set; } = 1;
        }

        public class GetUsersResponse
        {
            public int recordCount { get; set; }
            public List<User> users { get; set; }
        }

        public class GetRolesResponse
        {
            public int recordCount { get; set; }
            public List<Role> roles { get; set; }
        }

        public class GetPermissionsResponse
        {
            public int recordCount { get; set; }
            public List<Permission> permissions { get; set; }
        }

        public class UpdateUserRolesRequest
        {
            public int id { get; set; }
            public string roles { get; set; }
        }

        public class UpdateUserStatusRequest
        {
            public int id { get; set; }
            public bool active { get; set; }
        }
    }
}