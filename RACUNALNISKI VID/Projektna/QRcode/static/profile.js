$(document).ready(function() {

    const url = document.URL;
    var address = url.split(":5000")[0];

    // VRNI VSE ITEME OD UPORABNIKA
    $.ajax({
        url: address + ":3000/user/returnItems",
        type: 'GET',
        crossDomain: true,
        xhrFields: { withCredentials: true },
        success: function(res) {

            var html = "";
            // sestavi html
            for(var i=0; i<res.items.length; i++){
                html += '<td><img id="item_img" src="' + res.items[i][0].image + '"></td>' +
                '<td><b>Name: </b>' + res.items[i][0].name + '</td>' +
                '<td><b>ID: </b><p class="itemID">' + res.items[i][0]._id + '</p></td>';
            }
            $('#myItems').html(html);

            // dobi id-je od vseh itemov
            var all = $(".itemID").map(function() {
                return this.innerHTML;
            }).get();
            var qr_string = all.join();

            // GENERIRAJ QR KODO
            $.ajax({
                data : {'value' : qr_string},
                type : 'POST',
                url : '/GenerateQR'
            })
            .done(function(data) {
                if(!data.error){
                    $('#QR_code').attr('src', '/static/qr/'+data.src);
                }
            })
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alert(xhr.status);
            alert(thrownError);
        }
    })

    function readCookie(name) {
        var nameEQ = name + "=";
        var ca = document.cookie.split(';');
        for(var i=0;i < ca.length;i++) {
            var c = ca[i];
            while (c.charAt(0)==' ') c = c.substring(1,c.length);
            if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length,c.length);
        }
        return null;
    }
});