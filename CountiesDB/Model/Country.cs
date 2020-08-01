namespace CountiesDB.Model
{
    public class Country
    {
        public string Name { get; set; }
        public string NumericCode { get; set; }
        public string Capital { get; set; }
        public double? Area { get; set; }
        public int? Population { get; set; }
        public string Region { get; set; }
    }
}
