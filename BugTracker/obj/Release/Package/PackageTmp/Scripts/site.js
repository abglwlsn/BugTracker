$(document).ready(function () {

    //datepicker
    $('.datepicker').datepicker();

    //datatables
    $('.data-table').DataTable({
        responsive: true
    });

    $('.data-table-xl').DataTable({
        "columnDefs": [
            { className: "dt-body-right", "targets": -1 },
            { "width": "95px", "targets":-1 }
        ],
        responsive: true
    })

    $('.data-table-a').DataTable({
        "columnDefs": [
            { className: "dt-body-right", "targets": -1 },
        ],
        responsive: true
    })

    $('.data-table-n').DataTable({
        "columnDefs": [
          { "width": "45%", "targets": 4 },
          { "width": "20%", "targets" : 2 }
        ],
        responsive: true
    });

    $('.data-table-d').DataTable({
        "columnDefs": [
          { "width": "25%", "targets": 2 },
          { "width": "25%", "targets": 3 }
        ],
        responsive: true
    });

    $('.data-table-t').DataTable({
        "columnDefs": [
          { "width": "50%", "targets": 3 },
          { className: "dt-body-right", "targets" : -1 }
        ],
        responsive: true
    });

    $('.data-table-p').DataTable({
        "columnDefs": [
          { "width": "50%", "targets": 2 },
          { className: "dt-body-right", "targets": -1 }
        ],
        responsive:true
    });

    //chosen plugin
        $(".chosen-select").chosen();

    //cancel
        $('.cancel').click(function () {
            window.history.back();
        });

    //partial views handling
    function AssignPartialViewHandler(divContain, divRender, target, controllerName, actionName, hasDataTag) {
        var loadUrl = "/" + controllerName + "/" + actionName;

        $(divContain).on('click', target, function () {
            $(divRender).load(loadUrl + (hasDataTag ? ('/' + $(this).data('id')) : ""));
        })
    }

    //function AssignRoleHandler(divContain, divRender, target) {
    //
    //    $(divContain).on('click', target, function () {
    //        $(divRender).load('/Admin/_AddRemoveUsers/' + $(this).data('name'));
    //    })
    //}

    AssignPartialViewHandler('#usersRender', '#userInfoRender', '.userInfo', 'Admin', '_UserInfo', true);
    //AssignPartialViewHandler('#usersRender', '#rolesRender', '.manageUserRoles', 'Admin', '_AddRemoveRole', true);
    //AssignRoleHandler('#rolesRender', '#editView', '.addRemoveUser');

});