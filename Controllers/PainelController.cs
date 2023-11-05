using System.Diagnostics;
using System.Drawing;
using System.Security.Claims;
using System.Text;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using KBRPETS.Models;
using KBRPETS.Models.Enums;
using KBRPETS.Models.Exceptions;
using KBRPETS.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using Path = System.IO.Path;

namespace KBRPETS.Controllers
{
    public class PainelController : Controller
    {

        private readonly ILogger<PainelController> _logger;
        private readonly UsersService _usersService;
        private readonly SolicitationsService _solicitationsService;
        private readonly PetsService _petsService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PainelController(ILogger<PainelController> logger, UsersService usersService,
            SolicitationsService solicitationsService, PetsService petsService, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _usersService = usersService;
            _solicitationsService = solicitationsService;
            _petsService = petsService;
            _webHostEnvironment = webHostEnvironment;
        }


        // -------------------------------------------------------------------------------------------------------------------


        // Login GET
        public IActionResult Login() {
            return View("login");
        }

        // Login POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(User user)
        {

            // Veja UserService -> LoginVerification
            if (_usersService.LoginVerification(user)) {
                // Criando uma nova Sessão com o ID do Usuário
                HttpContext.Session.SetInt32("userId", _usersService.AssociateId(user)); 
                return RedirectToAction("Painel", "painel");
            }

            else {
                ModelState.AddModelError("", "Email Inválido ou senha inválido!");
                return View("login");
            }
        }


        // -------------------------------------------------------------------------------------------------------------------

        // Painel GET
        [Route("painel/dashboard")]
        public IActionResult Painel(int page = 1, int pageSize = 3) {

            // Recebendo uma nova Sessão com o ID do Usuário
            // e passando os dados do usuário respectivo
            // Desse ID. Esse código é Boilerplate.

            int? id = HttpContext.Session.GetInt32("userId");

            if (id == null) {
                return RedirectToAction(nameof(Login));
            }

            var thisUser = _usersService.FindById(id.Value);
            if (thisUser == null) {
                return RedirectToAction(nameof(Login));
            }

            // Paginação
            var allUsers = _usersService.FindAllUsers();
            var totalUsers = allUsers.Count();
            var totalPages = (int)Math.Ceiling((double)totalUsers / pageSize);

            var viewModel = new PainelViewModel {
                ThisUser = thisUser,
                AllUsers = allUsers.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return View(viewModel);
        }
        
        // Metodo de pesquisa
        [HttpPost]
        public IActionResult Search(string search, ActiveEnum? status, DateTime? de, DateTime? ate, int page = 1, int pageSize = 3) {

            int? idOg = HttpContext.Session.GetInt32("userId");

            if (idOg == null || _usersService.FindById(idOg.Value) == null) {
                return RedirectToAction(nameof(Login));
            }

            var thisUser = _usersService.FindById(idOg.Value);
            if (thisUser == null) {
                return RedirectToAction(nameof(Login));
            }

            //-----------------------------------------------------------------------------

            var users = _usersService.FindAllUsers();
            var usersToCompare = _usersService.FindAllUsers();

            if (!string.IsNullOrEmpty(search)) {
                users = users.FindAll(x => x.Name.Contains(search));
            }

            if (status.HasValue) {
                users = users.FindAll(x => x.Status == status);
            }

            if (de.HasValue) {
                users = users.FindAll(x => x.Date >= de);
            }

            if (ate.HasValue) {
                users = users.FindAll(x => x.Date <= ate);
            }

            // Comparando se a Query de pesquisa é idêntica
            // à lista original instanciada pelo Painel
            var isEqual = users.OrderBy(p => p.Id).SequenceEqual(usersToCompare.OrderBy(p => p.Id));

            // Se for, retorna a view com a paginação correta.
            if (isEqual) {
                var totalCount = users.Count();
                var totalPages = (int) Math.Ceiling((decimal)totalCount / pageSize);
                var usersPerPage = users
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.TotalPages = totalPages;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;

                var equalViewModel = new PainelViewModel
                {
                    ThisUser = thisUser,
                    AllUsers = users,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalPages = totalPages
                };

                return View("Painel", equalViewModel);
            }

            // Caso contrário, retorne os elementos encontrados
            var viewModel = new PainelViewModel
            {
                ThisUser = thisUser,
                AllUsers = users
            };

            return View("Painel", viewModel);
        }



        
        // -------------------------------------------------------------------------------------------------------------------


        // Recuperar GET
        public IActionResult RecuperarSenha() {
            return View("recuperar-senha");
        }

        // Recuperar GET

        public ActionResult RecoverPasswd() {
            return View("recuperar-senha");
        }


        // Recuperar [POST]
        [HttpPost]
        public ActionResult RecoverPasswd(string email) {

            // Caso email fornecido seja vazio, retorne
            if (email == null) {
                return RedirectToAction(nameof(RecoverPasswd));
            }

            // Caso o Email não esteja vazio, encontre sua entrada
            // e pegue seus dados. Neste caso, Password será enviado pelo Email
            var thisUser = _usersService.FindByEmail(email);
            if (thisUser!= null && !string.IsNullOrEmpty(thisUser.Email)) {

                Email emailS = new Email {
                    Username = thisUser.Name,
                    EmailAdress = thisUser.Email,
                    Subject = "KBRPets - Recuperação de Senha",
                    Message = $"Olá! Esse é um E-mail de Recuperação para a sua senha do painel de cadastro da KBRPets!\n" +
                              $"Sua senha é: {thisUser.Password}\n\n" +
                              $"Caso tenha recebido este E-mail por engano, favor ignorar.\n\n-KBRPets",
                };
                   
                emailS.OnPost();
            }

            // Depois disso, retorne a página de Login
            return RedirectToAction("Login", "painel");
        }





        // -------------------------------------------------------------------------------------------------------------------


        // Editar GET
        public IActionResult Editar(int? id) {
            int? idOg = HttpContext.Session.GetInt32("userId");

            if (idOg == null || _usersService.FindById(idOg.Value) == null)
            {
                return RedirectToAction(nameof(Login));
            }

            if (id == null || _usersService.FindById(id.Value) == null)
            {
                return RedirectToAction(nameof(Error), new { message = "Invalid ID." });
            }

            var viewModel = new AdministrationViewModel
            {
                ThisUser = _usersService.FindById(idOg.Value),
                ReferencedUser = _usersService.FindById(id.Value)
            };

            return View(viewModel);
        }


        // Editar POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(AdministrationViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _usersService.FindById(viewModel.ReferencedUser.Id);

            if (user == null)
            {
                return NotFound();
            }

            // Verifica se existe
            var allUsers = _usersService.FindAllUsers();
            var check = allUsers.Where(x => x.Name == user.Name || x.Email == user.Email);


            if (check == null) {
                return RedirectToAction(nameof(Painel));
            }

            user.Name = viewModel.ReferencedUser.Name;
            user.Email = viewModel.ReferencedUser.Email;
            user.Password = viewModel.ReferencedUser.Password;

            _usersService.Update(user);

            return RedirectToAction(nameof(Painel));
        }

