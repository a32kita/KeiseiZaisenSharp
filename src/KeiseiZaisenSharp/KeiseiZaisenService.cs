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

        private KeiseiZaisenConfigurationSources? _configurationSources;

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

        private async Task<T> _getJsonDataAsync<T>(Uri uri)
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

        private async Task _initializeConfigurationSources()
        {
            this._configurationSources = new KeiseiZaisenConfigurationSources(
                (await this.GetRawIkisakisAsync()).Ikisaki,
                (await this.GetRawStationsAsync()).Station,
                (await this.GetRawStopsAsync()).Stop,
                (await this.GetRawSyasyusAsync()).Syasyu);
        }

        public async Task<TrafficInfo> GetRawTrafficInfoAsync()
        {
            var uri = new Uri("https://zaisen.tid-keisei.jp/data/traffic_info.json?ts=1724384830035");
            return await this._getJsonDataAsync<TrafficInfo>(uri);
        }

        public async Task<Stations> GetRawStationsAsync()
        {
            var uri = new Uri("https://zaisen.tid-keisei.jp/config/station.json?ver=2.04");
            return await this._getJsonDataAsync<Stations>(uri);
        }

        public async Task<Ikisakis> GetRawIkisakisAsync()
        {
            var uri = new Uri("https://zaisen.tid-keisei.jp/config/ikisaki.json?ver=2.04");
            return await this._getJsonDataAsync<Ikisakis>(uri);
        }

        public async Task<Stops> GetRawStopsAsync()
        {
            var uri = new Uri("https://zaisen.tid-keisei.jp/config/stop.json?ver=2.04");
            return await this._getJsonDataAsync<Stops>(uri);
        }

        public async Task<Syasyus> GetRawSyasyusAsync()
        {
            var uri = new Uri("https://zaisen.tid-keisei.jp/config/syasyu.json?ver=2.04");
            return await this._getJsonDataAsync<Syasyus>(uri);
        }

        public async Task<IEnumerable<KeiseiZaisenTrain>> GetAllTrainsAsync(bool sort = true)
        {
            if (this._configurationSources == null)
                await this._initializeConfigurationSources();
            if (this._configurationSources == null)
                throw new Exception();

            var trafficInfos = await this.GetRawTrafficInfoAsync();
            var trafficSections = new List<TrafficSection>();
            trafficSections.AddRange(trafficInfos.TS);
            trafficSections.AddRange(trafficInfos.EK);

            if (sort)
            {
                var alphOrder = new Func<char, int>(c =>
                {
                    return c switch
                    {
                        'D' => 0,  // D が最初
                        'E' => 1,  // E が次
                        'U' => 2,  // U が最後
                        _ => 3     // その他は想定外
                    };
                });

                trafficSections.Sort((x, y) =>
                {
                    // 数値部分を比較
                    int numberX = int.Parse(x.Id.Substring(1)); // 文字列の2文字目以降を数値に変換
                    int numberY = int.Parse(y.Id.Substring(1));
                    int result = numberX.CompareTo(numberY);

                    // 数値が同じ場合、アルファベット部分をカスタム順序で比較
                    if (result == 0)
                    {
                        result = alphOrder(x.Id[0]).CompareTo(alphOrder(y.Id[0]));
                    }

                    return result;
                });
            }

            var resultTrains = new List<KeiseiZaisenTrain>();
            foreach (var sect in trafficSections)
            {
                for (var i = 0; i < sect.Tr.Count; i++)
                {
                    resultTrains.Add(new KeiseiZaisenTrain(this._configurationSources, sect, i));
                }
            }

            return resultTrains;
        }

        public void Dispose()
        {
            this._checkDisposed();
            this._httpClient?.Dispose();
            this._isDisposed = true;
        }
    }
}
