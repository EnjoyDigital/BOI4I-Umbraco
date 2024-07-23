var loanCalcButton = $('#loan-calc-button');

loanCalcButton.on('click', function () {
  calculate()
});

$('input').on('keypress', function (e) {
  if (e.keyCode == 13) {
    loanCalcButton.trigger('click');
  }
})

// validation
function checkFields() {
  var valid = true;

  $('input.form-field-required').each(function () {
    if ($(this).val() === '') {
      valid = false;
    }
  });

  $('select.form-field-required').each(function () {
    if ($(this).val() === null) {
      valid = false;
    }
  });

  return valid;
}


// calculation function
function calculate() {
  checkConditionals()

  $('.re-calculate-error').hide();
  $('.results-wrapper .contents').show();

  if (checkFields()) {
    var overdraftinput = parseFloat($("#overdraft-input").val());
    var arrangedoverdraft = parseFloat($("#arranged-overdraft-input").val());

    if (overdraftinput >= 1 && overdraftinput <= 25000) {
      $('.error-msg.overdraft-field').hide();
    } else {
      $('.error-msg.overdraft-field').show();
      $('.error-msg.error-main').show();
    }

    if (arrangedoverdraft >= 0 && arrangedoverdraft <= 25000) {
      $('.error-msg.arranged-field').hide();
    } else {
      $('.error-msg.arranged-field').show();
      $('.error-msg.error-main').show();
    }

    if (overdraftinput >= 1 && overdraftinput <= 25000 && arrangedoverdraft >= 0 && arrangedoverdraft <= 25000) {
      $('.error-msg').hide();

      runCalculations()

      $('.overdraft-calculator.results').css('display', 'flex');
      $('html, body').animate({ scrollTop: $(".overdraft-calculator.results").offset().top }, 1000, function () {
        $('.overdraft-calculator-header.results').addClass('show');
      });
    } else {
      $('.error-msg.error-main').show();
    }
  } else {
    $('.error-msg.error-main').show();
  }
}

// change link
$(document).on('click', '.changelink', function () {
  $('html, body').animate({ scrollTop: '0px' }, 1000, function () {
    $('.overdraft-calculator-header.results').removeClass('show');
  });
});

// if results error is showing
function checkResultsError() {
  var show = true;

  if ($('.results-wrapper .error-msg.error-main').is(":visible")) {
    show = false;
  }

  return show;
}

//if inputs have changed
function checkResultsChanged() {
  var show = true;

  if ($('.results-wrapper .re-calculate-error').is(":visible")) {
    show = false;
  }

  return show;
}

// scrolling functions
function isScrolledIntoView(elem) {
  var elementTop = $(elem).offset().top + 250;
  var elementBottom = elementTop + $(elem).outerHeight();
  var viewportTop = $(window).scrollTop();
  var viewportBottom = viewportTop + $(window).height();
  return elementBottom > viewportTop && elementTop < viewportBottom;
}

$(window).scroll(function () {
  if (checkResultsError()) {
    $('.results-wrapper .error-msg.error-main').hide();
  } else {
    $('.results-wrapper .error-msg.error-main').show();
  }

  if (checkResultsError() && checkResultsChanged()) {
    $('.results-wrapper .contents').show();
  } else {
    $('.results-wrapper .contents').hide();
  }

  if ($('.overdraft-calculator.results').is(":visible")) {
    if (isScrolledIntoView($('.overdraft-calculator.results'))) {
      if (checkResultsError() && checkResultsChanged()) {
        $('.overdraft-calculator-header.results').addClass('show');
      } else {
        $('.overdraft-calculator-header.results').removeClass('show');
      }
    } else {
      $('.overdraft-calculator-header.results').removeClass('show');
    }
  }
});

