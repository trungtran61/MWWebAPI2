using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MWWebAPI2.Models
{
    public class Lookup
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
        public string Category { get; set; }
        public bool active { get; set; }
        public string Sequence { get; set; }
    }

    public class APIResponse
    {
        public int ResponseCode { get; set; }
        public string ResponseText { get; set; }
    }

    /*
    public class CuttingMethodRequest
    {
        public string CuttingMethod { get; set; }       
    }
    */
    public class CuttingMethodTemplate
    {
        public int Id { get; set; }
        public string CuttingMethod { get; set; }
        public string Template { get; set; }        
    }

    public class DBResponse
    {
        public int ReturnCode { get; set; }
        public int RecordsAffected { get; set; }
        public string Message { get; set; }
    }
}
