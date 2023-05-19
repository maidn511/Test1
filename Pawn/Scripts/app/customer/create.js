$(function () {
    $("#btnCreate").click(function () {
        var isError = Validate();
        if (!isError) {
            $("#frmAddCustomer").submit();
        }
    });
})

function success(data) {
    if (data === isSuccess) {
        notification(notifiType.success, "Cập nhật khách hàng thành công", "Success");
        setTimeout(function () {
            location.href = urlCustomer;
        },
            1500);
    } else {
        notification(notifiType.error, "Có lỗi xảy ra. Vui lòng thử lại sau");
    }
}

