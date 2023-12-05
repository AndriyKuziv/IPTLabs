using IPTLab2;
using System.Text;

string answer = "";

while(answer != "0")
{
    //Console.Write("Generate a set(1), Generate hash(2), Encrypt or decrypt a file(3), exit(0) | ");
    Console.Write("1 - Generate a set;\n" +
    "2 - Generate hash\n" +
    "3 - Encrypt/decrypt a file using RC5\n" +
    "4 - Encrypt/decrypt a file using RSA\n");
    answer = Console.ReadLine();

    string[] allowed = { "1", "2", "3", "4", "0" };

    while (!allowed.Contains(answer))
    {
        Console.WriteLine("Please enter valid answer");
        //Console.Write("Generate a set(1), Generate hash(2), Encrypt or decrypt a file(3), exit(0) | ");
        Console.Write("1 - Generate a set;\n" +
            "2 - Generate hash\n" +
            "3 - Encrypt/decrypt a file using RC5\n" +
            "4 - Encrypt/decrypt a file using RSA\n");
        answer = Console.ReadLine();
    }

    switch (answer)
    {
        case "1":
            RandGenMenu.Open();
            break;
        case "2":
            HashingMenu.Open();
            break;
        case "3":
            RC5CryptoMenu.Open();
            break;
        case "4":
            RSACryptoMenu.Open();
            break;
    }
}

