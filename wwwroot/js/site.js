htmx.onLoad(function(content) {
    const sortables = content.querySelectorAll(".sortable");
    for (let i = 0; i < sortables.length; i++) {
        const sortable = sortables[i];
        const sortableInstance = new Sortable(sortable, {
            animation: 150,
            ghostClass: 'blue-background-class',
            group: 'tickets',
            onChoose: function (evt) {
                console.log("onChoose: ", sortable.attributes["data-lane"]);
            },
            onUnchoose: function (evt) {
                console.log("onUnchoose: ", sortable.attributes["data-lane"]);
            },
            onStart: function (evt) {
                console.log("onStart: ", sortable.attributes["data-lane"]);
            },
            onEnd: function (evt) {
                console.log("onEnd: ", sortable.attributes["data-lane"]);
            },
            onAdd: function (evt) {
                console.log("onAdd: ", sortable.attributes["data-lane"]);
            },
            onUpdate: function (evt) {
                console.log("onUpdate: ", sortable.attributes["data-lane"]);
            },
            onRemove: function (evt) {
                console.log("onRemove: ", sortable.attributes["data-lane"]);
            },
            onMove: function (evt) {
                console.log("onMove: ", sortable.attributes["data-lane"]);
            },
        });
        
        // htmx.on("#lanes", "htmx:trigger", function (evt) {
        //     //console.debug("Sortable: htmx:trigger: ", evt);
        //     sortableInstance.option("disabled", true);
        // });

        // htmx.on("#lanes", "htmx:afterSwap", function(evt) {
        //     //console.debug("Sortable: htmx:afterSwap: ", evt);
        //     sortableInstance.option("disabled", false);
        // });
    }
})