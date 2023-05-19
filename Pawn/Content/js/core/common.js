$(function () {
    InitControl.Checkbox();
    InitControl.Main();
});

var InitControl = {
    Main: function () {
        // checkbox picker
        $(".skin-flat input:visible").iCheck({
            checkboxClass: "icheckbox_flat-green",
            radioClass: "iradio_flat-green"
        });

        // type number
        $("input[data-type=number]").on("input propertychange paste", function () {
            $(this).val($(this).val().replace(/[^0-9]/, ""));
        });

        //enter key input
        $("input[data-button]").keyup(function (e) {
            if (e.keyCode === 13) {
                $($(this).attr("data-button")).click();
            }
        });

        //Date Picker
        if ($.fn.datetimepicker)
            $(".hasDatepicker").datetimepicker({
                format: _dateFormat.toUpperCase()
            });

        // select picker
        $("[data-provide=selectpicker]").select2({
            allowClear: true
        });

        $(".select2-selection").addClass("border-primary");
    },
    Checkbox: function () {
        $("input[data-check]").change(function () {
            $($(this).attr("data-check")).val($(this).is(":checked"));
        });
    },
    Pagination: function (_this, ajaxCall) {
        var divPaging = $(_this).attr("data-paging");
        var totalRows = $(_this).attr("data-totalrows");
        var intPageSize = $(_this).attr("data-pagesize") || pagesize;

        $(divPaging).pagination({
            items: totalRows,
            itemsOnPage: intPageSize,
            displayedPages: 3,
            prevText: "«",
            nextText: "»",
            cssStyle: "light-theme",
            onPageClick: function (pageNumber) {
                ajaxCall(pageNumber);
            }
        });
        if (parseInt(totalRows) <= parseInt(intPageSize)) {
            $(divPaging).hide();
        } else {
            $(divPaging).show();
        }
    },
    SetChecked: function (chkParent, chkChildren) {
        $(document).on("change", chkParent, function () {
            $(chkChildren).prop("checked", $(this).is(":checked"));
        });
        $(document).on("change", chkChildren, function () {
            $(chkParent).prop("checked", $(chkChildren).length === $(chkChildren).filter(function () { return $(this).is(":checked") }).length);
        });
    },
    UploadFile: function (element, success) {
        element.fileupload({
            dataType: "json",
            add: function (e, data) {
                var numFile = data.originalFiles.length;
                var numFileTrue = data.originalFiles.filter((v, i) => (/\.(gif|jpg|jpeg|png)$/i).test(v.name)).length;
                if (numFile !== numFileTrue) {
                    notification(notifiType.warning, "Please upload file png, jpg, jpeg");
                    return false;
                }
                if (data.originalFiles.filter((v, i) => v.size > 10 * 1024 * 1014).length > 0) {
                    notification(notifiType.warning, "Please upload file <= 10MB");
                    return false;
                }
                data.submit();
            },
            progress: function (e, data) {

            },
            done: function (e, data) {
                success(data);
            },
            fail: function (e, data) {
                notification(notifiType.error, "Upload file lỗi. Xin vui lòng thử lại sau");
            }
        });
    }
}
var Action = {
    ConfirmDelete: function (title, type, fn) {

    }
}


function isEmail(email) {
    var regex = /^([a-zA-Z0-9_.+-])+\@(([a-zA-Z0-9-])+\.)+([a-zA-Z0-9]{2,4})+$/;
    return regex.test(email);
}


