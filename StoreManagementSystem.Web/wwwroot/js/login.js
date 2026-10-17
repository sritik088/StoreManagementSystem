document.addEventListener("DOMContentLoaded", function () {
    const password = document.getElementById("password");
    const toggleButton = document.getElementById("togglePassword");
    const toggleIcon = document.getElementById("togglePasswordIcon");

    if (password && toggleButton && toggleIcon) {
        toggleButton.addEventListener("click", function () {
            const showPassword = password.type === "password";

            password.type = showPassword ? "text" : "password";
            toggleIcon.classList.toggle("bi-eye", !showPassword);
            toggleIcon.classList.toggle("bi-eye-slash", showPassword);

            toggleButton.setAttribute("aria-pressed", String(showPassword));
            toggleButton.setAttribute(
                "aria-label",
                showPassword ? "Hide password" : "Show password"
            );
        });
    }
});

(function () {
    "use strict";

    // Apply navigation handling only to the Login page.
    if (window.location.pathname.toLowerCase() !== "/account/login") {
        return;
    }

    // Add a Login history entry so Back does not leave Login directly.
    function protectLoginHistory() {
        window.history.pushState(
            { loginPage: true },
            "",
            window.location.href
        );
    }

    protectLoginHistory();

    window.addEventListener("popstate", function () {
        protectLoginHistory();
    });

    // Reload a cached Login page so the server checks authentication.
    window.addEventListener("pageshow", function (event) {
        if (event.persisted) {
            window.location.reload();
        }
    });
})();