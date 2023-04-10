$(document).ready(function () {
    // Load initial content based on URL
    const url = window.location.href;
    const listId = getListIdFromUrl(url);
    if (listId) {
        $(`.todo-list-link[data-list-id="${listId}"]`).closest(".list-group-item").addClass("active");
        $(`.todo-list-link[data-list-id="${listId}"]`).addClass("active");
        loadTasksForList(listId);
    }

    // Handle clicks on todo list links
    $(document).on("click", ".todo-list-link", function (e) {
        e.preventDefault();
        const listId = $(this).data("list-id");

        $(".list-group-item").removeClass("active");
        $(".todo-list-link").removeClass("active");
        $(this).closest(".list-group-item").addClass("active");
        $(this).closest(".todo-list-link").addClass("active");
        loadTasksForList(listId);
    });

    // Handle back button clicks
    $(window).on("popstate", function (e) {
        const url = window.location.href;
        const listId = getListIdFromUrl(url);
        if (listId) {
            $(".list-group-item").removeClass("active");
            $(".todo-list-link").removeClass("active");
            $(`.todo-list-link[data-list-id="${listId}"]`).closest(".list-group-item").addClass("active");
            $(`.todo-list-link[data-list-id="${listId}"]`).addClass("active");
            loadTasksForList(listId, false); // pass false to not push state again
        } else {
            // If no listId in URL, load the initial content
            const url = window.location.href;
            const listId = getListIdFromUrl(url);
            if (listId) {
                loadTasksForList(listId);
            }
        }
    });

    // Helper function to load tasks for a given list ID
    function loadTasksForList(listId, pushState = true) {
        const url = '/ToDoList/todolist/' + listId;
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
            error: function (x) {
                console.log(x)
                console.log("Error loading tasks.");
            }
        });
    }

    // Helper function to extract list ID from URL
    function getListIdFromUrl(url) {
        const match = url.match(/\/ToDoList\/todolist\/(\d+)/);
        if (match && match.length > 1) {
            return match[1];
        }
        return null;
    }
});
