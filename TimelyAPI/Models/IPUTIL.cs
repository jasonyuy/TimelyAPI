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
    public class IPUTIL
    {
        public static string UtilityQuery(string strParameter, string strUtility, string strRoom, string strModifier, double dblDuration)
        {
            //Querys IP21 based on start and end times or equipment found in CCDB
            string strResult = "Sorry! I'm having difficulties connecting to IP21 right now, please try again later";
            string strTagResult = null;
            string strPrettyPrint = null;
            string strStartTimeResult = null;
            string strDurationPrint = null;

            //Re-Define search windows if a duration is detected
            if (dblDuration > 0)
            {
                string strSQLStartTime = "select TO_CHAR(SYSDATE - " + dblDuration + " / 86400,'YYYY-MM-DD HH24:MI:SS') START_TIME from dual";
                strStartTimeResult = OracleSQL.SimpleQuery("DATATOOLS", strSQLStartTime);
                strDurationPrint = " in the last " + Math.Round(dblDuration / 3600) + " hours";
            }

            //Build the tag query and query IP21 for the tag, store result in strTagResult
            string strQueryTagName = "SELECT name from ip_analogdef where ip_description like '" + strRoom + "%' and ip_description like '%" + strUtility + "%' and IP_ENG_UNITS like 'DEGC'";
            strTagResult = IP21.GenericQuery("IP-UTIL", strQueryTagName);

            //Build the tag query and query IP21 for the tag, store result in strTagResult
            string strSortOrder = "order by ts desc"; //Treat lack of qualifers as "current"
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
                    case "MINIMUM":
                        strParameterField = "MIN(" + strParameterField + ")";
                        strSortOrder = null;
                        break;
                    case "MAX":
                    case "MAXIMUM":
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

            string strQueryIP21 = "SELECT " + strParameterField + " FROM history where name='" + strTagResult + "' " + strSortOrder + ";";
            strResult = IP21.GenericQuery("IP-UTIL", strQueryIP21);

            //Pretty print the result
            strPrettyPrint = "The current " + strParameter + " value for " + strRoom + " " + strUtility + " is " + strResult + " Deg C";
            return strPrettyPrint.Trim();
        }
    }
}