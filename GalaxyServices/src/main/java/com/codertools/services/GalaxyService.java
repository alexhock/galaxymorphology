package com.codertools.services;

import com.codertools.StaticResourceConfiguration;
import com.codertools.cache.GalaxyDataStore;
import com.codertools.models.*;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Component;
import org.springframework.stereotype.Service;

import javax.inject.Inject;
import java.util.ArrayList;
import java.util.List;

/**
 * Created by ah14aeb on 11/11/2016.
 */
@Component
public class GalaxyService {

    private static final Logger LOG = LoggerFactory.getLogger(StaticResourceConfiguration.class);

    @Inject
    private GalaxyDataStore dataStore;

    public List<OutGalaxy> doRaDecSearch(double ra, double dec, double maxSeparation) {

        List<OutGalaxy> galaxies = new ArrayList<>();

        List<GalaxyNearFind> raGalaxies = dataStore.find(ra, dec, maxSeparation);

        // fast forward through them
        for(GalaxyNearFind galaxyNF : raGalaxies) {
            GalaxyKey key = galaxyNF.getGalaxyKey();
            Galaxy galaxy = dataStore.getGalaxy(key.getKey());
            if (!galaxy.getHasPhotometry())
                continue;
            OutGalaxy outGalaxy = new OutGalaxy(galaxy);
            outGalaxy.setSeparation(galaxyNF.getSeparation());
            galaxies.add(outGalaxy);
        }

        return galaxies;
    }

    public List<OutGalaxy> getSimilar(GalaxyKey galaxyKey) {

        Galaxy galaxy = dataStore.getGalaxy(galaxyKey);
        if (galaxy == null)
            return null;

        if (galaxy.getIndex() != galaxyKey.getIndex()) {
            LOG.info(String.format("Diff indexes Index of request %s index of cache %s fieldId %s objectId %s ",
                    galaxyKey.getIndex(), galaxy.getIndex(), galaxyKey.getFieldId(), galaxyKey.getObjectId()));
        }

        List<Galaxy> cacheGalaxies = dataStore.getNNs(galaxy);

        return convertToOutGalaxy(cacheGalaxies);

    }

    public static List<OutGalaxy> convertToOutGalaxy(List<Galaxy> cacheGalaxies) {
        return convertToOutGalaxy(cacheGalaxies, cacheGalaxies.size());
    }

    public static List<OutGalaxy> convertToOutGalaxy(List<Galaxy> cacheGalaxies, int max) {
        List<OutGalaxy> galaxies = new ArrayList<>();

        if (max == -1 || max > cacheGalaxies.size())
            max = cacheGalaxies.size();

        for (int i=0;i<max;i++) {
            Galaxy cacheGalaxy = cacheGalaxies.get(i);
            if (!cacheGalaxy.getHasPhotometry())
                continue;
            OutGalaxy outGalaxy = new OutGalaxy(cacheGalaxy);
            galaxies.add(outGalaxy);
        }
        return galaxies;
    }

    public List<GalaxyClass> getAllClasses(int level, int rows, int from, int to) {

        ArrayList<GalaxyClass> classes = new ArrayList<>();

        int maxClass = (level + 1) * 10;

        if (to > maxClass)
            to = maxClass;

        for(int i=from;i<to;i++) {
            GalaxyClass c = new GalaxyClass(level, i, rows);
            classes.add(c);
        }

        return classes;
    }

    public List<GalaxyClass> getClasses(int classNum, int level, int requestedLevel) {

        ArrayList<GalaxyClass> classes = new ArrayList<>();

        int topLevel = 1;
        requestedLevel = 6;
/*
        List<Integer> subClassIds = getSubClasses(topLevel, levelRequested);
        for(int classId : subClassIds) {
            GalaxyClass gc = new GalaxyClass(requestedLevel, classId);

        }
*/
        return classes;
    }

    public List<OutGalaxy> getGalaxiesForClassAndLevel(int classId, int levelId, int max) {

        List<Galaxy> galaxies = dataStore.getGalaxiesForClassAndLevel(classId, levelId);
        if (galaxies == null)
            return new ArrayList<OutGalaxy>();

        return convertToOutGalaxy(galaxies, max);
    }

}
