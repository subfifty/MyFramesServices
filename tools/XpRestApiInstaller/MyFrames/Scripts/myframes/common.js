const PresenceType = "restapi";

const MyFramesBase = GetMyFramesBase();
const XPhoneServer = GetXPhoneServer();

var BaseUrlPresence = MyFramesBase + "presence/api/presence/";
if (PresenceType == "restapi") {
    BaseUrlPresence = MyFramesBase + "api/presence/users/";
}
const BaseUrlClip = MyFramesBase + "api/clip";
const BaseUrlAnyBell = MyFramesBase + "api/anybell";
const BaseUrlRestApiAgents = MyFramesBase + "api/powershell/scripts/Get-TeamDeskAssignedAgents?key=TeamDeskName&value="
const BaseUrlRestApiHotlines = MyFramesBase + "api/powershell/scripts/Get-TeamDeskHotlinesAndNumbers?key=UserEmail&value="
const BaseUrlRestApiUser = MyFramesBase + "api/powershell/scripts/Get-XPhoneUserByAccountName?key=UserAccount&value=";
const BaseUrlRestApiConfig = MyFramesBase + "api/powershell/scripts/Get-TeamDeskConfig?key=UserEmail&value=";
const BaseUrlRestApiDialParam = MyFramesBase + "api/powershell/scripts/Get-MainLineDialParam?key=UserAccount&value=";
const BaseUrlRestApiExtension = MyFramesBase + "api/powershell/scripts/Get-MainLineExtension?key=UserAccount&value=";

function AddSalt(url) {
    let salt = "salt=" + Math.random().toString(36).substr(2, 5);
    if (url.includes("?"))
        return url + "&" + salt;
    else
        return url + "?" + salt;
}
function GetMyFramesBase() {
    let base = "";
    if (window.location.protocol.toLowerCase().startsWith("http")) {
        let path = window.location.pathname;
        let page = path.split("/").pop();
        path = path.replace(page, "");
        return window.location.origin + path;
    } else {
        return "https://sbc.subfifty.net/xphoneconnect/myframes/"
        //return "http://10.1.1.54/xphoneconnect/myframes/"
    }
}
function GetXPhoneServer() {
    let url = new URL(window.location.href);
    if (url.protocol.toLowerCase().startsWith("http")) {
        return url.protocol + "//" + url.host;
    } else {
        return "https://sbc.subfifty.net";
    }
}
