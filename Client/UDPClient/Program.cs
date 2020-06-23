using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Security.Cryptography;

namespace UDPClient
{
    class Program
    {
        //необходимые глобальные переменные
        static int localPort = 4000;
        static Socket listeningSocket;
        static string[] ip_adresses = new string[255];
        static int index;
        static int delay;
        static int tmp;
        static double median;
        static double Max = 0;
        static ulong ElementCounter = 0;
        static double SummElement = 0;
        static int ElementModeCount = 0;
        static int formode = 0;
        static double sredArif;
        static double standOtkl;
        static double predstandOtkl = 0;
        static double[] arrMode = new double[5000];
        static EndPoint remoteIp;
        static ulong predNumOfPackage = 0;
        static ulong errIntCounter = 0;
        static ulong PckgWithoutErrors = 0;
        static void Main(string[] args)
        {
            //проверяем есть ли файл конфига в текущем каталоге с .exe файлом
            string conffile = "config.xml";
            if (!File.Exists(conffile))
            {
                //если конфиг отсутствует то записываем дефолтные значения
                Console.WriteLine("Файл конфигурации не найден!");
                localPort = 4000;
                ip_adresses[index] = "127.0.0.1";
                delay = 0;
            }
            else configure(conffile); //передаем путь файла конфигурации в процедуру
            //в блоке обработки исклчений
            try
            {
                //создаем совет
                listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                //запускаем таск с прослышиванием пакетов
                Thread myThread = new Thread(new ThreadStart(Listen));
                myThread.Start(); // запускаем поток
                tmp = tmp++;
                Console.WriteLine("Данные получены, поток получения информации запущен");
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
            //в основном потоке крутим бесконечный цикл пока не нажата "Х" для выхода из приложения
            //или Энтер для вывода расчетов
            while (Console.ReadKey().Key != ConsoleKey.X) {
                if (Console.ReadKey().Key == ConsoleKey.Enter)
                {
                    Close();
                    Console.WriteLine("Среднее арифметическое: {0}", sredArif);
                    Console.WriteLine("Медиана: {0}",median);
                    Console.WriteLine("Стандартное отклонение: {0}", standOtkl);
                    Console.WriteLine("Мода: " + Mode(arrMode));
                    Console.WriteLine("Потерянных пакетов: {0}", errIntCounter);
                    Console.WriteLine("Правильная последовательность пакетов: {0}", PckgWithoutErrors);
                }

            }
        }
        //процедура закрытия сокета, чтоб уж наверняка
        private static void Close()
        {
            if (listeningSocket != null)
            {
                listeningSocket.Shutdown(SocketShutdown.Both);
                listeningSocket.Close();
                listeningSocket = null;
            }
        }
        //процедура прослуштвания сокета

        private static void Listen()
        {
            try
            {
                //пробуем получить информацию с вбитых в конфиг адресов
                //данные программа дальше получать с адреса с которого придут первые данные
                int i = 0; 
                while (true)
                {
                    int bytes = 0;
                    if (i < index)
                    {
                        byte[] data = new byte[256];
                        //пробуем создать эндпоинт на полученнный из конфига адрес
                        remoteIp = new IPEndPoint(IPAddress.Parse(ip_adresses[i]), localPort);
                        listeningSocket.Bind(remoteIp);
                        bytes = listeningSocket.ReceiveFrom(data, ref remoteIp);
                    }
                    else i=0;
                    //если пришли данные то пишем пользователю и выходим из текущего цикла
                    //входим во второй цикл с получением
                    if (bytes != 0)
                    {
                        Console.WriteLine("Данные получены с IP-адреса {0}, порт: {1}", ip_adresses[i], localPort);
                        break;
                    }
                }
                //поток получения данных
                listeningSocket.ReceiveBufferSize = 4096;
                while (true)
                {
                    //установка задержки для потока, задержка берется из конфига
                    Thread.Sleep(delay);
                    int bytes = 0;
                    StringBuilder builder = new StringBuilder();
                    byte[] data = new byte[256];
                    //получаем данные
                    bytes = listeningSocket.ReceiveFrom(data, ref remoteIp);
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    //конвертируем из байтов в беззнаковый удлиненный челочисленный
                    string tmpstr = builder.ToString();
                    int srchSym = tmpstr.IndexOf(":");
                    ulong strNumOfPcg = Convert.ToUInt64(tmpstr.Remove(srchSym));
                    string bodyOfPcg = (tmpstr.Remove(0, srchSym+1));
                    ulong curr = Convert.ToUInt64(bodyOfPcg);
                    calculate(curr, strNumOfPcg);
                    //передаем текущую цифру в процедуру для расчетов
                    IPEndPoint remoteFullIp = remoteIp as IPEndPoint;
                    //выводим адрес получения и текущую ифнормацию
                    Console.WriteLine($"{remoteFullIp.Address}: -  {bodyOfPcg}");
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
        private static void calculate(ulong currentInt, ulong numberOfPackage)
        {
            //вычисление среднего арифметического
            try
            {
                ElementCounter++;
                SummElement = SummElement + currentInt;
            }
            catch (OverflowException)
            {
                ElementCounter = 0;
                SummElement = 0;
            }
            sredArif = SummElement / ElementCounter;
            //вычисление медианы
            ulong currMax = currentInt;
            if (currMax>Max)
            {
                Max = currMax;
                if (Max % 2 == 0) median = Max / 2;
                else median = (Max/2+((Max+1)/2)) / 2;
            }
            else
            {
                if (Max % 2 == 0) median = Max / 2;
                else median = (Max / 2 + ((Max + 1) / 2)) / 2;
            }
            //вычисление моды
            ElementModeCount++;
            if (ElementModeCount > 5000)
            {
                ElementModeCount = 1;
                formode = 1;
                arrMode[ElementModeCount-1] = currentInt;
            }
            else
            {
                arrMode[ElementModeCount-1] = currentInt;
            }
            //стандартное отклонение
            standOtkl = Math.Pow((currentInt - sredArif), 2);
            standOtkl = ((predstandOtkl + standOtkl) / ElementCounter);
            standOtkl = Math.Sqrt(standOtkl);
            predstandOtkl = standOtkl;
            //потерянные пакеты
            if (((numberOfPackage - predNumOfPackage) == numberOfPackage) | ((numberOfPackage - predNumOfPackage) == 1))
            {
                try
                {
                    PckgWithoutErrors++;
                }
                catch (OverflowException)
                {
                    PckgWithoutErrors = 0;
                }
            }       
            else if ((numberOfPackage - predNumOfPackage) > 1)
            {
                try
                {
                    errIntCounter = errIntCounter + (numberOfPackage - predNumOfPackage);
                }
                catch (OverflowException)
                {
                    errIntCounter = 0;
                }
            }
            predNumOfPackage = numberOfPackage;;

        }
        //процедура для расчета моды
        static double Mode(double[] arr)
        {
            if (arr.Length == 0)
                throw new ArgumentException("Маccив не может быть пустым");
            if (formode == 0)
                Array.Resize(ref arr, ElementModeCount - 1);
            Dictionary<double, int> dict = new Dictionary<double, int>();
            foreach (double elem in arr)
            {
                if (dict.ContainsKey(elem))
                    dict[elem]++;
                else
                    dict[elem] = 1;
            }

            int maxCount = 0;
            double mode = Double.NaN;
            foreach (double elem in dict.Keys)
            {
                if (dict[elem] > maxCount)
                {
                    maxCount = dict[elem];
                    mode = elem;
                }
            }
            return mode;
        }
        //считываем ифнормацию из конфига
        private static void configure(string conf)
        {
            using (XmlTextReader reader = new XmlTextReader("config.xml"))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement("client") && !reader.IsEmptyElement)
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement("port"))
                                try
                                {
                                    localPort = Convert.ToInt32(reader.ReadString());
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
                            else if (reader.IsStartElement("delay"))
                                try
                                {
                                    delay = Convert.ToInt32(reader.ReadString());
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

