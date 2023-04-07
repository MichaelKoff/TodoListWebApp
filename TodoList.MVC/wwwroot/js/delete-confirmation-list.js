﻿function showDeleteConfirmation(event) {
    event.preventDefault();

    const id = event.currentTarget.getAttribute('data-id');
    const form = event.currentTarget.closest('form');

    const confirmDeleteButton = document.getElementById('confirmDeleteButton');
    confirmDeleteButton.setAttribute('data-id', id);
    confirmDeleteButton.setAttribute('data-token', form.elements['__RequestVerificationToken'].value);

    const confirmDeleteModal = $('#confirmDeleteModal');
    confirmDeleteModal.modal('show');

    confirmDeleteModal.find('.btn-secondary').click(function () {
        confirmDeleteModal.modal('hide');
    });

    confirmDeleteButton.removeEventListener('click', onDeleteButtonClick);
    confirmDeleteButton.addEventListener('click', onDeleteButtonClick);
}

function onDeleteButtonClick() {
    const id = this.getAttribute('data-id');
    const url = `/ToDoList/Delete/${id}`;

    const token = this.getAttribute('data-token');

    $.ajax({
        url: url,
        method: 'POST',
        headers: {
            'RequestVerificationToken': token
        },
        success: function (result) {
            const todoListItems = $(result).find("#todo-list-items").html();
            $("#todo-list-items").html(todoListItems);
            $('#confirmDeleteModal').modal('hide');

            const lastTodoListLink = $("#todo-list-container .todo-list-link").last();
            if (lastTodoListLink.length > 0) {
                lastTodoListLink.trigger("click");
                lastTodoListLink.focus();
            }
        },
        error: function () {
            console.log('Error deleting item.');
        }
    });
}