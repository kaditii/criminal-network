// Relative URLs use the same server as the page (port 5062).
async function apiFetch(url, options = {}) {
    const response = await fetch(url, {
        ...options,
        headers: { ...options.headers, "X-Requested-With": "CriminalNetwork" }
    });
    if (response.status === 401 && !url.startsWith("/api/auth/")) {
        window.location.replace("login.html");
        throw new Error("Your session has expired. Please log in again.");
    }
    return response;
}

// ASP.NET validation errors use an errors object; our other errors use message.
function getApiError(result) {
    if (result.message) return result.message;
    if (result.errors) return Object.values(result.errors).flat().join(" ");
    return "The request could not be completed. Please try again.";
}
