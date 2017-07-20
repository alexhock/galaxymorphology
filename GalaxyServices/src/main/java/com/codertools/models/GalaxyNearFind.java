package com.codertools.models;

/**
 * Created by ah14aeb on 16/11/2016.
 */
public class GalaxyNearFind {

    private double separation;
    private GalaxyKey galaxy;

    public GalaxyNearFind(double separation, GalaxyKey galaxy) {
        this.separation = separation;
        this.galaxy = galaxy;
    }

    public double getSeparation() { return separation; }
    public GalaxyKey getGalaxyKey() { return galaxy; }

}
