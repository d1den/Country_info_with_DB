using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Runtime.InteropServices.WindowsRuntime;
using System.CodeDom;

namespace CountiesDB
{
    /// <summary>
    /// Класс для работы с базой данных стран
    /// </summary>
    static class WorkWithDB
    {
        /// <summary>
        /// Свойство для работы с именем сервера
        /// </summary>
        public static string Server { set; get; } = "SQLEXPRESS";

        /// <summary>
        /// Свойство для работы с именем базы данных
        /// </summary>
        public static string DataBase { set; get; } = "InfoAboutCountry";

        /// <summary>
        /// Свойство для работы с именем таблицы стран
        /// </summary>
        public static string City { set; get; } = "Cityes";

        /// <summary>
        /// Свойство для работы с именем таблицы регионов
        /// </summary>
        public static string Region { set; get; } = "Regions";

        /// <summary>
        /// Свойство для работы с именем таблицы стран
        /// </summary>
        public static string Country { set; get; } = "Countries";

        /// <summary>
        /// Метод для проверки состояния соединения
        /// </summary>
        /// <returns> Возвращает лог значение наличия соединения </returns>
        public static bool TryConnection()
        {
            try
            {
                // Строка для подключения
                string connectionString = String.Format(@"Data Source=.\{0};Initial Catalog={1};Integrated Security=True", Server, DataBase);
                using (SqlConnection connection = new SqlConnection(connectionString)) // Устанавливаем соединение с БД
                {
                    connection.Open(); // Открываем соединение
                    return true; // Если открылось, То возвращаем 1
                }
            }
            catch
            {
                return false; // Если произошла ошибка, то 0
            }
        }

