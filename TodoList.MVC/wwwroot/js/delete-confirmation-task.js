function showTaskDeleteConfirmation(event) {
    event.preventDefault();

    const id = event.currentTarget.getAttribute('data-id');
    const todoListId = event.currentTarget.getAttribute('data-todolistid');
    const form = event.currentTarget.closest('form');

    const confirmDeleteButton = document.getElementById('confirmDeleteButton');
    confirmDeleteButton.setAttribute('data-id', id);
    confirmDeleteButton.setAttribute('data-todolistid', todoListId);
    confirmDeleteButton.setAttribute('data-token', form.elements['__RequestVerificationToken'].value);

    const confirmDeleteModal = $('#confirmDeleteModal');
    confirmDeleteModal.modal('show');

    confirmDeleteModal.find('.btn-secondary').click(function () {
        confirmDeleteModal.modal('hide');
    });

    confirmDeleteButton.removeEventListener('click', onDeleteButtonClick);
    confirmDeleteButton.removeEventListener('click', onTaskDeleteButtonClick);
    confirmDeleteButton.addEventListener('click', onTaskDeleteButtonClick);
}

function onTaskDeleteButtonClick() {
    const id = this.getAttribute('data-id');
    const todoListId = this.getAttribute('data-todolistid');
    const url = `/DeleteTask/${id}?todoListId=${todoListId}`;

    const token = this.getAttribute('data-token');

    $.ajax({
        url: url,
        method: 'POST',
        headers: {
            'RequestVerificationToken': token
        },
        success: function (result) {
            updateCurrentTab();

            const taskListHtml = $(result).find("#task-list").html();
            $("#task-list-container #task-list").html(taskListHtml);
            
            let activeButton = document.getElementById(currentTab);
            selectTab({ currentTarget: activeButton }, currentTab.replace('-button', ''));

            $('#confirmDeleteModal').modal('hide');
        },
        error: function (xhr) {
            window.location.href = xhr.responseText;
        }
    });
}
