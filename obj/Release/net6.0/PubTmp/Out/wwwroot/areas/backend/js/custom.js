// Here goes your custom javascript

$(document).ready(function () {
    $('.img_dialog').on('click', function () {
        let img = $(this).find('img').eq(0);
        let src = img.attr('src');
        $('#exampleModalToggle .modal-body').html($('<img>').attr('src', src).addClass('img-fluid'));
        $('#exampleModalToggle').modal('show');
    });

});

function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
}

function eraseCookie(name) {
    document.cookie = name + '=; Path=/; Expires=Thu, 01 Jan 1970 00:00:01 GMT;';
}