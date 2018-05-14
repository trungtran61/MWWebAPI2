using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MWWebAPI2.Models;
using static MWWebAPI2.Models.SecurityModels;

namespace MWWebAPI2.DBRepository
{
    public class DBSecurityRepository : DBRepositoryBase, IDisposable
    {
        private AppSettings appSettings;
        public DBSecurityRepository(AppSettings _appSettings)
        {
            appSettings = _appSettings;
            securityConnectionString = appSettings.SecurityConnectionString;
        }
        private static string securityConnectionString;
        public UserAuth ValidateUser(UserAuthRequest userAuthRequest)
        {
            User user = new User();
            UserAuth userAuth = new UserAuth();

            using (SqlConnection con = new SqlConnection(securityConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("ValidateUser", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 20).Value = userAuthRequest.userName;
                    cmd.Parameters.Add("@Password", SqlDbType.VarChar, 20).Value = userAuthRequest.password;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        user.id = Convert.ToInt16(reader["Id"].ToString());
                        user.userName = reader["UserName"].ToString();
                        user.firstName = reader["firstName"].ToString();
                        user.email = reader["Email"].ToString();
                        user.permissions = reader["Permissions"].ToString();
                    }
                }
                con.Close();
            }

            if (user != null)
            {
                // Build User Security Object
                userAuth = BuildUserAuthObject(user);
            }
            return userAuth;
        }

        public SecurityModels.GetUsersResponse GetUsers(SecurityModels.GetListRequest getUsersRequest)
        {
            SecurityModels.GetUsersResponse getUsersResponse = new SecurityModels.GetUsersResponse();
            List<SecurityModels.User> Users = new List<SecurityModels.User>();

            using (SqlConnection con = new SqlConnection(securityConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetUsers", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@searchParm", SqlDbType.VarChar, 30).Value = getUsersRequest.searchParm;
                    cmd.Parameters.Add("@pageSize", SqlDbType.Int, 0).Value = getUsersRequest.pageSize;
                    cmd.Parameters.Add("@pageNumber", SqlDbType.Int, 0).Value = getUsersRequest.pageNumber;

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    int recordNumber = 1;

                    while (reader.Read())
                    {
                        if (recordNumber == 1)
                            getUsersResponse.recordCount = Convert.ToInt32(reader["recordCount"].ToString());

                        Users.Add(new SecurityModels.User
                        {
                            id = Convert.ToInt32(reader["ID"].ToString()),
                            userName = reader["UserName"].ToString(),
                            firstName = reader["firstName"].ToString(),
                            lastName = reader["lastName"].ToString(),
                            email = reader["Email"].ToString(),
                            active = Convert.ToBoolean(reader["Active"].ToString())
                        });
                    }
                    getUsersResponse.users = Users;
                }
                con.Close();
            }
            return getUsersResponse;
        }

        public SecurityModels.User GetUser(int id)
        {
            SecurityModels.User user = new SecurityModels.User();

            using (SqlConnection con = new SqlConnection(securityConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetUser", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        user.id = Convert.ToInt32(reader["ID"].ToString());
                        user.userName = reader["UserName"].ToString();
                        user.password = reader["Password"].ToString();
                        user.firstName = reader["firstName"].ToString();
                        user.lastName = reader["lastName"].ToString();
                        user.email = reader["Email"].ToString();
                        user.active = id == 0 ? false : Convert.ToBoolean(reader["Active"].ToString());
                        user.roles = GetUserRoles(reader["Roles"].ToString());
                        user.permissions = reader["Permissions"].ToString();
                    }
                }
                con.Close();
            }
            return user;
        }

        public SecurityModels.Role GetRole(int id)
        {
            SecurityModels.Role role = new SecurityModels.Role();

            using (SqlConnection con = new SqlConnection(securityConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetRole", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        role.id = Convert.ToInt32(reader["ID"].ToString());
                        role.name = reader["Name"].ToString();
                        role.displayName = reader["displayName"].ToString();
                        role.active = id == 0 ? false : Convert.ToBoolean(reader["Active"].ToString());
                        role.permissions = GetRolePermissions(reader["Permissions"].ToString());

                    }
                }
                con.Close();
            }
            return role;
        }

        public SecurityModels.Permission GetPermission(int id)
        {
            SecurityModels.Permission permission = new SecurityModels.Permission();

            using (SqlConnection con = new SqlConnection(securityConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetPermission", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        permission.id = Convert.ToInt32(reader["ID"].ToString());
                        permission.name = reader["Name"].ToString();
                        permission.displayName = reader["displayName"].ToString();
                        permission.active = id == 0 ? false : Convert.ToBoolean(reader["Active"].ToString());

                    }
                }
                con.Close();
            }
            return permission;
        }

        private List<SecurityModels.UserRole> GetUserRoles(string roles)
        {
            List<SecurityModels.UserRole> userRoles = new List<SecurityModels.UserRole>();
            List<string> lstRoles = roles.Split(',').ToList();

            foreach (string role in lstRoles)
            {
                userRoles.Add(
                    new SecurityModels.UserRole
                    {
                        name = role.Substring(0, role.IndexOf(':')),
                        assigned = role.Substring(role.IndexOf(':') + 1) == "1"
                    }
                    );
            }

            return userRoles;
        }

        private List<SecurityModels.RolePermission> GetRolePermissions(string permissions)
        {
            List<SecurityModels.RolePermission> rolePermissions = new List<SecurityModels.RolePermission>();
            List<string> lstPermisions = permissions.Split(',').ToList();

            foreach (string permision in lstPermisions)
            {
                rolePermissions.Add(
                    new SecurityModels.RolePermission
                    {
                        name = permision.Substring(0, permision.IndexOf(':')),
                        assigned = permision.Substring(permision.IndexOf(':') + 1) == "1"
                    }
                    );
            }

            return rolePermissions;
        }

        public SecurityModels.GetRolesResponse GetRoles(SecurityModels.GetListRequest getRolesRequest)
        {
            SecurityModels.GetRolesResponse getRolesResponse = new SecurityModels.GetRolesResponse();
            List<SecurityModels.Role> Roles = new List<SecurityModels.Role>();

            using (SqlConnection con = new SqlConnection(securityConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetRoles", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@searchParm", SqlDbType.VarChar, 30).Value = getRolesRequest.searchParm;
                    cmd.Parameters.Add("@pageSize", SqlDbType.Int, 0).Value = getRolesRequest.pageSize;
                    cmd.Parameters.Add("@pageNumber", SqlDbType.Int, 0).Value = getRolesRequest.pageNumber;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    int recordNumber = 1;

                    while (reader.Read())
                    {
                        if (recordNumber == 1)
                            getRolesResponse.recordCount = Convert.ToInt32(reader["recordCount"].ToString());

                        recordNumber++;

                        Roles.Add(new SecurityModels.Role
                        {
                            id = Convert.ToInt32(reader["ID"].ToString()),
                            name = reader["Name"].ToString(),
                            displayName = reader["displayName"].ToString(),
                            active = Convert.ToBoolean(reader["Active"].ToString())
                        });
                    }
                    getRolesResponse.roles = Roles;
                }
                con.Close();
            }
            return getRolesResponse;
        }

        public SecurityModels.GetPermissionsResponse GetPermissions(SecurityModels.GetListRequest getPermissionsRequest)
        {
            SecurityModels.GetPermissionsResponse getPermissionsResponse = new SecurityModels.GetPermissionsResponse();
            List<SecurityModels.Permission> Permissions = new List<SecurityModels.Permission>();

            using (SqlConnection con = new SqlConnection(securityConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetPermissions", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@searchParm", SqlDbType.VarChar, 30).Value = getPermissionsRequest.searchParm;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    int recordNumber = 1;

                    while (reader.Read())
                    {
                        if (recordNumber == 1)
                            getPermissionsResponse.recordCount = Convert.ToInt32(reader["recordCount"].ToString());

                        Permissions.Add(new SecurityModels.Permission
                        {
                            id = Convert.ToInt32(reader["ID"].ToString()),
                            name = reader["Name"].ToString(),
                            displayName = reader["displayName"].ToString(),
                            active = Convert.ToBoolean(reader["Active"].ToString())
                        });
                    }
                    getPermissionsResponse.permissions = Permissions;
                }
                con.Close();
            }
            return getPermissionsResponse;
        }

        public void UpdateUserRoles(SecurityModels.UpdateUserRolesRequest updateUserRolesRequest)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(securityConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UpdateUserRoles", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = updateUserRolesRequest.id;
                        cmd.Parameters.Add("@Roles", SqlDbType.VarChar).Value = updateUserRolesRequest.roles;
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
            }
            catch
            {
                throw;
            }
        }

        public void UpdateUserStatus(SecurityModels.UpdateUserStatusRequest updateUserStatusRequest)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(securityConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UpdateUserStatus", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = updateUserStatusRequest.id;
                        cmd.Parameters.Add("@Active", SqlDbType.Bit).Value = updateUserStatusRequest.active;
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
            }
            catch
            {
                throw;
            }
        }

        public int UpdateUserProfile(SecurityModels.User user)
        {
            DataTable tblRoles = new DataTable();
            tblRoles.Columns.Add("Name", typeof(string));
            tblRoles.Columns.Add("assigned", typeof(bool));

            if (user.roles != null)
            {
                foreach (SecurityModels.UserRole userRole in user.roles)
                {
                    if (userRole.assigned)
                    {
                        DataRow row = tblRoles.NewRow();
                        row["Name"] = userRole.name;
                        row["assigned"] = userRole.assigned;
                        tblRoles.Rows.Add(row);
                    }
                }
            }

            try
            {
                using (SqlConnection con = new SqlConnection(securityConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UpdateUserProfile", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = user.id;
                        cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 20).Value = user.userName;
                        cmd.Parameters.Add("@Password", SqlDbType.VarChar, 20).Value = user.password;
                        cmd.Parameters.Add("@Active", SqlDbType.Bit).Value = user.active ? 1 : 0;
                        cmd.Parameters.Add("@firstName", SqlDbType.VarChar, 50).Value = user.firstName;
                        cmd.Parameters.Add("@lastName", SqlDbType.VarChar, 50).Value = user.lastName;
                        cmd.Parameters.Add("@Email", SqlDbType.VarChar, 100).Value = user.email;
                        cmd.Parameters.Add("@Roles", SqlDbType.Structured, 0).Value = tblRoles;
                        con.Open();
                        user.id = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    con.Close();
                }
            }
            catch
            {
                throw;
            }

            return user.id;
        }

        public int UpdateRole(SecurityModels.Role role)
        {
            DataTable tblPermissions = new DataTable();
            tblPermissions.Columns.Add("Name", typeof(string));
            tblPermissions.Columns.Add("assigned", typeof(bool));

            if (role.permissions != null)
            {
                foreach (SecurityModels.RolePermission permission in role.permissions)
                {
                    if (permission.assigned)
                    {
                        DataRow row = tblPermissions.NewRow();
                        row["Name"] = permission.name;
                        row["assigned"] = permission.assigned;
                        tblPermissions.Rows.Add(row);
                    }
                }
            }

            try
            {
                using (SqlConnection con = new SqlConnection(securityConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UpdateRole", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = role.id;
                        cmd.Parameters.Add("@Name", SqlDbType.VarChar, 50).Value = role.name;
                        cmd.Parameters.Add("@displayName", SqlDbType.VarChar, 50).Value = role.displayName;
                        cmd.Parameters.Add("@Active", SqlDbType.Bit).Value = role.active ? 1 : 0;
                        cmd.Parameters.Add("@Permissions", SqlDbType.Structured, 0).Value = tblPermissions;
                        con.Open();
                        role.id = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    con.Close();
                }
            }
            catch
            {
                throw;
            }

            return role.id;
        }

        public void UpdateRoleStatus(SecurityModels.Role role)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(securityConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UpdateRoleStatus", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = role.id;
                        cmd.Parameters.Add("@Active", SqlDbType.Bit).Value = role.active;
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
            }
            catch
            {
                throw;
            }
        }

        public int UpdatePermission(SecurityModels.Permission permission)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(securityConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UpdatePermission", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = permission.id;
                        cmd.Parameters.Add("@Name", SqlDbType.VarChar, 50).Value = permission.name;
                        cmd.Parameters.Add("@displayName", SqlDbType.VarChar, 50).Value = permission.displayName;
                        cmd.Parameters.Add("@Active", SqlDbType.Bit).Value = permission.active ? 1 : 0;
                        con.Open();
                        permission.id = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    con.Close();
                }
            }
            catch
            {
                throw;
            }

            return permission.id;
        }

        public void UpdatePermissionStatus(SecurityModels.Permission permission)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(securityConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UpdatePermissionStatus", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = permission.id;
                        cmd.Parameters.Add("@Active", SqlDbType.Bit).Value = permission.active;
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
            }
            catch
            {
                throw;
            }
        }

        protected List<UserClaim> GetUserClaims(User user)
        {
            List<UserClaim> userClaims = new List<UserClaim>();
            List<string> lstPermissions = user.permissions.Split(',').ToList();

            foreach (string permision in lstPermissions)
            {
                userClaims.Add(
                    new UserClaim
                    {
                        claimType = permision,
                        claimValue = "True"
                    }
                );
            }

            return userClaims;
        }

        protected UserAuth BuildUserAuthObject(User authUser)
        {
            UserAuth ret = new UserAuth();
            List<UserClaim> claims = new List<UserClaim>();
            ret.id = authUser.id;
            ret.userName = authUser.userName;
            ret.firstName = authUser.firstName;
            ret.isAuthenticated = true;
            ret.claims = GetUserClaims(authUser);
            ret.bearerToken = BuildJwtToken(ret);
            return ret;
        }
        protected string BuildJwtToken(UserAuth authUser)
        {
            SymmetricSecurityKey key = new SymmetricSecurityKey(
              Encoding.UTF8.GetBytes(appSettings.JWTKey));

            // Create standard JWT claims
            List<Claim> jwtClaims = new List<Claim>();
            jwtClaims.Add(new Claim(JwtRegisteredClaimNames.Sub, authUser.userName));
            jwtClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            // Add custom claims
            foreach (var claim in authUser.claims)
            {
                jwtClaims.Add(new Claim(claim.claimType, claim.claimValue));
            }

            // Create the JwtSecurityToken object
            var token = new JwtSecurityToken(
              issuer: appSettings.JWTIssuer,
              audience: appSettings.JWTAudience,
              claims: jwtClaims,
              notBefore: DateTime.UtcNow,
              expires: DateTime.UtcNow.AddMinutes(
                  appSettings.JWTMinutesToExpiration),
              signingCredentials: new SigningCredentials(key,
                          SecurityAlgorithms.HmacSha256)
            );

            // Create a string representation of the Jwt token
            return new JwtSecurityTokenHandler().WriteToken(token); ;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}