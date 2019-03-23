using System.Collections.Generic;
using UnityEngine;

namespace UniEasy
{
    public class EasyCsv
    {
        private readonly string[] RowSymbol = { "\n" };
        private readonly string[] ColumnSymbol = { ";" };
        private string[][] values;
        private Dictionary<string, Coordinate> target;
        private Dictionary<string, Dictionary<string, int>> RowDictionary;
        private Dictionary<string, Dictionary<string, int>> ColumnDictionary;

        protected struct Coordinate
        {
            public int row;
            public int column;
        }

        public int RowCount
        {
            get
            {
                if (values == null || values.Length <= 0)
                {
                    return 0;
                }
                return values.Length - 1;
            }
        }

        public int ColumnCount
        {
            get
            {
                if (values == null || values.Length <= 0)
                {
                    return 0;
                }
                return values[0].Length;
            }
        }

        public EasyCsv(TextAsset textAsset)
        {
            var rows = textAsset.text.Split(RowSymbol, System.StringSplitOptions.None);
            values = new string[rows.Length][];
            for (int i = 0; i < rows.Length; i++)
            {
                values[i] = rows[i].Split(ColumnSymbol, System.StringSplitOptions.None);
                var count = values[i].Length;
                values[i][count - 1] = values[i][count - 1].Replace("\r", "");
            }

            target = new Dictionary<string, Coordinate>();
            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColumnCount; j++)
                {
                    var content = GetValue(i, j);
                    if (!target.ContainsKey(content))
                    {
                        target.Add(content, new Coordinate() { row = i, column = j });
                    }
                    else
                    {
#if UNITY_EDITOR
                        Debug.LogWarning(string.Format("This data name '{0}' conflicts! Please make sure you will not use this data name to search.", content));
#endif
                    }
                }
            }

            RowDictionary = new Dictionary<string, Dictionary<string, int>>();
            ColumnDictionary = new Dictionary<string, Dictionary<string, int>>();
        }

        public string GetValue(int row, int column)
        {
            if (row < 0 || column < 0 || values == null)
            {
                return default(string);
            }
            if (values.Length <= 0 || row >= values.Length)
            {
                return default(string);
            }
            if (values[0] == null || column >= values[0].Length)
            {
                return default(string);
            }
            return values[row][column];
        }

        public Dictionary<string, int> ToRowDictionary(string name)
        {
            if (RowDictionary.ContainsKey(name))
            {
                return RowDictionary[name];
            }
            var dictionary = new Dictionary<string, int>();
            if (target.ContainsKey(name))
            {
                int i = 0;
                foreach (var value in values[target[name].row])
                {
                    dictionary.Add(value, i);
                    i++;
                }
                RowDictionary.Add(name, dictionary);
            }
            return dictionary;
        }

        public Dictionary<string, int> ToColumnDictionary(string name)
        {
            if (ColumnDictionary.ContainsKey(name))
            {
                return ColumnDictionary[name];
            }
            var dictionary = new Dictionary<string, int>();
            if (target.ContainsKey(name))
            {
                for (int i = 0; i < RowCount; i++)
                {
                    dictionary.Add(GetValue(i, target[name].column), i);
                }
                ColumnDictionary.Add(name, dictionary);
            }
            return dictionary;
        }

        public string GetValue(string name, string rowName, string columnName)
        {
            var rowDictionary = this.ToRowDictionary(name);
            var columnDictionary = this.ToColumnDictionary(name);
            if (columnDictionary.ContainsKey(rowName) && rowDictionary.ContainsKey(columnName))
            {
                var row = columnDictionary[rowName];
                var column = rowDictionary[columnName];
                return GetValue(row, column);
            }
            return default(string);
        }
    }
}
