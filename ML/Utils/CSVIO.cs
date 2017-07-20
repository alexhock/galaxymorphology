using System;
using System.Collections.Generic;
using System.IO;


namespace Utils
{
    public class CSVIO
    {
        public static List<T[]> Load<T>(String filePath, char colDelimiter = ',', String lineDelimiter = "\r\n")
        {

            var samples = new List<T[]>();
            var lines = File.ReadAllLines(filePath);
            foreach (String line in lines)
            {
                String[] colVals = line.Split(colDelimiter);
                T[] f = new T[colVals.Length];
                for (int i = 0; i < colVals.Length; i++)
                {
                    f[i] = (T)Convert.ChangeType(colVals[i], typeof(T));
                }
                samples.Add(f);
            }
            return samples;

        }

        public static void Save<T>(String filePath, List<T[]> data, char colDelimiter = ',', String lineDelimiter = "\r\n")
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                for (int i = 0; i < data.Count; i++)
                {
                    T[] line = data[i];
                    for (int j = 0; j < line.Length; j++)
                    {
                        sw.Write(line[j].ToString());
                        if (j < line.Length - 1)
                            sw.Write(colDelimiter);
                    }
                    sw.Write(lineDelimiter);
                }
            }
        }

    }
}
