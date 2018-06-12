using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace MWWebAPI2.Models
{
    /// <summary>
    /// Summary description for ToolSetupSheetModel
    /// </summary>
    public class ToolSetUp
    {
        public int ID { get; set; }
        public string Sequence { get; set; }
        public string N { get; set; }
        public string ToolNumber { get; set; }
        public string TONumber { get; set; }
        public string CuttingMethod { get; set; }
        public string SpecialComment { get; set; }
        public int PartsPerCorner { get; set; }
        public int SecondsPerTool { get; set; }
        public string Snippet { get; set; }
        public string ToolItem { get; set; }
        public string ToolName { get; set; }
        public string ToolMWID { get; set; }
        public string ToolDesc { get; set; }
        public string ToolLoc { get; set; }
        public string ToolHolder1Item { get; set; }
        public string ToolHolder1Name { get; set; }
        public string ToolHolder1MWID { get; set; }
        public string ToolHolder1Loc { get; set; }
        public string ToolHolder2Item { get; set; }
        public string ToolHolder2Name { get; set; }
        public string ToolHolder2MWID { get; set; }
        public string ToolHolder2Loc { get; set; }
        public string ToolHolder3Item { get; set; }
        public string ToolHolder3Name { get; set; }
        public string ToolHolder3MWID { get; set; }
        public string ToolHolder3Loc { get; set; }
        public string ToolID { get; set; }
        public string ToolHolder1ID { get; set; }
        public string ToolHolder2ID { get; set; }
        public string ToolHolder3ID { get; set; }
        public string ToolImage { get; set; }
        public string ToolHolder1Image { get; set; }
        public string ToolHolder2Image { get; set; }
        public string ToolHolder3Image { get; set; }
        public string Comment { get; set; }
        public string ModifiedBy { get; set; }
    }
    public class ToolSetupSheet
    {
        public int SetUpSheetID { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string Revision { get; set; }
        public string Operation { get; set; }
        public string Machine { get; set; }
        public string InputDate { get; set; }
        public string ProgramNumber { get; set; }
        public string ProgramLocation { get; set; }
        public string UOM { get; set; }
        public string MaterialType { get; set; }
        public string MaterialHeatTreated { get; set; }
        public string MaterialForm { get; set; }
        public string MaterialSize { get; set; }
        public string MachineWorkHoldingTo { get; set; }
        public string CutWorkHoldingTo { get; set; }        
        public string workHolding1ItemNumber { get; set; }

        public string workHolding2ItemNumber { get; set; }
        public string workHolding3ItemNumber { get; set; }
        public string workHolding1ImagePath { get; set; }
        public string workHolding2ImagePath { get; set; }
        public string workHolding3ImagePath { get; set; }
        public string workHolding1MWID { get; set; }
        public string workHolding2MWID { get; set; }
        public string workHolding3MWID { get; set; }
        public string workHolding1Location { get; set; }
        public string workHolding2Location { get; set; }
        public string workHolding3Location { get; set; }
        public string workHoldingComments { get; set; }
        public string workHoldingImageNoPart { get; set; }
        public string workHoldingImageWithPart { get; set; }
        public string workHoldingImageComplete { get; set; }
        public string Torque { get; set; }
        public string HoldPartOn { get; set; }
        public string BarStickOutBefore { get; set; }
        public string FaceOff { get; set; }
        public string Z0 { get; set; }
        public string CutOffToolThickness { get; set; }
        public string OAL { get; set; }
        public string BarStickOutAfter { get; set; }
        public string BarPullOut { get; set; }
        public string PartStickOutMinimum { get; set; }
        public string Comments { get; set; }
        public string Program { get; set; }
        public string ModifiedBy { get; set; }
        public List<ToolSetUp> ToolsSetUp { get; set; }

    }

    public class MWJsonResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
    }

    public class ToolSetupSheetHeader
    {
        public int ID { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string Revision { get; set; }
        public string Operation { get; set; }
    }

    public class ConversionRule
    {
        public int ID { get; set; }
        public string FromMachineId { get; set; }
        public string ToMachineId { get; set; }
        public string FromSnippet { get; set; }
        public string ToSnippet { get; set; }
        public string ModifiedBy { get; set; }
    }

    public class ConvertedProgramRequest
    {
        public int SetUpSheetID { get; set; }
        public string Program { get; set; }
        public string MachineId { get; set; }
        public string ModifiedBy { get; set; }
    }

    public class ToolSetupGroupRequest
    {
        public int MainID { get; set; }
        public int[] IDs { get; set; }
        public string ModifiedBy { get; set; }
    }

    public class ToolSetupSearchResult
    {
        public int ID { get; set; }
        public string SpecialComment { get; set; }
        public string GroupType { get; set; }
    }

    public class AddToolSetupRequest
    {
        public int SetUpSheetID { get; set; }
        public string[] ID_GroupType { get; set; }
        public string ModifiedBy { get; set; }
    }

    public class DeleteToolSetupRequest
    {
        public int SetupSheetID { get; set; }
        public int ToolSetupID { get; set; }
        public string ModifiedBy { get; set; }
    }

    public class ProgramSaveRequest
    {
        public string Program { get; set; }
        public string MachineID { get; set; }
        public int SetUpSheetID { get; set; }
        public string ModifiedBy { get; set; }
    }

    public class UploadProgramRequest
    {
        public int SetUpSheetID { get; set; }
        public bool UseExistingSheet { get; set; }
        public string UploadedProgramText { get; set; }        
        public string ModifiedBy { get; set; }
    }
    public class ConvertProgramRequest
    {
        public string Program { get; set; }
        public string FromMachineID { get; set; }
        public string ToMachineID { get; set; }
    }

    public class SaveCodeColumnsRequest
    {
        public string Code { get; set; }
        public string Columns { get; set; }        
    }

    public class CopyCodeColumnsRequest
    {
        public string Code { get; set; }
        public string CopyToCode { get; set; }
    }
    public class ToolSetupSearchRequest
    {
        public string SearchTerm { get; set; }
    }

    public class LookUpRequest
    {
        public string Category { get; set; }
        public string SearchTerm { get; set; }
    }

    public class CheckOutCheckInItem
    {
        public int ID { get; set; }
        public int Qty { get; set; }
    }
    public class CheckOutCheckInRequest
    {
        public string Action { get; set; }
        public string ModifiedBy { get; set; }
        public List<CheckOutCheckInItem> CheckOutCheckInItems { get; set; }
    }
    //SaveLookupCategoryRequest
    public class SaveLookupCategoryRequest
    {
        public string Category { get; set; }
        public string ModifiedBy { get; set; }
        public List<LookupCategoryValue> LookupCategoryValues { get; set; }
    }
    public class ToolCuttingMethod
    {
        public int ID { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
        public bool Connected { get; set; }
    }
        public class ToolInventoryColumn
    {
        public string Name { get; set; }
        public string Header { get; set; }
        public bool Searchable { get; set; }
        public int Sequence { get; set; }
        public string RelatedTable { get; set; }
        public string RelatedIDField { get; set; }
        public string RelatedTextField { get; set; }
        public string InputType { get; set; }
        public int UISize { get; set; }
        public string PropertyName { get; set; }
        public bool Display { get; set; }
        public string Required { get; set; }        

    }

    public class ToolInventoryCodeColumn
    {
        public string Name { get; set; }
        public string Header { get; set; }
        public bool Show { get; set; }
        public int Sequence { get; set; }
    }

    public class ToolInventorySearch
    {
        public string[] Code { get; set; }
        public string Name { get; set; }
        public string ItemNumber { get; set; }
        public string CategoryID { get; set; }
        public string MWID { get; set; }
        public string Radius { get; set; }
        public string NumberOfCutters { get; set; }
        public string ChipBreaker { get; set; }
        public string Material { get; set; }
        public string Grade { get; set; }
        public string Location { get; set; }
        public string ExternalLocation { get; set; }
        public string Manufacturer { get; set; }
        public string Comment { get; set; }
        public string StatusID { get; set; }
        public string ToolGroupNumber { get; set; }
        public string Description { get; set; }
        public string CuttingMethods { get; set; }
        public int pageNumber { get; set; } = 1;
        public int pageSize { get; set; } = 25;
        public string sortColumn { get; set; }
        public string sortDirection { get; set; }
        public string[] SelectedToolIDs { get; set; }
    }

    public class LookupCategorySearch
    {
        public string Category { get; set; }
        public int pageNumber { get; set; } = 1;
        public int pageSize { get; set; } = 20;        
    }

    public class ToolInventorySearchResult
    {
        public int ID { get; set; }
        public string Unit { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string ItemNumber { get; set; }
        public string Manufacturer { get; set; }
        public string VendorID { get; set; }
        public string MWID { get; set; }
        public string Location { get; set; }
        public string Radius { get; set; }
        public string CuttingMethods { get; set; }
        public string NumOfCutters { get; set; }
        public string Material { get; set; }
        public string Grade { get; set; }

        public string OnHand { get; set; }
        public string ChipBreaker { get; set; }
        public string CheckedOut { get; set; }
        public string Comment { get; set; }
        public string Description { get; set; }
        public string ExternalLocation { get; set; }
        public string CategoryName { get; set; }
        public string CategoryID { get; set; }
        public string Status { get; set; }
        public string StatusID { get; set; }
        public string isLocked { get; set; }
        public string OrderPoint { get; set; }
        public string InventoryLevel { get; set; }
        public string ToolGroupNumber { get; set; }
        public string UnitPrice { get; set; }
        public string PackSize { get; set; }        
        public string ImagePath { get; set; }
        public string Angle { get; set; }
        public string Diameter { get; set; }
        public string Direction { get; set; }
        public string FluteLength { get; set; }
        public string ImageCode { get; set; }
        public string isSent { get; set; }
        public string LBS { get; set; }
        public string MachineNumber { get; set; }
        public string MaxDepthOfCut { get; set; }
        public string NewAppDate { get; set; }
        public string NumOfFlutes { get; set; }
        public string OAL { get; set; }
        public string POID { get; set; }
        public string ShankDiameter { get; set; }
        public string NeckDiameter { get; set; }
        public string StationNumber { get; set; }
        public string OrderApproved { get; set; }
        public string Width { get; set; }
        public List<LinkedTool> LinkedTools { get; set; }
        public VendorInfo VendorInfo { get; set; }
    }

    public class ToolInventorySaveRequest
    {
        public int ID { get; set; }
        public string Unit { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string ItemNumber { get; set; }
        public string Manufacturer { get; set; }
        public string VendorID { get; set; }
        public string MWID { get; set; }
        public string Location { get; set; }
        public string Radius { get; set; }
        public string NumOfCutters { get; set; }
        public string Material { get; set; }
        public string Grade { get; set; }

        public string OnHand { get; set; }
        public string ChipBreaker { get; set; }
        public string CheckedOut { get; set; }
        public string Comment { get; set; }
        public string Description { get; set; }
        public string ExternalLocation { get; set; }
        public string CategoryID { get; set; }
        public string StatusID { get; set; }
        public string isLocked { get; set; }
        public string OrderPoint { get; set; }
        public string InventoryLevel { get; set; }
        public string ToolGroupNumber { get; set; }
        public string UnitPrice { get; set; }
        public string PackSize { get; set; }
        public string ImagePath { get; set; }
        public string Angle { get; set; }
        public string Diameter { get; set; }
        public string Direction { get; set; }
        public string FluteLength { get; set; }
        public string ImageCode { get; set; }
        public string isSent { get; set; }
        public string LBS { get; set; }
        public string MachineNumber { get; set; }
        public string MaxDepthOfCut { get; set; }
        public string NewAppDate { get; set; }
        public string NumOfFlutes { get; set; }
        public string OAL { get; set; }
        public string POID { get; set; }
        public string ShankDiameter { get; set; }
        public string NeckDiameter { get; set; }
        public string StationNumber { get; set; }
        public string OrderApproved { get; set; }
        public string Width { get; set; }
        public string[] CuttingMethodID { get; set; }    
    }

    public class LinkedTool
    {        
        public int ID { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
    }

    public class LinkToolRequest
    {
        public int ParentID { get; set; }
        public List<int> ChildIDs { get; set; }
        public string Action { get; set; }        
    }
    public class ToolInventorySearchResults
    {
        public List<ToolInventorySearchResult> SearchResults { get; set; }
        public int recordCount { get; set; }        
    }

    public class LookupCategories
    {
        public List<LookupCategoryValue> lookupCategoryValues { get; set; }
        public int recordCount { get; set; }
    }
    public class LookupCategoryValue
    {
        public int ID { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
        public bool active { get; set; }
    }

    public class Company
    {
        public int ID { get; set; }
        public string CompanyName { get; set; }
        public string CompanyID { get; set; }
        public bool active { get; set; }
    }

    public class VendorInfo
    {
        public int ID { get; set; }       
        public string CompanyName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string Dept { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Mobile { get; set; }
        public string TollFree { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
    }

    public class Order
    {
        public int id { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public DateTime pickUpTime { get; set; }
        public DateTime pickUpDate { get; set; }
        public OrderItem[] orderItems { get; set; }
    }

    public class OrderItem
    {
        public int id { get; set; }
        public string name { get; set; }
        public int qty { get; set; }
        public decimal price { get; set; }
        public string instructions { get; set; }
    }
}