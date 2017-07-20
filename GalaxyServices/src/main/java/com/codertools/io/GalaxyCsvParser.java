package com.codertools.io;

import com.codertools.cache.GalaxyDataStore;
import com.codertools.models.Galaxy;
import org.apache.commons.csv.CSVFormat;
import org.apache.commons.csv.CSVParser;
import org.apache.commons.csv.CSVRecord;
import org.springframework.stereotype.Component;

import javax.inject.Inject;
import java.io.FileReader;
import java.io.IOException;
import java.io.Reader;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

/**
 * Created by ah14aeb on 11/11/2016.
 */
@Component
public class GalaxyCsvParser {

    @Inject
    public GalaxyDataStore dataStore;

    //public GalaxyCsvParser() {}

    public ArrayList<Galaxy> loadCatalogData(String filePath) {
        //String catalogPath = dbFolderPath + "/final_classifications_debug.csv";
        ArrayList<double[]> data = LoadDoubles(filePath);

        ArrayList<Galaxy> galaxies = new ArrayList<>();

        //0,7,33,84963,12410,1577,189.154924,62.094161,2,12411.882,1575.068,189.154857,62.094129,2,0,0,17,17,8,66,32,32,32,15,118,118,118,118,118,118,118,118,118,0.9933,0.99211,0.98781,0.97762,0.97762,0.97762,0.97021,0.97021,0.97021,0.97021,0.97021,0.92414,0.92414,0.92414,0.92414,0.92414,0.92414,0.92414,0.92414,0.92414
        for (int i = 0; i < data.size(); i++) {
            double[] row = data.get(i);

            byte fieldId = (byte) row[0];
            int numPatches = (int) row[1];
            int wh = (int) row[2];
            int objectId = (int) row[3];
            double x = row[4];
            double y = row[5];
            double cra = row[6];
            double cdec = row[7];
            int sObjectId = (int) row[8];
            double sx = row[9];
            double sy = row[10];
            double sra = row[11];
            double sdec = row[12];

            int[] classifications = new int[20];
            for (int j = 13; j < 33; j++) // classifications
                classifications[j - 13] = (int) row[j];

            double[] proxes = new double[20];
            for (int j = 33; j < 53; j++)
                proxes[j - 33] = row[j];

            Galaxy g = new Galaxy(i, fieldId, objectId, sObjectId, numPatches, sra, sdec, classifications, proxes);
            galaxies.add(g);

        }

        return galaxies;
    }

    public void loadNNData(String filePath, ArrayList<Galaxy> galaxies) {

        ArrayList<int[]> data = LoadInts(filePath);

        int rowSize = data.get(0).length;
        int numNNsIndex = 5;
        int numNNs = rowSize - numNNsIndex;

        // 0,1,84963,0,0,52278,18,52006,527
        for (int i = 0; i < data.size(); i++) {
            int[] galRow = data.get(i);
            int fieldId = galRow[0];
            int objectId = galRow[2];
            int index = galRow[3];

            //String key = String.valueOf(fieldId) + String.valueOf(objectId);
            Galaxy gal = galaxies.get(index);

            if (gal.getObjectId() != objectId) {
                System.out.println("Object is different!!!!");
            }
            if (gal.getFieldId() != fieldId) {
                System.out.println("Object field is different!!!");
            }
            if (index != i)
                System.out.println("ERROR");
            //if (galRow[6] == i)
            //    System.out.println("ERROR");
            int[] nns = new int[numNNs];
            System.arraycopy(galRow, numNNsIndex, nns, 0, numNNs);
            gal.setNNs(nns);
        }

    }

