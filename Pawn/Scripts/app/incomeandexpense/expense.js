$(document).ready(function () {
    var scope = angular.element($("#table")).scope();
    scope.categoryOptions = [];
    scope.method = 'chi';
    scope.customer = 'nhận';
    scope.Parameters.voucherType = 2;
    scope.Search();
    scope.Modal.Type = 2;//Phiếu chi
    scope.getSelectStatus(1);
    scope.PrintData = function (item) {
        scope.PrintExpense = item;
        setTimeout(function () { printDocument('FromPhieuChi'); }, 1000);
    }
   
});
