$(function () {
    $("#btnCreate").click(function () {
        var isError = Validate("frmAddStore");
        if (!isError) {
            $("#txtMoneyNumber").val($("#txtMoneyNumber").val().replace(/,/gi, ""));
            $("#frmAddStore").submit();
        }
    });
})

function success(data) {
    ResponseMessage(data);
    if (data.Type == 2) {
        setTimeout(function () {
            location.href = urlStore;
        },
            1500);
    }
}

