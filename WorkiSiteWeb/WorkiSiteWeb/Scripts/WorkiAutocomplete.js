function WorkiCityAutoComplete(textField) {
    //properties
    var _textField = textField;
    var _overrideCities = {
        75001: "Paris 1er",
        75002: "Paris 2e",
        75003: "Paris 3e",
        75004: "Paris 4e",
        75005: "Paris 5e",
        75006: "Paris 6e",
        75007: "Paris 7e",
        75008: "Paris 8e",
        75009: "Paris 9e",
        75010: "Paris 10e",
        75011: "Paris 11e",
        75012: "Paris 12e",
        75013: "Paris 13e",
        75014: "Paris 14e",
        75015: "Paris 15e",
        75016: "Paris 16e",
        75017: "Paris 17e",
        75018: "Paris 18e",
        75019: "Paris 19e",
        75020: "Paris 20e",
        13001: "Marseille 1er",
        13002: "Marseille 2e",
        13003: "Marseille 3e",
        13004: "Marseille 4e",
        13005: "Marseille 5e",
        13006: "Marseille 6e",
        13007: "Marseille 7e",
        13008: "Marseille 8e",
        13009: "Marseille 9e",
        13010: "Marseille 10e",
        13011: "Marseille 11e",
        13012: "Marseille 12e",
        13013: "Marseille 13e",
        13014: "Marseille 14e",
        13015: "Marseille 15e",
        13016: "Marseille 16e",
        69001: "Lyon 1er",
        69002: "Lyon 2e",
        69003: "Lyon 3e",
        69004: "Lyon 4e",
        69005: "Lyon 5e",
        69006: "Lyon 6e",
        69007: "Lyon 7e",
        69008: "Lyon 8e",
        69009: "Lyon 9e"
    }

    SetAutocomplete = function () {
        $(_textField).autocomplete({ minLength: 3,
            //http://www.geonames.org/export/geonames-search.html
            //http://www.geonames.org/export/web-services.html
            source: function (request, response) {
                $.ajax({
                    url: "http://api.geonames.org/postalCodeSearchJSON?username=tah91&country=FR&featureClass=P",
                    dataType: "jsonp",
                    data: {
                        style: "full",
                        maxRows: 12,
                        placename: request.term
                    },
                    success: function (data) {
                        var toDisplay = new Array();
                        response($.map(data.postalCodes, function (item) {
                            var name = item.placeName;
                            if (item.postalCode in _overrideCities)
                                name = _overrideCities[item.postalCode];
                            var displayName = name + ' (' + item.postalCode + ')';
                            var alreadyHere = $.inArray(displayName, toDisplay);
                            if (alreadyHere > -1)
                                return;
                            toDisplay.push(displayName);
                            if (item.postalCode.length > 5)
                                return;
                            return {
                                label: displayName,
                                value: displayName
                            }
                        }));
                    }
                });
            },
            select: function (event, ui) {
                var pos = ui.item.position;
                var lct = ui.item.locType;
            },
            selectFirst: true
        });
    }

    //public methods
    this.SetAutocomplete = SetAutocomplete;
}