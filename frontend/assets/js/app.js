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

    // Remembers the page so it can be reopened after a refresh.
    sessionStorage.setItem("selectedPage", pageName);
}

navButtons.forEach(function (button) {
    // Changes the page when a navigation button is clicked.
    button.addEventListener("click", function () {
        showPage(button.dataset.page);
    });
});

// Opens the last selected page, or Suspects when the site is first opened.
const selectedPage = sessionStorage.getItem("selectedPage") || "suspects";
showPage(selectedPage);

// Gets the suspect form and its two control buttons.
const suspectForm = document.getElementById("suspect-form");
const showSuspectFormButton = document.getElementById("show-suspect-form");
const cancelSuspectFormButton = document.getElementById("cancel-suspect-form");
const suspectNameInput = document.getElementById("suspect-name");
const suspectDobInput = document.getElementById("suspect-dob");
const suspectRiskInput = document.getElementById("suspect-risk");
const suspectFormError = document.getElementById("suspect-form-error");
const suspectFormTitle = document.getElementById("suspect-form-title");
const saveSuspectButton = document.getElementById("save-suspect");
const suspectSearch = document.getElementById("suspect-search");
const riskFilter = document.getElementById("risk-filter");
let editingSuspectId = null;

showSuspectFormButton.addEventListener("click", function () {
    resetSuspectForm();
    suspectForm.classList.remove("hidden");
});

cancelSuspectFormButton.addEventListener("click", function () {
    resetSuspectForm();
    suspectForm.classList.add("hidden");
});

// Stores the suspect records received from the backend.
let suspects = [];

const suspectTable = document.getElementById("suspect-table");

// Gets the suspect records from the C# backend.
async function loadSuspects() {
    try {
        const response = await fetch("http://127.0.0.1:5062/api/suspects");

        if (!response.ok) {
            throw new Error("Could not load suspects.");
        }

        suspects = await response.json();
        displaySuspects();
        displayRelationships();
        displayNetwork();
    } catch (error) {
        suspectTable.innerHTML = `
            <tr>
                <td colspan="5" class="p-4 text-center text-red-700">
                    Could not load suspects. Make sure the backend is running.
                </td>
            </tr>
        `;
    }
}

// Gets the relationship form and its two control buttons.
const relationshipForm = document.getElementById("relationship-form");
const showRelationshipFormButton = document.getElementById("show-relationship-form");
const cancelRelationshipFormButton = document.getElementById("cancel-relationship-form");
const relationshipPersonOne = document.getElementById("relationship-person-one");
const relationshipPersonTwo = document.getElementById("relationship-person-two");
const relationshipTypeInput = document.getElementById("relationship-type");
const relationshipFormError = document.getElementById("relationship-form-error");
const relationshipFormTitle = document.getElementById("relationship-form-title");
const saveRelationshipButton = document.getElementById("save-relationship");
const relationshipTable = document.getElementById("relationship-table");
let editingRelationshipId = null;

// Stores the relationship records received from the backend.
let relationships = [];

// Opens the form when the user wants to add a relationship.
showRelationshipFormButton.addEventListener("click", function () {
    resetRelationshipForm();
    relationshipForm.classList.remove("hidden");
});

// Clears and closes the form when the user cancels.
cancelRelationshipFormButton.addEventListener("click", function () {
    resetRelationshipForm();
    relationshipForm.classList.add("hidden");
});

// Gets the relationship records from the C# backend.
async function loadRelationships() {
    try {
        const response = await fetch("http://127.0.0.1:5062/api/relationships");

        if (!response.ok) {
            throw new Error("Could not load relationships.");
        }

        relationships = await response.json();
        displayRelationships();
        displayNetwork();
    } catch (error) {
        relationshipTable.innerHTML = `
            <tr>
                <td colspan="5" class="p-4 text-center text-red-700">
                    Could not load relationships. Make sure the backend is running.
                </td>
            </tr>
        `;
    }
}

// Adds the current suspects to both relationship dropdowns.
function displayRelationshipSuspects() {
    const firstOption = '<option value="">Choose a suspect</option>';

    relationshipPersonOne.innerHTML = firstOption;
    relationshipPersonTwo.innerHTML = firstOption;

    suspects.forEach(function (suspect) {
        const personOneOption = document.createElement("option");
        personOneOption.value = suspect.id;
        personOneOption.textContent = suspect.name;
        relationshipPersonOne.appendChild(personOneOption);

        const personTwoOption = document.createElement("option");
        personTwoOption.value = suspect.id;
        personTwoOption.textContent = suspect.name;
        relationshipPersonTwo.appendChild(personTwoOption);
    });
}

