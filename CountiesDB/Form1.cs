using CountiesDB.Logic;
using CountiesDB.Model;
using CountiesDB.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CountiesDB
{
    public partial class Form1 : Form
    {
        ApiHelper api = new ApiHelper();
        FormLogic logic = new FormLogic();
        public Form1()
        {
            InitializeComponent();
            // Выводим на раздел настроек значения по умолчанию
            tbConfigServ.Text = DataBaseHelper.Server;
            tbConfigBd.Text = DataBaseHelper.DataBase;
            rbSecWin.Checked = true;
            tbConfigUserId.ReadOnly = true;
            tbConfigPassword.ReadOnly = true;

            // Устанавливаем изначально вывод всех стран
            rbGetAllDb.Checked = true;
            rbGetAllApi.Checked = true;
            // Настраиваем таблицы
            SetupTable(tableGetDb);
            SetupTable(tableAddDb);
            SetupTable(tableGetApi);
            SetupAnotherTable(tableGetApi);
            SetupAnotherTable(tableAddDb);
            tableAddDb.RowCount = 1;
            tableAddDb.Height = tableAddDb.ColumnHeadersHeight + tableAddDb.Rows[0].Height;
            tableAddDb.ReadOnly = false;

            rtbHelp.Text = "В данном приложении реализована работа с базой данных, хранящий основную информаци о странах.\n\n" +
                "Для настройки подключения используйте пункт 'Подключение к БД'," +
                " в ктором можно указать имя сервера, бд и таблиц, а также просмотреть, устанавливается соединение с БД. " +
                "Изначально подключение уже настроено для работы с разработанной БД, однако, если используется БД с другим названием, то " +
                "есть возможность изменить настройки.\n\n" +
                "Для вывода информации о странах воспользуйтесь разделом 'Просмотр БД', где можете просмотреть либо все страны БД, либо " +
                "конкретную страну, введя её название. Если такой страны нет в БД, то будет предложено добавить её.\n\n" +
                "Добавление страны происходит в разделе 'Добавить запись', где необходимо ввести все параметры страны (площадь - вещественной число, население - целое) " +
                "после чего нажать клавишу добавить. Если такая страна уже есть, то её данные обновятся.\n\n\n" +
                "Разработчик: Денисов Дмитрий Сергеевич.";
            PrintConnectionStatus();
        }

        private async void PrintConnectionStatus()
        {
            lbStatus.Text = (await Task.Run(() => logic.TestConnection()));
        }

        private void SetupTable(DataGridView table)
        {
            table.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            table.AllowUserToResizeColumns = true;
            table.AllowUserToResizeRows = false;
            table.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            table.AllowUserToAddRows = false;
            table.AllowUserToDeleteRows = false;
            table.ReadOnly = true;
            table.RowHeadersVisible = false;
            table.ColumnHeadersVisible = true;
        }

        private void SetupAnotherTable(DataGridView table)
        {
            table.ColumnCount = 6;
            table.Columns[0].Name = "Название";
            table.Columns[1].Name = "Код_страны";
            table.Columns[2].Name = "Столица";
            table.Columns[3].Name = "Площадь";
            table.Columns[4].Name = "Население";
            table.Columns[5].Name = "Регион";
        }

        /// <returns>Вовзращает выбранную кнопку</returns>
        private DialogResult GetMessageYesNo(string message)
        {
            return MessageBox.Show(
                    message,
                    "Сообщение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
        }

        // Кнопка - применить настройки к соединению с БД
        private void button1_Click(object sender, EventArgs e)
        {
            logic.ApplySettings(rbSecWin.Checked, tbConfigServ.Text, tbConfigBd.Text, tbConfigUserId.Text, tbConfigPassword.Text);
            PrintConnectionStatus();
        }

        // Кнопка - выполнить вывод данных из БД
        private void button2_Click(object sender, EventArgs e)
        {
            DataSet ds = logic.GetDataFromDb(rbGetAllDb.Checked, tbDb.Text);
            if (ds.Tables[0].Rows.Count == 0)
            {
                DialogResult result = GetMessageYesNo("Такой страны в БД не найдено.Желаете добавить её в базу данных ? ");
                if (result == DialogResult.Yes)
                {
                    tabControl1.SelectedTab = tabControl1.TabPages[2];
                    tableAddDb.Rows[0].Cells[0].Value = tbDb.Text;
                }
            }
            tableGetDb.DataSource = ds.Tables[0];
        }

        // Кнопка - добавить новую запись в таблицу
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                string name = Convert.ToString(tableAddDb.Rows[0].Cells[0].Value);
                string code = Convert.ToString(tableAddDb.Rows[0].Cells[1].Value);
                string city = Convert.ToString(tableAddDb.Rows[0].Cells[2].Value);
                double area = (tableAddDb.Rows[0].Cells[3].Value != null) ? double.Parse(Convert.ToString(tableAddDb.Rows[0].Cells[3].Value)) : 0.0;
                int population = (tableAddDb.Rows[0].Cells[4].Value != null) ? int.Parse(Convert.ToString(tableAddDb.Rows[0].Cells[4].Value)) : 0;
                string region = Convert.ToString(tableAddDb.Rows[0].Cells[5].Value);

                int changeRow = DataBaseHelper.AddOrUpdateValue(name, code, city, area, population, region);

                _ = MessageBox.Show(string.Format("Была изменена/добавлена {0} строка!", changeRow));
                tableAddDb.Rows.Clear();
                tableAddDb.Rows.Add();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message + "\nОбратите внимание, что площадь - дробное число, а население - целое!",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // Кнопка - вывод стран с Api
        private void btApi_Click(object sender, EventArgs e)
        {
            tableGetApi.Rows.Clear();
            List<Country> countries = new List<Country>();
            if (rbGetAllApi.Checked)
            {
                try
                {
                    countries = api.GetAllCountries();
                }
                catch
                {
                    MessageBox.Show("Ошибка! Возможно, у вас нет подключения к интернету, либо временно не работает Api");
                }
            }
            else
            {
                try
                {
                    string name = tbApi.Text;
                    countries = api.GetOneCountry(name);
                    DialogResult result = GetMessageYesNo("Желаете добавить эту страну в БД?");
                    if (result == DialogResult.Yes)
                    {
                        int changeRow = DataBaseHelper.AddOrUpdateValue(countries[0].Name, countries[0].NumericCode,
                            countries[0].Capital, countries[0].Area, countries[0].Population, countries[0].Region);
                        _ = MessageBox.Show(string.Format("Была изменена/добавлена {0} строка!", changeRow));
                    }
                }
                catch
                {
                    MessageBox.Show("Ошибка! Возможно, вы ошиблись при вводе страны. Перепроверьте введённый текст (он должен быть на английском языке)." +
                        " Также может быть, что у вас нет подключения к интернету, либо временно не работает Api.");
                }
            }
            if (countries.Count > 0)
            {
                for (int i = 0; i < countries.Count; i++)
                {
                    int rowNumber = tableGetApi.Rows.Add();
                    tableGetApi.Rows[rowNumber].Cells[0].Value = countries[i].Name;
                    tableGetApi.Rows[rowNumber].Cells[1].Value = countries[i].NumericCode;
                    tableGetApi.Rows[rowNumber].Cells[2].Value = countries[i].Capital;
                    tableGetApi.Rows[rowNumber].Cells[3].Value = countries[i].Area;
                    tableGetApi.Rows[rowNumber].Cells[4].Value = countries[i].Population;
                    tableGetApi.Rows[rowNumber].Cells[5].Value = countries[i].Region;
                }
            }
            else
            {
                DialogResult result = GetMessageYesNo("С Api не получено стран. Желаете добавить страну в БД вручную?");
                if (result == DialogResult.Yes)
                {
                    tabControl1.SelectedTab = tabControl1.TabPages[2];
                    // Добавляем в раздел добавления название не найденной страны
                    tableAddDb.Rows[0].Cells[0].Value = tbApi.Text;
                }
            }
        }

        // Выбран какой-то рб отвечающий за тип аутентификации 
        private void rbSecWin_CheckedChanged(object sender, EventArgs e)
        {
            if (rbSecWin.Checked)
            {
                tbConfigUserId.ReadOnly = true;
                tbConfigPassword.ReadOnly = true;
                tbConfigUserId.Clear();
                tbConfigPassword.Clear();
            }
            else
            {
                tbConfigUserId.ReadOnly = false;
                tbConfigPassword.ReadOnly = false;
            }
        }
    }
}
