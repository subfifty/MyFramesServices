XP.Contacts = {}
XP.Contacts_list = []
XP.Views = [];
XP.Client = {}
XP.IsDebug = getQueryParam("start_debugger") == 1;
var contactFields = ['ACCOUNTCODE', 'ADAPTER', 'ADAPTERCLASSIFICATION', 'ADAPTERDISPLAYNAME', 'AGGREGATEDMATCH', 'BIRTHDAY', 'CATEGORY', 'CITY1', 'CITY2', 'CNOCOMPANY', 'CNOFAX', 'CNOFAX2', 'CNOMAIN', 'CNOMOBILE', 'CNOOTHER', 'CNOPRIVATE', 'COMPANY', 'COMPANY2', 'COUNTRY1', 'COUNTRY2', 'CREATENEWDATA', 'CUSTOMERNO', 'DBAPPLICATION', 'DBAPPVERSION', 'DBDATABASE', 'DBENTITYTYPE', 'DBINSTANCE', 'DBNATIVEID', 'DBPARENTID', 'DBSERVER', 'DEPARTMENT', 'DISTINGUISHEDNAME', 'EMAIL1', 'EMAIL2', 'EMAIL3', 'FIRSTNAME', 'FUZZYMATCH', 'HASPHOTO', 'ICONINDEX', 'ID', 'NAME', 'NOTE', 'POSITION', 'PRIORITY', 'PRIVATE', 'SALUTATION', 'SELECTORDISPLAYNAME', 'SELECTORID', 'STATE1', 'STATE2', 'STREET1', 'STREET2', 'TEMP', 'TITLE', 'USERDEF_1', 'USERDEF_10', 'USERDEF_11', 'USERDEF_12', 'USERDEF_13', 'USERDEF_14', 'USERDEF_15', 'USERDEF_16', 'USERDEF_17', 'USERDEF_18', 'USERDEF_19', 'USERDEF_2', 'USERDEF_20', 'USERDEF_3', 'USERDEF_4', 'USERDEF_5', 'USERDEF_6', 'USERDEF_7', 'USERDEF_8', 'USERDEF_9', 'VDIR', 'WEATHERIMAGE', 'WEB', 'ZIP1', 'ZIP2',]
contactFields.sort();
var callFields = ['CALLEDEMAIL', 'CALLEDNO', 'CALLNO', 'DISPLAYNAME', 'DURATION', 'IsEmpty', 'REDIRECTCALLNO', 'REDIRECTDISPLAYNAME', 'REDIRECTTYPE', 'STATE', 'TIME',]
callFields.sort();
var userFields = ['CITY1', 'CNOCOMPANY', 'CNOFAX', 'CNOMAIN', 'CNOMOBILE', 'CNOPRIVATE', 'COMPANY', 'DEPARTMENT', 'EMAIL1', 'FIRSTNAME', 'NAME', 'POSITION', 'SALUTATION', 'STATE1', 'STREET1', 'USERDEF_1', 'USERDEF_10', 'USERDEF_11', 'USERDEF_12', 'USERDEF_13', 'USERDEF_14', 'USERDEF_15', 'USERDEF_16', 'USERDEF_17', 'USERDEF_18', 'USERDEF_19', 'USERDEF_2', 'USERDEF_20', 'USERDEF_3', 'USERDEF_4', 'USERDEF_5', 'USERDEF_6', 'USERDEF_7', 'USERDEF_8', 'USERDEF_9', 'ZIP1',]
userFields.sort();

$(function () {
    $('#pnl_waiting').hide();
    if (!XP.IsDebug) {
        // add pseudo client for preview purposes
        params = {}; location.search.replace(/[?&]+([^=&]+)=([^&]*)/gi, function (s, k, v) { params[k.toLowerCase()] = v })
        var client = params['client'];
        if (params['preview']) {
            $('body').append($('template#' + client).html())
            $('.content').detach().appendTo('#preview-client-content')
            $('[name="preview_client_iframe"]').remove()
        }

        $("#NodesToMove").detach().appendTo('#DestinationContainerNode')
        init();
    }
    else {
        $('#pnl_debugger').show();
        window.onkeydown = function (e) {
            if (e.keyCode == 123) {
                setTimeout(
                    function () {
                        $('#pnl_debugger').hide();
                        init();
                    }, 5000)
            }
        }
    }
})

