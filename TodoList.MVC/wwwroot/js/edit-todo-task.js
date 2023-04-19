function openModal(button) {
    const modalId = $(button).data('modal-id');
    $(`#todo-modal-${modalId}`).modal('show');
    $(`#todo-modal-${modalId} form`).removeData('validator').removeData('unobtrusiveValidation');
    $.validator.unobtrusive.parse(`#todo-modal-${modalId} form`);

    $(document).off("submit", `#todo-modal-${modalId} form`).on("submit", `#todo-modal-${modalId} form`, function (e) {
        e.preventDefault();
        let form = $(this);
        let formData = form.serialize();

        $.ajax({
            url: form.attr("action"),
            method: "POST",
            data: formData,
            success: function (data) {
                updateCurrentTab();

                $(`#todo-modal-${modalId}`).modal('hide');
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

function cancelEdit(id) {
    $(`#todo-modal-${id}`).find('form')[0].reset();
    $(`#todo-modal-${id}`).modal('hide');
}