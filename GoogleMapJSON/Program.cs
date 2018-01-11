using System;
using System.IO;
using CsvParsing;
using System.Linq;
using System.Net;
using System.Web;
using Newtonsoft.Json.Linq;

//Tools, package manager, console: update-package -reinstall
//make sure you have http://fldsvrtfs01:81/nuget/Default as a nuget source
 
namespace GoogleMapJSON
{
    class Program
    {
        static void Main(string[] args)
        {

            JObject parsedObject;
             
            var key = @"AIzaSyC_q00OxyZuj1901ufeJ-Bmx6fhtdVq6PA"; // use your API key
            using (var target = new StreamWriter(@"\\fldsvm_apps01\SQLSHARED\Deployments\20180105\CS\WriteLines.csv"))

            using (var csvInputStream = File.OpenRead(@"\\fldsvm_apps01\SQLSHARED\Deployments\20180105\CS\StartPoint.csv"))
            {
                foreach (var location in CsvParser.GetRecordsFromInputStream(csvInputStream).Select(record => new
                {
                    Latitude = record[0],
                    Longitude = record[1]
                }))
                {
                    var url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={location.Latitude},{location.Longitude}&key={HttpUtility.UrlEncode(key)}";

                    var request = WebRequest.Create(url);

                    using (var response = request.GetResponse())
                    using (var responseStream = response.GetResponseStream())
                    using (var streamReader = new StreamReader(responseStream))
                    {
                        parsedObject = JObject.Parse(streamReader.ReadToEnd());
                    }

                    var status = parsedObject["status"].ToString();                   

                    if (status == "OK")
                    {
                        var firstMatch = parsedObject["results"].First;

                        if (firstMatch != null)
                        {

                            var formatted_address = firstMatch["formatted_address"];

                            //var county = firstMatch["address_components"]
                            //    .AsJEnumerable()
                            var county = parsedObject["results"]
                                .SelectMany(t => t["address_components"])
                                .Where(t => t["types"].Any(l => l.ToString() == "administrative_area_level_2") && t["long_name"] != null)
                                .Select(t => t["long_name"].ToString())
                                .FirstOrDefault();

                            //Console.Write($"status: \"{status}\"");
                            //Console.Write($", match types: \"{String.Join(", ", firstMatch["types"])}\"");
                            //Console.Write($", formatted_address: \"{formatted_address}\"");
                            //Console.WriteLine($", county: \"{county}\"");
                            target.WriteLine("\"" + location.Latitude + "\",\"" + location.Longitude +"\"," + $"\"{formatted_address}\"" + "," + $"\"{county}\"" + "," + $"\"{String.Join(", ", firstMatch["types"])}\"");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"status: {status}");
                    }
                }
            }
        }
    }
}
