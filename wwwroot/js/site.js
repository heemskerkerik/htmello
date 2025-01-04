htmx.config.methodsThatUseUrlParams = ["get"];

htmx.onLoad(function(content) {
    const sortables = content.querySelectorAll(".sortable");
    for (let i = 0; i < sortables.length; i++) {
        const sortable = sortables[i];
        const sortableInstance = new Sortable(sortable, {
            ghostClass: 'blue-background-class',
            group: 'cards',
        });
        
        // disable sortable whenever a request is triggered on a .sortable
        htmx.on("htmx:trigger", function(evt) {
            if (evt.target.className !== "sortable")
                return;

            sortableInstance.option("disabled", true);
        });

        htmx.on("htmx:afterRequest", function(evt) {
            if (evt.target.className !== "sortable")
                return;

            sortableInstance.option("disabled", false);
        });
    }
})