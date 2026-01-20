using HtmlAgilityPack;
using backend.Dtos;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace backend.Services
{
    public class BlackboardService : IBlackboardService
    {
        private const string BASE_URL = "https://aulavirtual.ual.es";
        private const string LOGIN_PATH = "/webapps/login/";
        private const string API_ME_URL = "/learn/api/public/v1/users/me";

        public async Task<LoginResponseDto> AuthenticateAsync(string username, string password)
        {
            var handler = new HttpClientHandler
            {
                UseCookies = false,
                AllowAutoRedirect = false,
                SslProtocols = System.Security.Authentication.SslProtocols.Tls12
            };

            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri(BASE_URL);

                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.Add("Accept", "*/*");
                client.DefaultRequestHeaders.ExpectContinue = false;

                var requestGet = new HttpRequestMessage(HttpMethod.Get, LOGIN_PATH);
                var responseGet = await client.SendAsync(requestGet);
                var htmlGet = await responseGet.Content.ReadAsStringAsync();

                if (!responseGet.Headers.TryGetValues("Set-Cookie", out var cookieHeaders))
                    return new LoginResponseDto { IsSuccess = false, Message = "Error: Sin cookies iniciales." };

                var cookieList = new List<string>();
                foreach (var header in cookieHeaders) cookieList.Add(header.Split(';')[0]);
                string manualCookieHeader = string.Join("; ", cookieList);

                var doc = new HtmlDocument();
                doc.LoadHtml(htmlGet);
                var nonceNode = doc.DocumentNode.SelectSingleNode("//input[@name='blackboard.platform.security.NonceUtil.nonce.ajax']");
                var nonceValue = nonceNode?.GetAttributeValue("value", "");

                if (string.IsNullOrEmpty(nonceValue))
                    return new LoginResponseDto { IsSuccess = false, Message = "Error: Sin Nonce." };

                string encodedUser = Uri.EscapeDataString(username);
                string encodedPass = Uri.EscapeDataString(password);
                string encodedNonce = Uri.EscapeDataString(nonceValue);

                string payload = $"user_id={encodedUser}&password={encodedPass}&action=login&new_loc=&blackboard.platform.security.NonceUtil.nonce.ajax={encodedNonce}";
                var content = new StringContent(payload, Encoding.UTF8, "application/x-www-form-urlencoded");

                var requestPost = new HttpRequestMessage(HttpMethod.Post, LOGIN_PATH)
                {
                    Content = content
                };

                requestPost.Headers.Add("Cookie", manualCookieHeader);
                requestPost.Headers.Referrer = new Uri(BASE_URL + LOGIN_PATH);
                requestPost.Headers.Add("Origin", BASE_URL);

                var responsePost = await client.SendAsync(requestPost);

                if (responsePost.StatusCode == HttpStatusCode.Found || responsePost.StatusCode == HttpStatusCode.Redirect || responsePost.StatusCode == HttpStatusCode.SeeOther)
                {
                    string finalCookies = manualCookieHeader;
                    if (responsePost.Headers.TryGetValues("Set-Cookie", out var newCookieHeaders))
                    {
                        var newCookies = new List<string>();
                        foreach (var h in newCookieHeaders) newCookies.Add(h.Split(';')[0]);
                        finalCookies = finalCookies + "; " + string.Join("; ", newCookies);
                    }

                    return new LoginResponseDto
                    {
                        IsSuccess = true,
                        Message = "Login Exitoso",
                        SessionCookie = finalCookies // Save the session cookies at the frontend
                    };
                }
                else
                {
                    var errorHtml = await responsePost.Content.ReadAsStringAsync();
                    string cleanHtml = Regex.Replace(errorHtml, "<noscript>.*?</noscript>", "", RegexOptions.Singleline);
                    return new LoginResponseDto { IsSuccess = false, Message = "Fallo en login. Revise contraseña." };
                }
            }
        }

        public async Task<UserResponseDto> GetUserDataAsync(string sessionCookie)
        {
            if (string.IsNullOrEmpty(sessionCookie))
                return new UserResponseDto { IsSuccess = false, Message = "Session cookie faltante." };

            var handler = new HttpClientHandler
            {
                UseCookies = false,
                AllowAutoRedirect = false,
                SslProtocols = System.Security.Authentication.SslProtocols.Tls12
            };

            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri(BASE_URL);
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.Add("Accept", "*/*");
                client.DefaultRequestHeaders.ExpectContinue = false;

                var requestApi = new HttpRequestMessage(HttpMethod.Get, API_ME_URL);
                requestApi.Headers.Add("Cookie", sessionCookie);

                var responseApi = await client.SendAsync(requestApi);

                if (responseApi.IsSuccessStatusCode)
                {
                    var jsonApi = await responseApi.Content.ReadAsStringAsync();

                    var root = JsonNode.Parse(jsonApi);

                    var flatUser = new UserDetailDto
                    {
                        Given = root?["name"]?["given"]?.ToString() ?? string.Empty,
                        Family = root?["name"]?["family"]?.ToString() ?? string.Empty,
                        Email = root?["contact"]?["email"]?.ToString() ?? string.Empty,
                        Avatar = root?["avatar"]?["viewUrl"]?.ToString() ?? string.Empty
                    };

                    return new UserResponseDto
                    {
                        IsSuccess = true,
                        Message = "OK",
                        UserData = flatUser
                    };
                }
                else
                {
                    return new UserResponseDto { IsSuccess = false, Message = $"API returned {(int)responseApi.StatusCode}" };
                }
            }
        }
    }
}