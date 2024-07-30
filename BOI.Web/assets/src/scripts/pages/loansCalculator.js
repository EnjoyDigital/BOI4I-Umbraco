var loanCalcButton = $("#loan-calc-button");

loanCalcButton.on("click", function () {
    calculate();
});

$("input#loan-input").on("keypress", function (e) {
    if (e.keyCode === 13) {
        loanCalcButton.trigger("click");
    }
});

function calculate() {
    var input = document.getElementById("loan-input").value;
    if (input >= 1000 & input <= 40000) {
        $(".error-msg").hide();
        $(".results-wrapper .results").empty();
        $(".calculator-wrapper").animate({ right: "100%" }, 400).fadeOut(200, function () {
            $(".results-wrapper").css("right", "0");
            $(".results-wrapper").fadeIn(500);
        });
        $(".ni-calculator-header.results").animate({ top: "0" }, 400);
        var input2 = Math.round(input / 100) * 100;
        var apr = 7.5;
        var calcapr = apr / 100;
        var maxtime = 84;
        let mintime = 12;
        
        if (input2 < 3000) {
            if (input2 < 2000) {
                mintime = 18
            }
                
            
            apr = 15.70;

            maxtime = 36;
        } else if (input2 < 5000) {
            apr = 10.7;

            maxtime = 60;
        } else if (input2 < 7500) {
            apr = 7.3;

            maxtime = 84;
        } else if (input2 < 15000) {
            apr = 6.10;

            maxtime = 84;
        } else if (input2 < 25001) {
            apr = 6.10;
            maxtime = 84;
            
        } 
        else if (input2 <= 40000) {
            apr = 8.50;
            mintime = 48;
            maxtime = 84;
        }
        calcapr = apr / 100;
        var i;
        for (i = mintime; i <= maxtime; i += 6) {
            var term = i;
            var calc = input2 * ((Math.pow(1 + calcapr, 1 / 12) - 1) / (1 - Math.pow(1 + (Math.pow(1 + calcapr, 1 / 12) - 1), -term)));
            var total = term * calc;
            var monthly = (Math.round(calc * 100) / 100).toFixed(2);
            var totalcost = total.toFixed(2);
            var overTerm = "Over";
            var years = Math.floor(term / 12);
            var months = term % 12;
           
            var length = "" + years + " years";
            if (term === 12) {
                length = "" + years + " year";
            } else if (months === 6) {
                length = "" + years + " 1/2 years";
            }
            length = overTerm + " " + length;
            //if (term !== maxtime) {
            //    length = length + " and over";
            //}
            //else {
            //    length = "Up to " + length;
            //}


            $(".ni-calculator-header.results #apr").html(apr + "%");
            $(".ni-calculator-header.results #loanamount").html("&#163;" + input2);
            $(".results-wrapper .results").append("<div class=\"result\"><input type=\"radio\" name=\"results\" value=\"https://apply-loan.bankofirelanduk.com/ni/loan-quotation-landingpage?0&sourcecode=90001&loanamount=" + input2 + "&loanterm=" + term + "\" id=\""
                + term + "\"><label for=\"" + term + "\"><span class=\"price\">&#163;" + monthly + "</span><span class=\"small\">per month</span><span>" + length + "</span></label></div>");

        }
    } else {
        $(".error-msg").show();
    }
}

$(document).on("click", ".results .result input", function () {
    $(".results-wrapper .button-area").fadeIn();
    var radioValue = $("input[name='results']:checked").val();
    document.getElementById("apply-now-button").href = radioValue;
});

$(document).on("click", ".changelink", function () {
    $(".results-wrapper").animate({ right: "100%" }, 400).fadeOut(200, function () {
        $(".calculator-wrapper").css("right", "0");
        $(".calculator-wrapper").fadeIn(500);
    });
    $(".results-wrapper .button-area").fadeOut();
    $(".ni-calculator-header.results").animate({ top: "-100px" }, 400);
});
