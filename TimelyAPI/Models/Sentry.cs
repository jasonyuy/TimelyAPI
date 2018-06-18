using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using PADMEServiceLibrary;

namespace TimelyAPI.Models
{
    public class Sentry
    {
        public static string CreateJob(string strCCDBParameter, string strIPFermParameter, string strIPRecParameter, string strUserUnix, string strEquipment, string strStation, string strLot, double dblDuration, string strlimit, string strlimitvalue, string strModifier)
        {
            //Initialize variables
            string strResult = null;
            string strIP21Result = null;
            string strTagResult = null;
            string strJobParameter = null;
            string strJobOperation = null;
            string strJobContext = null;
            string strJobCriteria = null;

            //Figure out where the parameter is located CCDB or IP-FERM
            if (!string.IsNullOrEmpty(strCCDBParameter)) { strJobParameter = strCCDBParameter; };
            if (!string.IsNullOrEmpty(strIPFermParameter)) { strJobParameter = strIPFermParameter; };
            if (!string.IsNullOrEmpty(strIPRecParameter)) { strJobParameter = strIPRecParameter; };
            if (!string.IsNullOrEmpty(strlimitvalue)) { strJobCriteria = strlimitvalue; };
            if (!string.IsNullOrEmpty(strEquipment)) { strJobContext = strEquipment; };
            if (!string.IsNullOrEmpty(strStation)) { strJobContext = strStation; };
            if (!string.IsNullOrEmpty(strLot)) { strJobContext = strLot; };

            //Figure out what the current value is so that the job operation can be assigned, if no limit value is found, the user is asking for a state change
            if (!string.IsNullOrEmpty(strlimitvalue))
            {
                if (strlimit == "REACH" || strlimit == "HIT" || strlimit == "WILL BE")
                {
                    if (!string.IsNullOrEmpty(strIPFermParameter))
                    {
                        string strEquipmentNumeric = new String(strEquipment.Where(Char.IsDigit).ToArray());

                        string strQueryTagName = "SELECT name from ip_analogdef where name like '" + strEquipmentNumeric + "%' " + IP21.TagAliasDescription[strIPFermParameter.ToUpper()];
                        strTagResult = IP21.GenericQuery("IP-FERM", strQueryTagName);

                        string strQueryIP21 = "SELECT value FROM history where name='" + strTagResult + "' and request=4 order by ts desc;";
                        strIP21Result = IP21.GenericQuery("IP-FERM", strQueryIP21);

                        if (Convert.ToDouble(strIP21Result) < Convert.ToDouble(strlimitvalue)) { strJobOperation = "GREATER"; };
                        if (Convert.ToDouble(strIP21Result) > Convert.ToDouble(strlimitvalue)) { strJobOperation = "LESS"; }
                    }
                }

                if (strlimit == "GREATER" || strlimit == "ABOVE") { strJobOperation = "GREATER"; };
                if (strlimit == "LESS" || strlimit == "BELOW") { strJobOperation = "LESS"; };

                //Create Job for Sentry
                if (strUserUnix != "test")
                {
                    OracleSQL.OracleWrite("DATATOOLS", "insert into MSAT_SERVICE_JOBS (JOB_ID,APPLICATION,REQUEST_TIME,EXPIRATION_TIME, " +
                        "USER_ID,JOB_PARA_ID,JOB_CONTEXT,JOB_OPERATION,JOB_CRITERIA) values " +
                        "(SQ_MSAT_SERVICE_JOB_ID.NextVal,'SentryService',CURRENT_TIMESTAMP,CURRENT_TIMESTAMP+20," +
                        "(select USER_ID from MSAT_TIMELY_USERS where UNIX='" + strUserUnix + "')," +
                        "(select PARA_ID from MSAT_PARAMETERS where UPPER(PARA_NAME)='" + strJobParameter + "'),'" +
                        strJobContext + "','" + strJobOperation + "','" + strJobCriteria + "')");
                }

                strResult = "A Sentry alert for when " + strJobParameter + " on " + strJobContext + " is " + strJobOperation + " than " + strJobCriteria +
                    " was created for you. This alert will auto expire in 20 days if not triggered";
            }
            else
            {
                if (strlimit == "ENABLE" || strlimit == "ACTIVATE" || strlimit == "TURN ON")
                {
                    if (string.IsNullOrEmpty(strlimitvalue)) { strJobCriteria = strModifier; };
                    //Create Job for Sentry
                    if (strUserUnix != "test")
                    {
                        OracleSQL.OracleWrite("DATATOOLS", "insert into MSAT_SERVICE_JOBS (JOB_ID,APPLICATION,REQUEST_TIME,EXPIRATION_TIME, " +
                            "USER_ID,JOB_PARA_ID,JOB_CONTEXT,JOB_OPERATION,JOB_CRITERIA) values " +
                            "(SQ_MSAT_SERVICE_JOB_ID.NextVal,'SentryService',CURRENT_TIMESTAMP,CURRENT_TIMESTAMP+20," +
                            "(select USER_ID from MSAT_TIMELY_USERS where UNIX='" + strUserUnix + "')," +
                            "(select PARA_ID from MSAT_PARAMETERS where UPPER(PARA_NAME)='" + strJobParameter + "'),'" +
                            strJobContext + "','STATE_CHANGE','" + strJobCriteria + "')");
                    }

                    strResult = "Sentry will notify you when " + strJobParameter + " on " + strJobContext + " is changed/updated. " +
                        "This monitoring will be active for the next 20 days. To disable, reply with disable, deactivate, or turn off";
                }
            }
            

            return strResult;
        }

