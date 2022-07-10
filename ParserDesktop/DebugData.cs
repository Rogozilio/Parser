using System;

namespace ParserDesktop
{
    public class DebugData
    {
        public static void Show(DataForRequest data)
        {
            Console.WriteLine("Имя: " + data.Name);
            Console.WriteLine("Телефон: " + data.Phone);
            Console.WriteLine("Цена: " + data.Price);
            Console.WriteLine("Адрес: " + data.Address);
            Console.WriteLine("Этаж: " + data.Floor);
            Console.WriteLine("Число комнат: " + data.NumbersOfRoom);
            Console.WriteLine("Площадь общая: " + data.TotalArea);
            Console.WriteLine("Площадь кухни: " + data.KitchenArea);
            Console.WriteLine("Ремонт: " + data.Renovation);
            Console.WriteLine("Ссылка : " + data.Ref);
            Console.WriteLine();
        }
    }
}