using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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


        private Uri _baseUri;

        /// <summary>
        /// 接続先のシステムのベース URI を取得します。
        /// </summary>
        public string BaseUri
        {
            get => this._baseUri.ToString();
        }

        /// <summary>
        /// 接続先のシステムのバージョンを取得します。
        /// </summary>
        public string SystemVersion
        {
            get;
        }

        /// <summary>
        /// <see cref="Uri"/> とシステムのバージョン情報から <see cref="KeiseiZaisenService"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="baseUri">(省略可) システムのベース URL を指定します。デバッグ用非本番環境やキャッシュ環境を利用する場合にのみご利用ください。末尾を / の形で指定してください。</param>
        /// <param name="systemVersion">(省略可) システムのバージョンを指定します。</param>
        public KeiseiZaisenService(Uri? baseUri = null, string systemVersion = "2.04")
        {
            if (baseUri == null)
                baseUri = new Uri("https://zaisen.tid-keisei.jp/");
            this._baseUri = baseUri;
            
            this.SystemVersion = systemVersion;

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
#if true
            using (var hres = await this._httpClient.GetAsync(uri))
            using (var hcon = await hres.Content.ReadAsStreamAsync())
            {
                if (hcon == null)
                    throw new Exception("Failed to loading HTTP Response Content.");

                var result = await JsonSerializer.DeserializeAsync<T>(hcon, this._jsonSerializerOptions);
#elif DEBUG
            using (var hres = await this._httpClient.GetAsync(uri))
            {
                var hconStr = await hres.Content.ReadAsStringAsync();
                if (String.IsNullOrEmpty(hconStr))
                    throw new Exception("Failed to loading HTTP Response Content.");
                var result = JsonSerializer.Deserialize<T>(hconStr);

                try
                {
                    var outputDir = Path.Combine(
                        Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                        "serverData_debug");
                    if (Directory.Exists(outputDir) == false)
                        Directory.CreateDirectory(outputDir);

                    var fileName = Path.GetFileName(uri.LocalPath);
                    var outputPath = Path.Combine(outputDir, fileName);

                    File.WriteAllText(outputPath, hconStr);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.GetType().FullName);
                    Console.Error.WriteLine(ex.Message);
                }
#endif
                if (result == null)
                    throw new Exception("Failed to loading JSON data.");

                return result;
            }
        }

        private async Task _initializeConfigurationSources()
        {
            try
            {
                this._configurationSources = new KeiseiZaisenConfigurationSources(
                    (await this.GetRawIkisakisAsync()).Ikisaki,
                    (await this.GetRawStationsAsync()).Station,
                    (await this.GetRawStopsAsync()).Stop,
                    (await this.GetRawSyasyusAsync()).Syasyu);
            }
            catch (Exception ex)
            {
                throw new Exception("在線システムからの構成情報の取得に失敗しました。", ex);
            }
        }

        /// <summary>
        /// (非推奨) リアルタイムの在線情報の生データを取得します。
        /// </summary>
        /// <returns></returns>
        public async Task<TrafficInfo> GetRawTrafficInfoAsync()
        {
            var unixEpcTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var uri = new Uri(this._baseUri, $"data/traffic_info.json?ts={unixEpcTime.ToString()}");

            return await this._getJsonDataAsync<TrafficInfo>(uri);
        }

        /// <summary>
        /// (非推奨) 駅一覧情報の生データを取得します。
        /// </summary>
        /// <returns></returns>
        public async Task<Stations> GetRawStationsAsync()
        {
            var uri = new Uri(this._baseUri, $"config/station.json?ver={this.SystemVersion}");

            return await this._getJsonDataAsync<Stations>(uri);
        }

        /// <summary>
        /// (非推奨) 行き先一覧の生データを取得します。
        /// </summary>
        /// <returns></returns>
        public async Task<Ikisakis> GetRawIkisakisAsync()
        {
            var uri = new Uri(this._baseUri, $"config/ikisaki.json?ver={this.SystemVersion}");

            return await this._getJsonDataAsync<Ikisakis>(uri);
        }

        /// <summary>
        /// (非推奨) 停車場情報の生データを取得します。
        /// </summary>
        /// <returns></returns>
        public async Task<Stops> GetRawStopsAsync()
        {
            var uri = new Uri(this._baseUri, $"config/stop.json?ver={this.SystemVersion}");

            return await this._getJsonDataAsync<Stops>(uri);
        }

        /// <summary>
        /// (非推奨) 種別一覧の生データを取得します。
        /// </summary>
        /// <returns></returns>
        public async Task<Syasyus> GetRawSyasyusAsync()
        {
            var uri = new Uri(this._baseUri, $"config/syasyu.json?ver={this.SystemVersion}");

            return await this._getJsonDataAsync<Syasyus>(uri);
        }

        /// <summary>
        /// 現在営業線上にいるすべての列車の情報を取得します。
        /// </summary>
        /// <param name="sort"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<IEnumerable<KeiseiZaisenTrain>> GetAllTrainsAsync(bool sort = true)
        {
            if (this._configurationSources == null)
                await this._initializeConfigurationSources();
            if (this._configurationSources == null)
                throw new Exception();

            var trafficInfos = await this.GetRawTrafficInfoAsync();
            var trafficSections = new List<TrafficSection>();
            trafficSections.AddRange(trafficInfos.TS ?? new List<TrafficSection>());
            trafficSections.AddRange(trafficInfos.EK ?? new List<TrafficSection>());

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

        /// <summary>
        /// 名前から停車場の情報 (<see cref="StopEntry"/>) を取得します。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="exactMatch"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<StopEntry?> FindStopEntryAsync(string name, bool exactMatch = false)
        {
            if (this._configurationSources == null)
                await this._initializeConfigurationSources();
            if (this._configurationSources == null)
                throw new Exception();

            if (exactMatch)
                return this._configurationSources.Stops.FirstOrDefault(item => item.Name.Equals(name));
            
            return this._configurationSources.Stops.FirstOrDefault(item => item.Name.Contains(name));
        }

        /// <summary>
        /// <see cref="StopEntry"/> から隣の停車場 (<see cref="StopEntry"/>) の情報を取得します。
        /// </summary>
        /// <param name="stopEntry">停車場</param>
        /// <param name="direction">進行方向 (0 = 上り, 1 = 下り)</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<StopEntry?> GetNextStopAsync(StopEntry stopEntry, int direction = 0)
        {
            if (this._configurationSources == null)
                await this._initializeConfigurationSources();
            if (this._configurationSources == null)
                throw new Exception();

            return this._configurationSources.GetNextStop(stopEntry, direction);
        }

        public void Dispose()
        {
            this._checkDisposed();
            this._httpClient?.Dispose();
            this._isDisposed = true;
        }
    }
}
