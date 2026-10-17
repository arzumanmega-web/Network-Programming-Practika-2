using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.ConstrainedExecution;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;
namespace Client
{

    public class User
    {
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
        static void JsonSerializer(Users users)
        {
            var jsl = new JsonSerializerSettings();
            jsl.TypeNameHandling = TypeNameHandling.All;
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
        static Users JsonDeserializer()
        {
            var jsl = new JsonSerializerSettings();
            jsl.TypeNameHandling = TypeNameHandling.All;
            Users users = null;
            var js = new JsonSerializer();

            using (var sr = new StreamReader("users.json"))
            {
                using (var jr = new JsonTextReader(sr))
                {
                    users = js.Deserialize<Users>(jr);
                }
            }
            return users;
        }


        static TcpListener? listener;
        static BinaryWriter? writer;
        static BinaryReader? reader;
        static List<TcpClient> tcpClients = new List<TcpClient>();
        static async Task Main(string[] args)
        {
            Users users = JsonDeserializer();
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
                    //qebul edib cap edir
                    var read = Task.Run(() =>
                    {
                        User? user = null;

                        while (true)
                        {
                            var stream = client.GetStream();
                            reader = new BinaryReader(stream);
                            var choice = reader.ReadString();

                            if (choice == "0")  break; 

                            if (choice == "1")
                            {
                                
                                var stream2 = client.GetStream();
                                reader = new BinaryReader(stream2);
                                var username = reader.ReadString();//axin gelen
                                Console.WriteLine(username);
                                var stream3 = client.GetStream();
                                reader = new BinaryReader(stream3);
                                var password = reader.ReadString();//axin gelen
                                Console.WriteLine(password);

                                var result = users.AddUser(username, password);
                                var stream4 = client.GetStream();
                                writer = new BinaryWriter(stream);
                                writer.Write(result);//axin geden

                                JsonSerializer(users);

                            }
                            else if (choice == "2")
                            {
                                var stream1 = client.GetStream();
                                reader = new BinaryReader(stream1);
                                var username = reader.ReadString();

                                var stream2 = client.GetStream();
                                reader = new BinaryReader(stream2);
                                var password = reader.ReadString();
                                try
                                {
                                    user = users.Login(username, password);
                                    if (user.IsOnline)
                                    {
                                        var stream3 = client.GetStream();
                                        writer = new BinaryWriter(stream3);
                                        writer.Write($"Salam {user.UserName} xos gelmisiniz...");//axin geden

                                        var stream4 = client.GetStream();
                                        writer = new BinaryWriter(stream4);
                                        writer.Write("break");

                                        break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    var stream4 = client.GetStream();
                                    writer = new BinaryWriter(stream4);
                                    writer.Write(ex.Message);//axin geden

                                    var stream5 = client.GetStream();
                                    writer = new BinaryWriter(stream5);
                                    writer.Write("");
                                }
                            }
                        }
                        try
                        {
                            if (user !=null)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"Client {user.UserName} connected: {client.Client.RemoteEndPoint}");
                                Console.WriteLine($"{user.UserName} -- {((user.IsOnline) ? "Online" : "Ofline")}");
                                Console.ResetColor();

                            }
                            while (user != null)
                            {
                                
                                var stream1 = client.GetStream();
                                reader = new BinaryReader(stream1);
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
                        finally
                        {
                            if (user != null)
                            {
                                user.IsOnline = false;
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"{user.UserName} -- {((user.IsOnline) ? "Online" : "Ofline")}");
                                JsonSerializer(users);
                                Console.ResetColor();
                            }
                            client.Close();
                        }
                        

                    });

                    Task.WaitAll(read);
                });


            }



        }
    }
}
