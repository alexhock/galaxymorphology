$(document).ready(function() {
    $.ajax({
        url: "/info/?id=22"
    }).then(function(data) {
        $('.greeting-id').append(data.id);
        $('.greeting-content').append(data.content);
    });
});