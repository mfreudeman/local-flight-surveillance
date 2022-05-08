using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using LFS.Data.API.AeroAPI;
using LFS.Data.API.AeroAPI.Model;
using LFS.Data.API.AeroAPI.Helper;

namespace DataTests
{
    [TestClass]
    public class AeroAPITests
    {
        [TestMethod]
        public void TestFlightsByFlightIdentifier()
        {
            AeroAPI.TestDataProvidor = new AeroAPIFlightsTestDataProvidor();
            
            var response = AeroAPI.FlightsByFlightIdentifier("apikeyvalue", "JBU906");
            response.Wait();
            var result = response.Result;

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Results);
            Assert.IsTrue(result.Results.Count > 0);

            Flight inProgressFlight = FlightsHelper.FirstInProgress(result.Results);

            Assert.IsNotNull(inProgressFlight);
        }
    }
}
