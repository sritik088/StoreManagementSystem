
(function () {
    "use strict";

    function initializeProductCategoryDropdowns() {
        const mainCategory = document.getElementById("MainCategoryId");
        const category = document.getElementById("CategoryId");
        const subCategory = document.getElementById("SubCategoryId");

        if (!mainCategory || !category || !subCategory) {
            return;
        }

        // Prevent duplicate event handlers.
        if (mainCategory.dataset.dropdownInitialized === "true") {
            return;
        }

        mainCategory.dataset.dropdownInitialized = "true";

        const categoryUrl = "/Product/GetCategories";
        const subCategoryUrl = "/Product/GetSubCategories";

        async function loadOptions(url, parameterName, parameterValue, dropdown, placeholder) {
            dropdown.replaceChildren(new Option(placeholder, ""));
            dropdown.disabled = true;

            if (!parameterValue) {
                dropdown.disabled = false;
                return;
            }

            try {
                const query = new URLSearchParams({
                    [parameterName]: parameterValue
                });

                const response = await fetch(`${url}?${query.toString()}`, {
                    method: "GET",
                    headers: {
                        "Accept": "application/json"
                    },
                    credentials: "same-origin"
                });

                if (!response.ok) {
                    throw new Error(`Request failed: ${response.status}`);
                }

                const items = await response.json();

                items.forEach(item => {
                    dropdown.add(new Option(item.name, item.id));
                });
            } catch (error) {
                console.error("Unable to load product category options:", error);
                dropdown.replaceChildren(new Option("Unable to load options", ""));
            } finally {
                dropdown.disabled = false;
            }
        }

        mainCategory.addEventListener("change", async function () {
            // Reset both dependent dropdowns.
            category.replaceChildren(new Option("-- Select Category --", ""));
            subCategory.replaceChildren(new Option("-- Select Sub Category --", ""));

            await loadOptions(
                categoryUrl,
                "mainCategoryId",
                mainCategory.value,
                category,
                "-- Select Category --"
            );
        });

        category.addEventListener("change", async function () {
            subCategory.replaceChildren(new Option("-- Select Sub Category --", ""));

            await loadOptions(
                subCategoryUrl,
                "categoryId",
                category.value,
                subCategory,
                "-- Select Sub Category --"
            );
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener(
            "DOMContentLoaded",
            initializeProductCategoryDropdowns
        );
    } else {
        initializeProductCategoryDropdowns();
    }
})();
