tippy('[data-tippy-content]', {
    placement: "bottom",
    offset: [0, -5],
    theme: "payg"
})

/* Variables */
var $paygForm = $('#PaygCalculator'),
    $calcContainer = $('.calculator-container'),
    $optionsContainer = $('.options-container'),
    $loanAmountInput = $('#LoanAmount'),
    $recalculateButton = $('#Recalculate')

/* Clear loan amount input on focus */
$loanAmountInput.on('focus', function() {
    $(this).val('')
})

/* Take user back to the calculator input screen */
$recalculateButton.on('click', function(e) {
    e.preventDefault()

    $optionsContainer.fadeOut(250, function() {
        $calcContainer.fadeIn(250)

        $loanAmountInput.trigger('focus')
    })
})

/* Handle form submission */
$paygForm.on('submit', function(e) {
    e.preventDefault()

    /* Get loan amount from user-input and validate it */
    var $loanAmount = $loanAmountInput.val()

    if ($loanAmount < parseInt($loanAmountInput.attr('min')) || $loanAmount > parseInt($loanAmountInput.attr('max'))) {
        $('#PaygCalcErrorMessage').css('display','block')
        return false
    }

    $('#PaygCalcErrorMessage').css('display','none')
    
    $calcContainer.fadeOut(250, function() {
        $optionsContainer.fadeIn(250)

        /* Construct formatter */
        var formatter = new Intl.NumberFormat('en-GB', {
            style: 'currency',
            currency: 'GBP',
        
            // These options are needed to round to whole numbers if that's what you want.
            //minimumFractionDigits: 0, // (this suffices for whole numbers, but will print 2500.10 as $2,500.1)
            //maximumFractionDigits: 0, // (causes 2500.99 to be printed as $2,501)
        });

        /* Run: No Pay As You Grow option functions 
         * and assign to front-end table cells
         */

        // No Pay As You Grow option: First Repayment
        $('#NoPaygOptions [data-first-repayment]').html(
            formatter.format(
                PMT($loanAmount,noPaygOption.interestRate,noPaygOption.compoundingPeriodsPerYear,noPaygOption.periodInMonths)
            )
        )

        // No Pay As You Grow option: Repayment Amount after 6 Months
        $('#NoPaygOptions [data-repayment-after-6-months]').html(
            formatter.format(
                PMT($loanAmount,noPaygOption.interestRate,noPaygOption.compoundingPeriodsPerYear,noPaygOption.periodInMonths)
            )
        )

        // No Pay As You Grow option: Total Amount You Will Repay
        $('#NoPaygOptions [data-total-amount-repayable]').html(
            formatter.format(
                TotalCosts(
                    PMT($loanAmount,noPaygOption.interestRate,noPaygOption.compoundingPeriodsPerYear,noPaygOption.periodInMonths),
                    noPaygOption.periodInMonths
                )
            )
        )


        /* Run: Term Extension to 10 Years functions 
         * and assign to front-end table cells
         */

        // Term Extension to 10 years: First Repayment
        $('#TermExtensionTenYears [data-first-repayment]').html(
            formatter.format(
                PMT($loanAmount,termExtensionTenYrs.interestRate,termExtensionTenYrs.compoundingPeriodsPerYear,termExtensionTenYrs.periodInMonths)
            )
        )

        // Term Extension to 10 years: Repayment Amount after 6 Months
        $('#TermExtensionTenYears [data-repayment-after-6-months]').html(
            formatter.format(
                PMT($loanAmount,termExtensionTenYrs.interestRate,termExtensionTenYrs.compoundingPeriodsPerYear,termExtensionTenYrs.periodInMonths)
            )
        )

        // Term Extension to 10 years: Total Amount You Will Repay
        $('#TermExtensionTenYears [data-total-amount-repayable]').html(
            formatter.format(
                TotalCosts(
                    PMT($loanAmount,termExtensionTenYrs.interestRate,termExtensionTenYrs.compoundingPeriodsPerYear,termExtensionTenYrs.periodInMonths),
                    termExtensionTenYrs.periodInMonths
                )
            )
        )

        // Term Extension to 10 years: Cost Increase over NO PAYG Option
        $('#TermExtensionTenYears [data-cost-increase]').html(
            formatter.format(
                TotalInterestCosts(
                    TotalCosts(
                        PMT($loanAmount,termExtensionTenYrs.interestRate,termExtensionTenYrs.compoundingPeriodsPerYear,termExtensionTenYrs.periodInMonths),
                        termExtensionTenYrs.periodInMonths
                    ),
                    TotalCosts(
                        PMT($loanAmount,noPaygOption.interestRate,noPaygOption.compoundingPeriodsPerYear,noPaygOption.periodInMonths),
                        noPaygOption.periodInMonths
                    )
                )
            )
        )


        /* Run: Interest Only (6 months) functions 
         * and assign to front-end table cells
         */

        // Get Interest Charged during Holiday
        interestOnlySixMonths.interestChargedHoliday = InterestChargedHoliday($loanAmount,interestOnlySixMonths.interestRate)

        // Interest Only (6 Months): First Repayment
        $('#InterestOnlySixMonths [data-first-repayment]').html(
            formatter.format(
                MonthlyInterestHolidayCost(interestOnlySixMonths.interestChargedHoliday)
            )
        )

        // Interest Only (6 Months): Repayment Amount after 6 Months
        $('#InterestOnlySixMonths [data-repayment-after-6-months]').html(
            formatter.format(
                PMT($loanAmount,interestOnlySixMonths.interestRate,interestOnlySixMonths.compoundingPeriodsPerYear,interestOnlySixMonths.periodInMonths)
            )
        )

        $('#InterestOnlySixMonths [data-total-amount-repayable]').html(
            formatter.format(
                CapitalHolidayTotalCosts(
                    PMT($loanAmount,interestOnlySixMonths.interestRate,interestOnlySixMonths.compoundingPeriodsPerYear,interestOnlySixMonths.periodInMonths),
                    interestOnlySixMonths.periodInMonths,
                    interestOnlySixMonths.interestChargedHoliday
                )
            )
        )

        // Interest Only (6 Months): Total Amount You Will Repay
        $('#InterestOnlySixMonths [data-cost-increase]').html(
            formatter.format(
                TotalInterestCosts(
                    CapitalHolidayTotalCosts(
                        PMT($loanAmount,interestOnlySixMonths.interestRate,interestOnlySixMonths.compoundingPeriodsPerYear,interestOnlySixMonths.periodInMonths),
                        interestOnlySixMonths.periodInMonths,
                        interestOnlySixMonths.interestChargedHoliday
                    ),
                    TotalCosts(
                        PMT($loanAmount,noPaygOption.interestRate,noPaygOption.compoundingPeriodsPerYear,noPaygOption.periodInMonths),
                        noPaygOption.periodInMonths
                    )
                )
            )
        )


        /* Run: Full Repayment Holiday (6 months) functions 
         * and assign to front-end table cells
         */

        // Get Interest Charged during Holiday
        fullRepaymentHolidaySixMonths.interestChargedHoliday = InterestChargedHoliday($loanAmount,fullRepaymentHolidaySixMonths.interestRate)

        // Get Interest Applied at Month 3
        fullRepaymentHolidaySixMonths.interestAtMonthThree = InterestAppliedAtMonthThree($loanAmount,fullRepaymentHolidaySixMonths.interestRate)

        // Get Balance at Month 3
        fullRepaymentHolidaySixMonths.balanceAtMonthThree = BalanceAtMonthThree($loanAmount,fullRepaymentHolidaySixMonths.interestAtMonthThree)

        // Get Interest Applied at Month 6
        fullRepaymentHolidaySixMonths.interestAtMonthSix = InterestAppliedAtMonthSix(fullRepaymentHolidaySixMonths.balanceAtMonthThree,fullRepaymentHolidaySixMonths.interestRate)

        // Get Balance at Month 6
        fullRepaymentHolidaySixMonths.balanceAtMonthSix = BalanceAtMonthSix(fullRepaymentHolidaySixMonths.balanceAtMonthThree,fullRepaymentHolidaySixMonths.interestAtMonthSix)

        // Full Repayment Holiday (6 Months): Repayment Amount after 6 Months
        $('#FullRepaymentHolidaySixMonths [data-repayment-after-6-months]').html(
            formatter.format(
                PMT(fullRepaymentHolidaySixMonths.balanceAtMonthSix,fullRepaymentHolidaySixMonths.interestRate,fullRepaymentHolidaySixMonths.compoundingPeriodsPerYear,fullRepaymentHolidaySixMonths.periodInMonths)
            )
        )

        $('#FullRepaymentHolidaySixMonths [data-total-amount-repayable]').html(
            formatter.format(
                TotalCosts(
                    PMT(fullRepaymentHolidaySixMonths.balanceAtMonthSix,fullRepaymentHolidaySixMonths.interestRate,fullRepaymentHolidaySixMonths.compoundingPeriodsPerYear,fullRepaymentHolidaySixMonths.periodInMonths),
                    fullRepaymentHolidaySixMonths.periodInMonths
                )
            )
        )

        $('#FullRepaymentHolidaySixMonths [data-cost-increase]').html(
            formatter.format(
                TotalInterestCosts(
                    TotalCosts(
                        PMT(fullRepaymentHolidaySixMonths.balanceAtMonthSix,fullRepaymentHolidaySixMonths.interestRate,fullRepaymentHolidaySixMonths.compoundingPeriodsPerYear,fullRepaymentHolidaySixMonths.periodInMonths),
                        fullRepaymentHolidaySixMonths.periodInMonths
                    ),
                    TotalCosts(
                        PMT($loanAmount,noPaygOption.interestRate,noPaygOption.compoundingPeriodsPerYear,noPaygOption.periodInMonths),
                        noPaygOption.periodInMonths
                    )
                )
            )
        )


        /* Run: Term Extension 10 Years + Interest Only (6 months) functions 
         * and assign to front-end table cells
         */

        // Get Interest Charged during Holiday
        termExtensionTenYearsPlusInterestOnly.interestChargedHoliday = InterestChargedHoliday($loanAmount,termExtensionTenYearsPlusInterestOnly.interestRate)

        // Interest Only (6 Months): First Repayment
        $('#TermExtensionTenYearsPlusInterestOnly [data-first-repayment]').html(
            formatter.format(
                MonthlyInterestHolidayCost(termExtensionTenYearsPlusInterestOnly.interestChargedHoliday)
            )
        )

        // Interest Only (6 Months): Repayment Amount after 6 Months
        $('#TermExtensionTenYearsPlusInterestOnly [data-repayment-after-6-months]').html(
            formatter.format(
                PMT($loanAmount,termExtensionTenYearsPlusInterestOnly.interestRate,termExtensionTenYearsPlusInterestOnly.compoundingPeriodsPerYear,termExtensionTenYearsPlusInterestOnly.periodInMonths)
            )
        )

        $('#TermExtensionTenYearsPlusInterestOnly [data-total-amount-repayable]').html(
            formatter.format(
                CapitalHolidayTotalCosts(
                    PMT($loanAmount,termExtensionTenYearsPlusInterestOnly.interestRate,termExtensionTenYearsPlusInterestOnly.compoundingPeriodsPerYear,termExtensionTenYearsPlusInterestOnly.periodInMonths),
                    termExtensionTenYearsPlusInterestOnly.periodInMonths,
                    termExtensionTenYearsPlusInterestOnly.interestChargedHoliday
                )
            )
        )

        // Interest Only (6 Months): Total Amount You Will Repay
        $('#TermExtensionTenYearsPlusInterestOnly [data-cost-increase]').html(
            formatter.format(
                TotalInterestCosts(
                    CapitalHolidayTotalCosts(
                        PMT($loanAmount,termExtensionTenYearsPlusInterestOnly.interestRate,termExtensionTenYearsPlusInterestOnly.compoundingPeriodsPerYear,termExtensionTenYearsPlusInterestOnly.periodInMonths),
                        termExtensionTenYearsPlusInterestOnly.periodInMonths,
                        termExtensionTenYearsPlusInterestOnly.interestChargedHoliday
                    ),
                    TotalCosts(
                        PMT($loanAmount,noPaygOption.interestRate,noPaygOption.compoundingPeriodsPerYear,noPaygOption.periodInMonths),
                        noPaygOption.periodInMonths
                    )
                )
            )
        )


        /* Run: Term Extension to 10 Years + Full Repayment Holiday (6 months) functions 
         * and assign to front-end table cells
         */

        // Get Interest Charged during Holiday
        termExtensionTenYearsPlusFullRepaymentHolidaySixMonths.interestChargedHoliday = InterestChargedHoliday($loanAmount,termExtensionTenYearsPlusFullRepaymentHolidaySixMonths.interestRate)

        // Get Interest Applied at Month 3
        termExtensionTenYearsPlusFullRepaymentHolidaySixMonths.interestAtMonthThree = InterestAppliedAtMonthThree($loanAmount,termExtensionTenYearsPlusFullRepaymentHolidaySixMonths.interestRate)

        // Get Balance at Month 3
        termExtensionTenYearsPlusFullRepaymentHolidaySixMonths.balanceAtMonthThree = BalanceAtMonthThree($loanAmount,termExtensionTenYearsPlusFullRepaymentHolidaySixMonths.interestAtMonthThree)

        // Get Interest Applied at Month 6
        termExtensionTenYearsPlusFullRepaymentHolidaySixMonths.interestAtMonthSix = InterestAppliedAtMonthSix(termExtensionTenYearsPlusFullRepaymentHolidaySixMonths.balanceAtMonthThree,termExtensionTenYearsPlusFullRepaymentHolidaySixMonths.interestRate)

        // Get Balance at Month 6
        termExtensionTenYearsPlusFullRepaymentHolidaySixMonths.balanceAtMonthSix = BalanceAtMonthSix(termExtensionTenYearsPlusFullRepaymentHolidaySixMonths.balanceAtMonthThree,termExtensionTenYearsPlusFullRepaymentHolidaySixMonths.interestAtMonthSix)

        // Full Repayment Holiday (6 Months): Repayment Amount after 6 Months
        $('#TermExtensionTenYearsPlusFullRepaymentHoliday [data-repayment-after-6-months]').html(
            formatter.format(
                PMT(termExtensionTenYearsPlusFullRepaymentHolidaySixMonths.balanceAtMonthSix,termExtensionTenYearsPlusFullRepaymentHolidaySixMonths.interestRate,termExtensionTenYearsPlusFullRepaymentHolidaySixMonths.compoundingPeriodsPerYear,termExtensionTenYearsPlusFullRepaymentHolidaySixMonths.periodInMonths)
            )
        )

        $('#TermExtensionTenYearsPlusFullRepaymentHoliday [data-total-amount-repayable]').html(
            formatter.format(
                TotalCosts(
                    PMT(termExtensionTenYearsPlusFullRepaymentHolidaySixMonths.balanceAtMonthSix,termExtensionTenYearsPlusFullRepaymentHolidaySixMonths.interestRate,termExtensionTenYearsPlusFullRepaymentHolidaySixMonths.compoundingPeriodsPerYear,termExtensionTenYearsPlusFullRepaymentHolidaySixMonths.periodInMonths),
                    termExtensionTenYearsPlusFullRepaymentHolidaySixMonths.periodInMonths
                )
            )
        )

        $('#TermExtensionTenYearsPlusFullRepaymentHoliday [data-cost-increase]').html(
            formatter.format(
                TotalInterestCosts(
                    TotalCosts(
                        PMT(termExtensionTenYearsPlusFullRepaymentHolidaySixMonths.balanceAtMonthSix,termExtensionTenYearsPlusFullRepaymentHolidaySixMonths.interestRate,termExtensionTenYearsPlusFullRepaymentHolidaySixMonths.compoundingPeriodsPerYear,termExtensionTenYearsPlusFullRepaymentHolidaySixMonths.periodInMonths),
                        termExtensionTenYearsPlusFullRepaymentHolidaySixMonths.periodInMonths
                    ),
                    TotalCosts(
                        PMT($loanAmount,noPaygOption.interestRate,noPaygOption.compoundingPeriodsPerYear,noPaygOption.periodInMonths),
                        noPaygOption.periodInMonths
                    )
                )
            )
        )
    })
})

