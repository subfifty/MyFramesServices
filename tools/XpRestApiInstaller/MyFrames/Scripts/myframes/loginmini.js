function Execute(svc, uid, pwd) {
    let body = {
        'username': uid,
        'password': pwd
    }

    let xhr = new XMLHttpRequest();
    
    xhr.onreadystatechange = function () {
        //console.log("xhr.readyState = " + xhr.readyState);
        if (xhr.readyState == 4 && xhr.status == 200) {
            let json = JSON.parse(xhr.responseText);
            //console.log(json);

            if ( json.IsAuthenticated ) {
                LS.SetUserName(json.UserName);
                LS.SetAccessToken(json.AccessToken);
                LS.SetRefreshToken(json.RefreshToken);
                LS.SetFullName(json.FullName);

                document.getElementById('pwd').value = "";
                document.getElementById("fullNameDiv").innerText = json.FullName;

                document.getElementById('servivceurl').value = LS.GetLruServiceUrl();
                document.getElementById('uid').value = LS.GetLruUid();

                START();
            }
        }
    };

    LS.SetUserName("");

    let BaseUrlLogon =  LS.GetLruServiceUrl();
    if ( !BaseUrlLogon.endsWith("/") ) 
        BaseUrlLogon += "/";
    let url = BaseUrlLogon + "logon";
    console.log(url);

    xhr.open("POST", url, true);
    xhr.setRequestHeader("Accept", "*/*");
    xhr.setRequestHeader("Content-Type", "application/json" );
    xhr.send(JSON.stringify(body));
}

function Logon() {
  let frm1 = XP.FormToObject('#loginMini');
  //console.log(frm1);

  let svc = new URL(frm1.servivceurl);
  if ( svc.pathname == "/" ) {
    svc.pathname = "/xphoneconnect/myframes/xphone/auth";
  }

  LS.SetLruServiceUrl(svc.href);
  LS.SetLruUid(frm1.uid);

  LS.SetAccessToken("refresh");
  Execute(svc.href, frm1.uid, frm1.pwd);
}

function Logout() {
    ChangeView("View_LoggedOut");

    LS.SetUserName("");
    LS.SetAccessToken("");
    LS.SetRefreshToken("");
    LS.SetFullName("Anmeldung");
}

function Refresh() {
  if ( IsValidToken(LS.GetAccessToken()) )
    return;

  RefreshAuth();
}

function IsLoggedOut() {
    return (( LS.GetUserName() == "") || LS.GetLruUid() == "");
}

XP.OnLoad(function() {
    document.getElementById('servivceurl').value = LS.GetLruServiceUrl();
    document.getElementById('uid').value = LS.GetLruUid();
    document.getElementById('pwd').value = "";
    document.getElementById('fullNameDiv').innerText =  LS.GetFullName();

    Refresh();
    window.window.setInterval(Refresh, 60000);
})
