using MySqlConnector;
using System;
using System.Configuration;

namespace TolyMusic_for_PC.Library
{
    public class DB
    {
        //変数宣言
        private MySqlConnection con;
        //コンストラクタ
        public DB()
        {
            string constring = "Server=" + ConfigurationSettings.AppSettings["DB_Server"] + ";";
            constring += "Database=" + ConfigurationSettings.AppSettings["DB_Database"] + ";";
            constring += "User ID=" + ConfigurationSettings.AppSettings["DB_User"] + ";";
            constring += "Password=" + ConfigurationSettings.AppSettings["DB_Password"] + ";";
            con = new MySqlConnection(constring);
        }
    }
}