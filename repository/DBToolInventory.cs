using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.IO;
using MWWebAPI2.Models;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;

namespace MWWebAPI2.DBRepository
{
    public class DBToolInventoryRepository : IDisposable
    {
        private readonly IConfiguration Configuration;
        private static AppSettings appSettings = new AppSettings();
        private static IMemoryCache cache;
        private static IHostingEnvironment hostingEnvironment;
        public DBToolInventoryRepository(AppSettings _appSettings,
            IHostingEnvironment _hostingEnvironment,
            IMemoryCache _cache,
            IConfiguration configuration)
        {
            appSettings = _appSettings;
            hostingEnvironment = _hostingEnvironment;
            cache = _cache;
            Configuration = configuration;
        }

        public DBToolInventoryRepository()
        {            
        }
        //private static string imageLibrary = ConfigurationManager.AppSettings["imageLibrary"];
        private static string imageUrl = appSettings.ImageUrl;
        public string[] GetCuttingMethodTemplate(string cuttingMethod)
        {
            List<string> retTemplate = new List<string>();

            using (SqlConnection con = new SqlConnection(appSettings.MWConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("spGetCuttingMethodTemplate", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@CuttingMethod", SqlDbType.VarChar, 50).Value = cuttingMethod;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string Template = reader["Template"].ToString();
                        string[] lines = Template.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.None);

                        StringBuilder sbLines = new StringBuilder();
                        for (int i = 0; i < lines.Count(); i++)
                        {
                            sbLines.AppendLine(lines[i]);
                        }
                        retTemplate.Add(sbLines.ToString());
                    }
                }
                con.Close();
            }
            return retTemplate.ToArray();
        }

