﻿function editTodoStatus(id) {
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
                selectTab({ currentTarget: activeButton }, currentTab.replace('-button', ''));
            },
            error: function (xhr) {
                window.location.href = xhr.responseText;
            }
        });
    });
}