function init() {

    // set maximum size for client
    if (XP.Client.MaximumWidth != 0) {
        $('.content').css('max-width', (XP.Client.MaximumWidth) + 'px')
            .css('overflow', 'hidden')
    }
    if (XP.Client.MaximumHeight != 0) {
        $('.content').css('max-height', (XP.Client.MaximumHeight) + 'px')
            .css('overflow', 'hidden')
    }

    // remove contacts with missing dashboard
    var selector_ids = $('#ddl_contacts option').map(function (i, option) { return option.value }).get();
    $.each(selector_ids, function (i, selector_id) {
        if (typeof (XP.Contacts[selector_id]) == 'undefined')
            $("#ddl_contacts option[value='" + selector_id + "']").remove();
    })

    $('#ddl_contacts').change(function () {
        showContact();
    })

    if (XP.Contacts_list.length == 0)
        $('#ddl_contacts').replaceWith("<i>Kein Eintrag vorhanden.</i>")
    else {
        XP.Contact = XP.Contacts_list[0];
        showContact();
        $('.view').each(function (i, view) {
            XP.Contacts_list[i].Height = $(view).parent().parent().outerHeight();
            XP.Contacts_list[i].Width = $(view).parent().parent().outerWidth();
        })
    }


    if (typeof (window.external) != "undefined" && typeof (window.external.SearchCompleted) != "undefined") {
        try {
            if (XP.Contacts_list.length == 0 && getQueryParam('debug') == 1) {
                XP.Contacts_list[0] = { Height: 30, Width: 100, Index: 0 };
            }
            window.external.SearchCompleted(JSON.stringify(XP.Contacts_list))
        } catch (e) {
            alert("Error: " + e)
        }
    }

    // show log/debug button only, if it's no debug session
    if (typeof (debugInfo) != 'undefined' && !XP.IsDebug) {
        $('body').append('<div id="btn_log">Log/Debug</div>');
        $('#btn_log').click(function () {
            var win = window.open("Dashboard/DebugInfo", "_blank");
        })

        // send debugInfo entries to client log
        try {
            var filter = $.map(contactFields, function (val, i) { return { id: 'Contact.' + val, label: 'Contact.' + val, type: 'string' }; })
            filter = filter.concat($.map(callFields, function (val, i) { return { id: 'Call.' + val, label: 'Call.' + val, type: 'string' }; }))
            filter = filter.concat($.map(userFields, function (val, i) { return { id: 'User.' + val, label: 'User.' + val, type: 'string' }; }))

            $('#querybuilder').queryBuilder({
                filters: filter,
                operators: ['equal', 'not_equal', 'contains', 'not_contains', 'begins_with', 'ends_with', 'less', 'greater', 'is_empty', 'is_not_empty','is_true', 'is_false'/*, 'bitwise_and'*/,'bit_set', 'bit_cleared']
            });

            $.each(debugInfo.Sections, function (section, v) {
                if (debugInfo.Sections[section].length > 0) {
                    $.each(v, function (i, line) {
                        if (line.Message.match(/\$§\$/)) {
                            // handle query builder rules
                            var s = line.Message.split('$§$');
                            var rules = s[1];
                            $('#querybuilder').queryBuilder('setRules', JSON.parse(rules));
                            var sql_raw = $('#querybuilder').queryBuilder('getSQL', false, true);
                            XP.LogInfo(s[0] + ' Additional condition mismatch (' + sql_raw.sql + ') ' + (s.length > 2 ? s[2] : ''), line.Status);
                        }
                        else
                            XP.LogInfo(line.Message, line.Status);
                    });
                }
            })
        } catch (e) {
            console.log('Logging by client failed: ' + e);
        }

        // js error handler for debugInfo page
        window.addEventListener('error', function (e) {
            if (e.error) {
                if (debugInfo.Sections['SECTION_JAVASCRIPT'] === undefined)
                    debugInfo.Sections['SECTION_JAVASCRIPT'] = [];
                debugInfo.Sections['SECTION_JAVASCRIPT'].push({ Message: '<pre>' + e.error.message + "<br>" + e.error.stack + '</pre>', Status: 0 });
                $('#btn_log').css("background-color", "red");
                $('#btn_log').fadeTo('fast', 0).fadeTo('fast', 0.4).fadeTo('fast', 0).fadeTo('fast', 0.2)
            }
        });
    }

    // add debugger statement to views
    if (XP.IsDebug) {
        for (var view in XP.Views) {
            //XP.Views[view] = '<script>debugger</script>' + XP.Views[view];
        }
    }

    new ResizeSensor($('.content'), function () {
        if (window.external && window.external.OnSizeChanged) {
            window.external.OnSizeChanged();
            console.log('content size changed');
        }
    });

}


function getQueryParam(key) {
    key = key.replace(/[*+?^$.\[\]{}()|\\\/]/g, "\\$&"); // escape RegEx control chars
    var match = location.search.match(new RegExp("[?&]" + key + "=([^&]+)(&|$)",'i'));
    return match && decodeURIComponent(match[1].replace(/\+/g, " "));
}

