using MySqlConnector;
using System;
using System.Configuration;

namespace TolyMusic_for_PC.Library
{
    public class DB
    {
        //変数宣言
        private MySqlConnection con;
        //通信開始
        private void ConUP()
        {
            string constring = "Server=" + Properties.Settings.Default.LibraryServerAdress + ";";
            constring += "Database=" + Properties.Settings.Default.LibraryServerPort + ";";
            constring += "User ID=" + Properties.Settings.Default.LibraryServerUser + ";";
            constring += "Password=" + Properties.Settings.Default.LibraryServerPass + ";";
            con = new MySqlConnection(constring);
            con.Open();
        }
        //通信終了
        private void ConDown()
        {
            con.Close();
            con.Dispose();
        }
        //insert
        public void NonQuery(string query, MySqlParameter[] parameters)
        {
            ConUP();
            MySqlCommand cmd = new MySqlCommand(query, con);
            foreach (var param in parameters)
            {
                cmd.Parameters.Add(param);
            }
            cmd.ExecuteNonQuery();
            ConDown();
        }
    }
}