/* No PAYG Option */
var noPaygOption = {
    interestRate: 0.025, // Interest Rate = 2.5%
    compoundingPeriodsPerYear: 12, // Compounding Periods Per Year
    periodInMonths: 60 // Period in Months
}

/* Term extension to 10 years */
var termExtensionTenYrs = {
    interestRate: 0.025, // Interest Rate = 2.5%
    compoundingPeriodsPerYear: 12, // Compounding Periods Per Year
    periodInMonths: 108 // Period in Months
}

/* Interest Only (6 months) */
var interestOnlySixMonths = {
    interestRate: 0.025, // Interest Rate = 2.5%
    compoundingPeriodsPerYear: 12, // Compounding Periods Per Year
    periodInMonths: 60, // Period in Months
    interestChargedHoliday: 0 // Interest Charged during Holiday = LoanAmount*InterestRate/2
}

/* Full Repayment Holiday (6 months) */
var fullRepaymentHolidaySixMonths = {
    interestRate: 0.025, // Interest Rate = 2.5%
    compoundingPeriodsPerYear: 12, // Compounding Periods Per Year
    periodInMonths: 60, // Period in Months
    interestChargedHoliday: 0, // Interest Charged during Holiday = LoanAmount*InterestRate/2
    interestAtMonthThree: 0, // Interest Applied at Month 3 = LoanAmount*InterestRate/4
    balanceAtMonthThree: 0, // Balance at Month 3 function = (LoanAmount  + InterestAppliedAtMonthThree)
    interestAtMonthSix: 0, // Interest Applied at Month 6 function = (BalanceAtMonthThree * InterestRate) / 4
    balanceAtMonthSix: 0 // Balance at Month 6 function = (BalanceAtMonthThree + InterestAppliedAtMonthSix)
}

