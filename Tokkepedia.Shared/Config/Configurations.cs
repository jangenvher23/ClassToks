using System;
using System.Collections.Generic;
using System.Text;

namespace Tokkepedia.Shared.Config
{
    public static class Configurations
    {
        //public const string Url = "https://tokkepedia-dev.azurewebsites.net/";
        //public const string BaseUrl = "https://tokkepediadev.azure-api.net";
        public const string ApiPrefix = "/v1";
        //public const string ApiKey = "4d511486b72a41a6ae8d6716a101d0c4";
        public const string CodePrefix = "?code=";
        public const string ServiceId = "tokkepedia";
        public const string DevicePlatform = "android";

#if DEBUG
        public const string Url = "https://tokkepedia-dev.azurewebsites.net/";
        public const string BaseUrl = "https://tokkepediadev.azure-api.net";
        public const string ApiKey = "4d511486b72a41a6ae8d6716a101d0c4";
#elif RELEASE
        public const string Url = " https://tokkepediab.com/";
        public const string BaseUrl = "https://tokkepedia.azure-api.net";
        public const string ApiKey = "bdf93a29c82d41e48daf2700247220e5";
#endif
    }
}
