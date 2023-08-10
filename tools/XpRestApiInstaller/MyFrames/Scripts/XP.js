
var XP = {
    ViewID: '',
    ViewCulture: '',
    Contact: {},
    Call: {},
    User: {},
    Data: {},
    external: null,
    IsDebug: false,
    FormToObject: function (form) {
        var d = {};
        $(form).serializeArray().map(function (x) { d[x.name] = x.value; });
        return d;
    },
    SerializeForm: function (form) {
        var d = {};
        $(form).serializeArray().map(function (x) { d[x.name] = x.value; });
        return JSON.stringify(d);
    },
    SerializeFormToUrlParams: function (form) {
        return $(form).serialize();
    },
    ExecAction: function (applicationID, actionID, param, successHandler, errorHandler, timeout) {
        // now you can add XP.Contact, XP.Call, XP.User or XP.Data to watch window, to show it's contents
        if (this.IsDebug)
            debugger;

        // clean XP objects from unneeded properties
        var contact_clean = jQuery.extend({}, XP.Contact);
        delete contact_clean.Height;
        delete contact_clean.Width;
        delete contact_clean.SelectorDisplayname;
        delete contact_clean.Index;
        var call_clean = jQuery.extend({}, XP.Call);
        delete call_clean.IsEmpty;

        // prepare contextData as JSON string
        var contextData = $('<textarea />').html(JSON.stringify({ XP: { Contact: contact_clean, Call: call_clean, User: XP.User, Data: XP.Data } }, function (key, value) {
            if (typeof value == 'undefined' || value == null || typeof value == 'string' && value == '') {
                return undefined;
            }
            return value;
        })).text();
        if (this.external) {
            try {
                // call executor
                this.external.ExecAction(applicationID, actionID, param, contextData, successHandler, errorHandler, timeout);
            } catch (e) {
                // handle old version signature
                if (e.name == 'TypeError') {
                    this.external.ExecAction(applicationID, actionID, param);
                }
                else
                    alert(e)
            }
        }
        else {
            if (window.parent && window.parent.$ && window.parent.$.notify)
                window.parent.$.notify(ResX.CantExecInPreview, { globalPosition: "top center", className: 'warning' });

        }
    },
    AddAppLinkData: function (key, value) {
        XP.Data[key] = value;
    },
    CallSoapService: function (url, message) {
        var response;
        $.ajax({
            url: serverRoot + 'CORSProxy.aspx?url=' + encodeURI(url),
            type: "POST",
            dataType: "xml",
            async: false,
            data: message,
            contentType: 'text/xml; charset="utf-8"',
            success: function (data, status) {
                response = $.xml2json(data);
                if (typeof (response["#document"]) != 'undefined'
                    && typeof (response["#document"]["soap:Envelope"]) != 'undefined'
                    && typeof (response["#document"]["soap:Envelope"]["soap:Body"]) != 'undefined')
                    response = response["#document"]["soap:Envelope"]["soap:Body"];
            },
            processData: false,
            error: function (request, status, error) { alert(error) }
        });

        return response;
    },
    PreventClose: function () {
        if (typeof (window.external.PreventClose) != "undefined")
            window.external.PreventClose()
    },
    OnLoad: function (onload) {
        $(onload);
    },
    BuildAccordionTable: function (output, data, title, onSubject, onDetail) {
        // handle empty callbacks
        if (!onSubject || onSubject == null)
            onSubject = function (row) { return row.Subject };
        if (!onDetail || onDetail == null)
            onDetail = function (row) { return row.Detail };

        // set width of this xp-cell to 100% if not already set
        var cell = $(output).parents('.xp-cell');
        if (typeof (cell.attr('style')) == 'undefined' || (';' + cell.attr('style').toLowerCase()).indexOf(';width:') == -1)
            cell.css('width', '100%');

        // handle title
        var t = '';
        if (title && title.length != 0)
            t = '<div class="xpc-accordion-table-title">' + title + '</div>';
        // build table divs
        data.forEach(function (row) {
            t += '<div class="xpc-accordion-table-subject">' + onSubject(row) + '<span>+</span></div>';
            t += '<div class="xpc-accordion-table-detail">' + onDetail(row) + '</div>';
        })
        output.innerHTML = t;

        $('.xpc-accordion-table-detail').hide();

        // attach click handler to subject divs
        $('.xpc-accordion-table-subject').on('click', function () {
            var is_open = $(this).next().is(":visible");
            $('.xpc-accordion-table-detail').slideUp('fast');
            $('.xpc-accordion-table-subject').find('span').text('+');
            if (!is_open) {
                $(this).next().slideToggle('fast');
                $(this).find('span').text('-');
            }
        });
    },
    BuildSimpleTable: function (output, data, title, onRow) {
        // handle title
        if (title && title.length != 0)
            t = '<div class="xpc-simple-table-title">' + title + '</div>';

        // handle empty callback
        if (!onRow || onRow == null)
            onRow = function (row) { return row };

        data.forEach(function (row) { t += '<div class="xpc-simple-table-row">' + onRow(row) + '</div>'; });
        output.innerHTML = t;
    },
    LogError: function (message) {
        if (typeof (window.external) != 'undefined' && typeof (window.external.LogDebugInfo) != 'undefined')
            window.external.LogDebugInfo(message, 0);
        console.error(message);
    },
    LogInfo: function (message) {
        if (typeof (window.external) != 'undefined' && typeof (window.external.LogDebugInfo) != 'undefined')
            window.external.LogDebugInfo(message, 1);
        console.info(message);
    },
    ClientMessage: function (message) {
        if (typeof (window.external) != 'undefined' && typeof (window.external.ClientMessage) != 'undefined') {
            return window.external.ClientMessage(message);
        }
        else
            console.log("window.external.ClientMessage is not defined");
    },
    OnClientMessage: function (handler) {
        XP.OnClientMessageHander = handler;
    },
    OnCallChanged: function (handler) {
        window.OnCallChanged = handler;
    }
};

