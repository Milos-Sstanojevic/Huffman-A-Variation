using System.Text;

class HuffmanCode
{
    public Dictionary<char, Tuple<int, string>> _codeTable;
    public Dictionary<char, int> frequencyMap = new Dictionary<char, int>();
    public List<string> codes = new List<string>();
    public string encodedTextWhole = "";


    //Funkcija za postavljanje inicijalne tabele sa kodovima za sve karaktere
    public void InitializeCodeTable()
    {
        _codeTable = new Dictionary<char, Tuple<int, string>>();

        List<char> asciiChars = Enumerable.Range(32, 127 - 32 + 1).Select(i => (char)i).ToList();
        asciiChars.Add('\r');
        asciiChars.Add('\n');

        Random rng = new Random();
        int n = asciiChars.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            char value = asciiChars[k];
            asciiChars[k] = asciiChars[n];
            asciiChars[n] = value;
        }

        for(int i=0;i<asciiChars.Count;i++)
        {
            string code = GenerateCode(i+1);
            _codeTable[asciiChars[i]] = new Tuple<int, string>(0, code);
            frequencyMap[asciiChars[i]] = 0;
            codes.Add(code);
        }
    }

    //Funkcija za odredjivanje koda za svaki karakter u tekstu
    public void GenerateHuffmanCodes(char c)
    {
        frequencyMap[c]++;

        SortTable();

        string encodedText = _codeTable[c].Item2;
        string invertedEncodedText = new string(encodedText.Reverse().ToArray());
        encodedTextWhole += invertedEncodedText;
        Console.WriteLine($"Symbol: {c}, Current Code: {encodedText}");

        string decodedSymbol = DecompressBinaryInput(encodedText);
        Console.WriteLine($"Decompressed Symbol: {decodedSymbol}");
    }

    public string DecompressBinaryInput(string binaryInput)
    {
        StringBuilder decodedText = new StringBuilder();
        StringBuilder currentCode = new StringBuilder();
        foreach (char bit in binaryInput)
        {
            currentCode.Append(bit);
            foreach (var kvp in _codeTable)
            {
                if (kvp.Value.Item2 == currentCode.ToString())
                {
                    decodedText.Append(kvp.Key);
                    currentCode.Clear();
                    break;
                }
            }
        }
        Console.WriteLine();
        Console.WriteLine("Decoded text: " + decodedText);
        Console.WriteLine();
        return decodedText.ToString();
    }

    //Funkcija za kreiranje koda za svaki simbol
    private string GenerateCode(int length)
    {
        string code = "";
        if (length == 1)
        {
            code = "0";
            return code;
        }
        for (int i = 0; i < length - 1; i++)
        {
            code += "1";
        }
        code += "0";
        return code;
    }

    //Funkcija za upis kodirane tabele u fajl
    public void WriteHuffmanCodesToFile(string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            foreach (var kvp in _codeTable)
            {
                writer.WriteLine($"{kvp.Key} {kvp.Value.Item1} {kvp.Value.Item2}");
            }
        }
    }

    //Funkcija za organizaciju kodiranih podataka u bajtove i njihov upis u binarni fajl
    public void ConvertToBytesAndWriteToFile(string encodedText, string filePath)
    {
        int padding = 8 - (encodedText.Length % 8);
        encodedText = encodedText.PadRight(encodedText.Length + padding, '0');

        List<byte> bytes = new List<byte>();
        for (int i = 0; i < encodedText.Length; i += 8)
        {
            string byteString = encodedText.Substring(i, 8);
            byte b = Convert.ToByte(byteString, 2);
            bytes.Add(b);
        }

        File.WriteAllBytes(filePath, bytes.ToArray());
    }

    //Funkcija za dekodiranje kodiranih podataka
    public string DecompressText(string encodedText)
    {
        string decompressedText = "";
        int endIndex = encodedText.Length-1; 

        while (endIndex >= 0)
        {
            string currentCode = "";
            bool codeFound = false;

            //Citanje koda bit po bit od pozadi
            for (int i = endIndex; i >= 0; i--)
            {
                currentCode += encodedText[i]; 

                //Provera da li trenutni kod (currentCode) postoji u kodnoj tabeli
                foreach (var kvp in _codeTable)
                {
                    if (kvp.Value.Item2 == currentCode)
                    {
                        decompressedText = kvp.Key + decompressedText; 
                        frequencyMap[kvp.Key]--; 
                        endIndex = i - 1; 
                        codeFound = true;

                        SortTable();

                        break;
                    }
                }

                //Prekini petlju ako je pronadjen kod
                if (codeFound)
                    break; 
            }

            //Predji na sledeci bit ako nije pronadjen kod
            if (!codeFound)
                endIndex--;
        }

        return decompressedText;
    }

    private void SortTable()
    {
        var sortedList = _codeTable.ToList();

        sortedList.Sort((x, y) =>
        {
            int freqComparison = frequencyMap[y.Key].CompareTo(frequencyMap[x.Key]);
            return freqComparison == 0 ? x.Key.CompareTo(y.Key) : freqComparison;
        });

        Dictionary<char, Tuple<int, string>> newCodeTable = new Dictionary<char, Tuple<int, string>>();
        for (int j = 0; j < sortedList.Count; j++)
        {
            char symbol = sortedList[j].Key;
            string code = _codeTable[symbol].Item2;
            newCodeTable[symbol] = new Tuple<int, string>(frequencyMap[symbol], codes[j]);
        }

        _codeTable = newCodeTable;
    } 
        
    //Funkcija za citanje iz fajla
    public string ReadFile(string filePath)
    {
        string text;
        try
        {
            text = File.ReadAllText(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while reading the file: {ex.Message}");
            text = null;
        }
        return text;
    }

    public string GetWholeEncodedText() => encodedTextWhole;
}


class Program
{
    static void Main(string[] args)
    {
        HuffmanCode huffman = new HuffmanCode();
        string initialCodeTable = "initial_code_table.txt";
        string codeTableFilePath = "code_table.txt";
        string binaryFile = "encoded_text.bin";

        huffman.InitializeCodeTable();
        huffman.WriteHuffmanCodesToFile(initialCodeTable);

        Console.WriteLine("Should the input be read from the file? (Y/N)");
        string choise = Console.ReadLine();

        string input="";

        if (choise.ToUpper() == "Y")
        {
            input = huffman.ReadFile("input.txt");
        }
        if(choise.ToUpper() == "N")
        {
            Console.WriteLine("Enter the text to compress and decompress (type 'quit' to exit):");
            input=Console.ReadLine();
        }

        string encodedText = "";

        foreach (char ch in input)
        {
            huffman.GenerateHuffmanCodes(ch);
            huffman.WriteHuffmanCodesToFile(codeTableFilePath);
            Console.WriteLine();
            encodedText += ch; 
        }

        Console.WriteLine("Decompressed text: "+huffman.DecompressText(huffman.GetWholeEncodedText()));

        huffman.ConvertToBytesAndWriteToFile(huffman.GetWholeEncodedText(), binaryFile);

        Console.WriteLine("End text: " + encodedText);
        Console.WriteLine("Whole encoded text: " + huffman.GetWholeEncodedText());


    }
}