/* Term Extension to 10 years + Interest Only (6 months) */
var termExtensionTenYearsPlusInterestOnly = {
    interestRate: 0.025, // Interest Rate = 2.5%
    compoundingPeriodsPerYear: 12, // Compounding Periods Per Year
    periodInMonths: 102, // Period in Months
    interestChargedHoliday: 0 // Interest Charged during Holiday = LoanAmount*InterestRate/2
}

/* Term Extension to 10 years + Full Repayment Holiday (6 months) */
var termExtensionTenYearsPlusFullRepaymentHolidaySixMonths = {
    interestRate: 0.025, // Interest Rate = 2.5%
    compoundingPeriodsPerYear: 12, // Compounding Periods Per Year
    periodInMonths: 102, // Period in Months
    interestChargedHoliday: 0, // Interest Charged during Holiday = LoanAmount*InterestRate/2
    interestAtMonthThree: 0, // Interest Applied at Month 3 = LoanAmount*InterestRate/4
    balanceAtMonthThree: 0, // Balance at Month 3 function = (LoanAmount  + InterestAppliedAtMonthThree)
    interestAtMonthSix: 0, // Interest Applied at Month 6 function = (BalanceAtMonthThree * InterestRate) / 4
    balanceAtMonthSix: 0 // Balance at Month 6 function = (BalanceAtMonthThree + InterestAppliedAtMonthSix)
}

