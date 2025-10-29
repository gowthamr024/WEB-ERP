document.addEventListener("keydown", function (e) {
    if (e.ctrlKey && e.shiftKey && e.key.toLowerCase() === "e") {
        e.preventDefault();
        openErrorLogPopup();
    }
});

document.getElementById("closeErrorLogBtn").addEventListener("click", function () {
    document.getElementById("errorLogPopup").style.display = "none";
});

function openErrorLogPopup() {
    const popup = document.getElementById("errorLogPopup");
    popup.style.display = "flex";

    // Fetch logs from backend
    fetch("/ErrorLog/GetLatest") // Your API endpoint
        .then(res => res.json())
        .then(data => {
            document.getElementById("errorLogContent").innerText = data.join("\n");
        })
        .catch(err => {
            document.getElementById("errorLogContent").innerText = "Failed to load logs.\n" + err;
        });
}
