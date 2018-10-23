﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Mijem_test_app.Models
{
    [HandleError]
 /*
     This model includes the location,
     the person who reserved it and
     FOR when they reserved it (not
     when they reserved it)
*/
    public class ReservationDate
    {
        //key
        public int Id { get; set; }
        //location name
        public Reservation Reservation { get; set; }
        //contact
        public Contact Contact { get; set; }
        //date reserved for
        public DateTime ReservedDate { get; set; }

        //here we are also going to look to see if the venue is already booked. if it is, then another user cannot reserve it
    }
}