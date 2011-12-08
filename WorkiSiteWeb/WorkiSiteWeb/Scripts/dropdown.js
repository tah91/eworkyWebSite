function InitDropdown(dropdownId) {
    $(dropdownId + " dt a").live('click', function (event) {
		$(dropdownId + " dd ul").toggle();
	});

    $(dropdownId + " dd ul li a").live('click', function (event) {
		//var text = $(this).html();
		//$(".dropdown dt a span").html(text);
		$(dropdownId + " dd ul").hide();
	});

	$(document).bind('click', function (e) {
		var clicked = $(e.target);
		if (!clicked.parents().hasClass("dropdown"))
			$(dropdownId + " dd ul").hide();
	});
};