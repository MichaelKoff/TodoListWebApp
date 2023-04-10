let currentFormId = null;

function editTitle(id) {
    // hide any currently displayed form
    if (currentFormId !== null) {
        $(`#editTitleContainer_${currentFormId}`).hide();
        $(`a[data-list-id="${currentFormId}"], button[data-id="${currentFormId}"]`).show();
    }

    // set the new current form ID
    currentFormId = id;

    // hide the current title link
    $(`a[data-list-id="${id}"], button[data-id="${id}"]`).hide();

    // show the edit form
    $(`#editTitleContainer_${id}`).show();

    // set focus to the text input
    $(`#editTitleContainer_${id} input[type="text"]`).focus();

    // parse validation rules for the form
    $(`#editTitleContainer_${id} form`).removeData('validator');
    $(`#editTitleContainer_${id} form`).removeData('unobtrusiveValidation');
    $.validator.unobtrusive.parse(`#editTitleContainer_${id} form`);

    // handle form submission
    $(document).off("submit", `#editTitleContainer_${id} form`).on("submit", `#editTitleContainer_${id} form`, function (e) {
        e.preventDefault();
        let form = $(this);

        $.ajax({
            url: form.attr("action"),
            method: "POST",
            data: form.serialize(),
            success: function (data) {
                $(`#editTitleContainer_${id}`).hide();
                let newHtml = $(data).find('#todo-list-items').html();
                $('#todo-list-items').html(newHtml);

                let updatedTitle = $(`a[data-list-id="${id}"]`);
                updatedTitle.trigger("click");
            },
            error: function () {
                window.location.href = "/Home/Error";
                console.log("Error updating title.");
            }
        });
    });
}

function cancelEditTitle(id) {
    // get the current title value from the link
    let currentTitle = $(`a[data-list-id="${id}"]`).text();

    // set the input value to the current title value
    $(`#editTitleContainer_${id} input[type="text"]`).val(currentTitle);

    // hide the edit form
    $(`#editTitleContainer_${id}`).hide();

    // show the current title link
    $(`a[data-list-id="${id}"], button[data-id="${id}"]`).show();

    currentFormId = null;
}