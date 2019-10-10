using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using System.Collections.Generic;
using System.Net.Http;
using GreenBookApiTests.DataAccess.Component;
using GreenBook.TestInterfaces;
using RPBooksApiTestingFramework;
using Moq;
using System;
using TestsHelper;

namespace GreenBook.TestControllers
{
	public class PersonTestController : TestBase
	{
		private IPersonServices _personService;
		private ITokenServices _tokenService;
		private IUnitOfWork _unitOfWork;
		private List<Product> _products;
		private List<Token> _tokens;
		private DapperRepository _personRepository;
		private DapperRepository _tokenRepository;
		private HttpClient _client;

		private HttpResponseMessage _response;
		private string _token;
		private string ServiceBaseURL = "";

		public void PersonInitSetup()
		{
			ServiceBaseURL = Properties["hostUrl"];
			//_products = SetUpProducts();
			//_tokens = SetUpTokens();
			//_tokenRepository = SetUpTokenRepository();
			//_personRepository = SetUpProductRepository();
			//var unitOfWork = new Mock<IUnitOfWork>();
			//unitOfWork.SetupGet(s => s.ProductRepository).Returns(_personRepository);
			//unitOfWork.SetupGet(s => s.TokenRepository).Returns(_tokenRepository);
			//_unitOfWork = unitOfWork.Object;
			//_personService = new IPersonServices(_unitOfWork);
			//_tokenService = new ITokenServices(_unitOfWork);
			//_client = new HttpClient { BaseAddress = new Uri(ServiceBaseURL) };
			//var tokenEntity = _tokenService.GenerateToken(1);
			//_token = tokenEntity.AuthToken;
			//_client.DefaultRequestHeaders.Add("Token", _token);
		}

		//private static List<Product> SetUpPersons()
		//{
		//	var prodId = new int();
		//	var products = DataInitializer.GetAllProducts();
		//	foreach (Product prod in products)
		//		prod.ProductId = ++prodId;
		//	return products;
		//}

		//private static List<Token> SetUpTokens()
		//{
		//	var tokId = new int();
		//	var tokens = DataInitializer.GetAllTokens();
		//	foreach (Token tok in tokens)
		//		tok.TokenId = ++tokId;
		//	return tokens;
		//}

		public void DisposeAllObjects()
		{
			_tokenService = null;
			_personService = null;
			_unitOfWork = null;
			_tokenRepository = null;
			_personRepository = null;
			_tokens = null;
			_products = null;
			if (_response != null)
				_response.Dispose();
			if (_client != null)
				_client.Dispose();
		}

		//private GenericRepository<Token> SetUpTokenRepository()
		//{
		//	// Initialise repository
		//	var mockRepo = new Mock<GenericRepository<Token>>(MockBehavior.Default, _dbEntities);

		//	// Setup mocking behavior
		//	mockRepo.Setup(p => p.GetAll()).Returns(_tokens);

		//	mockRepo.Setup(p => p.GetByID(It.IsAny<int>()))
		//	.Returns(new Func<int, Token>(
		//	id => _tokens.Find(p => p.TokenId.Equals(id))));

		//	mockRepo.Setup(p => p.Insert((It.IsAny<Token>())))
		//	.Callback(new Action<Token>(newToken =>
		//	{
		//		//dynamic maxTokenID = _tokens.Last().TokenId;
		//		//dynamic nextTokenID = maxTokenID + 1;
		//		//newToken.TokenId = nextTokenID;
		//		//_tokens.Add(newToken);
		//	}));

		//	mockRepo.Setup(p => p.Update(It.IsAny<Token>()))
		//	.Callback(new Action<Token>(token =>
		//	{
		//		var oldToken = _tokens.Find(a => a.TokenId == token.TokenId);
		//		oldToken = token;
		//	}));

		//	mockRepo.Setup(p => p.Delete(It.IsAny<Token>()))
		//	.Callback(new Action<Token>(prod =>
		//	{
		//		var tokenToRemove =
		//_tokens.Find(a => a.TokenId == prod.TokenId);

		//		if (tokenToRemove != null)
		//			_tokens.Remove(tokenToRemove);
		//	}));

		//	// Return mock implementation object
		//	return mockRepo.Object;
		//}
	}
}
