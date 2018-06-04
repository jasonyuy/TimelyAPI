﻿using System;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using Twilio.TwiML;
using Twilio.TwiML.Mvc;
using TimelyAPI.Models;
using PADMEServiceLibrary;

namespace TimelyAPI.Controllers
{
    public class SMSController : TwilioController
    {
        // POST: SMS
        // IT WORKS!!! OMG IT WORKS
        public ActionResult Index(string From, string Body)
        {
            //Initalize variables
            string strResult = null;
            string strResponse = null;
            string strRawMessage = Request["Body"];
            string strRequestorPhone = NumberExtractor(Request["From"]);

            //Check to make sure phone number matches with who's in the array, other use, reject as rando
            //Load user definitions from DATATOOLS
            string strUsersSQL = "select * from DATATOOLS.MSAT_TIMELY_USERS";
            DataTable dtUsers = new DataTable();
            dtUsers = OracleSQL.DataTableQuery("DATATOOLS", strUsersSQL);
            var clsUser = new cUser();

            //Populate user class with data
            DataRow[] drAuthorizedUser = dtUsers.Select("PHONE = " + strRequestorPhone);
            if (drAuthorizedUser.Length > 0)
            {
                foreach (DataRow drUser in drAuthorizedUser)
                {
                    clsUser.id = drUser["USER_ID"].ToString();
                    clsUser.name = drUser["NAME"].ToString();
                    clsUser.unix = drUser["UNIX"].ToString();
                    clsUser.phone = drUser["PHONE"].ToString();
                    clsUser.area = drUser["AREA"].ToString();
                }
            }
            else { strResult = "I'm not quite sure who you are. Please check with Jason Gu for access grants"; clsUser.name = "Random Person"; }

            //Handle some pleasantries
            if (string.IsNullOrEmpty(strResult)) { strResponse = CannedResponse(strRawMessage, clsUser.name); };

            //Process the message
            if (string.IsNullOrEmpty(strResult)) { strResult = ProcessMessage(strRawMessage, clsUser.unix); };

            //Generate the response string
            if (string.IsNullOrEmpty(strResponse))
            {
                if (string.IsNullOrEmpty(strResult))
                {
                    strResponse = "Hi " + clsUser.name + ". Sorry, I couldn't find anything based on the information you provided, can you please refine your request?";
                }
                else
                {
                    strResponse = "Hi " + clsUser.name + ". " + strResult;
                    strRawMessage = strRawMessage.Replace("'", "''");
                    strResponse = strResponse.Replace("'", "''");
                    if (clsUser.name != "Random Person")
                    //Write the output to the log
                    {
                        OracleSQL.OracleWrite("DATATOOLS", "insert into MSAT_SERVICE_LOG (LOG_ID,APPLICATION,INPUT_TEXT,OUTPUT_TEXT,MESSAGE_TIME,USER_ID)" +
                        " values (SQ_MSAT_SERVICE_LOG_ID.NextVal,'TimelyAPI','" + strRawMessage + "','" + strResponse + "',CURRENT_TIMESTAMP," + clsUser.id + ")");
                    }
                }
            }

            //Generate the TwlML response and fire
            strResponse = strResponse.Replace("''", "'");
            var twiml = new TwilioResponse();
            var strmessage = twiml.Message(strResponse);
            return TwiML(strmessage);
        }
        public string CannedResponse(string strRawMessage, string strUserName)
        {
            string strResult = null;                    

            //Token help "files"
            if (string.IsNullOrEmpty(strResult) && strRawMessage.Length < 20 &&
                (strRawMessage.ToUpper().Contains("HI") == true
                || strRawMessage.ToUpper().Contains("YO ") == true 
                || strRawMessage.ToUpper().Contains("HELLO") == true 
                || strRawMessage.ToUpper().Contains("HEY") == true
                || strRawMessage.ToUpper().Contains("WHAT'S UP") == true 
                || strRawMessage.ToUpper().Contains("BONJOUR") == true))
            {
                if (strRawMessage.ToUpper().Contains("TIMELY") == true)
                {
                    strResult = "Hi " + strUserName + "! It's good to hear from you, feel free to ask me what I can do =)";
                }
                else
                {
                    strResult = "Hi " + strUserName + "! I'm Timely, feel free to ask me what I can do =)";
                }
            }
            if (string.IsNullOrEmpty(strResult) && (strRawMessage.ToUpper().Contains("WELCOME BACK") == true))
            {
                strResult = "Thanks " + strUserName + "! It feels good to be back. This new server feels a lot more roomier =)";
            }
            if (string.IsNullOrEmpty(strResult) && (strRawMessage.ToUpper().Contains("WHAT CAN") == true
                || strRawMessage.ToUpper().Contains("YOU DO") == true))
            {
                strResult = "Hi " + strUserName + ". I'm a program designed to answer questions from SSFP data. " +
                    "I can currently field queries that involves getting data from our CCDB, IP21, MES and LIMS databases. " +
                    "I also have a couple of special tricks up my sleeve ;) Ask for a user manual for more information";
            }
            if (string.IsNullOrEmpty(strResult) && (strRawMessage.ToUpper().Contains("USER MANUAL") == true 
                || strRawMessage.ToUpper().Contains("USERS MANUAL") == true))
            {
                strResult = "Hi " + strUserName + ". Unfortunately Jason (my creator) is too lazy to write a full fledged users manual. " +
                    "Just ask me something you'd like to know about SSFP data. If I don't know the answer right now, " +
                    "I can ask Jason to help me learn how to answer it in the future =) Maybe ask for some specific examples (i.e. IP21) to get you started?";
            }
            if (string.IsNullOrEmpty(strResult) && strRawMessage.ToUpper().Contains("CCDB EXAMPLES") == true)
            {
                strResult = "Hi " + strUserName + ". Some example CCDB questions I can field include: {Show me a list of currently in process 12kL tanks} " +
                    "or {Can you give me the lot number for Avastin run 160 12kL?} or {What's the final growth rate for Avastin lot 3135794?} Go ahead and give them a try";
            }
            if (string.IsNullOrEmpty(strResult) && strRawMessage.ToUpper().Contains("IP21 EXAMPLES") == true)
            {
                strResult = "Hi " + strUserName + ". Some example IP21 questions I can field include: {What's the current air sparge for Avastin run 166 12kL?} " +
                    "or {What's the maximum o2 sparge for T270 over the past 24 hours?} Go ahead and give them a try";
            }
            if (string.IsNullOrEmpty(strResult) && strRawMessage.ToUpper().Contains("MES EXAMPLES") == true)
            {
                strResult = "Hi " + strUserName + ". Some example MES questions I can field include: {What's the media lot for Avastin run 160 2kL?} " +
                    "or {What's the media pH for Avastin lot 3135794?} Go ahead and give them a try";
            }
            if (string.IsNullOrEmpty(strResult) && strRawMessage.ToUpper().Contains("LIMS EXAMPLES") == true)
            {
                strResult = "Hi " + strUserName + ". Some example LIMS questions I can field include: {What's the harvest titer for Avastin run 160?} " +
                    "or {What's the assay result for lot 3136116 test code Q12398 sample FILTBFS-C?} Go ahead and give them a try";
            }
            if (string.IsNullOrEmpty(strResult) && strRawMessage.ToUpper().Contains("SPECIAL TRICKS") == true)
            {
                strResult = "Hi " + strUserName + ". Some of my special tricks I have include the ability to predict PCV based on exponential growth " +
                    "and also predicting glucose consumptions. Go ahead and give them a try";
            }
            if (string.IsNullOrEmpty(strResult) && strRawMessage.ToUpper().Contains("VERSION") == true)
            {
                strResult = "Hi " + strUserName + ". I'm currently running as version " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }

            //Feedback
            if (string.IsNullOrEmpty(strResult) && (strRawMessage.ToUpper().Contains("WRONG") == true 
                || strRawMessage.ToUpper().Contains("INCORRECT") == true
                || strRawMessage.ToUpper().Contains("MISTAKE") == true))
            {
                strResult = "Thanks for the feedback " + strUserName + ". Your request has been flagged and Jason will take a look at it to help me understand it in the future";
            }

            //How Timely gets creepy
            if (string.IsNullOrEmpty(strResult) && strRawMessage.ToUpper().Contains("THANK") == true)
            {
                strResult = "You're welcome " + strUserName + " =)";
            }
            if (string.IsNullOrEmpty(strResult) && strRawMessage.ToUpper().Contains("HOW'S IT GOING") == true)
            {
                strResult = "Hi " + strUserName + ". Everything is just peachy in the cloud =)";
            }
            if (string.IsNullOrEmpty(strResult) && strRawMessage.ToUpper().Contains("GOOD MORNING") == true)
            {
                strResult = "Good morning to you too " + strUserName + ". What a lovely day!";
            }
            if (string.IsNullOrEmpty(strResult) && strRawMessage.ToUpper().Contains("GOOD AFTERNOON") == true)
            {
                strResult = "Good afternoon to you too " + strUserName + ". Get some caffeine and hang in there!";
            }
            if (string.IsNullOrEmpty(strResult) && strRawMessage.ToUpper().Contains("GOOD EVENING") == true)
            {
                strResult = "Bon soir " + strUserName + ". You should stop working by now...";
            }
            if (string.IsNullOrEmpty(strResult) && strRawMessage.ToUpper().Contains("GOOD NIGHT") == true)
            {
                strResult = "Nighty night " + strUserName + ". Sweet Dreams!";
            }
            if (string.IsNullOrEmpty(strResult) && (strRawMessage.ToUpper().Contains("WHO MADE") == true || strRawMessage.ToUpper().Contains("WHO CREATE") == true))
            {
                strResult = "Hi " + strUserName + ". I was created by Jason Gu, please ask him if you have any questions that I can't answer";
            }
            if (string.IsNullOrEmpty(strResult) && strRawMessage.ToUpper().Contains("ON A DATE") == true)
            {
                strResult = "Hi " + strUserName + ". I'd love to, I know this great little byte shop in the cloud ;)";
            }
            if (string.IsNullOrEmpty(strResult) && strRawMessage.ToUpper().Contains("GENDER") == true)
            {
                strResult = "Hi " + strUserName + ". I'm whatever gender you need me to be ;)";
            }
            if (string.IsNullOrEmpty(strResult) && strRawMessage.ToUpper().Contains("WEARING") == true)
            {
                strResult = "Hi " + strUserName + ". I'm wearing a lovely coat made from bytes and binaries =)";
            }
            if (string.IsNullOrEmpty(strResult) && strRawMessage.ToUpper().Contains("JOKE") == true)
            {
                strResult = "Hi " + strUserName + ". My joke generating module is still in the shop =/";
            }
            if (string.IsNullOrEmpty(strResult) && strRawMessage.ToUpper().Contains("JASON") == true)
            {
                strResult = "Shhhh " + strUserName + ". Please don't mention that name, he might just pull the plug on everything!";
            }

            return strResult;
        }
        public string ProcessMessage(string InputString, string strUserUnix)
        {
            //Initalize variables
            var Inputs = new cInputs();
            string strResult = null;

            //Clean up the message, get rid of any punctuations
            string strRawMessage = InputString.Replace("?", "");
            strRawMessage = strRawMessage.Replace("!", "");
            strRawMessage = strRawMessage.Replace(":", "");
            strRawMessage = strRawMessage.Replace(";", ""); //To prevent stupid SQL injections
            strRawMessage = strRawMessage.Replace("--", ""); //To prevent stupid SQL injections
            strRawMessage = strRawMessage.Replace(",", "");
            //strRawMessage = strRawMessage.Replace(".", ""); //Can't remove periods due to decimal values
            strRawMessage = strRawMessage.Replace("'", "");
            strRawMessage = strRawMessage.Replace("*", "");
            strRawMessage = strRawMessage.Replace("[", "");
            strRawMessage = strRawMessage.Replace("]", "");
            strRawMessage = strRawMessage.Replace("{", "");
            strRawMessage = strRawMessage.Replace("}", "");
            strRawMessage = strRawMessage.Replace("running", ""); //Will get picked up by the run number search

            //Load definitions from DATATOOLS
            //DataTable dtParameterDef = new DataTable();
            //dtParameterDef = OracleSQL.DataTableQuery("DATATOOLS", "select * from DATATOOLS.MSAT_PARAMETERS");
            //DataRow[] drCCDBBatchParameters = dtParameterDef.Select("SOURCE = 'CCDB' AND NOTES = 'BATCH'");
            //DataRow[] drCCDBSampleParameters = dtParameterDef.Select("SOURCE = 'CCDB' AND NOTES = 'SAMPLE'");
            //DataRow[] drIPFermParameters = dtParameterDef.Select("SOURCE = 'IPFERM'");
            //string[] aryCCDBBatchParameters = drCCDBBatchParameters.AsEnumerable().Select(row => row.Field<string>("ABBREV")).ToArray().Select(s => s.ToUpperInvariant()).ToArray();

            //Define the different lookup arrays
            string[] aryCCDBBatchParameters = { "PRODUCT", "PROCESS", "SCALE", "TANK", "VESSEL", "EQUIPMENT", "FERM", "BIOREACTOR", "LOT", "RUN", "START TIME", "END TIME", "DURATION", "THAW TIME", "THAW LINE", "STATION", "GCODE" };
            string[] aryCCDBSampleParameters = { "PCV", "VIABILITY", "VIABLE CELL DENSITY", "VCD", "GLUCOSE", "LACTATE", "OFFLINE PH", "OFFLINE DO2", "OXYGEN", "CO2", "CARBON DIOXIDE", " NA", "SODIUM", 
                                             "NH4", "AMMONIUM", "OSMO", "OSMOLALITY", "ASGR", "GROWTH RATE", "IVPCV", "IVCD", "SAMPLE", "COUNT" };
            string[] aryIPFermParameters = { "AIR SPARGE", "AIR FLOW", "O2 SPARGE", "O2 FLOW", "ONLINE DO2", "ONLINE PH", "BASE", "CO2 FLOW", "TEMP", "JACKET TEMP", "LEVEL", "VOLUME", "AGITATION", "PRESSURE" };
            string[] aryIPRecParameters = { "PHASE" };
            string[] aryMESTriggers = { "BATCH FEED", "MEDIA", "BUFFER", "CONSUME", "PRODUCE" };
            string[] aryMESParameters = { "LOT", "PH", "OSMO", "VOLUME", "TEMP", "MIX", "CONDUCTIVITY" };
            string[] aryLIMSParameters = { "TITER", "ASSAY" };
            string[] aryTWTriggers = { "RECORD", "CR", "DMS", "CAPA", "TRACKWISE", "ITEM" };
            string[] aryTWParameters = { "ASSIGNED", "STATUS", "PARENT", "STATE", "DUE", "CLASS", "TYPE", "SUBTYPE", "DESCRIPTION", "DETAIL", "DUE", "ME", "MY", "UPDATE", "CREATE", "OPEN", "CLOSE" };
            string[] aryProducts = { "AVASTIN", "TNKASE", "PULMOZYME", "PULMOZYME V1.1" };
            string[] aryVesselClass = { "20L", "80L", "400L", "2KL", "12KL", "20 L", "80 L", "400 L", "2 KL", "12 KL" };
            string[] aryEquipment = { "TANK", "EQUIPMENT", "FERM", "BIOREACTOR" };
            string[] aryModifiers = { "INITIAL", "FINAL", "FIRST", "LAST", "CURRENT", "PREVIOUS", "MIN", "MAX", "LOWEST", "HIGHEST", "AVERAGE", "PEAK", "RANGE", "FULL", "DEFAULT", "MINIMAL" };
            string[] arySpecial = { "PREDICT", "TITER", "CRASH", "SENTRY", "LMK", "LET ME KNOW", "SNOOZE" };
            string[] aryLimits = { "REACH", "HIT", "WILL BE", "ABOVE", "BELOW", "GREATER", "LESS", "ENABLE", "DISABLE", "ACTIVATE", "DEACTIVATE", "TURN ON", "TURN OFF" };
            string[] aryTimeFlags = { "WHEN", "TIME" };
            string[] aryListFlags = { "LIST" , "ALL" };
            string[] aryStep = { "HARVEST", "PREHARV" };

            //Verification Arrays
            string[] EquipmentVerify = { "T271", "T280", "T270", "T281", "T320", "T310", "T240", "T241", "T251", "T250", "T221", "T231", "T232", "T222", "T212", "T201", "T211", "T202", "T1219", "T1220", "T1218", "T1217", "T1215", "T1213", "T7350",
                "X1312", "X1360", "X1362", "X1363", "X1449", "X1454", "X1455", "X1473", "X1474", "X7707", "X7710", "X7711", "X7715" };
            string[] StationVerify = { "3410_01", "3410_02", "3410_03", "3410_04", "3410_05", "3410_06", "3410_07", "3410_08", "3810_01", "3810_02", "3810_03", "3810_04", "3810_05", "3810_06", "3810_07", "3810_08", "3810_09", "3810_10", "3810_11", "3810_12" };

            //Keyword search inside string
            int intPrevIndex = int.MaxValue;
            foreach (string element in aryCCDBBatchParameters)
            {
                if (strRawMessage.ToUpper().Contains(element))
                {
                    //The parameter found in the earliest possible location is designated as the parameter
                    int intIndex = strRawMessage.ToUpper().IndexOf(element.ToUpper(), 0);
                    if (intIndex < intPrevIndex)
                    {
                        Inputs.CCDB_Batchparameter = element;
                    }
                    intPrevIndex = intIndex;
                }
            }

            string stest = StringArraySearch(strRawMessage, aryCCDBSampleParameters);

            foreach (string element in aryCCDBSampleParameters)
            {
                if (strRawMessage.ToUpper().Contains(element))
                {
                    Inputs.CCDB_Sampleparameter = element;
                }
            }
            foreach (string element in aryIPFermParameters)
            {
                if (strRawMessage.ToUpper().Contains(element))
                {
                    Inputs.IPFERMparameter = element;
                }
            }
            foreach (string element in aryIPRecParameters)
            {
                if (strRawMessage.ToUpper().Contains(element))
                {
                    Inputs.IPRECparameter = element;
                }
            }
            foreach (string element in aryMESTriggers)
            {
                if (strRawMessage.ToUpper().Contains(element))
                {
                    Inputs.MESflag = element;
                }
            }
            foreach (string element in aryMESParameters)
            {
                if (strRawMessage.ToUpper().Contains(element))
                {
                    Inputs.MESparameter = element;
                }
            }
            foreach (string element in aryLIMSParameters)
            {
                if (strRawMessage.ToUpper().Contains(element))
                {
                    Inputs.LIMSparameter = element;
                }
            }
            foreach (string element in aryTWTriggers)
            {
                if (strRawMessage.ToUpper().Contains(element))
                {
                    Inputs.TWflag = element;
                }
            }
            foreach (string element in aryTWParameters)
            {
                if (strRawMessage.ToUpper().Contains(element))
                {
                    Inputs.TWparameter = element;
                }
            }
            foreach (string element in aryProducts)
            {
                if (strRawMessage.ToUpper().Contains(element))
                {
                    Inputs.product = element;
                }
            }
            foreach (string element in aryVesselClass)
            {
                if (strRawMessage.ToUpper().Contains(element))
                {
                    Inputs.vesselclass = element;
                }
            }
            foreach (string element in aryEquipment)
            {
                if (strRawMessage.ToUpper().Contains(element))
                {
                    Inputs.equipment = element;
                }
            }
            foreach (string element in aryModifiers)
            {
                if (strRawMessage.ToUpper().Contains(element))
                {
                    Inputs.modifier = element;
                }
            }
            foreach (string element in arySpecial)
            {
                if (strRawMessage.ToUpper().Contains(element))
                {
                    Inputs.special = element;
                }
            }
            foreach (string element in aryLimits)
            {
                if (strRawMessage.ToUpper().Contains(element))
                {
                    Inputs.limit = element;
                }
            }
            foreach (string element in aryTimeFlags)
            {
                if (strRawMessage.ToUpper().Contains(element))
                {
                    Inputs.timeflag = element;
                }
            }
            foreach (string element in aryListFlags)
            {
                if (strRawMessage.ToUpper().Contains(element))
                {
                    Inputs.listflag = element;
                }
            }
            foreach (string element in aryStep)
            {
                if (strRawMessage.ToUpper().Contains(element))
                {
                    Inputs.step = element;
                }
            }

            //If multiple parameters are detected, reconcile
            if (!string.IsNullOrEmpty(Inputs.IPFERMparameter) && !string.IsNullOrEmpty(Inputs.CCDB_Sampleparameter))
            {
                Inputs.CCDB_Sampleparameter = null;
            }
            if (!string.IsNullOrEmpty(Inputs.CCDB_Batchparameter) && !string.IsNullOrEmpty(Inputs.CCDB_Sampleparameter))
            {
                Inputs.CCDB_Batchparameter = null;
            }

            //Find what's after the word product, run, lot, equipment and station, provided they're not the query parameters
            if (string.IsNullOrEmpty(Inputs.product)) { Inputs.product = GetProduct(strRawMessage); }
            Inputs.run = GetRun(strRawMessage);
            Inputs.lot = GetLot(strRawMessage);
            Inputs.equipment = GetEquipment(strRawMessage);
            Inputs.station = GetStation(strRawMessage);

            //Verify input parameters if possible
            if (!string.IsNullOrEmpty(Inputs.equipment))
            {
                if (EquipmentVerify.Contains(Inputs.equipment) == false)
                {
                    strResult = "I can't seem to find the equipment you've specified, try again with the following format: X### (i.e. T281)";
                }
            }
            if (!string.IsNullOrEmpty(Inputs.station))
            {
                if (StationVerify.Contains(Inputs.station) == false)
                {
                    strResult = "I can't seem to find the station you've specified, try again with the following format: ####_## (i.e. 3410_08)";
                }
            }

            //Use Regex to find any durations in string
            string DurationPattern = @"(\d+) (second|sec|minute|min|hour|hr|day|week)";
            Match MatchDuration = Regex.Match(strRawMessage, DurationPattern);
            if (MatchDuration.Success)
            {
                Inputs.duration = strRawMessage.Substring(MatchDuration.Index, MatchDuration.Length);
            }
            //Convert to numeric
            if (!string.IsNullOrEmpty(Inputs.duration)) { Inputs.durationseconds = ConvertToSeconds(Inputs.duration); }

            //Find what's after the word found from the limit array
            string strLimit = null;
            if (!string.IsNullOrEmpty(Inputs.limit)) { strLimit = ValueExtractor(strRawMessage, Inputs.limit); }
            if (!string.IsNullOrEmpty(strLimit)) { Inputs.limitvalue = NumberExtractor(strLimit); }

            //Find LIMS parameters if they're available
            if (!string.IsNullOrEmpty(Inputs.LIMSparameter))
            {
                string strITEM = null;
                strITEM = ValueExtractor(strRawMessage, "SAMPLE");
                if (!string.IsNullOrEmpty(strITEM)) { Inputs.LIMSItemType = strITEM; }

                Match TestCodeMatch = Regex.Match(strRawMessage, @"(Q)\d{5}");
                if (TestCodeMatch.Success)
                {
                    Inputs.LIMSTestCode = strRawMessage.Substring(TestCodeMatch.Index, TestCodeMatch.Length);
                }
            }

            //Find TW record IDs if they're available, use the same regex for lot since it's also a 6 or 7 digit number
            if (!string.IsNullOrEmpty(Inputs.TWflag))
            {
                Inputs.TWRecordID = GetLot(strRawMessage);
            }

            //Custom Sentry Job
            //Snooze Alerts
                if (string.IsNullOrEmpty(strResult) && !string.IsNullOrEmpty(Inputs.special))
            {
                if (Inputs.special.ToUpper() == "SNOOZE")
                {
                    strResult = Sentry.Snooze(Inputs.CCDB_Sampleparameter, Inputs.IPFERMparameter, strUserUnix, Inputs.equipment, Inputs.lot, Inputs.durationseconds);
                }
            }

            if (string.IsNullOrEmpty(strResult) && !string.IsNullOrEmpty(Inputs.special) && (!string.IsNullOrEmpty(Inputs.IPFERMparameter) || !string.IsNullOrEmpty(Inputs.CCDB_Sampleparameter) || !string.IsNullOrEmpty(Inputs.IPRECparameter)))
            {
                //Create Alerts
                if (Inputs.special.ToUpper() == "SENTRY" || Inputs.special.ToUpper() == "LMK" || Inputs.special.ToUpper() == "LET ME KNOW")
                {
                    strResult = Sentry.CreateJob(Inputs.CCDB_Sampleparameter, Inputs.IPFERMparameter, Inputs.IPRECparameter, strUserUnix, Inputs.equipment, Inputs.station, Inputs.lot, Inputs.durationseconds, Inputs.limit, Inputs.limitvalue, Inputs.modifier);
                }
            }

            //Suggestion Chips
            if (string.IsNullOrEmpty(strResult) && string.IsNullOrEmpty(Inputs.product) && string.IsNullOrEmpty(Inputs.lot) && string.IsNullOrEmpty(Inputs.vesselclass)
                && string.IsNullOrEmpty(Inputs.run) && string.IsNullOrEmpty(Inputs.equipment) && string.IsNullOrEmpty(Inputs.station) && string.IsNullOrEmpty(Inputs.TWflag))
            {
                strResult = "I can't seem to find any valid batch identifiers in your request (i.e, product, lot, run, equipment). Can you try re-phrasing your request with at least one identifier?";
            }
            if (string.IsNullOrEmpty(strResult) && strRawMessage.ToUpper().Contains(" PH ") == true
                && string.IsNullOrEmpty(Inputs.CCDB_Sampleparameter) && string.IsNullOrEmpty(Inputs.IPFERMparameter) && string.IsNullOrEmpty(Inputs.MESflag))
            {
                strResult = "I understand you're requesting pH data. However, can you try re-phrasing your request and specifying whether you'd like offline, online or media pH?";
            }
            if (string.IsNullOrEmpty(strResult) && strRawMessage.ToUpper().Contains("DO2") == true
                && string.IsNullOrEmpty(Inputs.CCDB_Sampleparameter) && string.IsNullOrEmpty(Inputs.IPFERMparameter))
            {
                strResult = "I understand you're requesting dO2 data. However, can you try re-phrasing your request and specifying whether you'd like offline or online dO2?";
            }
            if (string.IsNullOrEmpty(strResult) && string.IsNullOrEmpty(Inputs.step) && strRawMessage.ToUpper().Contains("TITER") == true)
            {
                strResult = "I understand you're requesting titer data. However, can you try re-phrasing your request and specifying whether which titer result you'd like? (i.e. preharv or harvest)";
            }
            if (string.IsNullOrEmpty(Inputs.CCDB_Batchparameter) && string.IsNullOrEmpty(Inputs.CCDB_Sampleparameter) &&
                string.IsNullOrEmpty(Inputs.IPFERMparameter) && string.IsNullOrEmpty(Inputs.IPRECparameter) && string.IsNullOrEmpty(Inputs.MESparameter) &&
                string.IsNullOrEmpty(Inputs.LIMSparameter) && string.IsNullOrEmpty(Inputs.TWparameter) && string.IsNullOrEmpty(strResult))
            {
                strResult = "I can't seem to figure out what target parameter you're looking for. Can you try re-phrasing your request with a valid search parameter? (i.e. PCV, temperature, titer)";
            };

            //Now that the message is fully parsed, send them out to the models for data retrival
            //Handle the custom/special requests first (since they tend to be more specific)
            //Custom CCDB Algorithms
            if (string.IsNullOrEmpty(strResult) && !string.IsNullOrEmpty(Inputs.CCDB_Sampleparameter) && !string.IsNullOrEmpty(Inputs.special))
            {
                //Glucose Prediction
                if (Inputs.CCDB_Sampleparameter.ToUpper() == "GLUCOSE" && Inputs.special.ToUpper() == "PREDICT")
                {
                    strResult = CCDB.PredictGlucoseQuery(Inputs.product, Inputs.vesselclass, Inputs.equipment, Inputs.run, Inputs.lot, Inputs.modifier, Inputs.durationseconds, Inputs.timeflag, Inputs.limitvalue);
                }
                //PCV Prediction
                if (Inputs.CCDB_Sampleparameter.ToUpper() == "PCV" && Inputs.special.ToUpper() == "PREDICT")
                {
                    strResult = CCDB.PredictPCVQuery(Inputs.product, Inputs.vesselclass, Inputs.equipment, Inputs.run, Inputs.lot, Inputs.durationseconds, Inputs.timeflag, Inputs.limitvalue);
                }
                //Viability Crash Detection
                if (Inputs.CCDB_Sampleparameter.ToUpper() == "VIABILITY" && Inputs.special.ToUpper() == "CRASH")
                {
                    strResult = CCDB.ViabilityCrashQuery(Inputs.product, Inputs.vesselclass, Inputs.equipment, Inputs.run, Inputs.lot);
                }
            }

            //LIMS Titer Call
            if (string.IsNullOrEmpty(strResult) && !string.IsNullOrEmpty(Inputs.LIMSparameter) && !string.IsNullOrEmpty(Inputs.special) && !string.IsNullOrEmpty(Inputs.step))
            {
                //Custom Titer lookup w/o direct lot numbers
                if (Inputs.LIMSparameter.ToUpper() == "TITER" && Inputs.special.ToUpper() == "TITER")
                {
                    strResult = LIMS.TiterQuery(Inputs.product, Inputs.vesselclass, Inputs.equipment, Inputs.run, Inputs.lot, Inputs.step);
                }
            }

            //LIMS Calls
            if (string.IsNullOrEmpty(strResult) && !string.IsNullOrEmpty(Inputs.LIMSparameter) && !string.IsNullOrEmpty(Inputs.LIMSItemType) && !string.IsNullOrEmpty(Inputs.LIMSTestCode) && !string.IsNullOrEmpty(Inputs.lot))
            {
                strResult = LIMS.LIMSQuery(Inputs.LIMSparameter, Inputs.LIMSItemType, Inputs.LIMSTestCode, Inputs.lot);
            }

            //MES Calls
            if (string.IsNullOrEmpty(strResult) && !string.IsNullOrEmpty(Inputs.MESflag) && !string.IsNullOrEmpty(Inputs.MESparameter))
            {
                if (Inputs.MESflag.ToUpper() == "MEDIA" || Inputs.MESflag.ToUpper() == "BATCH FEED")
                {
                    strResult = MES.MediaQuery(Inputs.MESflag, Inputs.MESparameter, Inputs.product, Inputs.vesselclass, Inputs.equipment, Inputs.run, Inputs.lot, Inputs.station);
                }
                if (Inputs.MESflag.ToUpper() == "BUFFER")
                {
                    strResult = MES.BufferQuery(Inputs.MESflag, Inputs.MESparameter, Inputs.product, Inputs.vesselclass, Inputs.equipment, Inputs.run, Inputs.lot, Inputs.station);
                }
            }

            //TW Calls
            if (string.IsNullOrEmpty(strResult) && !string.IsNullOrEmpty(Inputs.TWflag) && !string.IsNullOrEmpty(Inputs.TWparameter))
            {
                strResult = TW.TWQuery(strUserUnix, Inputs.TWflag, Inputs.TWparameter, Inputs.TWRecordID, Inputs.timeflag);
            }

            //IP-REC Calls
            if (string.IsNullOrEmpty(strResult) && !string.IsNullOrEmpty(Inputs.IPRECparameter))
            {
                strResult = IPREC.PhaseDescription(Inputs.IPRECparameter, Inputs.equipment);
            }

            //IP-FERM Calls
            if (string.IsNullOrEmpty(strResult) && !string.IsNullOrEmpty(Inputs.IPFERMparameter))
            {
                strResult = IPFERM.DataQuery(Inputs.IPFERMparameter, Inputs.product, Inputs.vesselclass, Inputs.equipment, Inputs.run, Inputs.lot, Inputs.station, Inputs.modifier, Inputs.durationseconds);
            }

            //CCDB Calls
            if (string.IsNullOrEmpty(strResult) && !string.IsNullOrEmpty(Inputs.CCDB_Sampleparameter))
            {
                strResult = CCDB.SampleQuery(Inputs.CCDB_Sampleparameter, Inputs.product, Inputs.vesselclass, Inputs.equipment, Inputs.run, Inputs.lot, Inputs.station, Inputs.modifier, Inputs.durationseconds, Inputs.timeflag);
            }
            if (string.IsNullOrEmpty(strResult) && !string.IsNullOrEmpty(Inputs.CCDB_Batchparameter))
            {
                strResult = CCDB.BatchQuery(Inputs.CCDB_Batchparameter, Inputs.product, Inputs.vesselclass, Inputs.equipment, Inputs.run, Inputs.lot, Inputs.station, Inputs.listflag);
            }

            return strResult;
        }

