using IPTLab2;

string answer = "";

while(answer != "0")
{
    Console.Write("Generate a set(1), Generate hash(2), exit(0) | ");
    answer = Console.ReadLine();

    string[] allowed = { "1", "2", "0" };

    while (!allowed.Contains(answer))
    {
        Console.WriteLine("Please enter valid answer");
        Console.Write("Generate a set(1), Generate hash(2), exit(0) | ");
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
    }
}

