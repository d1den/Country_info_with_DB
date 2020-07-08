using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CountiesDB
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            // Выводим на в раздел настроек значения по умолчанию
            textBox1.Text = WorkWithDB.Server;
            textBox2.Text = WorkWithDB.DataBase;
            textBox3.Text = WorkWithDB.City;
            textBox4.Text = WorkWithDB.Region;
            textBox5.Text = WorkWithDB.Country;
            // Устанавливаем изначально вывод всех стран
            radioButton1.Checked = true;
            // Настраиваем таблицы
            TableConfig(dataGridView1);
            TableConfig(dataGridView2);
            // Дополнительная настройка второй таблицы
            dataGridView2.ColumnCount = 6;
            dataGridView2.RowCount = 1;
            dataGridView2.Columns[0].Name = "Название";
            dataGridView2.Columns[1].Name = "Код_страны";
            dataGridView2.Columns[2].Name = "Столица";
            dataGridView2.Columns[3].Name = "Площадь";
            dataGridView2.Columns[4].Name = "Население";
            dataGridView2.Columns[5].Name = "Регион";
            dataGridView2.Height = dataGridView2.ColumnHeadersHeight + dataGridView2.Rows[0].Height;
            dataGridView2.ReadOnly = false;

            richTextBox1.Text = "В данном приложении реализована работа с базой данных, хранящий основную информаци о странах.\n\n" +
                "Для настройки подключения используйте пункт 'Подключение к БД'," +
                " в ктором можно указать имя сервера, бд и таблиц, а также просмотреть, устанавливается соединение с БД. " +
                "Изначально подключение уже настроено для работы с разработанной БД, однако, если используется БД с другим названием, то " +
                "есть возможность изменить настройки.\n\n" +
                "Для вывода информации о странах воспользуйтесь разделом 'Просмотр БД', где можете просмотреть либо все страны БД, либо " +
                "конкретную страну, введя её название. Если такой страны нет в БД, то будет предложено добавить её.\n\n" +
                "Добавление страны происходит в разделе 'Добавить запись', где необходимо ввести все параметры страны (площадь - вещественной число, население - целое) " +
                "после чего нажать клавишу добавить. Если такая страна уже есть, то её данные обновятся.\n\n\n" +
                "Разработчик: Денисов Дмитрий Сергеевич.";
            // Проверяем соединение
            if (WorkWithDB.TryConnection())
            {
                label6.Text = "Статус соединения: доступно";
            }
            else
            {
                label6.Text = "Статус соединения: недоступно";
            }
        }

        /// <summary>
        /// Метод настройки параметров таблицы DataGridView
        /// </summary>
        /// <param name="table">Принимает объект таблицы</param>
        private void TableConfig(DataGridView table)
        {
            table.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            table.AllowUserToResizeColumns = false;
            table.AllowUserToResizeRows = false;
            table.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            table.AllowUserToAddRows = false;
            table.AllowUserToDeleteRows = false;
            table.ReadOnly = true;
            table.RowHeadersVisible = false;
            table.ColumnHeadersVisible = true; // Индексы столбцов видны
        }

        // Клавиша - применить настройки к соединению с БД
        private void button1_Click(object sender, EventArgs e)
        {
            // Если пользователь изменил настройки, то записываем их в класс работы с БД
            WorkWithDB.Server = textBox1.Text;
            WorkWithDB.DataBase = textBox2.Text;
            WorkWithDB.City = textBox3.Text;
            WorkWithDB.Region = textBox4.Text;
            WorkWithDB.Country = textBox5.Text;
            // Проверяем соединение
            if (WorkWithDB.TryConnection())
            {
                label6.Text = "Статус соединения: доступно";
            }
            else
            {
                label6.Text = "Статус соединения: недоступно";
            }
        }

        // Выполнить вывод данных из БД
        private void button2_Click(object sender, EventArgs e)
        {
            DataSet ds; // Создаём объект данных
            if (radioButton1.Checked) // Если вывести все страны
            {
                ds = WorkWithDB.GetCountries(); // Помещаем туда полученные с БД данные
                if (ds.Tables[0].Rows.Count == 0) // Если нет стран
                {
                    DialogResult result = MessageBox.Show(
                    "В БД нет ни одной страны. Желаете добавить страну в базу данных?",
                    "Сообщение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Yes)
                    {
                        // Переходим на вкладку добавления новой страны
                        tabControl1.SelectedTab = tabControl1.TabPages[1];
                    }
                }
            }
            else // Если вывести одну конкретную страну
            {
                ds = WorkWithDB.GetCountries(textBox6.Text); // Вызываем метод поиска этой страны в БД
                if (ds.Tables[0].Rows.Count == 0) // Если она не нашлась
                {
                    DialogResult result = MessageBox.Show(
                    "Такой страны в БД не найдено. Желаете добавить её в базу данных?",
                    "Сообщение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Yes)
                    {
                        // Переходим на вкладку добавления новой страны
                        tabControl1.SelectedTab = tabControl1.TabPages[1];
                        dataGridView2.Rows[0].Cells[0].Value = textBox6.Text; // Добавляем в таблицу название не найденной страны
                    }
                }
            }
            // Отображаем данные
            dataGridView1.DataSource = ds.Tables[0];
        }

        // Добавить новую запись в таблицу
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                // Получаем данные, введённые в таблицу
                string name = Convert.ToString(dataGridView2.Rows[0].Cells[0].Value);
                string code = Convert.ToString(dataGridView2.Rows[0].Cells[1].Value);
                string city = Convert.ToString(dataGridView2.Rows[0].Cells[2].Value);
                double area = double.Parse(Convert.ToString(dataGridView2.Rows[0].Cells[3].Value));
                int population = int.Parse(Convert.ToString(dataGridView2.Rows[0].Cells[4].Value));
                string region = Convert.ToString(dataGridView2.Rows[0].Cells[5].Value);
                // Вызываем метод добавления/изменения страны, получаем кол-во редактированных строк
                int changeRow = WorkWithDB.AddNewCountry(name, code, city, area, population, region);
                // Выводим сообщения об успешном добавлении/изменении строки
                MessageBox.Show(String.Format("Была изменена/добавлена {0} строка!", changeRow));
                dataGridView2.Rows.Clear(); // Очищаем данные в таблице
                dataGridView2.Rows.Add();
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
    }
}
