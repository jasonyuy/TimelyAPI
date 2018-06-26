using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PADMEServiceLibrary;

namespace TimelyAPI.Models
{
    public class CCDB
    {
        //Main Calls
        public static string BatchQuery(string strParameter, string strProduct, string strVesselClass, string strEquipment, string strRun, string strLot, string strStation, string strListFlag)
        {
            //Initialize variables
            string strResult = "Sorry! I can't seem to find the batch data you requested, can you refine your request and try again?";
            string strScaleID = null;
            string strPrettyPrint = null;
            string strParameterField = parameterField[strParameter.ToUpper()];
            if (!string.IsNullOrEmpty(strVesselClass)) { strScaleID = ScaleID[strVesselClass.ToUpper()]; };

            //Specific Batch Query Example
            //select * from ISI.CCBATCHES where SCALEID='12000' and RUN=162 and PRODUCTID='rhuMAb VEGF G7 v1.2' order by INOCTIME

            //Current Active Batches Example
            //select * from ISI.CCBATCHES where HARVESTTIME is null and INOCTIME > (SYSDATE - 30) order by INOCTIME desc

            //Define Base query
            string strSQLbase = "select <FIELD> from ISI.CCBATCHES where ISI.CCBATCHES.BATCHID is not null";

            //For parameters that could also be search criterias (e.g. Run/Lot/Equipment), check if they're the target parameter, if so, null out the value
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
                case "TANK":
                    strEquipment = null;
                    break;
                case "VESSEL":
                    strEquipment = null;
                    break;
                case "FERM":
                    strEquipment = null;
                    break;
                case "BIOREACTOR":
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

            //Combine all constraints
            string strConstraints = ConcatConstraints(strProduct, strVesselClass, strEquipment, strRun, strLot, strStation);
            string strConstraintsPP = PrettyPrintConstraints(strProduct, strVesselClass, strEquipment, strRun, strLot, strStation);

            //Check if user wants to return a list
            if (!string.IsNullOrEmpty(strListFlag))
            {
                string[] strList = null;
                string strSQLFinal = strSQLbase.Replace("<FIELD>", "distinct " + strParameterField) + strConstraints + " and HARVESTTIME is null and INOCTIME > (SYSDATE - 30)";
                strList = OracleSQL.ListQuery("CCDB", strSQLFinal);
                strPrettyPrint = "The following " + strConstraintsPP + " " + strParameter.ToLower() + "s are currently in process: " + string.Join(", ", strList);
            }
            else
            {
                string strSQLFinal = strSQLbase.Replace("<FIELD>", strParameterField) + strConstraints + " order by INOCTIME desc";
                strResult = OracleSQL.SimpleQuery("CCDB", strSQLFinal);
                strPrettyPrint = "The " + strParameter + " value for " + strConstraintsPP + " is " + strResult + " " + parameterUOM[strParameter.ToUpper()];
            }

            //Pretty print the resul
            return strPrettyPrint.Trim();
        }
        public static string SampleQuery(string strParameter, string strProduct, string strVesselClass, string strEquipment, string strRun, string strLot, string strStation, string strModifier, double dblDuration, string sTimeflag)
        {
            //Initialize variables, default error message
            string strResult = "Sorry! I can't seem to find the sample data you requested, can you refine your request and try again?";
            string strParameterField = parameterField[strParameter.ToUpper()];
            
            //Treat lack of qualifers as "current"
            string strSQLSort = " order by SAMPLEID desc"; 
            if (string.IsNullOrEmpty(strModifier)) { strModifier = "CURRENT"; };

            //Sample Query
            //select * from ISI.CCSAMPLES, ISI.CCBATCHES where ISI.CCSAMPLES.BATCHID=ISI.CCBATCHES.BATCHID and SCALEID='12000' and RUN=162 and PRODUCTID='rhuMAb VEGF G7 v1.2' order by SAMPLEID

            //Define Base query
            string strSQLbase = "select <FIELD> from ISI.CCSAMPLES, ISI.CCBATCHES where ISI.CCSAMPLES.BATCHID=ISI.CCBATCHES.BATCHID";

            //Combine all constraints
            string strConstraints = ConcatConstraints(strProduct, strVesselClass, strEquipment, strRun, strLot, strStation);
            string strConstraintsPP = PrettyPrintConstraints(strProduct, strVesselClass, strEquipment, strRun, strLot, strStation);

            //Find the most likely batch that matches search criteria
            string strBatchID = OracleSQL.SimpleQuery("CCDB", "SELECT ISI.CCBATCHES.BATCHID FROM ISI.CCBATCHES where ISI.CCBATCHES.BATCHID is not null" + strConstraints +
                " order by INOCTIME desc");

            if (!string.IsNullOrEmpty(strBatchID))
            {
                if (!string.IsNullOrEmpty(strModifier))
                {
                    switch (strModifier.ToUpper())
                    {
                        case "INITIAL":
                            strSQLSort = " order by SAMPLEID";
                            break;
                        case "FINAL":
                            strSQLSort = " order by SAMPLEID desc";
                            break;
                        case "FIRST":
                            strSQLSort = " order by SAMPLEID";
                            break;
                        case "LAST":
                            strSQLSort = " order by SAMPLEID desc";
                            break;
                        case "CURRENT":
                            strSQLSort = " order by SAMPLEID desc";
                            break;
                        case "PREVIOUS":
                            strSQLSort = " order by SAMPLEID desc";
                            strParameterField = "lag(" + strParameterField + ")over(order by SAMPLEID)";
                            break;
                        case "MIN":
                            strParameterField = "MIN(" + strParameterField + ")";
                            break;
                        case "MAX":
                            strParameterField = "MAX(" + strParameterField + ")";
                            break;
                        case "AVERAGE":
                            strParameterField = "ROUND(AVG(" + strParameterField + "),2)";
                            break;
                        case "PEAK":
                            strParameterField = "MAX(" + strParameterField + ")";
                            break;
                        default:
                            break;
                    }
                }

                //Handle if user is requesting a data point from a specific duration
                if (dblDuration > 0)
                {
                    strSQLSort = " order by SAMPLEID) order by TIMEDIFF";
                    string strSQLFinal = strSQLbase.Replace("<FIELD>", strParameterField + " from (select ABS(DURATION - " + dblDuration / 3600 + " ) TIMEDIFF, DURATION, " + strParameterField)
                        + " and ISI.CCBATCHES.BATCHID= " + strBatchID + strSQLSort;
                    strResult = OracleSQL.SimpleQuery("CCDB", strSQLFinal);
                }
                else
                {
                    string strSQLFinal = strSQLbase.Replace("<FIELD>", strParameterField) + " and ISI.CCBATCHES.BATCHID= " + strBatchID + strSQLSort;
                    strResult = OracleSQL.SimpleQuery("CCDB", strSQLFinal);
                }
            }

            if (!string.IsNullOrEmpty(sTimeflag))
            {
                string strPrettyPrint = "The " + strModifier.ToLower() + " " + strParameter + " time for " + strConstraintsPP + " was on " + strResult;
                return strPrettyPrint.Trim();
            }
            else
            {
                //Pretty print the result
                string strPrettyPrint = "The " + strModifier.ToLower() + " " + strParameter + " value for " + strConstraintsPP + " is " + strResult + " " + parameterUOM[strParameter.ToUpper()];
                return strPrettyPrint.Trim();
            }
            
        }
        public static string PredictGlucoseQuery(string strProduct, string strVesselClass, string strEquipment, string strRun, string strLot, string strModifier, double dblDuration, string strTimeFlag, string strlimitvalue)
        {
            //Glucose prediction, becuase TK, caclulation done in hours
            string strResult = "Sorry! I can't perform any glucose predictions with the given parameters, can you refine your request and try again?";
            double dblResult = -1;
            string strConstraint = null;
            string strSort = " and DURATION>0";

            //Sample Query
            //Durations in CCDB is measured in hours
            //select GLUCOSE from (select ABS(DURATION - 200) TIMEDIFF, DURATION, GLUCOSE from ISI.CCSAMPLES, ISI.CCBATCHES where ISI.CCSAMPLES.BATCHID=ISI.CCBATCHES.BATCHID    
            //and SCALEID='12000' and RUN=162 and PRODUCTID='rhuMAb VEGF G7 v1.2' order by SAMPLEID) order by TIMEDIFF

            //Define Base query
            string strSQLbase = "select <FIELD> from ISI.CCSAMPLES, ISI.CCBATCHES where ISI.CCSAMPLES.BATCHID=ISI.CCBATCHES.BATCHID";

            //Default scale to 12kL as that'll be 99% of searches
            if (!string.IsNullOrEmpty(strRun) && string.IsNullOrEmpty(strVesselClass) && string.IsNullOrEmpty(strLot)) { strVesselClass = "12KL"; };

            //Combine all constraints
            string strConstraints = ConcatConstraints(strProduct, strVesselClass, strEquipment, strRun, strLot, null);

            //Find the top most in-progress run that matches the search criteria, because predictions are only applicable for a single, non-completed run.
            string strBatchID = OracleSQL.SimpleQuery("CCDB", "SELECT ISI.CCBATCHES.BATCHID FROM ISI.CCBATCHES where ISI.CCBATCHES.BATCHID is not null" + strConstraints +
                " and INOCTIME > (SYSDATE - 30) and HARVESTTIME is null order by INOCTIME desc");

            //If proper batch is found, make the calculation
            if (!string.IsNullOrEmpty(strBatchID))
            {
                //Find the duration of the previous max glucose
                string strSQLFinal = strSQLbase.Replace("<FIELD>", "DURATION from (select CNT, DURATION, GLUCOSE, ROUND(POWER((GLUCOSE - lag(GLUCOSE) over(order by CNT desc)) / GLUCOSE,2),2) MARK " +
                    " from (select rownum CNT, DURATION, GLUCOSE") + " and ISI.CCBATCHES.BATCHID= " + strBatchID + " order by DURATION desc)) where MARK > 1 order by DURATION desc";
                string strMarkDuration = OracleSQL.SimpleQuery("CCDB", strSQLFinal);
                if (!string.IsNullOrEmpty(strMarkDuration)) { strSort = " and DURATION>" + strMarkDuration; };

                //Use the last three data points if it's TK's formula
                if (strModifier == "TK")
                {
                    strConstraint = " from (select DURATION, GLUCOSE, rownum CNT from (select DURATION, GLUCOSE ";
                    strSort = strSort + " order by DURATION desc)) where CNT<=3 ";
                }

                //Calculate the slope and intercept
                string strSQLSlope = strSQLbase.Replace("<FIELD>", "ROUND(REGR_SLOPE(GLUCOSE,DURATION),9)" + strConstraint) + " and ISI.CCBATCHES.BATCHID= " + strBatchID + strSort;
                string strSlope = OracleSQL.SimpleQuery("CCDB", strSQLSlope);

                string strSQLIntercept = strSQLbase.Replace("<FIELD>", "ROUND(REGR_INTERCEPT(GLUCOSE,DURATION),9)" + strConstraint) + " and ISI.CCBATCHES.BATCHID= " + strBatchID + strSort;
                string strIntercept = OracleSQL.SimpleQuery("CCDB", strSQLIntercept);

                //Get the current duration
                string strSQLDuration = strSQLbase.Replace("<FIELD>", "distinct ROUND((SYSDATE-CAST((FROM_TZ(CAST(INOCTIME AS TIMESTAMP),'+00:00') AT TIME ZONE 'US/Pacific') AS DATE))*24,9) VAL")
                    + " and ISI.CCBATCHES.BATCHID= " + strBatchID;
                string strCurrentDuration = OracleSQL.SimpleQuery("CCDB", strSQLDuration);

                //Make the final calculation
                if (!string.IsNullOrEmpty(strSlope) && !string.IsNullOrEmpty(strIntercept) && !string.IsNullOrEmpty(strCurrentDuration))
                {
                    //Check if the user is requesting a timestamp
                    if (!string.IsNullOrEmpty(strTimeFlag) && !string.IsNullOrEmpty(strlimitvalue))
                    {
                        //User is requesting a time based on a desired target value, go find it. Keep in mind duration is calculated in hours
                        dblDuration = (Convert.ToDouble(strlimitvalue) - Convert.ToDouble(strIntercept)) / Convert.ToDouble(strSlope);

                        //Get the desired time from the duration
                        if (dblDuration > Convert.ToDouble(strCurrentDuration))
                        {
                            string strTimeQuery = strSQLbase.Replace("<FIELD>", "CAST((FROM_TZ(CAST(INOCTIME + " + Convert.ToDouble(dblDuration) / 24 + " AS TIMESTAMP),'+00:00') AT TIME ZONE 'US/Pacific') AS DATE)")
                                + " and ISI.CCBATCHES.BATCHID= " + strBatchID;
                            strResult = OracleSQL.SimpleQuery("CCDB", strTimeQuery);
                        }
                        else
                        {
                            strResult = "The glucose value for this batch has likely already exceeded " + strlimitvalue + " g/L by now";
                        }
                    }
                    else
                    {
                        dblResult = Convert.ToDouble(strSlope) * (Convert.ToDouble(strCurrentDuration) + dblDuration / 3600) + Convert.ToDouble(strIntercept);
                    }
                }
                else
                {
                    strResult = "Sorry! I can't make glucose consumption prediction for the batch you requested as a batch/glucose feed just occurred. Please try again later.";
                }
            }
            else
            {
                strResult = "Sorry! Either the batch you requested is already complete or I can't seem to find the batch you requested, can you refine your request and try again?";
            }

            if (dblResult > -1) { strResult = Math.Round(dblResult, 2).ToString(); };
            return strResult;
        }
        public static string PredictPCVQuery(string strProduct, string strVesselClass, string strEquipment, string strRun, string strLot, double dblDuration, string strTimeFlag, string strlimitvalue)
        {
            //PCV prediction, only really useful for N-3 thru N-1, maybe at the beginning of N as well
            string strResult = "Sorry! I can't perform any PCV predictions with the given parameters, can you refine your request and try again?";
            double dblResult = -1;

            //Sample Query
            //Durations in CCDB is measured in hours
            //select GLUCOSE from (select ABS(DURATION - 200) TIMEDIFF, DURATION, GLUCOSE from ISI.CCSAMPLES, ISI.CCBATCHES where ISI.CCSAMPLES.BATCHID=ISI.CCBATCHES.BATCHID    
            //and SCALE='12000' and RUN=162 and PRODUCTID='rhuMAb VEGF G7 v1.2' order by SAMPLEID) order by TIMEDIFF

            //Define Base query
            string strSQLbase = "select <FIELD> from ISI.CCSAMPLES, ISI.CCBATCHES where ISI.CCSAMPLES.BATCHID=ISI.CCBATCHES.BATCHID";

            //Combine all constraints
            string strConstraints = ConcatConstraints(strProduct, strVesselClass, strEquipment, strRun, strLot, null);

            //Find the top most in-progress run that matches the search criteria, because predictions are only applicable for a single, non-completed run.
            string strBatchID = OracleSQL.SimpleQuery("CCDB", "SELECT ISI.CCBATCHES.BATCHID FROM ISI.CCBATCHES where ISI.CCBATCHES.BATCHID is not null" + strConstraints +
                " and INOCTIME > (SYSDATE - 30) and HARVESTTIME is null order by INOCTIME desc");

            if (!string.IsNullOrEmpty(strBatchID))
            {
                //Calculate the slope and intercept
                string strSQLSlope = strSQLbase.Replace("<FIELD>", "ROUND(REGR_SLOPE(LN(PCV),DURATION),9)") + " and ISI.CCBATCHES.BATCHID= " + strBatchID + " order by DURATION";
                string strSlope = OracleSQL.SimpleQuery("CCDB", strSQLSlope);

                string strSQLIntercept = strSQLbase.Replace("<FIELD>", "ROUND(REGR_INTERCEPT(LN(PCV),DURATION),9)") + " and ISI.CCBATCHES.BATCHID= " + strBatchID + " order by DURATION";
                string strIntercept = OracleSQL.SimpleQuery("CCDB", strSQLIntercept);

                //Get the current duration
                string strSQLDuration = strSQLbase.Replace("<FIELD>", "distinct ROUND((SYSDATE-CAST((FROM_TZ(CAST(INOCTIME AS TIMESTAMP),'+00:00') AT TIME ZONE 'US/Pacific') AS DATE))*24,9) VAL")
                     + " and ISI.CCBATCHES.BATCHID= " + strBatchID;
                string strCurrentDuration = OracleSQL.SimpleQuery("CCDB", strSQLDuration);

                //Make the final calculation
                if (!string.IsNullOrEmpty(strSlope) && !string.IsNullOrEmpty(strIntercept) && !string.IsNullOrEmpty(strCurrentDuration))
                {
                    if (!string.IsNullOrEmpty(strTimeFlag) && !string.IsNullOrEmpty(strlimitvalue))
                    {
                        //User is requesting a time based on a desired target value, go find it. Keep in mind duration is calculated in hours
                        dblDuration = (Math.Log(Convert.ToDouble(strlimitvalue)) - Convert.ToDouble(strIntercept)) / Convert.ToDouble(strSlope);

                        //Get the desired time from the duration
                        if (dblDuration > Convert.ToDouble(strCurrentDuration))
                        {
                            string strTimeQuery = strSQLbase.Replace("<FIELD>", "CAST((FROM_TZ(CAST(INOCTIME + " + Convert.ToDouble(dblDuration) / 24 + " AS TIMESTAMP),'+00:00') AT TIME ZONE 'US/Pacific') AS DATE)")
                                + " and ISI.CCBATCHES.BATCHID= " + strBatchID;
                            strResult = OracleSQL.SimpleQuery("CCDB", strTimeQuery);
                        }
                        else
                        {
                            strResult = "The PCV for this batch has likely already exceeded " + strlimitvalue + "% by now";
                        }
                    }
                    else
                    {
                        dblResult = Math.Exp(Convert.ToDouble(strSlope) * (Convert.ToDouble(strCurrentDuration) + dblDuration / 3600) + Convert.ToDouble(strIntercept));
                    }
                }
                else
                {
                    strResult = "Sorry! I can't make any PCV predictions as there isn't enough data yet. Please try again later";
                }

                if (dblResult > -1) { strResult = Math.Round(dblResult, 2).ToString(); };
            }
            else
            {
                strResult = "Sorry! Either the batch you requested is already complete or I can't seem to find the batch you requested, can you refine your request and try again?";
            }

            return strResult;
        }
        public static string ViabilityCrashQuery(string strProduct, string strVesselClass, string strEquipment, string strRun, string strLot)
        {
            //Viability Crash prediction...cuz why not
            string strResult = "Sorry! I can't analyze any viability crashes with the given parameters, can you refine your request and try again?";
            double dblResult = -1;

            //Sample Query
            //Durations in CCDB is measured in hours
            //select VIABILITY from ISI.CCSAMPLES, ISI.CCBATCHES where ISI.CCSAMPLES.BATCHID=ISI.CCBATCHES.BATCHID    
            //and SCALE='12000' and RUN=162 and PRODUCTID='rhuMAb VEGF G7 v1.2' order by SAMPLEID

            //Define Base query
            string strSQLbase = "select <FIELD> from ISI.CCSAMPLES, ISI.CCBATCHES where ISI.CCSAMPLES.BATCHID=ISI.CCBATCHES.BATCHID";

            //Default class to 12kL if not provided with run and product (will be 99%) of searches
            if (!string.IsNullOrEmpty(strProduct) && !string.IsNullOrEmpty(strRun) && string.IsNullOrEmpty(strVesselClass))
            {
                strVesselClass = "12KL";
            }

            //Combine all constraints
            string strConstraints = ConcatConstraints(strProduct, strVesselClass, strEquipment, strRun, strLot, null);

            //Calculate the slope based on the last 4 viability points
            string strSQLSlope = strSQLbase.Replace("<FIELD>", "ROUND(REGR_SLOPE(VIABILITY,DURATION),9) from (select DURATION, VIABILITY, rownum CNT from (select DURATION, VIABILITY")
                + strConstraints + " order by DURATION desc)) where CNT<=4";
            string strSlope = OracleSQL.SimpleQuery("CCDB", strSQLSlope);

            //Viability crash threshold is around -0.4, -0.3 might be threshold for early drop/crash
            if (!string.IsNullOrEmpty(strSlope))
            {
                if (Convert.ToDouble(strSlope) <= -0.4)
                {
                    //Viability drop detected, get the viability drop over the duration
                    string strSQLDuration = strSQLbase.Replace("<FIELD>", "MAX(DURATION) - MIN(DURATION) from (select DURATION, VIABILITY, rownum CNT from (select DURATION, VIABILITY")
                        + strConstraints + " order by DURATION desc)) where CNT<=4";
                    string strDuration = OracleSQL.SimpleQuery("CCDB", strSQLDuration);

                    string strSQLViability = strSQLbase.Replace("<FIELD>", "MAX(VIABILITY) - MIN(VIABILITY) from (select DURATION, VIABILITY, rownum CNT from (select DURATION, VIABILITY")
                        + strConstraints + " order by DURATION desc)) where CNT<=4";
                    string strViability = OracleSQL.SimpleQuery("CCDB", strSQLViability);

                    if (!string.IsNullOrEmpty(strDuration) && !string.IsNullOrEmpty(strViability))
                    {
                        strResult = "Uh oh...Viability crash detected! There was a viability drop of " + strViability + "% over " + strDuration + " hours";
                    }
                }
                else if (Convert.ToDouble(strSlope) <= -0.3 && Convert.ToDouble(strSlope) > -0.4)
                {
                    strResult = "Hmm...there might be a viability drop with this batch, it might be too early to tell, suggest keeping an eye on it.";
                }
                else
                {
                    strResult = "No viability crash detected. Culture looks fine =)";
                }
            }
            else
            {
                strResult = "Sorry! There isn't enough viability data yet. Please try again later.";
            }

            if (dblResult > -1) { strResult = Math.Round(dblResult, 2).ToString(); };
            return strResult;
        }