        // -------------------------------------------------------------------------------------------------------------------

        // Cadastrar GET
        public IActionResult Cadastrar() {

            int? id = HttpContext.Session.GetInt32("userId");

            if (id == null)
            {
                // Console.WriteLine("Id is null");
                return RedirectToAction(nameof(Login));
            }

            var thisUser = _usersService.FindById(id.Value);
            if (thisUser == null)
            {
                // Console.WriteLine("Id not found");
                return RedirectToAction(nameof(Login));
            }

            var viewModel = new AdministrationViewModel() {
                ThisUser = thisUser,
            };

            return View("cadastrar", viewModel);
        }


        // Cadastrar POST
        // Este método cadastra um novo Admin
        // ou Usuário para adicionar e remover dados
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cadastrar(User user, string passCheck) {
            if (!ModelState.IsValid) {

                int? id = HttpContext.Session.GetInt32("userId");

                if (id == null) {
                    // Console.WriteLine("Id is null");
                    return RedirectToAction(nameof(Login));
                }

                var thisUser = _usersService.FindById(id.Value);
                if (thisUser == null) {
                    // Console.WriteLine("Id not found");
                    return RedirectToAction(nameof(Login));
                }

                var viewModel = new PainelViewModel() {
                    ThisUser = thisUser,
                };

                return RedirectToAction(nameof(Painel), viewModel);
            }

            // Verifique se o Usuário já existe, se sim, retorne.
            var check = _usersService.FindAllUsers().Find(x => x.Name == user.Name || x.Email == user.Email);
            if (check != null) {

                int? id = HttpContext.Session.GetInt32("userId");

                if (id == null) {
                    // Console.WriteLine("Id is null");
                    return RedirectToAction(nameof(Login));
                }

                var thisUser = _usersService.FindById(id.Value);
                if (thisUser == null) {
                    // Console.WriteLine("Id not found");
                    return RedirectToAction(nameof(Login));
                }

                var viewModel = new AdministrationViewModel() {
                    ThisUser = thisUser,
                };

                return RedirectToAction(nameof(Painel), viewModel);
            }


            // Verifica senhas fornecidas nos dois campos
            if (user.Password != passCheck) {
                Console.WriteLine("Senha Diferente");
                int? id = HttpContext.Session.GetInt32("userId");

                if (id == null) {
                    return RedirectToAction(nameof(Login));
                }

                var thisUser = _usersService.FindById(id.Value);
                if (thisUser == null) {
                    return RedirectToAction(nameof(Login));
                }

                var viewModel = new AdministrationViewModel() {
                    ThisUser = thisUser,
                };

                return RedirectToAction(nameof(Painel), viewModel);
            }

            // Deixe o usuario cadastrado ativo
            user.Status = ActiveEnum.ATIVO;

            _usersService.Insert(user);
            return RedirectToAction(nameof(Painel));
        }


