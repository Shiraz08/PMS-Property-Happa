if (localStorage.getItem("color"))
    $("#color").attr("href", "../assets/css/" + localStorage.getItem("color") + ".css");
if (localStorage.getItem("dark"))
    $("body").attr("class", "dark-only");
$('<div class="customizer-links"> <div class="nav flex-column nac-pills" id="c-pills-tab" role="tablist" aria-orientation="vertical"> <a class="nav-link" id="c-pills-layouts-tab" data-bs-toggle="pill" href="#c-pills-layouts" role="tab" aria-controls="c-pills-layouts" aria-selected="true" data-original-title="" title=""> <div class="settings"><i class="icon-paint-bucket"></i></div></a> <a class="nav-link" id="c-pills-home-tab" data-bs-toggle="pill" href="#c-pills-home" role="tab" aria-controls="c-pills-home" aria-selected="true" data-original-title="" title=""> <div class="settings"><i class="icon-settings"></i></div></a> </div></div><div class="customizer-contain"> <div class="tab-content" id="c-pills-tabContent"> <div class="customizer-header"> <i class="icofont-close icon-close"></i> <h5>Preview Settings</h5> <p class="mb-0">Try It Real Time <i class="fa fa-thumbs-o-up txt-primary"></i></p></div><div class="customizer-body custom-scrollbar"> <div class="tab-pane fade show active" id="c-pills-home" role="tabpanel" aria-labelledby="c-pills-home-tab"> <h6>Layout Type</h6> <ul class="main-layout layout-grid"> <li data-attr="ltr" class="active"> <div class="header bg-light"> <ul> <li></li><li></li><li></li></ul> </div><div class="body"> <ul> <li class="bg-light sidebar"></li><li class="bg-light body"><span class="badge badge-primary">LTR</span></li></ul> </div></li><li data-attr="rtl"> <div class="header bg-light"> <ul> <li></li><li></li><li></li></ul> </div><div class="body"> <ul> <li class="bg-light body"><span class="badge badge-primary">RTL</span></li><li class="bg-light sidebar"></li></ul> </div></li><li data-attr="ltr" class="box-layout px-3"> <div class="header bg-light"> <ul> <li></li><li></li><li></li></ul> </div><div class="body"> <ul> <li class="bg-light sidebar"></li><li class="bg-light body"><span class="badge badge-primary">Box</span></li></ul> </div></li></ul> <h6 class="">Sidebar Type</h6> <ul class="sidebar-type layout-grid"> <li data-attr="normal-sidebar"> <div class="header bg-light"> <ul> <li></li><li></li><li></li></ul> </div><div class="body"> <ul> <li class="bg-dark sidebar"></li><li class="bg-light body"></li></ul> </div></li><li data-attr="compact-sidebar"> <div class="header bg-light"> <ul> <li></li><li></li><li></li></ul> </div><div class="body"> <ul> <li class="bg-dark sidebar compact"></li><li class="bg-light body"></li></ul> </div></li></ul> </div><div class="tab-pane fade" id="c-pills-layouts" role="tabpanel" aria-labelledby="c-pills-layouts-tab"> <h6 class="">Unlimited Color</h6> <ul class="layout-grid unlimited-color-layout"> <input id="ColorPicker1" type="color" value="#3e5fce" name="Background"/> <input id="ColorPicker2" type="color" value="#ffce00" name="Background"/> <button type="button" class="color-apply-btn btn btn-primary color-apply-btn">Apply</button> </ul> <h6>Light layout</h6> <ul class="layout-grid customizer-color"> <li class="color-layout" data-attr="color-1" data-primary="#6362e7" data-secondary="#ffc500"> <div></div></li><li class="color-layout" data-attr="color-2" data-primary="#10539c" data-secondary="#ec9a71"> <div></div></li><li class="color-layout" data-attr="color-3" data-primary="#2c5f2d" data-secondary="#90b757"> <div></div></li><li class="color-layout" data-attr="color-4" data-primary="#0E7C7B" data-secondary="#dbb98f"> <div></div></li><li class="color-layout" data-attr="color-5" data-primary="#5f4b8b" data-secondary="#e69a8d"> <div></div></li><li class="color-layout" data-attr="color-6" data-primary="#c38c81" data-secondary="#89d4df"> <div></div></li></ul> <h6 class="">Dark Layout</h6> <ul class="layout-grid customizer-color dark"> <li class="color-layout" data-attr="color-1" data-primary="#3e5fce" data-secondary="#ffce00"> <div></div></li><li class="color-layout" data-attr="color-2" data-primary="#603f83" data-secondary="#c7d3d4"> <div></div></li><li class="color-layout" data-attr="color-3" data-primary="#262223" data-secondary="#ddc6b6"> <div></div></li><li class="color-layout" data-attr="color-4" data-primary="#234e70" data-secondary="#fbf8bf"> <div></div></li><li class="color-layout" data-attr="color-5" data-primary="#317773" data-secondary="#dbb98f"> <div></div></li><li class="color-layout" data-attr="color-6" data-primary="#755139" data-secondary="#f2edd7"> <div></div></li></ul> <h6 class="">Mix Layout</h6> <ul class="layout-grid customizer-mix"> <li class="color-layout active" data-attr="light-only"> <div class="header bg-light"> <ul> <li></li><li></li><li></li></ul> </div><div class="body"> <ul> <li class="bg-light sidebar"></li><li class="bg-light body"></li></ul> </div></li><li class="color-layout" data-attr="dark-sidebar"> <div class="header bg-light"> <ul> <li></li><li></li><li></li></ul> </div><div class="body"> <ul> <li class="bg-dark sidebar"></li><li class="bg-light body"></li></ul> </div></li><li class="color-layout" data-attr="dark-only"> <div class="header bg-dark"> <ul> <li></li><li></li><li></li></ul> </div><div class="body"> <ul> <li class="bg-dark sidebar"></li><li class="bg-dark body"></li></ul> </div></li></ul> </div></div></div></div>').appendTo($('body'));
(function (){})();

