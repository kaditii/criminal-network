// Gets the navigation buttons and all of the pages.
const navButtons = document.querySelectorAll(".nav-button");
const pages = document.querySelectorAll(".page");

// Hides the other pages and shows the page the user selected.
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
    // Changes the page when a navigation button is clicked.
    button.addEventListener("click", function () {
        showPage(button.dataset.page);
    });
});

// Gets the suspect form and its two control buttons.
const suspectForm = document.getElementById("suspect-form");
const showSuspectFormButton = document.getElementById("show-suspect-form");
const cancelSuspectFormButton = document.getElementById("cancel-suspect-form");
const suspectNameInput = document.getElementById("suspect-name");
const suspectDobInput = document.getElementById("suspect-dob");
const suspectRiskInput = document.getElementById("suspect-risk");

showSuspectFormButton.addEventListener("click", function () {
    suspectForm.classList.remove("hidden");
});

cancelSuspectFormButton.addEventListener("click", function () {
    suspectForm.classList.add("hidden");
});

// This is temporary sample data until the database is connected.
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

// Goes through the suspects and adds each one to the table.
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

// Finds the next ID by adding one to the current highest ID.
function getNextSuspectId() {
    if (suspects.length === 0) {
        return 1001;
    }

    const currentIds = suspects.map(function (suspect) {
        return suspect.id;
    });

    return Math.max(...currentIds) + 1;
}

// Adds the form values as a new suspect in the temporary array.
suspectForm.addEventListener("submit", function (event) {
    event.preventDefault();

    const newSuspect = {
        id: getNextSuspectId(),
        name: suspectNameInput.value,
        dateOfBirth: suspectDobInput.value,
        riskLevel: Number(suspectRiskInput.value)
    };

    suspects.push(newSuspect);
    displaySuspects();

    // Clears and closes the form after the suspect has been added.
    suspectForm.reset();
    suspectForm.classList.add("hidden");
});

// Displays the sample suspects when the page first loads.
displaySuspects();
