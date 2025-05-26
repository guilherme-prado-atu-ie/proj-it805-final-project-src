window.eKIBRA = {
    addAutoCompleteEvents: ({ inputAspFor, inputAspForHiddenId, aspForDescription, handlerName }) => {

        const inputSelector = `input[name="${inputAspFor}"]`;
        const inputHiddenIdSelector = `input[name="${inputAspForHiddenId}"]`;
        const anyDescriptionSelector = `[name="${aspForDescription}"]`;
        const selectSelector = `select[name="${inputAspFor}"], datalist[name="${inputAspFor}"]`;
        const selectClickSelector = `${selectSelector} > option`;

        const input = $(inputSelector);
        const select = $(selectSelector);
        const description = $(anyDescriptionSelector);
        const hiddenKey = $(inputHiddenIdSelector);

        if (input.length === 0) {
            console.debug(`fail to find ${inputSelector}`);
            return;
        }

        if (select.length === 0) { return; }
        if (hiddenKey.length === 0) { return; }

        const onClickHandler = function (event) {
            // add selected value [input]
            input.val(event.target.text);
            input.attr("title", `${event.target.value}`);

            // set the membership or id
            hiddenKey.val(event.target.value);
            // set the description (if passed)
            if (description.length > 0) { description.val(event.target.getAttribute("data-description")); }

            // clear and hide [select]
            select.empty().hide();
        }

        const onKeypressHandler = function (event) {
            // console.debug(event);
            // not char key
            if (event.key.length === 0 || event.key === "Backspace" || event.key === "Delete") {
                select.empty().hide();
                hiddenKey.val("");
                // set the description (if passed)
                if (description.length > 0) { description.val(event.target.getAttribute("data-description")); }
                input.attr("title", "");
                return;
            }
            // get the current value plus the new key char
            const query = `${event.target.value}${event.key}`;
            // minimum of 2 chars
            if (query.length < 2) {
                select.empty().hide();
                hiddenKey.val("");
                // set the description (if passed)
                if (description.length > 0) { description.val(""); }
                input.attr("title", "");
                return;
            }

            $.get(`?handler=${handlerName}`, { search: query }, function (resp) {
                select.empty().hide();
                const data = JSON.parse(resp);

                // add all data
                for (const { Title, Value, Display, Description } of data) {
                    select.append(`<option title="${Title}" value="${Value}" data-description="${Description}">${Display}</option>`);
                }
                // if hidden, show the select
                if (data.length > 0) {
                    select.show();
                }
            });
        }

        const onFocusInOrOut = function (event) {
            // console.debug(event);
            if (input.val().length === 0) {
                hiddenKey.val("");
                // set the description (if passed)
                if (description.length > 0) { description.val(""); }
                input.attr("title", "");
            }
        }

        input.off("focus ", onFocusInOrOut);
        input.on("focus", onFocusInOrOut);

        input.off("blur ", onFocusInOrOut);
        input.on("blur", onFocusInOrOut);

        input.off("keydown ", onKeypressHandler);
        input.on("keydown ", onKeypressHandler);

        $(document).off("click", selectClickSelector, onClickHandler);
        $(document).on("click", selectClickSelector, onClickHandler);
    }
}