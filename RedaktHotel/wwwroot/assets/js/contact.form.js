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

    var $form = $('#contact-form');

    $form.submit(function (e) {
        // remove the error class
        $('#contact-form input').removeAttr('style');

        // get the form data
        var formData = {
            'name' : $('input[name="form-name"]').val(),
            'surname' : $('input[name="form-surname"]').val(),
            'phone' : $('input[name="form-phone"]').val(),
            'email' : $('input[name="form-email"]').val(),
            'message' : $('textarea[name="form-message"]').val()
        };

        // process the form
        $.ajax({
            type : 'POST',
            url  : 'contact.php',
            data : formData,
            dataType : 'json',
            encode : true
        }).done(function (data) {
            // handle errors
            if (!data.success) {
                if (data.errors.name) {
                    $('#form-name').css('border-color','#ff0000');
                }
                
                if (data.errors.surname) {
                    $('#form-surname').css('border-color','#ff0000');
                }
                
                if (data.errors.phone) {
                    $('#form-phone').css('border-color','#ff0000');
                }

                if (data.errors.email) {
                    $('#form-email').css('border-color','#ff0000');
                }

                if (data.errors.message) {
                    $('#form-message').css('border-color','#ff0000');
                }
            } else {
                // display success message
                $form.html('<div class="alert alert-success">' + data.message + '</div>');
            }
        }).fail(function (data) {
            // for debug
            console.log(data)
        });

        e.preventDefault();
    });
}(jQuery, window, document));