/* PMT function = MontlyPayment */
function PMT (LoanAmount,InterestRate,CompoundingPeriodsPerYear,PeriodInMonths) {
    /*
    P = Monthly Payment
    Pv = Present Value (starting value of the loan)
    APR = Annual Percentage Rate
    R = Periodic Interest Rate = APR/number of interest periods per year
    n = Total number of interest periods (interest periods per year * number of years)
    */

    // P = (Pv*R) / [1 - (1 + R)^(-n)]

    return (LoanAmount * (InterestRate/CompoundingPeriodsPerYear)) / (1 - Math.pow((1 + (InterestRate/CompoundingPeriodsPerYear)),(-PeriodInMonths)))
    // return (50000 * (0.025/12)) / (1 - Math.pow((1 + (0.025/12)),(-60)))
}

/* Total Costs function = MonthlyPayment * Period in Months */
function TotalCosts (MonthlyPayment,PeriodInMonths) {
    return MonthlyPayment*PeriodInMonths
}

/* Cost Increase Over NO PAYG Option function = TotalCosts - NoPaygOption Total Cost */
function TotalInterestCosts (TotalCosts,NoPaygOptionTotalCost) {
    return TotalCosts - NoPaygOptionTotalCost
}

/* Interest Charged during Holiday function = LoanAmount*InterestRate/2 */
function InterestChargedHoliday (LoanAmount,InterestRate) {
    return (LoanAmount*InterestRate)/2
}

