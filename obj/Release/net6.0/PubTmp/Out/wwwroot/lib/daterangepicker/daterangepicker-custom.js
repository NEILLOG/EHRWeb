jQuery(function ($) {
    $.fn.daterangepicker.defaultOptions = {
        autoUpdateInput: false,
        showDropdowns: true,
        timePicker24Hour: true,
        locale: {
            format: 'YYYY/MM/DD',
            applyLabel: '確認',
            cancelLabel: '取消',
            fromLabel: '從',
            toLabel: '到',
            weekLabel: 'W',
            customRangeLabel: 'Custom Range',
            daysOfWeek: ["日", "一", "二", "三", "四", "五", "六"],
            monthNames: ["一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月"],
        }
    }

    $('.daterange_selector').on('apply.daterangepicker', function (ev, picker) {
        if (picker.timePicker) {
            if (picker.singleDatePicker) {
                $(ev.target).val(picker.startDate.format('YYYY/MM/DD HH:mm'));
            } else {
                $(ev.target).val(picker.startDate.format('YYYY/MM/DD HH:mm') + " - " + picker.endDate.format('YYYY/MM/DD HH:mm'));
            }
        } else {
            if (picker.singleDatePicker) {
                $(ev.target).val(picker.startDate.format('YYYY/MM/DD'));
            } else {
                $(ev.target).val(picker.startDate.format('YYYY/MM/DD') + " - " + picker.endDate.format('YYYY/MM/DD'));
            }
        }
    }).on('cancel.daterangepicker', function (ev, picker) {
        $(ev.target).val('');
    });
});




