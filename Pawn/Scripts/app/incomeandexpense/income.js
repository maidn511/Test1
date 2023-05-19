$(document).ready(function () {
    var scope = angular.element($("#table")).scope();
    scope.categoryOptions = [];
    scope.method = 'thu';
    scope.customer = 'nộp';
    scope.Parameters.voucherType = 1;
    scope.Search();
    scope.Modal.Type = 1;//Phiếu Thu
    scope.getSelectStatus(4);
    scope.PrintData = function (item) {
        scope.PrintIncome = item;
        setTimeout(function () { printDocument('FromPhieuThu'); }, 1000);
       
    }
});

