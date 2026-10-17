using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Praktika_Server
{
    internal class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("==================== CLIENT ===================");

            var ip = IPAddress.Parse("192.168.1.82");
            var port = 27001;
            var ep = new IPEndPoint(ip, port);

            var server = new TcpClient();
            try
            {
                server.Connect(ep);

                if (server.Connected)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Server to connect..");
                    Console.ResetColor();
                    var writerr = Task.Run(() =>
                    {
                        var afterwhile = false;

                        while (true)
                        {
                            Console.Write("1.New user\n2.Login\n0.exit\nEnter choice: ");
                            var choice = Console.ReadLine();
                            var stream = server.GetStream();
                            var writer42 = new BinaryWriter(stream);
                            writer42.Write(choice);
                            if (choice == "0")
                            {

                                break;
                            }
                            if (choice == "1")
                            {
                                Console.Write("Enter username: ");
                                var username = Console.ReadLine();

                                Console.Write("Enter password: ");
                                var password = Console.ReadLine();

                                var stream1 = server.GetStream();
                                var writer = new BinaryWriter(stream1);
                                writer.Write(username);

                                var stream2 = server.GetStream();
                                var writer2 = new BinaryWriter(stream2);
                                writer2.Write(password);

                                var stream3 = server.GetStream();
                                var writer3 = new BinaryReader(stream3);
                                var result = writer3.ReadString();
                                Console.WriteLine(result);
                            }
                            else if (choice == "2")
                            {
                                Console.Write("Enter username: ");
                                var username = Console.ReadLine();
                                var stream1 = server.GetStream();
                                var writer = new BinaryWriter(stream1);
                                writer.Write(username);

                                Console.Write("Enter password: ");
                                var password = Console.ReadLine();
                                var stream2 = server.GetStream();
                                var writer2 = new BinaryWriter(stream2);
                                writer2.Write(password);

                                var stream3 = server.GetStream();
                                var writer3 = new BinaryReader(stream3);
                                var result = writer3.ReadString();
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.WriteLine(result);
                                Console.ResetColor();

                                var stream4 = server.GetStream();
                                var writer4 = new BinaryReader(stream4);
                                var result2 = writer3.ReadString();
                                if (result2 == "break")
                                {
                                    afterwhile = true;
                                    break;
                                }
                            }

                        }

                        while (afterwhile)
                        {
                            Console.Write("Enter message: ");
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            var message = Console.ReadLine();
                            Console.ResetColor();
                            var stream1 = server.GetStream();
                            var writer = new BinaryWriter(stream1);
                            writer.Write(message);
                            if (message.Trim() == "exit")
                            {
                                break;
                            }
                        }



                    });

                    Task.WaitAll(writerr);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
            








        }
    }
}