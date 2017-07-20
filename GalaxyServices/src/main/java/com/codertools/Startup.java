package com.codertools;

import com.codertools.cache.GalaxyDataStore;
import com.codertools.io.GalaxyCsvParser;
import com.codertools.models.Galaxy;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.boot.ApplicationArguments;
import org.springframework.boot.ApplicationRunner;
import org.springframework.stereotype.Component;
import javax.inject.Inject;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

@Component
public class Startup implements ApplicationRunner {

    private static final Logger LOG = LoggerFactory.getLogger(StaticResourceConfiguration.class);

    @Value("${db.path}")
    private String dbFolderPath;

    @Inject
    private GalaxyDataStore galaxyCache;

    @Inject
    private GalaxyCsvParser galaxyCsvParser;

    public Startup() { }

    public void run(ApplicationArguments args) {

        LOG.info("********************* STARTUP *******************");
        LOG.info("Loading catalog");

        // load catalog data
        String catalogPath = dbFolderPath + "/final_catalogue_debug.csv";
        ArrayList<Galaxy> gals = galaxyCsvParser.loadCatalogData(catalogPath);
        // create indexed gal map
        HashMap<String, Galaxy> galMap = new HashMap<>();
        for(Galaxy gal : gals) {
            galMap.put(gal.getKey(), gal);
        }

        // load nn data
        String nnPath = dbFolderPath + "/finalNN-40.csv";
        LOG.info("Loading nn indexes: " + nnPath);
        galaxyCsvParser.loadNNData(nnPath, gals);

        // load skelton data
        String skeltonPath = dbFolderPath + "/candels_matches.csv";
        LOG.info("Loading Skelton data: " + skeltonPath);
        galaxyCsvParser.loadSkeltonData(skeltonPath, galMap);


        galaxyCache.setGalaxies(gals);
        galaxyCache.setGalaxiesMap(galMap);
        // load gz data

        // create indexes
        galaxyCache.createIndexes();

        LOG.info("********************* FINISHED STARTUP *******************");

    }
}
