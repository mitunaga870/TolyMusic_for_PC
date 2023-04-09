using System.Data.SQLite;

namespace TolyMusic_for_PC
{
    public class LocalDB
    {
        private conn = new SQLiteConnection("data source=local.db");
        public LocalDB()
        {
            
        }
    }
}