$(document).ready(function () {
    // Apply primary and secondary colors stored in localStorage
    const primaryColor = localStorage.getItem('primaryColor');
    const secondaryColor = localStorage.getItem('secondaryColor');

    // Check if primary and secondary colors exist and apply them
    if (primaryColor) {
        document.documentElement.style.setProperty('--theme-primary', primaryColor);
    }

    if (secondaryColor) {
        document.documentElement.style.setProperty('--theme-secondary', secondaryColor);
    }

    // Check and apply dark mode class based on localStorage
    if (localStorage.getItem("dark")) {
        $("body").addClass("dark-only");
    } else {
        $("body").removeClass("dark-only");
    }

    // Your existing logic to inject customizer links into the page
    $('<div class="customizer-links"> ... </div>').appendTo($('body'));

    // Customizer color click event bindings and other customizer logic
    $(".customizer-color li").on('click', function () {
        // Existing logic for handling customizer color clicks
    });

    // Apply dark mode toggle logic
    $(".customizer-color.dark li").on('click', function () {
        // Existing logic for handling dark mode toggle
    });

    // Additional customizer and theme settings logic as needed
    // ...

    // Example: Update the color picker values based on localStorage
    $("#ColorPicker1").val(primaryColor || '#defaultPrimaryColor');
    $("#ColorPicker2").val(secondaryColor || '#defaultSecondaryColor');

    // Example: Listen for color picker changes and update localStorage accordingly
    $("#ColorPicker1, #ColorPicker2").on('change', function () {
        const newPrimaryColor = $("#ColorPicker1").val();
        const newSecondaryColor = $("#ColorPicker2").val();
        localStorage.setItem('primaryColor', newPrimaryColor);
        localStorage.setItem('secondaryColor', newSecondaryColor);
        document.documentElement.style.setProperty('--theme-primary', newPrimaryColor);
        document.documentElement.style.setProperty('--theme-secondary', newSecondaryColor);
    });

    // Reload the page to apply the newly selected colors (from color picker or other theme changing mechanism)
    $(".color-apply-btn").click(function () {
        location.reload(true);
    });
});
