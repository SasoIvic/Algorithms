$(document).ready(function() {

    const url = document.URL;
    var address = url.split(":5000")[0];

    $("#submitUpload").click(function(){

        var form_data = new FormData();
        form_data.append('photo', $('#file').prop('files')[0]);

        // NALOŽI SLIKO IN JO DEKODIRAJ
        $.ajax({
            url: '/Upload',
            type: 'POST',
            data: form_data,
            contentType: false,
            cache: false,
            processData: false,
            success: function(res){

                var data = {
                    ids:res
                }
                // DOBI ITEME Z TEMI ID-ji
                $.ajax({
                    url: address + ":3000/user/getItemsById",
                    type: 'POST',
                    data: data,
                    crossDomain: true,
                    xhrFields: { withCredentials: true },
                    success: function(res) {
                        alert("OK");

                        var html = "";
                        // sestavi html
                        for(var i=0; i<res.items.length; i++){
                            html += '<td><img id="item_img" src="' + res.items[i][0].image + '"></td>' +
                            '<td><b>Name: </b>' + res.items[i][0].name + '</td>' +
                            '<td><b>ID: </b><p class="itemID">' + res.items[i][0]._id + '</p></td>';
                        }
                        $('#items').html(html);

                    },
                });
            },
        });
    });
});