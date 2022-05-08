using System.IO;
using System.Reflection;
using LFS.Data.API.AeroAPI;

namespace DataTests
{
    internal class AeroAPIFlightsTestDataProvidor : ITestDataProvidor
    {
        public Stream Response()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string directoryPath = Path.GetDirectoryName(assemblyPath);
            string dataFilePath = Path.Combine(new string[] { directoryPath, "DataFiles", "AeroAPI", "flights_jbu906.json" });
            if (!File.Exists(dataFilePath))
            {
                throw new FileNotFoundException("Unable to find file", dataFilePath);
            }
            return File.OpenText(dataFilePath).BaseStream;
        }
    }
}
