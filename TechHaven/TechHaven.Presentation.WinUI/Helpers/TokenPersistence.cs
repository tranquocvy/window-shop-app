using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TechHaven.Shared.DTOs.Auth;
using TechHaven.Shared.DTOs.Common;
using Windows.Security.Credentials;
using TechHaven.Shared.DTOs.Users;
using TechHaven.Presentation.WinUI.Helpers;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace TechHaven.Presentation.WinUI.Helpers
{
    public static class TokenPersistence
    {
        private const string ResourceName = "TechHaven.RefreshToken";
        private const string EncryptedDbConfigResourceName = "TechHaven.EncryptedDbConfig";

        // Toggle for using mock restore flow (useful for offline testing)
        public static bool UseMock { get; set; } = false;

        public static void SaveRefreshToken(string refreshToken, string? encryptedDbConfig = null, bool clearDbConfigIfNull = false)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                Debug.WriteLine("SaveRefreshToken: called with empty token");
                return;
            }
            try
            {
                var vault = new PasswordVault();
                
                // Remove existing refresh token entries
                try
                {
                    var existing = vault.RetrieveAll().Where(c => c.Resource == ResourceName).ToList();
                    foreach (var e in existing)
                        vault.Remove(e);
                }
                catch (Exception exExisting)
                {
                    Debug.WriteLine($"SaveRefreshToken: error removing existing entries: {exExisting.Message}");
                }

                // Save refresh token
                var cred = new PasswordCredential(ResourceName, Environment.MachineName, refreshToken);
                vault.Add(cred);
                Debug.WriteLine($"SaveRefreshToken: saved refresh token (length={refreshToken.Length}) to PasswordVault");

                // Save encrypted DB config if provided
                if (!string.IsNullOrWhiteSpace(encryptedDbConfig))
                {
                    try
                    {
                        // Remove existing DB config entries
                        var existingDbConfig = vault.RetrieveAll().Where(c => c.Resource == EncryptedDbConfigResourceName).ToList();
                        foreach (var e in existingDbConfig)
                            vault.Remove(e);
                    }
                    catch { }

                    var dbConfigCred = new PasswordCredential(EncryptedDbConfigResourceName, Environment.MachineName, encryptedDbConfig);
                    vault.Add(dbConfigCred);
                    Debug.WriteLine($"SaveRefreshToken: saved encrypted DB config (length={encryptedDbConfig.Length}) to PasswordVault");
                }
                else if (clearDbConfigIfNull)
                {
                    // Only remove DB config if explicitly requested (e.g., switching from external to standard login)
                    RemoveEncryptedDbConfig();
                    Debug.WriteLine("SaveRefreshToken: cleared DB config as requested");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SaveRefreshToken: failed to save refresh token: {ex.Message}");
            }
        }

        public static string? GetRefreshToken()
        {
            try
            {
                var vault = new PasswordVault();
                var list = vault.RetrieveAll();
                var entry = list.FirstOrDefault(x => x.Resource == ResourceName);
                if (entry == null)
                {
                    Debug.WriteLine("GetRefreshToken: no entry found in PasswordVault");
                    return null;
                }
                
                var cred = vault.Retrieve(ResourceName, entry.UserName);
                cred.RetrievePassword();
                Debug.WriteLine($"GetRefreshToken: retrieved token from PasswordVault (length={cred.Password?.Length ?? 0})");
                return cred.Password;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetRefreshToken: error reading PasswordVault: {ex.Message}");
                return null;
            }
        }

        public static string? GetEncryptedDbConfig()
        {
            try
            {
                var vault = new PasswordVault();
                var list = vault.RetrieveAll();
                var entry = list.FirstOrDefault(x => x.Resource == EncryptedDbConfigResourceName);
                if (entry == null)
                {
                    Debug.WriteLine("GetEncryptedDbConfig: no entry found in PasswordVault");
                    return null;
                }
                
                var cred = vault.Retrieve(EncryptedDbConfigResourceName, entry.UserName);
                cred.RetrievePassword();
                Debug.WriteLine($"GetEncryptedDbConfig: retrieved config from PasswordVault (length={cred.Password?.Length ?? 0})");
                return cred.Password;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetEncryptedDbConfig: error reading PasswordVault: {ex.Message}");
                return null;
            }
        }

        private static void RemoveEncryptedDbConfig()
        {
            try
            {
                var vault = new PasswordVault();
                var existing = vault.RetrieveAll().Where(c => c.Resource == EncryptedDbConfigResourceName).ToList();
                foreach (var e in existing)
                {
                    try
                    {
                        vault.Remove(e);
                    }
                    catch { }
                }
                
                if (existing.Count > 0)
                    Debug.WriteLine($"RemoveEncryptedDbConfig: removed {existing.Count} config(s) from PasswordVault");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RemoveEncryptedDbConfig: error: {ex.Message}");
            }
        }

        public static void RemoveRefreshToken()
        {
            try
            {
                var vault = new PasswordVault();
                
                // Remove refresh token
                try
                {
                    var existing = vault.RetrieveAll().Where(c => c.Resource == ResourceName).ToList();
                    if (existing.Count == 0)
                    {
                        Debug.WriteLine("RemoveRefreshToken: no entry to remove");
                    }
                    else
                    {
                        foreach (var e in existing)
                        {
                            try
                            {
                                vault.Remove(e);
                            }
                            catch (Exception exRem)
                            {
                                Debug.WriteLine($"RemoveRefreshToken: failed to remove one entry: {exRem.Message}");
                            }
                        }
                        Debug.WriteLine($"RemoveRefreshToken: removed {existing.Count} refresh token(s) from PasswordVault");
                    }
                }
                catch (Exception exExisting)
                {
                    Debug.WriteLine($"RemoveRefreshToken: error retrieving entries: {exExisting.Message}");
                }

                // Also remove encrypted DB config
                RemoveEncryptedDbConfig();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RemoveRefreshToken: error removing token: {ex.Message}");
            }
        }

        // Try restore session by exchanging persisted refresh token for new access token
        public static async Task<bool> TryRestoreSessionAsync()
        {
            try
            {
                Debug.WriteLine("TryRestoreSessionAsync: Attempting session restore via persisted refresh token...");
                var token = GetRefreshToken();
                if (string.IsNullOrWhiteSpace(token))
                {
                    Debug.WriteLine("TryRestoreSessionAsync: no persisted token found");
                    return false;
                }

                Debug.WriteLine($"TryRestoreSessionAsync: found persisted token (length={token.Length})");

                // Check if we have encrypted DB config (external DB session)
                var encryptedDbConfig = GetEncryptedDbConfig();
                var isExternalSession = !string.IsNullOrWhiteSpace(encryptedDbConfig);
                
                if (isExternalSession)
                {
                    Debug.WriteLine($"TryRestoreSessionAsync: detected external DB session (config length={encryptedDbConfig!.Length})");
                }

                // If mock mode enabled, perform offline restore
                if (UseMock)
                {
                    Debug.WriteLine("TryRestoreSessionAsync: using mock restore path");

                    TokenStore.RefreshToken = token;
                    TokenStore.AccessToken = "mock-access-token";

                    AppState.CurrentUser = new UserDto
                    {
                        UserId = 1,
                        UserName = "mockuser",
                        UserFullName = "Mock User",
                        RoleId = 1,
                        RoleName = "Admin",
                        IsActive = true
                    };

                    Debug.WriteLine("TryRestoreSessionAsync: mock restore successful");
                    return true;
                }

                TokenStore.RefreshToken = token;

                using var client = new HttpClient { BaseAddress = AppState.ApiBaseUri };
                
                // Choose endpoint based on session type
                string endpoint;
                object dto;
                
                if (isExternalSession)
                {
                    endpoint = "api/Auth/refresh-token-external";
                    dto = new RefreshTokenExternalRequestDto 
                    { 
                        RefreshToken = token,
                        EncryptedDbConfig = encryptedDbConfig!
                    };
                    Debug.WriteLine("TryRestoreSessionAsync: using refresh-token-external endpoint");
                }
                else
                {
                    endpoint = "api/Auth/refresh-token";
                    dto = new RefreshTokenRequestDto { RefreshToken = token };
                    Debug.WriteLine("TryRestoreSessionAsync: using refresh-token endpoint");
                }

                var resp = await client.PostAsJsonAsync(endpoint, dto).ConfigureAwait(false);
                var content = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                Debug.WriteLine($"TryRestoreSessionAsync: refresh endpoint returned status {resp.StatusCode}");
                
                if (!resp.IsSuccessStatusCode) 
                { 
                    Debug.WriteLine("TryRestoreSessionAsync: refresh failed, removing persisted token"); 
                    RemoveRefreshToken(); 
                    TokenStore.RefreshToken = null; 
                    return false; 
                }

                var wrapper = await resp.Content.ReadFromJsonAsync<ResponseWrapper<RefreshTokenResponseDto>>().ConfigureAwait(false);
                if (wrapper == null || !wrapper.Success || wrapper.Data == null) 
                { 
                    Debug.WriteLine("TryRestoreSessionAsync: wrapper invalid, removing persisted token"); 
                    RemoveRefreshToken(); 
                    TokenStore.RefreshToken = null; 
                    return false; 
                }

                TokenStore.AccessToken = wrapper.Data.AccessToken;
                TokenStore.RefreshToken = wrapper.Data.NewRefreshToken;

                // Update persisted tokens
                // For external sessions: preserve or update the encrypted DB config from response
                // For standard sessions: don't clear the existing DB config (in case it was there from a previous external session)
                string? newEncryptedDbConfig = null;
                if (isExternalSession)
                {
                    // External session: use the config from response if available, otherwise keep the existing one
                    newEncryptedDbConfig = !string.IsNullOrWhiteSpace(wrapper.Data.EncryptedDbConfig) 
                        ? wrapper.Data.EncryptedDbConfig 
                        : encryptedDbConfig;
                }
                // For standard session, don't pass any config and don't clear it
                SaveRefreshToken(TokenStore.RefreshToken ?? string.Empty, newEncryptedDbConfig);

                // Try to populate current user from API
                try
                {
                    using var userClient = new HttpClient { BaseAddress = AppState.ApiBaseUri };
                    userClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStore.AccessToken);

                    ResponseWrapper<UserDto>? userWrapper = null;
                    try
                    {
                        userWrapper = await userClient.GetFromJsonAsync<ResponseWrapper<UserDto>>("api/Auth/me").ConfigureAwait(false);
                    }
                    catch { }

                    if (userWrapper == null)
                    {
                        try
                        {
                            userWrapper = await userClient.GetFromJsonAsync<ResponseWrapper<UserDto>>("api/Users/me").ConfigureAwait(false);
                        }
                        catch { }
                    }

                    if (userWrapper != null && userWrapper.Success && userWrapper.Data != null)
                    {
                        AppState.CurrentUser = userWrapper.Data;
                        Debug.WriteLine("TryRestoreSessionAsync: populated AppState.CurrentUser from API");
                    }
                    else
                    {
                        Debug.WriteLine("TryRestoreSessionAsync: unable to populate current user from API");
                    }
                }
                catch (Exception exUser)
                {
                    Debug.WriteLine($"TryRestoreSessionAsync: error fetching user info: {exUser.Message}");
                }

                Debug.WriteLine("TryRestoreSessionAsync: refresh succeeded and tokens updated");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TryRestoreSessionAsync: exception during restore: {ex.Message}");
                try { RemoveRefreshToken(); } catch { }
                TokenStore.RefreshToken = null;
                return false;
            }
        }
    }
}