// functions on first load
$(document).on('ready', function () {
  if (checkFields()) {
    $('.results-wrapper .contents').show();
    $('.results-wrapper .error-msg.error-main').hide();
  } else {
    $('.results-wrapper .contents').hide();
    $('.results-wrapper .error-msg.error-main').show();
  }

  if ($('.overdraft-calculator.results').is(":visible")) {
    if (isScrolledIntoView($('.overdraft-calculator.results'))) {
      if (checkFields()) {
        $('.overdraft-calculator-header.results').addClass('show');
      } else {
        $('.overdraft-calculator-header.results').removeClass('show');
      }
    } else {
      $('.overdraft-calculator-header.results').removeClass('show');
    }
  }
});

// hiding and showing conditional fields
$(".calculator-wrapper").on("change", "input, select", function () {
  checkConditionals()
  $('.re-calculate-error').show();
  $('.results-wrapper .contents').hide();
});

var usersetrate = false;
var excludesurcharges;

function checkConditionals() {
  var selectedaccount = $('#account-type').find(':selected');
  //Determines if surcharge is excluded
  excludesurcharges = $(selectedaccount).data('exclude-surcharges') == 'True';


  // show course length field
  if ($(selectedaccount).data('show-course-length') == "True") {
    $('.course-length-section').show();
    $('.course-length-section select').addClass('form-field-required');
  } else {
    $('.course-length-section').hide();
    $('.course-length-section select').removeClass('form-field-required');
  }

  // hide arranged overdraft field
  if ($(selectedaccount).data('hide-arr') == "True") {
    $('.arranged-overdraft-section').hide();
    $('.arranged-overdraft-section input').val(0);
    $('.arranged-overdraft-section input').removeClass('form-field-required');
  } else {
    $('.arranged-overdraft-section').show();
    $('.arranged-overdraft-section input').addClass('form-field-required');
  }

  // show arranged overdraft interest rate field
  if ($(selectedaccount).data('show-arr-interest-rate') == "True") {
    if (parseFloat($('#arranged-overdraft-input').val()) >= 1) {
      $('.arranged-interest-section').show();
      $('.arranged-interest-section input').addClass('form-field-required');
      usersetrate = true;
    } else {
      $('.arranged-interest-section').hide();
      $('.arranged-interest-section input').removeClass('form-field-required');
      usersetrate = false;
    }
  } else {
    $('.arranged-interest-section').hide();
    $('.arranged-interest-section input').removeClass('form-field-required');
    usersetrate = false;
  }
}

// function to truncate number to 2 places

function truncateNumber(input) {
  var with2Decimals = input.toString().match(/^-?\d+(?:\.\d{0,2})?/)[0];
  return parseFloat(with2Decimals);
}

// all calculations

