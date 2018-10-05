using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using PADMEServiceLibrary;

namespace TimelyAPI.Models
{
    public class GODW
    {
        public static string GODWQuery(string strUserUnix, string strFlag, string strParameter, string strThemeID)
        {
            string strResult = null;
            string strParameterField = null;
            string strSQLThemeID = null;
            string strPrettyPrint = null;

            //GOWD Query Example
            //select distinct PROJECT_THEME_DESC from S_F_FTE_ACTUAL where PROJECT_THEME_NAME='70094'

            //Understand what the user is searching for, set at the parameter field
            switch (strParameter.ToUpper())
            {
                case "ASSIGNED":
                    strParameterField = "distinct PROJECT_THEME_DESC";
                    break;
                case "RECORD":
                    strParameterField = "distinct PROJECT_THEME_DESC";
                    break;
                default:
                    strParameterField = "distinct PROJECT_THEME_DESC";
                    break;
            }

            //Define Base query
            string strSQLbase = "select <FIELD> from S_F_FTE_ACTUAL where PROJECT_THEME_NAME is not null";

            //Build the conditional clauses from information provided
            if (!string.IsNullOrEmpty(strThemeID)) { strSQLThemeID = " and PROJECT_THEME_NAME='" + strThemeID.Trim().ToLower() + "'"; };

            string strSQLFinal = strSQLbase.Replace("<FIELD>", strParameterField) + strSQLThemeID ;
            strResult = OracleSQL.SimpleQuery("GODW", strSQLFinal);
            strPrettyPrint = "The project associated with theme " + strThemeID + " is " + strResult;

            return strPrettyPrint.Trim();
        }
    }
}