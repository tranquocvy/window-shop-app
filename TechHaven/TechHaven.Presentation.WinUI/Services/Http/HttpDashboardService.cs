using System;
using System.Net.Http;
using System.Threading.Tasks;
using TechHaven.Presentation.WinUI.Helpers;
using TechHaven.Presentation.WinUI.Services.Interfaces;
using TechHaven.Shared.DTOs.Common;
using TechHaven.Shared.DTOs.Dashboard;

namespace TechHaven.Presentation.WinUI.Services.Http
{
    public class HttpDashboardService : IDashboardService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/Dashboard";

        public HttpDashboardService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public Task<ResponseWrapper<DashboardDto>> GetDashboardAsync()
        {
            return _httpClient.GetWrapperFromJsonAsync<DashboardDto>(BaseUrl, "Failed to retrieve dashboard");
        }
    }
}