        /// <summary>
        /// Метод, выполняющий вывод всех стран из БД
        /// </summary>
        /// <returns> Возвращает объект таблицу с результатами </returns>
        public static DataSet GetCountries()
        {
            // Строка для подключения
            string connectionString = String.Format(@"Data Source=.\{0};Initial Catalog={1};Integrated Security=True", Server, DataBase);
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString)) // Устанавливаем соединение с БД
                {
                    connection.Open();
                    // Далее пишем запрос
                    string sql = String.Format("SELECT cs.name as Название," +
                        " cs.code as Код_страны, c.name as Столица," +
                        " cs.area as Площадь, cs.population as Население," +
                        " r.name as Регион" +
                        " FROM {0} as cs inner join {1} as c on cs.city_id = c.id" +
                        " inner join {2} as r on cs.region_id = r.id", Country, City, Region);
                    // Создаем объект DataAdapter
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);
                    // Создаем объект Dataset
                    DataSet ds = new DataSet();
                    // Заполняем Dataset
                    adapter.Fill(ds);
                    // Возвращаем таблицу
                    return ds;
                }
            }
            catch (SqlException ex) // Если произошла ошибка
            {
                // То выводим текст этой ошибки
                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                DataSet ds = new DataSet();
                return ds; // И возвращаем пустую таблицу
            }
        }

        /// <summary>
        /// Метод для вывода одной конкретной страны из БД
        /// </summary>
        /// <param name="countryName"> Принимает название страны </param>
        /// <returns> Возвращает наденные данные по этой стране </returns>
        public static DataSet GetCountries(string countryName)
        {
            // Строка для подключения
            string connectionString = String.Format(@"Data Source=.\{0};Initial Catalog={1};Integrated Security=True", Server, DataBase);
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString)) // Устанавливаем соединение с БД
                {
                    connection.Open();
                    // Далее пишем запрос для поиска конкретной страны
                    string sql = String.Format("SELECT cs.name as Название," +
                        " cs.code as Код_страны, c.name as Столица," +
                        " cs.area as Площадь, cs.population as Население," +
                        " r.name as Регион" +
                        " FROM {0} as cs inner join {1} as c on cs.city_id = c.id" +
                        " inner join {2} as r on cs.region_id = r.id " +
                        "where cs.name = '{3}'", Country, City, Region, countryName);
                    // Создаем объект DataAdapter
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);
                    // Создаем объект Dataset
                    DataSet ds = new DataSet();
                    // Заполняем Dataset
                    adapter.Fill(ds);
                    // Возвращаем таблицу
                    return ds;
                }
            }
            catch (SqlException ex) // Если произошла ошибка
            {
                // То выводим текст этой ошибки
                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                DataSet ds = new DataSet();
                return ds; // И возвращаем пустую таблицу
            }
        }

        /// <summary>
        /// Метод добавления новой страны в БД/Обновления существующей
        /// </summary>
        /// <param name="name"> Название страны </param>
        /// <param name="code"> Код страны </param>
        /// <param name="city"> Столица </param>
        /// <param name="area"> Площадь </param>
        /// <param name="population"> Население </param>
        /// <param name="region"> Регион </param>
        /// <returns> Возвращает кол-во изменённых строк </returns>
        public static int AddNewCountry(string name, string code, string city, double area, int population, string region)
        {
            int changeRow = 0; // Переменная для отслеживания кол-ва добавленных/изменённых стран
            int city_id = 0; // id Столицы
            int region_id = 0; // id Региона
            // Строка для подключения
            string connectionString = String.Format(@"Data Source=.\{0};Initial Catalog={1};Integrated Security=True", Server, DataBase);
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString)) // Устанавливаем соединение с БД
                {
                    connection.Open();
                    // Проверяем сначала наличие Столицы в таблице городов
                    string sql = String.Format("select count(*) from {0} where name = '{1}'", City, city);
                    SqlCommand command = new SqlCommand(sql, connection);
                    int count = (int)command.ExecuteScalar();
                    if (count != 0) // Если такая столица уже есть в таблице, то берём её id
                    {
                        sql = String.Format("select id from {0} where name = '{1}'", City, city);
                        command = new SqlCommand(sql, connection);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows) // Если есть данные
                            {
                                while (reader.Read())
                                {
                                    city_id = reader.GetInt32(0);
                                }
                            }
                        }
                    }
                    else // Иначе добавляем новый город и берём его id
                    {
                        sql = String.Format("insert into {0} values('{1}')", City, city); // Добавляем
                        command = new SqlCommand(sql, connection);
                        command.ExecuteNonQuery();
                        // Получаем его id
                        sql = String.Format("select id from {0} where name = '{1}'", City, city);
                        command = new SqlCommand(sql, connection);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows) // если есть данные
                            {
                                while (reader.Read())
                                {
                                    city_id = reader.GetInt32(0); // Получаем id
                                }
                            }
                        }
                    }

                    // Затем проверяем наличие Региона в таблице регионов (полностью аналогично столице)
                    sql = String.Format("select count(*) from {0} where name = '{1}'", Region, region);
                    command = new SqlCommand(sql, connection);
                    count = (int)command.ExecuteScalar();
                    if (count != 0)
                    {
                        sql = String.Format("select id from {0} where name = '{1}'", Region, region);
                        command = new SqlCommand(sql, connection);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows) // если есть данные
                            {
                                while (reader.Read())
                                {
                                    region_id = reader.GetInt32(0);
                                }
                            }
                        }
                    }
                    else
                    {
                        sql = String.Format("insert into {0} values('{1}')", Region, region);
                        command = new SqlCommand(sql, connection);
                        command.ExecuteNonQuery();
                        sql = String.Format("select id from {0} where name = '{1}'", Region, region);
                        command = new SqlCommand(sql, connection);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows) // если есть данные
                            {
                                while (reader.Read())
                                {
                                    region_id = reader.GetInt32(0);
                                }
                            }
                        }
                    }

                    // Далее проверяем страну по её коду
                    sql = String.Format("select count(*) from {0} where code = '{1}'", Country, code);
                    command = new SqlCommand(sql, connection);
                    count = (int)command.ExecuteScalar();
                    if (count != 0) // Если страна есть в таблице, то редактируем её значения
                    {
                        sql = String.Format("update {0} set name = '{1}'," +
                            " city_id = {2}, area = {3}," +
                            " population = {4}, region_id = {5} where code = {6}",
                            Country, name, city_id, area, population, region_id, code);
                        command = new SqlCommand(sql, connection);
                        changeRow = command.ExecuteNonQuery(); // Получаем кол-во изменённых строк
                    }
                    else // Если нет в таблице, то добавляем
                    {
                        sql = String.Format("insert into {0} values('{1}', '{2}', {3}, {4}, {5}, {6})",
                            Country, name, code, city_id, area, population, region_id);
                        command = new SqlCommand(sql, connection);
                        changeRow = command.ExecuteNonQuery(); // Получаем кол-во изменённых строк
                    }
                    return changeRow;
                }
            }
            catch (SqlException ex) // Если произошла ошибка
            {
                // То выводим текст этой ошибки
                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return changeRow;
            }
            
        }
    }
}
