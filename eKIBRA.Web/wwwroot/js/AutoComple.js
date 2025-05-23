window.eKIBRA = {
    addAutoCompleteEvents: ({ inputAspFor, inputAspForHidden, handlerName }) => {

        const inputSelector = `input[name="${inputAspFor}"]`;
        const inputHiddenSelector = `input[name="${inputAspForHidden}"]`;
        const selectSelector = `select[name="${inputAspFor}"], datalist[name="${inputAspFor}"]`;
        const selectClickSelector = `${selectSelector} > option`;

        const input = $(inputSelector);
        const select = $(selectSelector);
        const inputTargetKey = $(inputHiddenSelector);

        if (input.length === 0) {
            console.debug(`fail to find ${inputSelector}`);
            return;
        }

        if (select.length === 0) { return; }
        if (inputTargetKey.length === 0) { return; }

        const onClickHandler = function (event) {
            // add selected value [input]
            input.val(event.target.text);
            input.attr("title", `${event.target.value}`);

            // set the membership or id
            inputTargetKey.val(event.target.value);

            // clear and hide [select]
            select.empty().hide();
        }

        const onKeypressHandler = function (event) {
            // console.debug(event);
            // not char key
            if (event.key.length === 0 || event.key === "Backspace" || event.key === "Delete") {
                select.empty().hide();
                inputTargetKey.val("");
                input.attr("title", "");
                return;
            }
            // get the current value plus the new key char
            const query = `${event.target.value}${event.key}`;
            // minimum of 2 chars
            if (query.length < 2) {
                select.empty().hide();
                inputTargetKey.val("");
                input.attr("title", "");
                return;
            }

            $.get(`?handler=${handlerName}`, { search: query }, function (resp) {
                select.empty().hide();
                const data = JSON.parse(resp);

                // add all data
                for (const { Title, Value, Display } of data) {
                    select.append(`<option title="${Title}" value="${Value}">${Display}</option>`);
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
                inputTargetKey.val("");
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

// eKIBRA.addAutoCompleteEvents({ inputAspFor: "Input.TeamPlayerOne", inputAspForHidden: "Input.TeamPlayerOneX", handlerName: "SearchPlayer" });