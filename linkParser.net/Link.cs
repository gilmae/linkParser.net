public class Link
    {
        public string Rel { get; set; } = default!;
        public string Url { get; set; } = default!;
        public Dictionary<string, string> Params { get; set; }
        public bool InHeaders { get; set; }

        public Link()
        {
            Params = new Dictionary<string, string>();
        }
    }