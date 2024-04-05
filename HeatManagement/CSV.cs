using System.Collections.Generic;

namespace HeatManagement;

public static class CSVUtils
{
    public static string TableToString(List<string[]> table)
    {
        //escape commas and quotes
        string[] rows = new string[table.Count];
        for (int i = 0; i < table.Count; i++)
        {
            for (int j = 0; j < table[i].Length; j++)
            {
                if (table[i][j].Contains(',')) table[i][j] = $"\"{table[i][j].Replace("\"", "\"\"")}\"";
            }
            rows[i] = string.Join(',', table[i]);
        }
        return string.Join(/*Environment.NewLine*/ '\n', rows); //no mercy for windows users, \r\n is cringe
    }
}