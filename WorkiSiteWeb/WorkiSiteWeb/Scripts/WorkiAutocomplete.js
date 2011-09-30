function WorkiCityAutoComplete(textField) {
    //properties
    var _textField = textField;

    SetAutocomplete = function () {
        $(_textField).autocomplete({ minLength: 3,
            //http://www.geonames.org/export/geonames-search.html
            source: function (request, response) {
                $.ajax({
                    url: "http://api.geonames.org/searchJSON?username=tah91&country=FR&featureClass=A&featureClass=P",
                    dataType: "jsonp",
                    data: {
                        style: "full",
                        maxRows: 12,
                        q: request.term
                    },
                    success: function (data) {
                        var toDisplay = new Array();
                        response($.map(data.geonames, function (item) {
                            var displayName = item.name + ' (' + item.adminCode2 + ')';
                            var alreadyHere = $.inArray(displayName, toDisplay);
                            if (alreadyHere > -1)
                                return;
                            toDisplay.push(displayName);
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
            }
        });
    }

    //public methods
    this.SetAutocomplete = SetAutocomplete;
}