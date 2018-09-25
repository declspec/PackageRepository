namespace GetPkg {
    public class Token {
        public string Id { get; set; }
        public string UserId { get; set; }

        public bool IsAuthenticated => !string.IsNullOrEmpty(UserId);
    }
}
