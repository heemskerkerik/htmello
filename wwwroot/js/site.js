htmx.onLoad(function (content) {
    const sortables = content.querySelectorAll(".card-stack");
    sortables.forEach(sortable => {
        const sortableInstance = Sortable.create(sortable, {
            group: "cards",
            animation: 150,
        });

        // disable sortable whenever a request is triggered on a .card-stack
        htmx.on("htmx:trigger", function (evt) {
            if (evt.target.className !== "card-stack")
                return;

            sortableInstance.option("disabled", true);
        });

        htmx.on("htmx:afterRequest", function (evt) {
            if (evt.target.className !== "card-stack")
                return;

            sortableInstance.option("disabled", false);
        });
    });
});