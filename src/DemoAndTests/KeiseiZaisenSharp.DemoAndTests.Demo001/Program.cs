using KeiseiZaisenSharp.RawEntities.TrafficInfos;
using System.Text.Json;

namespace KeiseiZaisenSharp.DemoAndTests.Demo001
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var baseUri = new Uri("https://php-api-sv02.a32kita.net/keisei_dummy/");
            baseUri = null;

            using (var kzSv = new KeiseiZaisenService(baseUri))
                MainProc2(kzSv).Wait();
        }

        static async Task MainProc2(KeiseiZaisenService kzSv)
        {
            var trains = await kzSv.GetAllTrainsAsync();
            foreach (var train in trains)
                PrintTrainInfo(train);

            Console.WriteLine("青砥駅周辺の列車");
            foreach (var train in trains.Where(item => item.Location.Description.Contains("青砥")))
                PrintTrainInfo(train, false);

            Console.WriteLine();
            Console.WriteLine("JSON 出力");
            Console.WriteLine();

            var json = JsonSerializer.Serialize(trains, new JsonSerializerOptions() { WriteIndented = true });
            Console.WriteLine(json);
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "result.json"), json);

            Console.ReadLine();
        }

        static void PrintTrainInfo(KeiseiZaisenTrain train, bool detail = true)
        {
            if (detail)
            {
                Console.WriteLine("* ==============");
                Console.WriteLine("  {0} {1}行き", train.TrainType, train.Destination);
                Console.WriteLine("  列車番号 = {0}", train.TrainNumber);
                Console.WriteLine("  走行位置 = {0}", train.Location.Description);
                Console.WriteLine("  ==============");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("  {0} {1}行き ({2})", train.TrainType, train.Destination, train.TrainNumber);
            }
        }
    }
}
