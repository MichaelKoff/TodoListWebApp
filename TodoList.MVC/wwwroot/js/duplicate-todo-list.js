function duplicateTodoList(id) {
    $(document).off("submit", `#duplicate-todo-list-${id}`).on("submit", `#duplicate-todo-list-${id}`, function (e) {
        e.preventDefault();
        let form = $(this);

        $.ajax({
            url: form.attr("action"),
            method: "POST",
            data: form.serialize(),
            success: function (data) {
                let newHtml = $(data).find('#todo-list-items').html();
                $('#todo-list-items').html(newHtml);

                const lastTodoListLink = $("#todo-list-container .todo-list-link").last();
                if (lastTodoListLink.length > 0) {
                    lastTodoListLink.trigger("click");
                }
            },
            error: function (xhr) {
                window.location.href = xhr.responseText;
            }
        });
    });
}