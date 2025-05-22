$(document).ready(function () {
    // Debugging: Verify elements exist
    console.log('userDropdown exists:', $('#userDropdown').length > 0);
    console.log('taskStatusDropdown exists:', $('#taskStatusDropdown').length > 0);

    // Event handler for user dropdown
    $(document).on('change', '#userDropdown', function () {
        console.log('User dropdown changed to:', $(this).val());

        if ($(this).val()) {
            // User selected - ensure status isn't 1 (Unassigned)
            if ($('#taskStatusDropdown').val() == '1') {
                $('#taskStatusDropdown').val('2');
                console.log('Changed status from 1 to 2 because user was assigned');
            }

            // Enable all task status options except "Unassigned"
            $('#taskStatusDropdown option').each(function () {
                const val = $(this).val();
                if (val && val !== '1') {
                    $(this).prop('disabled', false);
                }
            });
        } else {
            // No user assigned
            $('#taskStatusDropdown').val('1');

            // Disable all options except "Unassigned"
            $('#taskStatusDropdown option').each(function () {
                const val = $(this).val();
                if (val !== '1') {
                    $(this).prop('disabled', true);
                }
            });

            console.log('User unassigned, status set to 1, others disabled');
        }
    });


    // Event handler for status dropdown
    $(document).on('change', '#taskStatusDropdown', function () {
        const selectedStatus = $(this).val();
        const selectedUser = $('#userDropdown').val();

        console.log('Status dropdown changed to:', selectedStatus);

        if (selectedStatus === '1') {
            // If status is Unassigned, reset user to Unassigned too
            $('#userDropdown option.unassigned-option').prop('selected', true).trigger('change');
            console.log('User set to Unassigned because status is Unassigned');
        } 
    });

    
});