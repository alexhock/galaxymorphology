package com.codertools.utils;

//import java.math.;

/**
 * Created by ah14aeb on 11/11/2016.
 */
public class AstrometryUtils {

    // all in degrees
    // replicated from http://www.stsci.edu/~ferguson/software/pygoodsdist/pygoods/angsep.py
    public static double calcAngularSeparation(double ra1deg, double dec1deg, double ra2deg, double dec2deg) {

        // Determine separation in degrees between two celestial objects
        // arguments are RA and Dec in decimal degrees.

        double ra1rad = ra1deg*Math.PI/180;
        double dec1rad = dec1deg*Math.PI/180;
        double ra2rad = ra2deg*Math.PI/180;
        double dec2rad = dec2deg*Math.PI/180;

        // calculate scalar product for determination
        // of angular separation
        double x = Math.cos(ra1rad) * Math.cos(dec1rad) * Math.cos(ra2rad) * Math.cos(dec2rad);
        double y = Math.sin(ra1rad) * Math.cos(dec1rad) * Math.sin(ra2rad) * Math.cos(dec2rad);
        double z = Math.sin(dec1rad) * Math.sin(dec2rad);

        double rad = Math.acos(x + y + z); // arccos(x+y+z); // Sometimes gives warnings when coords match

        // use Pythargoras approximation if rad < 1 arcsec
        double sep = 0.0d;
        //double sep = choose( rad < 0.000004848 , (Math.sqrt((Math.cos(dec1rad)*(ra1rad-ra2rad))**2+(dec1rad-dec2rad)**2),rad))
        if (rad > 0.000004848) {
            double l = Math.pow((Math.cos(dec1rad)*(ra1rad-ra2rad)), 2);
            double r = Math.pow((dec1rad-dec2rad), 2);
            sep = Math.sqrt(l + r);
            //sep = Math.sqrt((Math.cos(dec1rad)*(ra1rad-ra2rad))**2+(dec1rad-dec2rad)**2);
        }
        else {
            sep = rad;
        }

        // Angular separation
        sep = sep * 180/Math.PI;

        return sep;
    }

    public static void main(String[] args) {
        double sep = calcAngularSeparation(-57.1, 128.3, -54.3, 130.0);
        double sep2 = calcAngularSeparation(-57.1, 128.3, -57.1004, 128.3);

        System.out.println(sep);
        System.out.println(sep2);
    }
}
/*
http://www.stsci.edu/~ferguson/software/pygoodsdist/pygoods/angsep.py
#!/usr/bin/python
        # angsep.py
        # Program to calculate the angular separation between two points
        # whose coordinates are given in RA and Dec
        # From angsep.py Written by Enno Middelberg 2001

        from numpy import *
        import string
        import sys

        def angsep(ra1deg,dec1deg,ra2deg,dec2deg):
        """ Determine separation in degrees between two celestial objects
        arguments are RA and Dec in decimal degrees.
        """
        ra1rad=ra1deg*pi/180
        dec1rad=dec1deg*pi/180
        ra2rad=ra2deg*pi/180
        dec2rad=dec2deg*pi/180

        # calculate scalar product for determination
        # of angular separation

        x=cos(ra1rad)*cos(dec1rad)*cos(ra2rad)*cos(dec2rad)
        y=sin(ra1rad)*cos(dec1rad)*sin(ra2rad)*cos(dec2rad)
        z=sin(dec1rad)*sin(dec2rad)

        rad=arccos(x+y+z) # Sometimes gives warnings when coords match

        # use Pythargoras approximation if rad < 1 arcsec
        sep = choose( rad<0.000004848 , (
        sqrt((cos(dec1rad)*(ra1rad-ra2rad))**2+(dec1rad-dec2rad)**2),rad))

        # Angular separation
        sep=sep*180/pi

        return sep
*/