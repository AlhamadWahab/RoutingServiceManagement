namespace IdentityLayer
{
    public class JwtSetting
    {
        public string SIGNINGKEY { get; set; }
        public string AUDIENCE { get; set; }
        public double LIFTIME { get; set; }
        public string ISSUER { get; set; }
    }

}
