$(function () {
    InitControl.Pagination("#hidTotalRows", ajaxCall);
    $("#btnSearch").click(function () {
        $("#txtKeyWord").attr("data-value", $("#txtKeyWord").val());
        ajaxCall(1, true);
    });
});

function ajaxCall(page, isLoadPaing) {
    $.ajax({
        type: "POST",
        url: urlAjaxSearch,
        data: JSON.stringify({
            page: page
        }),
        dataType: "html",
        contentType: "application/json;charset:utf-8",
        success: function (rs) {
            if (rs !== "") {
                $("#divLogError").empty().html(rs);
                if (isLoadPaing)
                    InitControl.Pagination("#hidTotalRows", ajaxCall);
            }
        },
        error: function (rs) {

        }
    });
    return false;
};