function Validate(idDiv) {
    $((idDiv !== undefined ? "#" + idDiv + " " : "") + "[data-required=true]").each(function () {
        var id = $(this).attr("id");
        var error = $(this).attr("data-key");

        // check input null
        if ($("#" + id).val() === "") {
            addError(id, resources[error].replace("{field_name}", $("#" + id).attr("placeholder") || ""));
        } else {
            // Check độ dài tối thiểu của gtri
            if ($(this).attr("data-length") !== undefined && $(this).val().length < parseInt($(this).attr("data-length"))) {
                addError(id, resources.text_length.replace("{field_name}", $(this).attr("placeholder") || "").replace("{length}", $(this).attr("data-length")));
            }

            if ($(this).attr("data-type") !== undefined) {
                var type = $(this).attr("data-type");
                // Validate password
                if (type === "password") {
                    if ($(this).val().length < 6)
                        addError(id, resources.password_length);
                    else {
                        // Check confirm password
                        if ($(this).attr("data-compare") !== undefined) {
                            if ($("#" + $(this).attr("data-compare")).val() !== "" && $(this).val() !== $("#" + $(this).attr("data-compare")).val()) {
                                addError($(this).attr("data-compare"), resources.password_confirm);
                            }
                        }
                    }
                }
                // Check định dạng email
                if (type === "email" && !isEmail($(this).val())) {
                    addError(id, resources.email_format);
                }
            }
            if ($(this).attr("data-regex") !== undefined) {
                var regex = new RegExp($(this).attr("data-regex"));
                if (!regex.test($(this).val())) {
                    addError(id, resources.regex_user);
                }
            }
        }
    });

    $(".hasDatepicker").each(function () {
        if ($(this).val() !== "" && $(this).attr("data-compare") !== undefined) {
            if ($("#" + $(this).attr("data-compare")).val() !== "" && moment($(this).val(), "DD/MM/YYYY").toDate().getTime() >
                moment($("#" + $(this).attr("data-compare")).val(), "DD/MM/YYYY").toDate().getTime()) {
                addError($(this).attr("data-compare"), resources.date_compare);
            }
        }
    });

    return $((idDiv !== undefined ? "#" + idDiv + " " : "") + ".input-validation-error").length > 0;
}

function addError(id, error) {
    if ($("#" + id).prop("tagName") === "SELECT") {
        if ($("[aria-labelledby=select2-" + id + "-container]").next("div.help-block").length < 1) {
            $("[aria-labelledby=select2-" + id + "-container]").after("<div class='help-block' style='color:red'></div>");
        }
        $("[aria-labelledby=select2-" + id + "-container]").addClass("input-validation-error");
        $("[aria-labelledby=select2-" + id + "-container]").next().html('<ul role="alert"><li>' + error + "</li></ul>").show();
        $("[aria-labelledby=select2-" + id + "-container]").on("focus", function () {
            $(this).removeClass("input-validation-error");
            $(this).next().html("");
        });
    } else {
        if ($("#" + id).next("div.help-block").length < 1) {
            $("#" + id).after("<div class='help-block' style='color:red'></div>");
        }
        $("#" + id).addClass("input-validation-error");
        $("#" + id).next().html('<ul role="alert"><li>' + error + "</li></ul>").show();
        $("#" + id).on("focus", function () {
            $("#" + id).removeClass("input-validation-error");
            $("#" + id).next().html("");
        });
    }
}

var notifiType = {
    success: "success",
    error: "error",
    warning: "warning",
    info: "info"
}

function notification(type, text, title, timing) {
    /// <summary>Post notification</summary>  
    /// <param name="type" type="string">Loại notification. sử dụng: notifiType (ex: notifiType.success) </param>
    /// <param name="text" type="string">Nội dung thông báo </param>
    /// <param name="title" type="string">Tiêu đề thông báo. Mặc định là Lỗi</param>
    /// <param name="timing" type="number">Thời gian ẩn thông báo. Mặc định 3000 (3s)</param>
    /// <returns type="Number">The area.</returns> 
    toastr[type](text, title || "Lỗi", { "timeOut": timing || 3000 });
}



