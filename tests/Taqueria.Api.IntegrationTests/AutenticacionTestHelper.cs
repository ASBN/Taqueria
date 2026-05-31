using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Taqueria.Api.IntegrationTests;
using Taqueria.Application.DTOs.Auth;

internal static class AutenticacionTestHelper
{
    public static async Task<AuthResponseDto> AutenticarConPasswordOperativaAsync(
        this HttpClient client,
        string usuario,
        string passwordTemporal)
    {
        var passwordOperativa = $"Segura{usuario}123";
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginDto(usuario, passwordTemporal));
        if (!login.IsSuccessStatusCode)
        {
            login = await client.PostAsJsonAsync("/api/auth/login", new LoginDto(usuario, passwordOperativa));
        }

        login.EnsureSuccessStatusCode();
        var auth = (await login.Content.ReadFromJsonAsync<AuthResponseDto>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        if (!auth.Usuario.DebeCambiarPassword)
        {
            return auth;
        }

        var cambio = await client.PostAsJsonAsync(
            "/api/auth/cambiar-password",
            new CambiarPasswordDto(passwordTemporal, passwordOperativa, passwordOperativa));
        cambio.EnsureSuccessStatusCode();
        auth = (await cambio.Content.ReadFromJsonAsync<AuthResponseDto>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        return auth;
    }
}
