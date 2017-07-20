package com.codertools.models;

import java.util.Comparator;

/**
 * Created by ah14aeb on 16/11/2016.
 */
public class GalaxyKeyComparator implements Comparator<GalaxyKey> {

    private boolean compRA = true;

    public GalaxyKeyComparator(boolean compRA) {
        this.compRA = compRA;
    }

    // compare ra
    public int compare(GalaxyKey key1, GalaxyKey key2) {

        Double dec1 = key1.getDec();
        Double dec2 = key2.getDec();
        Double ra1 = key1.getRa();
        Double ra2 = key2.getRa();

        //if (key1.getFieldId() == key2.getFieldId() && key1.getObjectId() == key2.getObjectId())
        //    return 0;
        if (compRA){

            int retVal = ra1.compareTo(ra2);
            if(retVal != 0)
                return retVal;

            // ra is equal then compare dec
            return dec1.compareTo(dec2);
        }

        // comp Dec
        int retVal = dec1.compareTo(dec2);
        if (retVal != 0)
            return retVal;
        // if dec is equal then compare ra
        return ra1.compareTo(ra2);
    }
}
