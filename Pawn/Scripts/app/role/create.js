$(function () {
    $("#btnCreate").click(function () {
        var isError = Validate();
        if (!isError) {
            $("#frmAddRole").submit();
        }
    });
})

function success(data) {
    if (data === isSuccess) {
        notification(notifiType.success, "Cập nhật quyền thành công", "Success");
        setTimeout(function () {
            location.href = urlRole;
        },
            1500);
    } else {
        notification(notifiType.error, "Có lỗi xảy ra. Vui lòng thử lại sau");
    }
}

