using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorWebApp.Client.Services
{
    public class BananaService : IBananaService
    {
        private readonly HttpClient _http;

        public event Action OnChange;
        public int Bananas { get; set; } = 0;

        public BananaService(HttpClient http)
        {
            _http = http;
        }

        public async Task AddBananas(int amount)
        {
            var result = await _http.PutAsJsonAsync<int>("api/User/addbananas", amount);
            Bananas = await result.Content.ReadFromJsonAsync<int>();
            BananasChanged();
        }        

        public async Task GetBananas()
        {
            Bananas = await _http.GetFromJsonAsync<int>("api/user/getbananas");
            BananasChanged();
        }

        private void BananasChanged() => OnChange.Invoke();
    }
}
