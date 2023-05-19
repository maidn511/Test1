var xxx;
$(document).ready(function () {
    var scope = angular.element($("#table")).scope();
    scope.contractOptions = [];
    scope.rateTypes = [];
    scope.History = [];
    scope.CustomerList = [];
    getDataSelect();
    scope.Title = "Số ngày vay";
    scope.Note = "(VD : 10 ngày đóng lãi 1 lần thì điền số 10 )";
    scope.TitleLoan = "Vay thêm tiền";
    scope.DateTitleLoan = "Ngày trả trước gốc";
    scope.MoneyTitleLoan = "Số tiền gốc trả trước";
    scope.WithDrawal = {};
    scope.DrawalData = [];
    scope.TotalDebit = 0;
    scope.TotalHaving = 0;
    scope.showPayInterest = false;
    scope.DataPayInterest = {};
    scope.TotalCapitalPayDay = 0;
    scope.ListCapitalPayDays = [];
    scope.ExtentionContract = {};
    scope.DataCloseContract = {};

    scope.DataCloseContract.ToDateString = moment().format(_dateFormat.toUpperCase());
    scope.getTotal = function getTotal() {
        var total = 0;
        if (scope.ListOfItem != undefined && scope.ListOfItem != null && scope.ListOfItem.length > 0) {
            total = scope.ListOfItem.map(s => Number(s.MoneyNumber)).reduce((a, b) => a + b);
        }
        return total;
    }
    scope.ShowHistory = function (id) {
        if ($("#ls-tab").hasClass("active")) return false;
        Post_AjaxCaller(urlLoadHistoryCapital, { contractId: id, historyType: historyType },
            function (data, status, headers, config) {
                scope.History = data;
                if (data != undefined && data != null && data.length > 0) {
                    scope.TotalDebit = data.map(s => Number(s.DebitMoney)).reduce((a, b) => a + b);
                    scope.TotalHaving = data.map(s => Number(s.HavingMoney)).reduce((a, b) => a + b);
                }
            }, function (data, status, headers, config) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            });
    }
    function getDataSelect() {
        Post_AjaxCaller(urlLoadSelect, { intType: 2 },
            function (data, status, headers, config) {
                data.push({ Id: "-1", Text: "Tất cả hợp đồng chưa kết thúc", OrderIndex: -1 })
                scope.contractOptions = data;
                scope.Parameters.StatusContractId = "-1";
                scope.$apply();
            }, function (data, status, headers, config) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            }
        )

        Post_AjaxCaller(urlLoadSelect, { intType: 3 },
            function (data, status, headers, config) {
                scope.rateTypes = data;
                scope.Modal.Data.Method = scope.rateTypes[0].Id;
                scope.$apply();
            }, function (data, status, headers, config) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            }
        )

        Post_AjaxCaller(urlLoadCustomerList, {},
            function (data, status, headers, config) {
                scope.CustomerList = data;
                scope.$apply();
            }, function (data, status, headers, config) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            }
        )

    }
    scope.Parameters.StatusContractId = "-1";


    scope.Detail = {};
    scope.DetailCapital = function (id) {
        scope.LoadDataCapitalLoan(id);
        scope.LoadCapitalDetail(id);

        $("#ttl-tab").click();
        $("#detail").modal({ backdrop: 'static', keyboard: false });
    }
    scope.LoadCapitalDetail = function (id) {
        Post_AjaxCaller(urlLoadCapitalDetail, { id: id },
            function (data, status, headers, config) {
                scope.DataPayInterest.MoneyOrther = 0;
                scope.Detail = data;
                xxx = data;
                scope.BindData();
            }, function (data, status, headers, config) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            });
    }

    scope.loadRadio = function () {

        switch (scope.Modal.Data.Method) {
            case 15:
                scope.Title = "Số ngày vay";
                scope.Modal.Data.RateType = 0;
                scope.Note = "(VD : 10 ngày đóng lãi 1 lần thì điền số 10)";
                break;
            case 16:
                scope.Modal.Data.RateType = 2;
                scope.Title = "Số tháng vay";
                scope.Note = "(VD : 1 tháng đóng lãi 1 lần thì điền số 1)";
                break;
            case 17:
                scope.Modal.Data.RateType = 3;
                scope.Title = "Số tháng vay";
                scope.Note = "(VD : 1 tháng đóng lãi 1 lần thì điền số 1)";
                break;
            case 18:
                scope.Modal.Data.RateType = 4;
                scope.Title = "Số tuần vay";
                scope.Note = "(VD : 1 tuần đóng lãi 1 lần thì điền số 1)";
                break;
            case 19:
                scope.Modal.Data.RateType = 5;
                scope.Title = "Số tuần vay";
                scope.Note = "(VD : 1 tuần đóng lãi 1 lần thì điền số 1)";
                break;
            default:
                break;
        }
    }
    scope.BindData = function () {
        scope.DataPayInterest.ToDateString = moment(scope.Detail.LastLoanDate).format(_dateFormat.toUpperCase());
        //moment(scope.Detail.CapitalPayDayDate, _dateFormat.toUpperCase())
        //    .add(Number(scope.Detail.InterestRate - 1), "days").format(_dateFormat.toUpperCase());

        scope.DataPayInterest.NumberOfDays = moment(scope.DataPayInterest.ToDateString, _dateFormat.toUpperCase())
            .diff(moment(scope.Detail.CapitalPayDayDate, _dateFormat.toUpperCase()), 'days') + 1;

        scope.DataPayInterest.FromDateString = scope.Detail.CapitalPayDayDate;
        scope.DataPayInterest.CapitalId = scope.Detail.Id;
        scope.DateDiff();
        scope.DateDiffClose();
    }

    scope.LoadDataCapitalLoan = function (id) {
        Post_AjaxCaller(urlLoadDataCapitalLoan, { id: id },
            function (data, status, headers, config) {
                scope.DrawalData = data;
            }, function (data, status, headers, config) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            });
    }

    scope.DeleteLoan = function (item) {
        Post_AjaxCaller(urlDeleteCapitalLoan, { id: item.Id },
            function (data, status, headers, config) {
                ResponseMessage(data)
                if (data.Type == 2) {
                    scope.LoadDataCapitalLoan(scope.Detail.Id);
                    scope.LoadCapitalDetail(scope.Detail.Id);
                }
            }, function (data, status, headers, config) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            });
    }

    scope.MoneyLoan = 0;
    scope.Caculartor = function (fromDate, toDate) {
        var money = 0;
        var capitalDetail = {
            RateType: scope.Detail.RateType,
            TotalMoney: scope.Detail.TotalMoneySys,
            Method: scope.Detail.Method,
            InterestRate: scope.Detail.InterestRate,
        };
        var lstCapitalLoans = [];
        if (scope.Detail.ListCapitalLoan) {
            var lstCapitalLoans = [];
            for (var i = 0; i < scope.Detail.ListCapitalLoan.length; i++) {
                var item = scope.Detail.ListCapitalLoan[i];
                var objCapitalLoan = {
                    LoadDate: moment(item.LoadDate).format("MM/DD/YYYY"),
                    IsLoan: item.IsLoan,
                    MoneyNumber: item.MoneyNumber
                };
                lstCapitalLoans.push(objCapitalLoan);
            }
        }
        if (fromDate.indexOf("Invalid") == -1 && toDate.indexOf("Invalid") == -1 && scope.Detail.Method > 14) {
            Post_AjaxCaller(urlCalculatorMoneyPerDayPay,
                {
                    capitalDetail: capitalDetail,
                    lstCapitalLoans: lstCapitalLoans,
                    dtFromDate: fromDate,
                    dtToDate: toDate,
                },
                function (data) {
                    //scope.MoneyLoan = data;
                    //scope.DataPayInterest.MoneyNumber = data;
                    money = data;
                }, function (data) {
                    scope.message = 'Unexpected Error while loading data!!';
                    scope.result = "color-red";
                });
        }
        return money;
    }

    scope.$watch("DataPayInterest.ToDateString", function () {
        scope.DateDiff();
    })

    scope.$watch("DataCloseContract.ToDateString", function () {
        scope.DateDiffClose();
    })

    scope.NumberOfDay = 0;
    scope.DateDiff = function () {
        scope.NumberOfDay = moment(scope.DataPayInterest.ToDateString, _dateFormat.toUpperCase()).diff(moment(scope.Detail.CapitalPayDayDate, _dateFormat.toUpperCase()), 'days') + 1;
        var dtFromDate = moment(scope.Detail.CapitalPayDayDate, _dateFormat.toUpperCase()).format("MM/DD/YYYY");
        var dtToDate = moment(scope.DataPayInterest.ToDateString, _dateFormat.toUpperCase()).format("MM/DD/YYYY");
        scope.DataPayInterest.MoneyNumber = scope.Caculartor(dtFromDate, dtToDate);
        scope.TotalCapitalPayDay = parseInt((scope.DataPayInterest.MoneyOrther || 0)) + (scope.DataPayInterest.MoneyNumber || 0);
    }

    scope.DateDiffClose = function () {
        scope.DataCloseContract.NumberOfDayClose = moment(scope.DataCloseContract.ToDateString, _dateFormat.toUpperCase()).diff(moment(scope.Detail.CapitalPayDayDate, _dateFormat.toUpperCase()), 'days') + 1;
        var dtFromDate = moment(scope.Detail.FromDate, _dateFormat.toUpperCase()).format("MM/DD/YYYY");
        var dtToDate = moment(scope.DataCloseContract.ToDateString, _dateFormat.toUpperCase()).format("MM/DD/YYYY");
        scope.DataCloseContract.MoneyLoan = scope.Caculartor(dtFromDate, dtToDate) - (scope.Detail.MoneyPaid || 0);
        scope.DataCloseContract.TotalMoney = parseInt((scope.DataCloseContract.MoneyOrther || 0)) + scope.DataCloseContract.MoneyLoan;
    }

    scope.Test = function () {
        scope.TotalCapitalPayDay = parseInt((scope.DataPayInterest.MoneyOrther || 0)) + scope.DataPayInterest.MoneyNumber;
    }

    scope.TestClose = function () {
        scope.DataCloseContract.TotalMoney = parseInt((scope.DataCloseContract.MoneyOrther || 0)) + scope.DataCloseContract.MoneyLoan;
    }

    scope.CloseContract = function () {
        Post_AjaxCaller(urlCloseContract, { id: scope.Detail.Id, moneyNumber: scope.DataCloseContract.TotalMoney},
            function (data) {
                ResponseMessage(data)
                if (data.Type == 2) {
                    $("#detail").modal("hide");
                    scope.Search();
                }
            }, function (data) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            });
    }

    scope.AddWithDrawal = function () {

        Post_AjaxCaller(urlAddWithDrawCapital, { objCapitalLoan: scope.WithDrawal },
            function (data) {
                ResponseMessage(data)

                if (data.Type == 2) {
                    scope.WithDrawal = { IsLoan: scope.WithDrawal.IsLoan };
                    scope.LoadDataCapitalLoan(scope.Detail.Id);
                    scope.LoadCapitalDetail(scope.Detail.Id);
                    scope.Search();
                }
            }, function (data) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            });
    }

    scope.ChangeType = function (isLoan) {
        scope.WithDrawal = {};
        scope.WithDrawal.IsLoan = isLoan;
        scope.WithDrawal.CapitalId = scope.Detail.Id;
        if (isLoan) {
            scope.TitleLoan = "Vay thêm tiền";
            scope.DateTitleLoan = "Ngày vay thêm gốc";
            scope.MoneyTitleLoan = "Số tiền vay thêm";
        } else {
            scope.TitleLoan = "Trả gốc";
            scope.DateTitleLoan = "Ngày trả trước gốc";
            scope.MoneyTitleLoan = "Số tiền gốc trả trước";
        }
    }

    scope.AddPayDays = function () {

        Post_AjaxCaller(urlAddCapitalPayDays, { objCapitalPayDays: scope.DataPayInterest },
            function (data) {
                ResponseMessage(data)
                scope.LoadCapitalDetail(scope.Detail.Id);
            }, function (data) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            });
    };

    scope.Paid = function (item, index) {
        var isSubmit = true;
        if (index === scope.Detail.ListCapitalPayDay.length - 1) isSubmit = true
        else if (scope.Detail.ListCapitalPayDay[index + 1].IsPaid) {
            item.IsPaid = !item.IsPaid
            notification(notifiType.error, "Bạn phải hủy các lần đóng tiền sau đã");
            isSubmit = false;
        };
        if (isSubmit) {
            var obj = {
                FromDateString: moment(item.FromDate).format(_dateFormat.toUpperCase()),
                ToDateString: moment(item.ToDate).format(_dateFormat.toUpperCase()),
                Id: item.Id,
                MoneyNumber: item.MoneyNumber,
                IsPaid: item.IsPaid,
                CapitalId: scope.Detail.Id
            };
            Post_AjaxCaller(urlAddCapitalPayDays, { objCapitalPayDays: obj },
                function (data, status, headers, config) {
                    ResponseMessage(data);
                    scope.LoadCapitalDetail(scope.Detail.Id);
                }, function (data, status, headers, config) {
                    scope.message = 'Unexpected Error while loading data!!';
                    scope.result = "color-red";
                });
        }
    };

    scope.AddWithExtensionHistory = function () {
        scope.ExtentionContract.ContractID = scope.Detail.Id;
        scope.ExtentionContract.DocumentType = docType;
        Post_AjaxCaller(urlAddExtentionContract, { extentionContract: scope.ExtentionContract },
            function (data, status, headers, config) {
                ResponseMessage(data);
                if (data.Type == 2) {
                    scope.LoadCapitalDetail(scope.Detail.Id);
                }
            }, function (data, status, headers, config) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            });
    };



    scope.Search();
    scope.ConvertJsonToDatetime = function (date) {
        return new Date(parseInt(date.substr(6))).toLocaleDateString();
    }

    scope.$apply();
});