// Finds a suspect name from the ID stored in a relationship.
function getSuspectName(suspectId) {
    const suspect = suspects.find(function (item) {
        return item.id === suspectId;
    });

    return suspect ? suspect.name : "Unknown suspect";
}

// Draws the detective board using the suspects and relationships already loaded.
function displayNetwork() {
    const cards = document.getElementById("network-cards");
    const lines = document.getElementById("network-lines");
    const emptyMessage = document.getElementById("network-empty");

    cards.innerHTML = "";
    lines.innerHTML = "";
    emptyMessage.classList.toggle("hidden", suspects.length !== 0);

    if (suspects.length === 0) {
        return;
    }

    const centreX = 500;
    const centreY = 300;
    const radiusX = suspects.length === 1 ? 0 : 330;
    const radiusY = suspects.length === 1 ? 0 : 205;
    const positions = new Map();

    suspects.forEach(function (suspect, index) {
        const angle = (Math.PI * 2 * index / suspects.length) - Math.PI / 2;
        const x = centreX + Math.cos(angle) * radiusX;
        const y = centreY + Math.sin(angle) * radiusY;
        positions.set(suspect.id, { x: x, y: y });
    });

    // Strings are drawn first so the paper files sit on top of them.
    relationships.forEach(function (relationship) {
        const start = positions.get(relationship.firstSuspectId);
        const end = positions.get(relationship.secondSuspectId);

        if (!start || !end) {
            return;
        }

        addNetworkLine(lines, start, end, "network-string-shadow");
        addNetworkLine(lines, start, end, "network-string");

        const label = document.createElementNS("http://www.w3.org/2000/svg", "text");
        label.setAttribute("x", (start.x + end.x) / 2);
        label.setAttribute("y", (start.y + end.y) / 2 - 12);
        label.setAttribute("class", "network-link-label");
        label.textContent = relationship.type;
        lines.appendChild(label);
    });

    suspects.forEach(function (suspect, index) {
        const position = positions.get(suspect.id);
        const card = document.createElement("article");
        const initials = suspect.name
            .split(" ")
            .filter(Boolean)
            .slice(0, 2)
            .map(function (part) { return part[0].toUpperCase(); })
            .join("");

        card.className = "suspect-card";
        card.tabIndex = 0;
        card.setAttribute("role", "button");
        card.setAttribute("aria-label", "Open report for " + suspect.name);
        card.style.left = (position.x / 10) + "%";
        card.style.top = (position.y / 6.2) + "%";
        card.style.setProperty("--card-angle", ((index % 5) - 2) * 1.4 + "deg");

        const photo = document.createElement("div");
        photo.className = "suspect-photo";
        photo.textContent = initials || "?";

        const name = document.createElement("h3");
        name.className = "suspect-name";
        name.textContent = suspect.name;

        const meta = document.createElement("div");
        meta.className = "suspect-meta";

        const id = document.createElement("span");
        id.textContent = "SUBJECT #" + suspect.id;

        const risk = document.createElement("span");
        risk.className = "risk-mark";
        risk.textContent = "RISK " + suspect.riskLevel + "/5";

        meta.append(id, risk);
        card.append(photo, name, meta);
        card.addEventListener("click", function () {
            openSuspectReport(suspect.id);
        });
        card.addEventListener("keydown", function (event) {
            if (event.key === "Enter" || event.key === " ") {
                event.preventDefault();
                openSuspectReport(suspect.id);
            }
        });
        cards.appendChild(card);
    });
}

