using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using PADMEServiceLibrary;

namespace TimelyAPI.Models
{
    public class IPFERM
    {
        public static string DataQuery(string strParameter, string strProduct, string strVesselClass, string strEquipment, string strRun, string strLot, string strStation, string strModifier, double dblDuration)
        {
            //Querys IP21 based on start and end times or equipment found in CCDB
            string strResult = "Sorry! I'm having difficulties connecting to IP21 right now, please try again later";
            string strTagResult = null;
            string strTagUOM = null;
            string strStartTimeResult = null;
            string strEndTimeResult = null;
            string strEquipmentNumeric = null;
            string strDurationPrint = null;
            string strScaleID = null;
            if (string.IsNullOrEmpty(strModifier)) { strModifier = "CURRENT"; };
            if (!string.IsNullOrEmpty(strVesselClass)) { strScaleID = ScaleID[strVesselClass.ToUpper()]; };
            string strSQLSort = " order by INOCTIME desc";

            //Define Base query
            string strSQLbase = "select <FIELD> from ISI.CCBATCHES where ISI.CCBATCHES.BATCHID is not null";
            string strEqipmentResult = strEquipment;

            //Combine all constraints
            string strConstraints = CCDB.ConcatConstraints(strProduct, strVesselClass, strEquipment, strRun, strLot, strStation);
            string strConstraintsPP = CCDB.PrettyPrintConstraints(strProduct, strVesselClass, strEquipment, strRun, strLot, strStation);

            //Find the most likely batch that matches search criteria, if equipment modifer is filled
            if (string.IsNullOrEmpty(strEquipment) && string.IsNullOrEmpty(strStation))
            {
                string strSQLEquipment = strSQLbase.Replace("<FIELD>", "ISI.CCBATCHES.FERMID") + strConstraints + strSQLSort;
                strEqipmentResult = OracleSQL.SimpleQuery("CCDB", strSQLEquipment);
            }

            //If there's a seed train station, assign it to the equipment variable
            if (string.IsNullOrEmpty(strEquipment) && !string.IsNullOrEmpty(strStation))
            {
                strEquipmentNumeric = strStation;
            }

            //Extract the numerical value only from the equipment name
            if (!string.IsNullOrEmpty(strEquipment))
            {
                strEquipmentNumeric = new String(strEqipmentResult.Where(Char.IsDigit).ToArray());
            }

            //Extract the numerical value only from the equipment name
            if (string.IsNullOrEmpty(strEquipment) && string.IsNullOrEmpty(strStation))
            {
                strEquipmentNumeric = new String(strEqipmentResult.Where(Char.IsDigit).ToArray());
            }

            //Re-Define search windows if a duration is detected
            if (dblDuration > 0)
            {
                string strSQLStartTime = "select TO_CHAR(SYSDATE - " + dblDuration + " / 86400,'YYYY-MM-DD HH24:MI:SS') START_TIME from dual";
                strStartTimeResult = OracleSQL.SimpleQuery("CCDB", strSQLStartTime);
                strDurationPrint = " in the last " + Math.Round(dblDuration / 3600) + " hours";
            }
            else
            {
                string strSQLStartTime = strSQLbase.Replace("<FIELD>", "TO_CHAR(CAST((FROM_TZ(CAST(INOCTIME AS TIMESTAMP),'+00:00') AT TIME ZONE 'US/Pacific') AS DATE),'YYYY-MM-DD HH24:MI:SS') START_TIME") + strConstraints + strSQLSort;
                strStartTimeResult = OracleSQL.SimpleQuery("CCDB", strSQLStartTime);

                string strSQLEndTime = strSQLbase.Replace("<FIELD>", "TO_CHAR(CAST((FROM_TZ(CAST(HARVESTTIME AS TIMESTAMP),'+00:00') AT TIME ZONE 'US/Pacific') AS DATE),'YYYY-MM-DD HH24:MI:SS') END_TIME ") + strConstraints + strSQLSort;
                strEndTimeResult = OracleSQL.SimpleQuery("CCDB", strSQLEndTime);
            }

            // In order for PADME Service Library to work
            if (strParameter.ToUpper() == "TEMPERATURE") { strParameter = "TEMP"; }
            if (strParameter.ToUpper() == "AIR") { strParameter = "AIR SPARGE"; }
            //TODO: stop hardcoding

            //Build the tag query and query IP21 for the tag, store result in strTagResult
            string strQueryTagName = "SELECT name from ip_analogdef where name like '%" + strEquipmentNumeric + "%' " + IP21.TagAliasDescription[strParameter.ToUpper()] + " order by name desc";
            strTagResult = IP21.GenericQuery("IP-FERM", strQueryTagName);

            string strQueryTagUOM = "SELECT ip_eng_units from ip_analogdef where name like '%" + strEquipmentNumeric + "%' " + IP21.TagAliasDescription[strParameter.ToUpper()] + " order by name desc";
            strTagUOM = IP21.GenericQuery("IP-FERM", strQueryTagUOM);

            //Build the tag query and query IP21 for the tag, store result in strTagResult
            string strEndTimeConstraint = null;
            string strSortOrder = " order by ts desc"; //Treat lack of qualifers as "current"
            string strParameterField = "ROUND(value,3)";
            
            //Add query modifiers if requested
            if (!string.IsNullOrEmpty(strModifier))
            {
                switch (strModifier.ToUpper())
                {
                    case "CURRENT":
                        strSortOrder = " order by ts desc";
                        break;
                    case "MIN":
                        strParameterField = "MIN(" + strParameterField + ")";
                        strSortOrder = null;
                        break;
                    case "MAX":
                        strParameterField = "MAX(" + strParameterField + ")";
                        strSortOrder = null;
                        break;
                    case "AVERAGE":
                        strParameterField = "AVG(" + strParameterField + ")";
                        strSortOrder = null;
                        break;
                    case "RANGE":
                        strParameterField = "MAX(" + strParameterField + ") - MIN(" + strParameterField + ")";
                        strSortOrder = null;
                        break;
                    default:
                        break;
                }
            }

            if (!string.IsNullOrEmpty(strEndTimeResult)) { strEndTimeConstraint = "' and ts<=TIMESTAMP'" + strEndTimeResult; }
            string strQueryIP21 = "SELECT " + strParameterField + " FROM history where name='" + strTagResult + "' and ts>=TIMESTAMP'" + strStartTimeResult + strEndTimeConstraint + "'" + strSortOrder + ";";
            strResult = IP21.GenericQuery("IP-FERM", strQueryIP21);

            //Pretty print the result
            string strPrettyPrint = "The " + strModifier.ToLower() + " " + strParameter + " value for " + strConstraintsPP + strDurationPrint + " is " + strResult + " " + strTagUOM;
            return strPrettyPrint.Trim();
        }

        public static Dictionary<string, string> ScaleID =
            new Dictionary<string, string>
            {
                {"20L","7"},
                {"80L","3"}, 
                {"400L","4"}, 
                {"2KL","5"}, 
                {"12KL","6"}, 
            };
    }
}