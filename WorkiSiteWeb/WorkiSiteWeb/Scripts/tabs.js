//Set default open/close settings
$('.acc_container').hide(); //Hide/close all containers
//$('.acc_trigger:first').addClass('active').next().show(); //Add "active" class to first trigger, then show/open the immediate next container

//On Click
$('.acc_trigger').click(function () {
    if ($(this).next().is(':hidden')) { //If immediate next container is closed...
        $('.acc_trigger').removeClass('active').next().slideUp(); //Remove all "active" state and slide up the immediate next container
        $(this).toggleClass('active').next().slideDown(); //Add "active" state to clicked trigger and slide down the immediate next container
    }
    return false; //Prevent the browser jump to the link anchor
});

//Hide (Collapse) the toggle containers on load
$(".toogle_container:not(.active)").hide();

//$('.toogle_trigger_click:first').addClass('active').next().show();
//Switch the "Open" and "Close" state per click then slide up/down (depending on open/close state)
//$(".toogle_trigger_click").click(function () {
//    $(this).toggleClass("active").next().slideToggle();
//    return false; //Prevent the browser jump to the link anchor
//});

$(".toogle_trigger_click").click(function () {
    $(this).toggleClass("active").next().fadeToggle();
    return false; //Prevent the browser jump to the link anchor
});

function InitTab(tabId, tabItem, tabContent) {

	var tabItemClass = (tabItem || ".tabs");
	var tabContentClass = (tabContent || ".tab_content");
	//When page loads...
	var hiddenClass = "visuallyhidden";
	$(tabId + " " + tabContentClass).addClass(hiddenClass); //.hide(); //Hide all content
	$(tabId + " ul" + tabItemClass + " li:first").addClass("selected"); //.show(); //Activate first tab
	$(tabId + " " + tabContentClass + ":first").removeClass(hiddenClass); //.show(); //Show first tab content

    //On Click Event
	$(tabId + " ul" + tabItemClass + " li").click(function () {

		$(tabId + " ul" + tabItemClass + " li").removeClass("selected"); //Remove any "active" class
		$(this).addClass("selected"); //Add "active" class to selected tab
        $(tabId + " " + tabContentClass).addClass(hiddenClass); //hide(); //Hide all tab content

        var activeTab = $(this).find("a").attr("href"); //Find the href attribute value to identify the active tab + content
        $(activeTab).removeClass(hiddenClass);//.fadeIn(); //Fade in the active ID content
        return false;
    });

};