// Opens a case report for the selected suspect.
function openSuspectReport(suspectId) {
    const suspect = suspects.find(function (item) {
        return item.id === suspectId;
    });

    if (!suspect) {
        return;
    }

    const suspectRelationships = relationships.filter(function (relationship) {
        return relationship.firstSuspectId === suspectId
            || relationship.secondSuspectId === suspectId;
    });
    const initials = suspect.name
        .split(" ")
        .filter(Boolean)
        .slice(0, 2)
        .map(function (part) { return part[0].toUpperCase(); })
        .join("");

    document.getElementById("report-initials").textContent = initials || "?";
    document.getElementById("report-name").textContent = suspect.name;
    document.getElementById("report-id").textContent = "SUBJECT #" + suspect.id;
    document.getElementById("report-dob").textContent = suspect.dateOfBirth;
    document.getElementById("report-risk").textContent = suspect.riskLevel + " / 5";
    document.getElementById("report-connection-count").textContent = suspectRelationships.length;
    document.getElementById("report-summary").textContent = buildSuspectSummary(
        suspect,
        suspectRelationships.length
    );

    const connectionList = document.getElementById("report-connections");
    connectionList.innerHTML = "";

    if (suspectRelationships.length === 0) {
        const item = document.createElement("li");
        item.className = "report-none";
        item.textContent = "No confirmed associates in the current case file.";
        connectionList.appendChild(item);
    } else {
        suspectRelationships.forEach(function (relationship) {
            const associateId = relationship.firstSuspectId === suspectId
                ? relationship.secondSuspectId
                : relationship.firstSuspectId;
            const item = document.createElement("li");
            item.textContent = relationship.type + " — linked to " + getSuspectName(associateId);
            connectionList.appendChild(item);
        });
    }

    document.getElementById("suspect-report-modal").showModal();
}

// Creates a short summary from facts available in the frontend.
function buildSuspectSummary(suspect, connectionCount) {
    const riskDescription = suspect.riskLevel >= 4
        ? "high-priority"
        : suspect.riskLevel >= 3
            ? "moderate-risk"
            : "lower-risk";
    const connectionText = connectionCount === 1
        ? "one confirmed connection"
        : connectionCount + " confirmed connections";

    return suspect.name + " is classified as a " + riskDescription
        + " person of interest with " + connectionText + " in this network.";
}

const suspectReportModal = document.getElementById("suspect-report-modal");
const closeSuspectReportButton = document.getElementById("close-suspect-report");

closeSuspectReportButton.addEventListener("click", function () {
    suspectReportModal.close();
});

// Clicking the dark backdrop closes the report without treating clicks inside as close actions.
suspectReportModal.addEventListener("click", function (event) {
    if (event.target === suspectReportModal) {
        suspectReportModal.close();
    }
});

// Adds one SVG line between two suspect cards.
function addNetworkLine(svg, start, end, className) {
    const line = document.createElementNS("http://www.w3.org/2000/svg", "line");
    line.setAttribute("x1", start.x);
    line.setAttribute("y1", start.y);
    line.setAttribute("x2", end.x);
    line.setAttribute("y2", end.y);
    line.setAttribute("class", className);
    svg.appendChild(line);
}

// Goes through the relationships and adds each one to the table.
function displayRelationships() {
    relationshipTable.innerHTML = "";

    relationships.forEach(function (relationship) {
        const row = document.createElement("tr");
        row.className = "border-b";

        row.innerHTML = `
            <td class="p-3">${relationship.id}</td>
            <td class="p-3">${getSuspectName(relationship.firstSuspectId)}</td>
            <td class="p-3">${getSuspectName(relationship.secondSuspectId)}</td>
            <td class="p-3">${relationship.type}</td>
            <td class="p-3">
                <button class="edit-relationship mr-2 rounded bg-blue-700 px-3 py-1 text-white"
                        data-id="${relationship.id}">
                    Edit
                </button>
                <button class="delete-relationship rounded bg-red-700 px-3 py-1 text-white"
                        data-id="${relationship.id}">
                    Delete
                </button>
            </td>
        `;

        relationshipTable.appendChild(row);
    });

    // Adds a click event to each relationship action button.
    const editButtons = document.querySelectorAll(".edit-relationship");
    const deleteButtons = document.querySelectorAll(".delete-relationship");

    editButtons.forEach(function (button) {
        button.addEventListener("click", function () {
            editRelationship(Number(button.dataset.id));
        });
    });

    deleteButtons.forEach(function (button) {
        button.addEventListener("click", function () {
            deleteRelationship(Number(button.dataset.id));
        });
    });
}

// Sends a new or updated relationship to the backend.
relationshipForm.addEventListener("submit", async function (event) {
    event.preventDefault();

    const firstSuspectId = Number(relationshipPersonOne.value);
    const secondSuspectId = Number(relationshipPersonTwo.value);
    const type = relationshipTypeInput.value.trim();
    const endpoint = editingRelationshipId === null
        ? "http://127.0.0.1:5062/api/relationships"
        : "http://127.0.0.1:5062/api/relationships/" + editingRelationshipId;
    const method = editingRelationshipId === null ? "POST" : "PUT";

    const response = await fetch(endpoint, {
        method: method,
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            firstSuspectId: firstSuspectId,
            secondSuspectId: secondSuspectId,
            type: type
        })
    });

    if (!response.ok) {
        const result = await response.json();
        relationshipFormError.textContent = result.message;
        return;
    }

    await loadRelationships();
    resetRelationshipForm();
    relationshipForm.classList.add("hidden");
});

