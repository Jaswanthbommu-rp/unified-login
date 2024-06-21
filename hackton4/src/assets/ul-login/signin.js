var submit_form_btn = document.getElementById("submit-btn")
if (submit_form_btn != null) {
    submit_form_btn.addEventListener('click', function () {
        $("#submit_form").submit();
    });
}