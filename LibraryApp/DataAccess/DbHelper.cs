using System;
using System.Data;
using System.Data.SqlClient;

namespace LibraryApp.DataAccess
{
    public static class DbHelper
    {
        private static readonly string connectionString =
            @"Data Source=localhost\SQLEXPRESS;Initial Catalog=Library;Integrated Security=True";

        public static DataTable GetData(string query, params SqlParameter[] parameters)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        // Клонируется каждый параметр, чтобы избежать ошибки повторного использования
                        foreach (SqlParameter param in parameters)
                        {
                            cmd.Parameters.Add((SqlParameter)((ICloneable)param).Clone());
                        }
                    }

                    conn.Open();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            catch (SqlException ex)
            {
                System.Windows.MessageBox.Show("Ошибка SQL: " + ex.Message);
            }

            return dt;
        }

        public static int Execute(string query, params SqlParameter[] parameters)
        {
            int affectedRows = 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        // Клонируем каждый параметр
                        foreach (SqlParameter param in parameters)
                        {
                            cmd.Parameters.Add((SqlParameter)((ICloneable)param).Clone());
                        }
                    }

                    conn.Open();
                    affectedRows = cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                System.Windows.MessageBox.Show("Ошибка SQL: " + ex.Message);
            }

            return affectedRows;
        }
    }
}