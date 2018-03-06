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
    public class IPREC
    {
        public static string PhaseDescription(string strParameter, string strEquipment)
        {
            //Querys IP21 based on start and end times or equipment found in CCDB
            string strResult = "Sorry! I'm having difficulties connecting to IP21 right now, please try again later";
            string strTagResult = null;
            string strEquipmentNumeric = null;
            string strModifier = "CURRENT";
            string strPrettyPrint = null;

            //Extract the numerical value only from the equipment name
            if (!string.IsNullOrEmpty(strEquipment))
            {
                strEquipmentNumeric = new String(strEquipment.Where(Char.IsDigit).ToArray());
            }

            //Build the tag query and query IP21 for the tag, store result in strTagResult
            string strQueryTagName = "SELECT name from IP_textDef where name like '" + strEquipmentNumeric + "%' " + " and ip_description like 'Phase Description'";
            strTagResult = IP21.GenericQuery("IP-REC", strQueryTagName);

            //Build the tag query and query IP21 for the tag, store result in strTagResult
            string strSortOrder = "order by ip_trend_time desc"; //Treat lack of qualifers as "current"
            string strParameterField = "case IP_TREND_VALUE when '' then 'null' else IP_TREND_VALUE end";

            string strQueryIP21 = "SELECT " + strParameterField + " FROM \"" + strTagResult + "\" " + strSortOrder + ";";
            strResult = IP21.GenericQuery("IP-REC", strQueryIP21);

            if (strResult == "null")
            {
                strPrettyPrint = "There's no active " + strParameter + "s on " + strEquipment;
            }
            else
            {
                strPrettyPrint = "The " + strModifier.ToLower() + " " + strParameter + " for " + strEquipment + " is " + strResult;
            }

            //Pretty print the result
            return strPrettyPrint.Trim();
        }
    }
}