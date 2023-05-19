//----------------Factory----------------
app.factory("tableService", ["$http",
    function ($http) {
        var service = {};
        service.getData = function ($scope) {
            if ($("#table").attr("data-url") === "") return;

            $scope.ShowLoading = true;
            $http.post($("#table").attr("data-url"), {
                currentPage: $scope.pagingInfo.pageNumber > 0 ? $scope.pagingInfo.pageNumber : 1,
                pageSize: $scope.pagingInfo.numberObjectOfPage,
                keyword:'', //biến tìm kiếm toàn cục ở trang chủ
                parameters: JSON.stringify($scope.Parameters)// danh sách parameter truyền vào: ngày, nhân viên, khách h
                //optionalObj: service.optionalObj
            }).then(function mySucces(response) {
                if (response.data.data.length > 0) {
                    var data = response.data.data.slice(0);
                    data.forEach(function (myObject, index) {
                        for (var property in myObject) {
                            if (myObject.hasOwnProperty(property)) {
                                var item = myObject[property];
                                if (IsDateTime(item)) {
                                    var date = data[index][property];
                                    var newDate = ConvertJsonToDatetime(date);
                                    response.data.data[index][property] = newDate;
                                }
                            }
                        }
                    });
                }
                //$scope.pagingInfo.pageNumber = $scope.pagingInfo.pageNumber.toString();
                $scope.Modal.Data = {};
                $scope.ListOfItem = response.data.data;
                $scope.FilterData = response.data.data;
                if (response.data.data != undefined) {
                    if (response.data.data.length > 0) {
                        jQuery.each(response.data.data, function (i) {
                            //var myObject = $scope.FilterData[i];
                            $scope.PagingIndexs = ($scope.pagingInfo.numberObjectOfPage * $scope.pagingInfo.pageNumber) - $scope.pagingInfo.numberObjectOfPage;
                            $scope.ListOfItem[i].PageIndex = $scope.PagingIndexs + i + 1;
                            //$scope.FilterData[i].PageIndex = $scope.PagingIndexs + i + 1;
                        });
                        $scope.Pager = getPager($scope, response.data.totalRows, $scope.pagingInfo.pageNumber, $scope.pagingInfo.numberObjectOfPage);
                    }
                    if (response.data.addition) {
                        
                        $scope.AdditionData = response.data.addition;
                    }
                } else {
                    $scope.Pager = getPager($scope, 0);
                }
                $scope.ShowLoading = false;
                return false;

            }, false);
        }

        service.getDetail = function ($scope, item) {
            $scope.ShowLoading = true;
            $http.post($("#table").attr("data-detail-url"), {
                id: item.Id
            }).then(function mySucces(response) {
                if (response.data != undefined) {
                    var object = response.data;
                    for (var property in object) {
                        if (object.hasOwnProperty(property)) {
                            var item = object[property];
                            if (IsDateTime(item)) {
                                var date = object[index][property];
                                var newDate = ConvertJsonToDatetime(date);
                                response.data[property] = newDate;
                            }
                        }
                    }
                    $scope.Modal.Data = response.data.data;
                    $scope.Modal.ActionModal = "Update";
                    $scope.Modal.ActionUrl = $("#table").attr("data-update-url");
                    $("#backdrop").modal({ backdrop: 'static', keyboard: false });
                }
                //$scope.pagingInfo.pageNumber = $scope.pagingInfo.pageNumber.toString();

                $scope.ShowLoading = false;
                return false;

            }, false);
            $scope.Modal.Data = item;
            $scope.Modal.ActionModal = "Update";
            $scope.Modal.ActionUrl = $("#table").attr("data-update-url");
            $("#backdrop").modal({ backdrop: 'static', keyboard: false });
        }

        service.getSelectStatus = function ($scope, type) {
            $http.post($("#pawnApp").attr("data-selection-status-url"), {
                intType: type
            }).then(function mySucces(response) {
                $scope.categoryOptions = [];
                if (response.data != undefined) {
                    $scope.categoryOptions = response.data;
                }
            }, false);
        }

        function getPager($scope, totalItems, currentPage, pageSize) {
            // default to first page
            currentPage = currentPage || 1;

            // default page size is 100
            pageSize = pageSize || 100;

            // calculate total pages
            var totalPages = Math.ceil(totalItems / pageSize);

            var startPage, endPage;
            if (totalPages <= 10) {
                // less than 10 total pages so show all
                startPage = 1;
                endPage = totalPages;
            } else {
                // more than 10 total pages so calculate start and end pages
                if (currentPage <= 6) {
                    startPage = 1;
                    endPage = 10;
                } else if (currentPage + 4 >= totalPages) {
                    startPage = totalPages - 9;
                    endPage = totalPages;
                } else {
                    startPage = currentPage - 5;
                    endPage = currentPage + 4;
                }
            }

            // calculate start and end item indexes
            var startIndex = (currentPage - 1) * pageSize;
            var endIndex = Math.min(startIndex + pageSize - 1, totalItems - 1);

            // create an array of pages to ng-repeat in the pager control
            var pages = range(startPage, endPage);

            // return object with all pager properties required by the view
            return {
                totalItems: totalItems,
                currentPage: currentPage,
                pageSize: pageSize,
                totalPages: totalPages,
                startPage: startPage,
                endPage: endPage,
                startIndex: startIndex,
                endIndex: endIndex,
                pages: pages
            };
        }

        function range(min, max, step) {
            step = step || 1;
            var input = [];
            for (var i = min; i <= max; i += step) {
                input.push(i);
            }
            return input;
        };

        return service;
    }
]);

