// $Http Request
function Post_Http($http, url, data, finishedFunction, isShowMessages) {
    $http({
        url: url,
        method: "POST",
        data: data,
        headers: { "Content-Type": "application/json" }
    }).then(function (result) {
        finishedFunction(result);
    }, function (xhr) {
        if (isShowMessages)
            notification(notifiType.error, "Thất bại!", xhr.responseText);
    });
}

function Get_Http($http, url, data, finishedFunction, isShowMessages) {
    $http({
        url: url,
        method: "GET",
        data: data,
        cache: false,
    }).then(function (result) {
        finishedFunction(result);
    }, function (xhr) {
        if (isShowMessages)
            notification(notifiType.error, "Thất bại!", xhr.responseText);
    });
}

// Ajax request
function Get_AjaxCaller(url, data, successFunction, finishedFunction) {
    $.when(
        $.ajax({
            url: url,
            type: "GET",
            async: false,
            cache: false,
            data: data,
            success: function (result) {
                successFunction(result);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                //window.ShowErrorMessage(thrownError.message);
            }
        })
    ).then(finishedFunction);
}

function Post_AjaxCaller(url, data, successFunction, finishedFunction) {
    $.when(
        $.ajax({
            url: url,
            type: "POST",
            data: data,
            async: false,
            cache: false,
            success: function (result) {
                successFunction(result);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                notification(notifiType.error, "Thất bại!", thrownError.message);
            }
        })
    ).then(finishedFunction);
}

function AjaxCallerPartialViewCompile(url, data, scope, compile, divId, finishedFunction) {
    $.when(
        $.ajax({
            url: url,
            type: "GET",
            async: false,
            cache: false,
            contentType: 'application/html;charset=utf-8',
            dataType: 'html',
            data: data,
            success: function (result) {
                // scope.$apply(function () {
                var content = compile(result)(scope);
                //element.append(content);
                $(divId).empty();
                //jQuery(divId).html('');
                $(divId).append(content);

                //   })
                // $(divId).html(result);
                // compile(result)(scope); /// biên dịch từ html sang scope angular.
            },
            error: function (xhr, ajaxOptions, thrownError) {
                //window.ShowErrorMessage(thrownError.message);
            }
        })
    ).then(finishedFunction);
}

function AjaxCallerPartialView(url, data, scope, divId, finishedFunction) {
    $.when(
        $.ajax({
            url: url,
            type: "GET",
            async: false,
            cache: false,
            contentType: 'application/html;charset=utf-8',
            dataType: 'html',
            data: data,
            success: function (result) {
                $(divId).empty();
                $(divId).html(result);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                //window.ShowErrorMessage(thrownError.message);
            }
        })
    ).then(finishedFunction);
}

function ConfirmPostAjax(title, text, type, successMessage, url, data, finalFunction) {
    if (!title) title = "Do you want to save it?";
    if (!text) text = "You won't be able to undo this action.";
    if (!successMessage) successMessage = "Saved successfully!";
    type = type === 1 ? "question" : "warning";

    swal({
        title: title,
        text: text,
        type: type,
        animation: false,
        showCancelButton: true,
        showLoaderOnConfirm: true,
        confirmButtonClass: "btn-danger",
        width: 450,
        confirmButtonText: "Yes",
        closeOnClickOutside: false,
        preConfirm: function () {
            return new Promise(function (resolve) {
                setTimeout(function () {
                    window.Post_AjaxCaller(url,
                        data,
                        function (result) {
                            finalFunction(result);
                        },
                        function () {
                            setTimeout(function () {
                                swal.closeModal();
                            }, 1000);
                        });
                }, 100);


            });
        }
    }).then(function () {
    });
}

// Message configuration.
function ResponseMessage(result) {
    if (result == undefined || result == null || result == '') return;
    else {
        var response = result;
        if (response.Type == 1) {
            notification(notifiType.info, "Thông báo!", response.Message);
        }
        else if (response.Type == 2) {
            notification(notifiType.success, "Thành công!", response.Message);
        }
        else if (response.Type == 2) {
            notification(notifiType.warning, "Cảnh báo!", response.Message);
        }
        else if (response.Type == 4) {
            notification(notifiType.error, "Thất bại!", response.Message);
        }
        else {
            notification(notifiType.error, "Thất bại!", response.Message);
        }
    }
};

function getParameterByName(name) {
    var url = window.location.href;
    name = name.replace(/[\[\]]/g, "\\$&");
    var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return "";
    return decodeURIComponent(results[2].replace(/\+/g, " "));
};

function ConvertJsonToDatetime(date) {
    return moment(date).format(_dateFormat.toUpperCase());
}
function IsDateTime(str) {
    return new RegExp(/^\/Date\((\d+)(?:-(\d+))?\)\/$/).test(str);
}
var ChuSo = new Array(" không ", " một ", " hai ", " ba ", " bốn ", " năm ", " sáu ", " bảy ", " tám ", " chín ");
var Tien = new Array("", " nghìn", " triệu", " tỷ", " nghìn tỷ", " triệu tỷ");