// Opens the form and fills it with the relationship's current details.
function editRelationship(relationshipId) {
    const relationship = relationships.find(function (item) {
        return item.id === relationshipId;
    });

    editingRelationshipId = relationshipId;
    relationshipPersonOne.value = relationship.firstSuspectId;
    relationshipPersonTwo.value = relationship.secondSuspectId;
    relationshipTypeInput.value = relationship.type;
    relationshipFormTitle.textContent = "Edit relationship";
    saveRelationshipButton.textContent = "Update";
    relationshipFormError.textContent = "";
    relationshipForm.classList.remove("hidden");
}

// Returns the relationship form to add mode.
function resetRelationshipForm() {
    editingRelationshipId = null;
    relationshipForm.reset();
    relationshipFormError.textContent = "";
    relationshipFormTitle.textContent = "New relationship";
    saveRelationshipButton.textContent = "Save";
}

// Removes a relationship from the database after the user confirms it.
async function deleteRelationship(relationshipId) {
    const relationship = relationships.find(function (item) {
        return item.id === relationshipId;
    });

    const confirmed = confirm("Delete the relationship: " + relationship.type + "?");

    if (confirmed) {
        const response = await fetch(
            "http://127.0.0.1:5062/api/relationships/" + relationshipId,
            {
                method: "DELETE"
            }
        );

        if (!response.ok) {
            const result = await response.json();
            alert(result.message);
            return;
        }

        await loadRelationships();
    }
}

// Goes through the suspects and adds each one to the table.
function displaySuspects() {
    suspectTable.innerHTML = "";

    // Gets the values selected by the user for searching and filtering.
    const searchText = suspectSearch.value.toLowerCase();
    const selectedRisk = riskFilter.value;
    const filteredSuspects = suspects.filter(function (suspect) {
        const nameMatches = suspect.name.toLowerCase().includes(searchText);
        const riskMatches = selectedRisk === "" || suspect.riskLevel === Number(selectedRisk);

        return nameMatches && riskMatches;
    });

    if (filteredSuspects.length === 0) {
        suspectTable.innerHTML = `
            <tr>
                <td colspan="5" class="p-4 text-center text-gray-500">
                    No suspects match your search.
                </td>
            </tr>
        `;
        return;
    }

    filteredSuspects.forEach(function (suspect) {
        const row = document.createElement("tr");
        row.className = "border-b";

        row.innerHTML = `
            <td class="p-3">${suspect.id}</td>
            <td class="p-3">${suspect.name}</td>
            <td class="p-3">${suspect.dateOfBirth}</td>
            <td class="p-3">${suspect.riskLevel}</td>
            <td class="p-3">
                <button class="edit-suspect mr-2 rounded bg-blue-700 px-3 py-1 text-white"
                        data-id="${suspect.id}">
                    Edit
                </button>
                <button class="delete-suspect rounded bg-red-700 px-3 py-1 text-white"
                        data-id="${suspect.id}">
                    Delete
                </button>
            </td>
        `;

        suspectTable.appendChild(row);
    });

    // Adds a click event to each edit button after the rows are created.
    const editButtons = document.querySelectorAll(".edit-suspect");

    editButtons.forEach(function (button) {
        button.addEventListener("click", function () {
            editSuspect(Number(button.dataset.id));
        });
    });

    // Adds a click event to each delete button after the rows are created.
    const deleteButtons = document.querySelectorAll(".delete-suspect");

    deleteButtons.forEach(function (button) {
        button.addEventListener("click", function () {
            deleteSuspect(Number(button.dataset.id));
        });
    });

    displayRelationshipSuspects();
}

// Updates the table when the search or risk filter changes.
suspectSearch.addEventListener("input", displaySuspects);
riskFilter.addEventListener("change", displaySuspects);

// Gets the crime form and its buttons.
const crimeForm = document.getElementById("crime-form");
const showCrimeFormButton = document.getElementById("show-crime-form");
const cancelCrimeFormButton = document.getElementById("cancel-crime-form");
const crimeTable = document.getElementById("crime-table");
const crimeTypeInput = document.getElementById("crime-type");
const crimeDateInput = document.getElementById("crime-date");
const crimeLocationInput = document.getElementById("crime-location");
const crimeFormError = document.getElementById("crime-form-error");
const crimeFormTitle = document.getElementById("crime-form-title");
const saveCrimeButton = document.getElementById("save-crime");
let editingCrimeId = null;

