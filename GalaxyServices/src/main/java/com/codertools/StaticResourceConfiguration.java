package com.codertools;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Configuration;
import org.springframework.web.servlet.config.annotation.ResourceHandlerRegistry;
import org.springframework.web.servlet.config.annotation.WebMvcConfigurerAdapter;

import java.util.Arrays;

@Configuration
public class StaticResourceConfiguration extends WebMvcConfigurerAdapter {

    private static final Logger LOG = LoggerFactory.getLogger(StaticResourceConfiguration.class);

    @Value("${static.path}")
    private String staticPath;

    private static String[] paths = new String[] {
            "classpath:/META-INF/resources/",
            "classpath:/resources/",
            "classpath:/static/",
            "classpath:/public/"
    };

    @Override
    public void addResourceHandlers(ResourceHandlerRegistry registry) {

        if(staticPath != null) {

            String[] resLocations = new String[paths.length+1];

            resLocations[0] = "file:" + staticPath;
            for(int i=1;i<resLocations.length;i++)
                resLocations[i] = paths[i-1];

            LOG.info("Serving static content from " + Arrays.deepToString(resLocations));

            registry.addResourceHandler("/**").addResourceLocations(resLocations);
        }
        //#spring.resources.static-locations=classpath:/META-INF/resources/,classpath:/resources/,classpath:/static/,classpath:/public/,file:K:/web/candels/ # Locations of static resources.

        // see https://stackoverflow.com/questions/27381781/java-spring-boot-how-to-map-my-my-app-root-to-index-html
        /*@Override
        public void addViewControllers(ViewControllerRegistry registry) {
            registry.addViewController("/").setViewName("redirect:/index.html");
        }*/
    }
}