//#Patterns: S2068 : { "credentialWords": "passwor,passwords" }

public class Foobar {
    //#Error: S2068
    public string passwor = "Password123";
    //#Error: S2068
    public string passwords = "Password123;Password321";
    public string noPassword;
}
