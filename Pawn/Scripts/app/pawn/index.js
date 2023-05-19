$(document).ready(function () {
    var scope = angular.element($("#table")).scope();
    scope.Search();
    scope.InterestRateType = [];
    // #region Tuấn Em
    scope.showPayInterest = false;
    scope.CustomerList = [];
    getDataSelect();
    scope.Modal.Data = {};
    scope.CustomerData = {};
    scope.showDetailIdentityCard = false;
    scope.Parameters.documentStatus = 1;
    scope.getDataSelect = function () {
        var url = urlLoadSelectInterestRateType;
        Post_AjaxCaller(url, {}, function (data) {
            scope.InterestRateType = data;
            setDefault();
        }, function () { });
    };

    scope.ColumnSort = [
        { name: "Ngày tạo", value: "CreatedDate" },
        { name: "Code", value: "code" },
        { name: "Tên khách hàng", value: "CustomerName" },
        { name: "Ngày vay", value: "PawnDate" },
    ];
    scope.SortType = [
        { name: "Tăng dần", value: "asc" },
        { name: "Giảm dần", value: "desc" }
    ];

    function loadDataCustomer() {
        Post_AjaxCaller(urlLoadCustomerList,
            {},
            function (data, status, headers, config) {
                scope.CustomerList = data;
                scope.$apply();
            },
            function (data, status, headers, config) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            }
        );
    }

    function getDataSelect() {
        loadDataCustomer();
        Post_AjaxCaller(urlLoadSelectStaff, {}, function (data) {
            scope.staffList = data;
        }, function () { });

        Post_AjaxCaller(urlLoadSelectInterestRateType, {}, function (data) {
            scope.InterestRateType = data;
            scope.$apply();
        }, function () { });
    }

    

    function setDefault() {
        scope.Modal.Data = {};
        scope.Modal.Data.IsSystem = false;
        scope.Modal.Data.TotalMoney = 0;
        scope.Modal.Data.PawnDateNumber = 0;
        scope.Modal.Data.InterestRate = 0;
        scope.Modal.Data.InterestRateNumber = 0;
        scope.Modal.Data.IsCloseContract= false
        scope.Modal.Data.PawnDatePostString = moment().format(_dateFormat.toUpperCase());
        scope.Modal.Data.Code = scope.Maxcontract();
    }

    scope.ShowDetailByItem = function (item) {
        loadDataCustomer();
        Post_AjaxCaller(urlLoadDetailPawnContract,
            { idContract: item.Id },
            function (data, status, headers, config) {
                scope.Modal.Data = data;
                scope.Modal.Data.IsPaid = item.IsPaid;
                scope.Modal.Data.StoreIdConvert = scope.Modal.Data.StoreId;
                scope.CustomerData = scope.AdditionData.customers.find(x => x.Id === item.CustomerId);
            },
            function (data, status, headers, config) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            }
        );

    }

    scope.Maxcontract = function () {
        var value = "";
        Post_AjaxCaller(urlGetMaxContract, {}, function (data) {
            value = "VL-" + data;
        }, function () { });
        return value;
    }

    scope.Title = "Số ngày vay";
    scope.Note = "(VD : 10 ngày đóng lãi 1 lần thì điền số 10 )";
    scope.loadRadio = function () {
        switch (scope.Modal.Data.InterestRateType) {
            case 1:
                scope.Title = "Số ngày vay";
                scope.Modal.Data.InterestRateOption = 0;
                scope.Note = "(VD : 10 ngày đóng lãi 1 lần thì điền số 10)";
                break;
            case 2:
                scope.Modal.Data.InterestRateOption = 2;
                scope.Title = "Số tháng vay";
                scope.Note = "(VD : 1 tháng đóng lãi 1 lần thì điền số 1)";
                break;
            case 3:
                scope.Modal.Data.InterestRateOption = 3;
                scope.Title = "Số tháng vay";
                scope.Note = "(VD : 1 tháng đóng lãi 1 lần thì điền số 1)";
                break;
            case 4:
                scope.Modal.Data.InterestRateOption = 4;
                scope.Title = "Số tuần vay";
                scope.Note = "(VD : 1 tuần đóng lãi 1 lần thì điền số 1)";
                break;
            case 5:
                scope.Modal.Data.InterestRateOption = 5;
                scope.Title = "Số tuần vay";
                scope.Note = "(VD : 1 tuần đóng lãi 1 lần thì điền số 1)";
                break;
            default:
                break;
        }
    }

    scope.CloseContractClick = function (item) {
        swal({
            title: "",
            text: "Bạn có chắc chắn muốn đóng hợp đồng",
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
                        scope.CloseContract();
                    }, 100);


                });
            }
        }).then(function () {
        });
    }
    // #endregion


    // #region A Nam
    scope.LoadHistory = function (contractId) {
        scope.LoadHistoryRemind(contractId);
        scope.LoadHistoryAction(contractId);
    };
    scope.LoadHistoryRemind = function (contractId) {
        var url = $('#table').attr("data-history-remind-url");
        Post_AjaxCaller(url, { contractId: contractId },
            function (response) {
                scope.HistoryRemind = response.historyRemind;
            }, function () { });
    };
    scope.LoadHistoryAction = function (contractId) {
        if ($("#history-tab").hasClass("active")) return false;
        var url = $('#table').attr("data-history-action-url");
        Post_AjaxCaller(url, { contractId: contractId },
            function (response) {
                scope.HistoryAction = response.historyAction;
                if (response.historyAction != undefined && response.historyAction != null && response.historyAction.length > 0) {
                    scope.TotalDebit = response.historyAction.map(s => Number(s.DebitMoney)).reduce((a, b) => a + b);
                    scope.TotalHaving = response.historyAction.map(s => Number(s.HavingMoney)).reduce((a, b) => a + b);
                }
            }, function () { });
    };

    scope.AddHistoryRemind = function (content, contractId) {
        if (scope.pawnData.IsCloseContract) {
            notification(notifiType.error, "Hợp đồng này đã đóng <br /> Bạn phải mở lại hợp đồng để có thể cập nhập hợp đồng");
            return false;
        }
        var url = $('#table').attr("data-add-history-remind-url");
        Post_AjaxCaller(url, { content: content, contractId: contractId },
            function (response) {
                if (scope.remindForm.$valid) {
                    ResponseMessage(response);
                    scope.contentRemind = "";
                    scope.LoadHistoryRemind(contractId);
                    scope.remindForm.$setUntouched();
                    scope.remindForm.$setPristine();
                }
            }, function () { });
    };

    scope.LoadPawnDetail = function (item) {
       
        scope.LoadDetailById(item.Id);
        scope.DongLaiTab(item.Id);
        scope.LoadDataCapitalLoan(item.Id);
        scope.LoadChungTu(item.Id);
        $("#backdrop").modal({ backdrop: 'static', keyboard: false });

    }

    scope.LoadDetailById = function (contractId) {
        var url = $('#table').attr('data-detail-url');
        Post_AjaxCaller(url, { contractId: contractId },
            function (data) {
                scope.pawnData = data;
                console.log(data);
            }, function () { });
    }

    scope.DongLaiTab = function (id) {

        var url = $('#table').attr('data-get-donglai-url');
        Post_AjaxCaller(url, { id: id },
            function (response) {
                scope.DongLaiData = response;
                scope.BindingPaymentChange();
            }, function () { });
    }

    scope.PaymentChange = function (item, index) {
        if (scope.pawnData.IsCloseContract) {
            notification(notifiType.error, "Hợp đồng này đã đóng <br /> Bạn phải mở lại hợp đồng để có thể cập nhập hợp đồng");
            return false;
        }
        var JsonCopy = JSON.parse(JSON.stringify(scope.DongLaiData));
        JsonCopy.reverse();
        if (JsonCopy[0].Id != item.Id) {
            if (scope.DongLaiData[index + 1].IsPaid) {
                item.IsPaid = !item.IsPaid;
                notification(notifiType.error, "Bạn phải hủy các lần đóng tiền sau đã");
                return;
            };
        };
        item.FromDate = moment(item.FromDate).format("MM/DD/YYYY");
        item.ToDate = moment(item.ToDate).format("MM/DD/YYYY");
        item.LoanDate = moment().format("MM/DD/YYYY");
        item.CreatedDate = moment(item.CreatedDate).format("MM/DD/YYYY");
        var url = $('#table').attr('data-donglai-url');
        Post_AjaxCaller(url, { header: scope.pawnData, item: item },
            function (data) {
                ResponseMessage(data);
                // Thêm hàm load detail vào đây
                scope.LoadDetailById(item.ContractId);
                scope.DongLaiTab(item.ContractId);
                scope.LoadDataCapitalLoan(item.ContractId);
                // them ham load lai detail

            }, function (data) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            });

    }
    scope.PaymentChangeCustoms = function () {
        if (scope.DongLaiForm.$valid) {
            var url = $('#table').attr('data-donglaituybien-url');
            var JsonCopy = JSON.parse(JSON.stringify(scope.customs));
            JsonCopy.ToDate = moment(JsonCopy.ToDate, _dateFormat.toUpperCase()).format("MM/DD/YYYY");
            JsonCopy.FromDate = moment(JsonCopy.FromDate).format("MM/DD/YYYY");
            JsonCopy.IsPaid = true;
            Post_AjaxCaller(url, {
                header: scope.pawnData, item: JsonCopy
            },
                function (data) {
                    ResponseMessage(data);
                    scope.LoadDetailById(JsonCopy.ContractId);
                    scope.DongLaiTab(JsonCopy.ContractId);
                    scope.LoadDataCapitalLoan(JsonCopy.ContractId);
                    // them ham load lai detail

                }, function (data) {
                    scope.message = 'Unexpected Error while loading data!!';
                    scope.result = "color-red";
                });
        }
    }

    scope.BindingPaymentChange = function () {
        var data = $.grep(scope.DongLaiData, function (e) {
            return e.IsCurrent == true;
        })[0];
        if (data == undefined) return;
        var toD = new Date(parseInt(data.FromDate.substr(6)));
        scope.customs = {
            FromDate: data.FromDate,
            TotalDay: 1,
            ContractId: data.ContractId,
            ToDate: moment(toD.setDate(toD.getDate() + 1)).format(_dateFormat.toUpperCase()),
            OtherMoney: data.OtherMoney,
            InterestMoney: data.InterestMoney / data.TotalDay, //400k/ngay * totalday
            ContinueDate: moment(toD.setDate(toD.getDate() + 1)).add(1, 'days').format(_dateFormat.toUpperCase())
        };
        if (scope.pawnData.InterestRateType == 2) // thang dinh ky
        {
            scope.customs.InterestMoney = (scope.pawnData.TotalMoney + scope.pawnData.TienVayThem) * scope.pawnData.InterestRate / 100 / 30;

        }
    }

    scope.$watch("customs.ToDate",
        function () {
            if (scope.customs === null || scope.customs == undefined) return;
            var first = moment(scope.customs.FromDate);
            var second = moment(scope.customs.ToDate, "DD-MM-YYYY");
            scope.customs.TotalDay = second.diff(first, 'days') + 1;
            scope.customs.ContinueDate = moment(scope.customs.ToDate, "DD-MM-YYYY").add(1, 'days')
                .format(_dateFormat.toUpperCase());
        });

    scope.changeToDay = function (val) {
        scope.customs.TotalDay = val - 1;
        scope.customs.ToDate = moment(scope.customs.FromDate).add(val - 1, 'days').format(_dateFormat.toUpperCase());
    }


    // #region Đóng hợp đồng
    scope.LoadCloseContract = function () {
        if ($("#dhd-tab").hasClass("active")) return false;
        var numDay = moment().diff(moment(scope.pawnData.NgayPhaiDongLai), 'days') + 1;
        scope.DataCloseContract = {
            ToDateString: moment().format(_dateFormat.toUpperCase()),
            MoneyLoan: scope.pawnData.TienLaiMotNgay * numDay,
            MoneyOrther: 0,
            NumberOfDayClose: numDay
        };
    }
    scope.CloseContract = function () {
        if (scope.pawnData.IsCloseContract) {
            notification(notifiType.error, "Hợp đồng này đã đóng <br /> Bạn phải mở lại hợp đồng để có thể cập nhập hợp đồng");
            return false;
        }
        var url = $('#table').attr('data-donghopdong-url');
        var total = scope.DataCloseContract.MoneyLoan + parseFloat(scope.DataCloseContract.MoneyOrther) + scope.pawnData.TotalMoney + scope.pawnData.TienVayThem;
        Post_AjaxCaller(url, { contractId: scope.pawnData.Id, totalMoney: total },
            function (response) {
                ResponseMessage(response);
            }, function () { });
    }
    scope.InHopDongVay = function () {
        setTimeout(function () { printDocument('InHopDongVay'); }, 1000);
    }
    scope.InPhieuThu = function (item) {
        scope.PayData = item;
        scope.nextToDate = moment(item.ToDate).add(item.TotalDay - 1, 'days').format('DD-MM-YYYY');
        setTimeout(function () { printDocument('InPhieuThu'); }, 1000);
    }

    // #endregion

    // #endregion

    // #region Tuấn em

    // #region Trả bớt gốc + vay thêm
    scope.DrawalData = [];
    scope.WithDrawal = {};
    scope.TitleLoan = "Trả gốc";
    scope.DateTitleLoan = "Ngày trả trước gốc";
    scope.MoneyTitleLoan = "Số tiền gốc trả trước";

    scope.ChangeTypeLoan = function (isLoan) {
        scope.WithDrawal = {};
        scope.WithDrawal.LoadDateString = moment().format(_dateFormat.toUpperCase());
        scope.WithDrawal.IsLoan = isLoan;
        scope.WithDrawal.CapitalId = scope.pawnData.Id;
        scope.WithDrawal.MoneyNumber = 0;
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

    scope.LoadDataCapitalLoan = function (id) {
        Post_AjaxCaller(urlLoadDataCapitalLoan, { id: id },
            function (data, status, headers, config) {
                scope.DrawalData = data;
            }, function (data, status, headers, config) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            });
    }

    scope.AddWithDrawal = function () {
        if (scope.pawnData.IsCloseContract) {
            notification(notifiType.error,
                "Hợp đồng này đã đóng <br /> Bạn phải mở lại hợp đồng để có thể cập nhập hợp đồng");
            return false;
        }
        var data = $.grep(scope.DongLaiData,
            function (e) {
                return e.IsCurrent == true;
            })[0];
        if (data != null || data != undefined) {
            if (scope.WithDrawal.MoneyNumber < 1) {
                notification(notifiType.warning, "Cảnh báo!", "Số tiền phải lớn hơn 0!");
                return false;
            }
            if ((moment(data.FromDate).diff(moment(scope.WithDrawal.LoadDateString, "DD-MM-YYYY"), 'minutes')) > 0) {
                var me = (scope.WithDrawal.IsLoan) ? "[Vay Thêm]" : "[Trả góc]";
                notification(notifiType.warning,
                    "Cảnh báo!",
                    "Ngày " +
                    me +
                    " phải lớn hơn hoặc bằng ngày đóng lãi cuối cùng là ngày " +
                    moment(data.FromDate).format("DD-MM-YYYY") +
                    "!");
                return false;
            }
            if ((moment(scope.pawnData.ToDate).diff(moment(scope.WithDrawal.LoadDateString, "DD-MM-YYYY"), 'minutes')) <
                0) {
                var me = (scope.WithDrawal.IsLoan) ? "[Vay Thêm]" : "[Trả góc]";
                notification(notifiType.warning,
                    "Cảnh báo!",
                    "Ngày " +
                    me +
                    " phải bé hơn hoặc bằng ngày hết hạn là ngày " +
                    moment(scope.pawnData.ToDate).format("DD-MM-YYYY") +
                    "!");
                return false;
            }
            Post_AjaxCaller(urlAddWithDrawCapital,
                { header: scope.pawnData, objCapitalLoan: scope.WithDrawal },
                function (data) {
                    ResponseMessage(data);
                    if (data.Type == 2) {
                        scope.LoadDetailById(scope.pawnData.Id);
                        scope.DongLaiTab(scope.pawnData.Id);
                        scope.LoadDataCapitalLoan(scope.pawnData.Id);
                        scope.WithDrawal = {};
                        scope.WithDrawal.LoadDateString = moment().format(_dateFormat.toUpperCase());
                        scope.WithDrawal.MoneyNumber = 0;
                        scope.WithDrawal.Note = "";
                    }
                },
                function (data) {
                    scope.message = 'Unexpected Error while loading data!!';
                    scope.result = "color-red";
                });
        } else {
            notification(notifiType.warning,
                "Cảnh báo!",
                " Không thực hiện được thao tác do đã đóng đủ kỳ lãi!");
            return false;
        }
        return false;
    };

    scope.DeleteLoan = function (item) {
        Post_AjaxCaller(urlDeleteCapitalLoan, { header: scope.pawnData, id: item.Id },
            function (data, status, headers, config) {
                ResponseMessage(data);
                if (data.Type == 2) {
                    scope.LoadDetailById(scope.pawnData.Id);
                    scope.DongLaiTab(scope.pawnData.Id);
                    scope.LoadDataCapitalLoan(scope.pawnData.Id);
                }
            }, function (data, status, headers, config) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            });
    }
    // #endregion


    // #region Gia hạn
    scope.LoadGiaHan = function () {
        scope.ExtentionContract = {
            AddTime: 1,
            DocumentType: docType,
            ToDateContract: moment(scope.pawnData.ToDate).add(1, 'days').format("MM/DD/YYYY"),
            Note: '',
            ContractID: scope.pawnData.Id
        };
    }
    scope.AddWithExtensionHistory = function () {
        if (scope.pawnData.IsCloseContract) {
            notification(notifiType.error, "Hợp đồng này đã đóng <br /> Bạn phải mở lại hợp đồng để có thể cập nhập hợp đồng");
            return false;
        }
        var url = $('#table').attr('data-giahanthem-url');
        if (scope.ExtentionContract.AddTime < 1) {
            notification(notifiType.warning, "Cảnh báo!", "Số " + scope.pawnData.InterestRateTypeString + " phải lớn hơn 0!");
            return false;
        }
        Post_AjaxCaller(url, { model: scope.pawnData, extentionContract: scope.ExtentionContract },
            function (data, status, headers, config) {
                ResponseMessage(data);
                if (data.Type == 2) {
                    scope.LoadPawnDetail(scope.pawnData);
                }
            }, function (data, status, headers, config) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            });
    };

    // #endregion

    // #region KH Nợ / Trả nợ
    scope.AddDebt = function (isDebt) {
        if (scope.pawnData.IsCloseContract) {
            notification(notifiType.error, "Hợp đồng này đã đóng <br /> Bạn phải mở lại hợp đồng để có thể cập nhập hợp đồng");
            return false;
        }
        var obj = {
            MoneyNumber: isDebt ? scope.MoneyDebt : scope.MoneyHaving,
            IsDebt: isDebt,
            ContractId: scope.pawnData.Id,
            DocumentType: docType
        };
        if (obj.IsDebt && obj.MoneyNumber > ((scope.pawnData.TotalPay || 0) + (scope.pawnData.TotalHaving || 0))) {
            notification(notifiType.error, "Thất bại!", "Tiền nợ phải nhỏ hơn hoặc bằng số tiền đã thanh toán!");
            return false;
        }

        Post_AjaxCaller(urlAddDebt, obj,
            function (data, status, headers, config) {
                ResponseMessage(data);
                // Thêm hàm load detail vào đây
            }, function (data, status, headers, config) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            });
    }
    // #endregion

    // #region Upload Chung tu
    scope.DataChungTu = [];

    scope.LoadChungTu = function (id) {
        Post_AjaxCaller(urlLoadChungtu, { contractId: id, documentType: docType },
            function (response) {
                scope.DataChungTu = response;
            }, function () { });
    }

    scope.AddFile = function (obj) {
        obj.ContractId = scope.pawnData.Id;
        obj.ContractType = docType;
        Post_AjaxCaller(urlAddFile, obj,
            function (data, status, headers, config) {
                scope.DataChungTu.push(data);
                scope.$apply();
            }, function (data, status, headers, config) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            });
    }

    scope.DeleteImg = function (item) {
        Post_AjaxCaller(urlDeleteFile, { idFile: item.Id },
            function (data, status, headers, config) {
                ResponseMessage(data);
                if (data.Type == 2) {
                    scope.DataChungTu = $.grep(scope.DataChungTu, function (e) {
                        return e.Id != item.Id;
                    });
                }
            }, function (data, status, headers, config) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            });
    }

    InitControl.UploadFile($("#fileupload"),
        function (data) {
            if (data.result.Error == false)
                scope.AddFile(data.result.objFile)
        });

    $(".uploadfile").click(function () {
        if (scope.pawnData.IsCloseContract) {
            notification(notifiType.error, "Hợp đồng này đã đóng <br /> Bạn phải mở lại hợp đồng để có thể cập nhập hợp đồng");
            return false;
        }
        $("#fileupload").click();
    });

    // #endregion

    // #region Hẹn giờ

    scope.DateTimer = [];
    scope.AlarmDate = {
        DateString: moment().format(_dateFormat.toUpperCase())
    };

    scope.CallTabTimer = function () {
        if ($("#hg-tab").hasClass("active")) return false;
        scope.LoadTimer();
    }

    scope.LoadTimer = function () {
        Post_AjaxCaller(urlLoadTimer, { contractId: scope.pawnData.Id, docType: docType },
            function (response) {
                scope.DateTimer = response;
            }, function () { });
    };

    scope.AddAlarmDate = function (status) {
        if (scope.pawnData.IsCloseContract) {
            notification(notifiType.error, "Hợp đồng này đã đóng <br /> Bạn phải mở lại hợp đồng để có thể cập nhập hợp đồng");
            return false;
        }
        scope.AlarmDate.ContractId = scope.pawnData.Id;
        scope.AlarmDate.DocumentType = docType;
        scope.AlarmDate.Status = status;
        scope.AlarmDate.TimerDate = moment(scope.AlarmDate.DateString, _dateFormat.toUpperCase()).format("MM/DD/YYYY");

        Post_AjaxCaller(urlAddTimer, scope.AlarmDate,
            function (data, status, headers, config) {
                ResponseMessage(data);
                if (data.Type === 2) {
                    scope.LoadTimer();
                    scope.AlarmDate = {
                        DateString: moment().format(_dateFormat.toUpperCase())
                    };
                }
            }, function (data, status, headers, config) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            });
    };

    scope.OpenTimer = function (item) {
        scope.LoadPawnDetail(item);
        setTimeout(function () { $("#hg-tab").click(); }, 500);
    };

    scope.DeletePawn = function (item) {
        console.log(item);
        window.ConfirmPostAjax('Bạn có chắc chắn muốn ' + (item.IsDeleted === false ? "hủy" : "khôi phục") + ' hợp đồng <b>' + item.Code + '</b> của khách hàng <b>' + item.CustomerName + '</b> không ?',
            "Xác nhận thông tin",
            1,
            null,
            urlDeletePawn,
            { idPawn: item.Id },
            function (result) {
                ResponseMessage(result);
                if (result.Type === 2) {
                    scope.Search();
                }
            });
    }

    // #region  Chuyển cửa hàng

    scope.StoreList = [];
    getDataSelectStore();
    function getDataSelectStore() {
        Post_AjaxCaller(urlLoadSelectStoreWithoutCurrent, {},
            function (data, status, headers, config) {
                scope.StoreList = data;
            }, function (data, status, headers, config) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            }
        );
    }

    scope.ConvertStore = function (id) {
        Post_AjaxCaller(urlConvertStore, { vlId: id, storeId: scope.Modal.Data.StoreIdConvert },
            function (data, status, headers, config) {
                ResponseMessage(data);
                if (data.Type === 2) {
                    scope.Search();
                }
            }, function (data, status, headers, config) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            }
        );
    };
    // #endregion

    // #region Nợ xấu
    scope.ChangeIsDebt = function (item) {
        window.ConfirmPostAjax('Bạn có chắc chắn muốn chuyển hợp đồng vay lãi này' + (item.IsBadDebt == false ? " sang nợ xấu" : " thành bình thường") + '?',
            "Xác nhận thông tin",
            1,
            null,
            urlUpdateBadDebt,
            { idVayLai: item.Id, isBadDebt: !item.IsBadDebt },
            function (result) {
                ResponseMessage(result);
                if (result.Type === 2) {
                    scope.Search();
                }
            });
    };

    scope.UpdateMoneySI = function () {
        Post_AjaxCaller(urlUpdateMoneySI
            , { contractId: scope.Modal.Data.Id, moneyIntroduce: scope.Modal.Data.MoneyIntroduce, moneyService: scope.Modal.Data.MoneyServices}
            ,
            function (data, status, headers, config) {
                ResponseMessage(data);
            }, function (data, status, headers, config) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            });
    };
    
    // #endregion

    // #endregion

})


