var _NowEditorId = null;

$(document).ready(function () {

    var main_container = $('#tableOptions');

    //先初始化，避免postback回來之後，因未開啟過modal導致找不到instance
    new bootstrap.Modal($('#mdOptionTitle')[0]);
    new bootstrap.Modal($('#mdOptionText')[0]);
    new bootstrap.Modal($('#mdOptionRadio')[0]);
    new bootstrap.Modal($('#mdOptionCheckbox')[0]);

    main_container.tableDnD();

    $.each(main_container.find('tbody tr'), function (i, ele) {
        $(ele).find('.btnEdit').click(EditButton);
        $(ele).find('.btnDelete').click(DeleteButton);
        $(ele).find('.btnCopy').click(CopyButton);
    });

    //存檔: 標題
    $('#btnSaveTitle').click(function () {

        var container = $(this).parents('div.modal');
        var modal = bootstrap.Modal.getInstance(container[0]);
        var isUpdate = (_NowEditorId != '' && typeof _NowEditorId != 'undefined' && _NowEditorId != null) //是否為編輯

        var ele_title = container.find('#txtTitle');

        if (ele_title.val() == '') {
            Swal.fire('請輸入標題');
            return;
        }

        var NewRow = ParseModelToRow({
            ID: isUpdate ? _NowEditorId : _uuid(),
            Type: 1,
            TypeString: '標題',
            QuizDescription: ele_title.val(),
            FillDirection: '',
            Options: '',
            IsRequired: 0
        });

        NewRow.find('.btnEdit').click(EditButton);
        NewRow.find('.btnDelete').click(DeleteButton);
        NewRow.find('.btnCopy').click(CopyButton);

        if (isUpdate) {
            var ele = FindRow(_NowEditorId);
            $(ele).after(NewRow);
            DeleteRow(_NowEditorId);
        }
        else
            main_container.find('tbody').append(NewRow);

        modal.hide();
        //清除編輯器
        ele_title.val('');
        _NowEditorId = null;
        main_container.tableDnD();
        
    });

    //存檔: 簡答
    $('#btnSaveText').click(function () {
        var container = $(this).parents('div.modal');
        var modal = bootstrap.Modal.getInstance(container[0]);
        var isUpdate = (_NowEditorId != '' && typeof _NowEditorId != 'undefined' && _NowEditorId != null) //是否為編輯

        var ele_desc = container.find('#txtQuizDescriptionText');
        var ele_dir = container.find('#txtFillDirectionText');
        var ele_req = container.find('#isRequiredText');

        if (ele_desc.val() == '') {
            Swal.fire('請輸入問題描述');
            return;
        }

        var NewRow = ParseModelToRow({
            ID: isUpdate ? _NowEditorId : _uuid(),
            Type: 2,
            TypeString: '簡答',
            QuizDescription: ele_desc.val(),
            FillDirection: ele_dir.val(),
            Options: '',
            IsRequired: ele_req.is(':checked')
        });

        NewRow.find('.btnEdit').click(EditButton);
        NewRow.find('.btnDelete').click(DeleteButton);
        NewRow.find('.btnCopy').click(CopyButton);

        if (isUpdate) {
            var ele = FindRow(_NowEditorId);
            $(ele).after(NewRow);
            DeleteRow(_NowEditorId);
        }
        else
            main_container.find('tbody').append(NewRow);

        modal.hide();
        //清除編輯器
        ele_desc.val('');
        ele_dir.val('');
        ele_req.prop('checked', false);
        _NowEditorId = null;
        main_container.tableDnD();
        
    });

    //存檔: 單選
    $('#btnSaveRadio').click(function () {
        var container = $(this).parents('div.modal');
        var modal = bootstrap.Modal.getInstance(container[0]);
        var isUpdate = (_NowEditorId != '' && typeof _NowEditorId != 'undefined' && _NowEditorId != null) //是否為編輯

        var ele_desc = container.find('#txtQuizDescriptionRadio');
        var ele_dir = container.find('#txtFillDirectionRadio');
        var ele_req = container.find('#isRequiredRadio');
        var ele_options = container.find('.option');
        var options = [];

        if (ele_desc.val() == '') {
            Swal.fire('請輸入問題描述');
            return;
        }
        if (ele_options.length == 0) {
            Swal.fire('請至少輸入一個選項');
            return;
        }

        $.each(ele_options, function (i, e) {
            options.push($(e).val());
        }); console.log(options);

        var NewRow = ParseModelToRow({
            ID: isUpdate ? _NowEditorId : _uuid(),
            Type: 3,
            TypeString: '單選',
            QuizDescription: ele_desc.val(),
            FillDirection: ele_dir.val(),
            Options: options.join('|'),
            IsRequired: ele_req.is(':checked')
        });

        NewRow.find('.btnEdit').click(EditButton);
        NewRow.find('.btnDelete').click(DeleteButton);
        NewRow.find('.btnCopy').click(CopyButton);

        if (isUpdate) {
            var ele = FindRow(_NowEditorId);
            $(ele).after(NewRow);
            DeleteRow(_NowEditorId);
        }
        else
            main_container.find('tbody').append(NewRow);

        modal.hide();
        //清除編輯器
        ele_desc.val('');
        ele_dir.val('');
        ele_req.prop('checked', false);
        $('#containerRadio').empty();
        _NowEditorId = null;
        main_container.tableDnD();
        
    });

    //存檔: 複選
    $('#btnSaveCheckbox').click(function () {
        var container = $(this).parents('div.modal');
        var modal = bootstrap.Modal.getInstance(container[0]);
        var isUpdate = (_NowEditorId != '' && typeof _NowEditorId != 'undefined' && _NowEditorId != null) //是否為編輯

        var ele_desc = container.find('#txtQuizDescriptionCheckbox');
        var ele_dir = container.find('#txtFillDirectionCheckbox');
        var ele_req = container.find('#isRequiredCheckbox');
        var ele_options = container.find('.option');
        var options = [];

        if (ele_desc.val() == '') {
            Swal.fire('請輸入問題描述');
            return;
        }
        if (ele_options.length == 0) {
            Swal.fire('請至少輸入一個選項');
            return;
        }

        $.each(ele_options, function (i, e) {
            options.push($(e).val());
        });

        var NewRow = ParseModelToRow({
            ID: isUpdate ? _NowEditorId : _uuid(),
            Type: 4,
            TypeString: '複選',
            QuizDescription: ele_desc.val(),
            FillDirection: ele_dir.val(),
            Options: options.join('|'),
            IsRequired: ele_req.is(':checked')
        });

        NewRow.find('.btnEdit').click(EditButton);
        NewRow.find('.btnDelete').click(DeleteButton);
        NewRow.find('.btnCopy').click(CopyButton);

        if (isUpdate) {
            var ele = FindRow(_NowEditorId);
            $(ele).after(NewRow);
            DeleteRow(_NowEditorId);
        }
        else
            main_container.find('tbody').append(NewRow);

        modal.hide();
        //清除編輯器
        ele_desc.val('');
        ele_dir.val('');
        ele_req.prop('checked', false);
        $('#containerCheckbox').empty();
        _NowEditorId = null;
        main_container.tableDnD();
        
    });

    //-----

    //新增單選選項
    $('#btnAddOptionRadio').click(function () {
        var container = $('#containerRadio');
        var new_row = $("#tmpl-option").tmpl('');
        new_row.find('.btnDeleteOption').click(function () {
            $(this).parents('div.input-group').remove();
        });
        container.append(new_row);
    });

    //新增複選選項
    $('#btnAddOptionCheckbox').click(function () {
        var container = $('#containerCheckbox');
        var new_row = $("#tmpl-option").tmpl('');
        new_row.find('.btnDeleteOption').click(function () {
            $(this).parents('div.input-group').remove();
        });
        container.append(new_row);
    });

    //取消
    $('.btnCancelTitle').click(function () {
        $('#txtTitle').val('');
        _NowEditorId = null;
    });

    $('.btnCancelText').click(function () {
        $('#txtQuizDescriptionText').val('');
        $('#txtFillDirectionText').val('');
        $('#isRequiredText').prop('checked', false);
        _NowEditorId = null;
    });

    $('.btnCancelRadio').click(function () {

        $('#txtQuizDescriptionRadio').val('');
        $('#txtFillDirectionRadio').val('');
        $('#isRequiredRadio').prop('checked', false);
        $('#containerRadio').empty();
        _NowEditorId = null;
    });

    $('.btnCancelCheckbox').click(function () {
        $('#txtQuizDescriptionCheckbox').val('');
        $('#txtFillDirectionCheckbox').val('');
        $('#isRequiredCheckbox').prop('checked', false);
        $('#containerCheckbox').empty();
        _NowEditorId = null;
    });

});

