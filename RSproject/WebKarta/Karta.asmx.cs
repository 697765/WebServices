using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Services;

namespace WebKarta
{
    /// <summary>
    /// Summary description for Karta
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Karta : System.Web.Services.WebService
    {
        static double lat, lon;

        string api_lokacija="http://api.ipstack.com/check?access_key=f08ae4e36d0284ea8e57a9ffc1526853";
        static string api_key = "AIzaSyAvR-kvk3vAqURbVtXj1ceDn5Ex5f82JR4";

        HttpClient klijent = new HttpClient();
        

        [WebMethod]
        public List<double> DohvatiLokacijuKlijenta()
        {
            var odgovor_lokacija = klijent.GetStringAsync(api_lokacija).Result;

            JObject objekt = JObject.Parse(odgovor_lokacija);
            lat = objekt["latitude"].ToObject<double>();
            lon = objekt.GetValue("longitude").ToObject<double>();

            List<double> lokacija = new List<double>() { lat,lon};
            return lokacija;
        }

        [WebMethod]
        public string DohvatiAktivnosti(int radijus,string aktivnost)
        {
            string prvi = lat.ToString().Replace(",",".");
            string drugi = lon.ToString().Replace(",", ".");

            string api_restorani = $"https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={prvi},{drugi}&radius={radijus}&type={aktivnost}&key={api_key}";
            return(klijent.GetStringAsync(api_restorani).Result);
        }

        [WebMethod]
        public List<Bitmap> DohvatiSlikeAktivnosti(string response,string aktivnost)
        {
            List<Bitmap> niz_slika = new List<Bitmap>();
            Bitmap slika;

            JObject objekt = JObject.Parse(response);
            JArray results = objekt["results"].ToObject<JArray>();

            for(int i=0;i<results.Count;i++)
            {
                if (results[i].ToString().Contains("photos"))
                {
                    JArray photos = results[i]["photos"].ToObject<JArray>();
                    string photoreference = photos[0]["photo_reference"].ToObject<string>();

                    string api_slike = $"https://maps.googleapis.com/maps/api/place/photo?maxwidth=400&photoreference={photoreference}&key={api_key}";
                    var jpegByteArray = klijent.GetByteArrayAsync(api_slike).Result;

                    slika = (Bitmap)((new ImageConverter()).ConvertFrom(jpegByteArray));
                    slika.Save($@"C:\Users\korisnik\Downloads\RSprojekt\Slike\{i}.png");
                }
                else
                {
                    slika = (Bitmap)Image.FromFile($"C:/Users/korisnik/Downloads/RSprojekt/Slike/{aktivnost}.png");
                    slika.Save($@"C:\Users\korisnik\Downloads\RSprojekt\Slike\{i}.png");
                }
                niz_slika.Add(slika);

            }
            return niz_slika;
        }

        [WebMethod]
        public List<double> DohvatiLokacijeAktivnosti(string response)
        {
            List<double> lokacije = new List<double>();

            JObject objekt = JObject.Parse(response);
            JArray results = objekt["results"].ToObject<JArray>();

            for (int i=0;i<results.Count;i++)
            {
                JObject geometry = results[i]["geometry"].ToObject<JObject>();
                JObject location = geometry["location"].ToObject<JObject>();
                lokacije.Add(location["lat"].ToObject<double>());
                lokacije.Add(location["lng"].ToObject<double>());
            }

            return lokacije;
        }
    }
}