/* Monthly Interest Cost During Holiday function = InterestChargedHoliday/6 */
function MonthlyInterestHolidayCost (InterestChargedHoliday) {
    return InterestChargedHoliday/6
}

/* Capital Holiday Total Costs function = 
 * (MonthlyPayment * Period in Months) + InterestChargedHoliday 
 */
function CapitalHolidayTotalCosts (MonthlyPayment,PeriodInMonths,InterestChargedHoliday) {
    return MonthlyPayment*PeriodInMonths + InterestChargedHoliday
}

/* Interest Applied at Month 3 function = 
 * (LoanAmount * InterestRate) / 4
 */
function InterestAppliedAtMonthThree (LoanAmount,InterestRate) {
    return (LoanAmount * InterestRate) / 4
}

/* Balance at Month 3 function = 
 * (LoanAmount + InterestAppliedAtMonthThree)
 */
function BalanceAtMonthThree (LoanAmount,InterestAppliedAtMonthThree) {
    return parseInt(LoanAmount) + InterestAppliedAtMonthThree
}

/* Interest Applied at Month 6 function = 
 * (BalanceAtMonthThree * InterestRate) / 4
 */
function InterestAppliedAtMonthSix (BalanceAtMonthThree,InterestRate) {
    return (BalanceAtMonthThree * InterestRate) / 4
}

/* Balance at Month 6 function = 
 * (BalanceAtMonthThree + InterestAppliedAtMonthSix)
 */
function BalanceAtMonthSix (BalanceAtMonthThree,InterestAppliedAtMonthSix) {
    return BalanceAtMonthThree + InterestAppliedAtMonthSix
}