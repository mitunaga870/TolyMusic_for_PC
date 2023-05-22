using System.Collections.Generic;
using System.Collections.ObjectModel;
using MySql.Data.MySqlClient;

namespace TolyMusic_for_PC.Library
{
    public class DB
    {
        //変数宣言
        private MySqlConnection con;
        //通信開始
        private void ConUP()
        {
            string constring = "server=" + Properties.Settings.Default.LibraryServerAdress + ";";
            constring += "port=" + Properties.Settings.Default.LibraryServerPort + ";";
            constring += "database=toly_music;";
            constring += "uid=" + Properties.Settings.Default.LibraryServerUser + ";";
            constring += "pwd=" + Properties.Settings.Default.LibraryServerPass + ";";
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
        public void NonQuery(string query, Collection<MySqlParameter> parameters)
        {
            ConUP();
            MySqlCommand cmd = new MySqlCommand(query, con);
            foreach (var param in parameters)
            {
                if(param.Value == "")
                    param.Value = null;
                cmd.Parameters.Add(param);
            }
            cmd.ExecuteNonQuery();
            ConDown();
        }
        //select
        public Collection<Dictionary<string, object>> Read(string query)
        {
            ConUP();
            MySqlCommand cmd = new MySqlCommand(query, con);
            MySqlDataReader reader = cmd.ExecuteReader();
            Collection<Dictionary<string, object>> result = new Collection<Dictionary<string, object>>();
            while (reader.Read())
            {
                Dictionary<string, object> row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row.Add(reader.GetName(i), reader.GetValue(i));
                }
                result.Add(row);
            }
            reader.Close();
            cmd.Dispose();
            ConDown();
            return result;
        }
        public Collection<Dictionary<string,object>> Read(string query, Collection<MySqlParameter> parameters)
        {
            ConUP();
            MySqlCommand cmd = new MySqlCommand(query, con);
            foreach (var param in parameters)
            {
                cmd.Parameters.Add(param);
            }
            
            MySqlDataReader reader = cmd.ExecuteReader();
            Collection<Dictionary<string, object>> result = new Collection<Dictionary<string, object>>();
            while (reader.Read())
            {
                Dictionary<string,object> row = new Dictionary<string, object>();
                for(int i = 0; i < reader.FieldCount; i++)
                {
                    row.Add(reader.GetName(i), reader.GetValue(i));
                }
                result.Add(row);
            }
            reader.Close();
            cmd.Dispose();
            ConDown();
            return result;
        }
    }
}