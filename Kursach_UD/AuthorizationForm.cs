using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace kursach_UD
{
    public partial class AuthorizationForm : Form
    {
        public AuthorizationForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: данная строка кода позволяет загрузить данные в таблицу "_ISTb_20_1_Akulin_WarehouseDataSet.Goods". При необходимости она может быть перемещена или удалена.
            this.goodsTableAdapter.Fill(this._ISTb_20_1_Akulin_WarehouseDataSet.Goods);
            // TODO: данная строка кода позволяет загрузить данные в таблицу "_ISTb_20_1_Akulin_WarehouseDataSet.Buyer". При необходимости она может быть перемещена или удалена.
            this.buyerTableAdapter.Fill(this._ISTb_20_1_Akulin_WarehouseDataSet.Buyer);

        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(emailTextBox.Text) && !string.IsNullOrEmpty(passwordTextBox.Text))
            {
                string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
                // Подключаемся к базе данных MSSQL
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Формируем SELECT-запросс параметрами @email и @password
                    using (SqlCommand command = new SqlCommand("SELECT * FROM Administrator WHERE Email = @email AND Password = @password", connection))
                    {
                        command.Parameters.AddWithValue("@email", emailTextBox.Text);
                        command.Parameters.AddWithValue("@password", passwordTextBox.Text);


                        // Выполняем запрос и сохраняем результат в объект reader
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // Если указанная почта и пароль найдены в базе данных, открываем главную страницу
                            if (reader.HasRows)
                            {
                                MainForm mainForm = new MainForm();
                                mainForm.Show();
                                this.Hide();
                            }
                            else
                            {
                                // Иначе выводим сообщение об ошибке
                                MessageBox.Show("Неверная почта или пароль!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            else
            {
                MessageBox.Show("Введите почту и пароль!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void registerButton_Click(object sender, EventArgs e)
        {


            // Открываем форму главной страницы
            RegisterForm registerForm = new RegisterForm();
            registerForm.Show();
            this.Hide();
        }

        private void AuthorizationForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Close();
            }

        }
    }
}


 