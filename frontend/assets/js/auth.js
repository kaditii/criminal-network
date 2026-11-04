// Each page has its own form. data-action tells us which endpoint to use.
const authForm = document.getElementById("auth-form");
const action = authForm.dataset.action;
const message = document.getElementById("auth-message");
const submitButton = document.getElementById("auth-submit");

if (action === "login" && new URLSearchParams(location.search).has("registered")) {
    message.textContent = "Account created. Log in with your new password.";
}

authForm.addEventListener("submit", async function (event) {
    event.preventDefault();
    const username = authForm.elements.username.value;
    const password = authForm.elements.password.value;
    message.textContent = "";
    if (action === "register" && password !== authForm.elements.confirmPassword.value) {
        message.textContent = "The passwords do not match.";
        authForm.elements.confirmPassword.focus();
        return;
    }
    submitButton.disabled = true;
    try {
        const response = await apiFetch("/api/auth/" + action, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ username: username, password: password })
        });
        if (response.status === 429) {
            message.textContent = "Too many attempts. Please wait a minute and try again.";
        } else if (!response.ok) {
            message.textContent = getApiError(await response.json());
        } else if (action === "register") {
            window.location.replace("login.html?registered=1");
        } else {
            window.location.replace("index.html");
        }
    } catch (error) {
        message.textContent = "Could not reach the server. Check the backend is running and try again.";
    } finally {
        submitButton.disabled = false;
    }
});
