function applyTaskFilters() {
    const filterData = {
        assignedToId: $('#filterAssignedTo').val(),
        statusId: $('#filterStatus').val(),
        startDate: $('#filterStartDate').val(),
        endDate: $('#filterEndDate').val(),
        searchTerm: $('#filterSearchTerm').val(),
        sortBy: $("input[name='sortListing']:checked").val(),
        sortOrder: $('.form-tabs ul li.active').text().toLowerCase()
    };

    $.ajax({
        url: '/Admin/TaskIndex',
        type: 'GET',
        data: filterData,
        success: function (response) {
            $('#taskListContainer').html($(response).find('#taskListContainer').html());
            Common.hideFilter('#divFilters');
            Common.hideFilter('#divSorting');
        },
        error: function () {
            alert("Failed to load tasks");
        }
    });
}

function applySorting() {
    applyTaskFilters(); // Sorting is handled via the same AJAX call
}

function resetTaskFilters() {
    $('#taskFilterForm')[0].reset();
    applyTaskFilters();
}

function resetSorting() {
    $("input[name='sortListing'][value='DueDate']").prop('checked', true);
    $(".form-tabs ul li").removeClass("active");
    $(".form-tabs ul li:first").addClass("active"); // Default to ascending
    applyTaskFilters();
}

function setSortDirection(direction) {
    $(".form-tabs ul li").removeClass("active");
    if (direction === 'asc') {
        $(".form-tabs ul li:first").addClass("active");
    } else {
        $(".form-tabs ul li:last").addClass("active");
    }
}