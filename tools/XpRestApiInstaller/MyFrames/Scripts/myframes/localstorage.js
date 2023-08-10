var LS = {
    tokenAccessToken: "accessToken",
    tokenRefreshToken: "refreshToken",
    tokenUserName: "userName",
    tokenFullName: "fullName",
    tokenLru_uid: "lru_uid",
    tokenLru_serviceurl: "lru_serviceurl",
    GetAccessToken: function () {
        return localStorage.getItem(this.tokenAccessToken);
    },
    SetAccessToken: function (value) {
        localStorage.setItem(this.tokenAccessToken, value);
    },
    GetRefreshToken: function () {
        return localStorage.getItem(this.tokenRefreshToken);
    },
    SetRefreshToken: function (value) {
        localStorage.setItem(this.tokenRefreshToken, value);
    },
    GetUserName: function () {
        return localStorage.getItem(this.tokenUserName);
    },
    SetUserName: function (value) {
        return localStorage.setItem(this.tokenUserName, value);
    },
    GetFullName: function () {
        return localStorage.getItem(this.tokenFullName);
    },
    SetFullName: function (value) {
        return localStorage.setItem(this.tokenFullName, value);
    },
    GetLruUid: function () {
        return localStorage.getItem(this.tokenLru_uid);
    },
    SetLruUid: function (value) {
        return localStorage.setItem(this.tokenLru_uid, value);
    },
    GetLruServiceUrl: function () {
        return localStorage.getItem(this.tokenLru_serviceurl);
    },
    SetLruServiceUrl: function (value) {
        return localStorage.setItem(this.tokenLru_serviceurl, value);
    }
};
