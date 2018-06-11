using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TimelyAPI.Controllers;

namespace TimelyAPI.Tests
{
    [TestClass]
    public class TimelyAPITests
    {
        [TestMethod]
        public void CCDBBatchQueryListTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("Give me a list of currently in process 12kL tanks","test");
            //Assert
            Assert.IsTrue(strTestResult.ToUpper().Contains("FOLLOWING"));
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void CCDBBatchQueryProductByStationTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("What is the product on station 3410_06?", "test");
            //Assert
            Assert.IsTrue(strTestResult.ToUpper().Contains("PRODUCT"));
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void CCDBBatchQueryLotTest()
        { 
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("Can you give me the lot for Avastin run 160 12kL?","test");
            //Assert
            Assert.AreEqual("The LOT value for AVASTIN class 12KL run 160 is 3136230", strTestResult);
            Console.WriteLine(strTestResult);
        }
        
        [TestMethod]
        public void CCDBBatchQueryRunTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("Can you give me the run for lot 3136230","test");
            //Assert
            Assert.AreEqual("The RUN value for lot 3136230 is 160", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void CCDBBatchQueryEquipmentTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("Can you provide the equipment for 3136230","test");
            //Assert
            Assert.AreEqual("The EQUIPMENT value for lot 3136230 is T1219", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void CCDBSampleQueryCurrentTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("What's the current na value for Avastin run 160?", "test");
            //Assert
            Assert.AreEqual("The current  NA value for AVASTIN run 160 is 153 mmol/L", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void CCDBSampleQueryPreviousTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("What's the previous nh4 value for Avastin run 160?", "test");
            //Assert
            Assert.AreEqual("The previous NH4 value for AVASTIN run 160 is 2.19 mmol/L", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void CCDBSampleQueryMaxTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("What's the max VCD value for lot 3136230?", "test");
            //Assert
            Assert.AreEqual("The max VCD value for lot 3136230 is 126.90 10^5 cells/mL", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void CCDBSampleQueryMinTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("What's the min oxygen value for Avastin run 160?", "test");
            //Assert
            Assert.AreEqual("The min OXYGEN value for AVASTIN run 160 is 15.30 mmHg", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void CCDBSampleQueryAvgTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("What's the average co2 value for 3136230?", "test");
            //Assert
            Assert.AreEqual("The average CO2 value for lot 3136230 is 52.90 mmHg", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void CCDBSampleQueryInitialTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("What's the initial osmo value for Avastin run 160?", "test");
            //Assert
            Assert.AreEqual("The initial OSMO value for AVASTIN run 160 is 302 mOsm/kg", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void CCDBSampleQueryFinalTest()
         {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("What's the final IVPCV value for 3136230?", "test");
            //Assert
            Assert.AreEqual("The final IVPCV value for lot 3136230 is 318.33", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void CCDBSampleQueryAtDurationTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("What's the growth rate for 3136230 at 72 hours?", "test");
            //Assert
            Assert.AreEqual("The current GROWTH RATE value for lot 3136230 is 0.61 day-1", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void CCDBSampleQuerySampleTimeTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("When was the most recent sample for t7350?", "test");
            //Assert
            Assert.AreEqual("The current SAMPLE time for vessel T7350 was on 4/30/2018 10:23:00 AM", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void GlucosePredictionValueTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("Can you predict what the glucose value will be for Avastin Run 169 in 2 hours?", "test");
            //Assert
            double n;
            //Assert.IsTrue(double.TryParse(strTestResult, out n));
            //Assert.AreEqual("1", "1");
            Console.WriteLine(strTestResult);
        }
       
        [TestMethod]
        public void GlucosePredictionTimeTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("Can you predict when the glucose value for Avastin Run 169 will reach 1.0 g/l?", "test");
            //Assert
            DateTime n;
            Assert.IsTrue(DateTime.TryParse(strTestResult, out n));
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void ViabilityCrashTrueTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("Is there a viability crash for Avastin Run 164", "test");
            //Assert
            Assert.AreEqual("Uh oh...Viability crash detected! There was a viability drop of 34% over 44.37 hours", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void ViabilityCrashFalseTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("Is there a viability crash for Avastin Run 166", "test");
            //Assert
            Assert.AreEqual("No viability crash detected. Culture looks fine =)", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void IPFERMTempValueTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("What's the average temp for Avastin run 169 12kl?", "test");
            //Assert
            Assert.AreEqual("The average TEMP value for AVASTIN class 12KL run 169 is 33.73182 deg C", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void IPFERMTempValueTestAltOrder()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("average temperature for avastin 12kL run 169", "test");
            //Assert
            Assert.AreEqual("The average TEMP value for AVASTIN class 12KL run 169 is 33.73182 deg C", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void IPFERMValueTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("What's the max air sparge for Avastin run 167 12kL?", "test");
            //Assert
            Assert.AreEqual("The max AIR SPARGE value for AVASTIN class 12KL run 167 is 197.975 SLPM", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void IPFERMValueTestB3AMod()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("What's the max volume for T250?", "test");
            //Assert
            Assert.AreEqual("The max VOLUME value for vessel T250 is 2166.474 L", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void IPFERMValueOverDurationTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("What's the maximum o2 sparge for T271 over the past 24 hours?", "test");
            //Assert
            Assert.IsTrue(strTestResult.ToUpper().Contains("MAX O2 SPARGE"));
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void IPRECPhaseDescriptionTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("What's the current phase on X1454", "test");
            //Assert
            Assert.IsTrue(strTestResult.ToUpper().Contains("PHASE"));
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void MESMediaLotTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("What's the media lot for Avastin run 160 2kL?", "test");
            //Assert
            Assert.AreEqual("3135793", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void MESBFMediaLotTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("What's the batch feed lot for Avastin run 160 12kL?", "test");
            //Assert
            Assert.AreEqual("3136014", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void MESMediapHTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("What's the media pH for lot 3135794?", "test");
            //Assert
            Assert.AreEqual("7.03", strTestResult.Trim());
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void LIMSGeneralQueryTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("What's the assay result for lot 3136116 test code Q12398 sample FILTBFS?", "test");
            //Assert
            Assert.AreEqual("The ASSAY value for item type FILTBFS test code Q12398 lot 3136116 is 24.645587124 mg/mL", strTestResult);
            Console.WriteLine(strTestResult);
        }
        
        [TestMethod]
        public void LIMSTiterByRunQueryTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("What's the preharv titer for Avastin run 160?", "test");
            //Assert
            Assert.AreEqual("The PHCCF titer for AVASTIN class 12KL run 160 is 1.005 mg/mL", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void SentryCreateJob()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("Sentry, create an alert for when online pH is above 7.04 for T320", "test");
            //Assert
            Assert.AreEqual("A Sentry alert for when ONLINE PH on T320 is GREATER than 7.04 was created for you. This alert will auto expire in 20 days if not triggered", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void SentryCreateJobPhaseChange()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("Sentry, turn on full alerts for X1360 phases", "test");
            //Assert
            Assert.AreEqual("Sentry will notify you when PHASE on X1360 is changed/updated. This monitoring will be active for the next 20 days. To disable, reply with disable, deactivate, or turn off", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void SentrySnoozeAlert()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("Sentry, snooze alerts for online pH on T320 for 3 hrs", "test");
            //Assert
            Assert.AreEqual("Alerts for ONLINE PH on T320 will be snoozed for the next 3 hours", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void AmbiguityPHTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("Current pH on T320?", "test");
            //Assert
            Assert.AreEqual("I understand you're requesting pH data. However, can you try re-phrasing your request and specifying whether you'd like offline, online or media pH?", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void AmbiguityPHWithSessionTest()
        {
            //Arrange
            SMSController test = new SMSController();
            var session = new Dictionary<string, Object>();
            session["jokeID"] = null;
            session["chatStatus"] = null;
            session["prevMessage"] = null;
            //Act
            string strTestResult = test.ProcessMessage1("Current pH on T320?", "test", ref session);
            string strTestResult2 = test.ProcessMessage1("Media", "test", ref session);
            //Assert
            Assert.AreEqual("I understand you're requesting pH data. However, can you try re-phrasing your request and specifying whether you'd like offline, online or media pH?", strTestResult);
            Assert.AreEqual("6.206", strTestResult2);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void AmbiguityIdentifierTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("What is do2 in 250?", "test");
            //Assert
            Assert.AreEqual("I can't seem to find any valid batch identifiers in your request (i.e, product, lot, run, equipment). Can you try re-phrasing your request with at least one identifier?", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void AmbiguityIdentifierWithSessionTest()
        {
            //Arrange
            SMSController test = new SMSController();
            var session = new Dictionary<string, Object>();
            session["jokeID"] = null;
            session["chatStatus"] = null;
            session["prevMessage"] = null;
            //Act
            string strTestResult = test.ProcessMessage1("What is do2 in 250?", "test", ref session);
            string strTestResult2 = test.ProcessMessage1("T250", "test", ref session);
            string strTestResult3 = test.ProcessMessage1("Online", "test", ref session);
            //Assert
            Assert.AreEqual("I can't seem to find any valid batch identifiers in your request (i.e, product, lot, run, equipment). Can you try re-phrasing your request with at least one identifier?", strTestResult);
            Assert.AreEqual("I understand you're requesting dO2 data. However, can you try re-phrasing your request and specifying whether you'd like offline or online dO2?", strTestResult2);
            Assert.IsTrue(Regex.Match(strTestResult3, @"The current ONLINE DO2 value for vessel T250 is \d+.\d+ %Sat").Success);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void AmbiguityTiterTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("What is the titer for Avastin?", "test");
            //Assert
            Assert.AreEqual("I understand you're requesting titer data. However, can you try re-phrasing your request and specifying whether which titer result you'd like? (i.e. preharv or harvest)", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void AmbiguityTiterWithSessionTest()
        {
            //Arrange
            SMSController test = new SMSController();
            var session = new Dictionary<string, Object>();
            session["jokeID"] = null;
            session["chatStatus"] = null;
            session["prevMessage"] = null;
            //Act
            string strTestResult = test.ProcessMessage1("What is the titer for Avastin?", "test", ref session);
            string strTestResult2 = test.ProcessMessage1("Harvest", "test", ref session);
            //Assert
            Assert.AreEqual("I understand you're requesting titer data. However, can you try re-phrasing your request and specifying whether which titer result you'd like? (i.e. preharv or harvest)", strTestResult);
            Assert.IsTrue(Regex.Match(strTestResult2, @"The CLARCC titer for AVASTIN class 12KL is \d+.\d+ mg/mL").Success);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void VersionTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.CannedResponse("What's the current version?", "test");
            //Assert
            Assert.IsTrue(strTestResult.ToUpper().Contains("VERSION"));
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void DebugTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.CannedResponse("Hello Timely", "Jason");
            //Assert
            //Assert.AreEqual("I can see that you're requesting dO2 data. However, can you try re-phrasing your request and specifying whether you'd like offline or online dO2?", strTestResult);
            //Assert.AreEqual("1", "1");
            Assert.AreEqual("Hi Jason! It's good to hear from you, feel free to ask me what I can do =)", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void CannedResponseTestOriginal()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.CannedResponse("Tell me a joke", "Julia");
            //Assert
            //Assert.AreEqual("Shhhh Julia. Please don't mention that name, he might just pull the plug on everything!", strTestResult);
            Assert.AreEqual("Hi Julia. My joke generating module is still in the shop =/", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void CannedResponseTestFaster()
        {
            //Arrange
            SMSController test = new SMSController();
            var session = new Dictionary<string, Object>();
            session["jokeID"] = null;
            session["chatStatus"] = null;
            //Act
            string strTestResult = test.CannedResponse1("Tell me a joke", "Julia", ref session);
            //Assert
            //Assert.AreEqual("Shhhh Julia. Please don't mention that name, he might just pull the plug on everything!", strTestResult);
            Assert.AreEqual("Hi Julia. My joke generating module is still in the shop =/", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void KnockKnockJokeTest()
        {
            // Arrange
            SMSController test = new SMSController();
            var session = new Dictionary<string, Object>();
            session["jokeID"] = 10;
            session["chatStatus"] = 0;
            // Act
            string actual = test.CannedResponse1("Tell me a knock-knock joke.", "Julia", ref session);
            session["jokeID"] = 10;
            string actual1 = test.CannedResponse1("Who's there?", "Julia", ref session);
            string actual2 = test.CannedResponse1("Spell who?", "Julia", ref session);
            // Assert
            Assert.AreEqual("Knock knock.", actual);
            Assert.AreEqual("Spell.", actual1);
            Assert.AreEqual("W-H-O", actual2);
        }

        // User should be able to ask for a new joke at any point in the conversation
        // Test may fail if Random happens to generate the same random number twice in a row
        [TestMethod]
        public void KnockKnockJokeStartNewTest()
        {
            // Arrange
            SMSController test = new SMSController();
            var session = new Dictionary<string, Object>();
            session["jokeID"] = 10;
            session["chatStatus"] = 0;
            // Act
            string actual = test.CannedResponse1("Tell me a knock-knock joke.", "Julia", ref session);
            session["jokeID"] = 10;
            string actual1 = test.CannedResponse1("Who's there?", "Julia", ref session);
            string actual2 = test.CannedResponse1("Tell me a knock-knock joke.", "Julia", ref session);
            string actual3 = test.CannedResponse1("Who's there?", "Julia", ref session);
            // Assert
            Assert.AreEqual("Knock knock.", actual);
            Assert.AreEqual("Spell.", actual1);
            Assert.AreEqual("Knock knock.", actual2);
            Assert.AreNotEqual("Spell.", actual3);
        }

        // User should be able to ask for information on real things in the middle of joke
        [TestMethod]
        public void KnockKnockJokeSwitchToPHTest()
        {
            // Arrange
            SMSController test = new SMSController();
            var session = new Dictionary<string, Object>();
            session["jokeID"] = 10;
            session["chatStatus"] = 0;
            session["prevMessage"] = null;
            // Act
            string actual = test.CannedResponse1("Tell me a knock-knock joke.", "Julia", ref session);
            session["jokeID"] = 10;
            string actual1 = test.CannedResponse1("Who's there?", "Julia", ref session);
            //TODO: move more code out of Index()
            // for example, the code to decide whether to call CannedResponse() or ProcessMessage()
            string actual2 = test.ProcessMessage1("Do you know the current media pH on T320?", "Julia", ref session);
            // Assert
            Assert.AreEqual("Knock knock.", actual);
            Assert.AreEqual("Spell.", actual1);
            Assert.IsTrue(Regex.Match(actual2, @"\d+.\d+").Success);
        }

        [TestMethod]
        public void TWRecordAssignedtoMe()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("What records are assigned to me?", "yucheng");
            //Assert
            Assert.IsTrue(strTestResult.ToUpper().Contains("OPEN RECORDS"));
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void TWNoRecordsAssigned()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("What records are assigned to me?", "test");
            //Assert
            Assert.AreEqual("Hooray, you currently have no active records assigned to you! Keep crushing it", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void TWRecordStatusTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("What is the status of record 1251736?", "test");
            //Assert
            Assert.AreEqual("The STATUS of the record 1251736 is CLOSED", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void TWRecordParentTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("What's the parent for record 1402851", "test");
            //Assert
            Assert.AreEqual("The PARENT of the record 1402851 is 1401249", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void TWRecordUpdateTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("When was record 1156462 last updated?", "test");
            //Assert
            Assert.AreEqual("The record 1156462 was updated on 3/9/2016 12:04:47 PM", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void TWRecordOpenTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("When was record 1156462 opened?", "test");
            //Assert
            Assert.AreEqual("The record 1156462 was opened on 2/24/2016 11:43:22 AM", strTestResult);
            Console.WriteLine(strTestResult);
        }
        [TestMethod]
        public void TWRecordCloseTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("When was record 1156462 closed?", "test");
            //Assert
            Assert.AreEqual("The record 1156462 was closed on 3/9/2016 11:28:00 AM", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void TWRecordDueTest()
        {
            //Arrange
            SMSController test = new SMSController();
            //Act
            string strTestResult = test.ProcessMessage("When was DMS 1402851 due?", "test");
            //Assert
            Assert.AreEqual("The record 1402851 was DUE on 11/23/2017 12:00:00 AM", strTestResult);
            Console.WriteLine(strTestResult);
        }

        //When is the next pH/Temp shift for run xxx
    }
}
