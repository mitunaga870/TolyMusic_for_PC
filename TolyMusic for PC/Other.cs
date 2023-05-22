using System;
using System.Collections.Generic;

namespace TolyMusic_for_PC;

public class Other
{
    public static bool CheckDBValue(Dictionary<string,object> dic, string key)
    {
        return dic.ContainsKey(key) && dic[key] != DBNull.Value;
    }
}