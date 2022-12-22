﻿using System.Security.Claims;
using BookCommerceCustom1.Data;
using BookCommerceCustom1.Helpers;
using BookCommerceCustom1.Models;
using BookCommerceCustom1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe.Checkout;

namespace BookCommerceCustom1.Controllers
{
	[Authorize]
	public class ShportaController : Controller
	{
		private readonly Konteksti _konteksti;
		[BindProperty]
		public ShportaViewModel ShportaViewModel { get; set; }

		public ShportaController(Konteksti konteksti)
		{
			_konteksti = konteksti;
		}
		public IActionResult Listo()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
			ShportaViewModel = new ShportaViewModel
			{
				Shportat = _konteksti.Shportat.
					Include(x=>x.Produkti)
					.Where(x=>x.PerdorusiId==claim.Value),
				Porosia = new Porosia()
			};

			foreach (var shporta in ShportaViewModel.Shportat)
			{
				shporta.Cmimi = KalkuloCmiminSipasSasise(shporta.Sasia, shporta.Produkti.Cmimi,
					shporta.Produkti.Cmimi50, shporta.Produkti.Cmimi100);
				ShportaViewModel.Porosia.Totali = (shporta.Cmimi * shporta.Sasia);
			}

			return View(ShportaViewModel);
		}
		public IActionResult Permbledhje()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
			ShportaViewModel = new ShportaViewModel
			{
				Shportat = _konteksti.Shportat.
					Include(x => x.Produkti)
					.Where(x => x.PerdorusiId == claim.Value),
				Porosia = new Porosia()
			};
			ShportaViewModel.Porosia.Perdorusi = _konteksti.Perdorusit
				.FirstOrDefault(x => x.Id == claim.Value);
			ShportaViewModel.Porosia.Emri = ShportaViewModel.Porosia.Perdorusi.Emri;
			ShportaViewModel.Porosia.NumriITelefonit = ShportaViewModel.Porosia.Perdorusi.PhoneNumber;
			ShportaViewModel.Porosia.Rruga = ShportaViewModel.Porosia.Perdorusi.Rruga;
			ShportaViewModel.Porosia.Qyteti = ShportaViewModel.Porosia.Perdorusi.Qyteti;
			ShportaViewModel.Porosia.KodiPostal = ShportaViewModel.Porosia.Perdorusi.KodiPostal;
			ShportaViewModel.Porosia.Shteti = ShportaViewModel.Porosia.Perdorusi.Shteti;



			foreach (var shporta in ShportaViewModel.Shportat)
			{
				shporta.Cmimi = KalkuloCmiminSipasSasise(shporta.Sasia, shporta.Produkti.Cmimi,
					shporta.Produkti.Cmimi50, shporta.Produkti.Cmimi100);
				ShportaViewModel.Porosia.Totali += (shporta.Cmimi * shporta.Sasia);
			}

