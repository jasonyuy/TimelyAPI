using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PADMEServiceLibrary;

namespace TimelyAPI.Models
{
    public class LIMS
    {
        public static string LIMSQuery(string strParameter, string strItemType, string strTestCode, string strLot)
        {
            string strResult = null;
            string strPrettyPrint = null;
            string strParameterField = "ASSAY_RESULTS_RAW";
            string strSQLItemType = null;
            string strSQLTestCode = null;
            string strSQLLot = null;
            string strUnits = null;
            string strPPItemType = null;
            string strPPTestCode = null;
            string strPPPLot = null;
            string strConstraintsPP = null;

            //LIMS Query Example
            //select * from MSAT.T_LIMS where ITEM_TYPE='PHCCF' and TEST_CODE='Q12318' and MES_LOT_NUMBER='3138969' and SITE_FLAG='SSF' order by ESB_WRITE_TIME desc;

            //Define Base query
            string strSQLbase = "select <FIELD> from MSAT.T_LIMS where SITE_FLAG='SSF'";

            //Build the conditional clauses from information provided
            if (!string.IsNullOrEmpty(strItemType)) { strSQLItemType = " and ITEM_TYPE like '%" + strItemType.Trim().ToUpper() + "%'"; };
            if (!string.IsNullOrEmpty(strTestCode)) { strSQLTestCode = " and TEST_CODE like '%" + strTestCode.Trim().ToUpper() + "%'"; };
            if (!string.IsNullOrEmpty(strLot)) { strSQLLot = " and MES_LOT_NUMBER='" + strLot.Trim() + "'"; };

            //Pretty print constrints
            if (!string.IsNullOrEmpty(strItemType)) { strPPItemType = " item type " + strItemType.Trim().ToUpper() ; };
            if (!string.IsNullOrEmpty(strTestCode)) { strPPTestCode = " test code " + strTestCode.Trim().ToUpper() ; };
            if (!string.IsNullOrEmpty(strLot)) { strPPPLot = " lot " + strLot.Trim() ; };
            strConstraintsPP = strPPItemType + strPPTestCode + strPPPLot;

            string strSQLFinal = strSQLbase.Replace("<FIELD>", strParameterField) + strSQLItemType + strSQLTestCode + strSQLLot + " order by ESB_WRITE_TIME desc";
            strResult = OracleSQL.SimpleQuery("LIMS", strSQLFinal);

            string strSQLUnit = strSQLbase.Replace("<FIELD>", "UNITS") + strSQLItemType + strSQLTestCode + strSQLLot + " order by ESB_WRITE_TIME desc";
            strUnits = OracleSQL.SimpleQuery("LIMS", strSQLUnit);

            strPrettyPrint = "The " + strParameter + " value for " + strConstraintsPP.Trim() + " is " + strResult + " " + strUnits;

            return strPrettyPrint.Trim();
        }
        public static string TiterQuery(string strProduct, string strVesselClass, string strEquipment, string strRun, string strLot, string strStep)
        {
            //Need to also connect to CCDB for this one to interpret
            string strResult = "Sorry! I can't find a titer value for this batch, it's most likely not in LIMS yet";
            string strParameterField = "ASSAY_RESULTS_RAW";
            string strSQLItemType = null;
            string strSQLTestCode = " and TEST_CODE in ('Q12318','Q12274')";
            string strSQLLot = null;
            string strPrettyPrint = null;
            string strUnits = null;

            //LIMS Query Example
            //select * from MSAT.T_LIMS where ITEM_TYPE='PHCCF' and TEST_CODE='Q12318' and MES_LOT_NUMBER='3138969' and SITE_FLAG='SSF' order by ESB_WRITE_TIME desc;

            //Define Base query
            string strSQLbase = "select <FIELD> from MSAT.T_LIMS where SITE_FLAG='SSF'";

            //Combine all constraints
            string strConstraints = CCDB.ConcatConstraints(strProduct, "12KL", strEquipment, strRun, strLot, null);
            string strConstraintsPP = CCDB.PrettyPrintConstraints(strProduct, "12KL", strEquipment, strRun, strLot, null);

            //Find the Harvest Lot - use IMS reports
            string strSQLHarvLot = "select BATCH from ISI.IMSREPORTS where AREA='CENTRIFUGE' and START_TIME is not null and RECIPE is not null " +
                "and START_TIME + 6 / 24 > (select HARVESTTIME - 6 / 24 from ISI.CCBATCHES where ISI.CCBATCHES.BATCHID is not null "
                + strConstraints + " and HARVESTTIME is not null)  order by START_TIME";
            strLot = OracleSQL.SimpleQuery("CCDB", strSQLHarvLot);

            //Build the conditional clauses from information provided
            try
            {
                strStep = Define.stepItemAlias[strStep];
            }
            catch (KeyNotFoundException)
            {
                strStep = null;
            }
            if (!string.IsNullOrEmpty(strStep)) { strSQLItemType = " and ITEM_TYPE='" + strStep + "'"; };
            if (!string.IsNullOrEmpty(strLot)) { strSQLLot = " and MES_LOT_NUMBER='" + strLot.Trim() + "'"; };

            //Find the titer result
            string strSQLFinal = strSQLbase.Replace("<FIELD>", strParameterField) + strSQLItemType + strSQLTestCode + strSQLLot + " and UPPER(COMPONENT) like '%CONC%' order by ESB_WRITE_TIME desc";
            strResult = OracleSQL.SimpleQuery("LIMS", strSQLFinal);

            //Find the titer units
            string strSQLUnit = strSQLbase.Replace("<FIELD>", "UNITS") + strSQLItemType + strSQLTestCode + strSQLLot + " and UPPER(COMPONENT) like '%CONC%' order by ESB_WRITE_TIME desc";
            strUnits = OracleSQL.SimpleQuery("LIMS", strSQLUnit);

            strPrettyPrint = "The " + strStep + " titer for " + strConstraintsPP.Trim() + " is " + strResult + " " + strUnits;

            return strPrettyPrint.Trim();
        }
    }
}