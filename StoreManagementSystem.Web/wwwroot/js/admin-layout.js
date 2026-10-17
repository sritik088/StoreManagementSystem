
(function () {
    "use strict";

    const storageKey = "admin-sidebar-scroll";

    function getScrollContainer() {
        const sidebar = document.querySelector(".sidebar");

        if (!sidebar) {
            return null;
        }

        const sidebarStyle = window.getComputedStyle(sidebar);

        if (
            sidebar.scrollHeight > sidebar.clientHeight &&
            /auto|scroll/.test(sidebarStyle.overflowY)
        ) {
            return sidebar;
        }

        const scrollableChild = Array.from(
            sidebar.querySelectorAll("*")
        ).find(function (element) {
            const style = window.getComputedStyle(element);

            return (
                element.scrollHeight > element.clientHeight &&
                /auto|scroll/.test(style.overflowY)
            );
        });

        return scrollableChild || sidebar;
    }

    function saveScrollPosition() {
        const container = getScrollContainer();

        if (!container) {
            return;
        }

        try {
            sessionStorage.setItem(
                storageKey,
                String(container.scrollTop)
            );
        } catch {
            // Continue if browser storage is unavailable.
        }
    }

    function restoreScrollPosition() {
        const container = getScrollContainer();

        if (!container) {
            return;
        }

        try {
            const saved = sessionStorage.getItem(storageKey);

            if (saved !== null) {
                container.scrollTop = Number(saved) || 0;
            }
        } catch {
            // Continue if browser storage is unavailable.
        }
    }

    function highlightActiveMenu() {
        const currentPath =
            window.location.pathname.toLowerCase();

        document
            .querySelectorAll(".sidebar .nav-link")
            .forEach(function (link) {
                const href = link.getAttribute("href");

                if (!href || href === "#") {
                    return;
                }

                try {
                    const url = new URL(
                        href,
                        window.location.origin
                    );

                    if (url.origin !== window.location.origin) {
                        return;
                    }

                    if (
                        url.pathname.toLowerCase() === currentPath
                    ) {
                        link.classList.add("active");
                    }
                } catch {
                    // Ignore invalid URLs.
                }
            });
    }

    function initializeSidebar() {
        const sidebar = document.querySelector(".sidebar");

        if (!sidebar) {
            return;
        }

        restoreScrollPosition();
        highlightActiveMenu();

        sidebar.addEventListener(
            "scroll",
            saveScrollPosition,
            { passive: true }
        );

        sidebar
            .querySelectorAll("a[href]")
            .forEach(function (link) {
                link.addEventListener(
                    "click",
                    saveScrollPosition
                );
            });
    }

    document.addEventListener(
        "DOMContentLoaded",
        initializeSidebar
    );

    window.addEventListener("pageshow", function () {
        window.requestAnimationFrame(restoreScrollPosition);
    });

    window.addEventListener(
        "pagehide",
        saveScrollPosition
    );
})();