        public static string Snooze(string strCCDBParameter, string strIPFermParameter, string strUserUnix, string strEquipment, string strLot, double dblDuration)
        {
            //Initialize variables
            string strResult = null;
            string strJobParameter = null;
            string strJobContext = null;

            //Figure out where the parameter is located CCDB or IP-FERM
            if (!string.IsNullOrEmpty(strCCDBParameter)) { strJobParameter = strCCDBParameter; };
            if (!string.IsNullOrEmpty(strIPFermParameter)) { strJobParameter = strIPFermParameter; };
            if (!string.IsNullOrEmpty(strEquipment)) { strJobContext = strEquipment; };
            if (!string.IsNullOrEmpty(strLot)) { strJobContext = strLot; };
            if (dblDuration == 0) { dblDuration = 2 * 60 * 60; }; //Default 2 hours for snooze alarm

            //If nothing is provided, then look up the previous alert from Service log and infer the details
            if (string.IsNullOrEmpty(strCCDBParameter) && string.IsNullOrEmpty(strIPFermParameter))
            {
                string strParaName = null;
                string strRefValue = OracleSQL.SimpleQuery("DATATOOLS", "select REFERENCE_VALUE from MSAT_SERVICE_LOG_VW where USER_UNIX='" + strUserUnix +
                    "' and APPLICATION='SentryService' order by MESSAGE_TIME desc");
                string strRefSQL = OracleSQL.SimpleQuery("DATATOOLS", "select REFERENCE_SQL from MSAT_SERVICE_LOG_VW where USER_UNIX='" + strUserUnix +
                    "' and APPLICATION='SentryService' order by MESSAGE_TIME desc");
                if (!string.IsNullOrEmpty(strRefSQL))
                {
                    strParaName = OracleSQL.SimpleQuery("DATATOOLS", "select CHECK_PARA_NAME from MSAT_SENTRY_DEFINE_VW where DEFINE_ID is not null " + strRefSQL );
                };

                if (string.IsNullOrEmpty(strJobContext) & !string.IsNullOrEmpty(strRefValue)) { strJobContext = strRefValue.ToUpper(); };
                if (string.IsNullOrEmpty(strJobParameter) & !string.IsNullOrEmpty(strParaName)) { strJobParameter = strParaName.ToUpper(); };
            }

            //Create Snooze job for Sentry
            if (strUserUnix != "test")
            {
                OracleSQL.OracleWrite("DATATOOLS", "insert into MSAT_SERVICE_JOBS (JOB_ID,APPLICATION,REQUEST_TIME,EXPIRATION_TIME, " +
                    "USER_ID,JOB_PARA_ID,JOB_CONTEXT,JOB_OPERATION) values " +
                    "(SQ_MSAT_SERVICE_JOB_ID.NextVal,'SentryService',CURRENT_TIMESTAMP,CURRENT_TIMESTAMP+(" + dblDuration / 86400 + ")," +
                    "(select USER_ID from MSAT_TIMELY_USERS where UNIX='" + strUserUnix + "')," +
                    "(select PARA_ID from MSAT_PARAMETERS where UPPER(PARA_NAME)='" + strJobParameter + "'),'" + strJobContext + "','SNOOZE')");
            }

            strResult = "Alerts for " + strJobParameter + " on " + strJobContext + " will be snoozed for the next " + Math.Round(dblDuration / 3600,2) + " hours";

            return strResult;
        }
        /// <summary>
        /// Queries MSAT_SENTRY_DEFINE for action/alert limits.
        /// </summary>
        /// <param name="strParameter">ex: online dO2</param>
        /// <param name="strProduct">ex: anti-Myostatin</param>
        /// <param name="strVesselClass">ex: 2kL</param>
        /// <param name="strLimitType">ex: upper action limit</param>
        /// <returns></returns>
        public static string LimitQuery(string strParameter, string strProduct, string strVesselClass, string strLimitType)
        {
            // Initialize variables
            strLimitType += " LIMIT";
            string strResult = $"Sorry! I can't seem to find the {strLimitType} you requested, can you refine your request and try again?";
            string strUOM = null;

            // Pretty print the user input, before any additional lookups
            string strConstraintsPP = PrettyPrintConstraints(strParameter, strProduct, strVesselClass, strLimitType);

            // Define SQL statement
            strLimitType = strLimitType.Replace(" ", "_");
            string strSQLLimitType = $"select {strLimitType}, CHECK_PARA_UOM from MSAT_SENTRY_DEFINE_VW where UPPER(DEFINE_NAME) = '{strParameter}' and UPPER(CCDB_NAME) = '{strProduct}' and UPPER(AREA_ALIAS) like '%{strVesselClass}%'";
            //string strSQLlimit = $"select {strLimit} from MSAT_SENTRY_DEFINE_VW where CHECK_PARA_NAME = :pParam and CCDB_NAME = :pProduct and AREA_ALIAS like '%pVessel%'";

            // Query the database
            DataTable dtResult = OracleSQL.DataTableQuery("DATATOOLS", strSQLLimitType);
            DataRow[] drResults = dtResult.Select();
            if (drResults.Length > 0)
            {
                strResult = drResults[0][strLimitType].ToString();
                strUOM = drResults[0]["CHECK_PARA_UOM"].ToString();
            }
            
            // Pretty print
            string strPrettyPrint = strConstraintsPP + " is ";
            switch (strParameter)
            {
                case "ONLINE PH": // Do people here like "pH 7.0" or "7.0 pH"?
                    strPrettyPrint += $"{strUOM} {strResult}";
                    break;
                default:
                    strPrettyPrint += $"{strResult} {strUOM}";
                    break;
            }

            return strPrettyPrint;
        }

        private static string PrettyPrintConstraints(string strParameter, string strProduct, string strVesselClass, string strLimitType)
        {
            string strTarget = "";
            string strInfo = "";

            if (!string.IsNullOrEmpty(strLimitType) && !string.IsNullOrEmpty(strParameter))
            {
                strTarget += $" {strLimitType.ToLower()} for {strParameter}";
            }

            if (!string.IsNullOrEmpty(strProduct)) { strInfo += " " + strProduct; }
            if (!string.IsNullOrEmpty(strVesselClass)) { strInfo += " " + strVesselClass; }

            return "The" + strTarget + " for" + strInfo;
        }
    }
}