package com.codertools.models;

import java.util.Comparator;

/**
 * Created by ah14aeb on 02/01/2017.
 */
public class GalaxyComparator  implements Comparator<Galaxy> {

    private int levelId = -1;

    public GalaxyComparator(int levelId) {
        this.levelId = levelId;
    }

    // compare ra
    public int compare(Galaxy galaxy1, Galaxy galaxy2) {

        Double proximity1 = galaxy1.getProxes()[levelId];
        Double proximity2 = galaxy2.getProxes()[levelId];

        int retVal = proximity1.compareTo(proximity2);

        return  retVal;

    }
}
