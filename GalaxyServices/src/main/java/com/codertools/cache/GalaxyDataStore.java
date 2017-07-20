package com.codertools.cache;

import com.codertools.models.*;
import com.codertools.utils.AstrometryUtils;
import org.springframework.stereotype.Component;

import javax.inject.Singleton;
import java.util.*;

@Component
public class GalaxyDataStore {

    private static ArrayList<Galaxy> allGalaxies;
    private static ArrayList<GalaxyKey> galPositions;
    private static ArrayList<GalaxyKey> galPositionsSortedByRa = new ArrayList<>();
    private static ArrayList<GalaxyKey> galPositionsSortedByDec = new ArrayList<>();
    private static HashMap<String, Galaxy> galaxyMap;

    // should set from configuration: which levels from the catalogue do we want
    // to include
    private static int[] levelIds = new int[] { 9, 15, 19};

    //private static HashMap<Integer, List<Integer>> level1Mapping;
    //private static HashMap<Integer, List<Galaxy>> level6GalaxiesByClass;

    // Keys are: LevelId, ClassId, returns list of galaxies sorted by proximity.
    private static HashMap<Integer, HashMap<Integer, List<Galaxy>>>
            galaxiesByLevelAndClass = new HashMap<>();

    public GalaxyDataStore()
    {

    }

    public void setGalaxies(ArrayList<Galaxy> galaxies) {
        GalaxyDataStore.allGalaxies = galaxies;
    }

    public void setGalaxiesMap(HashMap<String, Galaxy> galMap) {
        GalaxyDataStore.galaxyMap = galMap;
    }

    public static HashMap<Integer, List<Galaxy>> createClassGalaxyMapping(int level) {

        HashMap<Integer, List<Galaxy>> levelMapping = new HashMap<>();

        for (Galaxy gal : allGalaxies) {
            int[] classifications = gal.getClassifications();
            int levelClass = classifications[level];

            List<Galaxy> galaxyClassList = levelMapping.putIfAbsent(levelClass, new ArrayList<>());
            if (galaxyClassList == null) // happens if key was absent
                galaxyClassList = levelMapping.get(levelClass);
            galaxyClassList.add(gal);
        }

        return levelMapping;
    }

    public static List<Galaxy> sortGalaxiesByProximity(List<Galaxy> galaxies, int levelId) {

        GalaxyComparator gc = new GalaxyComparator(levelId);
        galaxies.sort(gc);

        return galaxies;
    }

    public void createIndexes() {

        // create sorted galaxy key indices -- used for near position search
        for(int i=0; i<allGalaxies.size();i++) {
            Galaxy gal = allGalaxies.get(i);
            GalaxyKey key = new GalaxyKey(i, gal.getFieldId(), gal.getObjectId(), gal.getRa(), gal.getDec());
            galPositionsSortedByRa.add(key);
            galPositionsSortedByDec.add(key);
        }

        // sort the galaxies for binary search
        galPositionsSortedByRa.sort(new GalaxyKeyComparator(true));
        galPositionsSortedByDec.sort(new GalaxyKeyComparator(false));
        galPositions = galPositionsSortedByRa;

        //////////////////////////////////////////
        // create level class list of galaxies mapping. Used for browsing the catalogue.
        // keys levelid, then classid
        for(int levelId : levelIds) {
            HashMap<Integer, List<Galaxy>> classGalaxies = createClassGalaxyMapping(levelId);
            for (List<Galaxy> galaxies : classGalaxies.values()) {
                sortGalaxiesByProximity(galaxies, levelId);
            }
            galaxiesByLevelAndClass.put(levelId, classGalaxies);
        }

    }

    public Galaxy getGalaxy(GalaxyKey galaxyKey) {
        return galaxyMap.get(galaxyKey.getKey());
    }
    public Galaxy getGalaxy(String key) {
        return galaxyMap.get(key);
    }


    public List<Galaxy> getGalaxiesForClassAndLevel(int classId, int levelId) {
        return galaxiesByLevelAndClass.get(levelId).get(classId);
    }

    public List<Galaxy> getNNs(Galaxy galaxy) {
        return getNNs(galaxy.getNNIndexes());
    }

