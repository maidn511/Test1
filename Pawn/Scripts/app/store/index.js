$(function () {
    InitControl.Pagination("#tbodyStore", ajaxCall);

    $(document).on("click", "a[data-action='btnDelete']", function () {
        var id = $(this).attr("data-id");
        swal({
            title: "Xóa cửa hàng",
            text: "Bạn có chắc chắn muốn xóa cửa hàng này?",
            icon: "warning",
            buttons: {
                cancel: {
                    text: "Hủy",
                    value: null,
                    visible: true,
                    className: "",
                    closeModal: true,
                },
                confirm: {
                    text: "Xóa",
                    value: true,
                    visible: true,
                    className: "",
                    closeModal: true
                }
            }
        }).then(isConfirm => {
            if (isConfirm) {
                $.ajax({
                    type: "POST",
                    url: urlAjaxDelete,
                    data: JSON.stringify({ intStoreId: id }),
                    contentType: "application/json;charset:utf-8",
                    dataType: "json",
                    success: function (rs) {
                        if (rs === isSuccess) {
                            notification(notifiType.success, "Xóa cửa hàng thành công", "Success");
                            ajaxCall(1, true);
                            return false;
                        }
                        notification(notifiType.error, "Có lỗi xảy ra. Vui lòng thử lại sau");
                    },
                    error: function (rs) {
                        notification(notifiType.error, "Có lỗi xảy ra. Vui lòng thử lại sau");
                    }
                });
            }
        });
    });

    $("#btnSearch").click(function () {
        $("#txtKeyword").attr("data-value", $("#txtKeyword").val());
        $("#chkIsActive").attr("data-value", ~~$("#chkIsActive").is(":checked"));
        ajaxCall(1, true);
    });
});



function ajaxCall(page, isLoadPaing) {
    $.ajax({
        type: "POST",
        url: urlAjaxSearch,
        data: JSON.stringify({
            intPageIndex: page,
            strKeyword: $("#txtKeyword").attr("data-value"),
            intIsActive: $("#chkIsActive").attr("data-value")
        }),
        dataType: "html",
        contentType: "application/json;charset:utf-8",
        success: function (rs) {
            if (rs !== "") {
                $("#divStore").empty().html(rs);
                if (isLoadPaing)
                    InitControl.Pagination("#tbodyStore", ajaxCall);
            }
        },
        error: function (rs) {

        }
    });
    return false;
};