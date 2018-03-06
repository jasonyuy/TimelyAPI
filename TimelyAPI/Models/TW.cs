using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PADMEServiceLibrary;

namespace TimelyAPI.Models
{
    public class TW
    {
        public static string TWQuery(string strUserUnix, string strFlag, string strParameter, string strRecordID, string strTimeflag)
        {
            string strResult = null;
            string strParameterField = null;
            string strSQLAssigneeUnix = null;
            string strSQLRecordID = null;
            string strPrettyPrint = null;

            //LIMS Query Example
            //select * from TWR_ALL_OPEN where ASSIGNEE_UNIX='yucheng';

            //Understand what the user is searching for, set at the parameter field
            switch (strParameter.ToUpper())
            {
                case "ASSIGNED":
                    strParameterField = "REC_ASSIGNED_TO";
                    strParameter = "assignee";
                    break;
                case "RECORD":
                    strParameterField = "REC_ID";
                    break;
                case "PARENT":
                    strParameterField = "PARENT_REC_ID";
                    break;
                case "TYPE":
                    strParameterField = "REC_PROJECT";
                    break;
                case "SUBTYPE":
                    strParameterField = "REC_SUBTYPE";
                    break;
                case "STATUS":
                    strParameterField = "REC_STATUS";
                    break;
                case "STATE":
                    strParameterField = "REC_STATE";
                    break;
                case "DESCRIPTION":
                    strParameterField = "REC_SHORT_DESCRIPTION";
                    break;
                case "DUE":
                    strParameterField = "TO_CLOSE";
                    break;
                case "UPDATE":
                    strParameterField = "CAST((FROM_TZ(CAST(UPDATED_UTC AS TIMESTAMP),'+00:00') AT TIME ZONE 'US/Pacific') AS DATE)";
                    strParameter = "updated";
                    break;
                case "CREATE":
                    strParameterField = "CAST((FROM_TZ(CAST(CREATED_UTC AS TIMESTAMP),'+00:00') AT TIME ZONE 'US/Pacific') AS DATE)";
                    break;
                case "OPEN":
                    strParameterField = "CAST((FROM_TZ(CAST(CREATED_UTC AS TIMESTAMP),'+00:00') AT TIME ZONE 'US/Pacific') AS DATE)";
                    strParameter = "opened";
                    break;
                case "CLOSE":
                    strParameterField = "CAST((FROM_TZ(CAST(CLOSED_UTC AS TIMESTAMP),'+00:00') AT TIME ZONE 'US/Pacific') AS DATE)";
                    strParameter = "closed";
                    break;
                default:
                    strParameterField = "REC_ID";
                    break;
            }

            //Define Base query
            string strSQLbase = "select <FIELD> from TWR where REC_ID is not null";

            //Build the conditional clauses from information provided
            if (!string.IsNullOrEmpty(strRecordID)) { strSQLRecordID = " and REC_ID='" + strRecordID.Trim().ToLower() + "'"; };
            if (!string.IsNullOrEmpty(strUserUnix)) { strSQLAssigneeUnix = " and REC_ASSIGNED_TO_UNIX='" + strUserUnix.Trim().ToLower() + "'"; };

            if (strParameter == "ME" || strParameter == "MY")
            {
                string[] strList = null;
                string strSQLFinal = strSQLbase.Replace("<FIELD>", strParameterField) + strSQLAssigneeUnix + " and REC_STATUS='OPEN' order by REC_ID desc";
                strList = OracleSQL.ListQuery("FOUNDRY", strSQLFinal);
                if (strList.Count() == 0)
                {
                    strPrettyPrint = "Hooray, you currently have no active records assigned to you! Keep crushing it";
                }
                else
                {
                    strPrettyPrint = "The following open records are currently assigned to you: " + string.Join(", ", strList);
                }
            }
            else
            {
                string strSQLFinal = strSQLbase.Replace("<FIELD>", strParameterField) + strSQLRecordID + " order by REC_ID desc";
                strResult = OracleSQL.SimpleQuery("FOUNDRY", strSQLFinal);
                if (!string.IsNullOrEmpty(strTimeflag))
                {
                    strPrettyPrint = "The record " + strRecordID + " was " + strParameter + " on " + strResult + " ";
                }
                else
                {
                    strPrettyPrint = "The " + strParameter + " of the record " + strRecordID + " is " + strResult + " ";
                }
            }

            return strPrettyPrint.Trim();
        }
    }
}