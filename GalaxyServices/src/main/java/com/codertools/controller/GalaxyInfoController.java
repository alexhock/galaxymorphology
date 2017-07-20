package com.codertools.controller;

import com.codertools.cache.GalaxyDataStore;
import com.codertools.exception.CustomException1;
import com.codertools.models.Galaxy;
import com.codertools.models.GalaxyKey;
import com.codertools.models.GalaxyNearFind;
import com.codertools.models.OutGalaxy;
import com.codertools.services.GalaxyService;
import org.glassfish.jersey.internal.inject.Custom;
import org.springframework.http.HttpStatus;
import org.springframework.web.bind.annotation.*;

import javax.inject.Inject;
import javax.servlet.http.HttpServletResponse;
import javax.ws.rs.NotFoundException;
import javax.ws.rs.WebApplicationException;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Collection;
import java.util.List;
import java.util.concurrent.ThreadLocalRandom;

/**
 * Created by ah14aeb on 11/11/2016.
 */
@RestController
public class GalaxyInfoController {

    //https://spring.io/guides/tutorials/bookmarks/ testing a rest service

    @Inject
    GalaxyService galaxyService;

    @RequestMapping("/info")
    public Galaxy getGalaxyInfo(@RequestParam(value="id", defaultValue="-1") String id) {
        return new Galaxy(id);
    }

    @RequestMapping(path="/nearsearch")
    public Collection<OutGalaxy> getGalaxiesFromRADEC(
            @RequestParam(value="ra", defaultValue = "-1") String sra,
            @RequestParam(value="dec", defaultValue = "-1") String sdec,
            @RequestParam(value="maxSep", defaultValue = "-1") String smaxSep) {

        if (sra == null || sra.isEmpty())
            throw new IllegalArgumentException("RA is missing");
        if (sdec == null || sdec.isEmpty())
            throw new IllegalArgumentException("DEC is missing");
        if (smaxSep == null || smaxSep.isEmpty())
            throw new IllegalArgumentException("Max Separation is missing");

        // validate
        double ra = -1d;
        double dec = -1d;
        double maxSep = -1d;
        try {
            ra = Double.parseDouble(sra);
            dec = Double.parseDouble(sdec);
            maxSep = Double.parseDouble(smaxSep);
        } catch(NumberFormatException ex) {
            throw new NumberFormatException("Invalid field entries");
        }

        if (maxSep > 5) {
            throw new IllegalArgumentException("Max separation is too large. Reduce to below 2");
        }

        maxSep = maxSep * 0.000277778; // convert to degrees (1 arcsecond = 0.000277778)

        List<OutGalaxy> outGalaxies = galaxyService.doRaDecSearch(ra, dec, maxSep);

/*        try {
            int rand = ThreadLocalRandom.current().nextInt(50, 250 + 1);
            Thread.sleep(rand);
        } catch(Exception ex) {
            System.out.println(ex);
        }
*/
        return outGalaxies;
    }

    @RequestMapping("/similar")
    public Collection<OutGalaxy> getSimilarFromGalaxyKey(
            @RequestParam(value="index", defaultValue = "-1") String inIndex,
            @RequestParam(value="fieldId", defaultValue = "-1") String inFieldId,
            @RequestParam(value="objectId", defaultValue = "-1") String inObjectId) {

        if (inIndex == null || inIndex.isEmpty())
            throw new IllegalArgumentException("RA is missing");
        if (inFieldId == null || inFieldId.isEmpty())
            throw new IllegalArgumentException("DEC is missing");
        if (inObjectId == null || inObjectId.isEmpty())
            throw new IllegalArgumentException("Max Separation is missing");

        // validate
        int index = -1;
        byte fieldId = -1;
        int objectId = -1;
        try {
            index = Integer.parseInt(inIndex);
            fieldId = Byte.parseByte(inFieldId);
            objectId = Integer.parseInt(inObjectId);
        } catch(NumberFormatException ex) {
            throw new NumberFormatException("Invalid field entries");
        }

        GalaxyKey galaxyKey = new GalaxyKey(index, fieldId, objectId);

        List<OutGalaxy> outGalaxies = galaxyService.getSimilar(galaxyKey);
        if (outGalaxies.size() == 0)
            throw new NotFoundException();
/*
        try {
            int rand = ThreadLocalRandom.current().nextInt(50, 250 + 1);
            Thread.sleep(rand);
        } catch(Exception ex) {
            System.out.println(ex);
        }
*/
        return outGalaxies;
    }

    @RequestMapping("/classification")
    public Collection<Galaxy> getGalaxiesFromClassification(
            @RequestParam(value="level", defaultValue = "-1") int level,
            @RequestParam(value="class", defaultValue = "-1") int classification) {

        return new ArrayList<Galaxy>();
    }

    //https://blog.jayway.com/2014/10/19/spring-boot-error-responses/
    @ExceptionHandler
    void handleIllegalArgumentException(IllegalArgumentException e, HttpServletResponse response) throws IOException {
        response.sendError(HttpStatus.BAD_REQUEST.value());
    }

    @ExceptionHandler(value = NumberFormatException.class)
    public String nfeHandler(NumberFormatException e, HttpServletResponse response) throws IOException {
        //response.sendError(HttpStatus.BAD_REQUEST.value());
        response.setStatus(HttpStatus.BAD_REQUEST.value());
        return e.getMessage();
    }

    @ExceptionHandler(value = NotFoundException.class)
    public String nfeHandler(NotFoundException e, HttpServletResponse response) throws IOException {
        response.sendError(HttpStatus.NOT_FOUND.value());
        return e.getMessage();
    }
}
