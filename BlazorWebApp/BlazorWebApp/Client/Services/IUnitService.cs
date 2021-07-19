using BlazorWebApp.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorWebApp.Client.Services
{
    public interface IUnitService
    {
        IList<Unit> Units { get; set; }
        IList<UserUnitResponse> MyUnits { get; set; }
        Task<string> AddUnit(int unitId);
        Task LoadUnitsAsync();
        Task LoadUserUnitsAsync();
    }
}
