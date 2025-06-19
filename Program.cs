using System;

namespace FrontEndCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
            ConnectionHttpServer con = new ConnectionHttpServer();
            con.StartServer();
        }
    }
}
