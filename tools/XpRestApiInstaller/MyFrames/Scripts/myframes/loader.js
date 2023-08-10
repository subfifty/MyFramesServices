  function ChangeView(viewID) {
    let views = document.getElementsByClassName("MainView");


    let viewExists = false;
    for (let i=0; i<views.length;i++) {
      if ( views[i].id == viewID )
        viewExists = true;
    }

    if ( ! viewExists )
      return;

    for (let i=0; i<views.length;i++) {
      views[i].hidden = true;
    }
    
    //console.log("Activate View: " + viewID);
    document.getElementById(viewID).hidden = false;
  }  

  var LoadingID = 0;
  function DelayedLoading() {
    //console.log(LS.GetAccessToken());
    if ( IsValidToken(LS.GetAccessToken()) ) {
      if ( LoadingID > 0 ) {
        console.log("Valid token found. Stop waiting.");
        window.window.clearInterval(LoadingID);
        LoadingID = 0;
      }
      //////////////////////////////////////////////////////
      START();
      //////////////////////////////////////////////////////
    } else {
      if ( LoadingID > 0 ) {
        console.log("Waiting for valid token...");
        ChangeView("View_LoggedOut");
      } else {
        RefreshAuth();
        //ChangeView("View_LoggedOut");
        console.log("Start waiting for valid token...");
        LoadingID = window.window.setInterval(DelayedLoading, 1000);
      }
    }
  }

  XP.OnLoad(function () {
    DelayedLoading();
  })
