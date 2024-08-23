using KeiseiZaisenSharp.RawEntities.TrafficInfos;

namespace KeiseiZaisenSharp.DemoAndTests.Demo001
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (var kzSv = new KeiseiZaisenService())
                MainProc2(kzSv).Wait();
        }

        static async Task MainProc2(KeiseiZaisenService kzSv)
        {
            var trains = await kzSv.GetAllTrainsAsync();
            foreach (var train in trains)
            {
                Console.WriteLine("* ==============");
                Console.WriteLine("  {0} {1}行き", train.TrainType, train.Destination);
                Console.WriteLine("  列車番号 = {0}", train.TrainNumber);
                Console.WriteLine("  走行位置 = {0}", train.Location.Description);
                Console.WriteLine("  ==============");
                Console.WriteLine();
            }

            Console.ReadLine();
        }

        static async Task MainProc(KeiseiZaisenService kzSv)
        {
            var trafficInfo = await kzSv.GetRawTrafficInfoAsync();
            var stations = await kzSv.GetRawStationsAsync();
            var ikisakis = await kzSv.GetRawIkisakisAsync();
            var stops = await kzSv.GetRawStopsAsync();
            var syasyus = await kzSv.GetRawSyasyusAsync();

            var enameConverter = (string s) =>
            {
                // 入力値 E001 など
                foreach (var st in stops.Stop)
                {
                    var codeInt = 0;
                    if (!Int32.TryParse(st.Code, out codeInt))
                        continue;

                    if (s == $"E{codeInt.ToString("000")}")
                        return st.Name;

                    if (s == $"D{codeInt.ToString("000")}")
                        return st.Name + "周辺 (下り)";

                    if (s == $"U{codeInt.ToString("000")}")
                        return st.Name + "周辺 (上り)";
                }

                return "不明";
            };

            var syasyuConverter = (string s) =>
            {
                var rs = syasyus.Syasyu.SingleOrDefault(item => item.Code == s);

                return rs?.Name ?? "不明";
            };

            var hokoConverter = (string s) =>
            {
                if (s == "0")
                    return "上り";
                if (s == "1")
                    return "下り";

                return "不明";
            };

            var alphOrder = (char c) =>
            {
                return c switch
                {
                    'D' => 0,  // D が最初
                    'E' => 1,  // E が次
                    'U' => 2,  // U が最後
                    _ => 3     // その他は想定外
                };
            };

            var trainInfos = new List<TrafficSection>();
            trainInfos.AddRange(trafficInfo.TS);
            trainInfos.AddRange(trafficInfo.EK);
            trainInfos.Sort((x, y) =>
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

            foreach (var trEnt in trainInfos)
            {
                var stop = enameConverter(trEnt.Id);

                Console.WriteLine("ID = {0} ({1})", trEnt.Id, stop);
                foreach (var tr in trEnt.Tr)
                {
                    var hoko = hokoConverter(tr.Hk);
                    var syasyu = syasyuConverter(tr.Sy);
                    var ikisaki = ikisakis.Ikisaki.SingleOrDefault(item => item.Code == tr.Ik)?.Name ?? "不明";
                    
                    Console.WriteLine("　・({0}) {1} {2}行き ({3})", hoko, syasyu, ikisaki, tr.No);
                }

                Console.WriteLine();
            }

            Console.WriteLine();
        }
    }
}
