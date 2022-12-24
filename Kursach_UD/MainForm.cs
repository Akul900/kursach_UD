using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kursach_UD
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            // Получаем список таблиц из базы данных и добавляем их в ComboBox
            string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES Where TABLE_NAME != 'Administrator' and TABLE_NAME != 'sysdiagrams'";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string tableName = reader.GetString(0);
                        tableComboBox.Items.Add(tableName);
                    }
                }
                connection.Close();
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            

        }

        private void tableComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string tableName = tableComboBox.SelectedItem.ToString();

            // Загружаем данные из выбранной таблицы

            dataGridView.Columns.Clear();
            string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            //в базе данных в элемент управления DataGridView
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = $"SELECT * FROM {tableName}";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    


                    dataGridView.DataSource = dataTable;
                    deleteColumnAdd();
                    UpdateColumnAdd();

                }
            }
        }

        private void deleteColumnAdd()
        {

            DataGridViewButtonColumn deleteColumn = new DataGridViewButtonColumn();
            deleteColumn.Name = "DeleteColumn";
            deleteColumn.HeaderText = "Удалить";
            deleteColumn.Text = "Удалить";
            deleteColumn.UseColumnTextForButtonValue = true;

            dataGridView.Columns.Add(deleteColumn);
            dataGridView.CellContentClick += dataGridView_CellContentClick;
        }

        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
         /*   if (dataGridView.Columns.Contains("Delete") && e.ColumnIndex == dataGridView.Columns["Delete"].Index && e.RowIndex >= 0)
            {*/
                // Получаем значение идентификатора строки, на которую нажали кнопку удаления
                int id = (int)dataGridView.Rows[e.RowIndex].Cells["Id"].Value;

            
                string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
                string tableName = tableComboBox.SelectedItem.ToString();

                // Открываем соединение с базой данных
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Формируем запрос на удаление строки
                    string query = $"DELETE FROM {tableName} WHERE Id = @Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.ExecuteNonQuery();
                        string selectQuery = $"SELECT * FROM {tableName}";
                        SqlCommand selectCommand = new SqlCommand(selectQuery, connection);
                        SqlDataAdapter adapter = new SqlDataAdapter(selectCommand);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        dataGridView.DataSource = dataTable;
                    }
                }

                // Обновляем таблицу после удаления строки
           /*
            }*/
        }

        private void UpdateColumnAdd()
        {

            DataGridViewButtonColumn updateColumn = new DataGridViewButtonColumn();
            updateColumn.Name = "UpdateColumn";
            updateColumn.HeaderText = "Обновить";
            updateColumn.Text = "Обновить";
            updateColumn.UseColumnTextForButtonValue = true;

            dataGridView.Columns.Add(updateColumn);
        }


        private void addButton_Click(object sender, EventArgs e)
        {


        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            string searchText = searchTextBox.Text;
            /*           string tableName = tableComboBox.SelectedItem.ToString();*/

            string tableName;
            if (tableComboBox.SelectedItem != null)
            {
                tableName = tableComboBox.SelectedItem.ToString();
            }
            else
            {
                MessageBox.Show("Не выбрана таблица для поиска");
                 return;
            }
/*
            if (string.IsNullOrWhiteSpace(tableName))
            {
                MessageBox.Show("Выберите таблицу для поиска", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }*/

            if (string.IsNullOrWhiteSpace(searchText))
            {
                MessageBox.Show("Введите текст для поиска", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Получаем список столбцов таблицы
            List<string> columns = new List<string>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableName", tableName);
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string columnName = reader.GetString(0);
                        columns.Add(columnName);
                    }
                }
            }

            // Формируем условие WHERE для запроса SELECT
            string whereClause = "";
            for (int i = 0; i < columns.Count; i++)
            {
                whereClause += $"{columns[i]} LIKE '%' + @SearchText + '%'";
                if (i < columns.Count - 1)
                {
                    whereClause += " OR ";
                }
            }


            // Выполняем запрос SELECT с условием WHERE
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string selectQuery = $"SELECT * FROM {tableName} WHERE {whereClause}";
                using (SqlCommand command = new SqlCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@SearchText", searchText);
                    SqlDataReader reader = command.ExecuteReader();

                    // Очищаем таблицу
                    if (dataGridView.CurrentRow == null)
                    {
                        dataGridView.Rows.Clear();

                    }
                    dataGridView.Columns.Clear();

                    // Добавляем столбцы таблицы
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        dataGridView.Columns.Add(reader.GetName(i), reader.GetName(i));
                    }

                    // Добавляем строки таблицы
                    while (reader.Read())
                    {
                        object[] row = new object[reader.FieldCount];
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[i] = reader.GetValue(i);
                        }
                        dataGridView.DataSource = null;
                        dataGridView.Rows.Add(row);
                    
                    }
                    deleteColumnAdd();
                    UpdateColumnAdd();
                }
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Close();
            }
       
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            string tableName = tableComboBox.SelectedItem.ToString();
            int id = (int)dataGridView.CurrentRow.Cells["Id"].Value;


        using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = $"DELETE FROM {tableName} WHERE Id = @Id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                    string selectQuery = $"SELECT * FROM {tableName}";
                    SqlCommand selectCommand = new SqlCommand(selectQuery, connection);
                    SqlDataAdapter adapter = new SqlDataAdapter(selectCommand);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    dataGridView.DataSource = dataTable;
                }

            }
        }

        private void updateButton_Click(object sender, EventArgs e)
        {

 

        }
    }
}
