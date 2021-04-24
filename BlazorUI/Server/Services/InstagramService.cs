using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Android.DeviceInfo;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorUI.Server.Services
{
    public class InstagramService
    {
        private IInstaApi _instaApi;

        public async Task<bool> LoginAsync(string userName,string password)
        {
            try
            {
                var userSession = new UserSessionData
                {
                    UserName = userName,
                    Password = password
                };

                var device = new AndroidDevice
                {
                    AndroidBoardName = "HONOR",
                    DeviceBrand = "HUAWEI",
                    HardwareManufacturer = "HUAWEI",
                    DeviceModel = "PRA-LA1",
                    DeviceModelIdentifier = "PRA-LA1",
                    FirmwareBrand = "HWPRA-H",
                    HardwareModel = "hi6250",
                    DeviceGuid = new Guid("be897499-c663-492e-a125-f4c8d3785ebf"),
                    PhoneGuid = new Guid("7b72321f-dd9a-425e-b3ee-d4aaf476ec52"),
                    DeviceId = ApiRequestMessage.GenerateDeviceIdFromGuid(new Guid("be897499-c663-492e-a125-f4c8d3785ebf")),
                    Resolution = "1080x1812",
                    Dpi = "480dpi",
                    FirmwareFingerprint = "HUAWEI/HONOR/PRA-LA1:7.0/hi6250/95414346:user/release-keys",
                    AndroidBootloader = "4.23",
                    DeviceModelBoot = "qcom",
                    FirmwareTags = "release-keys",
                    FirmwareType = "user"
                };

                var delay = RequestDelay.FromSeconds(1, 1);

                _instaApi = InstaApiBuilder.CreateBuilder()
                    .SetUser(userSession)
                    .UseLogger(new DebugLogger(LogLevel.Exceptions))
                    .SetRequestDelay(delay)
                    .Build();

                _instaApi.SetDevice(device);
                _instaApi.SetAcceptLanguage("tr-TR");

                const string stateFile = "state.bin";
                try
                {
                    if (File.Exists(stateFile))
                    {
                        using (var fs = File.OpenRead(stateFile))
                        {
                            _instaApi.LoadStateDataFromStream(fs);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                if (!_instaApi.IsUserAuthenticated)
                {
                    // login
                    Console.WriteLine($"Logging in as {userSession.UserName}");
                    delay.Disable();
                    var logInResult = await _instaApi.LoginAsync();
                    delay.Enable();
                    if (!logInResult.Succeeded)
                    {
                        Console.WriteLine($"Unable to login: {logInResult.Info.Message}");
                        return false;
                    }
                }
                var state = _instaApi.GetStateDataAsStream();
                using (var fileStream = File.Create(stateFile))
                {
                    state.Seek(0, SeekOrigin.Begin);
                    state.CopyTo(fileStream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return false;
        }

        public bool LogoutAsync()
        {
            var logoutResult = Task.Run(() => _instaApi.LogoutAsync()).GetAwaiter().GetResult();

            if (logoutResult.Succeeded) return true;

            return false;
        }

        public async Task<string> GetMediaIdFromUrlAsync(string url)
        {
            var result = await _instaApi.MediaProcessor.GetMediaIdFromUrlAsync(new Uri(url));

            if (!result.Succeeded)
                throw new Exception("Belirtilen gönderi bulunamadı.");

            return result.Value;
        }

        public async Task<InstaLikersList> GetMediaLikersAsync(string mediaId)
        {
            var result = await _instaApi.MediaProcessor.GetMediaLikersAsync(mediaId);

            if (!result.Succeeded)
                throw new Exception("Belirtilen gönderi bulunamadı.");

            return result.Value;
        }

        public async Task<InstaCommentList> GetMediaCommentsAsync(string mediaId)
        {
            var result = await _instaApi.CommentProcessor.GetMediaCommentsAsync(mediaId, PaginationParameters.Empty);

            if (!result.Succeeded)
                throw new Exception("Gönderi yorumları çekilirken hata oluştu.");

            return result.Value;
        }

        public async Task<InstaUserShortList> GetUserFollowersAsync(string userName)
        {
            var result = await _instaApi.UserProcessor.GetUserFollowersAsync(userName, PaginationParameters.Empty);

            if (!result.Succeeded)
                throw new Exception(result.Info.Message);

            return result.Value;
        }

        public async Task<bool> IsFollowAsync(string userName, string searchQuery = "")
        {
            var result = await _instaApi.UserProcessor.GetUserFollowersAsync(userName, PaginationParameters.Empty, searchQuery: searchQuery);

            if (!result.Succeeded)
                throw new Exception(result.Info.Message);

            bool isFollow = result.Value.Any(e => e.UserName == searchQuery);

            return isFollow;
        }
    }
}
