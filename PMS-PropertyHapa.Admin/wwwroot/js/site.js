getAllroles();
function CheckValidation() {
    if ($('#RoleTitle').val() == '') { $('#RoleTitle').css('border-color', 'red'); var Toast = Swal.mixin({ toast: true, position: 'top-end', showConfirmButton: false, progressBar: true, timer: 3000 }); Toast.fire({ icon: 'error', title: 'Required Fields are Mandatory...!' }); } else { $('#RoleTitle').css('border-color', '#ced4da') }
    if ($('#RoleKey').val() == '') { $('#RoleKey').css('border-color', 'red'); var Toast = Swal.mixin({ toast: true, position: 'top-end', showConfirmButton: false, progressBar: true, timer: 3000 }); Toast.fire({ icon: 'error', title: 'Required Fields are Mandatory...!' }); } else { $('#RoleKey').css('border-color', '#ced4da') }
    if ($('#RoleTitle').val() == '' || $('#RoleKey').val() == '') {
        return false;
    }
    else {
        return true;
    }
}
function CheckEditValidation() {
    if ($('#RoleTitles').val() == '') { $('#RoleTitles').css('border-color', 'red'); var Toast = Swal.mixin({ toast: true, position: 'top-end', showConfirmButton: false, progressBar: true, timer: 3000 }); Toast.fire({ icon: 'error', title: 'Required Fields are Mandatory...!' }); } else { $('#RoleTitles').css('border-color', '#ced4da') }
    if ($('#RoleKeys').val() == '') { $('#RoleKeys').css('border-color', 'red'); var Toast = Swal.mixin({ toast: true, position: 'top-end', showConfirmButton: false, progressBar: true, timer: 3000 }); Toast.fire({ icon: 'error', title: 'Required Fields are Mandatory...!' }); } else { $('#RoleKeys').css('border-color', '#ced4da') }
    if ($('#RoleTitles').val() == '' || $('#RoleKeys').val() == '') {
        return false;
    }
    else {
        return true;
    }
}
function Addrole() {
    var result = CheckValidation();
    if (result == true) {
        var RoleTitle = $('#RoleTitle').val();
        var RoleKey = $('#RoleKey').val();
        $.ajax({
            type: 'POST',
            url: '/GroupRoles/Create',
            data: { "RoleTitle": RoleTitle, "RoleKey": RoleKey, },
            success: function (response) {
                var Toast = Swal.mixin({ toast: true, position: 'top-end', showConfirmButton: false, progressBar: true, timer: 3000 });
                Toast.fire({ icon: 'success', title: 'Role Create Successfully..!' });
                $("#RoleTitle").val('');
                $("#RoleKey").val('');
                $('#AddRole').modal('hide');
                getAllroles();
            },
            failure: function (response) {
                $('#result').html(response);
            }
        });
    }
}
function getAllroles() {
    $.ajax({
        type: 'GET',
        url: '/GroupRoles/GetRoles',
        processData: false,
        contentType: false,
        success: function (response) {
            debugger
            var op = '';
            $.each(response, function (key, item) {
                debugger
                op += '<tr>';
                op += '<td class="name">' + item.RoleTitle + '</td>';
                op += '<td class="name">' + item.RoleKey + '</td>';
                if (item.RoleActive == true) {
                    op += '<td style="color:green">Active</td>';
                    op += '<td><div class="d-flex gap-2"><div class="edit"><button class="btn btn-sm btn-success edit-item-btn" data-bs-toggle="modal" data-bs-target="#EditRole" onclick="EditRole(\'' + item.Id + '\',\'' + item.GroupName + '\',\'' + item.RoleTitle + '\',\'' + item.RoleKey + '\')">Edit Role</button></div><div class="remove"></div><button class="btn btn-sm btn-danger remove-item-btn" onclick="DeleteInactive(\'' + item.Id + '\')">In-Active Role</button><button class="btn btn-sm btn-danger remove-item-btn" onclick="DeleteR(\'' + item.Id + '\')">Delete Role</button> </div></td>';
                }
                else {
                    op += '<td style="color:red">In-Active</td>';
                    op += '<td><div class="d-flex gap-2"><div class="edit"><button class="btn btn-sm btn-success edit-item-btn" data-bs-toggle="modal" data-bs-target="#EditRole" onclick="EditRole(\'' + item.Id + '\',\'' + item.GroupName + '\',\'' + item.RoleTitle + '\',\'' + item.RoleKey + '\')">Edit Role</button></div><div class="remove"></div><button class="btn btn-sm btn-info remove-item-btn" onclick="DeleteRoleactive(\'' + item.Id + '\')">Active Role</button><button class="btn btn-sm btn-danger remove-item-btn" onclick="DeleteR(\'' + item.Id + '\')">Delete Role</button>  </div></td>';
                }
                op += '</tr>';
            });
            op = op.replace(/^\s*|\s*$/g, '');
            op = op.replace(/\\r\\n/gm, '');
            var expr = "</tr>\\s*<tr";
            var regEx = new RegExp(expr, "gm");
            var newRows = op.replace(regEx, "</tr><tr");
            $("#grouproles").DataTable().clear().rows.add($(newRows)).draw();
        },
        failure: function (response) {
            $('#result').html(response);
        }
    });
}
function EditRole(Id, GroupName, RoleTitle, RoleKey) {
    debugger
    $('#Ids').val(Id);
    $('#GroupNames').val(GroupName);
    $('#RoleTitles').val(RoleTitle);
    $('#RoleKeys').val(RoleKey);
}
function EditRoles() {
    var result = CheckEditValidation();
    if (result == true) {
        $.ajax({
            type: 'POST',
            url: '/GroupRoles/Edit',
            data: { "Id": $('#Ids').val(), "RoleTitle": $('#RoleTitles').val(), "RoleKey": $('#RoleKeys').val() },
            success: function (response) {
                $('#EditRole').modal('hide');
                var Toast = Swal.mixin({ toast: true, position: 'top-end', showConfirmButton: false, progressBar: true, timer: 3000 });
                Toast.fire({ icon: 'success', title: 'Role Update Successfully..!' });
                getAllroles();
            },
            failure: function (response) {
                $('#result').html(response);
            }
        });
    }
}
function DeleteInactive(Id) {
    debugger;
    Swal.fire({
        title: 'Are you sure?',
        text: "You want to In-Active this Role Again..!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                type: 'POST',
                url: '/GroupRoles/InActivrrole',
                data: {
                    "Id": Id
                },
                success: function (response) {
                    var Toast = Swal.mixin({ toast: true, position: 'top-end', showConfirmButton: false, progressBar: true, timer: 3000 });
                    Toast.fire({ icon: 'success', title: 'Role In-Active Successfully..!' });
                    getAllroles();
                },
                failure: function (response) {
                    $('#result').html(response);
                }
            });
        }
    })
}
function DeleteRoleactive(Id) {
    debugger;
    Swal.fire({
        title: 'Are you sure?',
        text: "You want to Active this Role Again..!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                type: 'POST',
                url: '/GroupRoles/Reactivrrole',
                data: {
                    "Id": Id
                },
                success: function (response) {
                    var Toast = Swal.mixin({ toast: true, position: 'top-end', showConfirmButton: false, progressBar: true, timer: 3000 });
                    Toast.fire({ icon: 'success', title: 'Role Active Successfully..!' });
                    getAllroles();
                },
                failure: function (response) {
                    $('#result').html(response);
                }
            });
        }
    })
}

function DeleteR(Id) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to Delete this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                type: 'POST',
                url: '/GroupRoles/Delete',
                data: {
                    "Id": Id
                },
                success: function (response) {
                    var Toast = Swal.mixin({ toast: true, position: 'top-end', showConfirmButton: false, progressBar: true, timer: 3000 });
                    Toast.fire({ icon: 'success', title: 'Role Delete Successfully..!' });
                    getAllroles();
                },
                failure: function (response) {
                    $('#result').html(response);
                }
            });
        }
    })
}
