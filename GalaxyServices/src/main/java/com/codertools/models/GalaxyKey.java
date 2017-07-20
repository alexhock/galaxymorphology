package com.codertools.models;

import java.util.Comparator;

/**
 * Created by ah14aeb on 16/11/2016.
 */
public class GalaxyKey {

    private String key;
    private int index;
    private byte fieldId;
    private int objectId;
    private double ra;
    private double dec;

    public GalaxyKey(double ra, double dec) {
        this.ra = ra;
        this.dec = dec;
    }

    public GalaxyKey(int index, byte fieldId, int objectId) {
        this(index, fieldId, objectId, -99, -99);
    }

    public GalaxyKey(int index, byte fieldId, int objectId, double ra, double dec) {
        this.index = index;
        this.fieldId = fieldId;
        this.objectId = objectId;
        this.ra = ra;
        this.dec = dec;
        this.key = String.valueOf(fieldId) + String.valueOf(objectId);
    }

    public String getKey() { return this.key; }
    public int getIndex() { return this.index; }
    public byte getFieldId() { return this.fieldId; }
    public int getObjectId() { return this.objectId; }

    public double getRa() { return this.ra; }
    public double getDec() { return this.dec; }

}
/*
    public int compare(Galaxy g1, Galaxy g2) {
        double val1 = 0.0d;
        double val2 = 0.0d;

        if (this.useRa) {
            val1 = g1.getRa();
            val2= g2.getRa();
        } else {
            val1 = g1.getDec();
            val2 = g2.getDec();
        }

        if (val1 == val2) return 0;
        if (val1 > val2) return 1;
        return -1;


    }
 */