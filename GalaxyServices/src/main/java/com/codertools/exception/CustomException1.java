package com.codertools.exception;


import org.springframework.http.HttpStatus;
import org.springframework.web.bind.annotation.ResponseStatus;

@ResponseStatus(value= HttpStatus.NOT_FOUND, reason="No such Order")
public class CustomException1 extends BaseException {


    public CustomException1() {
        super(ErrorCodes.CUSTOM_EXCEPTION_1);
    }
}