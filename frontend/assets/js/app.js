const navButtons = document.querySelectorAll(".nav-button");
const pages = document.querySelectorAll(".page");

function showPage(pageName) {
    pages.forEach(function (page) {
        page.classList.add("hidden");
    });

    document.getElementById(pageName + "-page").classList.remove("hidden");

    navButtons.forEach(function (button) {
        button.classList.remove("bg-red-700", "text-white");
    });

    const activeButton = document.querySelector(`[data-page="${pageName}"]`);
    activeButton.classList.add("bg-red-700", "text-white");
}

navButtons.forEach(function (button) {
    button.addEventListener("click", function () {
        showPage(button.dataset.page);
    });
});
