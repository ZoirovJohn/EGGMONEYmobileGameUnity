using System;

[Serializable]
public class LoginData
{
    public string nickName;
    public string password;

    public LoginData(string nickName, string password)
    {
        this.nickName = nickName;
        this.password = password;
    }
}
