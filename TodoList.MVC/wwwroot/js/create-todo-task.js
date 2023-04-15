$(document).ready(function () {
    $(document).off("submit", "#create-task").on("submit", "#create-task", function (e) {
        e.preventDefault();
        let form = $(this);
        $.ajax({
            url: form.attr("action"),
            method: "POST",
            data: form.serialize(),
            success: function (data) {
                updateCurrentTab();

                let taskListHtml = $(data).find("#task-list").html();
                let formHtml = $(data).find("#create-task").html();

                $("#task-list-container #task-list").html(taskListHtml);
                $("#task-list-container #create-task").html(formHtml);

                let activeButton = document.getElementById(currentTab);
                selectTab({ currentTarget: activeButton }, currentTab.replace('-button', ''));

                form.find('input[name="Title"]').val('');
                form.find('input[name="Title"]').focus();
            },
            error: function () {
                console.log("Error creating todo list.");
            }
        });
    });
});