function executeAutostartActions() {
    var isUnique = (XP.Contacts_list.length == 1);
    // execute actions
    $("[data-autostart='ActionAutostart'],[data-autostart='ActionAutostartHide']").each(function (i, el) {
        el.childNodes[0].click();
    });
    if (isUnique) $("[data-autostart='ActionAutostartUnique'],[data-autostart='ActionAutostartUniqueHide']").each(function (i, el) {
        if (el && el.childNodes && el.childNodes[0])
        el.childNodes[0].click();
    });

    // hide actions
    $("[data-autostart='ActionAutostartHide']").hide();
    if (isUnique) $("[data-autostart='ActionAutostartUniqueHide']").hide();

}

function showContact() {
    var selector_id = $('#ddl_contacts').val();
    if (typeof (selector_id) == 'undefined')
        selector_id = XP.Contact.SELECTORID || '';
    $('.view-container').html('');
    var view = XP.Views[selector_id];
    $('#CONTAINER_' + selector_id).html(view);
    XP.Contact = XP.Contacts[selector_id];
    validate();
    setAttrs(selector_id);
    //var i = 0;
    //while (typeof window["autoexec_" + selector_id + "_" + i] === 'function') {
    //    window["autoexec_" + selector_id + "_" + i]();
    //    i++;
    //}

    //// replace # in urls in DashboardClient, workaround for bug in webcontrol
    //try {
    //    if (typeof(window.external) != 'undefined' && typeof(window.external.SearchCompleted) != 'undefined') {
    //        $('a[target]').each(function (i, el) {
    //            var href = $(el).attr('href');
    //            if (href.indexOf('#') > -1)
    //                $(el).attr('href', href.replace(/#/, '§§§'));
    //        });
    //    }
    //} catch (e) {
    //    alert(e);
    //}

    _copyStylesFromParent()

    // bugfix separator
    $('.HtmlComponentSeparator').attr('style', '')

    try {
        if (window.parent === undefined || window.parent.currentTab === undefined || window.parent.ViewForm.visible() && window.parent.currentTab == 3 || !window.parent.ViewForm.visible())
            executeAutostartActions();
        if (parent.refresh_test_viewid)
            parent.refresh_test_viewid()
    } catch (e) {
        console.error('autostart error:', e)
    }
}

function _copyStylesFromParent() {
    $('.dashboard-action').each(function (i, el) {
        try {
            var styles = $(el).parent().attr('style').split(';').filter(function (s) { return s.startsWith('background:') || s.startsWith('background-color:') || s.startsWith('color') || s.startsWith('text-decoration') }).join(';')
            $(el).attr('style', $(el).attr('style') + ';' + styles)
        } catch (e) {
            console.info('error in _copyStylesFromParent', e)
        }
    })
}

function validate() {
    var contactId = $('#ddl_contacts').val();
    if (typeof (contactId) == 'undefined')
        contactId = $('.view').attr('id');
    $('[data-validation]').each(function (i, el) {
        el = $(el);
        var template = el.data('validation');
        if (typeof (template) != 'undefined' && render(contactId,template) != 'valid') {
            el.hide();
        }
    })
}

var htmljs_counter = 0;
// set loader attributes to child elements (lazy loading)
function setAttrs(contactId) {
    htmljs_counter = 0;
    $('#' + contactId).find('[data-attrs]').each(function (i, cell) {
        cell = $(cell)
        attrs = cell.data('attrs');
        $.each(attrs, function (j, attr_val) {
            cell.find('[data-loader]').each(function (k, el) {
                el = $(el)
                loader = eval('(' + el.data('loader') + ')');
                if (loader.id = attr_val.id) {
                    $.each(attr_val.attrs, function (l, attr) {
                        var v = render(contactId, attr.value).replace('$$LINE_BREAK$$', '\n');
                        if (attr.attr == 'text')
                            el.text(v);
                        else if (attr.attr == 'html') {
                            h = $('<div>').append(v.replace(/scr__ipt>/gm, 'scr' + 'ipt>'));
                            $.each($('script', h), function (i, s) {
                                //$(s).html('function autoexec_' + contactId + '_' + htmljs_counter++ + '() { \n' + render(contactId, $(s).html()) + '\n}');
                                $(s).html(render(contactId, $(s).html()));
                            })
                            el.html(h.html());
                        }
                        else
                            el.attr(attr.attr, v);
                    })
                }
            })
        })
    })

    // set styles for action from parent cell
    _copyStylesFromParent()
}

function render(contactId, template) {
    if (template.match(/[{(](Contact|Call|User)\./)) {
        var c = XP.Contacts[contactId];
        delete c.DN;
        delete c.SelectorDisplayname;
        delete c.Index;
        $.ajax({
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            type: 'POST',
            url: serverRoot + 'Admin/RenderTemplate',
            async: false,
            data: JSON.stringify({ template: template, contact: c, call: XP.Call, user: XP.User}),
            success: function (data) {
                template = data.rendered;
            }
        })
    }
    return template;
}