$(function () {
    $.support.cors = true;

    // initialize needed function handlers on window.external for new (chromium) webview2
    if (window.chrome && window.chrome.webview) {
        window.external.SearchCompleted = function (json) {
            window.chrome.webview.postMessage(["SearchCompleted", json])
        };
        window.external.LogDebugInfo = function (message, status) {
            window.chrome.webview.postMessage(["LogDebugInfo", message, status])
        };
        window.external.PreventClose = function () {
            window.chrome.webview.postMessage(["PreventClose"])
        };
        window.external.OnSizeChanged = function () {
            window.chrome.webview.postMessage(["OnSizeChanged"])
        };
        window.external.ExecAction = function (applicationID, actionID, param, data, successHandler, errorHandler, timeout) {
            var getRandomName = function () { return ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, function (c) { return (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16) }) }
            var successHandlerName
            if (successHandler) {
                successHandlerName = successHandler.name
                // encapsulate anonymous functions in named function, to enable callback from webView2
                if (successHandlerName == '') {
                    var randomName = getRandomName()
                    successHandlerName = 'window['+randomName+']'
                    window[randomName] = successHandler
                }
            }
            var errorHandlerName
            if (errorHandler) {
                errorHandlerName = errorHandler.name
                // encapsulate anonymous functions in named function, to enable callback from webView2
                if (errorHandlerName == '') {
                    var randomName = getRandomName()
                    errorHandlerName = 'window[' + randomName + ']'
                    window[randomName] = errorHandler
                }
            }
            window.chrome.webview.postMessage(["ExecAction", applicationID, actionID, param, data, successHandlerName, errorHandlerName, timeout])
        };
        window.external.ClientMessage = function (message, status) {
            window.chrome.webview.postMessage(["ClientMessage", message])
        };
        window.chrome.webview.addEventListener('message', function(event) {
            if(event.data.ClientMessage)
                window.OnClientMessage(atob(event.data.ClientMessage))
            if (event.data.ExecAction) {
                if (event.data.ExecAction.success && event.data.ExecAction.success.handler)
                    eval(event.data.ExecAction.success.handler)(atob(event.data.ExecAction.success.resultBase64))
                if (event.data.ExecAction.error && event.data.ExecAction.error.handler)
                    eval(event.data.ExecAction.error.handler)(atob(event.data.ExecAction.error.resultBase64))
            }
        });
    }

    if ('ExecAction' in window.external)
        XP.external = window.external;
    else if (opener && opener.opener && 'ExecAction' in opener.opener.external)
        XP.external = opener.opener.external;

    window.OnClientMessage = function (message) {
        XP.LogInfo("OnClientMessage: " + message);

        // intercept & process CallInfo calls
        try {
            msg = JSON.parse(message)
            if (msg && msg.command == 'CallInfo') {
                var oldCall = Object.assign({}, XP.Call)
                XP.Call = msg.data;
                window.OnCallChanged(XP.Call, oldCall)
                return;
            }
        } catch (e) { }

        if ( typeof(XP.OnClientMessageHandler) == 'function')
            return XP.OnClientMessageHandler(message)
        else 
        return 'Please add handler for messages from Client to dashboard: XP.OnClientMessage(function(message) { dosomething(); return "some_return_value" })';
    };

    window.OnCallChanged = function (call, oldCall) {
        XP.LogInfo("OnCallChanged: " + JSON.stringify(call) );
        return 'Please add handler for Call state changed from Client to dashboard: XP.OnCallChanged(function(Call,oldCall) { if(Call.STATE != oldCall.STATE)... })';
    };

})
