function Utils() { }

//methods
//to convert from js double
Utils.GetCSharpDouble = function (val) {
    var str1 = val.toString().replace(".", ",");
    return str1;
}

Utils.GetJSDouble = function (val) {
    var str1 = parseFloat(val.toString().replace(",", "."));
    return str1;
}

function ReverseGeocoder(latitudeField, longitudeField, addressField) {
    //properties
    var _latitudeField = latitudeField;
    var _longitudeField = longitudeField;
    var _addressField = addressField;
    var _geocoder = new google.maps.Geocoder();

    //Geocode from an address
    ReverseGeocodeAddress = function () {
        var lat = jQuery.trim($(_latitudeField).val());
        var lng = jQuery.trim($(_longitudeField).val());
        var latLng = new google.maps.LatLng(lat, lng);
        _geocoder.geocode({ 'location': latLng, 'region': 'FR' }, _callbackForReverseGeocode);
    }

    _callbackForReverseGeocode = function (results, status) {
        if (status == google.maps.GeocoderStatus.OK) {
            var latToFill = results[0].geometry.location.lat();
            var lngToFill = results[0].geometry.location.lng();
            $(_latitudeField).val(latToFill);
            $(_longitudeField).val(lngToFill);
            $(_addressField).val(results[0].formatted_address);
        }
    }

    //public methods
    this.ReverseGeocodeAddress = ReverseGeocodeAddress;
}