    public List<Galaxy> getNNs(int[] nns) {
        List<Galaxy> galaxies = new ArrayList<>();
        for(int index : nns) {
            galaxies.add(allGalaxies.get(index));
        }
        return galaxies;
    }

    public List<Galaxy> getNNs(GalaxyKey galaxyKey) {
        Galaxy galaxy = getGalaxy(galaxyKey);
        return getNNs(galaxy);
    }

    public List<GalaxyNearFind> find2(double ra, double dec, double maxSeparation) {

        HashMap<String, GalaxyNearFind> galaxies = new HashMap<>();

        // get index for sorted RA
        int raIndex = getIndexOfNearest2(galPositionsSortedByRa, ra, dec, new GalaxyKeyComparator(true));
        // get index for sorted Dec
        int decIndex = getIndexOfNearest2(galPositionsSortedByDec, ra, dec, new GalaxyKeyComparator(false));

        loopBackAndForward(galPositionsSortedByRa, galaxies, raIndex, ra, dec, maxSeparation, true);

        loopBackAndForward(galPositionsSortedByDec, galaxies, decIndex, ra, dec, maxSeparation, false);

        //  remove duplicates -- dont need to sort, is done on client using sep.

        List<GalaxyNearFind> nearestGalaxies = new ArrayList<>();
        for(GalaxyNearFind gnf : galaxies.values())
            nearestGalaxies.add(gnf);

        nearestGalaxies.sort(new GalaxyNearFindComparator());

        return nearestGalaxies;
    }

    interface LoopCheck {
        boolean check(int indexFrom, int limit);
    }

    private int loopBackAndForward(
            List<GalaxyKey> sortedPositions,
            HashMap<String, GalaxyNearFind> nearestGalaxies,
            int initialIndex,
            double ra, double dec, double maxSeparation,
            boolean searchRA) {

        //LoopCheck loopCheck = (int indexFrom, int limit) -> (indexFrom > limit );

        // loop back
        int indexFrom = initialIndex;
        int checkCount = 0;
        double tempSep = 0.0d;
        while (indexFrom > 0 && tempSep < maxSeparation)
        {
            GalaxyKey tempPos = sortedPositions.get(indexFrom);

            double raDist = Math.abs(tempPos.getRa() - ra);
            double decDist = Math.abs(tempPos.getDec() - dec);

            if (raDist < maxSeparation && decDist < maxSeparation) {
                double galSeparation = AstrometryUtils.calcAngularSeparation(ra, dec, tempPos.getRa(), tempPos.getDec());
                if (galSeparation <= maxSeparation)
                    nearestGalaxies.put(tempPos.getKey(), new GalaxyNearFind(galSeparation, tempPos));
            }

            tempSep = searchRA ? raDist : decDist;
            checkCount++;
            indexFrom--;
        }

        // loop forward
        indexFrom = initialIndex + 1;
        tempSep = 0.0d;
        while (indexFrom < sortedPositions.size() && tempSep < maxSeparation) {

            GalaxyKey tempPos = sortedPositions.get(indexFrom);

            double raDist = Math.abs(tempPos.getRa() - ra);
            double decDist = Math.abs(tempPos.getDec() - dec);

            if (raDist < maxSeparation && decDist < maxSeparation) {
                double galSeparation = AstrometryUtils.calcAngularSeparation(ra, dec, tempPos.getRa(), tempPos.getDec());
                if (galSeparation <= maxSeparation)
                    nearestGalaxies.put(tempPos.getKey(), new GalaxyNearFind(galSeparation, tempPos));
            }

            tempSep = searchRA ? raDist : decDist;
            checkCount++;
            indexFrom++;
        }

        return checkCount;
    }

    private GalaxyNearFind getGalaxyNearFind(
            GalaxyKey tempPos,
            double ra, double dec, double maxSeparation) {

        GalaxyNearFind gnf = null;

        double raDist = Math.abs(tempPos.getRa() - ra);
        double decDist = Math.abs(tempPos.getDec() - dec);

        if (raDist < maxSeparation && decDist < maxSeparation) {
            double galSeparation = AstrometryUtils.calcAngularSeparation(ra, dec, tempPos.getRa(), tempPos.getDec());
            if (galSeparation <= maxSeparation)
                gnf = new GalaxyNearFind(galSeparation, tempPos);
        }

        return gnf;
    }

