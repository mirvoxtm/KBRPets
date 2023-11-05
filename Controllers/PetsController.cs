using KBRPETS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using KBRPETS.Data;
using KBRPETS.Models.Enums;
using KBRPETS.Services;
using Newtonsoft.Json;
using NuGet.Protocol.Plugins;

namespace KBRPETS.Controllers {
    public class PetsController : Controller {
        
        private readonly ILogger<PetsController> _logger;
        private readonly PetsService _petsService;
        private readonly SolicitationsService _solicitationsService;
        private readonly IConfiguration _configuration;

        public PetsController(ILogger<PetsController> logger, PetsService petsService, SolicitationsService solicitationsService , IConfiguration configuration) {
            _logger = logger;
            _petsService = petsService;
            _solicitationsService = solicitationsService;
            _configuration = configuration;
        }


        // GET Index
        public IActionResult Index() {
            return View("index");
        }



        // Pagina de Adocao com paginacao
        public IActionResult QueroAdotar(int page = 1, int pageSize = 7) {
            var listaPets = _petsService.FindAllPets().OrderByDescending(pet => pet.Date);

            var totalCount = listaPets.Count();
            var totalPages = (int) Math.Ceiling((decimal)totalCount / pageSize);

            var petsPerPage = listaPets
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            return View("quero-adotar", petsPerPage);
        }


        // GET da Pagina de Integra
        [Route("Integra/{id}/{name}")]
        public IActionResult Integra(int? id) {

            if (!id.HasValue)
            {
                return Error();
            }

            var pet = _petsService.FindById(id.Value);

            if (pet == null) {
                return Error();
            }

            if (pet.Active == false) {
                return RedirectToAction("QueroAdotar");
            }

            return View("integra", pet);
        }


        // Metodo de Pesquisa com referencia
        // de paginacao
        [HttpPost]
        public IActionResult Search(SpeciesEnum? especie, string raca, string local, PortEnum? porte, GenderEnum? sexo, int page = 1, int pageSize = 7) {
            var pet = _petsService.FindAllPets();
            var petToCompare = _petsService.FindAllPets();

            if (especie.HasValue) {
                pet = pet.Where(x => x.Species == especie);
            }

            if (!string.IsNullOrEmpty(raca)) {
                pet = pet.Where(x => x.Race == raca);
            }

            if (!string.IsNullOrEmpty(local)) {
                pet = pet.Where(x => x.Location.Split(',').ElementAt(1) == local);
            }

            if (porte.HasValue) {
                pet = pet.Where(x => x.Port == porte);
            }

            if (sexo.HasValue) {
                pet = pet.Where(x => x.Gender == sexo);
            }


            var isEqual = pet.OrderBy(p => p.Id).SequenceEqual(petToCompare.OrderBy(p => p.Id));

            // Caso a query de pesquisa seja igual a lista original
            if (isEqual) {
                var totalCount = pet.Count();
                var totalPages = (int) Math.Ceiling((decimal)totalCount / pageSize);

                var petsPerPage = pet
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.TotalPages = totalPages;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;

                return View("quero-adotar", petsPerPage);
            }

            return View("quero-adotar", pet);
        }
        


        // Formulario GET
        public IActionResult Formulario(int? id) {
            if (id == null) {
                return RedirectToAction(nameof(Index));
            }

            var thisPet = _petsService.FindById(id.Value);
            if (thisPet == null) {
                return RedirectToAction(nameof(Index));
            }

            var model = new PetFormViewModel {
                Pet = thisPet,
                Solicitation = new Solicitations { PetId = thisPet.Id.Value }
            };

            return View("formulario", model);
        }


        //Formulario POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Formulario(Solicitations solicitation) {

                var thisPet = _petsService.FindById(solicitation.PetId);

                if (!ModelState.IsValid) {
                    Console.WriteLine("Invalid ModelState");
                    return RedirectToAction(nameof(Formulario));
                }

                // Caso CPF seja invalido
                if (!_solicitationsService.CpfVerifier(solicitation.Cpf))
                {
                    ModelState.AddModelError("Solicitation.Cpf", "Invalid CPF");

                    if (thisPet == null)
                    {
                        Console.WriteLine("Pet not found");
                        return RedirectToAction(nameof(Index));
                    }

                    var model = new PetFormViewModel
                    {
                        Pet = thisPet,
                        Solicitation = new Solicitations { PetId = thisPet.Id.Value }
                    };

                    return View("formulario", model);
                }


                // Caso contrario, mande o Email
                Email email = new Email {
                    Username = solicitation.Name,
                    EmailAdress = solicitation.Email,
                    Subject = "KBRPets - Solicitação de Adoção",
                    Message = $"Olá! Esse é um E-mail de confirmação para a sua solicitação de adoção da KBRPets!\n" +
                              $"Ficamos feliz de ver que você quer adotar nosso querido {thisPet.Name}!\n\n" +
                              $"Iremos analizar sua situação, e te ligaremos assim que possível! \ud83d\ude09\n\n-KBRPets",
                };

                email.OnPost();
                _solicitationsService.Insert(solicitation);
                return RedirectToAction(nameof(Enviado));
        }

        // Retorna página quando solicitação for enviada
        public IActionResult Enviado() {
            return View("enviado");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}