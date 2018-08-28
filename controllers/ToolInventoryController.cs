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
using Microsoft.AspNetCore.Mvc.WebApiCompatShim;

namespace MWWebAPI2.Controllers
{
    //[Authorize]
    [Route("api")]
    public class ToolInventoryController : BaseApiController
    {
        private static AppSettings appSettings = new AppSettings();
        private static string imageLibrary = string.Empty;
        private static string imageUrl = appSettings.ImageUrl;
        DBToolInventoryRepository ToolInventoryRepo = null;
        public ToolInventoryController(AppSettings _appSettings)
        {
            appSettings = _appSettings;
            imageLibrary = appSettings.ImageLibrary;
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
        //public IActionResult GetByCategory([FromQuery]LookUpRequest lookUpRequest)
        public HttpResponseMessage GetByCategory([FromQuery]LookUpRequest lookUpRequest)
        {
            //return StatusCode(StatusCodes.Status200OK, ToolInventoryRepo.GetLookupByCategory(lookUpRequest.Category, lookUpRequest.SearchTerm));
            using (HttpRequestMessage request = HttpRequestMessageHttpContextExtensions.GetHttpRequestMessage(HttpContext))
            {
                return request.CreateResponse(HttpStatusCode.OK, ToolInventoryRepo.GetLookupByCategory(lookUpRequest.Category, lookUpRequest.SearchTerm));
            }
        }

        [Route("GetToolCategoryNames")]
        [HttpGet]
        public HttpResponseMessage GetToolCategoryNames(string searchTerm = "")
        {
            using (HttpRequestMessage request = HttpRequestMessageHttpContextExtensions.GetHttpRequestMessage(HttpContext))
            {
                return request.CreateResponse(HttpStatusCode.OK, ToolInventoryRepo.GetToolCategoryNames(searchTerm));
            }
            //return StatusCode(StatusCodes.Status200OK, ToolInventoryRepo.GetToolCategoryNames());
        }

        [Route("GetCuttingMethodsWithTemplate")]
        [HttpGet]
        public HttpResponseMessage GetCuttingMethodsWithTemplate([FromQuery]ToolSetupSearchRequest toolSetupSearchRequest)
        {
            using (HttpRequestMessage request = HttpRequestMessageHttpContextExtensions.GetHttpRequestMessage(HttpContext))
            {
                return request.CreateResponse(HttpStatusCode.OK, ToolInventoryRepo.GetCuttingMethodsWithTemplate(toolSetupSearchRequest.SearchTerm));
            }
        }

        [Route("cuttingmethodtemplate/update")]
        [HttpPost]
        public IActionResult UpdateCuttingMethodTemplate([FromBody] CuttingMethodTemplate cuttingMethodTemplate)
        {
            DBResponse dbResponse = ToolInventoryRepo.UpdateCuttingMethodTemplate(cuttingMethodTemplate);
            return StatusCode(StatusCodes.Status200OK, new APIResponse
            {
                ResponseCode = 0,
                ResponseText = "Template updated"
            });
        }

        [Route("getCuttingMethodtemplate/{cuttingMethod}")]
        [HttpGet]
        public HttpResponseMessage GetTemplate(string cuttingMethod)
        {
            // URL can't handle period so use | instead
            cuttingMethod = cuttingMethod.Replace('|', '.');

            using (HttpRequestMessage request = HttpRequestMessageHttpContextExtensions.GetHttpRequestMessage(HttpContext))
            {
                return request.CreateResponse(HttpStatusCode.OK, ToolInventoryRepo.GetCuttingMethodTemplate(cuttingMethod));
            }
        }

        [Route("getToolSetupSheet/{id}")]
        [HttpGet]
        public HttpResponseMessage GetToolSetupSheet(int id)
        {
            using (HttpRequestMessage request = HttpRequestMessageHttpContextExtensions.GetHttpRequestMessage(HttpContext))
            {
                return request.CreateResponse(HttpStatusCode.OK, ToolInventoryRepo.GetToolSetupSheet(id));
            }
        }

        [Route("ToolSetupSheet/Update")]
        [HttpPost]
        public IActionResult UpdateToolSetupSheet([FromBody] ToolSetupSheet toolSetupSheet)
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
        public IActionResult AddToolSetupToSetupSheet([FromBody] AddToolSetupRequest addToolSetupRequest)
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
        public IActionResult DeleteConversionRule([FromBody] ConversionRule conversionRule)
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
        public IActionResult DeleteToolSetup([FromBody] DeleteToolSetupRequest deleteToolSetupRequest)
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
        public IActionResult SaveToolSetupGroup([FromBody] ToolSetupGroupRequest toolSetupGroupRequest)
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
        public IActionResult SaveConvertedProgram([FromBody] ProgramSaveRequest convertProgramSaveRequest)
        {

            int newSetupSheetID = ToolInventoryRepo.SaveConvertedProgram(convertProgramSaveRequest);
            return StatusCode(StatusCodes.Status200OK, newSetupSheetID);
        }

        [Route("UploadProvenProgram")]
        [HttpPost]
        public IActionResult UploadProvenProgram([FromBody] UploadProgramRequest uploadProgramRequest)
        {

            int newSetupSheetID = ToolInventoryRepo.UploadProvenProgram(uploadProgramRequest);
            return StatusCode(StatusCodes.Status200OK, newSetupSheetID);
        }

        [Route("SaveProgram")]
        [HttpPost]
        public IActionResult SaveProgram([FromBody] ProgramSaveRequest convertProgramSaveRequest)
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
        public HttpResponseMessage GetMachines(string prefix = "")
        {
            List<string> retPartNumbers = ToolInventoryRepo.GetMachines(prefix);

            using (HttpRequestMessage request = HttpRequestMessageHttpContextExtensions.GetHttpRequestMessage(HttpContext))
            {
                return request.CreateResponse(HttpStatusCode.OK, retPartNumbers.ToArray());
            }
        }

        [Route("GetLookUpCategories/{searchTerm?}")]
        [HttpGet]
        public HttpResponseMessage GetLookUpCategories(string searchTerm = "")
        {
            List<string> getCategories = ToolInventoryRepo.GetLookUpCategories(searchTerm);
            using (HttpRequestMessage request = HttpRequestMessageHttpContextExtensions.GetHttpRequestMessage(HttpContext))
            {
                return request.CreateResponse(HttpStatusCode.OK, getCategories.ToArray());
            }
        }

        [Route("GetSSPPartNumbers/{partId?}")]
        [HttpGet]
        public HttpResponseMessage GetSSPPartNumbers(string partId = "")
        {
            List<string> retPartNumbers = ToolInventoryRepo.GetSSPPartNumbers(partId);
            var resultList = retPartNumbers.Where(x => x.IndexOf(partId, StringComparison.OrdinalIgnoreCase) > -1);
            using (HttpRequestMessage request = HttpRequestMessageHttpContextExtensions.GetHttpRequestMessage(HttpContext))
            {
                return request.CreateResponse(HttpStatusCode.OK, resultList.ToArray());
            }
        }

        [Route("GetMaterialSize/{materialType}/{term}")]
        [HttpGet]
        public HttpResponseMessage GetSSPPartNumbers(string materialType, string term)
        {
            List<string> retMaterialSize = ToolInventoryRepo.GetMaterialSize(term, materialType);
            var resultList = retMaterialSize.Where(x => x.IndexOf(term, StringComparison.OrdinalIgnoreCase) > -1);
            using (HttpRequestMessage request = HttpRequestMessageHttpContextExtensions.GetHttpRequestMessage(HttpContext))
            {
                return request.CreateResponse(HttpStatusCode.OK, resultList.ToArray());
            }
        }

        [Route("GetSSPOperations/{partId}/{operation?}")]
        [HttpGet]
        public HttpResponseMessage GetSSPOperations(string partId, string operation = "")
        {
            List<string> retOperations = ToolInventoryRepo.GetSSPOperations(operation, partId);
            var resultList = retOperations.Where(x => x.IndexOf(operation, StringComparison.OrdinalIgnoreCase) > -1);
            using (HttpRequestMessage request = HttpRequestMessageHttpContextExtensions.GetHttpRequestMessage(HttpContext))
            {
                return request.CreateResponse(HttpStatusCode.OK, resultList.ToArray());
            }
        }

        [Route("GetCuttingMethodTemplate/{cuttingMethod}/{n}")]
        [HttpGet]
        public HttpResponseMessage GetCuttingMethodTemplate(string cuttingMethod, string n)
        {
            List<string> retTemplate = ToolInventoryRepo.GetCuttingMethodTemplate(cuttingMethod, n);
            using (HttpRequestMessage request = HttpRequestMessageHttpContextExtensions.GetHttpRequestMessage(HttpContext))
            {
                return request.CreateResponse(HttpStatusCode.OK, retTemplate.ToArray());
            }
        }

        [Route("GetSetupSheets/{partNumber}/{revision}/{operation?}")]
        [HttpGet]
        public HttpResponseMessage GetSetupSheets(string partnumber, string revision, string operation = "")
        {
            List<ToolSetupSheetHeader> setupSheetHeaders = ToolInventoryRepo.GetSetupSheets(partnumber, revision, operation);
            using (HttpRequestMessage request = HttpRequestMessageHttpContextExtensions.GetHttpRequestMessage(HttpContext))
            {
                return request.CreateResponse(HttpStatusCode.OK, setupSheetHeaders);
            }
        }

        [Route("ConvertProgram")]
        [HttpPost]
        public HttpResponseMessage ConvertProgram([FromBody] ConvertProgramRequest convertProgramRequest)
        {
            using (HttpRequestMessage request = HttpRequestMessageHttpContextExtensions.GetHttpRequestMessage(HttpContext))
            {
                return request.CreateResponse(HttpStatusCode.OK, ToolInventoryRepo.ConvertProgram(convertProgramRequest));
            }
        }

        [Route("UploadProgram")]
        [HttpPost]
        public async Task<IActionResult> UploadProgam([FromBody] IFormFile file)
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
        public HttpResponseMessage GetToolInventoryColumns(string code)
        {
            using (HttpRequestMessage request = HttpRequestMessageHttpContextExtensions.GetHttpRequestMessage(HttpContext))
            {
                return request.CreateResponse(HttpStatusCode.OK, ToolInventoryRepo.GetToolInventoryColumns());
            }
        }

        [Route("GetToolInventoryColumns/{code}")]
        public IActionResult GetToolInventoryColumnsByCode(string code)
        {
            return StatusCode(StatusCodes.Status200OK, ToolInventoryRepo.GetToolInventoryColumnsByCode(code));

        }

        [Route("SaveToolInventoryCodeColumns")]
        [HttpPost]
        public IActionResult SaveToolInventoryCodeColumns([FromBody] SaveCodeColumnsRequest saveCodeColumnsRequest)
        {
            return StatusCode(StatusCodes.Status200OK, ToolInventoryRepo.SaveToolInventoryCodeColumns(saveCodeColumnsRequest));
        }

        [Route("CopyToolInventoryCodeColumns")]
        [HttpPost]
        public IActionResult CopyToolInventoryCodeColumns([FromBody] CopyCodeColumnsRequest copyCodeColumnsRequest)
        {
            return StatusCode(StatusCodes.Status200OK, ToolInventoryRepo.CopyToolInventoryCodeColumns(copyCodeColumnsRequest));
        }

        [Route("GetSelectedToolInventoryColumns/{codes?}/{searchable?}")]
        [HttpGet]
        public HttpResponseMessage GetSelectedToolInventoryColumns(string codes = "", bool searchable = false)
        {
            using (HttpRequestMessage request = HttpRequestMessageHttpContextExtensions.GetHttpRequestMessage(HttpContext))
            {
                return request.CreateResponse(HttpStatusCode.OK, ToolInventoryRepo.GetSelectedToolInventoryColumns(codes, searchable));
            }
        }

        [Route("GetToolNames")]
        [HttpGet]
        public HttpResponseMessage GetToolNames([FromQuery] LookUpRequest lookUpRequest)
        {
            using (HttpRequestMessage request = HttpRequestMessageHttpContextExtensions.GetHttpRequestMessage(HttpContext))
            {
                return request.CreateResponse(HttpStatusCode.OK, ToolInventoryRepo.GetToolNames(lookUpRequest.SearchTerm));
            }
        }

        [Route("GetSearchableToolInventoryColumns/{codes?}")]
        [HttpGet]
        public HttpResponseMessage GetSearchableToolInventoryColumns(string codes = "")
        {
            using (HttpRequestMessage request = HttpRequestMessageHttpContextExtensions.GetHttpRequestMessage(HttpContext))
            {
                return request.CreateResponse(HttpStatusCode.OK, ToolInventoryRepo.GetSelectedToolInventoryColumns(codes, true));
            }
        }

        [Route("GetToolDetails/{ToolID}")]
        [HttpGet]
        public HttpResponseMessage GetToolDetails(int ToolID)
        {
            using (HttpRequestMessage request = HttpRequestMessageHttpContextExtensions.GetHttpRequestMessage(HttpContext))
            {
                return request.CreateResponse(HttpStatusCode.OK, ToolInventoryRepo.GetToolDetails(ToolID));
            }
        }

        [Route("CopyTool")]
        [HttpPost]
        public IActionResult CopyTool([FromBody] ToolInventorySaveRequest copyToolRequest)
        {
            return StatusCode(StatusCodes.Status200OK, ToolInventoryRepo.CopyTool(copyToolRequest.ID));
        }

        [Route("CreateTool")]
        [HttpPost]
        public IActionResult CreateTool([FromBody] ToolInventorySaveRequest toolInventorySaveRequest)
        {
            return StatusCode(StatusCodes.Status200OK, ToolInventoryRepo.CreateTool(toolInventorySaveRequest));
        }

        [Route("DeleteTool")]
        [HttpPost]
        public IActionResult DeleteTool([FromBody] ToolInventorySaveRequest toolInventorySaveRequest)
        {
            return StatusCode(StatusCodes.Status200OK, ToolInventoryRepo.DeleteTool(toolInventorySaveRequest.ID));
        }

        [Route("SaveToolDetails")]
        [HttpPost]
        public IActionResult SaveToolDetails([FromBody] ToolInventorySaveRequest toolInventorySaveRequest)
        {
            return StatusCode(StatusCodes.Status200OK, ToolInventoryRepo.SaveToolDetails(toolInventorySaveRequest));
        }
        //

        [Route("UpdateToolVendor")]
        [HttpPost]
        public IActionResult UpdateToolVendor([FromBody] ToolInventorySearchResult toolInventorySearchResult)
        {
            return StatusCode(StatusCodes.Status200OK, ToolInventoryRepo.UpdateToolVendor(toolInventorySearchResult));
        }

        [Route("ToolInventorySearch")]
        [HttpGet]
        public IActionResult ToolInventorySearch([FromQuery] ToolInventorySearch toolInventorySearch)
        {
            return StatusCode(StatusCodes.Status200OK, ToolInventoryRepo.ToolInventorySearch(toolInventorySearch));
        }

        [Route("GetLookUpCategory")]
        [HttpGet]
        public HttpResponseMessage GetLookUpCategory([FromBody] LookupCategorySearch lookupCategorySearch)
        {
            using (HttpRequestMessage request = HttpRequestMessageHttpContextExtensions.GetHttpRequestMessage(HttpContext))
            {
                return request.CreateResponse(HttpStatusCode.OK, ToolInventoryRepo.GetLookUpCategory(lookupCategorySearch));
            }
        }

        [Route("GetToolCuttingMethods/{ToolID}/{AllMethods}")]
        [HttpGet]
        public HttpResponseMessage GetToolCuttingMethods(int ToolID, bool allMethods = true)
        {
            using (HttpRequestMessage request = HttpRequestMessageHttpContextExtensions.GetHttpRequestMessage(HttpContext))
            {
                return request.CreateResponse(HttpStatusCode.OK, ToolInventoryRepo.GetToolCuttingMethods(ToolID, allMethods));
            }
        }

        [Route("GetVendors/{categoryID?}/{searchTerm?}")]
        [HttpGet]
        public HttpResponseMessage GetVendors(string searchTerm = "", int categoryID = 0)
        {
            using (HttpRequestMessage request = HttpRequestMessageHttpContextExtensions.GetHttpRequestMessage(HttpContext))
            {
                return request.CreateResponse(HttpStatusCode.OK, ToolInventoryRepo.GetVendors(searchTerm, categoryID));
            }
        }

        [Route("GetVendorInfo/{ID}")]
        [HttpGet]
        public HttpResponseMessage GetVendorInfo(int ID)
        {
            using (HttpRequestMessage request = HttpRequestMessageHttpContextExtensions.GetHttpRequestMessage(HttpContext))
            {
                return request.CreateResponse(HttpStatusCode.OK, ToolInventoryRepo.GetVendorInfo(ID));
            }
        }
        //public LookupCategories GetLookUpCategory(LookupCategorySearch lookupCategorySearch)

        [Route("ToolInventorySearchSelected")]
        [HttpGet]
        public HttpResponseMessage ToolInventorySearchSelected([FromBody] ToolInventorySearch toolInventorySearch)
        {
            using (HttpRequestMessage request = HttpRequestMessageHttpContextExtensions.GetHttpRequestMessage(HttpContext))
            {
                return request.CreateResponse(HttpStatusCode.OK, ToolInventoryRepo.ToolInventorySearchSelected(toolInventorySearch));
            }
        }

        [Route("LinkTool")]
        [HttpPost]
        public IActionResult LinkTool([FromBody] LinkToolRequest linkToolRequest)
        {
            DBResponse dbResponse = ToolInventoryRepo.LinkTool(linkToolRequest);
            return StatusCode(StatusCodes.Status200OK, new APIResponse
            {
                ResponseCode = 0,
                ResponseText = linkToolRequest.Action + " successful."
            });
        }

        [Route("CheckOutCheckIn")]
        [HttpPost]
        public IActionResult CheckOutCheckIn([FromBody] CheckOutCheckInRequest checkOutCheckInRequest)
        {
            DBResponse dbResponse = ToolInventoryRepo.CheckOutCheckIn(checkOutCheckInRequest);
            return StatusCode(StatusCodes.Status200OK, new APIResponse
            {
                ResponseCode = 0,
                ResponseText = checkOutCheckInRequest.Action + " successful."
            });
        }

        [Route("SaveLookupCategory")]
        [HttpPost]
        public IActionResult SaveLookupCategory([FromBody] SaveLookupCategoryRequest saveLookupCategoryRequest)
        {
            DBResponse dbResponse = ToolInventoryRepo.SaveLookupCategory(saveLookupCategoryRequest);
            return StatusCode(StatusCodes.Status200OK, new APIResponse
            {
                ResponseCode = 0,
                ResponseText = "Save LookupCategory successful."
            });
        }
        [Route("UploadToolImage")]
        [HttpPost]
        public async Task<IActionResult> UploadToolImage()
        {

            var httpPostedFile = HttpContext.Request.Form.Files["toolImage"];

            if (httpPostedFile != null)
            {
                int toolID = Convert.ToInt32(HttpContext.Request.Form["toolId"]);
                string fileFormat = httpPostedFile.FileName.Substring(httpPostedFile.FileName.LastIndexOf('.') + 1);
                string fileName = string.Format("{0}.{1}", toolID, fileFormat);
                var fileSavePath =
                    string.Format("{0}\\{1}.{2}", imageLibrary + "", toolID, fileFormat);

                try
                {
                    using (var targetStream = System.IO.File.Create(fileSavePath))
                    {
                        await httpPostedFile.CopyToAsync(targetStream);
                        ToolInventoryRepo.SaveToolImageInfo(toolID, fileName);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }                
            }

            return StatusCode(StatusCodes.Status200OK, "");
        }
    }
}