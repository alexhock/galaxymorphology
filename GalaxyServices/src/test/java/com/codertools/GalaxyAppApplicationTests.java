package com.codertools;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.test.context.junit4.SpringRunner;

@RunWith(SpringRunner.class)
@SpringBootTest
public class GalaxyAppApplicationTests {

	static {
		System.setProperty("db.path", "K:/web/candels/catalogs/");
        System.setProperty("static.path", "K:/web/candels/");
	}

	@Test
	public void contextLoads() {
	}

}