function runCalculations() {
  // inputs
  var overdraftinput = parseFloat($("#overdraft-input").val());
  var overdraftdays = parseFloat($("#days-input").val());
  var arrangedoverdraft = parseFloat($("#arranged-overdraft-input").val());
  var usersetinterestrate = parseFloat($("#arranged-interest-input").val());

  // rates
  var selectedaccount = $('#account-type').find(':selected');

  if ($(selectedaccount).data('show-course-length') == "True") {
    selectedaccount = $('#course-length').find(':selected');
    var interestfreelimit = $(selectedaccount).val();

    if (arrangedoverdraft > interestfreelimit) {
      selectedaccount = $("#course-length-over-limit");
    }
  }

  var arrangedinterest = $(selectedaccount).data('arr-rate');
  var arrangedear = $(selectedaccount).data('arr-ear');
  var unarrangedinterest = $(selectedaccount).data('unarr-rate');
  var unarrangedear = $(selectedaccount).data('unarr-ear');
  var unarrangedinterestwo = $(selectedaccount).data('unarr-rate-wo');
  var unarrangedearwo = $(selectedaccount).data('unarr-ear-wo');


  // amount overdrawn
  if (overdraftinput > arrangedoverdraft) {
    $('#arranged-amount-overdrawn').html('&pound;' + arrangedoverdraft.toFixed(2));
    $('#unarranged-amount-overdrawn').html('&pound;' + (overdraftinput - arrangedoverdraft).toFixed(2));
    var arrangedamount = arrangedoverdraft;
    var unarrangedamount = overdraftinput - arrangedoverdraft;
  } else {
    $('#arranged-amount-overdrawn').html('&pound;' + overdraftinput.toFixed(2));
    $('#unarranged-amount-overdrawn').html('&pound;0.00');
    var arrangedamount = overdraftinput;
    var unarrangedamount = 0;
  }

  // arranged overdraft interest cost
  if (usersetrate) {
    var arrangedcost = truncateNumber((overdraftdays * (usersetinterestrate / 365) * arrangedamount) / 100);

    $('#arranged-interest-cost').html('&pound;' + arrangedcost.toFixed(2));

    if ($(selectedaccount).data('hide-arr') == "True") {
      $('#arranged-interest-rate').html('N/A');
    } else {
      $('#arranged-interest-rate').html(usersetinterestrate + '&percnt;');
    }
  } else {
    var arrangedcost = truncateNumber((overdraftdays * (arrangedinterest / 365) * arrangedamount) / 100);

    $('#arranged-interest-cost').html('&pound;' + arrangedcost.toFixed(2));

    if ($(selectedaccount).data('hide-arr') == "True") {
      $('#arranged-interest-rate').html('N/A');
    } else {
      $('#arranged-interest-rate').html(arrangedinterest + '&percnt;');
    }
  }

  // get surcharge rates
  var surchargeni = parseFloat($("#base-account-rates").data('surcharge-ni'));
  var surchargegb = parseFloat($("#base-account-rates").data('surcharge-gb'));

  // unarranged overdraft interest rates when user inputs arranged rate
  if (usersetrate) {
    var baserate = parseFloat($("#base-account-rates").data('base-rate'));
    var clearaccountni = parseFloat($("#base-account-rates").data('clear-ni'));
    unarrangedinterest = usersetinterestrate + surchargeni;
    unarrangedinterestwo = clearaccountni + surchargeni;
  }

  // with or without arranged rate to use in the calculation
  if (arrangedoverdraft > 0) {
    var uaintrate = unarrangedinterest;
  } else {
    var uaintrate = unarrangedinterestwo;
  }

  // show interest rate
  $('#unarranged-interest-rate').html(uaintrate + '&percnt;');

  // if surcharge GB or NI
  var selectedjurisdiction = $('#account-type').find(':selected').data('jurisdiction').toUpperCase();
  var surchargepercent;

  //Exlcude surcharge will set the percent to 0
  if (excludesurcharges) {
    surchargepercent = 0
  }
  else if (selectedjurisdiction == 'GB') {
    surchargepercent = surchargegb;
  } else {
    surchargepercent = surchargeni;
  }

  // unarranged interest cost without surcharge
  var costwosurcharge = truncateNumber((overdraftdays * ((uaintrate - surchargepercent) / 365) * unarrangedamount) / 100);

  // unarranged surcharge cost
  var surchargecost = truncateNumber((overdraftdays * (surchargepercent / 365) * unarrangedamount) / 100);
  //Removes minimum surcharge cost if excluded
  if (excludesurcharges) {
    surchargecost = 0
  }
  else if (surchargecost < 2 && ((costwosurcharge + surchargecost) > 0)) {
    surchargecost = 2
  }


  // combining surcharge and base costs
  var unarrangedcost = costwosurcharge + surchargecost;
  $('#unarranged-interest-cost').html('&pound;' + unarrangedcost.toFixed(2));

  // results header values
  $('.overdraft-calculator-header.results #headerdays').html(overdraftdays);
  $('.overdraft-calculator-header.results #headeramount').html("&pound;" + (arrangedamount + unarrangedamount).toFixed(2));

  // total overdraft cost
  $('#result-overdraft-cost').html("&pound;" + (arrangedcost + unarrangedcost).toFixed(2));
}

// help text
$(document).on('mouseenter', '.calc-help', function (e) {
    $(this).addClass('active');
});
$(document).on('mouseleave', '.calc-help', function (e) {
    $(this).removeClass('active');
});