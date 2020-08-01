using System.Data;

namespace CountiesDB.Logic
{
    class FormLogic
    {
        public string TestConnection()
        {
            return DataBaseHelper.TryConnect() ? "Статус соединения: доступно" : "Статус соединения: недоступно";
        }
        public void ApplySettings(bool typeSecurity, string configServ, string configBd, string configUserId, string configPassword)
        {
            if (typeSecurity)
            {
                DataBaseHelper.UpdateConnectionString(configServ, configBd, "True");
            }
            else
            {
                DataBaseHelper.UpdateConnectionString(configServ, configBd, "False", configUserId, configPassword);
            }
        }

        public DataSet GetDataFromDb(bool allCountries, string countryName)
        {
            DataSet result;
            if (allCountries)
            {
                result = DataBaseHelper.GetAllCountries();
            }
            else
            {
                result = DataBaseHelper.GetOneCountry(countryName);
            }
            return result;
        }
    }
}
