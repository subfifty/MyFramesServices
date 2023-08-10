
var arrOver = [];
var itemCache = [];
var selectedItem = null;
var isResize = false;
var rowId = 1;
var cellId = 1;
var dropped;
var MAX_TREELEVEL = 5;
var $currentItem;
var treeHashCode = 0;
var scriptErrorTimeout = null;

var XP = {
    ExecAction: function () {
        alert("XP.ExecAction isn't available in Dashboard Designer");
    }
};

dashboard = {
    id: '#dashboard',
    saveid: '#save_placeholder',
    canvas: { style: '', active: true },
    type: '',
    rows: [{ cells: [] }],
    allCells: {},
    getCell: function (id) { return dashboard.allCells[id]; },
    drag: function (e, ui) {

        if (typeof ($currentItem) != 'undefined') {
            //console.log("drag over: " + $currentItem.attr('id'));
            $('.design-cell-selector, .design-row-selector').removeClass('selected');
            var o = $('#' + $($currentItem).attr('id') + '_selector');
            var w = $currentItem.outerWidth();
            var h = $currentItem.outerHeight();
            var x = $currentItem.offset().left;
            var y = $currentItem.offset().top;
            var oWidth = Math.min((w - 4) / 3, 20);
            var xo = ui.offset.left;
            var yo = ui.offset.top;
            //console.log("over: " + $currentItem.attr('id') + " x: "+x+" y: "+y+" xo: "+ui.offset.left+"  yo: "+ui.offset.top+"");

            if (xo >= x && xo <= x + w && yo >= y && yo <= y + h) {
                //console.log("drag over show: " + $currentItem.attr('id'));
                // ar = aspect ratio
                var ar = w / h;
                // xc = x from center of item scaled with aspect ratio
                var xc = (x - ui.offset.left + w / 2) / ar;
                // yc = y from center of item
                var yc = y - ui.offset.top + h / 2


                if (xc > yc) {
                    if (-xc > yc) {
                        //console.log('bottom');
                        // bottom
                        currentPos = 'bottom';
                        o.offset({ top: $currentItem.offset().top + 2 + (h - 4) / 3 * 2, left: $currentItem.offset().left + 2 });
                        o.width(w - 4);
                        o.height(((h - 4)) / 3);
                        o.removeClass('design-add-cell');
                        o.addClass('design-add-row');
                    }
                    else {
                        // left
                        //console.log('left');
                        currentPos = 'left';
                        o.offset({ top: $currentItem.offset().top + 2, left: $currentItem.offset().left + 2 });
                        o.width(oWidth);
                        o.height((h - 4));
                        o.removeClass('design-add-row');
                        o.addClass('design-add-cell');
                    }
                }
                else {
                    if (-xc > yc) {
                        // right
                        //console.log('right');
                        currentPos = 'right';
                        o.offset({ top: $currentItem.offset().top + 2, left: $currentItem.offset().left + w - oWidth });
                        o.width(Math.min((w - 4) / 3, 20));
                        o.height((h - 4));
                        o.removeClass('design-add-row');
                        o.addClass('design-add-cell');
                    }
                    else {
                        // top
                        //console.log('top');
                        currentPos = 'top';
                        o.offset({ top: $currentItem.offset().top + 2, left: $currentItem.offset().left + 2 });
                        o.width(w - 4);
                        o.height(((h - 4) / 3));
                        o.removeClass('design-add-cell');
                        o.addClass('design-add-row');
                    }
                }
                o.show();
                o.addClass('selected');
            }
            else {
                //console.log("drag over hide: " + $currentItem.attr('id'));
                o.hide();
                o.removeClass('selected');
                o.removeClass('design-add-cell');
                o.removeClass('design-add-row');
                o.removeAttr('style')
                this.draw()
            }
        }
    },
    draw: function (isSave) {
//        $('#btn_item_delete').hide();

        this.cleanup(dashboard.rows);
        if (this.rows.length == 0)
            this.rows = [{ cells: [] }];
        if (typeof (isSave) != 'undefined' && isSave)
            this.isSave = true;
        else
            this.isSave = false;

        var dashboardDivID = (this.isSave ? this.saveid : this.id).replace('#','')
        var dashboardDiv = $('#'+dashboardDivID);
        rowId = 1;
        cellId = 1;
        dashboard.allCells = {};
        this.canvas.style = $('.dashboard-canvas').attr('style') || ''
        if(this.canvas.active)
            dashboardDiv.html('<div class="dashboard-canvas" id="' + dashboardDivID + '-canvas" style="' + this.canvas.style + '"><div class="dashboard-canvas-selector"/></div>');
        else if(!isSave)
            dashboardDiv.html('<div class="dashboard-canvas" id="' + dashboardDivID + '-canvas" ></div>');
        else 
            dashboardDiv.html('');

        var el = dashboardDiv.children().length ? dashboardDiv.children()[0] : dashboardDiv

        //console.log('html:',$(el).html(), this.canvas)
        dashboard.drawRows($(el), this.rows)
        if (this.isSave) {
            itemCache = [];
            $(this.saveid).find('.design-row,.design-cell').removeAttr('id');
            $(this.saveid).find('.design-row').addClass('xp-row');
            $(this.saveid).find('.design-cell').addClass('xp-cell');
            $(this.saveid).find('.design-row,.design-cell').removeClass('design-row design-cell');

            // reorder attrs
            $('[data-attrs]').each(function (i, el) {
                $(el).attr('data-attrs', JSON.stringify(sortObjByKey($(el).data('attrs'))))
                //console.log(JSON.stringify(sortObjByKey(selectedItem.attrs), dashboard.stringifyAttr))
            })
            


            if (this.rows.length == 1 && this.rows[0].cells.length == 0)
                return "";
            return $(this.saveid).html()
                .replace(/\{(Contact|Call|User)\.(\w+)\}/g, '%[%$1.$2%]%')
                .replace(/{/g, '\\{')
                .replace(/}/g, '\\}')
                .replace(/%\[%/g, '{')
                .replace(/%\]%/g, '}')
                .replaceAll(' style=""','');
        }

        $('.design-row, .design-cell').each(function (i, item) {
            var el = $(item);
            $('<div id="' + el.attr('id') + '_selector" data-parentid="' + el.attr('id') + '" class="design-' + (el.hasClass('design-row') ? 'row' : 'cell') + '-selector" />').prependTo(el);
        })
        .draggable({
            helper: 'clone',
            //revert: 'invalid',
            revert: function (o) {
                if (o === false) {
                    dashboard.draw();
                    return true;
                }
                else {
                    return false;
                }
            },
            appendTo: 'body',
            cursorAt: { top: 0, left: 0 },
            start: function (event, ui) {
                console.log("drag start");
                draggedId = $(this).attr('id');
                $draggedElement = $(ui.helper);
                draggedCellId = draggedId;
                $(this).hide();
                //dashboard.deleteItem(cells[draggedId]);
                isDrag = true;
                //$("#tabs_dashboard").tabs("option", "active", 1);
                $('#btn_item_delete').hide();

                updateCanvasBorders();
            },
            drag: function (e, ui) {
                dashboard.drag(e, ui);
            },
            stop: function (e, ui) {
                console.log("drag stop");
                isDrag = false;
                dashboard.changed();
            }

        })

        // prevent click on link in design mode
        $('.dashboard-action').click(function (e) {
            e.preventDefault();
        })

        $('.design-row, .design-cell').droppable({
            tolerance: 'pointer',
            over: function () {
                //if (arrOver.length < MAX_TREELEVEL) {
                //    arrOver.push($(this));
                //    //console.debug("Over: " + arrOver.length);
                //    dashboard.showOverlay();
                //}
            },
            out: function () {
                //arrOver.pop();
                //dashboard.showOverlay();
            },
            drop: function (event, ui) {
                $('#design-overlay').remove();
                console.log('drop',$currentItem)
                if ( typeof $currentItem != 'undefined' && event.target.id == $currentItem.attr('id')) {
                    console.log('drop')
                    if (typeof (draggedCellId) != 'undefined') {
                        try {
                            // drop of moved existing dashboard element
                            target = dashboard.getCell($currentItem.attr('id'));
                            source = dashboard.getCell(draggedCellId);

                            // remove source from old parent
                            (dashboard.isRow(source.parent) ? source.parent.cells : source.parent.rows).splice(dashboard.getItemPos(source), 1);

                            // add source to new position
                            if (currentPos == 'right')
                                dashboard.insertAfter(source, (dashboard.isRow(target) && target.cells.length > 0 ? target.cells[target.cells.length - 1] : target));
                            else if (currentPos == 'left')
                                dashboard.insertBefore(source, (dashboard.isRow(target) && target.cells.length > 0 ? target.cells[0] : target));
                            else if (currentPos == 'bottom')
                                dashboard.insertBelow(source, target);
                            else if (currentPos == 'top')
                                dashboard.insertAbove(source, target);
                            // trigger changed
                            dashboard.changed();
                        } catch (e) {
                            console.log("error while drop: " + e.message);
                        }

                        delete draggedCellId;
                        dashboard.draw();
                    }
                    else {
                        // drop of new element from toolbar
                        var item = $(ui.draggable).data();
                        dropped = item;
                        item.style = "";
                        cell = dashboard.getCell($currentItem.attr('id'));
                        dashboard.addCell(cell, item, currentPos);
                        $("#" + selectedItem.attr('id')).trigger("click");
                        dropped = null;
                        //dashboard.changed()
                    }
                }
                else {
                    dashboard.draw();
                }
            }
        })
        .hover(
            function (e) {
              if (!isDrag && !isResize && !$('#' + $(this).attr('id') + '_selector').hasClass('selected')) {
              //if (!isDrag && !isResize) {
                    arrOver.push($(this));
                  $('.design-row-selector, .design-cell-selector').removeClass('layout-border');
                  //$('#'+$(this).attr('id')+'_selector').addClass('layout-border');
                  e.stopPropagation();
                  e.preventDefault();
                  //dashboard.selectItem($(this));
              }
          }, function () {
              if (!isDrag && !isResize) {
                  var el = arrOver.pop();
                  if (el !== undefined) {
                      el.removeClass('layout-border');
                      if (arrOver.length > 0)
                          arrOver[arrOver.length - 1].addClass('layout-border');
                  }
              }
              if (selectedItem == null) {
                  //$('#design-overlay').remove();
              }
          }
        )
        .mouseover(function (e) {
            if (typeof ($(this).attr('id')) != 'undefined') {
                $currentItem = $(this);
            }
            e.stopPropagation();
        })
        .mousemove(function (e) {
            //if (e.target == this) {
            //    $currentItem = $(this);
            //    console.log("mm: " + $currentItem.attr('id'));
            //}
        })
        .click(function (event) {
            event.stopPropagation();
            dashboard.selectItem($(this))
        })
        .dblclick(function () {
            //var c = dashboard.allCells[$(this).attr('id')];
            //if (c.type !== undefined && (c.type == 'part' || c.type == 'action')) {
            //    if (confirm('Die Änderungen wurden noch nicht gespeichert, trotzdem fortfahren?')) {
            //        preselectedNode = dashboard.allCells[$(this).attr('id')];
            //        $('#tree').jstree().refresh();
            //    }
            //}
        })

        $('.dashboard-canvas-selector').click(function (event) {
            event.stopPropagation();
            dashboard.selectItem($('#'+dashboardDivID+'-canvas'))
        })

        if (isLayoutView)
            $('.dashboard-canvas, .design-cell, .design-row').addClass('layout');

        // set styles for action from parent cell
        $('.dashboard-action').each(function (i, el) {
            var styles = $(el).parent().attr('style')?.split(';').filter(s => s.startsWith('background:') || s.startsWith('background-color:') || s.startsWith('color') || s.startsWith('text-decoration')).join(';')
            $(el).attr('style', $(el).attr('style') + ';' + styles)
        })

        dashboard.drawn();
    },
    insertAbove: function (item, target) {
        if (dashboard.isRow(target) || target.parent.cells.length == 1) {
            if (!dashboard.isRow(target) && target.parent.cells.length == 1)
                target = target.parent;

            // target is row
            if (!dashboard.isRow(source))
                item = { cells: [item], id: 'row-' + rowId++, style: undefined }
            target.parent.rows.splice(dashboard.getItemPos(target), 0, item);
            item.parent = target.parent;
        }
        else {
            // target is cell
            // 2 new rows for the cells
            var rows = [ (dashboard.isRow(item) ? item : { id: 'row-' + rowId++, cells: [item], style: undefined })
                , { id: 'row-' + rowId++, cells: [target], style: undefined }]

            // new cell as container for the new rows
            var container = { id: 'cell-' + cellId++, attr: [], culture: '', name: '', parent: target.parent, type: 'container', validation: '', rows: rows }
            rows[0].parent = container;
            rows[1].parent = container;
            var pos = dashboard.getItemPos(target);
            target.parent.cells.splice(pos, 1, container);

            if (!dashboard.isRow(item))
                item.parent = rows[0];
            target.parent = rows[1];
        }
    },
    insertBelow: function (item, target) {
        if (dashboard.isRow(target) || target.parent.cells.length == 1) {
            if (!dashboard.isRow(target) && target.parent.cells.length == 1)
                target = target.parent;

            // target is row
            if (!dashboard.isRow(source))
                item = { cells: [item], id: 'row-' + rowId++, style: undefined }
            target.parent.rows.splice(dashboard.getItemPos(target) + 1, 0, item);
            item.parent = target.parent;
        }
        else {
            // target is cell
            // 2 new rows for the cells
            var rows = [{ id: 'row-' + rowId++, cells: [target], style: undefined }
                , (dashboard.isRow(item) ? item : { id: 'row-' + rowId++, cells: [item], style: undefined })]

            // new cell as container for the new rows
            var container = { id: 'cell-' + cellId++, attr: [], culture: '', name: '', parent: target.parent, type: 'container', validation: '', rows: rows }
            rows[0].parent = container;
            rows[1].parent = container;
            var pos = dashboard.getItemPos(target);
            target.parent.cells.splice(pos, 1, container);

            if(!dashboard.isRow(item))
                item.parent = rows[1];
            target.parent = rows[0];
        }
    },
    insertBefore: function (item, target) {
        if (dashboard.isRow(item)) {
            $.each(item.cells, function (i, cell) {
                target.parent.cells.splice(dashboard.getItemPos(target) + i, 0, cell);
                cell.parent = target.parent;
            })
        }
        else {
            if (dashboard.isRow(target)) {
                target.cells[0] = item;
                item.parent = target;
            }
            else {
                target.parent.cells.splice(dashboard.getItemPos(target), 0, item);
                item.parent = target.parent;
            }
        }
    },
    insertAfter: function (item, target) {
        if (dashboard.isRow(item)) {
            $.each(item.cells, function (i, cell) {
                target.parent.cells.splice(dashboard.getItemPos(target) + 1 + i, 0, cell);
                cell.parent = target.parent;
            })
        }
        else {
            if (dashboard.isRow(target)) {
                target.cells[0] = item;
                item.parent = target;
            }
            else {
                target.parent.cells.splice(dashboard.getItemPos(target) + 1, 0, item);
                item.parent = target.parent;
            }
        }
    },
    getItemPos: function (item) {
        return dashboard.getCellPos((dashboard.isRow(item) ? item.parent.rows : item.parent.cells), item.id);
    },
    isRow: function (item) {
        return (item.cells !== undefined);
    },
    drawRows: function (parent, rows) {
        $(rows).each(function (index, row) {
            var id = 'row-' + rowId++;
            row.id = id;
            var rowDiv = $('<div id="' + id + '" ' + (row.style !== undefined ? ' style="' + row.style + '" ' : '') + ' class="design-row"/>').appendTo(parent);
            if (dashboard.rows.length == 1 && dashboard.rows[0].cells.length == 0) {
                // empty dashboard, show info text
                rowDiv.css('min-width', '400px');
                rowDiv.css('min-height', '300px');
                rowDiv.html('<div class="empty-canvas"><span >' + ResX.PlaceItemsDragNDrop + '</span>')
            }
            //var rowDivSelector = $('<div id="' + id + '_selector" data-parentid="' + id + '" class="design-cell-selector" />').appendTo(rowDiv);

            dashboard.allCells[id] = row;
            if (typeof (dashboard.getCell(parent.attr('id'))) == 'undefined')
                dashboard.getCell(id).parent = dashboard;
            else
                dashboard.getCell(id).parent = dashboard.getCell(parent.attr('id'));
            dashboard.drawRow(rowDiv, row);
        })
    },
    drawRow: function (parent, row) {
        $(row.cells).each(function (index, cell) {
            dashboard.drawCell(parent, cell);
        })
    },
    drawCell: function (parent, cell) {
        //console.log('cell', cell)
        var id = 'cell-' + cellId++;
        if (cell.id === undefined && dropped !== undefined && dropped != null)
            dropped.id = id;
        cell.id = id;
        var attr = '';
        if (cell.style !== undefined)
            attr += ' style="' + cell.style + '" ';
        if (cell.attrs !== undefined) {
            if (!Array.isArray(cell.attrs))
                attr += ' data-attrs="' + String(JSON.stringify(cell.attrs, dashboard.stringifyAttr)).replace(/&/g, '&amp;').replace(/"/g, '&quot;') + '" ';
        }
        if (cell.styles !== undefined) {
            if (!Array.isArray(cell.styles))
                attr += ' data-styles="' + String(JSON.stringify(cell.styles, dashboard.stringifyAttr)).replace(/&/g, '&amp;').replace(/"/g, '&quot;') + '" ';
        }


        if (cell.validation === undefined)
            cell.validation = '';
        var validation = ' data-validation="' + cell.validation.replace(/&/g, '&amp;').replace(/"/g, '&quot;') + '" ';

        var autostart = '';
        if (cell.autostart === undefined)
            cell.autostart = '';
        if (cell.autostart != '')
            autostart = ' data-autostart="' + cell.autostart + '" ';
        //isValid = false;

        //if (cell.validation == '' || dashboard.render(cell.validation.replace(/\\{/g, '{').replace(/\\}/g, '}')) == 'valid') {
        isValid = true;
        //}

        var cellDiv = $('<div id="' + id + '" class="design-cell" ' + validation + '  ' + autostart + ' ' + attr + '/>').appendTo(parent);
        if (cell.type !== undefined) {
            if (this.isSave) {
                if (cell.type == 'part' || cell.type == 'action' || cell.type == 'htmlcomponent' || cell.type == 'field')
                    cellDiv.html('%[%' + (cell.type == 'part' ? 'Parts' : (cell.type == 'action' ? 'Actions' : (cell.type == 'htmlcomponent' ? 'HtmlComponents' : 'Contact'))) + '.' + cell.name + '%]%');
            }
            else {
                // cache rendered template
                if (typeof (itemCache[cell.name + '_' + cell.culture]) == 'undefined') {
                    cell.design = true;
                    jQuery.session_ajax({
                        url: serverRoot + 'Admin/GetHtml',
                        data: { type: cell.type, name: cell.name, culture: cell.culture, design: cell.design },
                        success: function (html) {
                            itemCache[cell.name + '_' + cell.culture] = html;
                        },
                    })
                }

                // copy style from first child of cell to cell itself
                cellDiv.html(itemCache[cell.name + '_' + cell.culture]);
                if (dropped !== undefined && dropped != null && cell.type == 'htmlcomponent' && cell.id == dropped.id) {
                    if (cellDiv.children().length > 0) {
                        cellDiv.attr('style', $(cellDiv.children()[0]).attr('style'));
                        cell.style = $(cellDiv.children()[0]).attr('style');
                        if (cell.name == 'SEPARATOR')
                            $(cellDiv.children()[0]).attr('style','')
                    }
                }

                // assign attrs to cell (for htmlcomponents inside a part)
                $.each(cellDiv.children('.xp-row').children('[data-attrs]'), function (i, el) {
                    if (typeof $(el).data('attrs') == 'object') {
                        var attrs = $(el).data('attrs');
                        $.each($(el).find('[data-loader]'), function (i, el1) {
                            var loader = eval('(' + $(el1).data('loader') + ')');
                            if (attrs[loader.id])
                                $.each(attrs[loader.id].attrs, function (j, attr) {
                                    // assign attribute
                                    if (attr.attr == 'text')
                                        $(el1).text(dashboard.render(isValid ? attr.value : ''));
                                    else if (attr.attr == 'html')
                                        $(el1).html(dashboard.render(isValid ? attr.value : '').replace(/\\n/gm, '\n').replace(/scr__ipt>/gm, 'scr' + 'ipt>'));
                                    else
                                        $(el1).attr(attr.attr, dashboard.render(attr.value));
                                })
                        })
                        $(el).find('[data-loader]').removeAttr('data-loader');
                    }
                })

                // create real ids for html components and assign all attrs to parent cell
                if (cell.attrs === undefined)
                    cell.attrs = {}
                $.each(cellDiv.find('[data-loader]'), function (i, el) {
                    var loader = eval('(' + $(el).data('loader') + ')');
                    $(el).attr('id', id + '_' + loader.id);

                    if (cell.attrs[loader.id] === undefined) {
                        // assign attrs to parent cell - new component dropped
                        cell.attrs[loader.id] = { id: loader.id, attrs: loader.attrs };
                    }

                    if (cell.attrs[loader.id] !== undefined) {
                        $.each(cell.attrs[loader.id].attrs, function (j, attr) {

                            // replace attribute value with text and index, something like 'Text 1'
                            if (attr.value == '[?]') {
                                var n = 1;
                                var found = true
                                while (found) {
                                    found = false;
                                    var cells = dashboard.getCells();
                                    $.each(cells, function (k, c) {
                                        var nn = 0;
                                        if (c.type !== undefined && c.type == cell.type && c.name == cell.name
                                            && c.attrs[attr.attr].attrs[0].value == attr.name + ' ' + n) {
                                            found = true;
                                            n++;
                                            return false;
                                        }
                                    })
                                }
                                attr.value = attr.name + ' ' + n;
                            }

                            // assign attribute
                            if (attr.attr == 'text')
                                $(el).text(dashboard.render(isValid ? attr.value : ''));
                            else if (attr.attr == 'html') {
                                var v = dashboard.render(isValid ? attr.value : '').replace(/\\n/gm, '\n').replace(/scr__ipt>/gm, 'scr' + 'ipt>');
                                if (attr.id == 'html' && v == '')
                                    v = "[" + attr.name + "]";
                                try {
                                    $(el).html(v);
                                } catch (e) {
                                    console.error(e)
                                }
                            }
                            else
                                $(el).attr(attr.attr, dashboard.render(attr.value));
                        })
                    }

                })
                if (cell.name == 'SEPARATOR')
                    $('.HtmlComponentSeparator').attr('style','')
            }
        }
        //var cellDivSelector = $('<div id="' + id + '_selector" data-parentid="' + id + '" class="design-cell-selector" />').appendTo(cellDiv);
        if (cell.selected !== undefined) {
            selectedItem = $('#' + cell.id);
            delete cell.selected;
        }

        dashboard.allCells[id] = cell;
        dashboard.allCells[id].parent = dashboard.allCells[parent.attr('id')];
        if (cell.rows !== undefined) {
            dashboard.drawRows(cellDiv, cell.rows);
        }
    },
    getCells: function (rows) {
        var cells = [];
        if (typeof rows == 'undefined')
            rows = dashboard.rows;
        for (var i = 0; i < rows.length; i++) {
            if (rows[i].cells !== undefined) {
                cells = cells.concat(rows[i].cells);
                for (var j = 0; j < rows[i].cells.length; j++) {
                    if (rows[i].cells[j].rows !== undefined) {
                        cells = cells.concat(dashboard.getCells(rows[i].cells[j].rows));
                    }
                }
            }
        }
        return cells;
    },
    showOverlay: function (item) {
        $('.dashboard-canvas-selector, .design-cell-selector, .design-row-selector').removeClass('selected');
        //$('#' + $(item).attr('id') + '_selector').addClass('selected');
        if (item !== undefined || arrOver.length > 0) {
            if (item === undefined)
                item = arrOver[arrOver.length - 1]
            if($(item).hasClass('dashboard-canvas'))
                $('.dashboard-canvas-selector').addClass('selected');
            else 
                $('#' + $(item).attr('id') + '_selector').addClass('selected');
        }
    },
    addCell: function (cell, dropped, pos) {
        var c = { type: dropped.type, name: dropped.name, culture: dropped.culture, style: dropped.style, attrs: dropped.attrs, validation: '', selected: true }
        try {
            switch (pos) {
                case '':
                    if (typeof (cell.cells) != 'undefined')
                        cell.cells.push(c);
                    else
                        cell = c;
                    break;
                case 'top':
                case 'bottom':
                    if (typeof (cell.cells) != 'undefined') {
                        // dropped on row
                        var i = dashboard.getCellPos(cell.parent.rows, cell.id);
                        cell.parent.rows.splice((pos == 'top' ? i : i + 1), 0, { cells: [c] });
                    }
                    else {
                        //dropped on cell
                        var i = dashboard.getCellPos(cell.parent.cells, cell.id);
                        if (pos == 'top')
                            cell.parent.cells[i] = { rows: [{ cells: [c] }, { cells: [cell] }] }
                        else
                            cell.parent.cells[i] = { rows: [{ cells: [cell] }, { cells: [c] }] }
                    }
                    break;
                case 'left':
                case 'right':
                    if (typeof (cell.cells) != 'undefined') {
                        // dropped on row
                        if (pos == 'left')
                            cell.cells.unshift(c);
                        else
                            cell.cells.push(c);
                    }
                    else {
                        //dropped on cell
                        var i = dashboard.getCellPos(cell.parent.cells, cell.id);
                        cell.parent.cells.splice((pos == 'left' ? i : i + 1), 0, c);
                    }
                    break;
                default:

            }

        } catch (e) {
            console.error("Error in addCell");
        }
        dashboard.draw();
    },
    getCellPos: function (arr, id) {
        for (var i = 0; i < arr.length; i++) {
            if (arr[i].id == id) {
                return i;
            }
        }
    },
    load: function (html) {
        rowId = 1;
        cellId = 1;
        $(dashboard.saveid).html(html);
        dashboard.rows = [];
        var rowsHtml = $(dashboard.saveid + ' .dashboard-canvas').children('.xp-row')
        if (rowsHtml.length == 0) // old templates does not have dashboard-canvas element
            rowsHtml = $(dashboard.saveid).children('.xp-row')
        this.canvas.style = $('.dashboard-canvas').attr('style') || '';
        rowsHtml.each(function (i, rowHtml) {
            dashboard.loadRow(dashboard.rows, $(rowHtml));
        })
        selectedItem = null;
    },
    isDirty: function () {
        if (dashboard.loadedRows === undefined)
            return false;
        var dirty = !_.isEqual(purifyRows(dashboard.loadedRows), purifyRows(dashboard.rows));
        console.log("dashboard.isDirty: " + dirty);
        return dirty;
    },
    loadRow: function (rows, rowHtml) {
        var row = { id: rowId++, cells: [], style: rowHtml.attr('style') };
        rows.push(row);
        var cellsHtml = rowHtml.children('.xp-cell');
        cellsHtml.each(function (i, cellHtml) {
            dashboard.loadCell(row.cells, $(cellHtml))
        })
    },
    loadCell: function (cells, cellHtml) {
        p = $(cellHtml).children('.xp-row').length > 0 ? ['container', ''] : cellHtml.text().replace("{", "").replace("}", "").split('.');
        var attrs = eval('(' + (typeof (cellHtml.data('attrs')) == 'undefined' ? '[]' : cellHtml.data('attrs').replace(/\\{/g, '{').replace(/\\}/g, '}')) + ')');
        var cell = {
            id: cellId++, type: (p[0] == 'Parts' ? 'part' : (p[0] == 'Views' ? 'view' : (p[0] == 'Contact' ? 'field' : (p[0] == 'Actions' ? 'action' : (p[0] == 'HtmlComponents' ? 'htmlcomponent' : p[0]))))), name: p[1], culture: ''
            , style: cellHtml.attr('style'), autostart: (cellHtml.data('autostart') !== undefined ? cellHtml.data('autostart') : ''), validation: (cellHtml.data('validation') !== undefined ? cellHtml.data('validation').replace(/\\{/g, '{').replace(/\\}/g, '}') : ''), attrs: attrs
        }
        cells.push(cell);
        $(cellHtml).children('.xp-row').each(function (i, rowHtml) {
            if (cell.rows === undefined)
                cell.rows = [];
            dashboard.loadRow(cell.rows, $(rowHtml));
        })
    },
    deleteItem: function (item, redraw = true) {
        var deleted = false
        if (item.id == 'dashboard-canvas') {
            dashboard.rows = []
            deleted = true
        }
        else if (item.parent !== undefined) {
            var items = (item.parent.rows !== undefined ? item.parent.rows : item.parent.cells);
            var i = dashboard.getCellPos(items, item.id);
            items.splice(i, 1);
            deleted = true
        }

        if (deleted) {
            $('#design-overlay').remove();
            selectedItem = null;
            if (redraw) {
                dashboard.draw();
                dashboard.changed()
            }
        }
    },
    selectItem: function (item) {
        $('#db-properties').show()
        $('#db_properties_no_selection').hide()
        $('#part_referenced_container').hide()
        $('.design-cell, .design-row').resizable();
        $('.design-cell, .design-row').resizable('destroy');
        $('.layout-border').removeClass('layout-border');
        dashboard.showOverlay(item);
        selectedItem = dashboard.allCells[item.attr('id')];
        selectedItem = selectedItem || { type: 'canvas', style: item.attr('style'), id: 'dashboard-canvas' }
        //$('#btn_item_delete').show();
        $('#btn_item_delete').offset({ top: $(item).offset().top - 8, left: $(item).offset().left + $(item).outerWidth() - 12 })
        if (selectedItem.type == 'canvas')
            $('#btn_item_delete').hide();
        $(item).resizable({
            handles: "e, s",
            grid: 5,
            alsoResize: "#design-overlay",
            start: function () {
                isResize = true;
                $('.design-row, .design-cell').removeClass('layout-border');
                $('#btn_item_delete').hide();
            },
            stop: function (e, ui) {
                isResize = false;
                var cell = dashboard.allCells[$(ui.element).attr('id')] || dashboard.canvas;
                if ($(ui.element).attr('style') != '')
                    cell.style = $(ui.element).attr('style');
                dashboard.selectItem(item);
            }
        });
        dashboard.updateCSSForm(selectedItem);
    },
    updateCSSForm: function (item) {
        console.log(item)
        $('#item-properties').html('<div class=no-properties>' + ResX.NoPropertiesAvailable + '</div>');
        if (item.type == 'canvas') {
            $('#properties-title').text($('#txt_id').val());
        }
        else if (typeof (item.type) == 'undefined') {
            //row 
            $('#properties-title').text( ResX.Row);
        }
        else {
            // cell
            typenames = { view: ResX.View, part: ResX.Widget, action: ResX.Action, htmlcomponent: ResX.HTMLComponent }
            if (item.type == 'action' || item.type == 'part')
                $('#properties-title').html(`${typenames[item.type]} <a href="#" onclick="preselectedNode = selectedItem; $('#tree').jstree().refresh();">${getItemName(item.type, item.name)}</a>`)
            else
                $('#properties-title').text(typenames[item.type] + ' ' + getItemName(item.type, item.name))

            $('#properties-title').attr('title',typenames[item.type] + ' ' + getItemName(item.type, item.name))
        }

        //var c = dashboard.allCells[$(this).attr('id')];
        //if (c.type !== undefined && (c.type == 'part' || c.type == 'action')) {
        //    if (confirm('Die Änderungen wurden noch nicht gespeichert, trotzdem fortfahren?')) {
        //        preselectedNode = dashboard.allCells[$(this).attr('id')];
        //        $('#tree').jstree().refresh();
        //    }
        //}


        // set default attributes for action (auto execution)
        //if (item.type == 'action' && typeof (item.attrs) == 'undefined') {
        //    item.attrs = [{ attr: 'autostart', value: '', name: 'Autostart', ctrltype: 'autostart' }]
        //}

        var fieldname = ''
        var hide_chk_empty = true
        var field_select_id = ''
        var txt_id = 'txt_loader_' + item.id + '_validation';
        var ctrl_id = 'ctrl_loader_' + item.id + '_validation';
        var ddl_id = 'ddl_loader_' + item.id + '_validation';
        var empty_id = 'chk_empty_' + item.id;


        // handle action and html components properties
        if (item && (item.type == 'htmlcomponent' || item.type == 'action') && typeof (item.attrs) != 'undefined') {
            $('#item-properties').html('');
            // attributes
            $.each(item.attrs, function (i, a) {
                $.each(a.attrs, function (j, attr) {
                    var id = 'txt_loader_' + item.id + '_' + a.id + '_' + attr.attr;

                    var labeltext = (attr.ctrltype == 'callfield' ? 'Data Field: Call' : ((attr.ctrltype == 'contactfield' ? 'Data Field: Contact' : (attr.ctrltype == 'userfield' ? 'Data Field: Current User' : attr.name))));
                    var label = $('<label class="c4b-input" for="' + id + '"></label>').appendTo('#item-properties');
                    fieldname = attr.value.replace('{','').replace('}','')
                    $('<input type="text" name="loader_' + attr.attr + '" id="' + id + '" value="' + quoteattr(attr.value) + '" placeholder=" " class="loader-input"><span>'+labeltext+'</span>').appendTo(label);

                    $('#' + id).data('id', a.id);
                    $.each(attr, function (key, value) {
                        $('#' + id).data(key, value);
                    })
                    if (attr.ctrltype !== undefined) {
                        if (attr.ctrltype == 'tpleditor') {
                            $('#' + id).tpleditor({ contactFields: contactFields, callFields: callFields, userFields: userFields, snippets: snippets, syntax: (attr.syntax !== undefined ? attr.syntax : null) });
                            if (attr.syntax !== undefined || item.name == 'IFRAME') {
                                // disable input for ace editor inputs  (multiline problem)
                                $('#' + id).attr('readonly', 'readonly');
                                $('#' + id).data('value', attr.value.replace(/\\n/gm, '\n'));
                                $('#' + id).val();
                            }
                        }
                        if (attr.ctrltype == 'contactfield' || attr.ctrltype == 'callfield' || attr.ctrltype == 'userfield') {
                            label.removeClass('c4b-input').addClass('c4b-select')
                            label.find('span').remove()
                            label.prepend( labeltext);
                            hide_chk_empty = false
                            type = (attr.ctrltype == 'contactfield' ? { fields: contactFields, entity: 'Contact' } : (attr.ctrltype == 'userfield' ? { fields: userFields, entity: 'User' } : { fields: callFields, entity: 'Call' }))

                            data = "";
                            $.each($('#' + id).data(), function (key, val) {
                                data += ' data-' + key + '="' + val + '" ';
                            })
                            field_select_id = id
                            var sel = $('<select ' + data + ' class="loader-input" id="' + id + '">');
                            $(type.fields).each(function (i, val) {
                                if ($('#' + id).data('value') == '{' + type.entity + '.' + val + '}') {
                                    sel.append($("<option>").attr('value', '{' + type.entity + '.' + val + '}').text(val).attr('selected', 'selected'));
                                }
                                else
                                    sel.append($("<option>").attr('value', '{' + type.entity + '.' + val + '}').text(val));
                            });
                            $('#' + id).replaceWith(sel[0].outerHTML);
                        }
                        $('#' + id).change(function () {
                            var oldval = $(this).data('value')
                            var newval = $(this).val()
                            $(this).data('value', newval)
                            if ($('#' + empty_id).is(':checked')) {
                                var r = rules2StringTemplate($('#ctrl_loader_' + item.id + '_validation').queryBuilder('getRules')).replace(oldval.slice(1, -1), newval.slice(1, -1))
                                setRules(txt_id, ctrl_id, ddl_id, r);
                            }
                            dashboard.setLoaderValues();
                        })
                    }
                })
            })

            // validation string
            if (typeof (item.validation) == 'undefined')
                item.validation = '';
            var label = $('<label  class="c4b-select" for="' + txt_id + '">' + ResX.AdditionalRules+'</label>').appendTo('#item-properties');
            $('<input type="text" name="loader_validation" id="'+txt_id+'" value="'+quoteattr(item.validation.replace(/\\{/g, '{').replace(/\\}/g, '}'))+'" class="validation-input">'
                +'<select id="'+ddl_id+'">'
                    +'<option value="0">'+ResX.None+'</option >'
                +'<option value="1">'+ResX.Custom+'</option>'
                + '</select>').appendTo(label)
            $('<div id="' + ctrl_id + '"></div>').appendTo('#item-properties');

            txt_id = '#' + txt_id;
            ddl_id = '#' + ddl_id;
            ctrl_id = '#' + ctrl_id;

            $(ctrl_id).queryBuilder({
                filters: getValidationFields(),
                operators: ['is_empty', 'is_not_empty']
            });

            setRules(txt_id, ctrl_id, ddl_id, item.validation);
            var rules = $(ctrl_id).queryBuilder('getRules')

            // build stringtemplate from querybuilder rule
            $(ctrl_id).on('change.queryBuilder', function (e) {
                console.log(1)
                $('#' + empty_id).prop('checked', existsEmptyCondition($(ctrl_id).queryBuilder('getRules'), fieldname))

                $(txt_id).val(rules2StringTemplate($(ctrl_id).queryBuilder('getRules')));
                dashboard.setLoaderValues();
            });
            $(ctrl_id).on('click.queryBuilder', function (e) {
                console.log(2)
                $('#' + empty_id).prop('checked', existsEmptyCondition($(ctrl_id).queryBuilder('getRules'), fieldname))

                $(txt_id).val(rules2StringTemplate($(ctrl_id).queryBuilder('getRules')));
                dashboard.setLoaderValues();
            });

            $(ddl_id).change(function () {
                if ($(this).val() == "1")
                    $(ctrl_id).show();
                else {
                    $(ctrl_id).hide();
                    $(txt_id).val("");
                    setRules(txt_id, ctrl_id, ddl_id, "");
                }
                //$('#' + empty_id).prop('checked', existsEmptyCondition($(ctrl_id).queryBuilder('getRules'), fieldname))
            })

            $(txt_id).change(function () {
                setRules(txt_id, ctrl_id, ddl_id, $(txt_id).val());
                //$('#' + empty_id).prop('checked', existsEmptyCondition($(ctrl_id).queryBuilder('getRules'), fieldname))
            });


            // action autostart
            if (item.type == 'action') {
                var id = 'ddl_loader_' + item.id + '_autostart';
                if (typeof (item.autostart) == 'undefined')
                    item.autostart = '';
                var label = $('<label class="c4b-select" for="' + id + '">' + ResX.ActionAutostart + '</label>').appendTo('#item-properties');
                var sel = $('<select name="loader_autostart" id="' + id + '" value="' + item.autostart + '" class="autostart-input">').appendTo(label);
                $('<option value="">'+ResX.ActionAutostartNone+'</option>').appendTo(sel);
                $('<option value="ActionAutostart">' + ResX.ActionAutostart + '</option>').appendTo(sel);
                $('<option value="ActionAutostartHide">' + ResX.ActionAutostartHide + '</option>').appendTo(sel);
                $('<option value="ActionAutostartUnique">' + ResX.ActionAutostartUnique + '</option>').appendTo(sel);
                $('<option value="ActionAutostartUniqueHide">' + ResX.ActionAutostartUniqueHide + '</option>').appendTo(sel);
                $('#'+id+' option[value="'+item.autostart+'"]').prop('selected',true);
            }

            // add checkbox (hide if field is empty)
            var checked = existsEmptyCondition(rules, fieldname) ? 'checked' : ''
            if (!hide_chk_empty) {
                $('<input type="checkbox" class="c4b-checkbox" ' + checked + ' id="' + empty_id + '"><label for="' + empty_id + '">' + ResX.HideWhenFieldEmpty + '</label>').appendTo('#item-properties');
                $('#' + empty_id).change(function (e) {
                    fieldname = $('#' + field_select_id).val().slice(1, -1)

                    var rules = $(ctrl_id).queryBuilder('getRules')
                    if ($(e.target).is(':checked')) {
                        // add not empty rule
                        if (rules == null)
                            $(ddl_id).val("1").trigger('change')

                        var rule = { field: fieldname, id: fieldname, input: "text", operator: "is_not_empty", type: "string", value: null }
                        if (rules == null || rules.condition == 'OR')
                            rules = { condition: 'AND', rules: rules || [], valid: true }
                        rules.rules = rules.rules || []
                        if (rules.condition == 'AND')
                            rules.rules.push(rule)
                        $(ctrl_id).queryBuilder('setRules', rules)
                    }
                    else {
                        // remove not empty rule
                        if (rules && rules.rules) {
                            rules.rules = rules.rules.filter(function (r) { return !(r.field == fieldname && r.operator == 'is_not_empty') })
                            if (rules.rules.length > 0)
                                $(ctrl_id).queryBuilder('setRules', rules)
                        }

                        if (!rules || rules.rules.length == 0) {
                            $(ddl_id).val("0").trigger('change');
                            setRules(txt_id, ctrl_id, ddl_id, '');
                        }
                    }
                    $(txt_id).val(rules2StringTemplate($(ctrl_id).queryBuilder('getRules')));
                    dashboard.setLoaderValues();
                })
            }
        }

        $('#item-properties input.loader-input,#item-properties input.validation-input, #item-properties select.autostart-input').keyup(function (e) {
            if (e.key !== 'Right' && e.key !== 'Left')
                dashboard.setLoaderValues();
            e.stopPropagation();
            e.preventDefault();
        }).change(function () {
            dashboard.setLoaderValues();
        })


        // handle css properties
        $('#item-css').html('')

        // format templates 
        jQuery.ajax({
            url: serverRoot + 'Admin/GetFormatTemplates',
            cache: false,
            async: false,
            success: function (templates) {
                var id = 'ddl_format_template_' + item.id;
                var label = $('<label style="grid-column: 1/3;width:100%;" class="c4b-select" for="' + id + '">' + ResX.FormatTemplate + '</label>').appendTo('#item-css');
                var sel = $('<select id="' + id + '" required>').appendTo(label);
                $('<option disabled selected hidden>' + ResX.Custom + '</option>').appendTo(sel);
                var s1 = (item.style || '').split(';')
                $.each(templates, function (i, t) {
                    var s2 = t.style.split(';')
                    $('<option value="' + t.style + '" style="' + t.style + '" ' + (_.intersection(s1, s2).length == s2.length ? 'selected' : '') + '>' + t.name + '</option>').appendTo(sel);
                })
                $('#' + id).change(function (e) {
                    item.style = this.value;
                    dashboard.updateCSSForm(item);
                    dashboard.setStyles(item);
                })
            },
            error: function (e) { console.error(e) }
        })


        // styles        
        if (typeof (item.style) == 'undefined')
            item.style = '';
        var styles = item.style.split(';')
        $.each(styles, function (i, elem) {
            if (elem.trim() != '' && elem.split(':').length > 1 ) {
                var name = elem.split(':')[0].trim();
                var value = elem.split(':')[1].trim();
                if (value.trim() != '') {
                    var id = 'txt_style_' + name;
                    $('<label for="' + id + '">' + name + ':</label>').appendTo('#item-css');
                    $('<input type="text" data-name="' + name + '" name="style_' + name + '" id="' + id + '" value="' + value + '" class="css-input">').appendTo('#item-css');
                }
            }
        })
        $('<label /><input type="text" placeholder="' + ResX.AdditionalStyles + '" name="style_css_additional" id="txt_css_additional">').appendTo('#item-css')
        $('#txt_css_additional').autocomplete({
            source: [
              "margin:",
              "margin-bottom:",
              "margin-left:",
              "margin-right:",
              "margin-top:",
              "padding:",
              "padding-bottom:",
              "padding-left:",
              "padding-right:",
              "padding-top:",
              "color:",
              "background-color:",
              "font-size:",
              "font-weight:",
              "font-family:",
              "border:",
              "width:",
              "height:",
            ],
            select: function (e) {
                e.stopPropagation();
                //e.preventDefault();
            },
            position: { my: "left bottom", at: "left top", collision: "flip" },
            minLength: 0
        }).focus(function () {
            $(this).autocomplete("search", "");
        });

        $('#item-css input').keyup(function (e) {
            if (e.which !== 0)
                dashboard.setStyles();
            e.stopPropagation();
            e.preventDefault();
        })
        .blur(function () {
            // it's a length value, add a 'px' to it, if no unit is given
            if ($(this).data('name') !== undefined && $(this).data('name').search(/(margin|padding|width|height)/i) != -1 && !isNaN(parseFloat($(this).val())) && isFinite($(this).val())) {
                $(this).val($(this).val() + 'px');
                dashboard.setStyles();
            }
            // it's a font-size, add a 'pt' to it, if no unit is given
            if ($(this).data('name') !== undefined && $(this).data('name').search(/font-size/i) != -1 && !isNaN(parseFloat($(this).val())) && isFinite($(this).val())) {
                $(this).val($(this).val() + 'pt');
                dashboard.setStyles();
            }
                // if it's a color value prepend a '#' if missing
            else if ($(this).data('name').search(/color/i) != -1 && $(this).val().search(/^[0-9a-f]{3,6}$/i) != -1) {
                $(this).val('#' + $(this).val());
                dashboard.setStyles();
            }
        })
        // prevent deselect, when using the form
        $('#item-css input').click(function (e) {
            e.stopPropagation();
            e.preventDefault();
        })

        // make 

        dashboard.setLoaderValues();
        //dashboard.setStyles();

        //$('.validation-input').tpleditor({ contactFields: contactFields, callFields: callFields, userFields: userFields });

        $('#item-css input').droppable({
            drop: function (e, ui) {
                $(this).insertAtCaret('{Contact.' + ui.draggable.data('name') + '}');
            }
        })

    },
    setStyles: function () {
        selectedItem.style = "";
        var updateForm = false;
        $('#item-css input').each(function (i, elem) {
            elem = $(elem);
            if (elem.attr('id') == 'txt_css_additional') {
                $.each(elem.val().split(';'), function (i, st) {
                    if (st.trim() != '' && st.split(':').length == 2) {
                        selectedItem.style += st.split(':')[0].trim() + ':' + st.split(':')[1].trim() + ';';
                        if (st.split(':')[1].trim().length > 0)
                            updateForm = true;
                    }
                })
            }
            else
                selectedItem.style += elem.data('name') + ':' + elem.val() + ';';
        })
        var oldstyle = $('#' + selectedItem.id).attr('style');
        $('#' + selectedItem.id).attr('style', selectedItem.style);
        var newstyle = $('#' + selectedItem.id).attr('style');
        if (updateForm) {
            dashboard.updateCSSForm(selectedItem);
            var inp = $($('#item-css input')[$('#item-css input').length - 2])
            inp.focus();
            inp[0].selectionStart = inp[0].selectionEnd = inp.val().length;
        }
        $('#design-overlay').width($('#' + selectedItem.id).width())
        $('#design-overlay').height($('#' + selectedItem.id).height());
        var item = $('#' + selectedItem.id);
        $('#btn_item_delete').offset({ top: $(item).offset().top - 8, left: $(item).offset().left + $(item).outerWidth() - 12 })

        // set styles for action from parent cell
        $('.dashboard-action').each(function (i, el) {
            $(el).attr('style', $(el).attr('style') + ';' + $(el).parent().attr('style'))
        })

        dashboard.changed();
    },
    setLoaderValues: function () {
        //console.log('setloadervalues')
        dashboard.changed()
        selectedItem.attrs = {};
        $('#item-properties input.validation-input').each(function (i, elem) {
            $('#' + selectedItem.id).data('validation', $(elem).val());
            dashboard.allCells[selectedItem.id].validation = $(elem).val();
        })

        $('#item-properties .autostart-input').each(function (i, elem) {
            $('#' + selectedItem.id).data('autostart', $(elem).val());
            dashboard.allCells[selectedItem.id].autostart = $(elem).val();
        })
        // uncoment for designer validation
        //var isValid = false;
        //if (cells[selectedItem.id].validation == '' || dashboard.render(cells[selectedItem.id].validation.replace(/\\{/g, '{').replace(/\\}/g, '}')) == 'valid') {
        isValid = true;
        //}
        $('#item-properties .loader-input').each(function (i, elem) {
            elem = $(elem);
            elem.data('value', elem.val());
            if (selectedItem.attrs[elem.data('id')] === undefined)
                selectedItem.attrs[elem.data('id')] = {};
            selectedItem.attrs[elem.data('id')].id = elem.data('id');
            if (selectedItem.attrs[elem.data('id')].attrs === undefined)
                selectedItem.attrs[elem.data('id')].attrs = [];
            selectedItem.attrs[elem.data('id')].attrs.push(elem.data())

            if (elem.data('attr') == 'text')
                $('#' + selectedItem.id + '_' + elem.data('id')).text(dashboard.render(isValid ? elem.val() : ''));
            else if (elem.data('attr') == 'html') {
                var v = dashboard.render((isValid ? elem.val() : '').replace(/\\n/gm, '\n').replace(/scr__ipt>/gm, 'scr' + 'ipt>'));
                var oldhtml = $('#' + selectedItem.id + '_' + elem.data('id')).html();
                try {
                    $('#' + selectedItem.id + '_' + elem.data('id')).html(v);
                }
                catch (e) {
                    try {
                        $('#' + selectedItem.id + '_' + elem.data('id')).html(oldhtml);
                    } catch (e) {

                    }
                    var tpl_button = $(this).parent().find('button');

                    if (scriptErrorTimeout)
                        window.clearTimeout(scriptErrorTimeout)

                    scriptErrorTimeout = window.setTimeout(function () {
                        tpl_button.trigger('click');
                        $.notify("[Error in script] " + e.message, { globalPosition: "top center", className: 'error', clickToHide: true, autoHide: false });
                        //alert('Error in script: ' + e.message);
                    }, 500)
                }
            }
            else
                $('#' + selectedItem.id + '_' + elem.data('id')).attr(elem.data('attr'), dashboard.render(elem.val()));
        })

        $('#' + selectedItem.id).data('attrs', JSON.stringify(sortObjByKey(selectedItem.attrs), dashboard.stringifyAttr));
        $('#design-overlay').width($('#' + selectedItem.id).width())
        $('#design-overlay').height($('#' + selectedItem.id).height());

        var item = $('#' + selectedItem.id);
        $('#btn_item_delete').offset({ top: $(item).offset().top - 8, left: $(item).offset().left + $(item).outerWidth() - 12 })

    },
    cleanup: function (rows) {
        // find empty rows, empty cells &  unnecessary cells
        try {
            if (typeof (rows) != 'undefined') {
                var parent = rows;
                $.each(rows, function (i, row) {
                    if (typeof (row) != 'undefined') {
                        if (row.cells !== undefined)
                            if (row.cells.length == 0 && row.parent.id != '#dashboard')
                                dashboard.deleteItem(row, false);
                                // unnecessary container cell?
                            else if (row.cells.length == 1 && row.cells[0].rows !== undefined && row.cells[0].rows.length == 1) {
                                row.cells[0].rows[0].parent = row.parent;
                                parent.splice(i, 1, row.cells[0].rows[0]);
                            }
                            else {
                                parent = row;
                                $.each(row.cells, function (i, cell) {
                                    if (cell.type == 'container') {
                                        if (cell.rows !== undefined) {
                                            // empty container cell?
                                            if (cell.rows.length == 0)
                                                dashboard.deleteItem(cell, false);
                                                // unnecessary container cell?
                                            else if (cell.rows.length == 1 && cell.rows[0].cells !== undefined && cell.rows[0].cells.length > 0) {
                                                parent.cells.splice(i, 1);
                                                for (j = 0; j < cell.rows[0].cells.length; j++) {
                                                    parent.cells.splice(i + j, 0, cell.rows[0].cells[j]);
                                                    cell.rows[0].cells[j].parent = parent;
                                                }
                                            }
                                        }
                                    }
                                    if (cell.rows !== undefined)
                                        dashboard.cleanup(cell.rows);

                                })
                            }

                        // recursivly clean up
                        if (typeof (row.rows) != 'undefined')
                            dashboard.cleanup(row.rows);
                    }
                });
            }
        } catch (e) {
            console.error("Error in cleanup");
        }
    },
    stringifyAttr: function (key, value) {
        if (key == 'uiDroppable' || key == 'c4bTpleditor' || key == 'customCombobox' || key == 'uiAutocomplete') {
            return;
        }
        return value;
    },
    render: function (template) {
        if (template.match(/[{(](Contact|Call|User)\./)) {
            jQuery.session_ajax({
                url: serverRoot + 'Admin/RenderTemplate',
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                type: 'POST',
                data: JSON.stringify({ template: template, contact: currentPreviewData.contact || null, call: currentPreviewData.call || null, user: currentPreviewData.user || null, design: true }),
                success: function (t) {
                    template = t.rendered;
                },
                async: false
            })
        }
        return template;
    },
    changed: function () {
        $(this.id).trigger('changed')
    },
    drawn: function () {
        $(this.id).trigger('drawn')
    }
}

function existsEmptyCondition(rules, fieldname) {
    var b = rules && ((rules.condition == 'AND' || rules.condition == 'OR' && rules.rules.length == 1) && rules.rules.some(function (r) { return r.field == fieldname && r.operator == 'is_not_empty' }))
    console.log('rules', rules, b)
    console.log(b)
    return b
}

function quoteattr(s) {
    preserveCR = '&#13;';
    return ('' + s) /* Forces the conversion to string. */
        .replace(/&/g, '&amp;') /* This MUST be the 1st replacement. */
        .replace(/'/g, '&apos;') /* The 4 other predefined entities, required. */
        .replace(/"/g, '&quot;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        /*
        You may add other replacements here for HTML only 
        (but it's not necessary).
        Or for XML, only if the named entities are defined in its DTD.
        */
        //.replace(/\r\n/g, preserveCR) /* Must be before the next replacement. */
        //.replace(/[\r\n]/g, preserveCR)
        .replace(/\n/g, '\\n')
    ;
}

$(function () {
    $(window).resize(function () {
        if (!isResize) {
            deselectItem()
        }
    });
    $('#dashboard-parent').click(function (e) {
        if (!isResize && e.target.id != 'dashboard') {
            deselectItem()
        }
    })

    $('body').keyup(function (event) {
        if (event.keyCode == 46) { // delete
            deleteItem();
        }
    })
    $('#btn_item_delete').click(deleteItem);
})

function deselectItem() {
    console.log('deselect')
    $('.design-cell-selector, .design-row-selector, .dashboard-canvas-selector').removeClass('selected');
    $('.layout-border').removeClass('layout-border');
    $('.design-cell.ui-resizable, .design-row.ui-resizable, .dashboard-canvas ui-resizable').resizable('destroy');
    selectedItem = null;
    $('#db-properties').hide();
    if(dashboard.canvas.active) // view
        $('#db_properties_no_selection').show()
    else // part
        $('#part_referenced_container').show()


    $('#btn_item_delete').hide();
}

function deleteItem() {
    if ($('#dashboard-canvas').is(':visible') && selectedItem != null
        && $(document.activeElement).parents('.form').length == 0
        && $(document.activeElement).parents('.c4b-tpleditor-form').length == 0
        && !$(document.activeElement).hasClass('ace_text-input')) {

        confirm_custom('', ResX.SureToRemoveElement, '',
            [
                {
                    text: ResX.Delete,
                    click: function () {
                        dashboard.deleteItem(selectedItem);
                        $('#btn_item_delete').hide();
                    },
                    class: 'button-secondary'
                },
                { text: ResX.Cancel, click: function () { console.log("cancel") }, class: 'button-primary' }
            ], 1
        )
    }
}

function getItemName(type, id) {
    if (type != 'htmlcomponent')
        return id;

    var name = "";
    jQuery.ajax({
        url: serverRoot + 'Admin/GetItemName',
        data: { type: type, id: id },
        cache: false,
        async: false,
        success: function (data) {
            name = data.name;
        }
    })
    return name;
}


var sessionTimestamp = 0;
var isValid;
var timeoutHandle = 0;
function isSessionValid() {
    if (new Date().getTime() - sessionTimestamp < 5000)
        return isValid;
    isValid = true;
    jQuery.ajax({
        url: serverRoot + 'Admin/GetSessionInfo',
        cache: false,
        async: false,
        success: function (data) {
            sessionTimestamp = new Date().getTime();
            clearTimeout(timeoutHandle);
            var t = 120 * 1000 * 60 + 5000;
            if (data.SessionTimeout !== undefined) 
                t = data.SessionTimeout * 1000 * 60 + 5000;
            //console.log("Timeout: " + t + "ms");
            timeoutHandle = setTimeout(isSessionValid, t);

            if (!data.SessionValid) {
                setTimeout(function () { document.location.href = serverRoot + 'Designer'; }, 500);;
                isValid = false;
            }

            if (data.Sessions.length > 1) {
                $('#session_user_info').text(data.Sessions.length + " " + ResX.AdminUserSessions);
            }
            else {
                $('#session_user_info').text();
            }

            if (treeHashCode == 0)
                treeHashCode = data.TreeHashCode;


            if (data.TreeHashCode != treeHashCode) {
                if (data.TreeHashCode != 0) {
                    window.setTimeout(function () {
                            if ($('#tree').jstree('get_selected').length == 1) {
                                preselectedNodeId = $('#tree').jstree('get_selected')[0];
                                console.log('external config change - reload tree/components - id: ' + preselectedNodeId);
                            }
                            $('#tree').data('isConfigChange', true);
                            $('#tree').jstree('refresh');
                            refreshComponents();
                    }, 200)
                }
            }
                
            treeHashCode = data.TreeHashCode;
        }
    })
    return isValid;
}


jQuery.extend({
    session_ajax: function (settings) {
        if (isSessionValid()) {
            settings.async = false;
            settings.cache = false;
            return $.ajax(settings);
        }
    },
    session_post: function (url, settings) {
        if (isSessionValid()) {
            settings.async = false;
            settings.cache = false;
            return $.post(url, settings);
        }
    },
    session_ajax_async: function (settings) {
        if (isSessionValid()) {
            settings.async = true;
            settings.cache = false;
            return $.ajax(settings);
        }
    },
    session_post_async: function (url, settings) {
        if (isSessionValid()) {
            settings.async = true;
            settings.cache = false;
            return $.post(url, settings);
        }
    }
});

function getModificationDate(type, name, culture) {
    var r = '';
    $.ajax({
        url: serverRoot + 'Admin/GetModificationDate?type=' + type + '&name=' + name + '&culture=' + culture,
        dataType: 'json',
        async: false,
        cache: false,
        success: function (data) {
            r = data.datetime
        }
    });
    return r;
}

function isUnique(type, name, originalName) {
    var r = true;
    $.ajax({
        url: serverRoot + 'Admin/IsUnique?type=' + type + '&name=' + name + '&originalName=' + originalName,
        dataType: 'json',
        async: false,
        cache: false,
        success: function (data) {
            r = data.IsUnique
        }
    });
    return r;
}

var ctrlPressed = false;
$(window).keydown(function (evt) {
    if (evt.ctrlKey)
        ctrlPressed = true;
});


function purifyRows(rows) {
    var cache = [];
    var json = JSON.stringify(rows, function (key, value)
    {
        if (typeof value === 'object' && value !== null) {
            if (cache.indexOf(value) !== -1 || key == 'parent' || key == 'c4bTpleditor' || key == 'uiDroppable' || value == '' || typeof value === undefined) {
                return;
            }
            cache.push(value);
        }
        return value;
    });
    json = json.replace(/,"style":""/g, '')
    return JSON.parse(json);
}

function sortObjByKey(value) {
    return (typeof value === 'object') ?
        (Array.isArray(value) ?
            value.map(sortObjByKey) :
            Object.keys(value).sort().reduce(
                (o, key) => {
                    if (value.callMenu) // exclude tpleditor attributes
                        return o
                    const v = value[key];
                    o[key] = sortObjByKey(v);
                    return o;
                }, {})
        ) :
        value;
}