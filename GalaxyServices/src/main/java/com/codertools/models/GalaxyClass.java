package com.codertools.models;

/**
 * Created by ah14aeb on 28/11/2016.
 */
public class GalaxyClass {

    private int level;
    private int classNum;
    private int rows;
    private static String ext = "png";
    private String title = "title";
    private String alt = "alt";

    public GalaxyClass(int level, int classNum) {
        this(level, classNum, 1);
    }

    public GalaxyClass(int level, int classNum, int rows) {
        this.level = level;
        this.classNum = classNum;
        this.rows = rows;
    }

    public int getLevel() {
        return this.level;
    }

    public int getClassNum() {
        return this.classNum;
    }

    public int getRows() {
        return this.rows;
    }

    public String getExt() {
        return this.ext;
    }
    public String getUrl() {
        return String.format("mosaic_%s_%s.%s", this.classNum, this.rows, this.ext);
    }

    public String getLink() {
        return String.format("browse/?level=%s&class=%s&reqLevel=%s", this.level, this.classNum, this.classNum+1);
    }

    public String getTitle() {
        return this.title;
    }

    public String getAlt() {
        return this.alt;
    }
}
