// Initialization function
var Common = {
    
    init: function (){
        $("#divFilters").hide();
        $("#divSorting").hide();
        $(".overlay").css('display', 'none');
        $('.select2').select2();
    },

    /* this is used to show filter on listing screen */
    showFilter:function(element, id){
        $(id).toggle();
        $(id).click(function (element) {
            element.stopPropagation();
        });
        $(element).toggleClass('active');
    },

    /* this is used to hide filter on listing screen */
    hideFilter:function(self, id){
        $(self).hide();
         $('.nt-sort, .nt-filter').removeClass('active');
    },

    /* this is used to open a drawer */
    openDrawer: function(self){
        Common.showLoader();
        $(".drawer-inner", self).animate({right: "0"});
        $("body").addClass('overflow-hidden');
        $(".overlay", self ).css('display', 'block');
        Common.hideLoader();
    },

    /* this is used to close a drawer */
    closeDrawer:function(self){
        Common.showLoader();
        $(".drawer-inner", self ).animate({right: "-100%"});
        $("body").removeClass('overflow-hidden');
        $(".overlay", self).css('display', 'none');
        Common.hideLoader();
    },

    /* this is used to show loader */
    showLoader: function(){
        $(".loading-wrapper").show();
    },

    /* this is used to hide loader */
    hideLoader: function(){
        $(".loading-wrapper").hide();
    },

    /* this is used to show project name edit option */
    ShowEditOption:function(element){
        $(element).next(".edit-name").show();
    },

    /* this is used to hide project name edit option */
    CloseEditOption:function(element){
        $(element).parents(".edit-name").hide();
    }
};


function initCommon() {
    $("#divFilters").hide();
    $("#divSorting").hide();
    $(".overlay").css('display', 'none');
    //$('.select2').select2();
}

// Filter functions
function showFilter(element, id) {
    $(id).toggle();
    $(id).click(function (element) {
        element.stopPropagation();
    });
    $(element).toggleClass('active');
}

function hideFilter(self, id) {
    $(self).hide();
    $('.nt-sort, .nt-filter').removeClass('active');
}

// Drawer functions
function openDrawer(self) {
    showLoader();
    $(".drawer-inner", self).animate({ right: "0" });
    $("body").addClass('overflow-hidden');
    $(".overlay", self).css('display', 'block');
    hideLoader();
}

function closeDrawer(self) {
    showLoader();
    $(".drawer-inner", self).animate({ right: "-100%" });
    $("body").removeClass('overflow-hidden');
    $(".overlay", self).css('display', 'none');
    hideLoader();
}

// Loader functions
function showLoader() {
    $(".loading-wrapper").show();
}

function hideLoader() {
    $(".loading-wrapper").hide();
}

// Edit name functions
function showEditOption(element) {
    $(element).next(".edit-name").show();
}

function closeEditOption(element) {
    $(element).parents(".edit-name").hide();
}

// User management functions
function loadUserDetails(userId) {
    showLoader();

    $.get(`/Admin/GetUserDetails?id=${userId}`)
        .done(function (data) {
            // Check if drawer already exists
            if ($('#drawerViewUser').length === 0) {
                $('body').append(data);
            } else {
                $('#drawerViewUser').replaceWith(data);
            }

            openDrawer('#drawerViewUser');
        })
        .fail(function () {
            alert('Failed to load user details');
        })
        .always(function () {
            hideLoader();
        });
}

function loadUserForEdit(userId) {
    showLoader();

    $.get(`/Admin/GetUserForEdit?id=${userId}`)
        .done(function (data) {
            if ($('#drawerEditUser').length === 0) {
                $('body').append(data);
            } else {
                $('#drawerEditUser').replaceWith(data);
            }

            openDrawer('#drawerEditUser');

        })
        .fail(function () {
            alert('Failed to load user for editing');
        })
        .always(function () {
            hideLoader();
        });
}
function deleteUser(userId, $button, callbacks = {}) {
    const defaultCallbacks = {
        onSuccess: () => {
            showToast('success', 'User deleted successfully.');
            // Optional: Reload after delay
            setTimeout(() => location.reload(), 2000);
        },
        onError: (message) => showToast('error', message || 'Failed to delete user'),
        onConfirm: () => confirm('Are you sure you want to delete this user?')
    };

    const { onSuccess, onError, onConfirm } = { ...defaultCallbacks, ...callbacks };

    if (!onConfirm()) return;

    $button.addClass('disabled');
    showLoader();

    $.ajax({
        url: '/Admin/DeleteUser',
        type: 'POST',
        data: { id: userId },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                onSuccess();
            } else {
                onError(response.message);
            }
        },
        error: function (xhr) {
            const errorMsg = xhr.status === 401
                ? 'You are not authorized to delete this user.'
                : 'An unexpected error occurred.';
            onError(errorMsg);
        },
        complete: function () {
            $button.removeClass('disabled');
            hideLoader();
        }
    });
}



