using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace TolyMusic_for_PC.Local
{
    public class DB
    {
        private static SQLiteConnection con = new SQLiteConnection("Data Source=local.sqlite3");
        public static void NonQuery(string query)
        {
            con.Open();
            SQLiteCommand cmd = new SQLiteCommand(query, con);
            cmd.ExecuteNonQuery();
            con.Close();
        }
        public static void NonQuery(string[] querys)
        {
            con.Open();
            SQLiteTransaction tran = con.BeginTransaction();
            Parallel.ForEach(querys, (query) =>
            {
                SQLiteCommand cmd = new SQLiteCommand(query, con);
                cmd.Transaction = tran;
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            });
            tran.Commit();
            tran.Dispose();
            con.Close();
        }
        public static void NonQuery(string query, List<SQLiteParameter[]> parameters)
        {
            con.Open();
            SQLiteTransaction tran = con.BeginTransaction();
            Parallel.ForEach(parameters, (parameter) =>
            {
                SQLiteCommand cmd = new SQLiteCommand(query, con);
                cmd.Transaction = tran;
                cmd.Parameters.AddRange(parameter);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            });
            tran.Commit();
            tran.Dispose();
            con.Close();
        }
        public static Collection<Dictionary<string,object>> Reader(string query)
        {
            //変数宣言
            Collection<Dictionary<string, object>> result = new Collection<Dictionary<string, object>>();
            con.Open();
            SQLiteCommand cmd = new SQLiteCommand(query, con);
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Dictionary<string,object> row = new Dictionary<string, object>();
                for(int i = 0; i < reader.FieldCount; i++)
                {
                    if(row.ContainsKey(reader.GetName(i)))
                        continue;
                    row.Add(reader.GetName(i), reader.GetValue(i));
                }
                result.Add(row);
            }
            reader.Close();
            cmd.Dispose();
            con.Close();
            return result;
        }
        public static Collection<Dictionary<string,object>> Reader(string query, SQLiteParameter[] parameters)
        {
            //変数宣言
            Collection<Dictionary<string, object>> result = new Collection<Dictionary<string, object>>();
            con.Open();
            SQLiteCommand cmd = new SQLiteCommand(query, con);
            cmd.Parameters.AddRange(parameters);
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Dictionary<string, object> row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if(row.ContainsKey(reader.GetName(i)))
                        continue;
                    row.Add(reader.GetName(i), reader.GetValue(i));
                }
                result.Add(row);
            }
            reader.Close();
            cmd.Dispose();
            con.Close();
            return result;
        }
    }
}