function ParseRowToModel(ele) {
    var container = $(ele);
    var ID = container.find('input.hidId').val();
    var Type = container.find('input.hidType').val(); 
    var QuizDescription = container.find('input.hidQuizDescription').val();
    var FillDirection = container.find('input.hidFillDirection').val();
    var Options = container.find('input.hidOptions').val();
    var IsRequired = container.find('input.hidIsRequired').val();
    var TypeString = '標題'; //題目類型: 1:標題 2:簡答題 3:單選 4:複選

    switch (Type) {
        case '1': TypeString = '標題'; break;
        case '2': TypeString = '簡答'; break;
        case '3': TypeString = '單選'; break;
        case '4': TypeString = '複選'; break;
    }

    return {
        ID: ID,
        Type: Type,
        QuizDescription: QuizDescription,
        FillDirection: FillDirection,
        Options: Options,
        IsRequired: IsRequired,
        TypeString: TypeString
    };
}

function ParseModelToRow(model) {
    console.log(model);
    return $("#tmpl-row").tmpl(model);
}

function ParseModelToEditor(model) {
    switch (model.Type) {
        case '1': //標題
            $('#txtTitle').val(model.QuizDescription);
            break;
        case '2': //簡答
            $('#txtQuizDescriptionText').val(model.QuizDescription);
            $('#txtFillDirectionText').val(model.FillDirection); console.log(model.IsRequired);
            $('#isRequiredText').prop('checked', model.IsRequired == "true");
            break;
        case '3': //單選
            $('#txtQuizDescriptionRadio').val(model.QuizDescription);
            $('#txtFillDirectionRadio').val(model.FillDirection);
            $('#isRequiredRadio').prop('checked', model.IsRequired == "true");

            var container = $('#containerRadio');
            var option = model.Options.split('|');

            $.each(option, function (i, e) {
                var new_row = $("#tmpl-option").tmpl({ option: e });
                new_row.find('.btnDeleteOption').click(function () {
                    $(this).parents('div.input-group').remove();
                });
                container.append(new_row);
            });

            break;
        case '4': //複選
            $('#txtQuizDescriptionCheckbox').val(model.QuizDescription);
            $('#txtFillDirectionCheckbox').val(model.FillDirection);
            $('#isRequiredCheckbox').prop('checked', model.IsRequired == "true");

            var container = $('#containerCheckbox');
            var option = model.Options.split('|');

            $.each(option, function (i, e) {
                var new_row = $("#tmpl-option").tmpl({ option: e });
                new_row.find('.btnDeleteOption').click(function () {
                    $(this).parents('div.input-group').remove();
                });
                container.append(new_row);
            });
            break;
    }
}

