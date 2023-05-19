$(function () {
    var chatHub = $.connection.notificationHub;

    chatHub.client.addMessage = function (content) {
        console.log(content);
    }

    $.connection.hub.start().done(function () {
        $("#tesst").click(function () {
            chatHub.server.send(user + ": test");
        })
    });
})