using System.Collections.Generic;
using UnityEngine;

namespace UniEasy
{
    public class EasyCsv
    {
        protected struct Coordinate
        {
            public int Row;
            public int Column;
        }

        private string[][] elements;
        private Dictionary<string, Coordinate> target;
        private Dictionary<string, Dictionary<string, int>> rowDict;
        private Dictionary<string, Dictionary<string, int>> columnDict;
        private readonly string[] RowSymbol = new string[] { "\n" };
        private readonly string[] ColumnSymbol = new string[] { "," };
        private const string EmptyStr = "";
        private const string NewLineStr = "\r";
        private const string ConflictWarning = "The data name \"{0}\" conflicts! Make sure you don't search using this data name.";

        public int RowCount
        {
            get
            {
                if (elements == null || elements.Length <= 0)
                {
                    return 0;
                }
                return elements.Length - 1;
            }
        }

        public int ColumnCount
        {
            get
            {
                if (elements == null || elements.Length <= 0)
                {
                    return 0;
                }
                return elements[0].Length;
            }
        }

        public EasyCsv(TextAsset textAsset)
        {
            var rows = textAsset.text.Split(RowSymbol, System.StringSplitOptions.None);
            elements = new string[rows.Length][];
            for (int i = 0; i < rows.Length; i++)
            {
                elements[i] = rows[i].Split(ColumnSymbol, System.StringSplitOptions.None);
                var count = elements[i].Length;
                elements[i][count - 1] = elements[i][count - 1].Replace(NewLineStr, EmptyStr);
            }

            target = new Dictionary<string, Coordinate>();
            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColumnCount; j++)
                {
                    var content = GetValue(i, j);
                    if (!target.ContainsKey(content))
                    {
                        target.Add(content, new Coordinate() { Row = i, Column = j });
                    }
                    else
                    {
#if UNITY_EDITOR
                        Debug.LogWarning(string.Format(ConflictWarning, content));
#endif
                    }
                }
            }

            rowDict = new Dictionary<string, Dictionary<string, int>>();
            columnDict = new Dictionary<string, Dictionary<string, int>>();
        }

        public string GetValue(int row, int column)
        {
            if (row < 0 || column < 0 || elements == null)
            {
                return default;
            }
            if (elements.Length <= 0 || row >= elements.Length)
            {
                return default;
            }
            if (elements[0] == null || column >= elements[0].Length)
            {
                return default;
            }
            return elements[row][column];
        }

        public Dictionary<string, int> GetRow(string name)
        {
            if (rowDict.ContainsKey(name))
            {
                return rowDict[name];
            }
            var dict = new Dictionary<string, int>();
            if (target.ContainsKey(name))
            {
                int i = 0;
                foreach (var value in elements[target[name].Row])
                {
                    dict.Add(value, i);
                    i++;
                }
                rowDict.Add(name, dict);
            }
            return dict;
        }

        public Dictionary<string, int> GetColumn(string name)
        {
            if (columnDict.ContainsKey(name))
            {
                return columnDict[name];
            }
            var dict = new Dictionary<string, int>();
            if (target.ContainsKey(name))
            {
                for (int i = 0; i < RowCount; i++)
                {
                    dict.Add(GetValue(i, target[name].Column), i);
                }
                columnDict.Add(name, dict);
            }
            return dict;
        }

        public string GetValue(string name, string rowName, string columnName)
        {
            var rowDict = GetRow(name);
            var columnDict = GetColumn(name);
            if (columnDict.ContainsKey(rowName) && rowDict.ContainsKey(columnName))
            {
                var row = columnDict[rowName];
                var column = rowDict[columnName];
                return GetValue(row, column);
            }
            return default;
        }
    }
}
