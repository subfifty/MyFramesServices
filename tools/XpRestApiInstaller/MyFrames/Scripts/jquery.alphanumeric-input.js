$(function () {
    $.widget("c4b.alphanumeric", {
        // default options
        options: {
            val: "",
            input: null,
            replacechar: '_'
        },
        _create: function () {
            this.input = this.element;
            var replacechar = this.options.replacechar;
            $(this.input).off('keyup');
            $(this.input).on('keyup', function (e) {
                var start = this.selectionStart;
                var end = this.selectionEnd;
                $(this).val($(this).val().replace(/[\W_]+/g, replacechar));
                // Move the caret
                this.selectionStart = start;
                this.selectionEnd = end;
            })

        },
        // events bound via _on are removed automatically
        // revert other modifications here
        _destroy: function () {
            // remove generated elements
        },
    })
})