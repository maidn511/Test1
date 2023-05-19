$(document).ready(function () {
    var scope = angular.element($("#table")).scope();
    scope.Parameters.fromDateString = moment().format(_dateFormat.toUpperCase());
    scope.Parameters.toDateString = moment().format(_dateFormat.toUpperCase());
    getNotes();
    scope.Search();
    scope.getCashBalance = function (balance) {
        if (scope.AdditionData == undefined) return 0;
        var total = 0;
        var debit = 0;
        var credit = 0;
        for (var i = 0; i < scope.AdditionData.summary.length; i++) {
            credit += scope.AdditionData.summary[i].CreditAccount;
            debit += scope.AdditionData.summary[i].DebitAccount;
        }
        total = balance + (credit - debit);
        return total;
    }
    scope.search2 = function () {
        getNotes();
        scope.Search();
    }
    function getNotes() {
        var url = $('#table').attr('data-notes-url');
        Post_AjaxCaller(url, { docType: scope.Parameters.documentType },
            function (data) {
                scope.Notes = data;
            }, function () {
                scope.message = 'Unexpected Error while loading data!!';
                scope.result = "color-red";
            });
    }

    scope.ExportExcel = function () {
        var url = $('#table').attr('data-export-excel') + "?parameters=" + JSON.stringify(scope.Parameters);
        window.open(url, '_blank');
    }
});


