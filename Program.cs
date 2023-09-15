using System;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Text;

namespace KeplerWeatherApp
{
    class Program
    {
        private const string ApiKey = "071852ecfb772b4a019ea54003aecbc5";
        private static readonly string ApiUrl = "http://api.weatherstack.com/current?access_key=";


        static async System.Threading.Tasks.Task Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Console.WriteLine("Please enter a city");
            string cityName = Console.ReadLine();
            try
            {
                string weatherResult = await FetchWeather(cityName);
                if(weatherResult == null)
                {
                    Console.WriteLine("Not weather data found, check city name");
                    return;
                }
                GenerateWeatherReport(weatherResult, cityName);
            } catch (Exception err)
            {
                Console.WriteLine(err);
            }
        }

        static async System.Threading.Tasks.Task<string> FetchWeather(string cityName)
        {
            string weatherResponse;
            using (var httpClient = new HttpClient())
            {
                weatherResponse = await httpClient.GetStringAsync(ApiUrl + ApiKey + "&query=" + cityName);
                return weatherResponse;
            }
        }

        static void GenerateWeatherReport(string weatherResult, string cityName)
        {
            Console.WriteLine("Generating PDF");
            JObject data = JObject.Parse(weatherResult);
            if(data.ContainsKey("error"))
            {
                 Console.WriteLine(data["error"]);
                return;
            }

            PdfDocument report = new PdfDocument();
            PdfPage page = report.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont font = new XFont("Times New Roman", 18, XFontStyle.Regular);
            gfx.DrawString($"Weather Report for {cityName}", font, XBrushes.Black,
                new XRect(0, 0, page.Width, page.Height),
                XStringFormats.Center);
            PdfPage page2 = report.AddPage();

            gfx = XGraphics.FromPdfPage(page2);

            int ySpacing = 40;
            gfx.DrawString($"Country: {data["location"]["country"]}", font, XBrushes.Black, 20, ySpacing);
            ySpacing += 20;

            gfx.DrawString($"Region: {data["location"]["region"]}", font, XBrushes.Black, 20, ySpacing);
            ySpacing += 20;

            gfx.DrawString($"City Name: {data["location"]["name"]}", font, XBrushes.Black, 20, ySpacing);
            ySpacing += 20;

            gfx.DrawString($"Weather Description: {string.Join(", ", data["current"]["weather_descriptions"].ToObject<string[]>())}", font, XBrushes.Black, 20, ySpacing);
            ySpacing += 20;

            gfx.DrawString($"Temperature: {data["current"]["temperature"]}°C", font, XBrushes.Black, 20, ySpacing);
            ySpacing += 20;

            gfx.DrawString($"Feels Like: {data["current"]["feelslike"]}°C", font, XBrushes.Black, 20, ySpacing);
            
            ySpacing += 20;
            string date = DateTime.Now.ToString("yyyyMMdd");
            string pdfFilename = $"{cityName}_{date}.pdf";
            report.Save(pdfFilename);
            Console.WriteLine($"Weather report saved to {pdfFilename}");
        }
    }
}
