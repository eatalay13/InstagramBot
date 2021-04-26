using AutoMapper;
using BlazorUI.Server.Services;
using BlazorUI.Shared.Models;
using InstagramApiSharp.Classes.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorUI.Server.Controllers
{
    [ApiController]
    [Route("api/{controller}/{action}")]
    public class InstagramController : ControllerBase
    {
        private readonly InstagramService _instagramService;
        private readonly IMapper _mapper;

        public InstagramController(InstagramService instagramService, IMapper mapper)
        {
            _instagramService = instagramService;
            _instagramService.LoginAsync("viphopsy", "Embe3413+").Wait();
            _mapper = mapper;
        }

        public async Task<List<CurrentUser>> GetCurrentUser()
        {
            var user = await _instagramService.GetCurrentUserAsync();
            return _mapper.Map<List<CurrentUser>>(user);
        }

        public async Task<List<CurrentUser>> SearchUser(string query)
        {
            var user = await _instagramService.SearchUser(query);
            return _mapper.Map<List<CurrentUser>>(user);
        }
    }
}
