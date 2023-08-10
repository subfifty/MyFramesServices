cellId = 2;
dashboard = {
    rows: [
        { cells: [{ id: 'cell-1', text: 'Part1' }] }
    ],

    getParent: function (id, node) {
        if (typeof (node) == 'undefined')
            node = this;
        ret = {};
        $(node.rows).each(function (i, row) {
            $(row.cells).each(function (ii, cell) {
                if (typeof (cell.id) != 'undefined') {
                    if (cell.id == id)
                        ret = { row: row, rows: node.rows, node: node };
                }
                else
                    ret = dashboard.getParent(id, cell);
            })
        })
        return ret;
    }
}


function addCell(id, pos, text) {
    newCell = { id: 'cell-' + (cellId++), text: text };
    if (id.lastIndexOf('cell-container', 0) === 0) {
        // inside of container, find first real child cell 
        cellid = $('#' + id).children('.cell:not([id^=cell-container])').attr('id');
        parent = dashboard.getParent(cellid);
        switch (pos) {
            case 0:
                parent.rows.splice(0, 0, { cells: [newCell] });
                break;
            case 1:
                if (parent.rows.length == 1)
                    parent.row.cells.push(newCell);
                else {
                    n = $.extend({}, parent.node)
                    parent.node.rows = [{ cells: [{ rows: n.rows }, newCell] }];
                }
                break;
            case 2:
                parent.rows.push({ cells: [newCell] });
                break;
            case 3:
                row.cells.splice(0, 0, newCell);
                break;
        }
    }
    else {
        parent = dashboard.getParent(id);
        switch (pos) {
            case 0:
                parent.node.rows.unshift({ cells: [newCell] });
                break;
            case 1:
                parent.row.cells.push(newCell);
                break;
            case 2:
                parent.node.rows.push({ cells: [newCell] });
                break;
            case 3:
                parent.row.cells.unshift(newCell);
                break;
        }
    }
    drawDashboard();
}

function getCellHtml(id, isContainer, width, height) {

    if (typeof (isContainer) == 'undefined')
        isContainer = false;
    cls = 'cell' + (isContainer? ' container' : '');

    return '<div style="width:'+width+'%;height:'+height+'%" class="'+cls+'" id="' + id + '"><span class="btn-design btn-new-left" data-pos="3">+</span><span class="btn-design btn-new-top" data-pos="0">+</span><span class="btn-design btn-new-right" data-pos="1">+</span><span class="btn-design btn-new-bottom" data-pos="2">+</span> <p>'
}

var arrOver = [];

function drawDashboard() {
    content = '';
    drawRows(dashboard.rows, 100);
    $('#dashboard').html(content);
    $('.btn-design').droppable({
        tolerance: 'pointer',
        over: function () {
            $(this).addClass('btn-design-selected');
        },
        out: function () {
            $(this).removeClass('btn-design-selected')
        },
        drop: function (event, ui) {
            addCell($(this).parent().attr('id'), $(this).data('pos'), ui.draggable.text());
        }
    })

    $(".cell").resizable()
    .droppable({
        tolerance: 'pointer',
        over: function () {
            arrOver.push($(this));
            $('span.btn-design').css('opacity', '0');
            $(this).children('span.btn-design').css('opacity', '0.5');
        },
        out: function () {
            $('span.btn-design').css('opacity', '0');
            arrOver.pop();
            if (arrOver.length > 0)
                arrOver[arrOver.length - 1].children('span.btn-design').css('opacity', '0.5');
        },
        drop: function (event, ui) {
        }
    })
    .draggable({
        start: function (event, ui) {
            old_zindex = $(this).css('z-index');
            old_left = $(this).css('left');
            old_top = $(this).css('top');
            $(this).css('z-index', 1);
            $(this).css('opacity', 0.5);
        },
        stop: function () {
            $(this).css('z-index', old_zindex);
            $(this).css('left', old_left);
            $(this).css('top', old_top);
            $(this).css('opacity', 1);
        }
    }); 
}
var currentRows = null;
var currentRow = null;
var currentCells = null;
var padding = 0;
var margin = 0;

function drawRows(rows, width) {
    currentRows = rows;
    if(rows.length > 1)
        content += getCellHtml('cell-container-' + cellId++, true, width - 2 * (padding + margin), 100 - 2 * (padding + margin));
    var height = 100 / rows.length - 2 * (padding + margin);
    $(rows).each(function (i, row) {
        currentRow = row;
        drawRow(row, height);
    })
    if (rows.length > 1)
        content += '</div>';
}

function drawRow(row, height) {
    var h = height;
    if (row.cells.length > 1) {
        content += getCellHtml('cell-container-' + cellId++, false, 100 - 2 * (padding + margin), height);
        h = 100 - 2* margin;
    }

    var width = 100 / row.cells.length - 2 * (padding + margin);
    $(row.cells).each(function (i, cell) {
        if (typeof (cell.rows) != 'undefined') {
            drawRows(cell.rows, width);
        }
        else {
            drawCell(cell, width,h);
        }
    })

    if (row.cells.length > 1) {
        content += '</div><div style="clear:both"></div>';
    }
    else {
        content += '<div style="clear:both"></div>';
    }
}

function drawCell(cell, width, height) {

    content += getCellHtml(cell.id, false, width, height) + cell.text + '(' + cell.id + ')' + '</div>';

}

$(document).ready(function () {
    drawDashboard();

    $(".part").draggable({
        cursor: "pointer",
        opacity: 0.35,
        start: function (event, ui) {
            old_zindex = $(this).css('z-index');
            old_left = $(this).css('left');
            old_top = $(this).css('top');
            $(this).css('z-index', 1);
            $('.cell').addClass('cell-layout');
            $(this).draggable("option", "cursorAt", {
                left: Math.floor(ui.helper.width() / 2),
                top: Math.floor(ui.helper.height() / 2)
            });

        },
        stop: function () {
            $(this).css('z-index', old_zindex);
            $(this).css('left', old_left);
            $(this).css('top', old_top);
            $('.cell').removeClass('cell-layout cell-layout-active');
            $('span.btn-design').css('opacity', '0');
        }
    })

});