function WorkiAutoComplete(textField) {
    //properties
    var _textField = textField;
    var _geocoder = new google.maps.Geocoder();

    SetAutocomplete = function () {
        $(_textField).autocomplete({ minLength: 5,

            source: function (request, response) {

                if (_geocoder == null) {
                    _geocoder = new google.maps.Geocoder();
                }

                var bottomLeft = new google.maps.LatLng(41.395378, -5.93291);
                var topRight = new google.maps.LatLng(52.444311, 11.381544);
                var topLeft = new google.maps.LatLng(51.800574, -5.815127);
                var bottomRight = new google.maps.LatLng(40.737341, 11.713035);
                var frBounds = new google.maps.LatLngBounds(bottomLeft, topRight);
                _geocoder.geocode({ 'address': request.term + ', fr', 'bounds': frBounds, 'region': 'FR' }, function (results, status) {
                    if (status == google.maps.GeocoderStatus.OK) {
                        response($.map(results, function (loc) {
                            var searchLoc = loc.geometry.location;
                            var lat = loc.geometry.location.lat();
                            var lng = loc.geometry.location.lng();
                            var latlng = new google.maps.LatLng(lat, lng);
                            if (!frBounds.contains(latlng))
                                return;
                            return {
                                label: loc.formatted_address,
                                value: loc.formatted_address
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

function WorkiGeocoder(latitudeField, longitudeField, addressField, form, evt, cityField, postalCodeField, countryField) {
    //properties
    var _latitudeField = latitudeField;
    var _longitudeField = longitudeField;
    var _addressField = addressField;
    var _cityField = cityField;
    var _postalCodeField = postalCodeField;
    var _countryField = countryField;
    var _form = form;
    var _evt = evt;
    var _geocoder = new google.maps.Geocoder();
    var _checkSimilarLocalisation = null;

    //Geocode from an address
    SearchFormSubmit = function (evt) {
        evt.preventDefault();
        $(_form).unbind('submit');
        var address = jQuery.trim($(_addressField).val());
        if (address.length < 1)
            return;
        GeocodeAddress(address);
    }

    //Geocode from an address
    GeocodeAddress = function (address, checkSimilarLocalisation) {
        $(_latitudeField).val("0");
        $(_longitudeField).val("0");
        _checkSimilarLocalisation = checkSimilarLocalisation;
        _geocoder.geocode({ 'address': address, 'region': 'FR' }, _callbackForGeocode);
    }

    _callbackForGeocode = function (results, status) {
        if (status == google.maps.GeocoderStatus.OK) {
            var latToFill = results[0].geometry.location.lat();
            var lngToFill = results[0].geometry.location.lng();
            $(_latitudeField).val(latToFill);
            $(_longitudeField).val(lngToFill);
            if (_countryField != null || _postalCodeField != null) {
                for (var addComponent in results[0].address_components) {
                    var component = results[0].address_components[addComponent];
                    for (typeIndex in component.types) {
                        if (component.long_name == null)
                            continue;
                        if (component.types[typeIndex] == 'country') {
                            $(_countryField).val(component.long_name);
                        }
                        else if (component.types[typeIndex] == 'postal_code') {
                            $(_postalCodeField).val(component.long_name);
                        }
                        else if (component.types[typeIndex] == 'locality') {
                            $(_cityField).val(component.long_name);
                        }
                    }
                }
            }
            if (_checkSimilarLocalisation != null) {
                _checkSimilarLocalisation.call(null, latToFill, lngToFill);
            }
        }
        if (form != null) {
            $(form).trigger('submit');
        }
        //        else {
        //            //alert("La géolocalisation de votre lieu a échouée");
        //            return;
        //        }
    }

    //public methods
    this.SearchFormSubmit = SearchFormSubmit;
    this.GeocodeAddress = GeocodeAddress;
}

function WorkiMap(mapDivId, latitudeField, longitudeField) {
    //properties
    var _mapDivId = mapDivId;
    var _searchMap = null;
    var _detailMap = null;
    var _markersArray = [];
    var _geocoder = new google.maps.Geocoder();
    var _latitudeField = latitudeField;
    var _longitudeField = longitudeField;
    var _initialBounds = null;
    var _initialWhere = null;

    //methods
    //load an empty map to fill
    LoadSearchMap = function (where) {
        var options = {
            zoom: 9,
            mapTypeControl: false,
            mapTypeId: google.maps.MapTypeId.ROADMAP
        };
        _searchMap = new google.maps.Map(document.getElementById(_mapDivId), options);

        var center = new google.maps.LatLng(48, 2); //Paris...
        _searchMap.setCenter(center);
        _initialWhere = where;
        _CenterSearchResults(where);
        _SetResetControl();
    }

    //center the search map on an address
    _CenterSearchResults = function (where) {
        _geocoder.geocode({ 'address': where }, _callbackForCenterSearchResults);
    }

    _callbackForCenterSearchResults = function (results, status) {
        if (status == google.maps.GeocoderStatus.OK) {
            _searchMap.setCenter(results[0].geometry.location);
            LoadPin(results[0].geometry.location, "Votre recherche", false, _searchMap, null, true);
            //_map.setZoom(9);
            //alert(results[0].geometry.location);
            //LoadPin(results[0].geometry.location, 'Votre recherche');
        }
        else {
            //alert("La géolocalisation de votre lieu a échouée");
            return;
        }
    }

    //load map with an adress
    LoadDetailMap = function (latitude, longitude, title, editable) {
        var options = {
            zoom: 15,
            mapTypeControl: false,
            mapTypeId: google.maps.MapTypeId.ROADMAP
        };
        _detailMap = new google.maps.Map(document.getElementById(_mapDivId), options);

        var center = new google.maps.LatLng(48, 2); //Paris...
        if (latitude != null && latitude != 0 && longitude != null && longitude != 0) {
            center = new google.maps.LatLng(latitude, longitude);
        }
        else {
            _detailMap.setZoom(5);
        }

        _detailMap.setCenter(center);
        LoadPin(center, title, editable, _detailMap);
    }

    ClearMap = function () {
        if (_markersArray) {
            for (i in _markersArray) {
                _markersArray[i].setMap(null);
            }
            _markersArray.length = 0;
        }
    }

    _SetResetControl = function () {

        var controlDiv = document.createElement('DIV');
        controlDiv.index = 1;

        // Set CSS styles for the DIV containing the control
        // Setting padding to 5 px will offset the control
        // from the edge of the map
        controlDiv.style.padding = '5px';

        // Set CSS for the control border
        var controlUI = document.createElement('div');
        controlUI.style.background = '#5DAFDE url(\'/Content/images/geoloc.png\') no-repeat center center';
        controlUI.style.borderRadius = '3px';
        controlUI.style.cursor = 'pointer';
        controlUI.style.width = '20px';
        controlUI.style.height = '20px';
        controlUI.title = 'Cliquer pour revenir à la position initial';
        controlDiv.appendChild(controlUI);

        // Set CSS for the control interior
        //        var controlText = document.createElement('DIV');
        //        controlText.style.fontFamily = 'Arial,sans-serif';
        //        controlText.style.fontSize = '11px';
        //        controlText.style.color = 'white';
        //        controlText.style.paddingLeft = '4px';
        //        controlText.style.paddingRight = '4px';
        //        controlText.innerHTML = 'Reset';
        //        controlUI.appendChild(controlText);

        // Setup the click event listeners: simply set the map to Chicago
        google.maps.event.addDomListener(controlUI, 'click', function () {
            _CenterSearchResults(_initialWhere);
            if (_initialBounds != null)
                FitBoundsSearchResults(_initialBounds);
        });

        _searchMap.controls[google.maps.ControlPosition.TOP_RIGHT].push(controlDiv);
    }

    //load an pin with address, nam and desc
    LoadPin = function (LL, name, dragable, theMap, clickHandler, currentPosition) {
    	if (dragable == null)
    		dragable = false;
    	if (theMap == null)
    		theMap = _searchMap;
    	var title = "<span class=\"pinTitle\"> " + name + "</span>";
    	var image = '/Content/images/iconeMap.png';
    	if (currentPosition != null)
    	    image = '/Content/images/iconeMapRed.png';
    	var marker = new google.maps.Marker({
    		position: LL,
    		map: theMap,
    		title: name,
    		icon: image
    	});
    	if (dragable == true)
    		_markersArray.push(marker);

    	marker.setDraggable(dragable);
    	if (dragable) {
    		google.maps.event.addListener(marker, 'dragend', function () {
    			//alert(marker.getPosition().lng());
    			$(_latitudeField).val(marker.getPosition().lat());
    			$(_longitudeField).val(marker.getPosition().lng());
    		});
    	}
    	if (clickHandler != null) {
    		google.maps.event.addListener(marker, 'click', clickHandler);
    	}

    }

    //ajust zoom of the search map on an address
    FitBoundsSearchResults = function (bounds) {
        _searchMap.fitBounds(bounds);
        _initialBounds = bounds;
    }

    //find address on the map with draggable pin
    FindAddressOnMap = function (where) {
        _geocoder.geocode({ 'address': where }, _callbackForFindAddressOnMap);
    }

    _callbackForFindAddressOnMap = function (results, status) {
        if (status == google.maps.GeocoderStatus.OK) {
            _detailMap.setCenter(results[0].geometry.location);
            _detailMap.setZoom(15);
            //alert(results[0].geometry.location);
            LoadPin(results[0].geometry.location, 'Votre choix', true, _detailMap);
        }
        else {
            //alert("La géolocalisation de votre lieu a échouée");
            return;
        }
    }

    //public methods
    this.LoadSearchMap = LoadSearchMap;
    this.LoadDetailMap = LoadDetailMap;
    this.ClearMap = ClearMap;
    this.LoadPin = LoadPin;
    this.FindAddressOnMap = FindAddressOnMap;
    this.FitBoundsSearchResults = FitBoundsSearchResults;
}
