using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TimelyAPI.Controllers;

namespace TimelyAPI.Tests
{
    [TestClass]
    public class TimelyAPITests
    {
        #region Test CCDB

        [TestMethod]
        public void CCDBBatchQueryListTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("Give me a list of currently in process 12kL tanks","test", ref session);
            //Assert
            Assert.IsTrue(strTestResult.ToUpper().Contains("FOLLOWING"));
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void CCDBBatchQueryProductByStationTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What is the product on station 3410_06?", "test", ref session);
            //Assert
            Assert.IsTrue(strTestResult.ToUpper().Contains("PRODUCT"));
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void CCDBBatchQueryLotTest()
        { 
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("Can you give me the lot for Avastin run 160 12kL?","test", ref session);
            //Assert
            Assert.AreEqual("The LOT value for AVASTIN class 12KL run 160 is 3136230", strTestResult);
            Console.WriteLine(strTestResult);
        }
        
        [TestMethod]
        public void CCDBBatchQueryRunTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("Can you give me the run for lot 3136230","test", ref session);
            //Assert
            Assert.AreEqual("The RUN value for lot 3136230 is 160", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void CCDBBatchQueryEquipmentTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("Can you provide the equipment for 3136230","test", ref session);
            //Assert
            Assert.AreEqual("The EQUIPMENT value for lot 3136230 is T1219", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void CCDBSampleQueryCurrentTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What's the current na value for Avastin run 160?", "test", ref session);
            //Assert
            //Assert.AreEqual("The current  NA value for AVASTIN run 160 is 153 mmol/L", strTestResult);
            Assert.AreEqual("The current NA value for AVASTIN run 160 is 153 mmol/L", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void CCDBSampleQueryPreviousTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What's the previous nh4 value for Avastin run 160?", "test", ref session);
            //Assert
            Assert.AreEqual("The previous NH4 value for AVASTIN run 160 is 2.19 mmol/L", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void CCDBSampleQueryMaxTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What's the max VCD value for lot 3136230?", "test", ref session);
            //Assert
            Assert.AreEqual("The max VCD value for lot 3136230 is 126.90 10^5 cells/mL", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void CCDBSampleQueryMinTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What's the min oxygen value for Avastin run 160?", "test", ref session);
            //Assert
            Assert.AreEqual("The min OXYGEN value for AVASTIN run 160 is 15.30 mmHg", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void CCDBSampleQueryAvgTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What's the average co2 value for 3136230?", "test", ref session); // for CCDB Sample should be pCO2. CO2 is the abbreviation for CO2 flow in IP-FERM
            //Assert
            Assert.AreEqual("The average CO2 value for lot 3136230 is 52.90 mmHg", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void CCDBSampleQueryInitialTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What's the initial osmo value for Avastin run 160?", "test", ref session);
            //Assert
            Assert.AreEqual("The initial OSMO value for AVASTIN run 160 is 302 mOsm/kg", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void CCDBSampleQueryFinalTest()
         {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What's the final IVPCV value for 3136230?", "test", ref session);
            //Assert
            Assert.AreEqual("The final IVPCV value for lot 3136230 is 318.33", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void CCDBSampleQueryAtDurationTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What's the growth rate for 3136230 at 72 hours?", "test", ref session); // 'growth' in MSAT_PARAM.
            //Assert
            Assert.AreEqual("The current GROWTH RATE value for lot 3136230 is 0.61 day-1", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void CCDBSampleQuerySampleTimeTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("When was the most recent sample for lot 3254207?", "test", ref session); //T7350
            //Assert
            Assert.AreEqual("The current SAMPLE time for lot 3254207 was on 5/18/2018 9:03:00 PM", strTestResult);
            Console.WriteLine(strTestResult);
        }

        #endregion
        #region Test CCDB Predictions
        /* 
         * NOTE: Test glucose prediction calculations as if today was 7/20/2018 8:00:00 AM
         * Actual glucose values: 3.05 g/L on 7/20/2018 6:07:00 AM, 1.09 g/L on 7/21/2018 12:21:00 AM
         */
        [TestMethod]
        public void GlucosePredictionValueTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("Can you predict what the glucose value will be for lot 3262626 in 2 hours?", "test", ref session);
            //Assert
            double n;
            Assert.IsTrue(double.TryParse(strTestResult, out n));
            Console.WriteLine(strTestResult);
        }
       
        [TestMethod]
        public void GlucosePredictionTimeTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("Can you predict when the glucose value for lot 3262626 will reach 1.0 g/l?", "test", ref session);
            //Assert
            //DateTime n;
            //Assert.IsTrue(DateTime.TryParse(strTestResult, out n));
            Assert.AreEqual("According to my calculations, the glucose value for this batch will drop below 1.0 g/L on 7/20/2018 4:04:54 PM", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void PCVPredictionTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("Can you predict what the PCV value will be for lot 3262626 in 2 hours?", "test", ref session);
            //Assert
            double n;
            Assert.IsTrue(double.TryParse(strTestResult, out n));
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void ViabilityCrashTrueTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("Is there a viability crash for Avastin Run 164", "test", ref session);
            //Assert
            Assert.AreEqual("Uh oh...Viability crash detected! There was a viability drop of 34% over 44.37 hours", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void ViabilityCrashFalseTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("Is there a viability crash for Avastin Run 166", "test", ref session);
            //Assert
            Assert.AreEqual("No viability crash detected. Culture looks fine =)", strTestResult);
            Console.WriteLine(strTestResult);
        }

        #endregion
        #region Test IP21

        [TestMethod]
        public void IPFERMTempValueTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What's the average temp for Avastin run 169 12kl?", "test", ref session);
            //Assert
            Assert.AreEqual("The average TEMP value for AVASTIN class 12KL run 169 is 33.73182 deg C", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void IPFERMTempValueTestAltOrder()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("average temperature for avastin 12kL run 169", "test", ref session);
            //Assert
            Assert.AreEqual("The average TEMP value for AVASTIN class 12KL run 169 is 33.73182 deg C", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void IPFERMTempValueSeedTrainTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("Temp for lot 3265507", "test", ref session);
            //Assert
            Assert.AreEqual("The current TEMP value for lot 3265507 is 37.166 deg C", strTestResult);
        }

        [TestMethod]
        public void IPFERMValueTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What's the max air sparge for Avastin run 167 12kL?", "test", ref session);
            //Assert
            Assert.AreEqual("The max AIR SPARGE value for AVASTIN class 12KL run 167 is 197.975 SLPM", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void IPFERMValueTestB3AMod()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What's the max volume for lot 3255913?", "test", ref session); //T250
            //Assert
            Assert.AreEqual("The max VOLUME value for lot 3255913 is 2209.342 L", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void IPFERMValueOverDurationTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What's the maximum o2 sparge for T271 over the past 24 hours?", "test", ref session);
            //Assert
            Assert.IsTrue(strTestResult.ToUpper().Contains("MAXIMUM O2 SPARGE"));
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void IPRECPhaseDescriptionTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What's the current phase on X1454", "test", ref session);
            //Assert
            Assert.IsTrue(strTestResult.ToUpper().Contains("PHASE"));
            Console.WriteLine(strTestResult);
        }

        #endregion
        #region Test MES

        [TestMethod]
        public void MESMediaLotTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What's the media lot for Avastin run 160 2kL?", "test", ref session);
            //Assert
            Assert.AreEqual("The MEDIA LOT for AVASTIN run 160 2KL is 3135793", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void MESBFMediaLotTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What's the batch feed lot for Avastin run 160 12kL?", "test", ref session);
            //Assert
            Assert.AreEqual("The BATCH FEED LOT for AVASTIN run 160 12KL is 3136014", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void MESMediapHTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What's the media pH for lot 3135794?", "test", ref session);
            //Assert
            Assert.AreEqual("The MEDIA PH for lot 3135794 is 7.03", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void MESBufferLotTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What's the buffer lot for Avastin?", "test", ref session);
            //Assert
            Assert.AreEqual("The BUFFER LOT for AVASTIN is 3212759", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void MESBufferPHTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What's the buffer pH for lot 3237133?", "test", ref session);//station 3410_01
            //Assert
            Assert.AreEqual("The BUFFER PH for lot 3237133 is 7.1", strTestResult);
            Console.WriteLine(strTestResult);
        }

        #endregion
        #region Test LIMS

        [TestMethod]
        public void LIMSGeneralQueryTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What's the assay result for lot 3136116 test code Q12398 sample FILTBFS?", "test", ref session);
            //Assert
            Assert.AreEqual("The ASSAY value for item type FILTBFS test code Q12398 lot 3136116 is 24.645587124 mg/mL", strTestResult);
            Console.WriteLine(strTestResult);
        }
        
        [TestMethod]
        public void LIMSTiterByRunQueryTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What's the preharv titer for Avastin run 160?", "test", ref session);
            //Assert
            Assert.AreEqual("The PHCCF titer for AVASTIN class 12KL run 160 is 1.005 mg/mL", strTestResult);
            Console.WriteLine(strTestResult);
        }

        #endregion
        #region Test Sentry

        [TestMethod]
        public void SentryActionLimitTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What's the lower action limit for online pH for anti-Myostatin 2kL?", "test", ref session);
            //Assert
            Assert.AreEqual("The lower action limit for ONLINE PH for ANTI-MYOSTATIN 2KL is pH 6.8", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void SentryActionLimitWithAliasTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What's the upper action limit for temperature for Pola 400L?", "test", ref session);
            //Assert
            Assert.AreEqual("The upper action limit for TEMPERATURE for POLA 400L is 38 Deg C", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void SentryCreateJob()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("Sentry, create an alert for when online pH is above 7.04 for T320", "test", ref session);
            //Assert
            Assert.AreEqual("A Sentry alert for when ONLINE PH on T320 is GREATER than 7.04 was created for you. This alert will auto expire in 20 days if not triggered", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void SentryCreateJobPhaseChange()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("Sentry, turn on full alerts for X1360 phases", "test", ref session);
            //Assert
            Assert.AreEqual("Sentry will notify you when PHASE on X1360 is changed/updated. This monitoring will be active for the next 20 days. To disable, reply with disable, deactivate, or turn off", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void SentryDisableJobPhaseChange()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("Sentry, turn off alerts for X1360 phases", "test", ref session);
            //Assert
            Assert.AreEqual("Alerts disabled for PHASE on X1360", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void SentrySnoozeAlert()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("Sentry, snooze alerts for online pH on T320 for 3 hrs", "test", ref session);
            //Assert
            Assert.AreEqual("Alerts for ONLINE PH on T320 will be snoozed for the next 3 hours", strTestResult);
            Console.WriteLine(strTestResult);
        }

        #endregion
        #region Test Ambiguity and Sessions

        [TestMethod]
        public void AmbiguityPHTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("Current pH on T320?", "test", ref session);
            //Assert
            Assert.AreEqual("I understand you're requesting pH data. However, can you try re-phrasing your request and specifying whether you'd like offline, online or media pH?", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void AmbiguityPHWithSessionTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("Current pH on T320?", "test", ref session);
            string strTestResult2 = test.ProcessMessage("Media", "test", ref session);
            //Assert
            Assert.AreEqual("I understand you're requesting pH data. However, can you try re-phrasing your request and specifying whether you'd like offline, online or media pH?", strTestResult);
            Assert.AreEqual("The MEDIA PH for T320 is 6.206", strTestResult2);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void AmbiguityIdentifierTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What is do2 in 250?", "test", ref session);
            //Assert
            Assert.AreEqual("I can't seem to find any valid batch identifiers in your request (i.e, product, lot, run, equipment). Can you try re-phrasing your request with at least one identifier?", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void AmbiguityIdentifierWithSessionTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What is do2 in 250?", "test", ref session); // Need to remove dO2 as an alias!
            string strTestResult2 = test.ProcessMessage("T250", "test", ref session);
            string strTestResult3 = test.ProcessMessage("Online", "test", ref session);
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
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What is the titer for Avastin?", "test", ref session);
            //Assert
            Assert.AreEqual("I understand you're requesting titer data. However, can you try re-phrasing your request and specifying which titer result you'd like? (i.e. preharv or harvest)", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void AmbiguityTiterWithSessionTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What is the titer for Avastin?", "test", ref session);
            string strTestResult2 = test.ProcessMessage("Harvest", "test", ref session);
            //Assert
            Assert.AreEqual("I understand you're requesting titer data. However, can you try re-phrasing your request and specifying which titer result you'd like? (i.e. preharv or harvest)", strTestResult);
            Assert.IsTrue(Regex.Match(strTestResult2, @"The CLARCC titer for AVASTIN class 12KL is \d+.\d+ mg/mL").Success);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void AmbiguityActionLimitTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What is the upper action limit for temperature for Avastin?", "test", ref session);
            //Assert
            Assert.AreEqual("I understand you're asking about upper action limit. However, I'm missing either the PRODUCT, VESSEL SIZE, or TARGET PARAMETER (i.e. Temperature) from the information you provided", strTestResult);
            Console.WriteLine(strTestResult);
        }

        #endregion
        #region Test Missing Identifier

        [TestMethod]
        public void UnknownEquipmentTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What is do2 in Avastin T123?", "test", ref session);
            //Assert
            Assert.AreEqual("I can't seem to find the equipment you've specified, try again with the following format: X### (i.e. T281)", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void UnknownStationTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What is do2 in Avastin 1234_56?", "test", ref session);
            //Assert
            Assert.AreEqual("I can't seem to find the station you've specified, try again with the following format: ####_## (i.e. 3410_08)", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void MissingParameterTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What's in T250?", "test", ref session);
            //Assert
            Assert.AreEqual("I can't seem to figure out what target parameter you're looking for. Can you try re-phrasing your request with a valid search parameter? (i.e. PCV, temperature, titer)", strTestResult);
            Console.WriteLine(strTestResult);
        }

        #endregion
        #region Test Other

        [TestMethod]
        public void VersionTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.CannedResponse("What's the current version?", "test", ref session);
            //Assert
            Assert.IsTrue(strTestResult.ToUpper().Contains("VERSION"));
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void DebugTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("Temperature for lot 3265507", "Jason", ref session);
            //Assert
            //Assert.AreEqual("I can see that you're requesting dO2 data. However, can you try re-phrasing your request and specifying whether you'd like offline or online dO2?", strTestResult);
            //Assert.AreEqual("1", "1");
            Assert.AreEqual("Hi Jason! It's good to hear from you, feel free to ask me what I can do =)", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void CannedResponseTestFaster()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.CannedResponse("Tell me a joke", "Julia", ref session);
            //Assert
            Assert.AreEqual("Hi Julia. My joke generating module is still in the shop =/", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void KnockKnockJokeTest()
        {
            // Arrange
            Arrange();
            // Act
            string actual = test.CannedResponse("Tell me a knock knock joke.", "Julia", ref session);
            session["jokeID"] = 10;
            string actual1 = test.CannedResponse("Who's there?", "Julia", ref session);
            string actual2 = test.CannedResponse("Spell who?", "Julia", ref session);
            // Assert
            Assert.AreEqual("Knock knock.", actual);
            Assert.AreEqual("Spell.", actual1);
            Assert.AreEqual("W-H-O", actual2);
        }

        [TestMethod]
        public void KnockKnockJokeEmojiTest()
        {
            // Arrange
            Arrange();
            // Act
            string actual = test.CannedResponse("Tell me a knock-knock joke.", "Julia", ref session);
            session["jokeID"] = 8;
            string actual1 = test.CannedResponse("Who's there?", "Julia", ref session);
            string actual2 = test.CannedResponse("No one who?", "Julia", ref session);
            // Assert
            Assert.AreEqual("Knock knock.", actual);
            Assert.AreEqual("No one.", actual1);
            Assert.AreEqual("🤐", actual2);
        }

        [TestMethod]
        public void KnockKnockJokeWrongInputTest()
        {
            // Arrange
            Arrange();
            // Act
            string actual = test.CannedResponse("Tell me a knock knock joke.", "Julia", ref session);
            session["jokeID"] = 10;
            string actual1 = test.CannedResponse("Who's there?", "Julia", ref session);
            string actual2 = test.CannedResponse("Neque porro quisquam est qui dolorem ipsum quia dolor sit amet", "Julia", ref session);

            // TODO: move code outside of Index()
            if (actual2 == null) { actual2 = test.ProcessMessage("Neque porro quisquam est qui dolorem ipsum quia dolor sit amet", "Julia", ref session); }

            // Assert
            Assert.AreEqual("Knock knock.", actual);
            Assert.AreEqual("Spell.", actual1);
            Assert.AreEqual("I can't seem to find any valid batch identifiers in your request (i.e, product, lot, run, equipment). Can you try re-phrasing your request with at least one identifier?", actual2);
        }

        // User should be able to ask for a new joke at any point in the conversation
        // NOTE: Test may fail if Random happens to generate the same random number twice in a row
        [TestMethod]
        public void KnockKnockJokeStartNewTest()
        {
            // Arrange
            Arrange();
            // Act
            string actual = test.CannedResponse("Tell me a knock-knock joke.", "Julia", ref session);
            session["jokeID"] = 10;
            string actual1 = test.CannedResponse("Who's there?", "Julia", ref session);
            string actual2 = test.CannedResponse("Tell me a knock-knock joke.", "Julia", ref session);
            string actual3 = test.CannedResponse("Who's there?", "Julia", ref session);
            // Assert
            Assert.AreEqual("Knock knock.", actual);
            Assert.AreEqual("Spell.", actual1);
            Assert.AreEqual("Knock knock.", actual2);
            Assert.AreNotEqual("Spell.", actual3);
        }

        // User should be able to ask for information on MSAT data in the middle of joke
        [TestMethod]
        public void KnockKnockJokeSwitchToMSATTest()
        {
            // Arrange
            Arrange();
            // Act
            string actual = test.CannedResponse("Tell me a knock-knock joke.", "Julia", ref session);
            session["jokeID"] = 10;
            string actual1 = test.CannedResponse("Who's there?", "Julia", ref session);
            //TODO: move more code out of Index()
            // for example, the code to decide whether to call CannedResponse() or ProcessMessage()
            string actual2 = test.ProcessMessage("Do you know the current media pH on T320?", "test", ref session);
            // Assert
            Assert.AreEqual("Knock knock.", actual);
            Assert.AreEqual("Spell.", actual1);
            Assert.IsTrue(Regex.Match(actual2, @"\d+.\d+").Success);
        }

        #endregion
        #region Test Trackwise

        [TestMethod]
        public void TWRecordAssignedtoMe()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What records are assigned to me?", "yucheng", ref session);
            //Assert
            Assert.IsTrue(strTestResult.ToUpper().Contains("OPEN RECORDS"));
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void TWNoRecordsAssigned()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What records are assigned to me?", "test", ref session);
            //Assert
            Assert.AreEqual("Hooray, you currently have no active records assigned to you! Keep crushing it", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void TWRecordStatusTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What is the status of record 1251736?", "test", ref session);
            //Assert
            Assert.AreEqual("The STATUS of the record 1251736 is CLOSED", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void TWRecordParentTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("What's the parent for record 1402851", "test", ref session);
            //Assert
            Assert.AreEqual("The PARENT of the record 1402851 is 1401249", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void TWRecordUpdateTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("When was record 1156462 last updated?", "test", ref session);
            //Assert
            Assert.AreEqual("The record 1156462 was updated on 3/9/2016 12:04:47 PM", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void TWRecordOpenTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("When was record 1156462 opened?", "test", ref session);
            //Assert
            Assert.AreEqual("The record 1156462 was opened on 2/24/2016 11:43:22 AM", strTestResult);
            Console.WriteLine(strTestResult);
        }
        [TestMethod]
        public void TWRecordCloseTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("When was record 1156462 closed?", "test", ref session);
            //Assert
            Assert.AreEqual("The record 1156462 was closed on 3/9/2016 11:28:00 AM", strTestResult);
            Console.WriteLine(strTestResult);
        }

        [TestMethod]
        public void TWRecordDueTest()
        {
            //Arrange
            Arrange();
            //Act
            string strTestResult = test.ProcessMessage("When was DMS 1402851 due?", "test", ref session);
            //Assert
            Assert.AreEqual("The record 1402851 was DUE on 11/23/2017 12:00:00 AM", strTestResult);
            Console.WriteLine(strTestResult);
        }

        #endregion

        //When is the next pH/Temp shift for run xxx

        private Dictionary<string, Object> session;
        private SMSController test;
        private void Arrange()
        {
            test = new SMSController();

            session = new Dictionary<string, Object>();
            session["jokeID"] = null;
            session["chatStatus"] = 0;
            session["prevMessage"] = null;
        }
    }
}