        //Support Calls
        public static string ConcatConstraints(string strProduct, string strVesselClass, string strEquipment, string strRun, string strLot, string strStation)
        {
            string strResult = null;

            //Initialize variables
            string strSQLProduct = null;
            string strSQLScale = null;
            string strSQLEquipment = null;
            string strSQLRun = null;
            string strSQLLot = null;
            string strSQLStation = null;
            string strScaleID = null;
            if (!string.IsNullOrEmpty(strVesselClass)) { strScaleID = ScaleID[strVesselClass.ToUpper()]; };

            //Check if product is available by lookup (shorthand)
            try
            {
                string strProductLookup = Define.productID[strProduct.ToUpper()];
                if (!string.IsNullOrEmpty(strProduct)) { strSQLProduct = " and PRODUCTID='" + Define.productID[strProduct.Trim().ToUpper()] + "'"; };
            }
            catch (KeyNotFoundException)
            {
                if (!string.IsNullOrEmpty(strProduct)) { strSQLProduct = " and UPPER(PRODUCTID) like '%" + strProduct.Trim().ToUpper() + "%'"; };
            }
            catch (NullReferenceException)
            { 
                //No products, do nothing
                strSQLProduct = null;
            }

            if (!string.IsNullOrEmpty(strVesselClass)) { strSQLScale = " and ISI.CCBATCHES.SCALEID=" + strScaleID + ""; };
            if (!string.IsNullOrEmpty(strEquipment)) { strSQLEquipment = " and ISI.CCBATCHES.FERMID='" + strEquipment.Trim().ToUpper() + "'"; };
            if (!string.IsNullOrEmpty(strRun)) { strSQLRun = " and RUN=" + strRun.Trim(); };
            if (!string.IsNullOrEmpty(strLot)) { strSQLLot = " and LOT='" + strLot.Trim() + "'"; };
            if (!string.IsNullOrEmpty(strStation)) { strSQLStation = " and STATION='" + strStation.Trim() + "'"; };

            //Combine all constraints
            strResult = strSQLProduct + strSQLScale + strSQLEquipment + strSQLRun + strSQLLot + strSQLStation;
            return strResult;
        }
        public static string PrettyPrintConstraints(string strProduct, string strVesselClass, string strEquipment, string strRun, string strLot, string strStation)
        {
            string strResult = null;

            //Initialize variables
            string strPPProduct = null;
            string strPPScale = null;
            string strPPEquipment = null;
            string strPPRun = null;
            string strPPLot = null;
            string strPPStation = null;

            //Check if product is available by lookup (shorthand)
            try
            {
                if (!string.IsNullOrEmpty(strProduct)) { strPPProduct = strProduct.Trim().ToUpper(); };
            }
            catch (NullReferenceException)
            {
                //No products, do nothing
                strPPProduct = null;
            }

            if (!string.IsNullOrEmpty(strVesselClass)) { strPPScale = " class " + strVesselClass.Trim().ToUpper() ; };
            if (!string.IsNullOrEmpty(strEquipment)) { strPPEquipment = " vessel " + strEquipment.Trim().ToUpper() ; };
            if (!string.IsNullOrEmpty(strRun)) { strPPRun = " run " + strRun.Trim(); };
            if (!string.IsNullOrEmpty(strLot)) { strPPLot = " lot " + strLot.Trim() ; };
            if (!string.IsNullOrEmpty(strStation)) { strPPStation = " station " + strStation.Trim() ; };

            //Combine all constraints
            strResult = strPPProduct + strPPScale + strPPEquipment + strPPRun + strPPLot + strPPStation;
            return strResult.Trim();
        }

