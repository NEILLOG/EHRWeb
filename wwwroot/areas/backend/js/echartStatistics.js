
function ActivityChart(Year, ActID, ActName, Filter, FilterValue) {
    var myChart = echarts.init(document.getElementById('ChartDIV'));
    myChart.showLoading();
    $.ajax({
        type: "Post",
        url: "/Backend/Statistics/GetActivityChart",
        dataType: "json",
        beforeSend: function (request) {
            request.setRequestHeader("X-XSRF-TOKEN", $('input[name="__X-XSRFVerificationToken"]').val());
        },
        data: {
            ActID: ActID,
            Filter: Filter
        },
        success: function (result) {
            console.log(result);           

            var option;

            option = {
                title: {
                    text: Year + '年 - ' + ActName + ' - ' + FilterValue + " - 統計",
                    left: 'center'
                },
                tooltip: {
                    trigger: 'item',
                    formatter: "{b}: {c}"
                },
                legend: {
                    type: 'scroll',
                    orient: 'vertical',
                    right: 'left',
                    top: 'center',
                    data: Object.values(result.chartData)

                },
                series: [
                    {
                        name: '',
                        type: 'pie',
                        radius: '80%',
                        center: ['50%', '50%'],
                        avoidLabelOverlap: false,
                        label: {
                            normal: {
                                show: true,
                                position: 'inner',
                                formatter: function (params) {
                                    if (params.percent > 3) {
                                        return params.percent + '%';
                                    } else {
                                        return '';
                                    }
                                }
                            }
                        },
                        data: (function () {
                            var res = [];
                            var len = result.chartData.length;
                            for (var i = 0; i < len; i++) {
                                res.push({
                                    value: result.chartValue[i],
                                    name: result.chartData[i]                            
                                });
                            }
                            return res;
                        })()
                    }
                ]
            };

            myChart.hideLoading();
            myChart.setOption(option);
        }
    });

    //-- 圖表自適應
    window.addEventListener('resize', function () {
        myChart.resize();
    })
}

function ConsultChart(Year) {
    var myChart = echarts.init(document.getElementById('ChartDIV'));
    myChart.showLoading();

    $.ajax({
        type: "Post",
        url: "/Backend/Statistics/GetConsultChart",
        dataType: "json",
        beforeSend: function (request) {
            request.setRequestHeader("X-XSRF-TOKEN", $('input[name="__X-XSRFVerificationToken"]').val());
        },
        data: {
            Year: Year
        },
        success: function (result) {
            console.log(result);

            var option;

            option = {
                title: {
                    text: Year + '年 - 統計',
                    left: 'center'
                },
                tooltip: {
                    trigger: 'item',
                    formatter: "{b}: {c}"
                },
                legend: {
                    type: 'scroll',
                    orient: 'vertical',
                    right: 'left',
                    top: 'center',
                    data: Object.values(result.chartData)

                },
                series: [
                    {
                        name: '',
                        type: 'pie',
                        radius: '80%',
                        center: ['50%', '50%'],
                        avoidLabelOverlap: false,
                        label: {
                            normal: {
                                show: true,
                                position: 'inner',
                                formatter: function (params) {
                                    if (params.percent > 3) {
                                        return params.percent + '%';
                                    } else {
                                        return '';
                                    }
                                }
                            }
                        },
                        data: (function () {
                            var res = [];
                            var len = result.chartData.length;
                            for (var i = 0; i < len; i++) {
                                res.push({
                                    value: result.chartValue[i],
                                    name: result.chartData[i]
                                });
                            }
                            return res;
                        })()
                    }
                ]
            };

            myChart.hideLoading();
            myChart.setOption(option);
        }
    });

    //-- 圖表自適應
    window.addEventListener('resize', function () {
        myChart.resize();
    })
}

