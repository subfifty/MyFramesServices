$(function () {
    // accordion
    $('.c4b-accordion > div:first-child').click(function () {
        $(this).parent().toggleClass('open')
    })

    // input
    $('.c4b-input input').attr('placeholder', ' ')
    $('.c4b-input').contents().filter(function () { return this.nodeType === 3 && $.trim(this.nodeValue) !== ''; }).wrap('<span/>');
})

function cb4_input_error(selector, error) {
    $(selector).each(function (index) {
        if (error) {
            // add error message
            var t = $(this).parent().children('span')
            if (!t.length)
                t =$('<span>aa</span>').appendTo($(this).parent())
            t.text(error)
            $(this).parent().addClass('c4b-error')
        }
        else {
            // remove error message
            var t = $(this).parent().children('span')
            var l = t.attr('data-label')
            if (l) {
                t.text(l)
                t.removeAttr('data-label')
            }
            else 
                t.remove()
            $(this).parent().removeClass('c4b-error')
        }
    })
}

function cb4_input_warning(selector, warning) {
    $(selector).each(function (index) {
        if (warning) {
            // add error message
            var t = $(this).parent().children('span')
            if (!t.length)
                t = $('<span>aa</span>').appendTo($(this).parent())
            t.text(warning)
            $(this).parent().addClass('c4b-warning')
        }
        else {
            // remove error message
            var t = $(this).parent().children('span')
            var l = t.attr('data-label')
            if (l) {
                t.text(l)
                t.removeAttr('data-label')
            }
            else
                t.remove()
            $(this).parent().removeClass('c4b-warning')
        }
    })
}