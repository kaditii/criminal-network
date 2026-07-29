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

const suspectForm = document.getElementById("suspect-form");
const showSuspectFormButton = document.getElementById("show-suspect-form");
const cancelSuspectFormButton = document.getElementById("cancel-suspect-form");

showSuspectFormButton.addEventListener("click", function () {
    suspectForm.classList.remove("hidden");
});

cancelSuspectFormButton.addEventListener("click", function () {
    suspectForm.classList.add("hidden");
});

const suspects = [
    {
        id: 1001,
        name: "John Smith",
        dateOfBirth: "1990-02-11",
        riskLevel: 3
    },
    {
        id: 1002,
        name: "Sarah Jones",
        dateOfBirth: "1985-07-22",
        riskLevel: 5
    }
];

const suspectTable = document.getElementById("suspect-table");

function displaySuspects() {
    suspectTable.innerHTML = "";

    suspects.forEach(function (suspect) {
        const row = document.createElement("tr");
        row.className = "border-b";

        row.innerHTML = `
            <td class="p-3">${suspect.id}</td>
            <td class="p-3">${suspect.name}</td>
            <td class="p-3">${suspect.dateOfBirth}</td>
            <td class="p-3">${suspect.riskLevel}</td>
            <td class="p-3">No actions yet</td>
        `;

        suspectTable.appendChild(row);
    });
}

displaySuspects();
