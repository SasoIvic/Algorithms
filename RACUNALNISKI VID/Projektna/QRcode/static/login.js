$(document).ready(function() {

    $("#loginBtn").click(function(){

        var username = document.getElementsByName("username")[0].value;
        var password = document.getElementsByName("password")[0].value;
        const url = document.URL;
        var address = url.split(":5000")[0];

        let data = {
            username:username,
            password:password
        }

        // PRIJAVA UPORABNIKA - pridobi jwt token in nastavi cookie
        $.ajax({
            url: address + ":3000/user/login",
            type: 'POST',
            data: data,
            success: function(res) {
                localStorage.setItem('jwt', res.jwt);
                setCookie('token1', res.jwt.substring(0, res.jwt.length/2));
                setCookie('token2', res.jwt.substring(res.jwt.length/2, res.jwt.length));
                const url = document.URL + "Profile";
                window.location = url;
            },
            error: function (xhr, ajaxOptions, thrownError) {
                alert(xhr.status);
                alert(thrownError);
            }
        })
    });

    function setCookie(name,value) {
        var expires = "";
        document.cookie = name + "=" + (value || "")  + expires + "; path=/";
    }

});