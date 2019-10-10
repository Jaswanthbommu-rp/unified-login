'use strict';

(function ($) {
  "use strict";

  function changeActive(element) {
    $('.scheduler .btn-calendar').removeClass('active');
    $(element).addClass('active');
  }

  //Clear event before setting the event
  $(document).off('click', '#dayview');
  //Setting the event
  $(document).on('click', '#dayview', function (event) {
    $('.fullcalendar').fullCalendar('changeView', 'agendaDay');
    changeActive(this);
  });

  //Clear event before setting the event
  $(document).off('click', '#weekview');
  //Setting the event
  $(document).on('click', '#weekview', function (event) {
    $('.fullcalendar').fullCalendar('changeView', 'agendaWeek');
    changeActive(this);
  });

  //Clear event before setting the event
  $(document).off('click', '#monthview');
  //Setting the event
  $(document).on('click', '#monthview', function (event) {
    $('.fullcalendar').fullCalendar('changeView', 'month');
    changeActive(this);
  });

  //Clear event before setting the event
  $(document).off('click', '#todayview');
  //Setting the event
  $(document).on('click', '#todayview', function (event) {
    $('.fullcalendar').fullCalendar('today');
  });
})(jQuery);