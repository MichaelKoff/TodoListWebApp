$(document).ready(function () {
    $(document).off("submit", "#create-todo-list").on("submit", "#create-todo-list", function (e) {
        e.preventDefault();
        let form = $(this);
        console.log(form);
        $.ajax({
            url: form.attr("action"),
            method: "POST",
            data: form.serialize(),
            success: function (result) {
                let todoListItems = $(result).find("#todo-list-items").html();
                $("#todo-list-items").html(todoListItems);

                let newForm = $(result).find('#create-todo-list');

                newForm.find('input[name="Title"]').val('');

                $('#create-todo-list').replaceWith(newForm);

                newForm.removeData("validator").removeData("unobtrusiveValidation");
                $.validator.unobtrusive.parse(newForm)

                newForm.find('input[name="Title"]').focus();

                const lastTodoListLink = $("#todo-list-container .todo-list-link").last();
                if (lastTodoListLink.length > 0) {
                    lastTodoListLink.trigger("click");
                }
            },
            error: function () {
                window.location.href = "/Home/Error";
                console.log("Error creating todo list.");
            }
        });
    });
});