/**
* @name: Parsley 自定義Validate Function
* @version: 1.0.0
* @copyright: Copyright © 2022 iscom-7-wolf.
*/

jQuery(function ($) {
    ///* 忽略驗證類型(全域)，由於官方文件未提供此設定方法，改為依照文件方法直接設定在 form 上，請全域搜尋 @data_parsley_excluded */
    //window.Parsley.options.excluded = "input[type=button], input[type=submit], input[type=reset], input[type=hidden], [disabled], :hidden, .d-none";

    //檔案大小限制(MB)
    window.Parsley.addValidator('maxFileSize', {
        validateString: function (_value, maxSize, parsleyInstance) {
            console.log(_value);
            console.log(maxSize);
            if (!window.FormData) {
                alert('You are making all developpers in the world cringe. Upgrade your browser!');
                return true;
            }
            // 錯誤訊息
            window.Parsley.addMessage('zh-tw', 'maxFileSize', '檔案大小不可超過 %s Mb.');
            // 判斷檔案大小
            var totalFileSize = 0;
            var files = parsleyInstance.$element[0].files;
            $(files).each(function (idx, obj) {
                totalFileSize = totalFileSize + obj.size;
            });
            return totalFileSize <= maxSize * 1024 * 1024;
        },
        requirementType: 'integer'
    });

    //檔案數量限制
    window.Parsley.addValidator('maxFileCount', {
        validateString: function (_value, maxCount, parsleyInstance) {
            if (!window.FormData) {
                alert('You are making all developpers in the world cringe. Upgrade your browser!');
                return true;
            }
            // 錯誤訊息
            window.Parsley.addMessage('zh-tw', 'maxFileCount', '上傳的檔案數量不可超過 %s 個.');

            // 判斷檔案數量
            var files = parsleyInstance.$element[0].files;
            return files.length <= maxCount;
        },
        requirementType: 'integer'
    });

    //檔案類型限制(以逗號分隔副檔名，EX:PDF,DOC,DOCX <大小寫沒差>)
    window.Parsley.addValidator('fileType', {
        validateString: function (_value, fileType, parsleyInstance) {
            if (!window.FormData) {
                alert('You are making all developpers in the world cringe. Upgrade your browser!');
                return true;
            }
            // 錯誤訊息
            window.Parsley.addMessage('zh-tw', 'fileType', '上傳的檔案類型，只能是 %s');

            // 判斷檔案數量
            var fileTypes = fileType.toLowerCase().split(',');
            var ok = true;
            var files = parsleyInstance.$element[0].files;
            $(files).each(function (idx, obj) {
                var fileNameSplit = obj.name.split(".");
                var fileExtension = fileNameSplit[fileNameSplit.length - 1];
                if (jQuery.inArray(fileExtension.toLowerCase(), fileTypes) < 0) {
                    ok = false;
                    return false; //等同於break
                }
            });
            return ok;
        },
        requirementType: 'string'
    });

    /** 
     * code sample:
     *  data_parsley_phone = "" - 市話/手機
     *  data_parsley_phone = "tel" - 市話
     *  data_parsley_phone = "mobile" - 手機
     */
    window.Parsley.addValidator('phone', {
        validateString: function (value, type) {
            let regex;
            switch (type) {
                case "tel":
                    regex = new RegExp("^0[2-8]\\d{0,1}-?(\\d{6,8})(#\\d{1,5}){0,1}$");
                    break;
                case "mobile":
                    regex = new RegExp("^09\\d{2}(\\d{6}|\\d{3}\\d{3})$");
                    break;
                default:
                    regex = new RegExp("^0[2-8]\\d{0,1}-?(\\d{6,8})(#\\d{1,5}){0,1}$|^09\\d{2}(\\d{6}|\\d{3}\\d{3})$");
                    break;
            }

            return regex.test(value);
        },
        requirementType: 'string',
        messages: {
            'en': 'Please enter a valid phone format.',
            'zh-tw': '請填入有效電話號碼'
        }
    });

    /** 
     * code sample:
     *  data_parsley_psw = "" - 6-12碼，其組成必須至少有一個特殊符號、一個大寫或小寫字母、一個數字
     */
    window.Parsley.addValidator('psw', {
        validateString: function (value) {
            let regex = new RegExp("^(?=.*\\d)(?=.*[a-zA-Z])(?=.*\\W).{6,12}$");

            return regex.test(value);
        },
        requirementType: 'string',
        messages: {
            'en': 'Password should contain 6 to 12 characters, at least one alphabetical character, one number and one special character.',
            'zh-tw': '密碼長度須為6-12碼，其組成必須至少有一個特殊符號、一個大寫或小寫字母、一個數字'
        }
    });

    /** 
     * code sample:
     *  data_parsley_idnumber = "" - 身分證或居留證號
     *  deps: taiwan-id-validator2.min.js
     */
    window.Parsley.addValidator('idnumber', {
        validateString: function (value) {
            console.log(value);
            return taiwanIdValidator.isNationalIdentificationNumberValid(value) || taiwanIdValidator.isResidentCertificateNumberValid(value);
        },
        requirementType: 'string',
        messages: {
            'en': 'Please enter a valid id number or resident certificate number.',
            'zh-tw': '請輸入合法身分證字號或居留證編號'
        }
    });

    /** 
     * code sample:
     *  data_parsley_sid = "" - NTU-學生證號
     */
    window.Parsley.addValidator('sid', {
        validateString: function (value) {
            let regex = new RegExp("^[A-Z]{1}[0-9]{8,9}$");

            return regex.test(value);
        },
        requirementType: 'string',
        messages: {
            'en': 'Please enter a valid student id number.',
            'zh-tw': '請輸入正確學生證號格式'
        }
    });

    ///** 
    // * code sample:
    // *  data_parsley_test = "" - 
    // */
    //window.Parsley.addValidator('test', {
    //    validateString: function (value, requeriment, instance, element ) {
    //        console.log(value); console.log(requeriment);
    //        console.log(instance); console.log(element);

    //        return true;
    //    },
    //    requirementType: 'boolean',
    //    messages: {
    //        'en': '',
    //        'zh-tw': ''
    //    }
    //});

    /** 
     * code sample:
     *  data_parsley_regex_example = ""
     * 
     */
    window.Parsley.addValidator('regexExample', {
        validateString: function (value) {
            var regx = new RegExp("^([a-z0-9]{5,})$");

            return regx.test(value);
        },
        requirementType: 'string',
        messages: {
            en: 'regex_ex wrong format',
            'zh-tw': 'regex_ex 格式錯誤'
        }
    });

});