function DeleteRow(target_id) {
    $('#tableOptions').find('tr').each(function (index, ele) {
        var id = $(ele).find('input.hidId').val();
        if (id == target_id) {
            $(ele).remove();
            return false;
        }
    });
}

function FindRow(target_id) {
    var target_ele;
    $('#tableOptions').find('tr').each(function (index, ele) {
        var id = $(ele).find('input.hidId').val();
        if (id == target_id)
            target_ele = $(ele);
    });

    return target_ele;
}

function EditButton() {
    var row = $(this).parents('tr');
    var model = ParseRowToModel(row);
    var id = model.ID;

    _NowEditorId = id; //將現在要編輯哪一列的ID存到全域的元件內

    //判斷這是哪一個題型
    var md = null;
    switch (model.Type) {
        case '1': md = bootstrap.Modal.getInstance($('#mdOptionTitle')[0]); break;
        case '2': md = bootstrap.Modal.getInstance($('#mdOptionText')[0]); break;
        case '3': md = bootstrap.Modal.getInstance($('#mdOptionRadio')[0]); break;
        case '4': md = bootstrap.Modal.getInstance($('#mdOptionCheckbox')[0]); break;
    }

    //將這一列的資料還原至編輯器
    ParseModelToEditor(model);
    md.show();
}

function CopyButton() {
    var row = $(this).parents('tr');
    var model = ParseRowToModel(row);
        model.ID = _uuid();

    var NewRow = ParseModelToRow(model);

    NewRow.find('.btnEdit').click(EditButton);
    NewRow.find('.btnDelete').click(DeleteButton);
    NewRow.find('.btnCopy').click(CopyButton);

    $('#tableOptions').find('tbody').append(NewRow);
}

function DeleteButton() {
    var row = $(this).parents('tr');
    Swal.fire({
        title: '確認刪除?',
        showCancelButton: true,
        confirmButtonText: '確認',
    }).then((result) => {
        if (result.isConfirmed) {
            row.remove();
        }
    });
}

function SortSection() {
    $('#tableOptions').find('tbody tr').each(function (index, ele) {

        $(ele).find('input[type=hidden]').each(function (i, hidden) {

            var o = $(hidden).prop('name');
                o = o.replace(/\[\d*\]/i, '[' + index + ']');

            $(hidden).prop('name', o);
        });
    });
}

//產生guid
function _uuid() { //參考:https://www.cythilya.tw/2017/03/12/uuid/
    var d = Date.now();
    if (typeof performance !== 'undefined' && typeof performance.now === 'function') {
        d += performance.now(); //use high-precision timer if available
    }
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = (d + Math.random() * 16) % 16 | 0;
        d = Math.floor(d / 16);
        return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
    });
}