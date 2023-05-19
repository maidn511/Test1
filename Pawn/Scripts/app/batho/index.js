
$(function () {
    var scope = angular.element($("#table")).scope();
    scope.Search();
    scope.staffList = [];
    scope.LoanTime = 0;
    scope.TurnAround = {
        FromDateString: moment(new Date()).format("DD-MM-YYYY")
    }

    scope.ColumnSort = [
        { name: "Ngày tạo", value: "CreatedDate" },
        { name: "Code", value: "code" },
        { name: "Tên khách hàng", value: "CustomerName" },
        { name: "Tiền đưa khách", value: "MoneyForGues" },
        { name: "Số ngày", value: "NumberDate" },
        { name: "Tiền đã đóng", value: "TotalPay" },
    ];
    scope.SortType = [
        { name: "Tăng dần", value: "asc" },
        { name: "Giảm dần", value: "desc" }
    ];
    // #region T.A Dam Dang
    //$('#backdrop').on('show.bs.modal', function (e) {
    //    alert("sdfd")
    //})
    scope.getDataSelect = function () {
        scope.SetDefaultModal();
        //var url = urlLoadSelectStaff;
        //Post_AjaxCaller(url, {}, function (data) {
        //    scope.staffList = data;
        //    scope.SetDefaultModal();
        //}, function () { });
    };

    scope.ShowDetailIdentityCard = function () {
        $('#detai-lIdentityCard').show();
    }

    scope.SetDefaultModal = function () {
        scope.Modal.Data = {
            LoanTime: 50,
            Frequency: 1,
            FromDateString: moment().format(_dateFormat.toUpperCase()),
            IsSystem: false,
            TotalMoney: 0,
            MoneyForGues: 0,
            IsCloseContract: false
        }
        scope.Modal.Data.Code = scope.Maxcontract();
    }

    scope.Maxcontract = function () {
        var value = "";
        Post_AjaxCaller(urlGetMaxContract, {}, function (data) {
            value = "BH-" + data;
        }, function () { });
        return value;
    }
    // #endregion


    // #region A Nam
    scope.TotalDebit = 0;
    scope.TotalHaving = 0;
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
        var url = $('#table').attr("data-add-history-remind-url");
        Post_AjaxCaller(url, { content: content, contractId: 5 },
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

    scope.LoadDaoHo = function () {
        scope.TurnAround = {
            TotalMoney: scope.BatHoData.TotalMoney,
            FromDateString: scope.BatHoData.FromDateString,
            MoneyForGues: scope.BatHoData.MoneyForGues,
            LoanTime: scope.BatHoData.LoanTime,
            Frequency: scope.BatHoData.Frequency,
            TotalHaving: scope.BatHoData.TotalHaving,
            MoneyOrther: scope.BatHoData.MoneyOrther
        };
    }

    scope.TurnAroundBatHo = function (originalId) {
        if (scope.BatHoData.IsCloseContract) {
            notification(notifiType.error, "Hợp đồng này đã đóng <br /> Bạn phải mở lại hợp đồng để có thể cập nhập hợp đồng");
            return false;
        }
        var url = $('#table').attr('data-turnaround-url'); if (url == undefined) return;
        if (scope.turnAroundForm.$valid) {
            scope.TurnAround.CustomerId = scope.BatHoData.CustomerId;
            window.ConfirmPostAjax('Bạn chắc chắn muốn đảo họ thông tin.',
                "Xác nhận thông tin",
                1,
                null,
                url,
                {
                    newModel: scope.TurnAround,
                    originalId: originalId
                },
                function (result) {
                    ResponseMessage(result);
                    scope.turnAroundForm.$setUntouched();
                    scope.turnAroundForm.$setPristine();
                });
        }
    }
    scope.InHopDongVay = function () {
        setTimeout(function () { printDocument('InHopDongVay'); }, 1000);
    }
    scope.InPhieuThu = function (item) {
        scope.PayData = item;
        setTimeout(function () { printDocument('InPhieuThu'); }, 1000);
    }
    // #endregion

    // #region Tuan Vip
    scope.CustomerList = [];
    scope.TotalPay = 0;
    scope.BatHoData = {};
    scope.PaymentNeedMoney = 0;
    scope.maxDateString = moment().format("DD-MM-YYYY");
    scope.DataChungTu = [];
    scope.IsPayDone = false;
    getDataSelect();
    scope.LoadDetailBatHo = function (id) {
        Post_AjaxCaller(urlLoadDetailBatHo, { id: id },
            function (data, status, headers, config) {
                scope.BatHoData = data;
                scope.BatHoData.ToDate = moment(new Date(parseInt(scope.BatHoData.FromDate.substr(6)))).add(scope.BatHoData.LoanTime - 1, "days").format(_dateFormat.toUpperCase());
                if (data.ListBatHoPay) {
                    scope.IsPayDone = scope.BatHoData.ListBatHoPay.length === scope.BatHoData.ListBatHoPay.filter(s => s.IsPaid == true).length;
                    var dataBh = data.ListBatHoPay.filter(s => s.IsPaid == true);
                    if (dataBh.length > 0)
                        scope.BatHoData.TotalPay = dataBh.map(s => Number(s.MoneyOfCustomer)).reduce((a, b) => a + b);
                }
                $("#detail").modal("show");
                $("#ldt-tab").click();
            }, function (data, status, headers, config) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            });
    }

    function loadCustomerList() {
        Post_AjaxCaller(urlLoadCustomerList,
            {},
            function (data, status, headers, config) {
                scope.CustomerList = data;
                console.log(data);
                scope.$apply();
            },
            function (data, status, headers, config) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            }
        );
    };

    function getDataSelect() {
        loadCustomerList();
        Post_AjaxCaller(urlLoadSelectStaff, {}, function (data) {
            scope.staffList = data;

        }, function () { });
        scope.SetDefaultModal();
    }

    scope.OpenTimer = function (id) {
        scope.LoadDetailBatHo(id);
        setTimeout(function () { $("#hg-tab").click(); }, 500);
    }
    //scope.LoadDetailBatHo(1);
    scope.GetDetailById = function (id) {
        loadCustomerList();
        Post_AjaxCaller(urlLoadDetailBatHoModel, { id: id },
            function (data) {
                scope.Modal.Data = data;
                scope.Modal.Data.StoreIdConvert = scope.Modal.Data.StoreId;
            }, function (data, status, headers, config) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            });
    }
    scope.Paid = function (item, index) {
        var isSubmit = true;
        if (scope.BatHoData.IsCloseContract) {
            notification(notifiType.error, "Hợp đồng này đã đóng <br /> Bạn phải mở lại hợp đồng để có thể cập nhập hợp đồng");
            return false;
        }

        if (index === scope.BatHoData.ListBatHoPay.length - 1) isSubmit = true
        else if (scope.BatHoData.ListBatHoPay[index + 1].IsPaid) {
            item.IsPaid = !item.IsPaid
            notification(notifiType.error, "Bạn phải hủy các lần đóng tiền sau đã");
            isSubmit = false;
        };
        if (isSubmit) {
            var obj = {
                FromDate: moment(new Date(parseInt(item.FromDate.substr(6)))).format("MM/DD/YYYY"),
                ToDate: moment(new Date(parseInt(item.ToDate.substr(6)))).format("MM/DD/YYYY"),
                LoanDate: moment(item.LoanDate, _dateFormat.toUpperCase()).format("MM/DD/YYYY") + moment().format(" HH:mm:ss"),
                Id: item.Id,
                MoneyOfCustomer: item.MoneyOfCustomer,
                IsPaid: item.IsPaid,
                PaymentNeedMoney: item.PaymentNeedMoney,
                BathoId: scope.BatHoData.Id
            };
            Post_AjaxCaller(urlUpdateBh, { item: obj },
                function (data, status, headers, config) {
                    ResponseMessage(data);
                    scope.LoadDetailBatHo(scope.BatHoData.Id)
                }, function (data, status, headers, config) {
                    scope.message = 'Unexpected Error while loading data!!';
                    scope.result = "color-red";
                });
        }
    }
    //scope.LoadDetailBatHo(1);
    scope.ChangePaymentMoney = function () {
        var obj = {
            money: scope.PaymentNeedMoney,
            id: scope.BatHoData.Id
        }
        window.ConfirmPostAjax('Bạn có chắc chắn muốn thay đổi lại số tiền đóng của 1 kỳ họ cho hợp đồng này không ?',
            "Xác nhận thông tin",
            1,
            null,
            urlUpdateBhPay,
            obj,
            function (data) {
                ResponseMessage(data);
                scope.LoadDetailBatHo(scope.BatHoData.Id)
                $("#btnUpdatePayment").click();
                scope.$apply();
            });
    }
    scope.FormatDate = function (date) {
        if (date.indexOf("Date") > -1)
            date = moment(date).format('DD/MM/YYYY');
        return date.indexOf("Invalid") > -1 ? null : date;
    }

    scope.Abs = function (money) {
        return Math.abs(money);
    }

    scope.NumberInterestRate = 0;
    scope.MoneyNumber = 0;
    scope.CalculatorContract = function () {
        scope.NumberInterestRate = scope.BatHoData.ListBatHoPay.filter(s => s.IsPaid == false).length;
        var dataBhPay = scope.BatHoData.ListBatHoPay.filter(s => s.IsPaid == false);
        if (dataBhPay.length > 0)
            scope.MoneyNumber = dataBhPay.map(s => Number(s.PaymentNeedMoney)).reduce((a, b) => a + b);
    }

    scope.ConfirmClostInstallment = function () {
        if (scope.BatHoData.IsCloseContract) {
            notification(notifiType.error, "Hợp đồng này đã đóng <br /> Bạn phải mở lại hợp đồng để có thể cập nhập hợp đồng");
            return false;
        }
        var text = "<span style='color:red'>" +
            (scope.BatHoData.MoneyOrther < 0
                ? "Tiền nợ cũ là: " + Math.abs(scope.BatHoData.MoneyOrther) + "VNĐ!"
                : "Tiền thừa là: " + Math.abs(scope.BatHoData.MoneyOrther) + "VNĐ!") + "</span><br />";
        var style = 'style = "margin-top:0"';
        var confirm = (scope.BatHoData.MoneyOrther != 0 ? text : "") + "Bạn có chắc chắn muốn <b>ĐÓNG</b> hợp đồng không?";
        var button1 = '<button type="button" role="button" tabindex="0" class="btn btn-primary swal2-styled closewithmoney">' + (scope.BatHoData.MoneyOrther < 0 ? "Trả nợ" : "Trả tiền thừa") + ' & Đóng hợp đồng</button>';
        var button = '<button type="button" role="button" tabindex="0" class="btn btn-success swal2-styled closecontract">Đóng hợp đồng</button>' +
            '<button type="button" role="button" tabindex="0" class="btn btn-danger swal2-styled cancelmodel"' + (scope.BatHoData.MoneyOrther == 0 ? style : "") + '>Thoát</button>';
        var htmlButton = scope.BatHoData.MoneyOrther != 0 ? button1 + button : button;
        swal({
            title: 'Xác nhận',
            html: confirm +
                "<br><br>" +
                htmlButton,
            showCancelButton: false,
            showConfirmButton: false
        });
    };

    $(document).on("click", ".swal2-styled.cancelmodel", function () {
        swal.close();
    })

    $(document).on("click", ".swal2-styled.closewithmoney", function () {
        scope.CloseInstallment();
    })

    $(document).on("click", ".swal2-styled.closecontract", function () {
        scope.CloseInstallment();
    })

    scope.CloseInstallment = function () {

        Post_AjaxCaller(urlCloseContract,
            {
                intBhId: scope.BatHoData.Id,
                moneyNumber: scope.BatHoData.MoneyOrther
            },
            function (data, status, headers, config) {
                ResponseMessage(data);
                if (data.Type == 2) {
                    $("#detail").modal("hide");
                    swal.close();
                    scope.Search();
                }
            }, function (data, status, headers, config) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            });
    }

    scope.AddDebt = function (isDebt) {
        if (scope.BatHoData.IsCloseContract) {
            notification(notifiType.error, "Hợp đồng này đã đóng <br /> Bạn phải mở lại hợp đồng để có thể cập nhập hợp đồng");
            return false;
        }
        var obj = {
            MoneyNumber: isDebt ? scope.MoneyDebt : scope.MoneyHaving,
            IsDebt: isDebt,
            ContractId: scope.BatHoData.Id,
            DocumentType: docType
        };
        if (obj.IsDebt && obj.MoneyNumber > ((scope.BatHoData.TotalPay || 0) + scope.BatHoData.TotalHaving)) {
            notification(notifiType.error, "Thất bại!", "Tiền nợ phải nhỏ hơn hoặc bằng số tiền đã thanh toán!");
            return false;
        }
        Post_AjaxCaller(urlAddDebt, obj,
            function (data, status, headers, config) {
                ResponseMessage(data);
                scope.LoadDetailBatHo(scope.BatHoData.Id);
            }, function (data, status, headers, config) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            });
    }

    scope.LoadChungTu = function (id) {
        if ($("#ct-tab").hasClass("active")) return false;
        Post_AjaxCaller(urlLoadChungtu, { contractId: id, documentType: docType },
            function (response) {
                scope.DataChungTu = response;
            }, function () { });
    }

    scope.AddFile = function (obj) {
        obj.ContractId = scope.BatHoData.Id;
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
    InitControl.UploadFile($("#fileupload1"),
        function (data) {

        });

    $(".uploadfile").click(function () {
        if ($("#txtMoneyDebt").val() === "15") {
            $("#fileupload1").click();
        }
        else {
            if (scope.BatHoData.IsCloseContract) {
                notification(notifiType.error, "Hợp đồng này đã đóng <br /> Bạn phải mở lại hợp đồng để có thể cập nhập hợp đồng");
                return false;
            }
            $("#fileupload").click();
        }
    });

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
        Post_AjaxCaller(urlLoadTimer, { contractId: scope.BatHoData.Id, docType: docType },
            function (response) {
                scope.DateTimer = response;
            }, function () { });
    };

    scope.AddAlarmDate = function (status) {
        if (scope.BatHoData.IsCloseContract) {
            notification(notifiType.error, "Hợp đồng này đã đóng <br /> Bạn phải mở lại hợp đồng để có thể cập nhập hợp đồng");
            return false;
        }
        scope.AlarmDate.ContractId = scope.BatHoData.Id;
        scope.AlarmDate.DocumentType = docType;
        scope.AlarmDate.Status = status;
        scope.AlarmDate.TimerDate = moment(scope.AlarmDate.DateString, _dateFormat.toUpperCase()).format("MM/DD/YYYY");

        Post_AjaxCaller(urlAddTimer, scope.AlarmDate,
            function (data, status, headers, config) {
                ResponseMessage(data);
                if (data.Type == 2) {
                    scope.LoadTimer();
                    scope.AlarmDate = {
                        DateString: moment().format(_dateFormat.toUpperCase())
                    };
                }
            }, function (data, status, headers, config) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            });
    }

    scope.ChangeIsDebt = function (item, status) {
        window.ConfirmPostAjax('Bạn có chắc chắn muốn chuyển hợp đồng bát họ này' + (item.IsBadDebt == false ? " sang nợ xấu" : " thành bình thường") + '?',
            "Xác nhận thông tin",
            1,
            null,
            urlUpdateBadDebt,
            { idBatHo: item.Id, isBadDebt: status },
            function (result) {
                ResponseMessage(result);
                if (result.Type == 2) {
                    scope.Search();
                }
            });
    }

    // #endregion

    scope.DeleteBatHo = function (item) {
        window.ConfirmPostAjax('Bạn có chắc chắn muốn ' + (item.IsDeleted == false ? "hủy" : "khôi phục") + ' hợp đồng bát họ <b>' + item.Code + '</b> của khách hàng <b>' + item.CustomerName + '</b> không ?',
            "Xác nhận thông tin",
            1,
            null,
            urlDeleteBatHo,
            { idBatHo: item.Id },
            function (result) {
                ResponseMessage(result);
                if (result.Type == 2) {
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
        )
    }

    scope.ConvertStore = function (id) {
        Post_AjaxCaller(urlConvertStore, { bathoId: id, storeId: scope.Modal.Data.StoreIdConvert },
            function (data, status, headers, config) {
                ResponseMessage(data);
                if (data.Type == 2) {
                    scope.Search();
                }
            }, function (data, status, headers, config) {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            }
        )
    }


    scope.UpdateMoneySI = function () {
        Post_AjaxCaller(urlUpdateMoneySI
            , { contractId: scope.Modal.Data.Id, moneyIntroduce: scope.Modal.Data.MoneyIntroduce, moneyService: scope.Modal.Data.MoneyServices }
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


