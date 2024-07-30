import 'parsleyjs'

export default function Forms() {
    formStyling()
    formSubmission()
    fileUpload()
    // keyValidation() // Disabling for when 2 forms on the page.
}

var _form = $('.umbraco-forms-form form');
_form.parsley();

function formStyling() {
    var inputs = $('input');
    
    inputs.on('input change', function() {
        var _val = $(this).val();
        if (_val.length) {
            $(this).addClass('has-value');
        } else {
            $(this).removeClass('has-value');
        }
    })
}

function formSubmission() {

    _form.on('submit', function (e) {
        e.preventDefault();

       


        var _t = '#' + $(this).parent('div').attr('id');
        var _success = '.umbraco-forms-submitmessage';
        var _form = $(this)[0];

        $(_t).find('input[type=submit], button[type=submit]').addClass('loading').html('<svg class="[ icon icon-loading ]"><use xlink:href="#sprite-icon-loading"></use></svg>');
        $.ajax({
            url: $(this).prop('action'),
            type: 'POST',
            enctype: 'multipart/form-data',
            data: new FormData(_form),
            processData: false,
            contentType: false,
            success: function (data) {
                var _data = $(data);
                var validationFailed = _data.find(_t);
                var formSuccess = _data.find(_success);

                
                if (validationFailed.length > 0) {
                    
                    $(_t).html(validationFailed);

                } else if (formSuccess.length > 0) {
                    $(_t).addClass('hidden');
                    setTimeout(function () {
                        $(_t).html(formSuccess);
                    }, 500)
                }

                setTimeout(function () {
                    $(_t).removeClass('hidden');                      
                }, 500)
            }
        });
    });
}

function fileUpload() {

    $('.file-wrapper .button').on('click', function (e) {
        e.preventDefault();
        $(this).after('<input type="text" readonly name="' + $(this).attr("name") + '" hidden value=""/>');
        $(this).prev('input[type=file]').trigger('click');
    });

    $('.umbraco-forms-field input[type=file]').on('change', function () {
        if ($(this).get(0).files.length !== 0) {
            $('.file-wrapper').find('input:text').val($(this).get(0).files[0].name);
        }

        var _files = $(this).get(0).files;
        var _html = '<li>Chosen files</li>';

        if (_files.length > 0) {
            for(var i = 0; i < _files.length; i++) {
                _html += '<li>' + _files[i].name + '</li>';
            }
    
            $(this).parents('.file-wrapper').next('.uploaded-files').removeClass('hidden').html( _html );
        } else {
            $(this).parents('.file-wrapper').next('.uploaded-files').html('');
        }

    });

}

function keyValidation() {
    _form.validate({
        onkeyup: function (input) {

            $(input).removeClass('valid valid-input invalid-input');
            validateInput(input);

        },
        onfocusout: function (input) {

            validateInput(input);

        }
    })
}

function validateInput(input) {
    if ($(input).data('regex')) {
        var _regex = new RegExp($(input).data('regex'));

        if (_regex.test($(input).val()) && $(input).val().length > 0) {
            $(input).addClass('valid-input');
        } else {
            $(input).removeClass('valid valid-input');
            $(input).addClass('invalid-input');
        }
    } else if ($(input).val().length > 0) {
        $(input).addClass('valid-input');
    } else {
        $(input).removeClass('valid valid-input');
        $(input).addClass('invalid-input');
    }
}