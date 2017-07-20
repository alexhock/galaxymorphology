package com.codertools.controller;


import com.codertools.models.GalaxyClass;
import com.codertools.models.OutGalaxy;
import com.codertools.services.GalaxyService;
import org.springframework.stereotype.Controller;
        import org.springframework.ui.Model;
        import org.springframework.web.bind.annotation.RequestMapping;
        import org.springframework.web.bind.annotation.RequestParam;

import javax.inject.Inject;
import java.util.ArrayList;
import java.util.List;

@Controller
public class NavigateController {

    private static int[][] mapper = new int[][] {
            {0, -1},
            {1, 9},
            {2, 11},
            {3, 13},
            {4, 15},
            {5, 17},
            {6, 19}
    };

    @Inject
    GalaxyService galaxyService;

    @RequestMapping("/view")
    public String viewClass(
            @RequestParam(value="class", required=true, defaultValue = "-1") int classId,
            @RequestParam(value="level", required=false, defaultValue="-1") int displayLevelId,
            Model model) {

        int max = 54;

        int levelId = mapper[displayLevelId][1];

        // list of galaxies and details
        List<OutGalaxy> galaxies = galaxyService.getGalaxiesForClassAndLevel(classId, levelId, max);

        model.addAttribute("galaxies", galaxies);
        model.addAttribute("classId", classId);
        model.addAttribute("displayLevelId", displayLevelId);
        return "view";
    }

    @RequestMapping("/browse")
    public String browseClasses(
            @RequestParam(value="class", required=false, defaultValue="-1") int classId,
            @RequestParam(value="level", required=false, defaultValue="6") int displayLevelId,
            @RequestParam(value="reqLevel", required=false, defaultValue="-1") int requestedLevelId,
            @RequestParam(value="rows", required=false, defaultValue="1") int rows,
            @RequestParam(value="from", required=false, defaultValue="0") int from,
            @RequestParam(value="to", required=false, defaultValue="31") int to,
            Model model) {

        // validate
        if (classId < -1 || classId > 200)
            throw new IllegalArgumentException("incorrect class number");
        if (displayLevelId < -1 || displayLevelId > 6)
            throw new IllegalArgumentException("incorrect level number");
        if (requestedLevelId < -1 || requestedLevelId > 6)
            throw new IllegalArgumentException("incorrect level number");
        if (rows > 3)
            rows = 1;
        if (from < 0)
            from = 0;

        int levelId = mapper[displayLevelId][1];

        List<GalaxyClass> classList = new ArrayList<>();
        if (requestedLevelId == -1 && levelId == -1) {
            // get everything for the level
            classList = galaxyService.getAllClasses(levelId, rows, from, to); //, requestedLevel
        } else {
            classList = galaxyService.getAllClasses(levelId, rows, from, to);
        }

        model.addAttribute("class", classId);
        model.addAttribute("displayLevel", displayLevelId);
        model.addAttribute("mosaics", classList);

        return "browse";
    }

}