function showToast(type, message, duration = 15) { // default: 5 seconds
    toastr.options = {
        "closeButton": true,
        "progressBar": true,
        "positionClass": "toast-top-right",
        "timeOut": duration,
        "extendedTimeOut": duration
    };

    switch (type) {
        case 'success':
            toastr.success(message);
            break;
        case 'error':
            toastr.error(message);
            break;
        case 'warning':
            toastr.warning(message);
            break;
        case 'info':
            toastr.info(message);
            break;
    }
}

function handleEditFormSubmit(form) {
    showLoader();

    $.ajax({
        url: $(form).attr('action'),
        type: 'POST',
        data: $(form).serialize(),
        success: function (response) {
            if (response.success) {
                closeDrawer('#drawerEditUser');
                showToast('success', response.message || 'User updated successfully.');

                setTimeout(() => {
                    location.reload();
                }, 3000); // Wait 3 seconds
            } else if (typeof response === 'string') {
                // Replace with validation error form
                $('#drawerEditUser').html(response);
            } else if (response.message) {
                showToast('error', response.message);
            }
        },
        error: function (xhr) {
            if (xhr.status === 401) {
                showToast('error', 'You are not authorized to perform this action.');
            } else {
                showToast('error', 'An unexpected error occurred.');
            }

            $.validator.unobtrusive.parse("#editUserForm");
        },
        complete: function () {
            hideLoader();
        }
    });
}

function loadTasks() {
    showLoader();
    console.log("thios ksadklcndaslk");
    $.ajax({
        url: '/Admin/GetTasks',
        type: 'GET',
        success: function (response) {
            $('#tasks-container').html(response);
            bindTaskEvents();
        },
        error: function (xhr) {
            alert('Failed to load tasks: ' + xhr.statusText);
        },
        complete: function () {
            hideLoader();
        }
    });
}


function bindTaskEvents() {
    // View task
    $('.view-task-btn').click(function () {
        const taskId = $(this).data('task-id');
        // Implement view task logic
    });

    // Edit task
    $('.edit-task-btn').click(function () {
        const taskId = $(this).data('task-id');
        // Implement edit task logic
    });

    // Delete task
    $('.delete-task').click(function () {
        const taskId = $(this).data('taskid');
        if (confirm('Are you sure you want to delete this task?')) {
            deleteTask(taskId);
        }
    });
}




// Task View Functions
function loadTaskDetails(taskId) {
    showLoader();

    $.get(`/Admin/GetTaskDetails?id=${taskId}`)
        .done(function (data) {
            // Check if drawer already exists
            if ($('#drawerViewTask').length === 0) {
                $('body').append(data);
            } else {
                $('#drawerViewTask').replaceWith(data);
            }

            openDrawer('#drawerViewTask');
        })
        .fail(function () {
            alert('Failed to load task details');
        })
        .always(function () {
            hideLoader();
        });
}


function loadTaskForEdit(taskId) {
    showLoader();

    $.get(`/Admin/GetTaskForEdit?id=${taskId}`)
        .done(function (data) {
            if ($('#drawerEditUser').length === 0) {
                $('body').append(data);
            } else {
                $('#drawerEditUser').replaceWith(data);
            }

            openDrawer('#drawerEditUser');

        })
        .fail(function () {
            alert('Failed to load user for editing');
        })
        .always(function () {
            hideLoader();
        });
}




function deleteTask(taskId, $button) {
    if (!confirm('Are you sure you want to delete this task?')) return;

    $button.addClass('disabled');
    showLoader();

    $.ajax({
        url: '/Admin/DeleteTask',
        type: 'POST',
        data: { id: taskId },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                showToast('success', 'Task deleted successfully.');
                setTimeout(() => location.reload(), 4); // Wait 2.5 seconds
            } else {
                showToast('error', response.message || 'Failed to delete task');
            }
        },
        error: function (xhr) {
            const msg = xhr.status === 401 ? 'Unauthorized action.' : 'Server error';
            showToast('error', msg);
        },
        complete: function () {
            $button.removeClass('disabled');
            hideLoader();
        }
    });
}



function loadCommentForm(taskId) {
    Common.showLoader();

    $.get(`/Admin/AddComment?taskId=${taskId}`)
        .done(function (data) {
            if ($('#drawerAddComment').length === 0) {
                $('body').append(data);
            } else {
                $('#drawerAddComment').replaceWith(data);
            }

            Common.openDrawer('#drawerAddComment');
            $.validator.unobtrusive.parse($('#addCommentForm'));
        })
        .fail(function () {
            showToast('error', 'Failed to load comment form' , 15);
        })
        .always(function () {
            Common.hideLoader();
        });
}



