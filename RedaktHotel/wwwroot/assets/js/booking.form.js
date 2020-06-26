/*
--------------------------------
Ajax Contact Form
--------------------------------
+ https://github.com/pinceladasdaweb/Ajax-Contact-Form
+ A Simple Ajax Contact Form developed in PHP with HTML5 Form validation.
+ Has a fallback in jQuery for browsers that do not support HTML5 form validation.
+ version 1.0.1
+ Copyright 2014 Pedro Rogerio
+ Licensed under the MIT license
+ https://github.com/pinceladasdaweb/Ajax-Contact-Form
*/

(function ($, window, document, undefined) {
    'use strict';

    var $form = $('#booking-form');

    $form.submit(function (e) {
        // remove the error class
        $('#booking-form input, #booking-form select').removeAttr('style');

        // get the form data
        var formData = {
            'room' : $('input[name="form-room"]').val(),
            'checkin' : $('input[name="form-checkin"]').val(),
            'checkout' : $('input[name="form-checkout"]').val(),
            'adults' : $('select[name="form-adults"]').val(),
            'childrens' : $('select[name="form-childrens"]').val(),
            'name' : $('input[name="form-name"]').val(),
            'surname' : $('input[name="form-surname"]').val(),
            'email' : $('input[name="form-email"]').val(),
            'phone' : $('input[name="form-phone"]').val(),
            'address1' : $('input[name="form-address1"]').val(),
            'address2' : $('input[name="form-address2"]').val(),
            'city' : $('input[name="form-city"]').val(),
            'country' : $('input[name="form-country"]').val(),
            'requirements' : $('textarea[name="form-requirements"]').val()
        };

        // process the form
        $.ajax({
            type : 'POST',
            url  : 'booking.php',
            data : formData,
            dataType : 'json',
            encode : true
        }).done(function (data) {
            // handle errors
            if (!data.success) {
                if (data.errors.checkin) {
                    $('#form-checkin').css('border-color','#ff0000');
                }
                
                if (data.errors.checkout) {
                    $('#form-checkout').css('border-color','#ff0000');
                }
                
                if (data.errors.adults) {
                    $('#form-adults').css('border-color','#ff0000');
                }
                
                if (data.errors.name) {
                    $('#form-name').css('border-color','#ff0000');
                }
                
                if (data.errors.surname) {
                    $('#form-surname').css('border-color','#ff0000');
                }
                
                if (data.errors.email) {
                    $('#form-email').css('border-color','#ff0000');
                }
                
                if (data.errors.phone) {
                    $('#form-phone').css('border-color','#ff0000');
                }
            } else {
                // display success message
                $(".booking-form").hide();
                $(".booking-complete").show();
            }
        }).fail(function (data) {
            // for debug
            console.log(data)
        });

        e.preventDefault();
    });
}(jQuery, window, document));
