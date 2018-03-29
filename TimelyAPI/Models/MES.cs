using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PADMEServiceLibrary;
/// <summary>
/// The purpose of this library is to make calls to the MES database
/// </summary>
/// 
namespace TimelyAPI.Models
{
    public class MES
    {
        public static string MediaQuery(string strTrigger, string strParameter, string strProduct, string strVesselClass, string strEquipment, string strRun, string strLot, string strStation)
        {
            // Query get Media information used for a selected run/lot/batch
            // Should answer the follwing: What's the media lot for Avastin Run 162 12kL?

            //Initialize variables
            string strResult = "Sorry! I'm having difficulties connecting to MES right now, please try again later";
            string strMediaBatchID = null;

            //Specific Batch Query Example
            //select BATCH_ID from SSFMES.CO_PROC_RESULT_ST where RESULTS like '" + strLot + "' and UPPER(UNIT_PROCEDURE_ID) like '%BATCH%FEED%'

            switch (strParameter.ToUpper())
            {
                case "PRODUCT":
                    strProduct = null;
                    break;
                case "SCALE":
                    strVesselClass = null;
                    break;
                case "EQUIPMENT":
                    strEquipment = null;
                    break;
                case "RUN":
                    strRun = null;
                    break;
                case "STATION":
                    strStation = null;
                    break;
                default:
                    break;
            }

            //Define Base query
            if (string.IsNullOrEmpty(strLot))
            {
                string strSQLbase = "select <FIELD> from ISI.CCBATCHES where ISI.CCBATCHES.BATCHID is not null";

                //Combine all constraints
                string strConstraints = CCDB.ConcatConstraints(strProduct, strVesselClass, strEquipment, strRun, strLot, strStation);

                //Get the culture lot from CCDB
                string strCultureLot = strSQLbase.Replace("<FIELD>", "LOT") + strConstraints + " order by INOCTIME desc";
                strLot = OracleSQL.SimpleQuery("CCDB", strCultureLot);
            }

            //Get the media batch ID
            if (strTrigger == "BATCH FEED")
            {
                strMediaBatchID = OracleSQL.SimpleQuery("MES", "select BATCH_ID from SSFMES.CO_PROC_RESULT_ST where RESULTS like '" + strLot.Trim() + "' and UPPER(UNIT_PROCEDURE_ID) like '%BATCH%FEED%'");
            }
            else
            {
                strMediaBatchID = OracleSQL.SimpleQuery("MES", "select BATCH_ID from SSFMES.CO_PROC_RESULT_ST where RESULTS like '" + strLot.Trim() + "' and UPPER(UNIT_PROCEDURE_ID) not like '%BATCH%FEED%'");
            }

            //If batch ID can't be found try it again in Genealogy table
            if (string.IsNullOrEmpty(strMediaBatchID))
            {
                strMediaBatchID = OracleSQL.SimpleQuery("MES", "select distinct BATCH_ID from SSFMES.MM_GENEALOGY_LG where DEST_LOT_ID='" + strLot.Trim() + "'");
            }

            //Get the result, if user is requesting a lot number, find it in Genealogy table
            if (strParameter == "LOT")
            {
                strResult = OracleSQL.SimpleQuery("MES", "select distinct DEST_LOT_ID from SSFMES.MM_GENEALOGY_LG where BATCH_ID='" + strMediaBatchID + "'");
            }
            else
            {
                strResult = OracleSQL.SimpleQuery("MES", "select case when RESULTN is not null then to_char(RESULTN) else RESULTS end VAL from SSFMES.CO_PROC_RESULT_ST where BATCH_ID like '" + strMediaBatchID
                    + "' and UPPER(STEP_INSTANCE_ID) like '%" + strParameter + "%' order by STEP_INSTANCE_ID, ENTRY_TIMESTAMP desc");
            }

            return strResult.Trim();
        }
        public static string BufferQuery(string strTrigger, string strParameter, string strProduct, string strVesselClass, string strEquipment, string strRun, string strLot, string strStation)
        {
            // Query get Media information used for a selected run/lot/batch
            // Should answer the follwing: What's the media lot for Avastin Run 162 12kL?

            //Initialize variables
            string strResult = null;
            string strBufferBatchID = null;

            //Specific Batch Query Example
            //select BATCH_ID from SSFMES.CO_PROC_RESULT_ST where RESULTS like '" + strLot + "' and UPPER(UNIT_PROCEDURE_ID) like '%BATCH%FEED%'

            switch (strParameter.ToUpper())
            {
                case "PRODUCT":
                    strProduct = null;
                    break;
                case "SCALE":
                    strVesselClass = null;
                    break;
                case "EQUIPMENT":
                    strEquipment = null;
                    break;
                case "RUN":
                    strRun = null;
                    break;
                case "LOT":
                    strLot = null;
                    break;
                case "STATION":
                    strStation = null;
                    break;
                default:
                    break;
            }

            //Define Base query
            if (string.IsNullOrEmpty(strLot))
            {
                string strSQLbase = "select <FIELD> from ISI.CCBATCHES where ISI.CCBATCHES.BATCHID is not null";

                //Combine all constraints
                string strConstraints = CCDB.ConcatConstraints(strProduct, strVesselClass, strEquipment, strRun, strLot, strStation);

                //Get the culture lot from CCDB
                string strCultureLot = strSQLbase.Replace("<FIELD>", "LOT") + strConstraints + " order by INOCTIME desc";
                strLot = OracleSQL.SimpleQuery("CCDB", strCultureLot);
            }

            strBufferBatchID = OracleSQL.SimpleQuery("MES", "select distinct BATCH_ID from SSFMES.MM_GENEALOGY_LG where DEST_LOT_ID='" + strLot.Trim() + "'");

            //Get the result, if user is requesting a lot number, find it in Genealogy table
            if (strParameter == "LOT")
            {
                strResult = OracleSQL.SimpleQuery("MES", "select distinct DEST_LOT_ID from SSFMES.MM_GENEALOGY_LG where BATCH_ID='" + strBufferBatchID + "'");
            }
            else
            {
                strResult = OracleSQL.SimpleQuery("MES", "select case when RESULTN is not null then to_char(RESULTN) else RESULTS end VAL from SSFMES.CO_PROC_RESULT_ST where BATCH_ID like '" + strBufferBatchID
                    + "' and UPPER(STEP_INSTANCE_ID) like '%" + strParameter + "%' order by STEP_INSTANCE_ID, ENTRY_TIMESTAMP desc");
            }

            return strResult;
        }

        private static Dictionary<string, string> productID =
            new Dictionary<string, string>
            {
                {"AVASTIN","rhuMAb VEGF G7 v1.2"},
                {"TNKASE","TNK-tPA"},
                {"PULMOZYME","rhDNase"},
            };
    }
}