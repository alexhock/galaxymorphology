package com.codertools.models;

import java.util.Comparator;

/**
 * Created by ah14aeb on 11/11/2016.
 */
public class Galaxy implements Comparable<Double>{

    private int index;
    private byte fieldId;
    private int objectId;
    private int skeltonObjectId;
    private int numPatches;
    private double ra;
    private double dec;
    private int[] classifications;
    private double[] proxes;
    private int[] nnIndexes;
    private double[] nnProxes;
    //private double[] skeltonGalaxy;
    private double f160w;
    private double f125w;
    private double f814w;
    private double f606w;
    private double f435w;
    private double kronRadius;
    private double zPhoto;
    private boolean hasPhotometry = false;

    public double getF160w() { return f160w; }
    public double getF125w() { return f125w; }
    public double getF814w() { return f814w; }
    public double getF606w() { return f606w; }
    public double getF435w() { return f435w; }
    public double getKronRadius() { return kronRadius; }
    public double getzPhoto() { return zPhoto; }
    public boolean getHasPhotometry() { return this.hasPhotometry; }

    public void setF160w(double f160w) { this.f160w = f160w; }
    public void setF125w(double f125w) { this.f125w = f125w; }
    public void setF606w(double f606w) { this.f606w = f606w; }
    public void setF814w(double f814w) { this.f814w = f814w; }
    public void setF435w(double f435w) { this.f435w = f435w; }
    public void setKronRadius(double kronRadius) { this.kronRadius = kronRadius; }
    public void setzPhoto(double zPhoto) { this.zPhoto = zPhoto; }
    public void setHasPhotometry(boolean hasPhotometry) { this.hasPhotometry = hasPhotometry; }


    public Galaxy(String id){
        this.objectId = Integer.parseInt(id);
    }
    public Galaxy(
            int index, byte fieldId, int objectId, int skeltonObjectId, int numPatches, double ra,
            double dec, int[] classifications, double[] proxes) {
            //int[] nns, double[] nnProxes, double[] skeltonGalaxy) {
        this.index = index;
        this.fieldId = fieldId;
        this.objectId = objectId;
        this.skeltonObjectId = skeltonObjectId;
        this.numPatches = numPatches;
        this.ra = ra;
        this.dec = dec;
        this.classifications = classifications;
        this.proxes = proxes;
        /*this.nns = nns;
        this.nnProxes = nnProxes;
        this.skeltonGalaxy = skeltonGalaxy;*/
    }

    public void setNNs(int[] nnIndexes) {
        this.nnIndexes = nnIndexes;
    }

    public String getKey() {
        String sFieldId = String.valueOf(this.fieldId);
        String sObjectId = String.valueOf(this.objectId);
        return sFieldId + sObjectId;
    }

    public int getIndex() { return this.index; }
    public byte getFieldId() { return this.fieldId; }
    public int getObjectId() { return this.objectId; }
    public int getSkeltonObjectId() { return this.skeltonObjectId; }
    public int getNumPatches() { return this.numPatches; }
    public double getRa() { return this.ra; }
    public double getDec() { return this.dec; }
    public int[] getClassifications() { return this.classifications; }
    public double[] getProxes() { return this.proxes; }
    //public double[] getSkeltonGalaxy() { return this.skeltonGalaxy; }
    public int[] getNNIndexes() { return this.nnIndexes; }
    public double[] getNnProxes() { return this.nnProxes; }

    public int compareTo(Double ra) {
        return Double.compare(this.ra, ra);
    }

}