var dateFormat = function () {
    var token = /d{1,4}|m{1,4}|yy(?:yy)?|([HhMsTt])\1?|[LloSZ]|"[^"]*"|'[^']*'/g,
        timezone = /\b(?:[PMCEA][SDP]T|(?:Pacific|Mountain|Central|Eastern|Atlantic) (?:Standard|Daylight|Prevailing) Time|(?:GMT|UTC)(?:[-+]\d{4})?)\b/g,
        timezoneClip = /[^-+\dA-Z]/g,
        pad = function (val, len) {
            val = String(val);
            len = len || 2;
            while (val.length < len) val = "0" + val;
            return val;
        };

    // Regexes and supporting functions are cached through closure
    return function (date, mask, utc) {
        var dF = dateFormat;

        // You can't provide utc if you skip other args (use the "UTC:" mask prefix)
        if (arguments.length === 1 && Object.prototype.toString.call(date) === "[object String]" && !/\d/.test(date)) {
            mask = date;
            date = undefined;
        }

        // Passing date through Date applies Date.parse, if necessary
        date = date ? new Date(date) : new Date;
        if (isNaN(date)) throw SyntaxError("invalid date");

        mask = String(dF.masks[mask] || mask || dF.masks["default"]);

        // Allow setting the utc argument via the mask
        if (mask.slice(0, 4) === "UTC:") {
            mask = mask.slice(4);
            utc = true;
        }

        var _ = utc ? "getUTC" : "get",
            d = date[_ + "Date"](),
            D = date[_ + "Day"](),
            m = date[_ + "Month"](),
            y = date[_ + "FullYear"](),
            H = date[_ + "Hours"](),
            M = date[_ + "Minutes"](),
            s = date[_ + "Seconds"](),
            L = date[_ + "Milliseconds"](),
            o = utc ? 0 : date.getTimezoneOffset(),
            flags = {
                d: d,
                dd: pad(d),
                ddd: dF.i18n.dayNames[D],
                dddd: dF.i18n.dayNames[D + 7],
                m: m + 1,
                mm: pad(m + 1),
                mmm: dF.i18n.monthNames[m],
                mmmm: dF.i18n.monthNames[m + 12],
                yy: String(y).slice(2),
                yyyy: y,
                h: H % 12 || 12,
                hh: pad(H % 12 || 12),
                H: H,
                HH: pad(H),
                M: M,
                MM: pad(M),
                s: s,
                ss: pad(s),
                l: pad(L, 3),
                L: pad(L > 99 ? Math.round(L / 10) : L),
                t: H < 12 ? "a" : "p",
                tt: H < 12 ? "am" : "pm",
                T: H < 12 ? "A" : "P",
                TT: H < 12 ? "AM" : "PM",
                Z: utc ? "UTC" : (String(date).match(timezone) || [""]).pop().replace(timezoneClip, ""),
                o: (o > 0 ? "-" : "+") + pad(Math.floor(Math.abs(o) / 60) * 100 + Math.abs(o) % 60, 4),
                S: ["th", "st", "nd", "rd"][d % 10 > 3 ? 0 : (d % 100 - d % 10 !== 10) * d % 10]
            };

        return mask.replace(token, function ($0) {
            return $0 in flags ? flags[$0] : $0.slice(1, $0.length - 1);
        });
    };
}();
// Some common format strings
dateFormat.masks = {
    "default": "ddd mmm dd yyyy HH:MM:ss",
    shortDate: "m/d/yy",
    mediumDate: "mmm d, yyyy",
    longDate: "mmmm d, yyyy",
    fullDate: "dddd, mmmm d, yyyy",
    shortTime: "h:MM TT",
    mediumTime: "h:MM:ss TT",
    longTime: "h:MM:ss TT Z",
    isoDate: "yyyy-mm-dd",
    isoTime: "HH:MM:ss",
    isoDateTime: "yyyy-mm-dd'T'HH:MM:ss",
    isoUtcDateTime: "UTC:yyyy-mm-dd'T'HH:MM:ss'Z'"
};
// Internationalization strings
dateFormat.i18n = {
    dayNames: [
        "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat",
        "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"
    ],
    monthNames: [
        "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec",
        "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"
    ]
};
// For convenience...
Date.prototype.format = function (mask, utc) {
    return dateFormat(this, mask, utc);
};
//check valid date
Date.prototype.isValid = function () {
    // An invalid date object returns NaN for getTime() and NaN is the only
    // object not strictly equal to itself.
    return this.getTime() === this.getTime();
};
