$(document).ready(function () {

    //datepicker
    //$('.datepicker').datepicker();

    //datatables
    $('.data-table').DataTable();

    //partial views handling
    function AssignPartialViewHandler(divContain, divRender, target, controllerName, actionName, hasDataTag) {
        var loadUrl = "/" + controllerName + "/" + actionName;

        $(divContain).on('click', target, function () {
            $(divRender).load(loadUrl + (hasDataTag ? ('/' + $(this).data('id')) : ""));
        })
    }

    AssignPartialViewHandler('#projectsRender', '#editView', '.editProject', 'Projects', '_Edit', true);
});