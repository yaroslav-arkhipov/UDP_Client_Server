using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;

namespace UDPServer
{
    class Program
    {
        static int remotePort;
        static Socket listeningSocket;
        static int index = 0;
        static string[] ip_adresses = new string[255];
        static int LowRange = 0;
        static int UpRange = 0;
        static EndPoint[] remotePoint = new IPEndPoint[255];
        static ulong numOfPackage = 0;
        static void Main(string[] args)
        {
            string conffile = "config.xml";
            if (!File.Exists(conffile))
            {
                Console.WriteLine("Файл конфигурации не найден!");
                remotePort = 4000;
                ip_adresses[index] = "127.0.0.1";
                LowRange = 0;
                UpRange = 10;
            }
            else configure(conffile); //передаем путь файла конфигурации в процедуру
            for (int i = 0;i<index;i++)
                {
                    try
                    {
                        remotePoint[i] = new IPEndPoint(IPAddress.Parse(ip_adresses[i]), remotePort);
                        Console.WriteLine("Создана точка передачи данных по адресу: " + ip_adresses[i] + " порт: " + remotePort);
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine(exc.Message);
                    }
                }
            
            listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Thread myThread = new Thread(new ThreadStart(Send));
            myThread.Start();
            Console.WriteLine("Сервер запущен");
            while (Console.ReadKey().Key != ConsoleKey.Enter) { }
        }

        private static void Send()
        {
            try
            {
                Random rand = new Random();
                while (true)
                {
                    for (int i = 0; i < index; i++)
                    {
                        Thread.Sleep(0); 
                        string message = Convert.ToString(numOfPackage)+":"+Convert.ToString(rand.Next(LowRange, UpRange));
                        byte[] data = Encoding.Unicode.GetBytes(message);
                        listeningSocket.SendTo(data, remotePoint[i]);
                    }
                    //нумерация пакетов отправки
                    try
                    {
                        numOfPackage++;
                    }
                    catch (OverflowException)
                    {
                        numOfPackage = 0;
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
            finally
            {
                Close();
            }
        }

    private static void Close()
        {
            if (listeningSocket != null)
            {
                listeningSocket.Shutdown(SocketShutdown.Both);
                listeningSocket.Close();
                listeningSocket = null;
            }
        }
    private static void configure(string conf)
        {
            using (XmlTextReader reader = new XmlTextReader(conf))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement("server") && !reader.IsEmptyElement)
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement("port"))
                                try
                                {
                                    remotePort = Int32.Parse(reader.ReadString());
                                }
                                catch (Exception exc)
                                {
                                    Console.WriteLine(exc.Message);
                                }
                            else if (reader.IsStartElement("ip"))
                            {
                                string tmpStr = reader.ReadString();
                                if (tmpStr.Length > 15)
                                {
                                    Console.WriteLine("Превышена длина IP-адреса в файле config.xml");
                                    break;
                                }
                                else
                                    ip_adresses[index] = tmpStr;
                                index++;
                            }
                            else if (reader.IsStartElement("lower_range"))
                                try
                                {
                                    LowRange = Int32.Parse(reader.ReadString());
                                }
                                catch (Exception exc)
                                {
                                    Console.WriteLine(exc.Message);
                                }
                            else if (reader.IsStartElement("upper_range"))
                                try
                                {
                                    UpRange = Int32.Parse(reader.ReadString());
                                }
                                catch (Exception exc)
                                {
                                    Console.WriteLine(exc.Message);
                                }
                            else if (!reader.IsStartElement() && reader.Name == "configuration")
                                break;
                        }
                    }
                }
            }
        }

    }
}
