using MWWebAPI2.Models;
using System.Collections.Generic;
using MWWebAPI2.DBRepository;
using System.Web;
using System.Linq;
using System;
using System.IO;
using System.Configuration;
using System.Net.Http;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MWWebAPI2.Controllers
{
    //[Authorize]
    [Route("api")]
    public class ToolInventoryController : BaseApiController
    {
         private static AppSettings appSettings= new AppSettings();       
        private static string imageLibrary = "appSettings.ImageLibrary";
        private static string imageUrl = appSettings.ImageUrl;
       DBToolInventoryRepository ToolInventoryRepo = null;
        public ToolInventoryController(AppSettings _appSettings)           
        {
            appSettings = _appSettings;  
            ToolInventoryRepo = new DBToolInventoryRepository(
            appSettings, null, null, null
        );          
        }
       
        [Route("SearchToolSetups")]
        [HttpPost]
        public IActionResult SearchToolSetups(ToolSetupSearchRequest toolSetupSearchRequest)
        {
            List<ToolSetupSearchResult> retSearchResult = ToolInventoryRepo.SearchToolSetups(toolSetupSearchRequest.SearchTerm);
            return StatusCode(StatusCodes.Status200OK, retSearchResult);
        }

        [Route("lookup")]
        [HttpGet]
        public IActionResult GetByCategory([FromQuery]LookUpRequest lookUpRequest)
        {
            return StatusCode(StatusCodes.Status200OK, ToolInventoryRepo.GetLookupByCategory(lookUpRequest.Category, lookUpRequest.SearchTerm));
        }

        [Route("GetToolCategoryNames")]       
        public IActionResult GetToolCategoryNames()
        {
            return StatusCode(StatusCodes.Status200OK, ToolInventoryRepo.GetToolCategoryNames());
        }

        [Route("GetCuttingMethodsWithTemplate")]
        [HttpGet]
        public IActionResult GetCuttingMethodsWithTemplate([FromQuery]ToolSetupSearchRequest toolSetupSearchRequest)
        {
            return StatusCode(StatusCodes.Status200OK, ToolInventoryRepo.GetCuttingMethodsWithTemplate(toolSetupSearchRequest.SearchTerm));
        }

        [Route("cuttingmethodtemplate/update")]
        [HttpPost]
        public IActionResult UpdateCuttingMethodTemplate(CuttingMethodTemplate cuttingMethodTemplate)
        {
            DBResponse dbResponse = ToolInventoryRepo.UpdateCuttingMethodTemplate(cuttingMethodTemplate);
            return StatusCode(StatusCodes.Status200OK, new APIResponse
            {
                ResponseCode = 0,
                ResponseText = "Template updated"
            });
        }

        [Route("getCuttingMethodtemplate/{cuttingMethod}")]       
        public IActionResult GetTemplate(string cuttingMethod)
        {
            // URL can't handle period so use | instead
            cuttingMethod = cuttingMethod.Replace('|', '.');
            return StatusCode(StatusCodes.Status200OK, ToolInventoryRepo.GetCuttingMethodTemplate(cuttingMethod));
        }

        [Route("getToolSetupSheet/{id}")]
        public ToolSetupSheet GetToolSetupSheet(int id)
        {
            return ToolInventoryRepo.GetToolSetupSheet(id);
        }

        [Route("ToolSetupSheet/Update")]
        [HttpPost]
        public IActionResult UpdateToolSetupSheet(ToolSetupSheet toolSetupSheet)
        {
            DBResponse dbResponse = ToolInventoryRepo.UpdateToolSetupSheet(toolSetupSheet);
            return StatusCode(StatusCodes.Status200OK, new APIResponse
            {
                ResponseCode = 0,
                ResponseText = "Setup Sheet updated"
            });
        }

        [Route("AddToolSetupToSetupSheet")]
        [HttpPost]
        public IActionResult AddToolSetupToSetupSheet(AddToolSetupRequest addToolSetupRequest)
        {
            DBResponse dbResponse = ToolInventoryRepo.AddToolSetupToSetupSheet(addToolSetupRequest);
            return StatusCode(StatusCodes.Status200OK, new APIResponse
            {
                ResponseCode = 0,
                ResponseText = "Setup Sheet updated"
            });
        }

        [Route("DeleteConversionRule")]
        [HttpPost]
        public IActionResult DeleteConversionRule(ConversionRule conversionRule)
        {

            DBResponse dbResponse = ToolInventoryRepo.DeleteConversionRule(conversionRule);
            return StatusCode(StatusCodes.Status200OK, new APIResponse
            {
                ResponseCode = 0,
                ResponseText = "Setup Sheet updated"
            });
        }

        [Route("DeleteToolSetup")]
        [HttpPost]
        public IActionResult DeleteToolSetup(DeleteToolSetupRequest deleteToolSetupRequest)
        {

            DBResponse dbResponse = ToolInventoryRepo.DeleteToolSetup(deleteToolSetupRequest);
            return StatusCode(StatusCodes.Status200OK, new APIResponse
            {
                ResponseCode = 0,
                ResponseText = "Setup Sheet updated"
            });
        }

        [Route("SaveToolSetupGroup")]
        [HttpPost]
        public IActionResult SaveToolSetupGroup(ToolSetupGroupRequest toolSetupGroupRequest)
        {

            DBResponse dbResponse = ToolInventoryRepo.SaveToolSetupGroup(toolSetupGroupRequest);
            return StatusCode(StatusCodes.Status200OK, new APIResponse
            {
                ResponseCode = 0,
                ResponseText = "Tool Setup Group updated"
            });
        }

        [Route("SaveConvertedProgram")]
        [HttpPost]
        public IActionResult SaveConvertedProgram(ProgramSaveRequest convertProgramSaveRequest)
        {

            int newSetupSheetID = ToolInventoryRepo.SaveConvertedProgram(convertProgramSaveRequest);
            return StatusCode(StatusCodes.Status200OK, newSetupSheetID);            
        }

        [Route("UploadProvenProgram")]
        [HttpPost]
        public IActionResult UploadProvenProgram(UploadProgramRequest uploadProgramRequest)
        {

            int newSetupSheetID = ToolInventoryRepo.UploadProvenProgram(uploadProgramRequest);
            return StatusCode(StatusCodes.Status200OK, newSetupSheetID);
        }

        [Route("SaveProgram")]
        [HttpPost]
        public IActionResult SaveProgram(ProgramSaveRequest convertProgramSaveRequest)
        {
            ToolInventoryRepo.SaveProgram(convertProgramSaveRequest);
            return StatusCode(StatusCodes.Status200OK, 0);          
        }

        [Route("GetSearchResults/{category}/{term?}")]
        public IActionResult GetSearchResults(string category, string term = "")
        {
            List<string> retResults = ToolInventoryRepo.GetSearchResults(term, category);

            var resultList = retResults.Where(x => x.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1);
            return StatusCode(StatusCodes.Status200OK, resultList.ToArray());
        }

        [Route("GetMachines/{prefix?}")]
        public IActionResult GetMachines(string prefix = "")
        {
            List<string> retPartNumbers = ToolInventoryRepo.GetMachines(prefix);
            return StatusCode(StatusCodes.Status200OK, retPartNumbers.ToArray());
        }

        [Route("GetLookUpCategories/{searchTerm?}")]
        public IActionResult GetLookUpCategories(string searchTerm = "")
        {
            List<string> getCategories = ToolInventoryRepo.GetLookUpCategories(searchTerm);
            return StatusCode(StatusCodes.Status200OK, getCategories.ToArray());
        }

        [Route("GetSSPPartNumbers/{partId?}")]
        public IActionResult GetSSPPartNumbers(string partId = "")
        {
            List<string> retPartNumbers = ToolInventoryRepo.GetSSPPartNumbers(partId);
            var resultList = retPartNumbers.Where(x => x.IndexOf(partId, StringComparison.OrdinalIgnoreCase) > -1);
            return StatusCode(StatusCodes.Status200OK, resultList.ToArray());
        }

        [Route("GetMaterialSize/{materialType}/{term}")]
        public IActionResult GetSSPPartNumbers(string materialType, string term)
        {
            List<string> retMaterialSize = ToolInventoryRepo.GetMaterialSize(term, materialType);
            var resultList = retMaterialSize.Where(x => x.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1);
            return StatusCode(StatusCodes.Status200OK, resultList.ToArray());
        }

        [Route("GetSSPOperations/{partId}/{operation?}")]
        public IActionResult GetSSPOperations(string partId, string operation = "")
        {
            List<string> retOperations = ToolInventoryRepo.GetSSPOperations(operation, partId);
            var resultList = retOperations.Where(x => x.IndexOf(operation, StringComparison.OrdinalIgnoreCase) > -1);
            return StatusCode(StatusCodes.Status200OK, resultList.ToArray());
        }

        [Route("GetCuttingMethodTemplate/{cuttingMethod}/{n}")]
        public IActionResult GetCuttingMethodTemplate(string cuttingMethod, string n)
        {
            List<string> retTemplate = ToolInventoryRepo.GetCuttingMethodTemplate(cuttingMethod, n);
            return StatusCode(StatusCodes.Status200OK, retTemplate.ToArray());
        }

        [Route("GetSetupSheets/{partNumber}/{revision}/{operation?}")]
        public IActionResult GetSetupSheets(string partnumber, string revision, string operation = "")
        {
            List<ToolSetupSheetHeader> setupSheetHeaders = ToolInventoryRepo.GetSetupSheets(partnumber, revision, operation);
            return StatusCode(StatusCodes.Status200OK, setupSheetHeaders);
        }

        [Route("ConvertProgram")]
        [HttpPost]
        public IActionResult ConvertProgram(ConvertProgramRequest convertProgramRequest)
        {
            return StatusCode(StatusCodes.Status200OK, ToolInventoryRepo.ConvertProgram(convertProgramRequest));
        }

        [Route("UploadProgram")]
        [HttpPost]
         public async Task<IActionResult> UploadProgam(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Content("file not selected");

            var path = Path.Combine(
                        appSettings.ProvenProgramsPath, 
                        file.Name);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return RedirectToAction("Files");
        }

        [Route("GetToolInventoryColumns")]
        public IActionResult GetToolInventoryColumns(string code)
        {
            return StatusCode(StatusCodes.Status200OK, ToolInventoryRepo.GetToolInventoryColumns());
        }

        [Route("GetToolInventoryColumns/{code}")]
        public IActionResult GetToolInventoryColumnsByCode(string code)
        {
            return StatusCode(StatusCodes.Status200OK, ToolInventoryRepo.GetToolInventoryColumnsByCode(code));
           
        }

        [Route("SaveToolInventoryCodeColumns")]
        [HttpPost]
        public IActionResult SaveToolInventoryCodeColumns(SaveCodeColumnsRequest saveCodeColumnsRequest)
        {
            return StatusCode(StatusCodes.Status200OK, ToolInventoryRepo.SaveToolInventoryCodeColumns(saveCodeColumnsRequest));
        }

        [Route("CopyToolInventoryCodeColumns")]
        [HttpPost]
        public IActionResult CopyToolInventoryCodeColumns(CopyCodeColumnsRequest copyCodeColumnsRequest)
        {
            return StatusCode(StatusCodes.Status200OK, ToolInventoryRepo.CopyToolInventoryCodeColumns(copyCodeColumnsRequest));
        }

        [Route("GetSelectedToolInventoryColumns/{codes?}")]
        public IActionResult GetSelectedToolInventoryColumns(string codes = "")
        {
            return StatusCode(StatusCodes.Status200OK, ToolInventoryRepo.GetSelectedToolInventoryColumns(codes));
        }

        [Route("GetToolNames")]
        [HttpGet]
        public IActionResult GetToolNames([FromQuery] LookUpRequest lookUpRequest)
        {
            return StatusCode(StatusCodes.Status200OK, ToolInventoryRepo.GetToolNames(lookUpRequest.SearchTerm));
        }

        [Route("GetSearchableToolInventoryColumns/{codes?}")]
        public IActionResult GetSearchableToolInventoryColumns(string codes = "")
        {
            return StatusCode(StatusCodes.Status200OK, ToolInventoryRepo.GetSelectedToolInventoryColumns(codes, true));
        }

        [Route("GetToolDetails/{ToolID}")]
        public IActionResult GetToolDetails(int ToolID)
        {
            return StatusCode(StatusCodes.Status200OK, ToolInventoryRepo.GetToolDetails(ToolID));
        }

        [Route("CopyTool")]
        [HttpPost]
        public IActionResult CopyTool(ToolInventorySaveRequest copyToolRequest)
        {
            return StatusCode(StatusCodes.Status200OK, ToolInventoryRepo.CopyTool(copyToolRequest.ID));
        }

        [Route("CreateTool")]
        [HttpPost]
        public IActionResult CreateTool(ToolInventorySaveRequest toolInventorySaveRequest)
        {
            return StatusCode(StatusCodes.Status200OK, ToolInventoryRepo.CreateTool(toolInventorySaveRequest));
        }


        [Route("DeleteTool")]
        [HttpPost]
        public IActionResult DeleteTool(ToolInventorySaveRequest toolInventorySaveRequest)
        {
            return StatusCode(StatusCodes.Status200OK, ToolInventoryRepo.DeleteTool(toolInventorySaveRequest.ID));
        }

        [Route("SaveToolDetails")]
        [HttpPost]
        public IActionResult SaveToolDetails(ToolInventorySaveRequest toolInventorySaveRequest)
        {
            return StatusCode(StatusCodes.Status200OK,ToolInventoryRepo.SaveToolDetails(toolInventorySaveRequest));
        }
        //

        [Route("UpdateToolVendor")]
        [HttpPost]
        public IActionResult UpdateToolVendor(ToolInventorySearchResult toolInventorySearchResult)
        {
            return StatusCode(StatusCodes.Status200OK,ToolInventoryRepo.UpdateToolVendor(toolInventorySearchResult));
        }

        [Route("ToolInventorySearch")]
        [HttpPost]
        public IActionResult ToolInventorySearch(ToolInventorySearch toolInventorySearch)
        {
            return StatusCode(StatusCodes.Status200OK,ToolInventoryRepo.ToolInventorySearch(toolInventorySearch));
        }
        
        [Route("GetLookUpCategory")]
        [HttpPost]
        public IActionResult GetLookUpCategory(LookupCategorySearch lookupCategorySearch)
        {
            return StatusCode(StatusCodes.Status200OK,ToolInventoryRepo.GetLookUpCategory(lookupCategorySearch));
        }

        [Route("GetToolCuttingMethods/{ToolID}/{AllMethods}")]
        public IActionResult GetToolCuttingMethods(int ToolID, bool allMethods = true)
        {
            return StatusCode(StatusCodes.Status200OK,ToolInventoryRepo.GetToolCuttingMethods(ToolID, allMethods));
        }

        [Route("GetVendors/{categoryID?}/{searchTerm?}")]
        public IActionResult GetVendors(string searchTerm = "", int categoryID = 0)
        {
            return StatusCode(StatusCodes.Status200OK,ToolInventoryRepo.GetVendors(searchTerm, categoryID));
        }

        [Route("GetVendorInfo/{ID}")]
        public IActionResult GetVendorInfo(int ID)
        {
            return StatusCode(StatusCodes.Status200OK,ToolInventoryRepo.GetVendorInfo(ID));
        }
        //public LookupCategories GetLookUpCategory(LookupCategorySearch lookupCategorySearch)

        [Route("ToolInventorySearchSelected")]
        [HttpPost]
        public IActionResult ToolInventorySearchSelected(ToolInventorySearch toolInventorySearch)
        {
            return StatusCode(StatusCodes.Status200OK,ToolInventoryRepo.ToolInventorySearchSelected(toolInventorySearch));
        }

        [Route("LinkTool")]
        [HttpPost]
        public IActionResult LinkTool(LinkToolRequest linkToolRequest)
        {
            DBResponse dbResponse = ToolInventoryRepo.LinkTool(linkToolRequest);
            return StatusCode(StatusCodes.Status200OK,new APIResponse
            {
                ResponseCode = 0,
                ResponseText = linkToolRequest.Action + " successful."
            });
        }

        [Route("CheckOutCheckIn")]
        [HttpPost]
        public IActionResult CheckOutCheckIn(CheckOutCheckInRequest checkOutCheckInRequest)
        {
            DBResponse dbResponse = ToolInventoryRepo.CheckOutCheckIn(checkOutCheckInRequest);
            return StatusCode(StatusCodes.Status200OK,new APIResponse
            {
                ResponseCode = 0,
                ResponseText = checkOutCheckInRequest.Action + " successful."
            });
        }

        [Route("SaveLookupCategory")]
        [HttpPost]
        public IActionResult SaveLookupCategory(SaveLookupCategoryRequest saveLookupCategoryRequest)
        {
            DBResponse dbResponse = ToolInventoryRepo.SaveLookupCategory(saveLookupCategoryRequest);
            return StatusCode(StatusCodes.Status200OK,new APIResponse
            {
                ResponseCode = 0,
                ResponseText = "Save LookupCategory successful."
            });
        }
        [Route("UploadToolImage")]
        [HttpPost]
        public IActionResult UploadToolImage()
        {
            /*
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                // Get the uploaded image from the Files collection
                var httpPostedFile = HttpContext.Current.Request.Files["UploadedImage"];

                if (httpPostedFile != null)
                {
                    int toolID = Convert.ToInt32(HttpContext.Current.Request["ToolID"]);
                    string fileFormat = httpPostedFile.FileName.Substring(httpPostedFile.FileName.LastIndexOf('.')+1);
                    string fileName = string.Format("{0}.{1}", toolID, fileFormat);
                    var fileSavePath = 
                        string.Format("{0}\\ToolInventory\\{1}.{2}", imageLibrary+"", toolID, fileFormat);

                    httpPostedFile.SaveAs(fileSavePath);
                    ToolInventoryRepo.SaveToolImageInfo(toolID, fileName);
                }
            }
            */
            return StatusCode(StatusCodes.Status200OK, "");
        }
    } 
}