        // -------------------------------------------------------------------------------------------------------------------

        public IActionResult Deletar(int? id)
        {

            var user = _usersService.FindById(id.Value);
            if (user == null)
            {
                // Console.WriteLine("Id not found");
                return RedirectToAction(nameof(Login));
            }

            _usersService.Remove(user);
            return RedirectToAction(nameof(Painel));
        }


        // -------------------------------------------------------------------------------------------------------------------

        // Remove sessão atual e redireciona para Login
        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "painel");
        }

        // --------------------------------------------------------------------------------------------


        // Metodo para ver as solicitações de Adocao com paginacao
        [Route("painel/solicitacoes")]
        public IActionResult Solicitacoes(int page = 1, int pageSize = 3) {

            int? id = HttpContext.Session.GetInt32("userId");

            if (id == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var thisUser = _usersService.FindById(id.Value);
            if (thisUser == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var solicitations = _solicitationsService.FindAllSolicitations();
            if (solicitations == null)
            {
                return Error();
            }

            var listAllPets = _petsService.FindAllPets();
            var petNames = listAllPets.ToDictionary(pet => pet.Id, pet => pet.Name);

            // Comparando o PetId de Solicitations com o Id do Pet que existe no Banco.
            // Isso é necessário pois nao foi possivel adicionar uma chave estrangeira usando 
            // da Classe Pet em Solicitations.

            var solicitationPetNames = solicitations.ToDictionary(solicitation => solicitation,
                solicitation => petNames.ContainsKey(solicitation.PetId) ? petNames[solicitation.PetId] : "");

            // Paginacao
            var totalSolicitations = solicitations.Count();
            var totalPages = (int)Math.Ceiling((double)totalSolicitations / pageSize);

            var viewModel = new SolicitationsViewModel {
                ThisUser = thisUser,
                Solicitations = solicitations.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                PetNames = solicitationPetNames,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return View(viewModel);
        }


        // -------------------------------------------------------------------------------------------------------------------

        // Método de pesquisa
        [HttpPost]
        public IActionResult SearchSolicitations(string search, ActiveEnum? status, DateTime? de, DateTime? ate)
        {

            int? idOg = HttpContext.Session.GetInt32("userId");

            if (idOg == null || _usersService.FindById(idOg.Value) == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var thisUser = _usersService.FindById(idOg.Value);
            if (thisUser == null)
            {
                return RedirectToAction(nameof(Login));
            }

            //-----------------------------------------------------------------------------

            var solicitations = _solicitationsService.FindAllSolicitations();

            Console.WriteLine($"Search: {search}, Number of users: {solicitations.Count}");


            if (!string.IsNullOrEmpty(search))
            {
                solicitations = solicitations.FindAll(x => x.Name.Contains(search));
            }

            if (de.HasValue)
            {
                solicitations = solicitations.FindAll(x => x.Date >= de);
            }

            if (ate.HasValue)
            {
                solicitations = solicitations.FindAll(x => x.Date <= ate);
            }

            //---------------------------------------------------------------------------
            
            var listAllPets = _petsService.FindAllPets();
            var petNames = listAllPets.ToDictionary(pet => pet.Id, pet => pet.Name);

            var solicitationPetNames = solicitations.ToDictionary(solicitation => solicitation,
                solicitation => petNames.ContainsKey(solicitation.PetId) ? petNames[solicitation.PetId] : "");


            var viewModel = new SolicitationsViewModel
            {
                ThisUser = thisUser,
                Solicitations = solicitations,
                PetNames = solicitationPetNames
            };

            Console.WriteLine($"Number of users after search: {solicitations.Count}");

            return View("solicitacoes", viewModel);
        }



        // --------------------------------------------------------------------------------------------


        // Pagina de Cadastro CRUD de Pets
        [Route("painel/cadastrar-pets")]
        public IActionResult CadastrarPets(int page = 1, int pageSize = 3)
        {

            int? id = HttpContext.Session.GetInt32("userId");

            if (id == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var thisUser = _usersService.FindById(id.Value);
            if (thisUser == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var findPets = _petsService.FindAllPets().OrderByDescending(pet => pet.Date);
            var totalCount = findPets.Count();
            var totalPages = (int) Math.Ceiling((decimal)totalCount / pageSize);

            var petsPerPage = findPets
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new CadastroPetsViewModel
            {
                ThisUser = thisUser,
                Pets = _petsService.FindAllPets().Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            };

            return View("cadastrar-pet", viewModel);
        }


        // -------------------------------------------------------------------------------------------------------------------

        // Formulario de Insercao de Pets
        public IActionResult InserirPet() {
            int? idOg = HttpContext.Session.GetInt32("userId");

            if (idOg == null) {
                return RedirectToAction(nameof(Login));
            }

            var thisUser = _usersService.FindById(idOg.Value);
            if (thisUser == null) {
                return RedirectToAction(nameof(Login));
            }

            var viewModel = new PetCrudViewModel() {
                ThisUser = thisUser,
                Pet = new Pet()
            };

            return View("inserir-pet", viewModel);
        }


        // Post do Formulario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult InserirPet(PetCrudViewModel model)
        {
            int? idOg = HttpContext.Session.GetInt32("userId");

            if (idOg == null || _usersService.FindById(idOg.Value) == null) {            
                return RedirectToAction(nameof(Login));
            }

            var thisUser = _usersService.FindById(idOg.Value);
            if (thisUser == null) {
                return RedirectToAction(nameof(Login));
            }

            //-----------------------------------------------------------------------------


            if (!ModelState.IsValid)
            {

                if (idOg == null) {
                    return RedirectToAction(nameof(Login));
                }

                thisUser = _usersService.FindById(idOg.Value);
                if (thisUser == null)
                {
                    return RedirectToAction(nameof(Login));
                }

                var viewModel = new PetCrudViewModel() {
                    ThisUser = _usersService.FindById(idOg.Value)
                };

                // Debugging
                // Para cada erro de ModelState, printe no console
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                foreach (var error in errors) {
                    Console.WriteLine(error);
                }

                Console.WriteLine("Model is invalid");
                return RedirectToAction(nameof(InserirPet), viewModel);
            }



            // Instanciacao de arquivo
            string filename = "";
            List<string> filePaths = new List<string>();

            if (model.ImageFileList == null || model.ImageFileList.Count < 5)
            {
                Console.WriteLine("Por favor, forneça pelo menos 5 imagens!");
                return RedirectToAction(nameof(InserirPet));
            }

            if (model.ImageFileList.Any(image => image == null))
            {
                Console.WriteLine("Todas as imagens precisam estar preenchidas!");
                return RedirectToAction(nameof(InserirPet));
            }

            foreach (IFormFile image in model.ImageFileList) {
                if (image == null || image.Length == 0) {

                    int? id = HttpContext.Session.GetInt32("userId");

                    if (id == null) {
                        return RedirectToAction(nameof(Login));
                    }

                    thisUser = _usersService.FindById(id.Value);
                    if (thisUser == null) {
                        return RedirectToAction(nameof(Login));
                    }

                    var viewModel = new PetCrudViewModel() {
                        ThisUser = _usersService.FindById(idOg.Value)
                    };


                    Console.WriteLine("Imagem nula detectada!");
                    return RedirectToAction(nameof(InserirPet), viewModel);
                }

                // Envio de arquivos para wwwroot/img
                string folder = Path.Combine(_webHostEnvironment.WebRootPath, "img");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                filename = Guid.NewGuid().ToString() + "_" + image.FileName;
                if (filename == null)
                {
                    Console.WriteLine("Filename é nulo!");
                    var viewModel = new PetCrudViewModel() {
                        ThisUser = _usersService.FindById(idOg.Value)
                    };

                    Console.WriteLine("Imagem nula detectada!");
                    return RedirectToAction(nameof(InserirPet), viewModel);
                }

                string filepath = Path.Combine(folder, filename);
                if (filepath == null)
                {
                    Console.WriteLine($"Filepath is Null!!");
                    var viewModel = new PetCrudViewModel() {
                        ThisUser = _usersService.FindById(idOg.Value)
                    };

                    Console.WriteLine("Imagem nula detectada!");
                    return RedirectToAction(nameof(InserirPet), viewModel);
                }

                using (var fileStream = new FileStream(filepath, FileMode.Create))
                {
                    image.CopyTo(fileStream);
                }

                if (!System.IO.File.Exists(filepath))
                {
                    Console.WriteLine($"Falha ao copiar o arquivo para {filepath}!");
                    var viewModel = new PetCrudViewModel() {
                        ThisUser = _usersService.FindById(idOg.Value)
                    };

                    Console.WriteLine("Imagem nula detectada!");
                    return RedirectToAction(nameof(InserirPet), viewModel);
                }

                string relativeFilepath = Path.GetRelativePath(_webHostEnvironment.WebRootPath, filepath);
                filePaths.Add(relativeFilepath);
            }


            Pet petToInsert = new Pet
            {
                Name = model.Pet.Name,
                Gender = model.Pet.Gender,
                Species = model.Pet.Species,
                Port = model.Pet.Port,
                Race = model.Pet.Race,
                Weight = model.Pet.Weight,
                Age = model.Pet.Age,
                Description = model.Pet.Description,
                Active = true,
                Date = DateTime.Now,
                // Adicionando os nomes dos arquivos inseridos concatenados com uma virgula
                Images = string.Join(",", filePaths),
                Location = model.Pet.Location
            };

            _petsService.Insert(petToInsert);
            Console.WriteLine("Tudo certo!");

            var AllDoneviewModel = new CadastroPetsViewModel() {
                ThisUser = thisUser,
                PageSize = 3,
                CurrentPage = 1
            };
            return RedirectToAction(nameof(CadastrarPets), AllDoneviewModel);
        }


        // -------------------------------------------------------------------------------------------------------------------

        public IActionResult DeletarPet(int? id)
        {
            var pet = _petsService.FindById(id.Value);
            if (pet == null)
            {
                return RedirectToAction(nameof(CadastrarPets));
            }

            // Remove qualquer solicitacao que tenha como base o animal excluido
            var solicitations = _solicitationsService.FindAllSolicitations().Where(s => s.PetId == id.Value);
            foreach (var solicitation in solicitations) {
                _solicitationsService.Remove(solicitation);
            }

            // Recupere os nomes dos arquivos de imagem e delete
            string[] imagePaths = pet.Images.Split(',');
            foreach (string imagePath in imagePaths) {
                string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, imagePath.Trim());
                FileInfo fileInfo = new FileInfo(fullPath);

                if (fileInfo.Exists)
                {
                    fileInfo.Delete();
                }
            }

            _petsService.Remove(pet);
            return RedirectToAction(nameof(CadastrarPets));
        }


        // -------------------------------------------------------------------------------------------------------------------

        // Editar GET
        public IActionResult EditarPet(int? id) {
            int? idOg = HttpContext.Session.GetInt32("userId");

            if (idOg == null || _usersService.FindById(idOg.Value) == null) {
                return RedirectToAction(nameof(Login));
            }

            if (id == null || _petsService.FindById(id.Value) == null) {
                return RedirectToAction(nameof(Error), new { message = "ID Invalido." });
            }

            var viewModel = new PetCrudViewModel() {
                ThisUser = _usersService.FindById(idOg.Value),
                Pet = _petsService.FindById(id.Value)
            };

            return View("editar-pet", viewModel);
        }

        // Editar POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarPet(PetCrudViewModel model)
        {

            // [!] Pega a Sessão do Usuário
            int? idOg = HttpContext.Session.GetInt32("userId");

            if (idOg == null) {
                return RedirectToAction(nameof(Login));
            }

            var thisUser = _usersService.FindById(idOg.Value);
            if (thisUser == null) {
                return RedirectToAction(nameof(Login));
            }
            //-----------------------------------------------------------------------------

            if (!ModelState.IsValid)
            {

                // [!] Failsafes caso ModelState esteja Inválido

                if (idOg == null) {
                    return RedirectToAction(nameof(Login));
                }

                thisUser = _usersService.FindById(idOg.Value);
                if (thisUser == null) {
                    return RedirectToAction(nameof(Login));
                }
            }

                // [!] Caso nenhuma Imagem tenha sido alterada
                // Faça Update com o model.Pet.Images atual

                if (model.ImageFileList == null || model.ImageFileList.Count < 1)
                {

                    var petToUpdateNoImages = _petsService.FindById(model.Pet.Id.Value);
                    petToUpdateNoImages.Name = model.Pet.Name;
                    petToUpdateNoImages.Gender = model.Pet.Gender;
                    petToUpdateNoImages.Species = model.Pet.Species;
                    petToUpdateNoImages.Port = model.Pet.Port;
                    petToUpdateNoImages.Race = model.Pet.Race;
                    petToUpdateNoImages.Weight = model.Pet.Weight;
                    petToUpdateNoImages.Age = model.Pet.Age;
                    petToUpdateNoImages.Description = model.Pet.Description;
                    petToUpdateNoImages.Date = DateTime.Now;
                    petToUpdateNoImages.Active = model.Pet.Active;
                    petToUpdateNoImages.Images = model.Pet.Images;
                    petToUpdateNoImages.Location = model.Pet.Location;

                    _petsService.Update(petToUpdateNoImages);
                    Console.WriteLine("Tudo certo!");
                    return RedirectToAction(nameof(CadastrarPets));
                }



                // [!] Instanciando Arquivo

                string filename = "";
                List<string> filePaths = new List<string>();

                foreach (IFormFile image in model.ImageFileList)
                {

                    // [!] Failsafes

                    if (image == null || image.Length == 0) {

                        int? id = HttpContext.Session.GetInt32("userId");

                        if (id == null) {
                            return RedirectToAction(nameof(Login));
                        }

                        thisUser = _usersService.FindById(id.Value);
                        if (thisUser == null) {
                            return RedirectToAction(nameof(Login));
                        }

                        var errorViewModel = new PetCrudViewModel() {
                            ThisUser = thisUser,
                            Pet = _petsService.FindById(model.Pet.Id.Value)
                        };

                        Console.WriteLine("Imagem nula detectada!");
                        return RedirectToAction(nameof(InserirPet), errorViewModel);
                    }

                    // [!] Associando caminho do arquivo

                    string folder = Path.Combine(_webHostEnvironment.WebRootPath, "img");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    filename = Guid.NewGuid().ToString() + "_" + image.FileName;
                    if (filename == null)
                    {
                        Console.WriteLine($"Filename is Null!!");
                        return RedirectToAction(nameof(InserirPet));
                    }

                    string filepath = Path.Combine(folder, filename);
                    if (filepath == null)
                    {
                        Console.WriteLine($"Filepath is Null!!");
                        return RedirectToAction(nameof(InserirPet));
                    }

                    using (var fileStream = new FileStream(filepath, FileMode.Create))
                    {
                        image.CopyTo(fileStream);
                    }

                    if (!System.IO.File.Exists(filepath))
                    {
                        Console.WriteLine($"Falha ao copiar o arquivo para {filepath}!");
                        var ioErrorviewModel = new PetCrudViewModel() {
                            ThisUser = thisUser,
                            Pet = _petsService.FindById(model.Pet.Id.Value)
                        };
                        return RedirectToAction(nameof(InserirPet), ioErrorviewModel);
                    }

                    string relativeFilepath = Path.GetRelativePath(_webHostEnvironment.WebRootPath, filepath);
                    filePaths.Add(relativeFilepath);
                }

                // [!] Criando uma lista das imagens anteriores e das imagens
                // novas para comparação
                List<string> oldImages = model.Pet.Images.Split(',').ToList();
                List<string> newImages = filePaths;

                // Adicionando as imagens antigas para o vetor
                // das novas imagens caso menos que 5 e mais que 1 imagem
                // tenha sido alterada
                foreach (string oldImage in oldImages)
                {
                    if (!newImages.Contains(oldImage))
                    {
                        newImages.Add(oldImage);
                    }
                }

                // Caso todas as imagens tenham sido alteradas
                // Elimine os arquivos de imagem anterior

                if (model.ImageFileList.Count == oldImages.Count)
                {
                    foreach (string oldImage in oldImages)
                    {
                        string oldfullPath = Path.Combine(_webHostEnvironment.WebRootPath, oldImage.Trim());
                        FileInfo fileInfo = new FileInfo(oldfullPath);

                        if (fileInfo.Exists)
                        {
                            fileInfo.Delete();
                        }
                    }

                    var petToUpdateAllImages = _petsService.FindById(model.Pet.Id.Value);
                    petToUpdateAllImages.Name = model.Pet.Name;
                    petToUpdateAllImages.Gender = model.Pet.Gender;
                    petToUpdateAllImages.Species = model.Pet.Species;
                    petToUpdateAllImages.Port = model.Pet.Port;
                    petToUpdateAllImages.Race = model.Pet.Race;
                    petToUpdateAllImages.Weight = model.Pet.Weight;
                    petToUpdateAllImages.Age = model.Pet.Age;
                    petToUpdateAllImages.Description = model.Pet.Description;
                    petToUpdateAllImages.Date = DateTime.Now;
                    petToUpdateAllImages.Active = model.Pet.Active;
                    petToUpdateAllImages.Images = string.Join(",", newImages);
                    petToUpdateAllImages.Location = model.Pet.Location;

                    _petsService.Update(petToUpdateAllImages);
                    Console.WriteLine("Tudo certo!");

                    var allImagesviewModel = new PetCrudViewModel() {
                        ThisUser = thisUser,
                    };
                    return RedirectToAction(nameof(CadastrarPets), allImagesviewModel);
                }

                // [!] Caso contrário, faça update com as
                // imagens antigas e novas juntas

                var petToUpdate = _petsService.FindById(model.Pet.Id.Value);

                petToUpdate.Name = model.Pet.Name;
                petToUpdate.Gender = model.Pet.Gender;
                petToUpdate.Species = model.Pet.Species;
                petToUpdate.Port = model.Pet.Port;
                petToUpdate.Race = model.Pet.Race;
                petToUpdate.Weight = model.Pet.Weight;
                petToUpdate.Age = model.Pet.Age;
                petToUpdate.Description = model.Pet.Description;
                petToUpdate.Date = DateTime.Now;
                petToUpdate.Active = model.Pet.Active;
                petToUpdate.Images = string.Join(",", newImages);
                petToUpdate.Location = model.Pet.Location;

                _petsService.Update(petToUpdate);
                Console.WriteLine("Tudo certo!");

                var viewModel = new PetCrudViewModel() {
                    ThisUser = thisUser,
                };
                return RedirectToAction(nameof(CadastrarPets), viewModel);
            }



            // Gerando um Arquivo CSV
            public string GenerateCsv(List<User> users)
            {
                var builder = new StringBuilder();
                builder.AppendLine("Id,Name,Email");
                foreach (var user in users)
                {
                    builder.AppendLine($"{user.Id},{user.Name},{user.Email}");
                }
                return builder.ToString();
            }

            // Exportando todos os usuarios para CSV
            public IActionResult ExportAllUsersToCSV() {
                var users = _usersService.FindAllUsers(); 
                var csvContent = GenerateCsv(users);
                return File(Encoding.UTF8.GetBytes(csvContent), "text/csv", "AllUsers.csv");
            }


            // Gerando um PDF com base no pacote NuGet
            public byte[] GeneratePdf(List<User> users)
            {
                using var memoryStream = new MemoryStream();
                using var writer = new PdfWriter(memoryStream);
                using var pdf = new PdfDocument(writer);
                using var document = new Document(pdf);

                var table = new Table(3);
                table.AddHeaderCell("Id");
                table.AddHeaderCell("Name");
                table.AddHeaderCell("Email");

                foreach (var user in users)
                {
                    table.AddCell(user.Id.ToString());
                    table.AddCell(user.Name);
                    table.AddCell(user.Email);
                }

                document.Add(table);
                document.Close();

                return memoryStream.ToArray();
            }

            [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
            public IActionResult Error() {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }