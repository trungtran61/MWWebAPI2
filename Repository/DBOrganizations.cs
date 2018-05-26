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
using static MWWebAPI2.Models.OrganizationModels;

namespace MWWebAPI2.DBRepository
{
    public class DBOrganizationsRepository : DBRepositoryBase, IDisposable
    {
        private AppSettings appSettings;
        public DBOrganizationsRepository(AppSettings _appSettings)
        {
            appSettings = _appSettings;
            MWConnectionString = appSettings.MWConnectionString;
        }
        private static string MWConnectionString;
        public GetOrganizationsResponse GetOrganizations(GetOrganizationsRequest getOrganizationsRequest)
        {
            GetOrganizationsResponse getOrganizationsResponse = new GetOrganizationsResponse();
            List<Organization> organizations = new List<Organization>();

            using (SqlConnection con = new SqlConnection(MWConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetUsers", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@nameParm", SqlDbType.VarChar, 50).Value = getOrganizationsRequest.nameParm;
                    cmd.Parameters.Add("@typeParm", SqlDbType.VarChar, 20).Value = getOrganizationsRequest.typeParm;
                    cmd.Parameters.Add("@addressParm", SqlDbType.VarChar, 50).Value = getOrganizationsRequest.addressParm;
                    cmd.Parameters.Add("@pageSize", SqlDbType.Int, 0).Value = getOrganizationsRequest.pageSize;
                    cmd.Parameters.Add("@pageNumber", SqlDbType.Int, 0).Value = getOrganizationsRequest.pageNumber;

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    int recordNumber = 1;

                    while (reader.Read())
                    {
                        if (recordNumber == 1)
                            getOrganizationsResponse.recordCount = Convert.ToInt32(reader["recordCount"].ToString());

                        organizations.Add(new Organization
                        {
                            id = Convert.ToInt32(reader["ID"].ToString()),
                            type = reader["Type"].ToString(),
                            name = reader["Name"].ToString(),
                            phone = reader["Phone"].ToString(),
                            fax = reader["Fax"].ToString(),
                            email = reader["Email"].ToString(),
                            active = Convert.ToBoolean(reader["Active"].ToString())
                        });
                    }
                    getOrganizationsResponse.organizations = organizations;
                }
                con.Close();
            }
            return getOrganizationsResponse;
        }

        public Organization GetOrganization(int id, string type)
        {
            Organization org = new Organization();

            using (SqlConnection con = new SqlConnection(MWConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetOrganization", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                    cmd.Parameters.Add("@Type", SqlDbType.VarChar, 20).Value = type;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        org.id = Convert.ToInt32(reader["ID"].ToString());
                        org.type = reader["Type"].ToString();
                        org.name = reader["Name"].ToString();
                        org.address = reader["Address"].ToString();
                        org.city = reader["City"].ToString();
                        org.state = reader["State"].ToString();
                        org.zip = reader["Zip"].ToString();
                        org.phone = reader["Phone"].ToString();
                        org.fax = reader["Fax"].ToString();
                        org.tollfree = reader["TollFree"].ToString();
                        org.email = reader["Email"].ToString();
                        org.website = reader["WebSite"].ToString();
                        org.active = id == 0 ? false : Convert.ToBoolean(reader["Active"].ToString());
                    }
                }
                con.Close();
            }
            return org;
        }


        public void UpdateOrganizationStatus(Organization org)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(MWConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("UpdateOrganizationStatus", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = org.id;
                        cmd.Parameters.Add("@Type", SqlDbType.Int).Value = org.type;
                        cmd.Parameters.Add("@Active", SqlDbType.Bit).Value = org.active;
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


        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}