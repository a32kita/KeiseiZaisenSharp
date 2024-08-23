using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using KeiseiZaisenSharp.RawEntities.IkisakiInfos;
using KeiseiZaisenSharp.RawEntities.StationInfos;
using KeiseiZaisenSharp.RawEntities.StopInfos;
using KeiseiZaisenSharp.RawEntities.SyasyuInfos;
using KeiseiZaisenSharp.RawEntities.TrafficInfos;

namespace KeiseiZaisenSharp
{
    public class KeiseiZaisenService : IDisposable
    {
        private HttpClient _httpClient;
        private bool _isDisposed;

        private JsonSerializerOptions _jsonSerializerOptions;

        public KeiseiZaisenService()
        {
            this._httpClient = new HttpClient();
            this._isDisposed = false;
            
            this._jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            };
        }

        private void _checkDisposed()
        {
            if (this._isDisposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            if (this._httpClient == null)
                throw new Exception("HttpClient is null.");
        }

        public async Task<T> GetJsonDataAsync<T>(Uri uri)
        {
            this._checkDisposed();
            using (var hres = await this._httpClient.GetAsync(uri))
            using (var hcon = await hres.Content.ReadAsStreamAsync())
            {
                if (hcon == null)
                    throw new Exception("Failed to loading HTTP Response Content.");

                var result = await JsonSerializer.DeserializeAsync<T>(hcon, this._jsonSerializerOptions);
                if (result == null)
                    throw new Exception("Failed to loading JSON data.");

                return result;
            }
        }

        public async Task<TrafficInfo> GetTrafficInfoAsync()
        {
            // サンプル
            var uri = new Uri("https://zaisen.tid-keisei.jp/data/traffic_info.json?ts=1724384830035");
            return await this.GetJsonDataAsync<TrafficInfo>(uri);
        }

        public async Task<Stations> GetStationsAsync()
        {
            // サンプル
            var uri = new Uri("https://zaisen.tid-keisei.jp/config/station.json?ver=2.04");
            return await this.GetJsonDataAsync<Stations>(uri);
        }

        public async Task<Ikisakis> GetIkisakisAsync()
        {
            // サンプル
            var uri = new Uri("https://zaisen.tid-keisei.jp/config/ikisaki.json?ver=2.04");
            return await this.GetJsonDataAsync<Ikisakis>(uri);
        }

        public async Task<Stops> GetStopsAsync()
        {
            // サンプル
            var uri = new Uri("https://zaisen.tid-keisei.jp/config/stop.json?ver=2.04");
            return await this.GetJsonDataAsync<Stops>(uri);
        }

        public async Task<Syasyus> GetSyasyusAsync()
        {
            // サンプル
            var uri = new Uri("https://zaisen.tid-keisei.jp/config/syasyu.json?ver=2.04");
            return await this.GetJsonDataAsync<Syasyus>(uri);
        }

        public void Dispose()
        {
            this._checkDisposed();
            this._httpClient?.Dispose();
            this._isDisposed = true;
        }
    }
}
