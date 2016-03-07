$(document).ready(function () {

    //datepicker
    //$('.datepicker').datepicker();

    //datatables
    $('.data-table').DataTable();

    //chosen plugin
    $(".chosen-select").chosen();


    //partial views handling
    function AssignPartialViewHandler(divContain, divRender, target, controllerName, actionName, hasDataTag) {
        var loadUrl = "/" + controllerName + "/" + actionName;

        $(divContain).on('click', target, function () {
            $(divRender).load(loadUrl + (hasDataTag ? ('/' + $(this).data('id')) : ""));
        })
    }

    function AssignRoleHandler(divContain, divRender, target) {

        $(divContain).on('click', target, function () {
            $(divRender).load('/Admin/_AddRemoveUsers/' + $(this).data('name'));
        })
    }

    AssignPartialViewHandler('#projectsRender', '#editView', '.editProject', 'Projects', '_Edit', true);
    AssignPartialViewHandler('#usersRender', '#editView', '.assignUser', 'Admin', '_AssignUserToTicket', true);
    AssignPartialViewHandler('#usersRender', '#editView', '.manageUserRoles', 'Admin', '_AddRemoveRole', true);
    AssignRoleHandler('#rolesRender', '#editView', '.addRemoveUser');

    //$('#usersRender').on('click', '.viewUserInfo', function () {
    //    $('#userInfo-' + ($(this).data('id'))).load('/Admin/_UserInfo/' + $(this).data('id'));
    //})
});