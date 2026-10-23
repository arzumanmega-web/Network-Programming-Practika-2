using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Client
{
    public class User
    {
        public string? LastUserIP { get; set; }
        public string? LastServerIP { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public bool IsOnline { get; set; } = false;
    }

    public class Users
    {
        public List<User> users = new List<User>();

        public string AddUser(string username, string password)
        {
            if (users.Count > 0)
            {
                foreach (var user in users)
                {
                    if (user.UserName == username)
                    {
                        return "Bu username artiq istifade olunub...";
                    }
                }
            }
            var new_user = new User() { UserName = username, Password = password };
            users.Add(new_user);
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine($"Yeni istifadeci {username} qeydiyyatdan kecdi...");
            Console.ResetColor();
            return "Hesab ugurla elave edildi...";
        }

        public User Login(string username, string password)
        {
            if (users.Count == 0) throw new Exception("Not user login...");

            foreach (var user in users)
            {
                if (user.UserName == username && user.Password == password)
                {
                    if (user.IsOnline == true)
                    {
                        throw new Exception("Username already looged in...");
                    }
                    user.IsOnline = true;
                    return user;
                }
            }
            throw new Exception("Wrong username and password...");
        }
    }

    internal class Program
    {
        static void JsonSerializerUsers(Users users)
        {
            var js = new JsonSerializer();
            using (var sw = new StreamWriter("users.json"))
            {
                using (var jw = new JsonTextWriter(sw))
                {
                    jw.Formatting = Newtonsoft.Json.Formatting.Indented;
                    js.Serialize(jw, users);
                }
            }
        }

        static Users JsonDeserializerUsers()
        {
            if (!File.Exists("users.json")) return new Users();

            Users? users = null;
            var js = new JsonSerializer();

            using (var sr = new StreamReader("users.json"))
            {
                using (var jr = new JsonTextReader(sr))
                {
                    users = js.Deserialize<Users>(jr);
                }
            }
            return users ?? new Users();
        }

        static TcpListener? listener;
        static List<TcpClient> tcpClients = new List<TcpClient>();

        static async Task Main(string[] args)
        {
            
            Users users = JsonDeserializerUsers();

            Console.WriteLine("==================== SERVER ===================");
            var ip = IPAddress.Parse("192.168.1.82");
            var port = 27001;
            var ep = new IPEndPoint(ip, port);
            listener = new TcpListener(ep);
            listener.Start();

            Console.WriteLine($"Listening on: {listener.LocalEndpoint}");

            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                tcpClients.Add(client);

                _ = Task.Run(() =>
                {
                    User? user = null;
                    var stream = client.GetStream();
                    var reader = new BinaryReader(stream);
                    var writer = new BinaryWriter(stream);

                    try
                    {
                        while (true)
                        {
                            var choice = reader.ReadString();

                            if (choice == "0") break;

                            if (choice == "1")
                            {
                                var username = reader.ReadString();
                                var password = reader.ReadString();
                                var result = users.AddUser(username, password);

                                writer.Write(result);
                                JsonSerializerUsers(users);
                            }
                            else if (choice == "2")
                            {
                                var username = reader.ReadString();
                                var password = reader.ReadString();
                                try
                                {
                                    user = users.Login(username, password);
                                    if (user.IsOnline)
                                    {
                                        writer.Write($"Salam {user.UserName} xos gelmisiniz...");
                                        writer.Write("break");

                                        user.LastUserIP = client?.Client.RemoteEndPoint?.ToString();
                                        user.LastServerIP = client?.Client?.LocalEndPoint?.ToString();

                                        JsonSerializerUsers(users);
                                        break; 
                                    }
                                }
                                catch (Exception ex)
                                {
                                    writer.Write(ex.Message);
                                    writer.Write("error"); 
                                }
                            }
                        }

                        if (user != null)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Client {user.UserName} connected: {client?.Client.RemoteEndPoint}");
                            Console.WriteLine($"{user.UserName} -- Online");
                            Console.ResetColor();

                            OnlineUsers(users);

                            while (client!.Connected)
                            {
                                var message = reader.ReadString();
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"{user.UserName} :-> {message}");
                                Console.ResetColor();

                                if (message.Trim() == "exit")
                                {
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        if (user != null)
                        {
                            user.IsOnline = false;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"{user.UserName} -- Offline");
                            Console.ResetColor();

                            tcpClients.Remove(client);
                            OnlineUsers(users);
                            JsonSerializerUsers(users);
                        }
                        client?.Close();
                    }
                });
            }
        }

        
        private static void OnlineUsers(Users users)
        {
            string list_Online_User = "";
            foreach (var item1 in users.users)
            {
                if (item1.IsOnline)
                {
                    list_Online_User += $"{item1.UserName} >> Online\n";
                }
            }

            foreach (var item2 in tcpClients.ToArray())
            {
                try
                {
                    var stream = item2.GetStream();
                    var w = new BinaryWriter(stream);
                    w.Write($"\n>>Online Users<<\n{list_Online_User}");
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}