namespace ParserDesktop
{
    public struct DataForRequest
    {
        public string Name;
        public string Phone;
        public string Price;
        public string Address;
        public string Floor;
        public string NumbersOfRoom;
        public string TotalArea;
        public string KitchenArea;
        public string Renovation;
        public string Ref;

        public string All
        {
            get => '\n' +
                   "Имя: " + Name + '\n' +
                   "Телефон: " + Phone + '\n' +
                   "Цена: " + Price + '\n' +
                   "Адрес: " + Address + '\n' +
                   "Этаж: " + Floor + '\n' +
                   "Число комнат: " + NumbersOfRoom + '\n' +
                   "Площадь общая: " + TotalArea + '\n' +
                   "Площадь кухни: " + KitchenArea + '\n' +
                   "Ремонт: " + Renovation + '\n' +
                   "Ссылка: " + Ref + 
                   '\n';
        }
    }
}