			return View(ShportaViewModel);
		}
		[HttpPost,ActionName("Permbledhje")]
		[ValidateAntiForgeryToken]
		public IActionResult PermbledhjePost()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
			ShportaViewModel.Shportat = _konteksti.Shportat.Include(x => x.Produkti)
				.Where(x => x.PerdorusiId == claim.Value).ToList();

			ShportaViewModel.Porosia.StatusiIPageses = Sd.StatusIPagesesNePritje;
			ShportaViewModel.Porosia.StatusiIPorosise = Sd.StatusNePritje;
			ShportaViewModel.Porosia.DataEPorosise = System.DateTime.Now;
			ShportaViewModel.Porosia.PerdorusiId = claim.Value;

			foreach (var shporta in ShportaViewModel.Shportat)
			{
				shporta.Cmimi = KalkuloCmiminSipasSasise(shporta.Sasia, shporta.Produkti.Cmimi,
					shporta.Produkti.Cmimi50, shporta.Produkti.Cmimi100);
				ShportaViewModel.Porosia.Totali += (shporta.Cmimi * shporta.Sasia);
			}

			_konteksti.Porosite.Add(ShportaViewModel.Porosia);
			_konteksti.SaveChanges();

			foreach (var shporta in ShportaViewModel.Shportat)
			{
				PorosiDetali porosiDetali = new PorosiDetali
				{
					ProduktiId = shporta.ProduktiId,
					PorosiaId = ShportaViewModel.Porosia.Id,
					Cmimi = shporta.Cmimi,
					Sasia = shporta.Sasia
				};
				_konteksti.PorosiDetalet.Add(porosiDetali);
				_konteksti.SaveChanges();
			}

			//Stripe
			var domain = "https://localhost:7032/";
			var options = new SessionCreateOptions
			{
				LineItems = new List<SessionLineItemOptions>()
				,
				Mode = "payment",
				SuccessUrl = domain+$"Shporta/KonfirmimiIPorosise?id={ShportaViewModel.Porosia.Id}" ,
				CancelUrl = domain+$"Shporta/Listo"
			};

			foreach (var shporta in ShportaViewModel.Shportat)
			{
				{
					var sessionLineItem = new SessionLineItemOptions
					{
						PriceData = new SessionLineItemPriceDataOptions
						{
							UnitAmount = (long?)shporta.Cmimi * 100,
							Currency = "eur",
							ProductData = new SessionLineItemPriceDataProductDataOptions
							{
								Name = shporta.Produkti.Emri,
							},
						},
						Quantity = shporta.Sasia,
					};
					options.LineItems.Add(sessionLineItem);

				}
			}


			var service = new SessionService();
			Session session = service.Create(options);
			ShportaViewModel.Porosia.SessionId = session.Id;
			ShportaViewModel.Porosia.PaymentIntentId=session.PaymentIntentId;
			_konteksti.SaveChanges();
			Response.Headers.Add("Location", session.Url);
			return new StatusCodeResult(303);

			//Stripe

			// _konteksti.Shportat.RemoveRange(ShportaViewModel.Shportat);
			// _konteksti.SaveChanges();
			// return RedirectToAction("Index", "Home");
		}

		public IActionResult KonfirmimiIPorosise(int id)
		{
			var porosia = _konteksti.Porosite.FirstOrDefault(x => x.Id == id);

			var service = new SessionService();
			Session session = service.Get(porosia.SessionId);
			if (session.PaymentStatus.ToLower()=="paid")
			{
				porosia.StatusiIPorosise = Sd.StatusIAprovuar;
				porosia.StatusiIPageses = Sd.StatusIPagesesIAprovuar;
				_konteksti.SaveChanges();
			}
			List<Shporta> shportat = _konteksti.Shportat.Where(x => x.PerdorusiId == porosia.PerdorusiId).ToList();
			_konteksti.Shportat.RemoveRange(shportat);
			_konteksti.SaveChanges();
			return View(id);
		}
		public IActionResult Plus(int shportaId)
		{
			var shporta = _konteksti.Shportat.FirstOrDefault(x => x.Id == shportaId);
			shporta.Sasia += 1;
			_konteksti.SaveChanges();
			return RedirectToAction(nameof(Listo));
		}
		public IActionResult Minus(int shportaId)
		{
			var shporta = _konteksti.Shportat.FirstOrDefault(x => x.Id == shportaId);
			if (shporta.Sasia<=1)
			{
				_konteksti.Shportat.Remove(shporta);
			}
			else
			{
				shporta.Sasia -= 1;
			}
			_konteksti.SaveChanges();
			return RedirectToAction(nameof(Listo));
		}
		public IActionResult Largo(int shportaId)
		{
			var shporta = _konteksti.Shportat.FirstOrDefault(x => x.Id == shportaId);
			_konteksti.Shportat.Remove(shporta);
			_konteksti.SaveChanges();
			return RedirectToAction(nameof(Listo));
		}

		private double KalkuloCmiminSipasSasise(double sasia, double cmimi, double cmimi50, double cmimi100)
		{
			if (sasia<=50)
			{
				return cmimi;
			}
			if (sasia<=100)
			{
				return cmimi50;
			}
			return cmimi100;
		}
	}
}