// Stores the crime records received from the backend.
let crimes = [];

// Gets the crime records from the C# backend.
async function loadCrimes() {
    try {
        const response = await fetch("http://127.0.0.1:5062/api/crimes");

        if (!response.ok) {
            throw new Error("Could not load crimes.");
        }

        crimes = await response.json();
        displayCrimes();
    } catch (error) {
        crimeTable.innerHTML = `
            <tr>
                <td colspan="5" class="p-4 text-center text-red-700">
                    Could not load crimes. Make sure the backend is running.
                </td>
            </tr>
        `;
    }
}

// Opens the form when the user wants to add a crime.
showCrimeFormButton.addEventListener("click", function () {
    resetCrimeForm();
    crimeForm.classList.remove("hidden");
});

// Closes and clears the form when the user cancels.
cancelCrimeFormButton.addEventListener("click", function () {
    resetCrimeForm();
    crimeForm.classList.add("hidden");
});

// Goes through the crimes and adds each one to the table.
function displayCrimes() {
    crimeTable.innerHTML = "";

    crimes.forEach(function (crime) {
        const row = document.createElement("tr");
        row.className = "border-b";

        row.innerHTML = `
            <td class="p-3">${crime.id}</td>
            <td class="p-3">${crime.type}</td>
            <td class="p-3">${crime.date}</td>
            <td class="p-3">${crime.location}</td>
            <td class="p-3">
                <button class="edit-crime mr-2 rounded bg-blue-700 px-3 py-1 text-white"
                        data-id="${crime.id}">
                    Edit
                </button>
                <button class="delete-crime rounded bg-red-700 px-3 py-1 text-white"
                        data-id="${crime.id}">
                    Delete
                </button>
            </td>
        `;

        crimeTable.appendChild(row);
    });

    // Adds a click event to each crime action button.
    const editButtons = document.querySelectorAll(".edit-crime");
    const deleteButtons = document.querySelectorAll(".delete-crime");

    editButtons.forEach(function (button) {
        button.addEventListener("click", function () {
            editCrime(Number(button.dataset.id));
        });
    });

    deleteButtons.forEach(function (button) {
        button.addEventListener("click", function () {
            deleteCrime(Number(button.dataset.id));
        });
    });
}

// Checks the crime details before they are added to the array.
function validateCrime(type, date, location) {
    const today = new Date().toISOString().split("T")[0];

    if (type === "") {
        return "The crime type is required.";
    }

    if (date === "") {
        return "The crime date is required.";
    }

    if (date > today) {
        return "The crime date cannot be in the future.";
    }

    if (location === "") {
        return "The location is required.";
    }

    return "";
}

// Sends a new or updated crime to the backend.
crimeForm.addEventListener("submit", async function (event) {
    event.preventDefault();

    const type = crimeTypeInput.value.trim();
    const date = crimeDateInput.value;
    const location = crimeLocationInput.value.trim();
    const errorMessage = validateCrime(type, date, location);

    // Stops the crime being added when the input is invalid.
    if (errorMessage !== "") {
        crimeFormError.textContent = errorMessage;
        return;
    }

    if (editingCrimeId === null) {
        const response = await fetch("http://127.0.0.1:5062/api/crimes", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                type: type,
                date: date,
                location: location
            })
        });

        if (!response.ok) {
            const result = await response.json();
            crimeFormError.textContent = result.message;
            return;
        }
    } else {
        const response = await fetch(
            "http://127.0.0.1:5062/api/crimes/" + editingCrimeId,
            {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    type: type,
                    date: date,
                    location: location
                })
            }
        );

        if (!response.ok) {
            const result = await response.json();
            crimeFormError.textContent = result.message;
            return;
        }
    }

    await loadCrimes();

    // Clears and closes the form after saving the crime.
    resetCrimeForm();
    crimeForm.classList.add("hidden");
});

// Opens the form and fills it with the crime's current details.
function editCrime(crimeId) {
    const crime = crimes.find(function (item) {
        return item.id === crimeId;
    });

    editingCrimeId = crimeId;
    crimeTypeInput.value = crime.type;
    crimeDateInput.value = crime.date;
    crimeLocationInput.value = crime.location;
    crimeFormTitle.textContent = "Edit crime";
    saveCrimeButton.textContent = "Update";
    crimeFormError.textContent = "";
    crimeForm.classList.remove("hidden");
}