        public DBResponse UpdateCuttingMethodTemplate(CuttingMethodTemplate cuttingMethodTemplate)
        {
            DBResponse dbResponse = new DBResponse();

            try
            {
                using (SqlConnection con = new SqlConnection(appSettings.MWConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("spUpdateCuttingMethodTemplate", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@CuttingMethod", SqlDbType.VarChar, 50).Value = cuttingMethodTemplate.CuttingMethod;
                        cmd.Parameters.Add("@Template", SqlDbType.VarChar).Value = cuttingMethodTemplate.Template;
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        dbResponse.RecordsAffected = rowsAffected;

                        if (rowsAffected > 0)
                        {
                            dbResponse.ReturnCode = 0;
                        }
                        else
                        {
                            dbResponse.ReturnCode = -1;
                            dbResponse.Message = "No update";
                        }
                    }
                    con.Close();
                }
            }
            catch
            {
                dbResponse.ReturnCode = -1;
                throw;
            }

            return dbResponse;
        }

        public List<CuttingMethodTemplate> GetCuttingMethodsWithTemplate(string term)
        {
            List<CuttingMethodTemplate> cuttingMethodTemplates = new List<CuttingMethodTemplate>();

            using (SqlConnection conn = new SqlConnection(appSettings.MWConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "spGetCuttingMethodsWithTemplate";
                    cmd.Parameters.Add("@searchterm", SqlDbType.VarChar, 50).Value = term;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    conn.Open();

                    using (SqlDataReader rdLookup = cmd.ExecuteReader())
                    {
                        while (rdLookup.Read())
                        {
                            CuttingMethodTemplate cuttingMethodTemplate = new CuttingMethodTemplate
                            {
                                CuttingMethod = rdLookup["CuttingMethod"].ToString(),
                                Template = rdLookup["Template"].ToString()
                            };
                            cuttingMethodTemplates.Add(cuttingMethodTemplate);
                        }
                    }
                }
            }
            return cuttingMethodTemplates;
        }
        public List<Lookup> GetLookupByCategory(string category, string term="")
        {
            if (term == null)            
                term  = string.Empty;
                
            List<Lookup> lookups = new List<Lookup>();

            using (SqlConnection conn = new SqlConnection(appSettings.MWConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "spGetLookUp";
                    cmd.Parameters.Add("@category", SqlDbType.VarChar, 50).Value = category;
                    cmd.Parameters.Add("@searchterm", SqlDbType.VarChar, 50).Value = term;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    conn.Open();

                    using (SqlDataReader rdLookup = cmd.ExecuteReader())
                    {
                        while (rdLookup.Read())
                        {
                            Lookup lookup = new Lookup
                            {
                                Id = Convert.ToInt32(rdLookup["HID"].ToString()),
                                Text = rdLookup["mText"].ToString(),
                                Value = rdLookup["mValue"].ToString(),
                                name = rdLookup["mText"].ToString(),
                                id = rdLookup["mValue"].ToString()
                            };
                            lookups.Add(lookup);
                        }
                    }
                }
            }
            return lookups;
        }

        //GetToolCategoryNames
        public List<Lookup> GetToolCategoryNames()
        {
            List<Lookup> lookups = new List<Lookup>();

            using (SqlConnection conn = new SqlConnection(appSettings.MWConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "GetToolCategoryNames";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    conn.Open();

                    using (SqlDataReader rdLookup = cmd.ExecuteReader())
                    {
                        while (rdLookup.Read())
                        {
                            Lookup lookup = new Lookup
                            {
                                Text = rdLookup["Name"].ToString(),
                                Value = rdLookup["ID"].ToString()
                            };
                            lookups.Add(lookup);
                        }
                    }
                }
            }
            return lookups;
        }

        public List<ToolInventoryColumn> GetToolInventoryColumns()
        {
            List<ToolInventoryColumn> toolInventoryColumns = new List<ToolInventoryColumn>();
            using (SqlConnection conn = new SqlConnection(appSettings.MWConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "GetToolInventoryColumns";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ToolInventoryColumn toolInventoryColumn = new ToolInventoryColumn
                            {
                                Name = reader["ColumnName"].ToString(),
                                Header = reader["ColumnHeader"].ToString(),
                                Searchable = Convert.ToBoolean(reader["Searchable"].ToString()),
                                Sequence = Convert.ToInt16(reader["Sequence"].ToString()),
                                RelatedTable = reader["RelatedTable"].ToString(),
                                RelatedIDField = reader["RelatedIDField"].ToString(),
                                RelatedTextField = reader["RelatedTextField"].ToString()
                            };
                            toolInventoryColumns.Add(toolInventoryColumn);
                        }
                    }
                }
            }
            return toolInventoryColumns;
        }

        public DBResponse CheckOutCheckIn(CheckOutCheckInRequest checkOutCheckInRequest)
        {
            DBResponse dbResponse = new DBResponse();
            StringBuilder sbItemQty = new StringBuilder();

            try
            {
                using (SqlConnection con = new SqlConnection(appSettings.MWConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("CheckOutCheckIn", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Action", SqlDbType.VarChar, 10).Value = checkOutCheckInRequest.Action;
                        cmd.Parameters.Add("@ModifiedBy", SqlDbType.VarChar, 30).Value = checkOutCheckInRequest.ModifiedBy;

                        foreach (CheckOutCheckInItem item in checkOutCheckInRequest.CheckOutCheckInItems)
                        {
                            sbItemQty.AppendFormat("{0}:{1},", item.ID, item.Qty);
                        }

                        cmd.Parameters.Add("@Items_Qtys", SqlDbType.VarChar).Value = sbItemQty.Remove(sbItemQty.Length - 1, 1).ToString();
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        dbResponse.RecordsAffected = rowsAffected;

                        if (rowsAffected > 0)
                        {
                            dbResponse.ReturnCode = 0;
                        }
                        else
                        {
                            dbResponse.ReturnCode = -1;
                            dbResponse.Message = "No update";
                        }
                    }
                    con.Close();
                }
            }
            catch
            {
                dbResponse.ReturnCode = -1;
                throw;
            }

            return dbResponse;
        }

        public DBResponse LinkTool(LinkToolRequest linkToolRequest)
        {
            DBResponse dbResponse = new DBResponse();
            StringBuilder sbChildIDs = new StringBuilder();

            try
            {
                using (SqlConnection con = new SqlConnection(appSettings.MWConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("LinkTool", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Action", SqlDbType.VarChar, 10).Value = linkToolRequest.Action;
                        cmd.Parameters.Add("@ParentID", SqlDbType.Int).Value = linkToolRequest.ParentID;

                        foreach (int childID in linkToolRequest.ChildIDs)
                        {
                            sbChildIDs.AppendFormat("{0},", childID);
                        }

                        cmd.Parameters.Add("@ChildIDs", SqlDbType.VarChar).Value = sbChildIDs.Remove(sbChildIDs.Length - 1, 1).ToString();
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        dbResponse.RecordsAffected = rowsAffected;

                        if (rowsAffected > 0)
                        {
                            dbResponse.ReturnCode = 0;
                        }
                        else
                        {
                            dbResponse.ReturnCode = -1;
                            dbResponse.Message = "No update";
                        }
                    }
                    con.Close();
                }
            }
            catch
            {
                dbResponse.ReturnCode = -1;
                throw;
            }

            return dbResponse;
        }

        public DBResponse AttachVendor(LinkToolRequest linkToolRequest)
        {
            DBResponse dbResponse = new DBResponse();
            StringBuilder sbChildIDs = new StringBuilder();

            try
            {
                using (SqlConnection con = new SqlConnection(appSettings.MWConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("LinkTool", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Action", SqlDbType.VarChar, 10).Value = linkToolRequest.Action;
                        cmd.Parameters.Add("@ParentID", SqlDbType.Int).Value = linkToolRequest.ParentID;

                        foreach (int childID in linkToolRequest.ChildIDs)
                        {
                            sbChildIDs.AppendFormat("{0},", childID);
                        }

                        cmd.Parameters.Add("@ChildIDs", SqlDbType.VarChar).Value = sbChildIDs.Remove(sbChildIDs.Length - 1, 1).ToString();
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        dbResponse.RecordsAffected = rowsAffected;

                        if (rowsAffected > 0)
                        {
                            dbResponse.ReturnCode = 0;
                        }
                        else
                        {
                            dbResponse.ReturnCode = -1;
                            dbResponse.Message = "No update";
                        }
                    }
                    con.Close();
                }
            }
            catch
            {
                dbResponse.ReturnCode = -1;
                throw;
            }

            return dbResponse;
        }

        public int UpdateToolVendor(ToolInventorySearchResult toolInventorySearchResult)
        {
            using (SqlConnection conn = new SqlConnection(appSettings.MWConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "UpdateToolVendor";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ToolID", SqlDbType.Int).Value = toolInventorySearchResult.ID;
                    if (!String.IsNullOrEmpty(toolInventorySearchResult.VendorID))
                        cmd.Parameters.Add("@VendorID", SqlDbType.Int).Value = toolInventorySearchResult.VendorID;

                    cmd.Connection = conn;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return toolInventorySearchResult.ID;
        }

        public int CreateTool(ToolInventorySaveRequest toolInventorySaveRequest)
        {
            using (SqlConnection conn = new SqlConnection(appSettings.MWConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "CreateTool";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Code", SqlDbType.VarChar, 50).Value = toolInventorySaveRequest.Code;
                    cmd.Connection = conn;
                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public int DeleteTool(int ToolID)
        {
            using (SqlConnection conn = new SqlConnection(appSettings.MWConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "DeleteTool";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ToolID", SqlDbType.Int).Value = ToolID;
                    cmd.Connection = conn;
                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }
        public int SaveToolDetails(ToolInventorySaveRequest toolInventorySaveRequest)
        {
            using (SqlConnection conn = new SqlConnection(appSettings.MWConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "SaveToolDetails";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID", SqlDbType.Int).Value = toolInventorySaveRequest.ID;
                    cmd.Parameters.Add("@Angle", SqlDbType.NVarChar, 100).Value = toolInventorySaveRequest.Angle;
                    cmd.Parameters.Add("@CategoryID", SqlDbType.Int).Value = toolInventorySaveRequest.CategoryID;
                    cmd.Parameters.Add("@CheckedOut", SqlDbType.Int).Value = toolInventorySaveRequest.CheckedOut;
                    cmd.Parameters.Add("@ChipBreaker", SqlDbType.NVarChar, 200).Value = toolInventorySaveRequest.ChipBreaker;
                    cmd.Parameters.Add("@Code", SqlDbType.NVarChar, 100).Value = toolInventorySaveRequest.Code;
                    cmd.Parameters.Add("@Comment", SqlDbType.NVarChar, 510).Value = toolInventorySaveRequest.Comment;
                    cmd.Parameters.Add("@CuttingMethodIDs", SqlDbType.VarChar).Value =
                            string.Join(",", toolInventorySaveRequest.CuttingMethodID);
                    cmd.Parameters.Add("@Description", SqlDbType.VarChar, 1000).Value = toolInventorySaveRequest.Description;
                    cmd.Parameters.Add("@Diameter", SqlDbType.NVarChar, 100).Value = toolInventorySaveRequest.Diameter;
                    cmd.Parameters.Add("@Direction", SqlDbType.NVarChar, 200).Value = toolInventorySaveRequest.Direction;
                    cmd.Parameters.Add("@ExternalLocation", SqlDbType.NVarChar, 200).Value = toolInventorySaveRequest.ExternalLocation;
                    cmd.Parameters.Add("@FluteLength", SqlDbType.NVarChar, 100).Value = toolInventorySaveRequest.FluteLength;
                    cmd.Parameters.Add("@Grade", SqlDbType.NVarChar, 100).Value = toolInventorySaveRequest.Grade;
                    cmd.Parameters.Add("@ImageCode", SqlDbType.NVarChar, 40).Value = toolInventorySaveRequest.ImageCode;
                    cmd.Parameters.Add("@InventoryLevel", SqlDbType.Decimal).Value = toolInventorySaveRequest.InventoryLevel;

                    if (toolInventorySaveRequest.isLocked == "on")
                        cmd.Parameters.Add("@isLocked", SqlDbType.Bit).Value = 1;
                    else
                        cmd.Parameters.Add("@isLocked", SqlDbType.Bit).Value = 0;
                    //cmd.Parameters.Add("@isSent", SqlDbType.Bit).Value = toolInventorySaveRequest.isSent;
                    cmd.Parameters.Add("@ItemNumber", SqlDbType.NVarChar, 100).Value = toolInventorySaveRequest.ItemNumber;
                    if (!string.IsNullOrEmpty(toolInventorySaveRequest.LBS))
                        cmd.Parameters.Add("@LBS", SqlDbType.Decimal).Value = toolInventorySaveRequest.LBS;
                    cmd.Parameters.Add("@Location", SqlDbType.NVarChar, 200).Value = toolInventorySaveRequest.Location;
                    cmd.Parameters.Add("@MachineNumber", SqlDbType.VarChar, 20).Value = toolInventorySaveRequest.MachineNumber;
                    cmd.Parameters.Add("@Manufacturer", SqlDbType.NVarChar, 200).Value = toolInventorySaveRequest.Manufacturer;
                    cmd.Parameters.Add("@Material", SqlDbType.NVarChar, 100).Value = toolInventorySaveRequest.Material;
                    cmd.Parameters.Add("@MaxDepthOfCut", SqlDbType.NVarChar, 100).Value = toolInventorySaveRequest.MaxDepthOfCut;
                    cmd.Parameters.Add("@MWID", SqlDbType.NVarChar, 100).Value = toolInventorySaveRequest.MWID;
                    cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 200).Value = toolInventorySaveRequest.Name;
                    if (!string.IsNullOrEmpty(toolInventorySaveRequest.NewAppDate))
                        cmd.Parameters.Add("@NewAppDate", SqlDbType.DateTime).Value = toolInventorySaveRequest.NewAppDate;
                    cmd.Parameters.Add("@NumOfCutters", SqlDbType.Int).Value = toolInventorySaveRequest.NumOfCutters;
                    cmd.Parameters.Add("@NumOfFlutes", SqlDbType.Int).Value = toolInventorySaveRequest.NumOfFlutes;
                    cmd.Parameters.Add("@OAL", SqlDbType.NVarChar, 100).Value = toolInventorySaveRequest.OAL;
                    cmd.Parameters.Add("@OnHand", SqlDbType.Decimal).Value = toolInventorySaveRequest.OnHand;
                    cmd.Parameters.Add("@OrderApproved", SqlDbType.Int).Value = toolInventorySaveRequest.OrderApproved;
                    cmd.Parameters.Add("@OrderPoint", SqlDbType.Decimal).Value = toolInventorySaveRequest.OrderPoint;
                    cmd.Parameters.Add("@PackSize", SqlDbType.Decimal).Value = toolInventorySaveRequest.PackSize;
                    if (!string.IsNullOrEmpty(toolInventorySaveRequest.POID))
                        cmd.Parameters.Add("@POID", SqlDbType.Int).Value = toolInventorySaveRequest.POID;
                    cmd.Parameters.Add("@Radius", SqlDbType.NVarChar, 100).Value = toolInventorySaveRequest.Radius;
                    cmd.Parameters.Add("@ShankDiameter", SqlDbType.NVarChar, 100).Value = toolInventorySaveRequest.ShankDiameter;
                    cmd.Parameters.Add("@NeckDiameter", SqlDbType.NVarChar, 100).Value = toolInventorySaveRequest.NeckDiameter;
                    cmd.Parameters.Add("@StationNumber", SqlDbType.VarChar, 20).Value = toolInventorySaveRequest.StationNumber;
                    cmd.Parameters.Add("@StatusID", SqlDbType.Int).Value = toolInventorySaveRequest.StatusID;
                    cmd.Parameters.Add("@ToolGroupNumber", SqlDbType.Int).Value = toolInventorySaveRequest.ToolGroupNumber;
                    cmd.Parameters.Add("@Unit", SqlDbType.VarChar, 50).Value = toolInventorySaveRequest.Unit;
                    cmd.Parameters.Add("@UnitPrice", SqlDbType.Money).Value = toolInventorySaveRequest.UnitPrice;
                    if (!string.IsNullOrEmpty(toolInventorySaveRequest.VendorID))
                        cmd.Parameters.Add("@VendorID", SqlDbType.Int).Value = toolInventorySaveRequest.VendorID;
                    cmd.Parameters.Add("@Width", SqlDbType.NVarChar, 100).Value = toolInventorySaveRequest.Width;
                    cmd.Connection = conn;
                    conn.Open();
                    toolInventorySaveRequest.ID = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            return toolInventorySaveRequest.ID;
        }

        public int CopyTool(int ToolID)
        {
            ToolInventorySearchResult toolInventorySearchResult = new ToolInventorySearchResult();

            using (SqlConnection conn = new SqlConnection(appSettings.MWConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "CopyTool";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ToolID", SqlDbType.Int).Value = ToolID;
                    cmd.Connection = conn;
                    conn.Open();
                    ToolID = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            return ToolID;
        }
        public ToolInventorySearchResult GetToolDetails(int ToolID)
        {
            ToolInventorySearchResult toolInventorySearchResult = new ToolInventorySearchResult();

            // new tool?
            if (ToolID == 0)
                return toolInventorySearchResult;

            using (SqlConnection conn = new SqlConnection(appSettings.MWConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "GetToolDetails";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ToolID", SqlDbType.Int).Value = ToolID;
                    cmd.Connection = conn;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            toolInventorySearchResult.ID = Convert.ToInt32(reader["ID"].ToString());
                            toolInventorySearchResult.ItemNumber = reader["ItemNumber"].ToString();
                            toolInventorySearchResult.Name = reader["Name"].ToString();
                            toolInventorySearchResult.CategoryName = reader["CategoryName"].ToString();
                            toolInventorySearchResult.CategoryID = reader["CategoryID"].ToString();
                            toolInventorySearchResult.MWID = reader["MWID"].ToString();
                            toolInventorySearchResult.Radius = reader["Radius"].ToString();
                            toolInventorySearchResult.Diameter = reader["Diameter"].ToString();
                            toolInventorySearchResult.Angle = reader["Angle"].ToString();
                            toolInventorySearchResult.Direction = reader["Direction"].ToString();
                            toolInventorySearchResult.Width = reader["Width"].ToString();
                            toolInventorySearchResult.NumOfCutters = reader["NumOfCutters"].ToString();
                            toolInventorySearchResult.MaxDepthOfCut = reader["MaxDepthOfCut"].ToString();
                            toolInventorySearchResult.NumOfFlutes = reader["NumOfFlutes"].ToString();
                            toolInventorySearchResult.FluteLength = reader["FluteLength"].ToString();
                            toolInventorySearchResult.OAL = reader["OAL"].ToString();
                            toolInventorySearchResult.ShankDiameter = reader["ShankDiameter"].ToString();
                            toolInventorySearchResult.NeckDiameter = reader["NeckDiameter"].ToString();
                            toolInventorySearchResult.ChipBreaker = reader["ChipBreaker"].ToString();
                            toolInventorySearchResult.Material = reader["Material"].ToString();
                            toolInventorySearchResult.Grade = reader["Grade"].ToString();
                            toolInventorySearchResult.OnHand = reader["OnHand"].ToString();
                            toolInventorySearchResult.CheckedOut = reader["CheckedOut"].ToString();
                            toolInventorySearchResult.Location = reader["Location"].ToString();
                            toolInventorySearchResult.ExternalLocation = reader["ExternalLocation"].ToString();
                            toolInventorySearchResult.OrderPoint = reader["OrderPoint"].ToString();
                            toolInventorySearchResult.InventoryLevel = reader["InventoryLevel"].ToString();
                            toolInventorySearchResult.UnitPrice = reader["UnitPrice"].ToString();
                            toolInventorySearchResult.Manufacturer = reader["Manufacturer"].ToString();
                            toolInventorySearchResult.Code = reader["Code"].ToString();
                            toolInventorySearchResult.VendorID = reader["VendorID"].ToString();
                            toolInventorySearchResult.Comment = reader["Comment"].ToString();
                            toolInventorySearchResult.Status = reader["Status"].ToString();
                            toolInventorySearchResult.StatusID = reader["StatusID"].ToString();
                            toolInventorySearchResult.ImageCode = reader["ImageCode"].ToString();
                            toolInventorySearchResult.POID = reader["POID"].ToString();
                            toolInventorySearchResult.PackSize = reader["PackSize"].ToString();
                            toolInventorySearchResult.Unit = reader["Unit"].ToString();
                            toolInventorySearchResult.isLocked = reader["isLocked"].ToString();
                            toolInventorySearchResult.ToolGroupNumber = reader["ToolGroupNumber"].ToString();
                            toolInventorySearchResult.isSent = reader["isSent"].ToString();
                            toolInventorySearchResult.Description = reader["Description"].ToString();
                            toolInventorySearchResult.CuttingMethods = reader["CuttingMethods"].ToString();
                            toolInventorySearchResult.OrderApproved = reader["OrderApproved"].ToString();
                            toolInventorySearchResult.NewAppDate = reader["NewAppDate"].ToString();
                            toolInventorySearchResult.MachineNumber = reader["MachineNumber"].ToString();
                            toolInventorySearchResult.StationNumber = reader["StationNumber"].ToString();
                            toolInventorySearchResult.LBS = reader["LBS"].ToString();
                            toolInventorySearchResult.OAL = reader["OAL"].ToString();
                            toolInventorySearchResult.ImagePath = imageUrl + reader["ImagePath"].ToString();
                            if (reader["LinkedTools"].ToString() != string.Empty)
                            {
                                var linkedTools = reader["LinkedTools"].ToString().Split(',').ToList();
                                toolInventorySearchResult.LinkedTools = new List<LinkedTool>();
                                foreach (string linkTool in linkedTools)
                                {
                                    string[] arrlinkTool = linkTool.Split('|');
                                    toolInventorySearchResult.LinkedTools.Add(
                                        new LinkedTool
                                        {
                                            ID = Convert.ToInt32(arrlinkTool[0]),
                                            Description = arrlinkTool[1],
                                            ImagePath = imageUrl + arrlinkTool[2]
                                        }
                                    );
                                }
                            }
                            toolInventorySearchResult.VendorInfo =
                                   new VendorInfo
                                   {
                                       CompanyName = reader["CompanyName"].ToString(),
                                       Address1 = reader["Address1"].ToString(),
                                       Address2 = reader["Address2"].ToString(),
                                       City = reader["City"].ToString(),
                                       State = reader["State"].ToString(),
                                       Zip = reader["Zip"].ToString(),
                                       Country = reader["Country"].ToString(),
                                       Phone = reader["Phone"].ToString(),
                                       Fax = reader["Fax"].ToString(),
                                       Mobile = reader["Mobile"].ToString(),
                                       Website = reader["Website"].ToString(),
                                       Email = reader["Email"].ToString(),
                                       TollFree = reader["Tollfree"].ToString()

                                   };
                        }
                    }
                }
            }

            return toolInventorySearchResult;
        }
        public ToolInventorySearchResults ToolInventorySearch(ToolInventorySearch toolInventorySearch)
        {
            ToolInventorySearchResults toolInventorySearchResults = new ToolInventorySearchResults();
            bool firstRecord = true;
            toolInventorySearchResults.SearchResults = new List<ToolInventorySearchResult>();
            using (SqlConnection conn = new SqlConnection(appSettings.MWConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "ToolInventorySearch";
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (toolInventorySearch.Code.Length > 0 && toolInventorySearch.Code[0] != string.Empty)
                        cmd.Parameters.Add("@Code", SqlDbType.VarChar).Value = string.Join(";", toolInventorySearch.Code);
                    if (!string.IsNullOrEmpty(toolInventorySearch.Name))
                        cmd.Parameters.Add("@Name", SqlDbType.VarChar, 50).Value = toolInventorySearch.Name;
                    if (!string.IsNullOrEmpty(toolInventorySearch.ItemNumber))
                        cmd.Parameters.Add("@ItemNumber", SqlDbType.VarChar, 50).Value = toolInventorySearch.ItemNumber;
                    if (!string.IsNullOrEmpty(toolInventorySearch.CategoryID))
                        cmd.Parameters.Add("@CategoryID", SqlDbType.Int).Value = toolInventorySearch.CategoryID;
                    if (!string.IsNullOrEmpty(toolInventorySearch.MWID))
                        cmd.Parameters.Add("@MWID", SqlDbType.Int).Value = toolInventorySearch.MWID;
                    if (!string.IsNullOrEmpty(toolInventorySearch.Radius))
                        cmd.Parameters.Add("@Radius", SqlDbType.VarChar, 50).Value = toolInventorySearch.Radius;
                    if (!string.IsNullOrEmpty(toolInventorySearch.ChipBreaker))
                        cmd.Parameters.Add("@ChipBreaker", SqlDbType.VarChar, 50).Value = toolInventorySearch.ChipBreaker;
                    if (!string.IsNullOrEmpty(toolInventorySearch.Material))
                        cmd.Parameters.Add("@Material", SqlDbType.VarChar, 50).Value = toolInventorySearch.Material;
                    if (!string.IsNullOrEmpty(toolInventorySearch.Grade))
                        cmd.Parameters.Add("@Grade", SqlDbType.VarChar, 50).Value = toolInventorySearch.Grade;
                    if (!string.IsNullOrEmpty(toolInventorySearch.Location))
                        cmd.Parameters.Add("@Location", SqlDbType.VarChar, 50).Value = toolInventorySearch.Location;
                    if (!string.IsNullOrEmpty(toolInventorySearch.ExternalLocation))
                        cmd.Parameters.Add("@ExtLocation", SqlDbType.VarChar, 50).Value = toolInventorySearch.ExternalLocation;
                    if (!string.IsNullOrEmpty(toolInventorySearch.Manufacturer))
                        cmd.Parameters.Add("@Manufacturer", SqlDbType.VarChar, 50).Value = toolInventorySearch.Manufacturer;
                    if (!string.IsNullOrEmpty(toolInventorySearch.Comment))
                        cmd.Parameters.Add("@Comment", SqlDbType.VarChar, 50).Value = toolInventorySearch.Comment;
                    if (!string.IsNullOrEmpty(toolInventorySearch.StatusID))
                        cmd.Parameters.Add("@StatusID", SqlDbType.Int).Value = toolInventorySearch.StatusID;
                    if (!string.IsNullOrEmpty(toolInventorySearch.ToolGroupNumber))
                        cmd.Parameters.Add("@ToolGroupNum", SqlDbType.Int).Value = toolInventorySearch.ToolGroupNumber;
                    if (!string.IsNullOrEmpty(toolInventorySearch.Description))
                        cmd.Parameters.Add("@Description", SqlDbType.VarChar, 50).Value = toolInventorySearch.Description;
                    if (!string.IsNullOrEmpty(toolInventorySearch.CuttingMethods))
                        cmd.Parameters.Add("@CuttingMethods", SqlDbType.VarChar).Value = toolInventorySearch.CuttingMethods;

                    if (toolInventorySearch.sortColumn != string.Empty)
                        cmd.Parameters.Add("@sortColumn", SqlDbType.VarChar, 50).Value = toolInventorySearch.sortColumn;
                    if (toolInventorySearch.sortDirection != string.Empty)
                        cmd.Parameters.Add("@sortDirection", SqlDbType.VarChar, 50).Value = toolInventorySearch.sortDirection;
                    cmd.Parameters.Add("@pageNumber", SqlDbType.Int).Value = toolInventorySearch.pageNumber;
                    cmd.Parameters.Add("@pageSize", SqlDbType.Int).Value = toolInventorySearch.pageSize;

                    if (toolInventorySearch.SelectedToolIDs != null)
                        if (toolInventorySearch.SelectedToolIDs.Length > 0)
                            cmd.Parameters.Add("@SelectedToolIDs", SqlDbType.VarChar).Value = string.Join(",", toolInventorySearch.SelectedToolIDs);

                    cmd.Connection = conn;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (firstRecord)
                            {
                                firstRecord = false;
                                toolInventorySearchResults.recordCount = Convert.ToInt16(reader["recordCount"].ToString());
                            }

                            toolInventorySearchResults.SearchResults.Add(new ToolInventorySearchResult
                            {
                                ID = Convert.ToInt32(reader["ID"].ToString()),
                                Unit = reader["Unit"].ToString(),
                                Code = reader["Code"].ToString(),
                                Name = reader["Name"].ToString(),
                                ItemNumber = reader["ItemNumber"].ToString(),
                                Manufacturer = reader["Manufacturer"].ToString(),
                                MWID = reader["MWID"].ToString(),
                                Location = reader["Location"].ToString(),
                                Radius = reader["Radius"].ToString(),
                                CuttingMethods = reader["CuttingMethods"].ToString(),
                                NumOfCutters = reader["NumOfCutters"].ToString(),
                                Material = reader["Material"].ToString(),
                                Grade = reader["Grade"].ToString(),
                                OnHand = reader["OnHand"].ToString(),
                                ChipBreaker = reader["ChipBreaker"].ToString(),
                                CheckedOut = reader["CheckedOut"].ToString(),
                                Comment = reader["Comment"].ToString(),
                                Description = reader["Description"].ToString(),
                                ExternalLocation = reader["ExternalLocation"].ToString(),
                                CategoryName = reader["CategoryName"].ToString(),
                                Status = reader["Status"].ToString(),
                                isLocked = reader["isLocked"].ToString(),
                                OrderPoint = reader["OrderPoint"].ToString(),
                                InventoryLevel = reader["InventoryLevel"].ToString(),
                                ToolGroupNumber = reader["ToolGroupNumber"].ToString(),
                                UnitPrice = reader["UnitPrice"].ToString(),
                                PackSize = reader["PackSize"].ToString()
                            }
                            );
                        }
                    }
                }
            }

            return toolInventorySearchResults;
        }

        public ToolInventorySearchResults ToolInventorySearchSelected(ToolInventorySearch toolInventorySearch)
        {
            ToolInventorySearchResults toolInventorySearchResults = new ToolInventorySearchResults();
            bool firstRecord = true;
            toolInventorySearchResults.SearchResults = new List<ToolInventorySearchResult>();
            using (SqlConnection conn = new SqlConnection(appSettings.MWConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "ToolInventorySearchSelected";
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (toolInventorySearch.sortColumn != string.Empty)
                        cmd.Parameters.Add("@sortColumn", SqlDbType.VarChar, 50).Value = toolInventorySearch.sortColumn;
                    if (toolInventorySearch.sortDirection != string.Empty)
                        cmd.Parameters.Add("@sortDirection", SqlDbType.VarChar, 50).Value = toolInventorySearch.sortDirection;
                    cmd.Parameters.Add("@pageNumber", SqlDbType.Int).Value = toolInventorySearch.pageNumber;
                    cmd.Parameters.Add("@pageSize", SqlDbType.Int).Value = toolInventorySearch.pageSize;
                    cmd.Parameters.Add("@SelectedToolIDs", SqlDbType.VarChar).Value = string.Join(",", toolInventorySearch.SelectedToolIDs);

                    cmd.Connection = conn;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (firstRecord)
                            {
                                firstRecord = false;
                                toolInventorySearchResults.recordCount = Convert.ToInt16(reader["recordCount"].ToString());
                            }

                            toolInventorySearchResults.SearchResults.Add(new ToolInventorySearchResult
                            {
                                ID = Convert.ToInt32(reader["ID"].ToString()),
                                Unit = reader["Unit"].ToString(),
                                Code = reader["Code"].ToString(),
                                Name = reader["Name"].ToString(),
                                ItemNumber = reader["ItemNumber"].ToString(),
                                Manufacturer = reader["Manufacturer"].ToString(),
                                MWID = reader["MWID"].ToString(),
                                Location = reader["Location"].ToString(),
                                Radius = reader["Radius"].ToString(),
                                CuttingMethods = reader["CuttingMethods"].ToString(),
                                NumOfCutters = reader["NumOfCutters"].ToString(),
                                Material = reader["Material"].ToString(),
                                Grade = reader["Grade"].ToString(),
                                OnHand = reader["OnHand"].ToString(),
                                ChipBreaker = reader["ChipBreaker"].ToString(),
                                CheckedOut = reader["CheckedOut"].ToString(),
                                Comment = reader["Comment"].ToString(),
                                Description = reader["Description"].ToString(),
                                ExternalLocation = reader["ExternalLocation"].ToString(),
                                CategoryName = reader["CategoryName"].ToString(),
                                Status = reader["Status"].ToString(),
                                isLocked = reader["isLocked"].ToString(),
                                OrderPoint = reader["OrderPoint"].ToString(),
                                InventoryLevel = reader["InventoryLevel"].ToString(),
                                ToolGroupNumber = reader["ToolGroupNumber"].ToString(),
                                UnitPrice = reader["UnitPrice"].ToString(),
                                PackSize = reader["PackSize"].ToString()
                            }
                            );
                        }
                    }
                }
            }

            return toolInventorySearchResults;
        }
        //
        public LookupCategories GetLookUpCategory(LookupCategorySearch lookupCategorySearch)
        {
            LookupCategories lookupCategories = new LookupCategories();
            bool firstRecord = true;
            lookupCategories.lookupCategoryValues = new List<LookupCategoryValue>();

            using (SqlConnection conn = new SqlConnection(appSettings.MWConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "GetLookUpCategory";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@pageNumber", SqlDbType.Int).Value = lookupCategorySearch.pageNumber;
                    cmd.Parameters.Add("@pageSize", SqlDbType.Int).Value = lookupCategorySearch.pageSize;
                    cmd.Parameters.Add("@Category", SqlDbType.VarChar).Value = lookupCategorySearch.Category;

                    cmd.Connection = conn;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (firstRecord)
                            {
                                firstRecord = false;
                                lookupCategories.recordCount = Convert.ToInt16(reader["recordCount"].ToString());
                            }

                            lookupCategories.lookupCategoryValues.Add(new LookupCategoryValue
                            {
                                ID = Convert.ToInt32(reader["ID"].ToString()),
                                Text = reader["Text"].ToString(),
                                Value = reader["Value"].ToString(),
                                active = bool.Parse(reader["isActive"].ToString())
                            }
                            );
                        }
                    }
                }
            }

            return lookupCategories;
        }
        //
        public List<ToolInventoryColumn> GetSelectedToolInventoryColumns(string codes, bool searchableOnly = false)
        {
            List<ToolInventoryColumn> toolInventoryColumns = new List<ToolInventoryColumn>();
            using (SqlConnection conn = new SqlConnection(appSettings.MWConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    if (searchableOnly)
                        cmd.CommandText = "GetSearchableToolInventoryColumns";
                    else
                        cmd.CommandText = "GetSelectedToolInventoryColumns";

                    cmd.CommandType = CommandType.StoredProcedure;
                    if (!string.IsNullOrEmpty(codes))
                        cmd.Parameters.Add("@codes", SqlDbType.VarChar).Value = codes;

                    cmd.Connection = conn;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ToolInventoryColumn toolInventoryColumn = new ToolInventoryColumn
                            {
                                Name = reader["ColumnName"].ToString(),
                                Header = reader["ColumnHeader"].ToString(),
                                Sequence = Convert.ToInt16(reader["Sequence"].ToString()),
                                InputType = reader["InputType"].ToString(),
                                UISize = Convert.ToInt16(reader["UISize"].ToString()),
                                PropertyName = reader["PropertyName"].ToString(),
                                Required = (reader.GetBoolean(reader.GetOrdinal("Required")) ? "required" : ""),
                                Display = (reader.GetBoolean(reader.GetOrdinal("Display")))
                            };
                            toolInventoryColumns.Add(toolInventoryColumn);
                        }
                    }
                }
            }
            return toolInventoryColumns;
        }

        public List<ToolCuttingMethod> GetToolCuttingMethods(int ToolID, bool allMethods = true)
        {
            List<ToolCuttingMethod> toolCuttingMethods = new List<ToolCuttingMethod>();
            using (SqlConnection conn = new SqlConnection(appSettings.MWConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "GetToolCuttingMethods";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ToolID", SqlDbType.Int).Value = ToolID;
                    cmd.Parameters.Add("@AllMethods", SqlDbType.Bit).Value = (allMethods) ? 1 : 0;

                    cmd.Connection = conn;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            toolCuttingMethods.Add(
                                 new ToolCuttingMethod
                                 {
                                     ID = Convert.ToInt32(reader["ID"].ToString()),
                                     Text = reader["Text"].ToString(),
                                     Value = reader["Value"].ToString(),
                                     Connected = (reader.GetBoolean(reader.GetOrdinal("Connected")) ? true : false)
                                 }
                                );
                        }
                    }
                }
            }
            return toolCuttingMethods;
        }
        public List<ToolInventoryCodeColumn> GetToolInventoryColumnsByCode(string code)
        {
            List<ToolInventoryCodeColumn> toolInventoryColumns = new List<ToolInventoryCodeColumn>();
            using (SqlConnection conn = new SqlConnection(appSettings.MWConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "GetToolInventoryColumnsByCode";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@code", SqlDbType.VarChar, 50).Value = code;
                    cmd.Connection = conn;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ToolInventoryCodeColumn toolInventoryColumn = new ToolInventoryCodeColumn
                            {
                                Name = reader["ColumnName"].ToString(),
                                Header = reader["ColumnHeader"].ToString(),
                                Sequence = Convert.ToInt16(reader["Sequence"].ToString()),
                                Show = Convert.ToBoolean(Convert.ToInt16(reader["Show"].ToString()))
                            };
                            toolInventoryColumns.Add(toolInventoryColumn);
                        }
                    }
                }
            }
            return toolInventoryColumns;
        }
        public ToolSetupSheet GetToolSetupSheet(int setupSheetId)
        {
            ToolSetupSheet toolSetupSheet = new ToolSetupSheet();
            try
            {
                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = appSettings.MWConnectionString;
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "spGetToolSetupSheet";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@SetupSheetID", SqlDbType.Int).Value = setupSheetId;
                        cmd.Connection = conn;
                        conn.Open();
                        DataSet dsSetUpSheet = new DataSet();
                        SqlDataAdapter daSetUpSheet = new SqlDataAdapter();
                        daSetUpSheet.SelectCommand = cmd;
                        daSetUpSheet.Fill(dsSetUpSheet);
                        DataRow drSetupSheet = dsSetUpSheet.Tables[0].Rows[0];
                        DataTable dtToolSetup = dsSetUpSheet.Tables[1];
                        toolSetupSheet.SetUpSheetID = setupSheetId;
                        toolSetupSheet.PartNumber = drSetupSheet["PartNumber"].ToString();
                        toolSetupSheet.Revision = drSetupSheet["Revision"].ToString();
                        toolSetupSheet.Operation = drSetupSheet["Operation"].ToString();
                        toolSetupSheet.InputDate = Convert.ToDateTime(drSetupSheet["InputDate"].ToString()).ToShortDateString();
                        toolSetupSheet.Machine = drSetupSheet["Machine"].ToString();
                        toolSetupSheet.ProgramNumber = drSetupSheet["ProgramNumber"].ToString();
                        toolSetupSheet.ProgramLocation = drSetupSheet["ProgramLocation"].ToString();
                        toolSetupSheet.UOM = drSetupSheet["UOM"].ToString();
                        toolSetupSheet.MaterialType = drSetupSheet["MaterialType"].ToString();
                        toolSetupSheet.MaterialHeatTreated = drSetupSheet["MaterialHeatTreated"].ToString();
                        toolSetupSheet.MaterialForm = drSetupSheet["MaterialForm"].ToString();
                        toolSetupSheet.MaterialSize = drSetupSheet["MaterialSize"].ToString();
                        toolSetupSheet.MachineWorkHoldingTo = drSetupSheet["MachineWorkHoldingTo"].ToString();
                        toolSetupSheet.CutWorkHoldingTo = drSetupSheet["CutWorkHoldingTo"].ToString();
                        toolSetupSheet.workHolding1ItemNumber = drSetupSheet["workHolding1ItemNumber"].ToString();
                        toolSetupSheet.workHolding2ItemNumber = drSetupSheet["workHolding2ItemNumber"].ToString();
                        toolSetupSheet.workHolding3ItemNumber = drSetupSheet["workHolding3ItemNumber"].ToString();
                        toolSetupSheet.workHolding1ImagePath = drSetupSheet["workHolding1ImagePath"].ToString();
                        toolSetupSheet.workHolding2ImagePath = drSetupSheet["workHolding2ImagePath"].ToString();
                        toolSetupSheet.workHolding3ImagePath = drSetupSheet["workHolding3ImagePath"].ToString();
                        toolSetupSheet.workHolding1MWID = drSetupSheet["workHolding1MWID"].ToString();
                        toolSetupSheet.workHolding2MWID = drSetupSheet["workHolding2MWID"].ToString();
                        toolSetupSheet.workHolding3MWID = drSetupSheet["workHolding3MWID"].ToString();
                        toolSetupSheet.workHolding1Location = drSetupSheet["workHolding1Location"].ToString();
                        toolSetupSheet.workHolding2Location = drSetupSheet["workHolding2Location"].ToString();
                        toolSetupSheet.workHolding3Location = drSetupSheet["workHolding3Location"].ToString();
                        toolSetupSheet.workHoldingComments = drSetupSheet["workHoldingComments"].ToString();
                        toolSetupSheet.workHoldingImageNoPart = drSetupSheet["workHoldingImageNoPart"].ToString();
                        toolSetupSheet.workHoldingImageWithPart = drSetupSheet["workHoldingImageWithPart"].ToString();
                        toolSetupSheet.workHoldingImageComplete = drSetupSheet["workHoldingImageComplete"].ToString();
                        toolSetupSheet.Torque = drSetupSheet["Torque"].ToString();
                        toolSetupSheet.HoldPartOn = drSetupSheet["HoldPartOn"].ToString();
                        toolSetupSheet.Z0 = drSetupSheet["Z0"].ToString();
                        toolSetupSheet.BarStickOutBefore = drSetupSheet["BarStickOutBefore"].ToString();
                        toolSetupSheet.FaceOff = drSetupSheet["FaceOff"].ToString();
                        toolSetupSheet.CutOffToolThickness = drSetupSheet["CutOffToolThickness"].ToString();
                        toolSetupSheet.OAL = drSetupSheet["OAL"].ToString();
                        toolSetupSheet.BarStickOutAfter = drSetupSheet["BarStickOutAfter"].ToString();
                        toolSetupSheet.BarPullOut = drSetupSheet["BarPullOut"].ToString();
                        toolSetupSheet.OAL = drSetupSheet["OAL"].ToString();
                        toolSetupSheet.PartStickOutMinimum = drSetupSheet["PartStickOutMinimum"].ToString();
                        toolSetupSheet.Comments = drSetupSheet["Comments"].ToString();
                        toolSetupSheet.Program = drSetupSheet["Program"].ToString();

                        List<ToolSetUp> lstToolSetup = new List<ToolSetUp>();

                        foreach (DataRow drToolSetup in dtToolSetup.Rows)
                        {
                            ToolSetUp toolSetup = new ToolSetUp();
                            toolSetup.ID = Convert.ToInt32(drToolSetup["Id"].ToString());
                            toolSetup.Sequence = drToolSetup["Sequence"].ToString();
                            toolSetup.N = drToolSetup["N"].ToString();
                            toolSetup.ToolNumber = drToolSetup["ToolNumber"].ToString();
                            toolSetup.TONumber = drToolSetup["TONumber"].ToString();
                            //toolSetup.CuttingMethodId = drToolSetup["CuttingMethodId"].ToString();
                            toolSetup.CuttingMethod = drToolSetup["CuttingMethod"].ToString();
                            toolSetup.SpecialComment = drToolSetup["SpecialComment"].ToString();
                            toolSetup.PartsPerCorner = Convert.ToInt32(drToolSetup["PartsPerCorner"].ToString());
                            toolSetup.SecondsPerTool = Convert.ToInt32(drToolSetup["SecondsPerTool"].ToString());
                            toolSetup.Comment = drToolSetup["Comment"].ToString();
                            toolSetup.Snippet = drToolSetup["Snippet"].ToString();
                            toolSetup.ToolDesc = drToolSetup["ToolDesc"].ToString();
                            toolSetup.ToolItem = drToolSetup["ToolItem"].ToString();
                            toolSetup.ToolName = drToolSetup["ToolName"].ToString();
                            toolSetup.ToolHolder1Item = drToolSetup["ToolHolder1Item"].ToString();
                            toolSetup.ToolHolder2Item = drToolSetup["ToolHolder2Item"].ToString();
                            toolSetup.ToolHolder3Item = drToolSetup["ToolHolder3Item"].ToString();
                            toolSetup.ToolHolder1Name = drToolSetup["ToolHolder1Name"].ToString();
                            toolSetup.ToolHolder2Name = drToolSetup["ToolHolder2Name"].ToString();
                            toolSetup.ToolHolder3Name = drToolSetup["ToolHolder3Name"].ToString();
                            toolSetup.ToolHolder1MWID = drToolSetup["ToolHolder1MWID"].ToString();
                            toolSetup.ToolHolder2MWID = drToolSetup["ToolHolder2MWID"].ToString();
                            toolSetup.ToolHolder3MWID = drToolSetup["ToolHolder3MWID"].ToString();
                            toolSetup.ToolHolder1Loc = drToolSetup["ToolHolder1Loc"].ToString();
                            toolSetup.ToolHolder2Loc = drToolSetup["ToolHolder2Loc"].ToString();
                            toolSetup.ToolHolder3Loc = drToolSetup["ToolHolder3Loc"].ToString();
                            toolSetup.ToolID = drToolSetup["ToolID"].ToString();
                            toolSetup.ToolHolder1ID = drToolSetup["ToolHolder1ID"].ToString();
                            toolSetup.ToolHolder2ID = drToolSetup["ToolHolder2ID"].ToString();
                            toolSetup.ToolHolder3ID = drToolSetup["ToolHolder3ID"].ToString();
                            toolSetup.ToolImage = "/imgLibrary/" + drToolSetup["ToolImage"].ToString();
                            toolSetup.ToolHolder1Image = "/imgLibrary/" + drToolSetup["ToolHolder1Image"].ToString();
                            toolSetup.ToolHolder2Image = "/imgLibrary/" + drToolSetup["ToolHolder2Image"].ToString();
                            toolSetup.ToolHolder3Image = "/imgLibrary/" + drToolSetup["ToolHolder3Image"].ToString();
                            lstToolSetup.Add(toolSetup);
                        }
                        toolSetupSheet.ToolsSetUp = lstToolSetup;
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return toolSetupSheet;
        }

        public DBResponse UpdateToolSetupSheet(ToolSetupSheet toolSetupSheet)
        {
            DBResponse dbResponse = new DBResponse();
            string sMachine = string.Empty;

            try
            {
                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = appSettings.MWConnectionString;
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "spSaveToolSetupSheet";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@SetupSheetID", SqlDbType.Int).Value = toolSetupSheet.SetUpSheetID;
                        cmd.Parameters.Add("@PartNumber", SqlDbType.VarChar, 20).Value = toolSetupSheet.PartNumber;
                        cmd.Parameters.Add("@Revision", SqlDbType.VarChar, 20).Value = toolSetupSheet.Revision;
                        if (toolSetupSheet.Operation != string.Empty)
                            cmd.Parameters.Add("@Operation", SqlDbType.Int).Value = toolSetupSheet.Operation;
                        cmd.Parameters.Add("@InputDate", SqlDbType.DateTime, 20).Value = toolSetupSheet.InputDate;

                        switch (toolSetupSheet.Machine.ToLower())
                        {
                            case "lathe":
                                sMachine = "L01";
                                break;
                            case "mill":
                                sMachine = "M01";
                                break;
                            case "wireEDM":
                                sMachine = "E01";
                                break;
                            default:
                                sMachine = toolSetupSheet.Machine;
                                break;
                        }

                        cmd.Parameters.Add("@Machine", SqlDbType.VarChar, 50).Value = sMachine;
                        cmd.Parameters.Add("@ProgramNumber", SqlDbType.VarChar, 20).Value = toolSetupSheet.ProgramNumber;
                        cmd.Parameters.Add("@ProgramLocation", SqlDbType.VarChar, 50).Value = toolSetupSheet.ProgramLocation;
                        cmd.Parameters.Add("@UOM", SqlDbType.VarChar, 20).Value = toolSetupSheet.UOM;
                        cmd.Parameters.Add("@MaterialType", SqlDbType.VarChar, 50).Value = toolSetupSheet.MaterialType;
                        cmd.Parameters.Add("@MaterialHeatTreated", SqlDbType.VarChar, 50).Value = toolSetupSheet.MaterialHeatTreated;
                        cmd.Parameters.Add("@MaterialForm", SqlDbType.VarChar, 50).Value = toolSetupSheet.MaterialForm;
                        cmd.Parameters.Add("@MaterialSize", SqlDbType.VarChar, 20).Value = toolSetupSheet.MaterialSize;
                        cmd.Parameters.Add("@MachineWorkHoldingTo", SqlDbType.VarChar, 20).Value = toolSetupSheet.MachineWorkHoldingTo;
                        cmd.Parameters.Add("@workHolding1ItemNumber", SqlDbType.VarChar, 20).Value = toolSetupSheet.workHolding1ItemNumber;
                        cmd.Parameters.Add("@workHolding2ItemNumber", SqlDbType.VarChar, 20).Value = toolSetupSheet.workHolding2ItemNumber;
                        cmd.Parameters.Add("@workHolding3ItemNumber", SqlDbType.VarChar, 20).Value = toolSetupSheet.workHolding3ItemNumber;
                        cmd.Parameters.Add("@workHolding1ImagePath", SqlDbType.VarChar, 100).Value = toolSetupSheet.workHolding1ImagePath;
                        cmd.Parameters.Add("@workHolding2ImagePath", SqlDbType.VarChar, 100).Value = toolSetupSheet.workHolding2ImagePath;
                        cmd.Parameters.Add("@workHolding3ImagePath", SqlDbType.VarChar, 100).Value = toolSetupSheet.workHolding3ImagePath;
                        cmd.Parameters.Add("@workHolding1MWID", SqlDbType.VarChar, 20).Value = toolSetupSheet.workHolding1MWID;
                        cmd.Parameters.Add("@workHolding2MWID", SqlDbType.VarChar, 20).Value = toolSetupSheet.workHolding2MWID;
                        cmd.Parameters.Add("@workHolding3MWID", SqlDbType.VarChar, 20).Value = toolSetupSheet.workHolding3MWID;
                        cmd.Parameters.Add("@workHolding1Location", SqlDbType.VarChar, 50).Value = toolSetupSheet.workHolding1Location;
                        cmd.Parameters.Add("@workHolding2Location", SqlDbType.VarChar, 50).Value = toolSetupSheet.workHolding2Location;
                        cmd.Parameters.Add("@workHolding3Location", SqlDbType.VarChar, 50).Value = toolSetupSheet.workHolding3Location;
                        cmd.Parameters.Add("@workHoldingComments", SqlDbType.VarChar, 200).Value = toolSetupSheet.workHoldingComments;
                        cmd.Parameters.Add("@workHoldingImageNoPart", SqlDbType.VarChar, 200).Value = toolSetupSheet.workHoldingImageNoPart;
                        cmd.Parameters.Add("@workHoldingImageWithPart", SqlDbType.VarChar, 200).Value = toolSetupSheet.workHoldingImageWithPart;
                        cmd.Parameters.Add("@workHoldingImageComplete", SqlDbType.VarChar, 200).Value = toolSetupSheet.workHoldingImageComplete;
                        cmd.Parameters.Add("@Torque", SqlDbType.VarChar, 20).Value = toolSetupSheet.Torque;
                        cmd.Parameters.Add("@HoldPartOn", SqlDbType.VarChar, 20).Value = toolSetupSheet.HoldPartOn;
                        cmd.Parameters.Add("@Z0", SqlDbType.VarChar, 20).Value = toolSetupSheet.Z0;
                        cmd.Parameters.Add("@BarStickOutBefore", SqlDbType.VarChar, 20).Value = toolSetupSheet.BarStickOutBefore;
                        cmd.Parameters.Add("@FaceOff", SqlDbType.VarChar, 20).Value = toolSetupSheet.FaceOff;
                        cmd.Parameters.Add("@CutOffToolThickness", SqlDbType.VarChar, 20).Value = toolSetupSheet.CutOffToolThickness;
                        cmd.Parameters.Add("@OAL", SqlDbType.VarChar, 20).Value = toolSetupSheet.OAL;
                        cmd.Parameters.Add("@BarStickOutAfter", SqlDbType.VarChar, 20).Value = toolSetupSheet.BarStickOutAfter;
                        cmd.Parameters.Add("@BarPullOut", SqlDbType.VarChar, 20).Value = toolSetupSheet.BarPullOut;
                        cmd.Parameters.Add("@PartStickOutMinimum", SqlDbType.VarChar, 20).Value = toolSetupSheet.PartStickOutMinimum;
                        cmd.Parameters.Add("@Comments", SqlDbType.VarChar, 200).Value = toolSetupSheet.Comments;
                        cmd.Parameters.Add("@Program", SqlDbType.VarChar).Value = toolSetupSheet.Program;
                        cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 20).Value = toolSetupSheet.ModifiedBy;
                        cmd.Parameters.Add("@CutWorkHoldingTo", SqlDbType.VarChar, 20).Value = toolSetupSheet.CutWorkHoldingTo;
                        // sql 2005 doesn't have table type
                        //cmd.Parameters.Add("@ToolsList", SqlDbType.Structured).Value = dtToolSetup;                        
                        //var pList = new SqlParameter("@ToolSetupList", SqlDbType.Structured);
                        //pList.TypeName = "dbo.ToolSetup";
                        //pList.Value = dtToolSetup;
                        //cmd.Parameters.Add(pList);
                        cmd.Connection = conn;
                        conn.Open();
                        //cmd.ExecuteNonQuery();
                        string sSetupSheetId = cmd.ExecuteScalar().ToString();
                        conn.Close();
                        dbResponse.ReturnCode = 0;
                        RefreshLookupCaches();
                    }
                }
                foreach (ToolSetUp toolSetUp in toolSetupSheet.ToolsSetUp)
                {
                    if (toolSetUp.N != string.Empty)
                        SaveToolsSetup(toolSetupSheet.SetUpSheetID, toolSetUp, toolSetupSheet.ModifiedBy);
                }
            }
            catch (Exception ex)
            {
                dbResponse.ReturnCode = -1;
                dbResponse.Message = ex.Message;
                throw;
            }

            return dbResponse;
        }

        private void SaveToolsSetup(int SetUpSheetID, ToolSetUp toolSetUp, string userName)
        {
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = appSettings.MWConnectionString;
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "spSaveToolSetupSheetToolSetups";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@SetupSheetID", SqlDbType.Int).Value = SetUpSheetID;
                    cmd.Parameters.Add("@ID", SqlDbType.Int).Value = toolSetUp.ID;
                    cmd.Parameters.Add("@Sequence", SqlDbType.Int).Value = toolSetUp.Sequence;
                    cmd.Parameters.Add("@N", SqlDbType.Int).Value = toolSetUp.N;
                    cmd.Parameters.Add("@TONumber", SqlDbType.Int).Value = toolSetUp.TONumber;
                    cmd.Parameters.Add("@ToolNumber", SqlDbType.Int).Value = toolSetUp.ToolNumber;
                    cmd.Parameters.Add("@CuttingMethod", SqlDbType.VarChar, 50).Value = toolSetUp.CuttingMethod;
                    cmd.Parameters.Add("@PartsPerCorner", SqlDbType.Int).Value = toolSetUp.PartsPerCorner;
                    cmd.Parameters.Add("@SecondsPerTool", SqlDbType.Int).Value = toolSetUp.SecondsPerTool;
                    cmd.Parameters.Add("@Comment", SqlDbType.VarChar, 50).Value = toolSetUp.Comment;
                    cmd.Parameters.Add("@Snippet", SqlDbType.VarChar).Value = toolSetUp.Snippet;
                    if (toolSetUp.ToolID != string.Empty)
                        cmd.Parameters.Add("@ToolID", SqlDbType.Int).Value = toolSetUp.ToolID;
                    if (toolSetUp.ToolHolder1ID != string.Empty)
                        cmd.Parameters.Add("@ToolHolder1ID", SqlDbType.Int).Value = toolSetUp.ToolHolder1ID;
                    if (toolSetUp.ToolHolder2ID != string.Empty)
                        cmd.Parameters.Add("@ToolHolder2ID", SqlDbType.Int).Value = toolSetUp.ToolHolder2ID;
                    if (toolSetUp.ToolHolder3ID != string.Empty)
                        cmd.Parameters.Add("@ToolHolder3ID", SqlDbType.Int).Value = toolSetUp.ToolHolder3ID;
                    cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 20).Value = userName;
                    cmd.Connection = conn;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public DBResponse AddToolSetupToSetupSheet(AddToolSetupRequest addToolSetupRequest)
        {
            DBResponse dbResponse = new DBResponse();
            try
            {
                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = appSettings.MWConnectionString;
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "spAddSelectedToolSetupToSetupSheet";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ToolSetupSheetID", SqlDbType.Int).Value = addToolSetupRequest.SetUpSheetID;
                        string ID_GroupTypes = string.Join(",", addToolSetupRequest.ID_GroupType.Select(x => x.ToString()).ToArray());
                        cmd.Parameters.Add("@ID_GroupTypes", SqlDbType.VarChar, 100).Value = ID_GroupTypes;
                        cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 20).Value = addToolSetupRequest.ModifiedBy;
                        cmd.Connection = conn;
                        conn.Open();
                        dbResponse.RecordsAffected = cmd.ExecuteNonQuery();
                        conn.Close();
                        dbResponse.ReturnCode = 0;
                    }
                }

            }
            catch 
            {
                throw;
            }
            return dbResponse;
        }
        public DBResponse DeleteConversionRule(ConversionRule conversionRule)
        {
            DBResponse dbResponse = new DBResponse();
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = appSettings.MWConnectionString;
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "spDeleteConversionRule";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Id", SqlDbType.Int).Value = conversionRule.ID;
                    cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 20).Value = conversionRule.ModifiedBy;
                    cmd.Connection = conn;
                    conn.Open();
                    dbResponse.RecordsAffected = cmd.ExecuteNonQuery();
                    dbResponse.ReturnCode = 0;
                }
            }
            return dbResponse;
        }

        public DBResponse DeleteToolSetup(DeleteToolSetupRequest deleteToolSetupRequest)
        {
            DBResponse dbResponse = new DBResponse();
            using (SqlConnection con = new SqlConnection(appSettings.MWConnectionString))
            {
                string query = "spDeleteToolSetup";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@SetupSheetID", SqlDbType.Int).Value = deleteToolSetupRequest.SetupSheetID;
                    cmd.Parameters.Add("@ToolSetupId", SqlDbType.Int).Value = deleteToolSetupRequest.ToolSetupID;
                    cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 20).Value = deleteToolSetupRequest.ModifiedBy;
                    con.Open();
                    dbResponse.RecordsAffected = cmd.ExecuteNonQuery();
                    con.Close();
                    dbResponse.ReturnCode = 0;
                }
            }
            return dbResponse;
        }

        public List<ConversionRule> GetConversionRules(string FromMachineId, string ToMachineId)
        {
            List<ConversionRule> ConversionRules = new List<ConversionRule>();

            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = appSettings.MWConnectionString;
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "spGetConversionRules";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@FromMachineId", SqlDbType.VarChar, 20).Value = FromMachineId;
                    cmd.Parameters.Add("@ToMachineId", SqlDbType.VarChar, 20).Value = ToMachineId;
                    cmd.Connection = conn;
                    conn.Open();

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        ConversionRule oConversionRule = new ConversionRule();
                        oConversionRule.ID = Convert.ToInt32(reader["ID"].ToString());
                        oConversionRule.FromSnippet = reader["FromSnippet"].ToString();
                        oConversionRule.ToSnippet = reader["ToSnippet"].ToString();
                        ConversionRules.Add(oConversionRule);
                    }
                }
            }

            return ConversionRules;
        }


        public List<string> GetLookUpCategories(string searchTerm)
        {
            List<string> categories = new List<string>();
            using (SqlConnection con = new SqlConnection(appSettings.MWConnectionString))
            {
                string query = "GetLookUpCategories";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (!string.IsNullOrEmpty(searchTerm))
                        cmd.Parameters.Add("@SearchTerm", SqlDbType.Char, 1).Value = searchTerm;

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        categories.Add(reader["Category"].ToString());
                    }
                }
                con.Close();
            }
            return categories;
        }

        public List<string> GetMachines(string machinePrefix)
        {
            List<string> retMachine = new List<string>();
            using (SqlConnection con = new SqlConnection(appSettings.MWConnectionString))
            {
                string query = "spGetMachines";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@MachinePrefix", SqlDbType.Char, 1).Value = machinePrefix;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        retMachine.Add(reader["MachineId"].ToString());
                    }
                }
                con.Close();
            }
            return retMachine;
        }

        public DBResponse SaveConversionRule(ConversionRule conversionRule)
        {
            DBResponse dbResponse = new DBResponse();
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = appSettings.MWConnectionString;
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "spSaveConversionRule";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@RuleId", SqlDbType.VarChar, 20).Value = conversionRule.ID;
                    cmd.Parameters.Add("@FromMachineId", SqlDbType.VarChar, 20).Value = conversionRule.FromMachineId;
                    cmd.Parameters.Add("@ToMachineId", SqlDbType.VarChar, 20).Value = conversionRule.ToMachineId;
                    cmd.Parameters.Add("@FromSnippet", SqlDbType.VarChar, 1000).Value = conversionRule.FromSnippet;
                    cmd.Parameters.Add("@ToSnippet", SqlDbType.VarChar, 1000).Value = conversionRule.ToSnippet;
                    cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 20).Value = conversionRule.ModifiedBy;
                    cmd.Connection = conn;
                    conn.Open();
                    dbResponse.RecordsAffected = cmd.ExecuteNonQuery();
                    conn.Close();
                    dbResponse.ReturnCode = 0;
                    RefreshLookupCaches();
                }
            }
            return dbResponse;
        }

        public int SaveConvertedProgram(ProgramSaveRequest convertProgramSaveRequest)
        {
            int newSetUpSheetID = 0;

            using (SqlConnection con = new SqlConnection(appSettings.MWConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("spCopyToolSetupSheet", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 20).Value = convertProgramSaveRequest.ModifiedBy;
                    cmd.Parameters.Add("@ToolSetupSheetID", SqlDbType.Int).Value = convertProgramSaveRequest.SetUpSheetID;
                    cmd.Parameters.Add("@NewToolSetupSheetID", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
                    cmd.ExecuteNonQuery();
                    newSetUpSheetID = (int)cmd.Parameters["@NewToolSetupSheetID"].Value;
                }
            }

            using (SqlConnection con = new SqlConnection(appSettings.MWConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("spUpdateConvertedToolSetupSheet", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 20).Value = convertProgramSaveRequest.ModifiedBy;
                    cmd.Parameters.Add("@Program", SqlDbType.VarChar).Value = convertProgramSaveRequest.Program;
                    cmd.Parameters.Add("@MachineId", SqlDbType.VarChar).Value = convertProgramSaveRequest.SetUpSheetID;
                    cmd.Parameters.Add("@ToolSetupSheetID", SqlDbType.Int).Value = newSetUpSheetID;
                    cmd.ExecuteNonQuery();
                }
                con.Close();
            }

            return newSetUpSheetID;
        }

        public int UploadProvenProgram(UploadProgramRequest uploadProgramRequest)
        {
            int newSetUpSheetID = uploadProgramRequest.SetUpSheetID;

            try
            {
                using (SqlConnection con = new SqlConnection(appSettings.MWConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("spUploadProvenProgram", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        con.Open();
                        cmd.Parameters.Add("@ToolSetupSheetID", SqlDbType.Int).Value = uploadProgramRequest.SetUpSheetID;
                        cmd.Parameters.Add("@UseExistingSheet", SqlDbType.Bit).Value = uploadProgramRequest.UseExistingSheet;
                        cmd.Parameters.Add("@UploadedProgramText", SqlDbType.VarChar).Value = uploadProgramRequest.UploadedProgramText;
                        cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 20).Value = uploadProgramRequest.ModifiedBy;
                        cmd.Parameters.Add("@NewToolSetupSheetID", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
                        cmd.ExecuteNonQuery();
                        newSetUpSheetID = (int)cmd.Parameters["@NewToolSetupSheetID"].Value;
                    }
                }
            }
            catch 
            {
                throw;
            }


            return newSetUpSheetID;
        }
        public void SaveProgram(ProgramSaveRequest programSaveRequest)
        {
            string filePath =
                string.Format("{0}\\tss_{1}.txt", appSettings.UnprovenProgramsPath, programSaveRequest.SetUpSheetID);
            File.WriteAllText(filePath, programSaveRequest.Program);
        }

        public DBResponse SaveToolSetupGroup(ToolSetupGroupRequest tooSetupGroupRequest)
        {
            DBResponse dbResponse = new DBResponse();
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = appSettings.MWConnectionString;
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "spSaveToolSetupGroup";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@MainID", SqlDbType.Int).Value = tooSetupGroupRequest.MainID;
                    string IDs = string.Join(",", tooSetupGroupRequest.IDs.Select(x => x.ToString()).ToArray());
                    cmd.Parameters.Add("@IDs", SqlDbType.VarChar, 100).Value = IDs;
                    cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 20).Value = tooSetupGroupRequest.ModifiedBy;
                    cmd.Connection = conn;
                    conn.Open();
                    dbResponse.RecordsAffected = cmd.ExecuteNonQuery();
                    conn.Close();
                    dbResponse.ReturnCode = 0;
                }
            }
            return dbResponse;
        }

        public List<ToolSetupSearchResult> SearchToolSetups(string searchTerm)
        {
            List<ToolSetupSearchResult> retToolSetups = new List<ToolSetupSearchResult>();
            using (SqlConnection con = new SqlConnection(appSettings.MWConnectionString))
            {
                string query = "spSearchToolSetups";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@SearchTerm", SqlDbType.VarChar, 50).Value = searchTerm;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        retToolSetups.Add(new ToolSetupSearchResult
                        {
                            ID = Convert.ToInt32(reader["ID"]),
                            SpecialComment = reader["SpecialComment"].ToString(),
                            GroupType = reader["GroupType"].ToString()
                        }
                            );
                    }
                }
                con.Close();
            }
            return retToolSetups;
        }

        public List<string> GetSearchResults(string term, string category)
        {
            List<string> retResults = new List<string>();
            string sStoredProc = "spGetLookupValues";

            using (SqlConnection con = new SqlConnection(appSettings.MWConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sStoredProc, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Category", SqlDbType.VarChar, 100).Value = category;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        retResults.Add(string.Format("{0}|{1}", reader["Name"], reader["Description"]));
                    }
                }
                con.Close();
            }

            return retResults;
        }

        public List<string> GetSSPPartNumbers(string term)
        {
            List<string> retPartNumbers = new List<string>();
            if (GetFromCache("PartNumber") == null)
            {
                using (SqlConnection con = new SqlConnection(appSettings.MWConnectionString))
                {
                    string query = "spGetSSPPartNumbers";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@SearchTerm", SqlDbType.VarChar, 100).Value = term.Replace("'", "''");
                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            retPartNumbers.Add(reader["PartNumber"].ToString());
                            //retPartNumbers.Add(string.Format("{0}|{1}|{2}", reader["PartNumber"], reader["Revision"], reader["HID"]));
                        }
                    }
                    con.Close();
                    if (term == "")
                        AddToCache("PartNumber", retPartNumbers);
                }
            }
            return retPartNumbers;
        }

        private void AddToCache(string cacheKey, object cacheValue)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(60));
            cache.Set(cacheKey, cacheValue, cacheEntryOptions);
        }

        public object GetFromCache(string cacheKey)
        {
            return cache.Get<Object>(cacheKey);
        }

        public List<string> GetMaterialSize(string term, string materialtype)
        {
            List<string> retMaterialSize = new List<string>();

            using (SqlConnection con = new SqlConnection(appSettings.MWConnectionString))
            {
                string query = "spGetMaterialSize";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@SearchTerm", SqlDbType.VarChar, 100).Value = term.Replace("'", "''");
                    cmd.Parameters.Add("@MaterialType", SqlDbType.VarChar, 100).Value = materialtype.Replace("'", "''");
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        retMaterialSize.Add(reader["mwSize"].ToString());
                    }
                }
                con.Close();
            }
            return retMaterialSize;
        }

        public List<string> GetSSPOperations(string term, string partid)
        {
            List<string> retOperations = new List<string>();

            using (SqlConnection con = new SqlConnection(appSettings.MWConnectionString))
            {
                string query = "spGetSSPOperations";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@SearchTerm", SqlDbType.VarChar, 100).Value = term.Replace("'", "''");
                    cmd.Parameters.Add("@PartId", SqlDbType.Int).Value = partid;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        retOperations.Add(reader["Operation"].ToString());
                    }
                }
                con.Close();
            }
            return retOperations;
        }

        public List<string> GetCuttingMethodTemplate(string cuttingMethod, string N)
        {
            List<string> retTemplate = new List<string>();
            string twoDigitN = N.PadLeft(2, '0');

            using (SqlConnection con = new SqlConnection(appSettings.MWConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("spGetCuttingMethodTemplate", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@CuttingMethod", SqlDbType.VarChar, 50).Value = cuttingMethod;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string Template = reader["Template"].ToString();
                        string[] lines = Template.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.None);
                        int iTindex = 0;
                        StringBuilder sbLines = new StringBuilder();
                        for (int i = 0; i < lines.Count(); i++)
                        {
                            //if (i == 0) lines[0] = "N" + twoDigitN;
                            Match match = Regex.Match(lines[i], @"[T][0-9]{4}");
                            if (match.Success)
                            {
                                iTindex++;
                                if (iTindex == 1 || iTindex == 3) lines[i] = Regex.Replace(lines[i], @"[T][0-9]{4}", "T" + twoDigitN + "00");
                                if (iTindex == 2) lines[i] = Regex.Replace(lines[i], @"[T][0-9]{4}", "T" + twoDigitN + twoDigitN);
                            }
                            sbLines.AppendLine(lines[i]);
                        }
                        retTemplate.Add(sbLines.ToString());
                        //retTemplate.Add(reader["Template"].ToString());
                    }
                }
                con.Close();
            }
            return retTemplate;
        }
        public List<ToolSetupSheetHeader> GetSetupSheets(string partnumber, string revision, string operation)
        {
            List<ToolSetupSheetHeader> setupSheetHeaders = new List<ToolSetupSheetHeader>();
            try
            {
                using (SqlConnection con = new SqlConnection(appSettings.MWConnectionString))
                {
                    string query = "spGetSetupSheets";
                    if (partnumber == "recent")
                        query = "spGetRecentSetupSheets";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        if (partnumber != "recent")
                        {
                            cmd.Parameters.Add("@partnumber", SqlDbType.VarChar, 100).Value = partnumber.Replace("'", "''");
                            cmd.Parameters.Add("@revision", SqlDbType.VarChar, 20).Value = revision;
                            if (operation != string.Empty)
                                cmd.Parameters.Add("@operation", SqlDbType.VarChar, 10).Value = operation;
                        }
                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            setupSheetHeaders.Add(
                                new ToolSetupSheetHeader
                                {
                                    ID = Convert.ToInt32(reader["ID"].ToString()),
                                    //PartName = reader["partname"].ToString(),
                                    PartNumber = reader["partnumber"].ToString(),
                                    Revision = reader["revision"].ToString(),
                                    Operation = reader["operation"].ToString()
                                });
                        }
                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return setupSheetHeaders;
        }

        //SaveToolInventoryCodeColumns

        public DBResponse SaveToolInventoryCodeColumns(SaveCodeColumnsRequest saveCodeColumnsRequest)
        {
            DBResponse dbResponse = new DBResponse();

            try
            {
                using (SqlConnection con = new SqlConnection(appSettings.MWConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("spSaveToolInventoryCodeColumns", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Code", SqlDbType.VarChar, 50).Value = saveCodeColumnsRequest.Code;
                        cmd.Parameters.Add("@Columns", SqlDbType.VarChar).Value = saveCodeColumnsRequest.Columns;
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        dbResponse.RecordsAffected = rowsAffected;

                        if (rowsAffected > 0)
                        {
                            dbResponse.ReturnCode = 0;
                        }
                        else
                        {
                            dbResponse.ReturnCode = -1;
                            dbResponse.Message = "No update";
                        }
                    }
                    con.Close();
                }
            }
            catch
            {
                dbResponse.ReturnCode = -1;
                throw;
            }

            return dbResponse;
        }

        public DBResponse CopyToolInventoryCodeColumns(CopyCodeColumnsRequest copyCodeColumnsRequest)
        {
            DBResponse dbResponse = new DBResponse();

            try
            {
                using (SqlConnection con = new SqlConnection(appSettings.MWConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("CopyToolInventoryCodeColumns", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Code", SqlDbType.VarChar, 50).Value = copyCodeColumnsRequest.Code;
                        cmd.Parameters.Add("@CopyToCode", SqlDbType.VarChar, 50).Value = copyCodeColumnsRequest.CopyToCode;
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        dbResponse.RecordsAffected = rowsAffected;

                        if (rowsAffected > 0)
                        {
                            dbResponse.ReturnCode = 0;
                        }
                        else
                        {
                            dbResponse.ReturnCode = -1;
                            dbResponse.Message = "No update";
                        }
                    }
                    con.Close();
                }
            }
            catch
            {
                dbResponse.ReturnCode = -1;
                throw;
            }

            return dbResponse;
        }
        public string ConvertProgram(ConvertProgramRequest convertProgramRequest)
        {
            //Get all rules then cache
            List<ConversionRule> lstAllRules = new List<ConversionRule>();
            if (GetFromCache("ConversionRules") == null)
            {
                using (SqlConnection con = new SqlConnection(appSettings.MWConnectionString))
                {
                    string query = "spGetConversionRules";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            lstAllRules.Add(new ConversionRule
                            {
                                FromMachineId = reader["FromMachineId"].ToString(),
                                ToMachineId = reader["ToMachineId"].ToString(),
                                FromSnippet = reader["FromSnippet"].ToString(),
                                ToSnippet = reader["ToSnippet"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
                AddToCache("ConversionRules", lstAllRules);
            }
            lstAllRules = (List<ConversionRule>)GetFromCache("ConversionRules");
            StringBuilder newLines = new StringBuilder();

            try
            {
                List<ConversionRule> lstRules = lstAllRules.FindAll(
                    rule => rule.FromMachineId == convertProgramRequest.FromMachineID && rule.ToMachineId == convertProgramRequest.ToMachineID);
                string[] lines = convertProgramRequest.Program.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

                for (int i = 0; i < lines.Length; i++)
                {
                    string newLine = ConvertLine(lines[i], lstRules);
                    newLines.AppendLine(newLine);
                }
            }
            catch 
            {
                throw;
            }

            return newLines.ToString();
        }

        private string ConvertLine(string line, List<ConversionRule> lstRules)
        {
            foreach (ConversionRule rule in lstRules)
            {
                line = line.Replace(rule.FromSnippet, rule.ToSnippet);
            }
            return line;
        }

        public List<string> GetToolNames(string searchTerm)
        {
            List<string> toolNames = new List<string>();
            using (SqlConnection conn = new SqlConnection(appSettings.MWConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "GetToolNames";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    cmd.Parameters.Add("@SearchTerm", SqlDbType.VarChar, 50).Value = searchTerm;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            toolNames.Add(reader["ToolName"].ToString());
                        }
                    }
                }
            }
            return toolNames;
        }

        public VendorInfo GetVendorInfo(int ID)
        {
            VendorInfo vendorInfo = new VendorInfo();
            using (SqlConnection conn = new SqlConnection(appSettings.MWConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "GetVendorInfo";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    cmd.Parameters.Add("@ID", SqlDbType.Int).Value = ID;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            vendorInfo.Address1 = reader["Address1"].ToString();
                            vendorInfo.Address2 = reader["Address2"].ToString();
                            vendorInfo.City = reader["City"].ToString();
                            vendorInfo.State = reader["State"].ToString();
                            vendorInfo.Zip = reader["Zip"].ToString();
                            vendorInfo.Country = reader["Country"].ToString();
                            vendorInfo.Phone = reader["Phone"].ToString();
                            vendorInfo.Fax = reader["Fax"].ToString();
                            vendorInfo.Mobile = reader["Mobile"].ToString();
                            vendorInfo.Website = reader["Website"].ToString();
                            vendorInfo.Email = reader["Email"].ToString();
                            vendorInfo.TollFree = reader["Tollfree"].ToString();
                        }
                    }
                }
            }
            return vendorInfo;
        }
        public List<Company> GetVendors(string searchTerm, int CategoryID)
        {
            List<Company> companies = new List<Company>();
            using (SqlConnection conn = new SqlConnection(appSettings.MWConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "GetVendors";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = conn;
                    cmd.Parameters.Add("@SearchTerm", SqlDbType.VarChar, 50).Value = searchTerm;
                    cmd.Parameters.Add("@CategoryID", SqlDbType.Int).Value = CategoryID;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            companies.Add(new Company
                            {
                                ID = Convert.ToInt32(reader["ID"].ToString()),
                                CompanyName = reader["CompanyName"].ToString(),
                                CompanyID = reader["CompanyID"].ToString()
                            });
                        }
                    }
                }
            }
            return companies;
        }

        public void SaveToolImageInfo(int toolID, string fileName)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(appSettings.MWConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SaveToolImageInfo", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ToolID", SqlDbType.Int).Value = toolID;
                        cmd.Parameters.Add("@FileName", SqlDbType.VarChar, 50).Value = fileName;
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
            }
            catch
            {
                throw;
            }

        }
        public DBResponse SaveLookupCategory(SaveLookupCategoryRequest saveLookupCategoryRequest)
        {
            DBResponse dbResponse = new DBResponse();
            StringBuilder sbValue = new StringBuilder();

            try
            {
                using (SqlConnection con = new SqlConnection(appSettings.MWConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SaveLookupCategory", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Category", SqlDbType.VarChar, 50).Value = saveLookupCategoryRequest.Category;
                        cmd.Parameters.Add("@ModifiedBy", SqlDbType.VarChar, 30).Value = saveLookupCategoryRequest.ModifiedBy;

                        foreach (LookupCategoryValue item in saveLookupCategoryRequest.LookupCategoryValues)
                        {
                            sbValue.AppendFormat("{0}^{1};{2}|{3},", item.ID, item.Value, item.Text, item.active);
                        }

                        cmd.Parameters.Add("@Values", SqlDbType.VarChar).Value = sbValue.Remove(sbValue.Length - 1, 1).ToString();
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        dbResponse.RecordsAffected = rowsAffected;

                        if (rowsAffected > 0)
                        {
                            dbResponse.ReturnCode = 0;
                        }
                        else
                        {
                            dbResponse.ReturnCode = -1;
                            dbResponse.Message = "No update";
                        }
                    }
                    con.Close();
                }
            }
            catch
            {
                dbResponse.ReturnCode = -1;
                throw;
            }

            return dbResponse;
        }
        public void RefreshLookupCaches()
        {
            RefreshLookupCache("Unit");
            RefreshLookupCache("MaterialType");
            RefreshLookupCache("MaterialForm");
            RefreshLookupCache("MaterialSize");
            RefreshLookupCache("MaterialHeatTreated");
            RefreshLookupCache("MachineWorkHoldingTo");
            RefreshLookupCache("Torque");
            RefreshLookupCache("HoldPartOn");
            RefreshLookupCache("Z0");
            RefreshLookupCache("BarStickOutBefore");
            RefreshLookupCache("FaceOff");
            RefreshLookupCache("CutOffToolThickness");
            RefreshLookupCache("BarStickOutAfter");
            RefreshLookupCache("OAL");
            RefreshLookupCache("PartStickOutMinimum");
            RefreshLookupCache("MachineType");
        }
        public void RefreshLookupCache(string category)
        {

            List<string> retResults = new List<string>();

            using (SqlConnection con = new SqlConnection(appSettings.MWConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("spGetLookupValues", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Category", SqlDbType.VarChar, 100).Value = category;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        retResults.Add(string.Format("{0}|{1}", reader["Name"], reader["Description"]));
                    }
                }
                con.Close();
                cache.Remove(category);
                AddToCache(category, retResults);
            }

        }

        public void Dispose()
        {

        }
    }

}