        private static Dictionary<string, string> parameterField =
            new Dictionary<string, string>
            {
                {"PRODUCT","PRODUCTID"},
                {"PROCESS","PRODUCTID"},
                {"SCALE","SCALEID"}, 
                {"TANK","ISI.CCBATCHES.FERMID"}, 
                {"VESSEL","ISI.CCBATCHES.FERMID"}, 
                {"EQUIPMENT","ISI.CCBATCHES.FERMID"}, 
                {"FERM","ISI.CCBATCHES.FERMID"},
                {"BIOREACTOR","ISI.CCBATCHES.FERMID"}, 
                {"LOT","LOT"},
                {"RUN","RUN"},
                {"START TIME","CAST((FROM_TZ(CAST(INOCTIME AS TIMESTAMP),'+00:00') AT TIME ZONE 'US/Pacific') AS DATE)"},
                {"END TIME","CAST((FROM_TZ(CAST(HARVESTTIME AS TIMESTAMP),'+00:00') AT TIME ZONE 'US/Pacific') AS DATE)"},
                {"DURATION","ROUND((SYSDATE-CAST((FROM_TZ(CAST(INOCTIME AS TIMESTAMP),'+00:00') AT TIME ZONE 'US/Pacific') AS DATE))*24,2)"},
                {"THAW TIME","CAST((FROM_TZ(CAST(THAWTIME AS TIMESTAMP),'+00:00') AT TIME ZONE 'US/Pacific') AS DATE)"},
                {"THAW LINE","THAWLINE"},
                {"STATION","STATION"},
                {"GCODE","GCODE"},
                {"PCV","PCV"},
                {"VIABILITY","VIABILITY"},
                {"VIABLE CELL DENSITY","DENSITY"},
                {"VCD","DENSITY"},
                {"GLUCOSE","GLUCOSE"},
                {"LACTATE","LACTATE"},
                {"OFFLINE PH","PH"},
                {"CO2","CO2"},
                {"CARBON DIOXIDE","CO2"},
                {" NA","NA"}, 
                {"NA","NA"}, //TODO: get rid of hardcoding..
                {"SODIUM","NA"},
                {"NH4","NH4"},
                {"AMMONIUM","NH4"},
                {"OFFLINE DO2","O2"},
                {"OXYGEN","O2"},
                {"OSMO", "OSMOLALITY"},
                {"OSMOLALITY","OSMOLALITY"},
                {"ASGR","SLOPE"},
                {"GROWTH RATE","SLOPE"},
                {"GROWTH","SLOPE"}, //TODO: get rid of hardcoding..
                {"IVPCV","IVPCV"},
                {"IVCD","IVCD"},
                {"SAMPLE","CAST((FROM_TZ(CAST(SAMPLETIME AS TIMESTAMP),'+00:00') AT TIME ZONE 'US/Pacific') AS DATE)"},
                {"COUNT","CAST((FROM_TZ(CAST(SAMPLETIME AS TIMESTAMP),'+00:00') AT TIME ZONE 'US/Pacific') AS DATE)"},
            };

