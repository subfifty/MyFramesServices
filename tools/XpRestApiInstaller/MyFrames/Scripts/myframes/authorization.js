//
// Required include: src="./scripts/jwt/jwt-decode.js"
//
function IsValidToken(token) {
    try {
        var decoded = jwt_decode(token);
        let now = Math.floor(Date.now() / 1000);
        //console.log("Expiration: " + decoded.exp);
        //console.log("Now       : " + now);
        //console.log("Difference: " + (Math.floor(decoded.exp) - now).toString());
        return Math.floor(decoded.exp) > now;
    }
    catch {
        return false;
    }
}

function RefreshAuth() {
    LS.SetAccessToken("refresh");

    let body = {};
    body.RefreshToken = LS.GetRefreshToken();
    body.ClientSecret = "";

    let xhr = new XMLHttpRequest();

    xhr.onreadystatechange = function () {
        console.log("xhr.readyState = " + xhr.readyState);
        if (xhr.readyState == 4) {
            if (xhr.status == 200) {
                let json = JSON.parse(xhr.responseText);
                //console.log(json);

                LS.SetAccessToken(json.AccessToken);
                LS.SetRefreshToken(json.RefreshToken);

            } else {
                console.log(xhr.statusText);
            }
        }
    };

    let BaseUrlLogon = LS.GetLruServiceUrl();
    if (BaseUrlLogon) {
        if (!BaseUrlLogon.endsWith("/"))
            BaseUrlLogon += "/";
        let url = BaseUrlLogon + "refresh";
        console.log(url);

        xhr.open("POST", url, true);
        xhr.setRequestHeader("Accept", "*/*");
        xhr.setRequestHeader("Content-Type", "application/json");
        xhr.send(JSON.stringify(body));
    } else {
        //document.getElementById("logout").hidden = true;
        //document.getElementById("main").hidden = false;
    }
}

// Interception of XMLHttpRequest.send(). Add Authorization Header. 
XMLHttpRequest.prototype.realSend = XMLHttpRequest.prototype.send;
var newSend = function (vData) {
    // "this" points to the XMLHttpRequest Object
    //this.setRequestHeader("Accept", "application/json");
    //this.setRequestHeader("Content-Type", "application/json; charset=utf-8" );
    if (LS.GetAccessToken()) {
        if (IsValidToken(LS.GetAccessToken())) {
            this.setRequestHeader("Authorization", "Bearer " + LS.GetAccessToken());
            this.realSend(vData);
        } else {
            if (LS.GetAccessToken() == "refresh") {
                this.realSend(vData);
            } else {
                console.log("Invalid access token!");
            }
        }
    } else {
        console.log("Missing access token!");
    }
};
XMLHttpRequest.prototype.send = newSend;    
