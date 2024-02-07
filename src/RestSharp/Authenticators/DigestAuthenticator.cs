//   Copyright (c) .NET Foundation and Contributors
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RestSharp.Authenticators;

/// <summary>
/// Allows "digest access authentication" for HTTP requests.
/// </summary>
/// <remarks>
/// Encoding can be specified depending on what your server expect (see https://stackoverflow.com/a/7243567).
/// UTF-8 is used by default but some servers might expect ISO-8859-1 encoding.
/// </remarks>
[PublicAPI]
public class DigestAuthenticator : IAuthenticator
{
    private string _username;
    private string _password;
    private string _realm;
    private string _nonce;
    private string _qop;
    private string _opaque;
    public DigestAuthenticator(string username, string password)
    {
        _username = username;
        _password = password;
    }
    public ValueTask Authenticate(IRestClient client, RestRequest request)
    {
        if (string.IsNullOrEmpty(_realm) || string.IsNullOrEmpty(_nonce) || string.IsNullOrEmpty(_qop))
        {
            try
            {
                FetchAuthenticationInfo(client, request);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                if (client.Options.ThrowOnAnyError)
                    throw ex;
                return new ValueTask(Task.FromResult(new HeaderParameter(KnownHeaders.Authorization, "")));
            }
        }
        string authorizationHeader = GenerateDigestAuthorization(request.Method.ToString(), client.Options.BaseUrl.ToString(), _username, _password, _realm, _nonce, _opaque);
        _realm = "";
        _nonce = "";
        _qop = "";
        return new ValueTask(Task.FromResult(request.AddOrUpdateParameter(new HeaderParameter(KnownHeaders.Authorization, authorizationHeader))));
    }
    private void FetchAuthenticationInfo(IRestClient client, RestRequest request)
    {
        RestClient headClient = new RestClient(new RestClientOptions(client.Options.BaseUrl.ToString())
        {
            MaxTimeout = Convert.ToInt32(TimeSpan.FromSeconds(10).TotalMilliseconds)
        });
        RestRequest headRequest = new RestRequest(client.BuildUri(request).PathAndQuery, request.Method);
        headRequest.Timeout = Convert.ToInt32(TimeSpan.FromSeconds(10).TotalMilliseconds);
        RestResponse headResponse = headClient.Execute(headRequest);
        if (headResponse.StatusCode == HttpStatusCode.Unauthorized)
        {
            var wwwAuthenticateHeader = headResponse.Headers.FirstOrDefault(h => h.Name == "WWW-Authenticate")?.Value.ToString();
            if (!string.IsNullOrEmpty(wwwAuthenticateHeader))
                ParseAuthenticationInfo(wwwAuthenticateHeader);
        }
        else
        {
            if (headResponse.StatusCode == HttpStatusCode.RequestTimeout)
                throw new Exception("The request timed out.");
            else
                throw new Exception("The server did not respond with a valid WWW-Authenticate in the header.");
        }
    }
    private void ParseAuthenticationInfo(string wwwAuthenticateHeader)
    {
        // Parse the WWW-Authenticate header and extract the necessary information (realm, nonce, qop, etc.)
        // You'll need to implement the parsing logic according to the structure of the header.
        // This will depend on the server's implementation of Digest authentication.
        // Example parsing code for demonstration purposes:
        // The parsing logic here assumes a specific format for the header. You should adjust this to match your server's format.
        var valores = ExtrairValoresPropriedades(wwwAuthenticateHeader);
        valores.TryGetValue("realm", out _realm);
        valores.TryGetValue("nonce", out _nonce);
        valores.TryGetValue("qop", out _qop);
        valores.TryGetValue("opaque", out _opaque);
    }
    public static Dictionary<string, string> ExtrairValoresPropriedades(string texto)
    {
        // Define o padrão da regex para encontrar os valores entre as aspas
        string padraoRegex = @"(\w+)=""([^""]+)""";
        var regex = new Regex(padraoRegex);
        // Cria um dicionário para armazenar os pares chave-valor
        var valores = new Dictionary<string, string>();
        // Encontra todas as correspondências na string
        var correspondencias = regex.Matches(texto);
        // Itera sobre as correspondências e adiciona ao dicionário
        foreach (Match correspondencia in correspondencias)
        {
            if (correspondencia.Success)
            {
                string chave = correspondencia.Groups[1].Value;
                string valor = correspondencia.Groups[2].Value;
                valores[chave] = valor;
            }
        }
        return valores;
    }
    private string GetValueFromHeader(string header, string key)
    {
        string searchKey = key + "=";
        int startIndex = header.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase);
        if (startIndex == -1)
            return null;
        startIndex += searchKey.Length;
        int endIndex = header.IndexOf('"', startIndex);
        if (endIndex == -1)
            return null;
        return header.Substring(startIndex, endIndex - startIndex);
    }
    private string GenerateDigestAuthorization(
        string method,
        string uri,
        string username,
        string password,
        string realm,
        string nonce,
        string opaque,
        string qop = "auth",
        string algorithm = "MD5")
    {
        string cnonce = Guid.NewGuid().ToString("N").Substring(0, 8).ToLower();
        string A1 = $"{username}:{realm}:{password}";
        string H_A1 = CalculateMD5Hash(A1);
        string A2 = $"{method.ToUpper()}:{uri}";
        string H_A2 = CalculateMD5Hash(A2);
        string request_digest = gerarRequestDigest(H_A1, nonce, "00000001", cnonce, qop, H_A2);
        string authorizationHeader = $"Digest username=\"{username}\", " +
            $"realm=\"{realm}\", " +
            $"nonce=\"{nonce}\", " +
            $"uri=\"{uri}\", " +
            $"algorithm=\"{algorithm}\", " +
            $"qop={qop}, " +
            $"nc=00000001, " +
            $"cnonce=\"{cnonce}\", " +
            $"response=\"{request_digest}\", " +
            $"opaque=\"{opaque}\"";
        return authorizationHeader;
    }
    private string CalculateMD5Hash(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
    private string gerarRequestDigest(string H_A1, string nonce, string nc, string cnonce, string qop, string H_A2)
    {
        string combined = "";
        if (qop == "auth")
            combined = $"{H_A1}:{nonce}:{nc}:{cnonce}:{qop}:{H_A2}";
        else
            combined = $"{H_A1}:{nonce}:{H_A2}";
        return CalculateMD5Hash(combined);
    }
}