        private static Dictionary<string, string> parameterUOM =
            new Dictionary<string, string>
            {
                {"PRODUCT",""},
                {"PROCESS",""},
                {"SCALE",""}, 
                {"TANK",""}, 
                {"VESSEL",""}, 
                {"EQUIPMENT",""}, 
                {"FERM",""},
                {"BIOREACTOR",""}, 
                {"LOT",""},
                {"RUN",""},
                {"START TIME",""},
                {"END TIME",""},
                {"DURATION","hours"},
                {"THAW TIME",""},
                {"THAW LINE",""},
                {"STATION",""},
                {"GCODE",""},    
                {"PCV","%"},
                {"VIABILITY","%"},
                {"VIABLE CELL DENSITY","10^5 cells/mL"},
                {"VCD","10^5 cells/mL"},
                {"GLUCOSE","g/L"},
                {"LACTATE","g/L"},
                {"OFFLINE PH","pH"},
                {"CO2","mmHg"},
                {"CARBON DIOXIDE","mmHg"},
                {" NA","mmol/L"},
                {"NA","mmol/L"}, //TODO: get rid of hardcoding..
                {"SODIUM","mmol/L"},
                {"NH4","mmol/L"},
                {"AMMONIUM","mmol/L"},
                {"OFFLINE DO2","mmHg"},
                {"OXYGEN","mmHg"},
                {"OSMO", "mOsm/kg"},
                {"OSMOLALITY","mOsm/kg"},
                {"ASGR","day-1"},
                {"GROWTH RATE","day-1"},
                {"GROWTH","day-1"}, //TODO: get rid of hardcoding..
                {"IVPCV",""},
                {"IVCD",""},
                {"SAMPLE",""},
                {"COUNT",""},
            };

        private static Dictionary<string, string> ScaleID =
            new Dictionary<string, string>
            {
                {"20L","20"},
                {"80L","80"}, 
                {"400L","400"}, 
                {"2KL","2000"}, 
                {"12KL","12000"}, 
            };
    }
}