    private int getIndexOfNearest2(
            ArrayList<GalaxyKey> sortedKeys,
            double ra,
            double dec,
            GalaxyKeyComparator keyComp) {

        GalaxyKey key = new GalaxyKey(ra, dec);
        int indexFrom = Collections.binarySearch(
                sortedKeys, key, keyComp);
        if (indexFrom < 0) {
            int indexOfNearest = ~indexFrom;
            if (indexOfNearest == sortedKeys.size())
                return sortedKeys.size() - 1;

            if (indexOfNearest == 0)
                indexFrom = 0;
            else
                // from time is between (indexOfNearest - 1) and indexOfNearest
                indexFrom = indexOfNearest;
        }
        return indexFrom;
    }


    public List<GalaxyNearFind> find(double ra, double dec, double maxSeparation) {

        ArrayList<GalaxyNearFind> nearestGalaxies = new ArrayList<>();

        // get index for sorted RA
        int raIndex = getIndexOfNearest(galPositions, ra, dec);

        // loop back
        int indexFrom = raIndex;
        double galSeparation = 0.0d;
        double raSep = ra;
        int checkCount = 0;
        while (indexFrom > 0 && Math.abs(raSep - ra) <= maxSeparation)
        {
            GalaxyKey tempPos = galPositions.get(indexFrom);

            // if within dec then
            if (Math.abs(tempPos.getDec() - dec) <= maxSeparation) {
                // calc the galaxy separation
                galSeparation = AstrometryUtils.calcAngularSeparation(ra, dec, tempPos.getRa(), tempPos.getDec());
                if (galSeparation <= maxSeparation)
                    nearestGalaxies.add(new GalaxyNearFind(galSeparation, tempPos));
                checkCount++;
            }

            raSep = tempPos.getRa();
            indexFrom--;
        }

        indexFrom = raIndex + 1;
        galSeparation = 0.0d;
        raSep = ra;
        while (indexFrom < galPositions.size() && Math.abs(raSep - ra) <= maxSeparation)
        {
            GalaxyKey tempPos = galPositions.get(indexFrom);

            if (Math.abs(tempPos.getDec() - dec) <= maxSeparation) {
                galSeparation = AstrometryUtils.calcAngularSeparation(ra, dec, tempPos.getRa(), tempPos.getDec());
                if (galSeparation <= maxSeparation)
                    nearestGalaxies.add(new GalaxyNearFind(galSeparation, tempPos));
                checkCount++;
            }
            raSep = tempPos.getRa();
            indexFrom++;
        }

        System.out.println(checkCount);

        nearestGalaxies.sort(new GalaxyNearFindComparator());

        return nearestGalaxies;
    }

    private int getIndexOfNearest(ArrayList<GalaxyKey> sortedByRa, double ra, double dec) {

        boolean compareRA = true;
        GalaxyKey key = new GalaxyKey(ra, dec);
        int indexFrom = Collections.<GalaxyKey>binarySearch(
                sortedByRa, key, new GalaxyKeyComparator(compareRA));
        if (indexFrom < 0) {
            int indexOfNearest = ~indexFrom;
            if (indexOfNearest == sortedByRa.size())
                return sortedByRa.size() - 1;

            if (indexOfNearest == 0)
                indexFrom = 0;
            else
                // from time is between (indexOfNearest - 1) and indexOfNearest
                indexFrom = indexOfNearest;
        }
        return indexFrom;
    }
/*
    public List<Integer> getLevelClassIds(int level, int parentClassId, int subLevel) {
        return level1Mapping.get(parentClassId);
    }

    public List<Integer> getSubLevelClassIds(int levelId, int classId) {
        return level1Mapping.get(classId);
    }
    */
}


/*
int binary_search(int A[], int key, int imin, int imax)
{
  // test if array is empty
  if (imax < imin)
    // set is empty, so return value showing not found
    return KEY_NOT_FOUND;
  else
    {
      // calculate midpoint to cut set in half
      int imid = midpoint(imin, imax);

      // three-way comparison
      if (A[imid] > key)
        // key is in lower subset
        return binary_search(A, key, imin, imid-1);
      else if (A[imid] < key)
        // key is in upper subset
        return binary_search(A, key, imid+1, imax);
      else
        // key has been found
        return imid;
    }
}
 */