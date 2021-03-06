﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Mijem_test_app.Models;

namespace Mijem_test_app.Controllers
{
    public class ReservationDatesController : Controller
    {
        //instantiate db cursor
        private ApplicationDbContext _context;

        public ReservationDatesController()
        {
            _context = new ApplicationDbContext();
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
        }

        [HttpPost]
        public ActionResult sortIndex()
        {
            TempData["sortBy"] = Request["sortBy"];

            return RedirectToAction("Index");
        }

        // GET: ReservationDates
        public ViewResult Index(int page = 0)
        {
            IEnumerable<Mijem_test_app.Models.ReservationDate> reservations;

            var _sortBy = TempData["sortBy"];

            switch (_sortBy)
            {
                case "dateAscending":
                    reservations = _context.ReservationDates
                        .Where(r => r.Deleted == false)
                        .Include(r => r.Reservation)
                        .Include(r => r.Contact)
                        .OrderBy(r => r.ReservedDate)
                        .ToList();
                    break;

                case "dateDescending":
                    reservations = _context.ReservationDates
                        .Where(r => r.Deleted == false)
                        .Include(r => r.Reservation)
                        .Include(r => r.Contact)
                        .OrderByDescending(r => r.ReservedDate)
                        .ToList();
                    break;
                case "alphaAscending":
                    reservations = _context.ReservationDates
                        .Where(r => r.Deleted == false)
                        .Include(r => r.Reservation)
                        .Include(r => r.Contact)
                        .OrderBy(r => r.Reservation.Name)
                        .ToList();
                    break;

                case "alphaDescending":
                    reservations = _context.ReservationDates
                        .Where(r => r.Deleted == false)
                        .Include(r => r.Reservation)
                        .Include(r => r.Contact)
                        .OrderByDescending(r => r.Reservation.Name)
                        .ToList();
                    break;

                case "ranking":
                    reservations = _context.ReservationDates
                        .Where(r => r.Deleted == false)
                        .Include(r => r.Reservation)
                        .Include(r => r.Contact)
                        .OrderByDescending(r => r.Reservation.Ranking)
                        .ToList();
                    break;

                default:
                    reservations = _context.ReservationDates
                        .Where(r => r.Deleted == false)
                        .Include(r => r.Reservation)
                        .Include(r => r.Contact)
                        .ToList();
                    break;
            }

            const int PageSize = 6;

            var count = reservations.Count();

            var data = reservations.Skip(page * PageSize).Take(PageSize).ToList();

            ViewBag.MaxPage = (count / PageSize) - (count % PageSize == 0 ? 1 : 0);

            ViewBag.Page = page;

            return View(data);
        }

        public ActionResult Proceed()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ConfirmLocation(ReservationDate date)
        {
            TempData["BookDate"] = date.ReservedDate;

            TempData["InfoFromTextBox"] = date.InfoFromTextBox;

            var locations = _context.ReservationDates
                .Include(r => r.Reservation)
                .Include(r => r.Contact)
                .ToList();
            return View(locations);
        }

        [HttpPost]
        public ActionResult Save(ReservationDate reservation)
        {
            var _contactID = (int)TempData["ContactID"];

            var _locationName = Request["locationName"];

            var _contact = _context.Contacts
                .Single(r => r.Id == _contactID);

            var _location = _context.Reservations
                .Single(r => r.Name == _locationName);

            var _ReservedDate = (DateTime)TempData["BookDate"];

            var _InfoFromTextBox = (string)TempData["InfoFromTextBox"];

            var __reservation = new ReservationDate
            {
                ReservedDate = _ReservedDate,
                Contact = _contact,
                Reservation = _location,
                InfoFromTextBox = _InfoFromTextBox,
                Deleted = false
            };
            
            _context.ReservationDates.Add(__reservation);

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Update(ReservationDate reservation)
        {
            var _reservation = _context.ReservationDates
                .Include(r => r.Reservation)
                .Include(r => r.Contact)
                .SingleOrDefault(r => r.Id == reservation.Id);

            _reservation.ReservedDate = reservation.ReservedDate;

            _reservation.InfoFromTextBox = reservation.InfoFromTextBox;

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(ReservationDate reservation)
        {
            var _reservation = _context.ReservationDates
                .Include(r => r.Reservation)
                .Include(r => r.Contact)
                .SingleOrDefault(r => r.Id == reservation.Id);

            _reservation.Deleted = true;

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult NewReservation()
        {
            var _number = (long)TempData["ContactNumber"];

            var _contact = _context.Contacts
                .SingleOrDefault(c => c.ContactNumber == _number);

            TempData["ContactName"] = _contact.Name;

            TempData["ContactType"] = _contact.ContactType;

            TempData["ContactBirthdate"] = _contact.BirthDate.ToShortDateString();

            TempData["ContactID"] = _contact.Id;

            TempData["ContactNumber"] = _contact.ContactNumber;

            return View();
        }

        [HttpPost]
        public ActionResult NewReservation(ReservationDate user)
        {
            var _existingContacts = _context.Contacts.ToList();

            foreach (var contact in _existingContacts)
            {
                if (contact.Id == ViewBag.ContactID)
                {
                    return View(contact);
                }
            }
            _context.ReservationDates.Add(user);

            _context.SaveChanges();

            return View(user);
        }

        [Route("ReservationDates/EditReservation/{id}")]
        public ActionResult EditReservation(int? id)
        {
            
            var reservation = _context.ReservationDates
                .Include(r => r.Reservation)
                .Include(r => r.Contact)
                .SingleOrDefault(r => r.Id == id);
            
            //ensure deleted reservations cannot be accessed
            if (reservation == null || !id.HasValue || reservation.Deleted == true)
                return HttpNotFound();

            return View(reservation);
        }

        [HttpPost]
        public JsonResult Search(string Prefix)
        {
            var contacts = _context.ReservationDates
                .Include(r => r.Reservation)
                .Include(r => r.Contact);

            var Name = (
                from N in contacts
                where N.Contact.Name.StartsWith(Prefix)
                select new { N.Contact.Name }
            );

            return Json(Name, JsonRequestBehavior.AllowGet);
        }
    }
}