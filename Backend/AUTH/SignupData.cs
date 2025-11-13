using System;

[Serializable]
public class SignupData
{
    public string nickName;
    public string email;
    public string password;
    public string phoneNumber;
    public string nation;
    public int age;

    public SignupData(string nickName, string email, string password, string phoneNumber, string nation, int age)
    {
        this.nickName = nickName;
        this.email = email;
        this.password = password;
        this.phoneNumber = phoneNumber;
        this.nation = nation;
        this.age = age;
    }
}
