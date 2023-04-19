$(document).ready(function () {
    // Load initial content based on URL
    const url = window.location.href;
    const listId = getListIdFromUrl(url);
    if (listId) {
        if (listId === "TasksDueToday") {
            activateTodoListLink("TasksDueToday");
        } else {
            activateTodoListLink(listId);
        }
    }

    // Handle clicks on todo list links
    $(document).on("click", ".todo-list-link", function (e) {
        e.preventDefault();
        const listId = $(this).data("list-id");
        activateTodoListLink(listId);
        loadTasks(listId);
    });

    // Handle back button clicks
    $(window).on("popstate", function (e) {
        const state = e.originalEvent.state;
        if (state) {
            const listId = state.listId || "TasksDueToday";
            activateTodoListLink(listId);
            loadTasks(listId, false);
        } else {
            activateTodoListLink("TasksDueToday");
            loadTasks("TasksDueToday", false);
        }
    });

    // Helper function to load tasks for a given list ID or "TasksDueToday" filter
    function loadTasks(listId, pushState = true) {
        let url;
        if (listId === "TasksDueToday") {
            url = "/ToDoList/TasksDueToday/";
        } else {
            url = `/ToDoList/todolist/${listId}`;
        }

        $.ajax({
            url: url,
            method: "POST",
            data: { id: listId },
            success: function (result) {
                $("#task-list-container").html(result);
                if (pushState) {
                    const stateObj = { listId: listId };
                    history.pushState(stateObj, null, url);
                }
            },
            error: function (xhr) {
                window.location.href = xhr.responseText;
            }
        });
    }

    // Helper function to activate the selected todo list link
    function activateTodoListLink(listId) {
        $(".list-group-item").removeClass("active");
        $(".todo-list-link").removeClass("active");
        if (listId === "TasksDueToday") {
            $("#due-today-link").closest(".list-group-item").addClass("active");
            $("#due-today-link").addClass("active");
        } else {
            $(`.todo-list-link[data-list-id="${listId}"]`).closest(".list-group-item").addClass("active");
            $(`.todo-list-link[data-list-id="${listId}"]`).addClass("active");
        }
    }

    // Helper function to extract list ID from URL
    function getListIdFromUrl(url) {
        const match = url.match(/\/ToDoList\/todolist\/(\d+)/);
        if (match && match.length > 1) {
            return match[1];
        }
        else if (url.endsWith("/ToDoList/TasksDueToday/")) {
            return "TasksDueToday";
        }

        return null;
    }
});
