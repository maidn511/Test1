$(function () {
    $("#btnCreate").click(function () {
        var isError = Validate("frmAddAccount");
        if (!isError) {
            var birth = moment($(".birthday").val(), _dateFormat.toUpperCase()).format("L");
            $("input[name=Birthday]").val(birth.indexOf("Invalid") > -1 ? null : moment($(".birthday").val(), _dateFormat.toUpperCase()).format("L"));
            $("#frmAddAccount").submit();
        }
    });
    //test checkin
    InitControl.UploadFile($("#fileupload"),
        function (data) {
            if (!data.result.Error) {
                $(".uploadfile").find("img").attr("src", data.result.url);
                $("input[name=Avatar]").val(data.result.url);
            }
        });

    $(".uploadfile").click(function () {
        $("#fileupload").click();
    });

    $("#chkChangePass").on("ifChanged", function () {
        $("#txtPassword").prop("disabled", $("#chkChangePass").is(":checked") === false).val("");
    })
})

function success(data) {
    if (data === isSuccess) {
        notification(notifiType.success, "Cập nhật tài khoản thành công", "Success");
        setTimeout(function () {
            location.href = urlAccount;
        },
            2000);
    } else {
        notification(notifiType.error, "Có lỗi xảy ra. Vui lòng thử lại sau");
    }
}