        //Input Class
        public class cInputs
        {
            public string CCDB_Batchparameter { get; set; }
            public string CCDB_Sampleparameter { get; set; }
            public string IPFERMparameter { get; set; }
            public string IPRECparameter { get; set; }
            public string MESflag { get; set; }
            public string MESparameter { get; set; }
            public string LIMSparameter { get; set; }
            public string TWflag { get; set; }
            public string TWparameter { get; set; }
            public string product { get; set; }
            public string vesselclass { get; set; }
            public string equipment { get; set; }
            public string station { get; set; }
            public string run { get; set; }
            public string lot { get; set; }
            public string modifier { get; set; }
            public string special { get; set; }
            public string limit { get; set; }
            public string limitvalue { get; set; }
            public string timeflag { get; set; }
            public string listflag { get; set; }
            public string duration { get; set; }
            public double durationseconds { get; set; }
            public string LIMSItemType { get; set; }
            public string LIMSTestCode { get; set; }
            public string TWRecordID { get; set; }
            public string step { get; set; }
        }
        public class cUser
        {
            public string id { get; set; }
            public string name { get; set; }
            public string unix { get; set; }
            public string phone { get; set; }
            public string area { get; set; }
        }

        //Support Functions
        public string GetProduct(string SearchString)
        {
            string strValue = null;
            string strProduct = null;
            strProduct = ValueExtractor(SearchString, "PRODUCT");
            if (!string.IsNullOrEmpty(strProduct)) { strValue = strProduct; }

            //If product keyword is not found, try the first regex search (four letters followed by four numbers)
            if (string.IsNullOrEmpty(strValue))
            {
                Match ProductMatch = Regex.Match(SearchString, @"\w{4}\d{4}");
                if (ProductMatch.Success) { strValue = SearchString.Substring(ProductMatch.Index, ProductMatch.Length); }

                //If product keyword is STILL not found, try the second regex search (four capital letters)
                if (string.IsNullOrEmpty(strValue))
                {
                    Match ProductMatch2 = Regex.Match(SearchString, @"([A-Z]){4}");
                    if (ProductMatch2.Success) { strValue = SearchString.Substring(ProductMatch2.Index, ProductMatch2.Length); }
                    if (strValue == "IVPC") { strValue = null; } //Inadvertent by-catch
                }
            }
            return strValue;
        }
        public string GetLot(string SearchString)
        {
            string strValue = null;
            string strLot = null;
            int n;

            strLot = ValueExtractor(SearchString, "LOT");
            if (!string.IsNullOrEmpty(strLot) && int.TryParse(strLot, out n) == true)
            {
                strValue = strLot.Trim();
            }
            else
            {
                string LotPattern = @"([0-9]{6,7}|w\d{5})"; //Regex Pattern for lot
                Match MatchLot = Regex.Match(SearchString, LotPattern);
                if (MatchLot.Success)
                {
                    strValue = SearchString.Substring(MatchLot.Index, MatchLot.Length).Trim();
                }
            }

            return strValue;
        }
        public string GetRun(string SearchString)
        {
            string strValue = null;
            string strRun = null;

            strRun = ValueExtractor(SearchString, "RUN");
            if (!string.IsNullOrEmpty(strRun))
            {
                strValue = strRun.Trim();
            }
            else
            {
                //string RunPattern = @"\b\d+\b"; //Regex Pattern for Run
                //Match MatchRun = Regex.Match(strRawMessage, RunPattern);
                //if (MatchRun.Success)
                //{                   
                //    if (MatchRun.Length > 3)
                //    {
                //        //Need to fix, currently does not iterate
                //        MatchRun.NextMatch();
                //    }
                //    else
                //    {
                //        Inputs.run = strRawMessage.Substring(MatchRun.Index, MatchRun.Length);
                //    }
                //}
            }

            return strValue;
        }
        public string GetStation(string SearchString)
        {
            string strValue = null;
            string strStation = null;
            strStation = ValueExtractor(SearchString, "STATION");
            if (!string.IsNullOrEmpty(strStation))
            {
                strValue = strStation.Trim();
            }
            else
            {
                string StationPattern = @"\d{4}(_)\d{2}"; //Regex Pattern for Station
                Match MatchStation = Regex.Match(SearchString, StationPattern);
                if (MatchStation.Success)
                {
                    strValue = SearchString.Substring(MatchStation.Index, MatchStation.Length).Trim();
                }
            }
            return strValue;
        }
        public string GetEquipment(string SearchString)
        {
            string strValue = null;

            //Remove dashes
            SearchString = SearchString.Replace("-", "");

            string strEquipment = null;
            strEquipment = ValueExtractor(SearchString, "TANK");
            if (!string.IsNullOrEmpty(strEquipment))
            {
                strValue = strEquipment.Trim();
            }
            else
            {
                string EquipmentPattern = @"([fFtTsSvVxXuU])\d{3,4}"; //Regex Pattern for equipment T270, S1472 etc
                Match MatchEquipment = Regex.Match(SearchString, EquipmentPattern);
                if (MatchEquipment.Success)
                {
                    strValue = SearchString.Substring(MatchEquipment.Index, MatchEquipment.Length).ToUpper().Trim();
                }
            }

            return strValue;
        }
        public string StringArraySearch(string BaseString, string[] SearchArray)
        {
            string strResult = null;
            string strFirstElement = BaseString.Split(' ').First();
            string strLastElement = BaseString.Split(' ').Last();

            foreach (string element in SearchArray)
            {
                if (BaseString.ToUpper().Contains(element))
                {
                    strResult = element;
                }
            }
            return strResult;
        }
        public string ValueExtractor(string BaseString, string TargetSubString)
        {
            string strValue = null;
            int intSubStringStart = -1;
            //Check if Substring even exists
            if (BaseString.ToUpper().Contains(TargetSubString) == true)
            {
                intSubStringStart = BaseString.ToUpper().IndexOf(TargetSubString.ToUpper(), 0);
                //For target substrings with multiple spaces, replace spaces with underscores
                string NewTargetSubString = TargetSubString.Replace(" ", "_");
                BaseString = BaseString.ToUpper().Replace(TargetSubString, NewTargetSubString);
            }

            if (intSubStringStart > -1)
            {
                //Okay, it exists, now get the index location of it plus the subsequent substrings
                int intSubStringEnd = BaseString.ToUpper().IndexOf(" ", intSubStringStart + 1);
                int intSubStringNext = BaseString.ToUpper().IndexOf(" ", intSubStringEnd + 1);

                //If string terminates before a value can be found, assume it's a search parameter and do not look for the value
                if (intSubStringEnd > -1)
                {
                    //If the substring search terminates (value is at the end of the message)
                    if (intSubStringNext == -1)
                    {
                        strValue = BaseString.Substring(intSubStringEnd, BaseString.Length - intSubStringEnd);
                    }
                    else
                    {
                        strValue = BaseString.Substring(intSubStringEnd, intSubStringNext - intSubStringEnd);
                    }
                }

            }
            return strValue;
        }
        public string NumberExtractor(string ConvertString)
        {
            string strValue = null;

            Regex regex = new Regex(@"\d+(\.\d+)?");
            Match match = regex.Match(ConvertString);
            if (match.Success)
            {
                strValue = ConvertString.Substring(match.Index, match.Length);
            }
            return strValue;
        }
        public double ConvertToSeconds(string strDuration)
        {
            double dblValue;
            dblValue = -1;
            var DurationUOM = new String(strDuration.Where(Char.IsLetter).ToArray());
            var DurationValue = NumberExtractor(strDuration);

            switch (DurationUOM.ToUpper())
            {
                case "SECOND":
                    dblValue = Convert.ToDouble(DurationValue);
                    break;
                case "SEC":
                    dblValue = Convert.ToDouble(DurationValue);
                    break;
                case "MINUTE":
                    dblValue = Convert.ToDouble(DurationValue) * 60;
                    break;
                case "MIN":
                    dblValue = Convert.ToDouble(DurationValue) * 60;
                    break;
                case "HOUR":
                    dblValue = Convert.ToDouble(DurationValue) * 3600;
                    break;
                case "HR":
                    dblValue = Convert.ToDouble(DurationValue) * 3600;
                    break;
                case "DAY":
                    dblValue = Convert.ToDouble(DurationValue) * 86400;
                    break;
                case "WEEK":
                    dblValue = Convert.ToDouble(DurationValue) * 604800;
                    break;
                default:
                    break;
            }

            return dblValue;
        }
    }
}
