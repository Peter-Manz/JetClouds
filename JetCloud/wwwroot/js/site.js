// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


function showForm_hideForm()
{
    var x = document.getElementById("uploadFileForm");
    var bg = document.getElementById("filesGrid");
    var uploadBtn = document.getElementById("iconTitle");
    var icon = document.getElementById("icon");
    var closeBtn = document.getElementById("closeBtn");

    if (x.style.display === "none") {
        x.style.display = "block";
        bg.style.filter = "blur(8px)";
        uploadBtn.style.color = "#662582";
        icon.style.color = "#662582";
        closeBtn.style.display = "block";
    } else {
        x.style.display = "none";
        bg.style.filter = "none";
        uploadBtn.style.color = "#f5f5f5";
        icon.style.color = "#f5f5f5";
        closeBtn.style.display = "none";
    }
}