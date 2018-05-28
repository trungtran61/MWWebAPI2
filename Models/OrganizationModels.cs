using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MWWebAPI2.Models
{
    public class OrganizationModels
    {
        public class Organization
        {
            public string type { get; set; }
            public int id { get; set; }
            public string name { get; set; }
            public string address { get; set; }
            public string city { get; set; }
            public string state { get; set; }
            public string zip { get; set; }
            public string phone { get; set; }
            public string fax { get; set; }
            public string tollfree { get; set; }
            public string email { get; set; }
            public string website { get; set; }
            public bool active { get; set; }
        }

        public class GetOrganizationsRequest
        {
            public string typeParm { get; set; } = "";
            public string nameParm { get; set; } = "";
            public string addressParm { get; set; } = "";
            public string sortColumn { get; set; }
            public string sortDirection { get; set; }
            public int activeOnly { get; set; }
            public int pageSize { get; set; } = 25;
            public int pageNumber { get; set; } = 1;
        }

        public class GetOrganizationsResponse
        {
            public int recordCount { get; set; }
            public List<Organization> organizations { get; set; }
        }

        public class UpdateOrganizationStatusRequest
        {
            public string type { get; set; }
            public int id { get; set; }
            public bool active { get; set; }
        }

        public enum OrganizationType
        {
            customer,
            manufacturer,
            vendor
        }
    }
}