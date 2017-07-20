package com.codertools.io;

/**
 * Created by ah14aeb on 11/11/2016.
 */

import java.io.FileWriter;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.Map;

import org.apache.commons.csv.CSVFormat;
import org.apache.commons.csv.CSVParser;
import org.apache.commons.csv.CSVRecord;

import java.io.*;
import java.io.BufferedOutputStream;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.Map;

/**
 * Created by alexh on 04/05/2016.
 */
public class CsvParserHelper {

    public static List<double[]> LoadDoubles(String filePath)
    {
        List<double[]> result = new ArrayList<>();

        try(Reader in = new FileReader(filePath)) {
            Iterable<CSVRecord> records = CSVFormat.RFC4180.parse(in);
            for (CSVRecord record : records) {
                double[] row = new double[record.size()];
                for (int i=0; i<row.length;i++)
                    row[i] = Double.parseDouble(record.get(i));
                result.add(row);
            }
        } catch(IOException iox) {
            System.out.println("Failed to parse: " + filePath);
            iox.printStackTrace();
        }
        /*
        try(CSVParser parser = CSVParser.parse(filePath, CSVFormat.RFC4180)) {
            for (CSVRecord csvRecord : parser.getRecords()) {
                double[] row = new double[csvRecord.size()];
                for (int i=0; i<row.length;i++)
                    row[i] = Double.parseDouble(csvRecord.get(i));
                result.add(row);
            }
        }
        catch(IOException iox) {
            System.out.println("Failed to parse: " + filePath);
            iox.printStackTrace();
        }
        */
        return result;
    }

    public static List<float[]> LoadFloats(String filePath)
    {
        List<float[]> result = new ArrayList<>();

        try(CSVParser parser = CSVParser.parse(filePath, CSVFormat.RFC4180)) {
            for (CSVRecord csvRecord : parser) {
                float[] row = new float[csvRecord.size()];
                for (int i=0; i<row.length;i++)
                {
                    row[i] = Float.parseFloat(csvRecord.get(i));
                }
                result.add(row);
            }
        }
        catch(IOException iox) {
            System.out.println("Failed to parse: " + filePath);
            iox.printStackTrace();
        }

        return result;
    }

    public static List<int[]> LoadInts(String filePath)
    {
        List<int[]> result = new ArrayList<int[]>();

        try(CSVParser parser = CSVParser.parse(filePath, CSVFormat.RFC4180)) {
            for (CSVRecord csvRecord : parser) {
                int[] row = new int[csvRecord.size()];
                for (int i=0; i<row.length;i++)
                {
                    row[i] = Integer.parseInt(csvRecord.get(i));
                }
                result.add(row);
            }
        }
        catch(IOException iox) {
            System.out.println("Failed to parse: " + filePath);
            iox.printStackTrace();
        }

        return result;
    }

    public static void SaveTxtInts(String filePath, List<int[]> data, char colDelimiter, String lineDelimiter)
    {
        try(FileWriter sw = new java.io.FileWriter(filePath))
        {
            for (int i = 0; i < data.size(); i++)
            {
                int[] line = data.get(i);
                for (int j = 0; j < line.length; j++)
                {
                    sw.write(String.valueOf(line[j]));
                    if (j < line.length - 1)
                        sw.write(colDelimiter);
                }
                sw.write(lineDelimiter);
            }
        }
        catch(Exception ex)
        {
            ex.printStackTrace();
        }
    }

    public static void SaveTxtFloats(String filePath, List<float[]> data, char colDelimiter, String lineDelimiter)
    {
        try(FileWriter sw = new java.io.FileWriter(filePath))
        {
            for (int i = 0; i < data.size(); i++)
            {
                float[] line = data.get(i);
                for (int j = 0; j < line.length; j++)
                {
                    sw.write(String.valueOf(line[j]));
                    if (j < line.length - 1)
                        sw.write(colDelimiter);
                }
                sw.write(lineDelimiter);
            }
        }
        catch(Exception ex)
        {
            ex.printStackTrace();
        }
    }

    public static void SaveTxtDoubles(String filePath, List<double[]> data, char colDelimiter, String lineDelimiter)
    {
        try(FileWriter sw = new java.io.FileWriter(filePath))
        {
            for (int i = 0; i < data.size(); i++)
            {
                double[] line = data.get(i);
                for (int j = 0; j < line.length; j++)
                {
                    sw.write(String.valueOf(line[j]));
                    if (j < line.length - 1)
                        sw.write(colDelimiter);
                }
                sw.write(lineDelimiter);
            }
        }
        catch(Exception ex)
        {
            ex.printStackTrace();
        }
    }

    public static void SaveTxtMapInts(String filePath, Map<Integer, Integer> data, char colDelimiter, String lineDelimiter)
    {
        try(FileWriter w = new FileWriter(filePath)) {
            Iterator it = data.entrySet().iterator();
            while (it.hasNext()) {
                Map.Entry pair = (Map.Entry) it.next();
                String line = String.format("%s%s %s%s",
                        pair.getKey().toString(), colDelimiter, pair.getValue().toString(), lineDelimiter);
                w.write(line);
            }
        } catch(IOException iox) {
            iox.printStackTrace();
        }
    }
}
