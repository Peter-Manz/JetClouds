
function showForm_hideForm()
{
    var x = document.getElementById("uploadFileForm");
    var bg = document.getElementById("filesGrid");
    var uploadBtn = document.getElementById("iconTitle");
    var icon = document.getElementById("icon");
    var closeBtn = document.getElementById("closeBtn");

    if (x.style.display === "block") {
        x.style.display = "none";
        bg.style.filter = "none";
        uploadBtn.style.color = "#f5f5f5";
        icon.style.color = "#f5f5f5";
        closeBtn.style.display = "none";
        document.getElementById("uploadFile").reset();
    } else {
        x.style.display = "block";
        bg.style.filter = "blur(8px)";
        uploadBtn.style.color = "#662582";
        icon.style.color = "#662582";
        closeBtn.style.display = "block";
       
    }
}

function showLogout_hideLogout() {
    var log = document.getElementById("profileLinks")
    if (log.style.display === "block") {
        log.style.display = "none";
    } else {
        log.style.display = "block";
    }
}


function show_hide() {
    var links = document.getElementById("links")
    if (links.style.display === "block") {
        links.style.display = "none";
    } else {
        links.style.display = "block";
    }
}