//----------------Controller----------------
app.controller("tableController", function ($scope, tableService, $filter) {
    //----------------Define available----------------
    $scope.pagingInfo = {
        pageNumber: 1,
        numberObjectOfPage: 10,
        paging: [
            { 'id': 10, 'label': "10" },
            { 'id': 20, 'label': "20" },
            { 'id': 50, 'label': "50" },
            { 'id': 100, 'label': "100" },
            { 'id': 200, 'label': "200" }
        ],
        keyword: ""
    };
    $scope.Parameters = {
        customerId: null,
        customerName: null,
        userId: null,
        userName: null,
        fromDateString: null,
        toDateString: null,
        timePawn: null,
        documentName: null,
        documentType: null,
        documentStatus: 1,
        voucherType: null,
        Keyword: "",
        StatusContractId: -1,
        Notes:null

    }
    //var startdate = moment();
    //startdate = startdate.subtract(30, "days");
    //startdate = startdate.format(_dateFormat.toUpperCase());

    //var enddate = moment();
    //enddate = enddate.add(1, "days");
    //enddate = enddate.format(_dateFormat.toUpperCase());

    //$scope.Parameters.fromDateString = startdate;
    //$scope.Parameters.toDateString = enddate;

    /* Bindable functions
 -----------------------------------------------*/
    $scope.Modal = {
        Data: {},
        ActionModal: "Add",
        Type: null, //1:Phiếu thu, 2:Phiếu chi Tùy mục đích sử dụng, Khai báo phía ngoài Index
        ActionUrl: $("#table").attr("data-add-url") // Use to insert/update modal
    };

   // getData();
    $scope.ShowLoading = false;
    //----------------Define public functions----------------
    $scope.AddNew = function() {
        $scope.Modal.Data = {};
        $scope.Modal.ActionModal = "Add";
        $scope.Modal.ActionUrl = $("#table").attr("data-add-url");
        $scope.Modal.Data.IsSystem = false
        $("#backdrop").modal({ backdrop: 'static', keyboard: false });
    };

    $scope.getDetailByItem = function(item) {
        $scope.Modal.Data = item;
        $scope.Modal.ActionModal = "Update";
        $scope.Modal.ActionUrl = $("#table").attr("data-update-url");
        $("#backdrop").modal({ backdrop: 'static', keyboard: false });
    }

    $scope.getDetail =function(item) {
        tableService.getDetail($scope, item);
    };

    $scope.Save = function (id) {
        if ($scope.nameForm.$valid) {
            window.ConfirmPostAjax('Bạn chắc chắn muốn lưu thông tin.',
                    "Xác nhận thông tin",
                    1,
                    null,
                    $scope.Modal.ActionUrl,
                    {
                        model: $scope.Modal.Data,
                        type: $scope.Modal.Type
                    },
                    function (result) {
                        ResponseMessage(result);
                        $scope.RefreshData();
                        $("div.modal[aria-labelledby]").modal("hide");
                        $scope.nameForm.$setUntouched();
                        $scope.nameForm.$setPristine();
                    });
        }
    }

    $scope.Delete = function (id) {
            window.ConfirmPostAjax('Bạn chắc chắn muốn xóa thông tin.',
                    "Xác nhận thông tin",
                    1,
                    null,
                    $("#table").attr("data-delete-url"),
                    {
                        id: id
                    },
                    function (result) {
                        ResponseMessage(result);
                        $scope.RefreshData();
                    });
    }

    $scope.RefreshData = function () {
        getData();
        $scope.Modal.Data={};
        $scope.Modal.ActionModal= "Add",
        $scope.Modal.ActionUrl=  $("#table").attr("data-add-url"); // Use to insert/update modal        
    }

    $scope.getSelectStatus = function (type) {
        getSelectStatus(type);
    }

    $scope.RefreshPage = function () {
        $scope.pagingInfo.keyword = "";
        $scope.pagingInfo.pageNumber = 1;
        $scope.pagingInfo.numberObjectOfPage = 10;
        $scope.Modal.Data = {};
        $scope.Modal.ActionModal = "Add",
        $scope.Modal.ActionUrl = $("#table").attr("data-add-url");
        getData();
    }

    $scope.Search = function () {
        getData();
    }

    $scope.selectNumberObjectOfPage = function () {
        $scope.pagingInfo.pageNumber = 1;
        getData();
    }

    $scope.SelectPage = function (page) {
        if (typeof page === "undefined" || page === null) {
            $scope.pagingInfo.pageNumber = $('#page option:selected').text();
        } else {
            if (page > 0) $scope.pagingInfo.pageNumber = page;
        }
        if (page !== null) {
            getData();
        }

    };

    $scope.ShowAll = function () {
        $scope.pagingInfo.keyword = "";
        getData();
    }

    $scope.checkAll = function () {
        if ($scope.selectedAll) {
            $scope.selectedAll = true;
        } else {
            $scope.selectedAll = false;
        }
        angular.forEach($scope.ListOfItem, function (item) {
            item.isSelected = $scope.selectedAll;
        });

    };

    function getData() {
        tableService.getData($scope);
    };

    function getSelectStatus(type) {
        tableService.getSelectStatus($scope, type);
    }

});

