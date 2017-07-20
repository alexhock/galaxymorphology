package com.codertools;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.context.ApplicationContext;
import org.springframework.web.servlet.DispatcherServlet;

@SpringBootApplication
public class GalaxyAppApplication {

	public static void main(String[] args) {


		ApplicationContext ctx = SpringApplication.run(GalaxyAppApplication.class, args);
		DispatcherServlet dispatcherServlet = (DispatcherServlet)ctx.getBean("dispatcherServlet");
		dispatcherServlet.setThrowExceptionIfNoHandlerFound(true);

		//SpringApplication.run(GalaxyAppApplication.class, args);
	}
}