function deleteTaskDetail(detailId) {
    console.log(detailId); // ID is logged
    if (!confirm('Are you sure you want to delete this activity log?')) return;

    $.ajax({
        url: `/admin/deleteDetail/${detailId}`,
        type: 'DELETE',
        success: function () {
            $(`#detail-${detailId}`).remove();
            if ($('.task-details-section .data-detail[id^="detail-"]').length === 0) {
                $('.task-details-section').append(`
                            <div class="col-lg-12 data-detail no-details">
                                <div class="data-val" style="font-style: italic;">
                                    No activity logged yet
                                </div>
                            </div>
                        `);
            }
        },
        error: function (xhr) {
            alert('Error deleting detail: ' + (xhr.responseText || 'Unknown error'));
        }
    });
}

function loadUserTaskForEdit(taskId) {
    showLoader();

    $.get(`/User/GetTaskForEdit?id=${taskId}`)
        .done(function (data) {
            if ($('#drawerEditUser').length === 0) {
                $('body').append(data);
            } else {
                $('#drawerEditUser').replaceWith(data);
            }

            openDrawer('#drawerEditUser');

        })
        .fail(function () {
            alert('Failed to load user for editing');
        })
        .always(function () {
            hideLoader();
        });
}

function deleteUserTask(taskId, $button, callbacks = {}) {
    // Default callbacks
    const defaultCallbacks = {
        onSuccess: () => console.log('Task Deleted'),
        onError: (message) => console.error('Delete error:', message),
        onConfirm: () => confirm('Are you sure you want to delete this task?')
    };

    // Merge provided callbacks with defaults
    const { onSuccess, onError, onConfirm } = { ...defaultCallbacks, ...callbacks };

    // Confirm deletion
    if (!onConfirm()) return;

    // UI state
    $button.addClass('disabled');
    showLoader();

    // AJAX call
    $.ajax({
        url: '/User/DeleteTask',
        type: 'POST',
        data: { id: taskId },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                onSuccess();
            } else {
                onError(response.message || 'Failed to delete user');
            }
        },
        error: function (xhr) {
            onError(xhr.statusText || 'Server error');
        },
        complete: function () {
            $button.removeClass('disabled');
            hideLoader();
        }
    });
}

function loadUpdateStatus(taskId) {
    showLoader();

    $.get(`/User/TaskStatus?id=${taskId}`) // Changed from UpdateStatus to TaskStatus
        .done(function (data) {
            if ($('#drawerEditUser').length === 0) {
                $('body').append(data);
            } else {
                $('#drawerEditUser').replaceWith(data);
            }
            openDrawer('#drawerEditUser');
        })
        .fail(function () {
            alert('Failed to load status update form');
        })
        .always(function () {
            hideLoader();
        });
}


// Document ready and event handlers
$(document).ready(function () {
    initCommon();
    //loadTasks();


    // View user button click handler
    $(document).on('click', '.view-user-btn', function () {
        const userId = $(this).data('user-id');
        loadUserDetails(userId);
    });

    // Edit user button click handler
    $(document).on('click', '.edit-user-btn', function () {
        const userId = $(this).data('user-id');
        loadUserForEdit(userId);
    });

    // Edit form submission handler
    $(document).on('submit', '#editUserForm', function (e) {
        e.preventDefault();
        handleEditFormSubmit(this);
    });

    $(document).on('click', '.delete-user', function () {
        const userId = $(this).data('userid');
        const $deleteButton = $(this);

        deleteUser(userId, $deleteButton, {
            onSuccess: () => {
                showToast('success', 'User deleted successfully.');
                setTimeout(() => location.reload(), 3000); // delay reload
            },
            onError: (message) => showToast('error', message)
        });
    });


    // Click handler for view task buttons
    $(document).on('click', '.view-task-btn', function () {
        const taskId = $(this).data('task-id');
        loadTaskDetails(taskId);
    });

    // Edit Task button click handler
    $(document).on('click', '.edit-task-btn', function () {
        const taskId = $(this).data('task-id');
        loadTaskForEdit(taskId);
    })


    $(document).on('click', '.delete-task', function () {
        const taskId = $(this).data('taskid');
        deleteTask(taskId, $(this));
    });



    $(document).on('click', '.add-comment-btn', function (e) {
        e.preventDefault();
        const taskId = $(this).data('task-id');
        loadCommentForm(taskId);
    });

    $(document).on('click', '.edit-user-task-btn', function () {
        const taskId = $(this).data('task-id');
        loadUserTaskForEdit(taskId);
    })


    $(document).on('click', '.delete-user-task', function () {
        const taskId = $(this).data('taskid');
        const $deleteButton = $(this);

        deleteUserTask(taskId, $deleteButton, {
            onSuccess: () => location.reload(),
            onError: (message) => alert(message)
        });
    });

    $(document).on('click', '.update-status-task', function () {
        const taskId = $(this).data('taskid'); // Changed from task-id to taskid
        loadUpdateStatus(taskId);
    });



});