//1. Hàm đọc số có ba chữ số;
function DocSo3ChuSo(baso) {
    var tram;
    var chuc;
    var donvi;
    var KetQua = "";
    tram = parseInt(baso / 100);
    chuc = parseInt((baso % 100) / 10);
    donvi = baso % 10;
    if (tram == 0 && chuc == 0 && donvi == 0) return "";
    if (tram != 0) {
        KetQua += ChuSo[tram] + " trăm ";
        if ((chuc == 0) && (donvi != 0)) KetQua += " linh ";
    }
    if ((chuc != 0) && (chuc != 1)) {
        KetQua += ChuSo[chuc] + " mươi";
        if ((chuc == 0) && (donvi != 0)) KetQua = KetQua + " linh ";
    }
    if (chuc == 1) KetQua += " mười ";
    switch (donvi) {
        case 1:
            if ((chuc != 0) && (chuc != 1)) {
                KetQua += " mốt ";
            }
            else {
                KetQua += ChuSo[donvi];
            }
            break;
        case 5:
            if (chuc == 0) {
                KetQua += ChuSo[donvi];
            }
            else {
                KetQua += " lăm ";
            }
            break;
        default:
            if (donvi != 0) {
                KetQua += ChuSo[donvi];
            }
            break;
    }
    return KetQua;
}

//2. Hàm đọc số thành chữ (Sử dụng hàm đọc số có ba chữ số)

function DocTienBangChu(SoTien) {
    var lan = 0;
    var i = 0;
    var so = 0;
    var KetQua = "";
    var tmp = "";
    var ViTri = new Array();
    if (SoTien < 0) return "Số tiền âm !";
    if (SoTien == 0) return "Không đồng !";
    if (SoTien > 0) {
        so = SoTien;
    }
    else {
        so = -SoTien;
    }
    if (SoTien > 8999999999999999) {
        //SoTien = 0;
        return "Số quá lớn!";
    }
    ViTri[5] = Math.floor(so / 1000000000000000);
    if (isNaN(ViTri[5]))
        ViTri[5] = "0";
    so = so - parseFloat(ViTri[5].toString()) * 1000000000000000;
    ViTri[4] = Math.floor(so / 1000000000000);
    if (isNaN(ViTri[4]))
        ViTri[4] = "0";
    so = so - parseFloat(ViTri[4].toString()) * 1000000000000;
    ViTri[3] = Math.floor(so / 1000000000);
    if (isNaN(ViTri[3]))
        ViTri[3] = "0";
    so = so - parseFloat(ViTri[3].toString()) * 1000000000;
    ViTri[2] = parseInt(so / 1000000);
    if (isNaN(ViTri[2]))
        ViTri[2] = "0";
    ViTri[1] = parseInt((so % 1000000) / 1000);
    if (isNaN(ViTri[1]))
        ViTri[1] = "0";
    ViTri[0] = parseInt(so % 1000);
    if (isNaN(ViTri[0]))
        ViTri[0] = "0";
    if (ViTri[5] > 0) {
        lan = 5;
    }
    else if (ViTri[4] > 0) {
        lan = 4;
    }
    else if (ViTri[3] > 0) {
        lan = 3;
    }
    else if (ViTri[2] > 0) {
        lan = 2;
    }
    else if (ViTri[1] > 0) {
        lan = 1;
    }
    else {
        lan = 0;
    }
    for (i = lan; i >= 0; i--) {
        tmp = DocSo3ChuSo(ViTri[i]);
        KetQua += tmp;
        if (ViTri[i] > 0) KetQua += Tien[i];
        if ((i > 0) && (tmp.length > 0)) KetQua += ',';//&& (!string.IsNullOrEmpty(tmp))
    }
    if (KetQua.substring(KetQua.length - 1) == ',') {
        KetQua = KetQua.substring(0, KetQua.length - 1);
    }
    KetQua = KetQua.substring(1, 2).toUpperCase() + KetQua.substring(2);
    return KetQua + " đồng.";//.substring(0, 1);//.toUpperCase();// + KetQua.substring(1);
}
//PadLeft PadRight 
function PadLeftRight(str, pad, location) {
    if (typeof str === 'undefined')
        return pad;
    if (location == 'L') {
        return (pad + str).slice(-pad.length);
    }
    else if (location == 'R') {
        return (str + pad).substring(0, pad.length);
    }
    else {
        return str;
    }
}

//print PDF
function printDocument(elem) {
    var mywindow = window.open('', 'PRINT');
    mywindow.document.open();
    mywindow.document.write('</head><body >');
    mywindow.document.write(document.getElementById(elem).innerHTML);
    mywindow.document.write('</body></html>');

    mywindow.document.close(); // necessary for IE >= 10
    mywindow.focus(); // necessary for IE >= 10*/

    mywindow.print();
    mywindow.close();

    return true;
}