import Globals from '../modules/globals/globals'
import ProductFilters from '../modules/productFilters'

Globals()

// Individually required scripts
ProductFilters()

$(document).on('click', '#productUpdate', function (e) {

    var formUrl = $(this).data('ajaxurl');
    var pageId = $(this).data('pageid');

    var productType = document.getElementById('ProductType').value;
    var productTerm = document.getElementById('ProductTerm').value;
    var productCategory = document.getElementById('ProductCategory').value;
    var productLTV = document.getElementById('ProductLTV').value;
    var interestOnly = $('#InterestOnly').is(':checked');

    //$.ajax({
    //    type: 'POST',
    //    url: formUrl,
    //    data: {
    //        'productType': productType, 'productTerm': productTerm, 'productCategory': productCategory, 'productLTV': productLTV, 'interestOnly': interestOnly, 'pageId': pageId
    //    },
    //    success: function (data) {
    //        $('#productsResult').html(data);
    //    }
    //});
});

$(document).on('click', '#productClear', function (e) {

    var formUrl = $(this).data('ajaxurl');
    var pageId = $(this).data('pageid');

    $('#ProductType').val('');
    $('#ProductTerm').val('');
    $('#ProductCategory').val('');
    $('#ProductLTV').val('');
    document.getElementById('InterestOnly').value = false;

    //$.ajax({
    //    type: 'POST',
    //    url: formUrl,
    //    data: {
    //        'productType': '', 'productTerm': '', 'productCategory': '', 'productLTV': '', 'interestOnly': false, 'pageId': pageId
    //    },
    //    success: function (data) {
    //        $('#productsResult').html(data);
    //    }
    //});
});