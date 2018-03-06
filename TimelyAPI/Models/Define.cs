using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimelyAPI.Models
{
    public class Define
    {
        //Common product abbreviations -> Product ID for CCDB
        public static Dictionary<string, string> productID =
            new Dictionary<string, string>
            {
                {"AVASTIN","rhuMAb VEGF G7 v1.2"},
                {"TNKASE","TNK-tPA"},
                {"PULMOZYME","rhDNase"},
                {"PULMOZYME V1.1","rhDNase v1.1"},
            };

        //Common step abbreviations -> Item Types for LIMS
        public static Dictionary<string, string> stepItemAlias =
            new Dictionary<string, string>
            {
                {"PREHARV","PHCCF"},
                {"HARVEST","CLARCC"},
            };
    }
}