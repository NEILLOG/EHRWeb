/**
 * FileInputCus 基於Bootstrap FileInput上傳下載套件
 * @param:settings {
 *  fileAjax: Ajax URL
 *  (唯獨)fileOBJ: 檔案物件
 *  (唯獨)inputOBJ: FileID物件
 *  (唯獨)fileValue: FileID物件的值
 * }
 * @param:options：可傳入FileInput Options(會複寫defaultOptions)
 * 
 * code sample:
 * - 變更預設設定 (ex: 將單檔大小限制改為 1024 kb)
 *      $('.File').each(function () {
            $(this).FileInputCus({
                maxFileSize: 1024
            });
        });
 */

(function ($) {
    //主要Function
    $.fn.FileInputCus = function (options, settings) {
        var implementSettings = $.extend(true, {}, $.fn.FileInputCus.defaultSettings, settings);
        var implementOptions = $.extend(true, {}, $.fn.FileInputCus.defaultOptions, options);

        var fileID = "#" + $(this).attr('FileID');
        if ($(fileID).length != 1) {
            $(this).after('<label style="color:red;">設定的id有重複或是找不到，因此不載入套件</label>');
            $(this).hide();
        } else {
            //if (typeof options.delete != "undefined") {
            //    setting.deleteOBJ = options.delete;
            //}

            //console.log(fileID);

            implementSettings.fileOBJ = $(this);
            implementSettings.inputOBJ = $(fileID);
            implementSettings.fileValue = implementSettings.inputOBJ.val();

            if (typeof implementOptions.delete != "undefined") {
                implementSettings.deleteOBJ = options.delete;
            }
            
            if (implementSettings.fileValue == null || implementSettings.fileValue == '') {
                //console.log(implementSettings);
                InitFileInput(implementSettings, implementOptions);
            } else {
                GetFile(implementSettings, implementOptions);
            }

        }
    };

    //預設FileInput Options
    $.fn.FileInputCus.defaultOptions = {
        language: "zh-TW",
        initialPreview: [],
        initialPreviewAsData: true,
        initialPreviewFileType: 'image',
        initialPreviewConfig: [],
        preferIconicPreview: false, // this will force thumbnails to display icons for following file extensions
        previewFileIconSettings: { // configure your icon file extensions
            'odt': '<i class="fas fa-file-word text-primary"></i>',
            'doc': '<i class="fas fa-file-word text-primary"></i>',
            'docx': '<i class="fas fa-file-word text-primary"></i>',
            'csv': '<i class="fas fa-file-csv text-success"></i>',
            'ods': '<i class="fas fa-file-excel text-success"></i>',
            'xls': '<i class="fas fa-file-excel text-success"></i>',
            'xlsx': '<i class="fas fa-file-excel text-success"></i>',
            'odp': '<i class="fas fa-file-powerpoint text-danger"></i>',
            'ppt': '<i class="fas fa-file-powerpoint text-danger"></i>',
            'pptx': '<i class="fas fa-file-powerpoint text-danger"></i>',
            'pdf': '<i class="fas fa-file-pdf text-danger"></i>',
            'zip': '<i class="fas fa-file-archive text-muted"></i>',
            'htm': '<i class="fas fa-file-code text-info"></i>',
            'txt': '<i class="fas fa-file-alt text-info"></i>',
            'mov': '<i class="fas fa-file-video text-warning"></i>',
            'mp3': '<i class="fas fa-file-audio text-warning"></i>',
            // note for these file types below no extension determination logic 
            // has been configured (the keys itself will be used as extensions)
            //'jpg': '<i class="fas fa-file-image text-danger"></i>',
            //'gif': '<i class="fas fa-file-image text-muted"></i>',
            //'png': '<i class="fas fa-file-image text-primary"></i>'
        },
        maxFileCount: 10,   // 檔案上傳數量
        validateInitialCount: true,
        overwriteInitial: false,

        //AJAX路徑 如果要post就不能放了
        //uploadUrl: '/example/FileUpload',
        maxFilePreviewSize: 10240,

        //拖曳功能無法POST
        //dropZoneEnabled: false,

        //允許預覽檔案格式                
        allowedPreviewTypes: ['image'],
        //allowedFileExtensions: ["jpg"],
        browseLabel: "選擇檔案",
        //預覽開啟
        showPreview: true,
        removeLabel: "全部清空",
        //uploadLabel: "全部上傳",
        //關掉AJAX上傳
        showUpload: false,
        //cancelLabel: "取消上傳",
        dropZoneTitle: "請拖曳或選擇檔案",
        showRemove: false,
        fileActionSettings: {
            showZoom: false,
        }
    };

    //預設FileInput Setting
    $.fn.FileInputCus.defaultSettings = {
        fileAjax: '/Backend/File/GetFile',
        fileOBJ: {},
        inputOBJ: {},
        fileValue: {},
        deleteOBJ: {}
    };


    //初始化FileInput
    function InitFileInput(settings, options, preview) {
        if (typeof preview != "undefined") {
            if (typeof preview.initialPreview != "undefined") {
                options.initialPreview = preview.initialPreview;
            }
            if (typeof preview.initialPreviewConfig != "undefined") {
                options.initialPreviewConfig = preview.initialPreviewConfig;
            }
        }
        settings.fileOBJ.fileinput(options)
            .on("filedeleted", function (event, data, previewId, index) {
                //刪除點擊後的事件 
                var count = $('.' + settings.deleteOBJ).length;

                settings.inputOBJ.after($('<input/>', {
                    name: settings.deleteOBJ + '[' + count + ']',
                    type: "text",
                    class: settings.deleteOBJ,
                    value: data
                }).hide());

                //刪除點擊後的事件
            })
            .on("filebeforedelete", function (event, key, data) {
                //傳true才會取消  詭異
                return !confirm("是否刪除?");
            })
            .on('filecleared', function (event) {
                console.log("filecleared");
            })
            .on('fileclear', function (event) {
                console.log("fileclear");
            });
    }

    //取得檔案資訊，並初始化FileInput
    function GetFile(settings, options) {
        $.ajax({
            url: settings.fileAjax,
            type: "POST",
            dataType: "json",
            data: { FileID: settings.fileValue },
            success: function (result) {
                if (result.isSuccess === true) {
                    //console.log(settings);
                    //console.log(result);
                    InitFileInput(settings, options, result.data);
                }
            }
        });
    }

})(jQuery);