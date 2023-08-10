$(function () {
	$.widget("c4b.tpleditor", {
		// default options
		options: {
			val: "",
			// callbacks
			container: null,
			button: null,
			dialog: null,
			input: null,
			contactMenu: null,
			callMenu: null,
			userMenu: null,
			snippetMenu: null,
			textarea: null,
			fields: [],
			syntax: null
		},
		_create: function () {
			var tpled = this;
			this.element.addClass("c4b-tpleditor-input");
			this.input = this.element;
			this.container = this.element.wrap('<div></div>').parent()
				.addClass("c4b-tpleditor")

			// open dialog button
			this.button = $("<button></button>", {
				"class": "c4b-tpleditor-button",
			})
				.appendTo(this.container)
				.button({
					text: false
				})
				.click(function (e) {
					e.preventDefault();
					tpled.textarea.val(tpled.input.val())
					tpled.dialog.dialog('open');
				});

			var tpl = this;
			new MutationObserver(
				function (mutations, observer) {
					$(mutations[0].target).next().css('display', mutations[0].target.style.display);
				}
			).observe(this.input.get(0), { attributes: true });


			// add dialog, if not already exists
			var snippetBtn = this.option('snippets') != null ? '<button class="c4b-button-tpled c4b-tpleditor-form-btn-snippets" >Snippets</button>' : ''
			var txt = $("label[for='" + $(this.element).attr('id') + "']").text()
			if (!txt)
				txt = $(this.element).parent().parent().text()
			var t = `<div class="c4b-tpleditor-form" title="${txt}">
				<form><fieldset>
					<textarea style="width:100%" rows="40" class="c4b-tpleditor-form-txt"></textarea>
				</fieldset></form>
				<button class="c4b-button-tpled c4b-tpleditor-form-btn-call-field" >Call State</button>
				<button class="c4b-button-tpled c4b-tpleditor-form-btn-user-field" >User</button>
				<button class="c4b-button-tpled c4b-tpleditor-form-btn-contact-field" >Contact</button>
				${snippetBtn}
				<button class="c4b-button c4b-tpleditor-form-btn-breakpoint" >Breakpoint</button>
`

			t += '<ul class="c4b-tpleditor-contact-menu">'
			$.each(this.option('contactFields'), function (i, field) {
				t += '<li>' + field + '</li>';
			})
			t += '</ul>'

			t += '<ul class="c4b-tpleditor-call-menu">'
			$.each(this.option('callFields'), function (i, field) {
				t += '<li>' + field + '</li>';
			})
			t += '</ul>'
			t += '<ul class="c4b-tpleditor-user-menu">'
			$.each(this.option('userFields'), function (i, field) {
				t += '<li>' + field + '</li>';
			})
			t += '</ul>'
			t += '<ul class="c4b-tpleditor-snippet-menu">'
			if (this.option('snippets') != null) {
				$.each(this.option('snippets'), function (i, snippet) {
					t += '<li data-code="' + snippet.Code.replace(/&/g, '&amp;').replace(/"/g, '&quot;') + '">' + snippet.Subject + '</li>';
				})
			}
			t += '</ul></div>'

			//console.log(t)
			$(t).appendTo(this.container);


			this.dialog = this.container.find('.c4b-tpleditor-form');

			// contact menu for field selection
			this.contactMenu = this.container.find('.c4b-tpleditor-contact-menu')
				.menu({
					select: function (event, ui) {
						var txt = '{Contact.' + ui.item.text() + '}';
						if (tpled.editor == null)
							tpled.textarea.insertAtCaret(txt);
						else
							tpled.editor.insert(txt);
						tpled.contactMenu.hide();
					}
				})
				.hide();

			// call menu for field selection
			this.callMenu = this.container.find('.c4b-tpleditor-call-menu')
				.menu({
					select: function (event, ui) {
						var txt = '{Call.' + ui.item.text() + '}';
						if (tpled.editor == null)
							tpled.textarea.insertAtCaret(txt);
						else
							tpled.editor.insert(txt);
						tpled.callMenu.hide();
					}
				})
				.hide();

			// user menu for field selection
			this.userMenu = this.container.find('.c4b-tpleditor-user-menu')
				.menu({
					select: function (event, ui) {
						var txt = '{User.' + ui.item.text() + '}';
						if (tpled.editor == null)
							tpled.textarea.insertAtCaret(txt);
						else
							tpled.editor.insert(txt);
						tpled.userMenu.hide();
					},
					create: function (event, ui) {
						//console.log('menucreate')
                    }
				})
				.hide();

			// snippet menu 
			this.snippetMenu = this.container.find('.c4b-tpleditor-snippet-menu')
				.menu({
					select: function (event, ui) {
						// decode html entities
						var txt = $("<div/>").html(ui.item.data('code')).text();
						if (tpled.editor == null)
							tpled.textarea.insertAtCaret(txt);
						else
							tpled.editor.insert(txt);
						tpled.snippetMenu.hide();
					}
				})
				.hide();

			$('body').on('click', function (event) {
				if(!$(event.target).hasClass('c4b-tpleditor-form-btn-contact-field'))
					tpled.contactMenu.hide();
				if (!$(event.target).hasClass('c4b-tpleditor-form-btn-call-field'))
					tpled.callMenu.hide();
				if (!$(event.target).hasClass('c4b-tpleditor-form-btn-user-field'))
					tpled.userMenu.hide();
				if (!$(event.target).hasClass('c4b-tpleditor-form-btn-snippets'))
					tpled.snippetMenu.hide();
			})

			this.textarea = this.container.find('.c4b-tpleditor-form-txt');

			var buttons = [
				{
					text: ResX.Cancel,
					class: 'c4b-button secondary',
					click: function () {
						tpled.dialog.dialog("close");
					}
				},
				{
					text: ResX.SaveAndClose,
					class: 'c4b-button primary',
					click: function () {
						if (tpled.editor != null) {
							var v = tpled.editor.getValue().replace(/(\r\n|\n|\r)/gm, '\\n')
							tpled.input.val(v);
						}
						else
							tpled.input.val(tpled.textarea.val());
						tpled.input.trigger("change");
						tpled.dialog.dialog("close");
					}
				}
			]

			this.container.find('.c4b-tpleditor-form-btn-call-field').click(function (e) { tpled.callMenu.show().focus().position({ my: "left bottom", at: "left top", of: e.target}) });
			this.container.find('.c4b-tpleditor-form-btn-user-field').click(function (e) { tpled.userMenu.show().focus().position({ my: "left bottom", at: "left top", of: e.target }) });
			this.container.find('.c4b-tpleditor-form-btn-contact-field').click(function (e) { tpled.contactMenu.show().focus().position({ my: "left bottom", at: "left top", of: e.target }) });
			this.container.find('.c4b-tpleditor-form-btn-snippets').click(function (e) { tpled.snippetMenu.show().focus().position({ my: "left bottom", at: "left top", of: e.target }) });
			this.container.find('.c4b-tpleditor-form-btn-breakpoint').click(function (e) {
				e.preventDefault();
				e.stopPropagation();
				if (tpled.editor == null)
					tpled.textarea.insertAtCaret('if(XP.IsDebug) debugger;;\n');
				else
					tpled.editor.insert('if(XP.IsDebug) debugger;\n');
			});

            
			this.dialog.dialog({
				dialogClass: 'c4b-tpleditor-dialog',
				autoOpen: false,
				width: 900,
				height: 550,
				modal: true,
				resizable: false,
				draggable: false,
				open: function (event, ui) {
				    var editor = null;
				    // syntax highlighting
				    if (tpled.options.syntax != null && tpled.editor == null) {
				        editor = ace.edit(tpled.textarea[0]);
				        editor.setTheme("ace/theme/tomorrow");
				        var session = editor.getSession();

                        // disable warnings regarding doctype or unexprected end of file
				        session.on("changeAnnotation", function () {
				            var annotations = session.getAnnotations() || [], i = len = annotations.length;
				            while (i--) {
				                if (/doctype first\. Expected/.test(annotations[i].text)) {
				                    annotations.splice(i, 1);
				                }
				                else if (/Unexpected End of file\. Expected/.test(annotations[i].text)) {
				                    annotations.splice(i, 1);
				                }
				            }
				            if (len > annotations.length) {
				                session.setAnnotations(annotations);
				            }
				        });
				        session.setMode("ace/mode/" + tpled.options.syntax);
				        tpled.editor = editor;
				    }

				    if (tpled.editor != null) {
				        tpled.editor.setValue(tpled.input.data('value').replace(/\\n/gm, '\n').replace(/scr__ipt>/gm, 'scr' + 'ipt>'));
				        tpled.editor.focus();
				        tpled.editor.clearSelection();
				        tpled.editor.resize();
				    }
				},
				resize: function (event, ui) {
				    if (tpled.editor != null) {
				        $('.ace_editor').height($('.c4b-tpleditor-form').height() - 40);
				        tpled.editor.resize();
				    }
				},
				buttons: buttons,
				close: function () {
					
				}
			});

		},
		// events bound via _on are removed automatically
		// revert other modifications here
		_destroy: function() {
			// remove generated elements
			this.button.remove();
		},
	})
})