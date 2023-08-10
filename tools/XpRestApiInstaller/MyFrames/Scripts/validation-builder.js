if (!String.prototype.startsWith) {
    String.prototype.startsWith = function (searchString, position) {
        position = position || 0;
        return this.indexOf(searchString, position) === position;
    };
}

function getValidationFields() {
    var filter = $.map(contactFields, function (val, i) { return { id: 'Contact.' + val, label: 'Contact.' + val, type: 'string' }; })
    filter = filter.concat($.map(callFields, function (val, i) { return { id: 'Call.' + val, label: 'Call.' + val, type: 'string' }; }))
    filter = filter.concat($.map(userFields, function (val, i) { return { id: 'User.' + val, label: 'User.' + val, type: 'string' }; }))

    filter.push({ id: 'Contact.DBAPPLICATION_MSCRM2011', label: 'Contact.DBAPPLICATION_MSCRM2011', type: 'string' });
    filter.push({ id: 'Contact.DBAPPLICATION_MSCRM2013', label: 'Contact.DBAPPLICATION_MSCRM2013', type: 'string' });
    filter.push({ id: 'Contact.DBENTITYTYPE_CONTACT', label: 'Contact.DBENTITYTYPE_CONTACT', type: 'string' });

    return filter;
}

function setRules(txt, ctrl, ddl, validation) {
    $(txt+', '+ctrl+', '+ddl).hide();
    $(ctrl).queryBuilder('reset');
    if (validation == null || validation == "") {
        $(ctrl).hide();
        $(ddl).show();
        $(ddl).val(0);

    }
    else {
        $(txt + ', ' + ctrl + ', ' + ddl).hide();
        var rules = null;
        try {
            rules = stringTemplate2Rules(validation);
            if (rules != null) {
                // if parsable show querybuilder
                $(ctrl).queryBuilder('setRules', rules);
                $(ddl).val(1);
                $(ctrl + ', ' + ddl).show();
            }
            else {
                $(txt).show();
                $(ctrl + ', ' + ddl).hide();
            }
        } catch (e) {
            console.log('Failed to parse validation!', e.message);
            $(txt).show();
            $(ctrl + ', ' + ddl).hide();
        }
    }
}


// parse stringtemplate validation template to JSON object
function stringTemplate2Rules(s) {
    s = s.replace(/\{if\((.+)\)\}valid\{endif\}/gi, '$1'); // remove if/endif

    var expressions = [];
    var i = 1;
    var s1 = "";

    while (s1 != s) {
        s1 = s;
        s = s.replace(/\(([^()]+)\)/g, function (match, submatch) {
            var name = "E" + (i++);
            expressions[name] = submatch;
            return name;
        })
    }

    var rules = convert(s);

    function convert(s) {
        var r = {};
        s = s.split(/(&&|\|\|)/);
        r.condition = s[1] == '&&' ? 'AND' : 'OR';
        r.rules = []
        for (var i = 0; i < s.length; i += 2) {
            var name = s[i].trim();
            if (typeof (expressions[name]) != 'undefined') {
                r.rules.push(convert(expressions[name]));
            }
            else {
                var rule = { type: 'string', value: null, input: 'text' };
                rule.operator = name.startsWith('!') ? 'is_empty' : 'is_not_empty';
                name = name.replace('!', '');
                rule.id = name;
                rule.field = name;
                r.rules.push(rule);
            }
        }
        return r;
    }
    return rules;
}

// convert rules object to stringtemplate
function rules2StringTemplate(rules) {
    if (!rules)
        return '';
    var result = "{if";
    result += _rules2StringTemplate(rules);
    result += '}valid{endif}';
    console.log(result);
    return result;
}

function _rules2StringTemplate(rules) {
    var result = '(';
    var op = rules.condition == 'AND' ? ' && ' : ' || ';
    var o = "";
    for (var i in rules.rules) {
        var rule = rules.rules[i];
        if (typeof rule.condition != "undefined") {
            result += o + _rules2StringTemplate(rule);
        }
        else {
            result += o + (rule.operator == 'is_empty' ? '!' : '') + rule.field;
        }
        o = op;
    }
    return result + ')';
}
