using System.Collections.Generic;
using System.IO;
using System.Net;
using CountiesDB.Model;
using Newtonsoft.Json;

namespace CountiesDB.Repositories
{
    public class WorkWithApi
    {
        // Ссылка на получение всех стран с Api
        private string mainApiUrl = "https://restcountries.eu/rest/v2";

        private List<Country> countries = new List<Country>();

        public List<Country> GetAllCountries()
        {
            string response = GetResponse(mainApiUrl);
            countries = JsonConvert.DeserializeObject<List<Country>>(response);
            return countries;
        }

        public List<Country> GetOneCountry(string name)
        {
            string response = GetResponse(OneCountryUrl(name));
            countries = JsonConvert.DeserializeObject<List<Country>>(response);
            return countries;
        }

        private string OneCountryUrl(string name)
        {
            return mainApiUrl + "/name/" + name;
        }

        private string GetResponse(string url)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            string response;
            using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
            {
                response = streamReader.ReadToEnd();
            }
            return response;
        }
    }
}
