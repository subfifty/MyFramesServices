$(function() {
    $.widget("custom.combobox", {

        options: {
            value: '0',
            source: '',
            autocomplete: null,
            select: null
        },

        _create: function () {

            // wrapper
            this.wrapper = this.element.wrap('<div></div>').parent();
            this.wrapper.addClass('custom-combobox');

            // input
            this.input = this.element;
            this.input
                .addClass('custom-combobox-input')

            var options = this.options;

            if(this.options.autocomplete != null) {
                this.input.autocomplete( this.options.autocomplete )
            } else {
                this.input.autocomplete({
                    delay: 0,
                    minLength: 0,
                    source: this.options.source,
                    select: function (event, ui) {
                        if (options.select != null) {
                            options.select(event, ui);
                        }
                    }
                })
            }
            // button
            var input = this.input;
            $("<a>")
                .attr("tabIndex", -1)
                .appendTo(this.wrapper)
                .button({
                    icons: {
                        primary: "ui-icon-triangle-1-s"
                    },
                    text: false
                })
                .removeClass("ui-corner-all")
                .addClass("custom-combobox-toggle ui-corner-right")
                .mousedown(function () {
                    wasOpen = input.autocomplete("widget").is(":visible");
                })
                .click(function () {
                    input.focus();

                    // Close if already visible
                    if (wasOpen) {
                        return;
                    }

                    // Pass empty string as value to search for, displaying all results
                    input.autocomplete("search", "");
                });

        },

        value: function (value) {

            // No value passed, act as a getter.
            if (value === undefined) {
                return this.options.value;
            }

            // Value passed, act as a setter.
            this.input.val(value);
        },

        _destroy: function() {
        }
    })
})