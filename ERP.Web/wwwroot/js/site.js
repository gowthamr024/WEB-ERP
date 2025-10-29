$(function () {
    // Apply to all AJAX requests automatically
    $.ajaxSetup({
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        }
    });
});
