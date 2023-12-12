using IPTLab2;
using IPTLab2.Menus;
using System.Text;

string answer = "";

while(answer != "0")
{
    Console.Write("\n1 - Generate a set;\n" +
    "2 - Generate hash\n" +
    "3 - Encrypt/decrypt a file using RC5\n" +
    "4 - Encrypt/decrypt a file using RSA\n");
    answer = Console.ReadLine();

    string[] allowed = { "1", "2", "3", "4", "0" };

    while (!allowed.Contains(answer))
    {
        Console.WriteLine("Please enter valid answer");
        Console.Write("\n1 - Generate a set;\n" +
            "2 - Generate hash\n" +
            "3 - Encrypt/decrypt a file using RC5\n" +
            "4 - Encrypt/decrypt a file using RSA" +
            "0 - Exit\n");
        answer = Console.ReadLine();
    }

    switch (answer)
    {
        case "1":
            RandGenMenu.Open();
            break;
        case "2":
            MDFiveMenu.Open();
            break;
        case "3":
            RCFiveCryptoMenu.Open();
            break;
        case "4":
            RSACryptoMenu.Open();
            break;
    }
}

