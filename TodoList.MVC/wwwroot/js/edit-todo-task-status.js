function editTodoStatus(id) {
    $(document).off("submit", `#update-task-status-${id}`).on("submit", `#update-task-status-${id}`, function (e) {
        e.preventDefault();
        let form = $(this);

        $.ajax({
            url: form.attr("action"),
            method: "POST",
            data: form.serialize(),
            success: function (data) {
                updateCurrentTab();

                const taskListHtml = $(data).find("#task-list").html();
                $("#task-list-container #task-list").html(taskListHtml);

                let activeButton = document.getElementById(currentTab);
                clickHandle({ currentTarget: activeButton }, currentTab.replace('-button', ''));
            },
            error: function () {
                window.location.href = "/Home/Error";
                console.log("Error updating task.");
            }
        });
    });
}