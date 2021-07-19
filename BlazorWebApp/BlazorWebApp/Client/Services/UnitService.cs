using Blazored.Toast.Services;
using BlazorWebApp.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorWebApp.Client.Services
{
    public class UnitService : IUnitService
    {
        private readonly IToastService _itoastService;
        private readonly HttpClient _http;
        private readonly IBananaService _bananaService;

        public UnitService(IToastService toastService, HttpClient http, IBananaService bananaService)
        {
            _itoastService = toastService;
            _http = http;
            _bananaService = bananaService;
        }

        public IList<Unit> Units { get; set; } = new List<Unit>();

        public IList<UserUnitResponse> MyUnits { get; set; } = new List<UserUnitResponse>();

        public async Task<string> AddUnit(int unitId)
        {
            var unit = Units.First(unit => unit.Id == unitId);
            var result = await _http.PostAsJsonAsync<int>("api/userunit", unitId);
            if(result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                await _bananaService.GetBananas();
                _itoastService.ShowSuccess($"Your {unit.Title} has been built!", "Unit built!");
                return "success";

                //Console.WriteLine($"{unit.Title} was built!");
                //Console.WriteLine($"Your army size: {MyUnits.Count}");
            }
            else
            {
                _itoastService.ShowError(await result.Content.ReadAsStringAsync());
                return "failure";
            }
        }

        public async Task LoadUnitsAsync()
        {
            if (Units.Count == 0)
            {
                Units = await _http.GetFromJsonAsync<IList<Unit>>("api/Unit");
            }
        }

        public async Task LoadUserUnitsAsync()
        {
            MyUnits = await _http.GetFromJsonAsync<IList<UserUnitResponse>>("api/userunit");
        }
    }
}