// Returns the crime form to add mode.
function resetCrimeForm() {
    editingCrimeId = null;
    crimeForm.reset();
    crimeFormError.textContent = "";
    crimeFormTitle.textContent = "New crime";
    saveCrimeButton.textContent = "Save";
}

// Removes a crime from the database after the user confirms it.
async function deleteCrime(crimeId) {
    const crime = crimes.find(function (item) {
        return item.id === crimeId;
    });

    const confirmed = confirm("Delete " + crime.type + "?");

    if (confirmed) {
        const response = await fetch(
            "http://127.0.0.1:5062/api/crimes/" + crimeId,
            {
                method: "DELETE"
            }
        );

        if (!response.ok) {
            const result = await response.json();
            alert(result.message);
            return;
        }

        await loadCrimes();
    }
}

// Opens the form and fills it with the suspect's current details.
function editSuspect(suspectId) {
    const suspect = suspects.find(function (item) {
        return item.id === suspectId;
    });

    editingSuspectId = suspectId;
    suspectNameInput.value = suspect.name;
    suspectDobInput.value = suspect.dateOfBirth;
    suspectRiskInput.value = suspect.riskLevel;
    suspectFormTitle.textContent = "Edit suspect";
    saveSuspectButton.textContent = "Update";
    suspectFormError.textContent = "";
    suspectForm.classList.remove("hidden");
}

// Returns the form to add mode and clears its inputs.
function resetSuspectForm() {
    editingSuspectId = null;
    suspectForm.reset();
    suspectFormError.textContent = "";
    suspectFormTitle.textContent = "New suspect";
    saveSuspectButton.textContent = "Save";
}

// Removes a suspect from the database after the user confirms it.
async function deleteSuspect(suspectId) {
    const suspect = suspects.find(function (item) {
        return item.id === suspectId;
    });

    const confirmed = confirm("Delete " + suspect.name + "?");

    if (confirmed) {
        const response = await fetch(
            "http://127.0.0.1:5062/api/suspects/" + suspectId,
            {
                method: "DELETE"
            }
        );

        if (!response.ok) {
            const result = await response.json();
            alert(result.message);
            return;
        }

        await loadSuspects();
    }
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

// Checks the suspect details and returns an error message if needed.
function validateSuspect(name, dateOfBirth, riskLevel, suspectId) {
    const namePattern = /^[a-zA-Z -]+$/;
    const today = new Date().toISOString().split("T")[0];

    if (!namePattern.test(name)) {
        return "The name must only contain letters, spaces or hyphens.";
    }

    if (dateOfBirth > today) {
        return "The date of birth cannot be in the future.";
    }

    if (riskLevel < 1 || riskLevel > 5) {
        return "The risk level must be between 1 and 5.";
    }

    const isDuplicate = suspects.some(function (suspect) {
        return suspect.id !== suspectId
            && suspect.name.toLowerCase() === name.toLowerCase()
            && suspect.dateOfBirth === dateOfBirth;
    });

    if (isDuplicate) {
        return "A suspect with this name and date of birth already exists.";
    }

    return "";
}

// Sends a new suspect to the backend when the form is submitted.
suspectForm.addEventListener("submit", async function (event) {
    event.preventDefault();

    const name = suspectNameInput.value.trim();
    const dateOfBirth = suspectDobInput.value;
    const riskLevel = Number(suspectRiskInput.value);
    const errorMessage = validateSuspect(name, dateOfBirth, riskLevel, editingSuspectId);

    // Stops the suspect being added when the input is invalid.
    if (errorMessage !== "") {
        suspectFormError.textContent = errorMessage;
        return;
    }

    if (editingSuspectId === null) {
        const response = await fetch("http://127.0.0.1:5062/api/suspects", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                name: name,
                dateOfBirth: dateOfBirth,
                riskLevel: riskLevel
            })
        });

        if (!response.ok) {
            const result = await response.json();
            suspectFormError.textContent = result.message;
            return;
        }

        await loadSuspects();
    } else {
        const response = await fetch(
            "http://127.0.0.1:5062/api/suspects/" + editingSuspectId,
            {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    name: name,
                    dateOfBirth: dateOfBirth,
                    riskLevel: riskLevel
                })
            }
        );

        if (!response.ok) {
            const result = await response.json();
            suspectFormError.textContent = result.message;
            return;
        }

        await loadSuspects();
    }

    // Clears and closes the form after the suspect has been added.
    resetSuspectForm();
    suspectForm.classList.add("hidden");
});

// Loads the suspect records when the page first opens.
loadSuspects();
loadCrimes();
loadRelationships();