    public void loadSkeltonData(String filePath, HashMap<String, Galaxy> galaxies) {
        //field_id,id_1,label,x_1,y_1,ra_1,dec_1,numpixels,w_h,id_2,x_2,y_2,ra_2,dec_2,f_f160w,f_f125w,f_f814w,f_f606w,f_f435w,kron_radius,z_p
        ArrayList<double[]> data = LoadDoubles(filePath);

        //field_id,id_1,label,x_1,y_1,ra_1,dec_1,numpixels,w_h,id_2,x_2,y_2,ra_2,dec_2,f_f160w,f_f125w,f_f814w,f_f606w,f_f435w,kron_radius,z_p
        for (int i = 0; i < data.size(); i++) {
            double[] galRow = data.get(i);
            int fieldId = (int) galRow[0];
            int objectId = (int) galRow[1];

            int x = (int) galRow[3];
            int y = (int) galRow[4];

            int numPixels = (int) galRow[7];
            int skeltonObjectId = (int) galRow[9];
            double ra = galRow[12];
            double dec = galRow[13];
            double f160w = galRow[14];
            double f125w = galRow[15];
            double f814w = galRow[16];
            double f606w = galRow[17];
            double f435w = galRow[18];
            double kronRadius = galRow[19];
            double z_p = galRow[20];

            String key = String.valueOf(fieldId) + String.valueOf(objectId);
            Galaxy gal = galaxies.get(key);

            if (gal.getObjectId() != objectId)
                System.out.println("Object is different!!!!");
            if (gal.getFieldId() != fieldId)
                System.out.println("Object field is different!!!");
            double decDiff = Math.abs(gal.getDec() - dec);
            double raDiff = Math.abs(gal.getRa() - ra);
            if (decDiff > 0.001d)
                System.out.println("Object dec is different!!!" + decDiff);
            if (raDiff > 0.001d)
                System.out.println("Object ra is different!!!" + raDiff);

            gal.setF160w(f160w);
            gal.setF125w(f125w);
            gal.setF814w(f814w);
            gal.setF606w(f606w);
            gal.setF435w(f435w);
            gal.setKronRadius(kronRadius);
            gal.setzPhoto(z_p);
            gal.setHasPhotometry(true);

        }


    }

    public void loadGZData(String filePath, HashMap<String, Galaxy> galaxies) {

        ArrayList<double[]> data = LoadDoubles(filePath);
        for (int i = 0; i < data.size(); i++) {
            double[] galRow = data.get(i);
            int fieldId = (int) galRow[85];
            int objectId = (int) galRow[86];

            int cleanSpiral = (int) galRow[20];
            int cleanFeature = (int) galRow[21];
            int cleanSmooth = (int) galRow[22];
            int cleanClumpy = (int) galRow[23];
            int cleanEdgeOn = (int) galRow[23];

            String key = String.valueOf(fieldId) + String.valueOf(objectId);
            Galaxy gal = galaxies.get(key);

            if (gal.getObjectId() != objectId)
                System.out.println("Object is different!!!!");
            if (gal.getFieldId() != fieldId)
                System.out.println("Object field is different!!!");


        }

    }

    public static ArrayList<double[]> LoadDoubles(String filePath) {
        ArrayList<double[]> result = new ArrayList<>();

        try (Reader in = new FileReader(filePath)) {
            Iterable<CSVRecord> records = CSVFormat.RFC4180.parse(in);
            for (CSVRecord record : records) {
                double[] row = new double[record.size()];
                for (int i = 0; i < row.length; i++)
                    row[i] = Double.parseDouble(record.get(i));
                result.add(row);
            }
        } catch (IOException iox) {
            System.out.println("Failed to parse: " + filePath);
            iox.printStackTrace();
        }

        return result;
    }

    public static ArrayList<int[]> LoadInts(String filePath) {
        ArrayList<int[]> result = new ArrayList<>();

        try (Reader in = new FileReader(filePath)) {
            Iterable<CSVRecord> records = CSVFormat.RFC4180.parse(in);
            for (CSVRecord record : records) {
                int[] row = new int[record.size()];
                for (int i = 0; i < row.length; i++)
                    row[i] = Integer.parseInt(record.get(i));
                result.add(row);
            }
        } catch (IOException iox) {
            System.out.println("Failed to parse: " + filePath);
            iox.printStackTrace();
        }

        return result;
    }
}

    /*
    public static ArrayList<double[]> LoadDoubles(String filePath)
    {
        ArrayList<double[]> result = new ArrayList<>();

        try(CSVParser parser = CSVParser.parse(filePath, CSVFormat.RFC4180)) {
            for (CSVRecord csvRecord : parser) {
                double[] row = new double[csvRecord.size()];

                for (int i=0; i<row.length;i++)
                {
                    String dataField = csvRecord.get(i);
                    row[i] = Double.parseDouble(dataField);
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

    public static ArrayList<int[]> LoadInts(String filePath)
    {
        ArrayList<int[]> result = new ArrayList<>();

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

}

    public static ArrayList<float[]> LoadFloats(String filePath)
    {
        ArrayList<float[]> result = new ArrayList<>();

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
*/