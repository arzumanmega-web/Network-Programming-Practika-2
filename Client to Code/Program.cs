using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Praktika_CLIENT
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

                    var stream = server.GetStream();
                    var writer = new BinaryWriter(stream);
                    var reader = new BinaryReader(stream);

                    bool afterwhile = false;

                    while (!afterwhile)
                    {
                        Console.Write("1.New user\n2.Login\n0.exit\nEnter choice: ");
                        var choice = Console.ReadLine();
                        writer.Write(choice!);

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

                            writer.Write(username!);
                            writer.Write(password!);

                            var result = reader.ReadString();
                            Console.WriteLine(result);
                        }
                        else if (choice == "2")
                        {
                            Console.Write("Enter username: ");
                            var username = Console.ReadLine();

                            Console.Write("Enter password: ");
                            var password = Console.ReadLine();

                            writer.Write(username!);
                            writer.Write(password!);

                            var result = reader.ReadString();
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine(result);
                            Console.ResetColor();

                            var result2 = reader.ReadString();
                            if (result2 == "break")
                            {
                                afterwhile = true;
                                break;
                            }
                        }
                    }

                    if (afterwhile)
                    {

                        Task.Run(() =>
                        {
                            try
                            {
                                while (server.Connected)
                                {
                                    var incomingMsg = reader.ReadString();
                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    Console.WriteLine(incomingMsg);
                                    Console.ResetColor();
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        });


                        while (server.Connected)
                        {
                            Console.Write("Enter message: ");
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            var message = Console.ReadLine();
                            Console.ResetColor();

                            if (string.IsNullOrEmpty(message)) continue;

                            writer.Write(message);

                            if (message.Trim() == "exit")
                            {
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
            finally
            {
                server.Close();
            }
        }
    }
}