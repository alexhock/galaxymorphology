package com.codertools.models;

import java.util.Comparator;

/**
 * Created by ah14aeb on 03/01/2017.
 */
public class GalaxyNearFindComparator  implements Comparator<GalaxyNearFind> {

    public GalaxyNearFindComparator() {

    }

    // compare ra
    public int compare(GalaxyNearFind find1, GalaxyNearFind find2) {

        Double sep1 = find1.getSeparation();
        Double sep2 = find2.getSeparation();

        return sep1.compareTo(sep2);
    }
}