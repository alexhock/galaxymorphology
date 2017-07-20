package com.codertools.models;

/**
 * Created by ah14aeb on 11/11/2016.
 */
public class SkeltonGalaxy {
    private String skeltonId;
    private float[] photometry;
    private float z_p;
    private float kronRadius;

    public SkeltonGalaxy(String id, float[] photometry, float z_p, float kronRadius) {
        this.skeltonId = id;
        this.photometry = photometry;
        this.z_p = z_p;
        this.kronRadius = kronRadius;
    }

    public String getSkeltonId() { return this.skeltonId; }
    public float[] getPhotometry() { return this.photometry; }
    public float getZ_p() { return this.z_p; }
    public float getKronRadius() { return this.kronRadius; }
}
