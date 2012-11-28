using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


[Serializable]
class SerializeTest{

    [NonSerialized]
    private int x; 

    private int y; 

    public SerializeTest(int a, int b){

    x = a; 
    y = b; 

    }

    public override String ToString(){

    return "{x=" + x + ", y=" + y + "}"; 

    }

    public static void teste(String[] args){

    SerializeTest st = new SerializeTest(66, 61); 
    Console.WriteLine("Before Binary Write := " + st);

    Console.WriteLine("\n Writing SerializeTest object to disk");
    Stream output  = File.Create("serialized.bin");
    BinaryFormatter bwrite = new BinaryFormatter(); 
    bwrite.Serialize(output, st); 
    output.Close(); 

   // Ok, tenho um objeto stream com os dados (output)
   // Preciso mandá-lo via socket
 // StreamWriter 

    


    Console.WriteLine("\n Reading SerializeTest object from disk\n");
    Stream input  = File.OpenRead("serialized.bin");
    BinaryFormatter bread = new BinaryFormatter(); 
    SerializeTest fromdisk = (SerializeTest)bread.Deserialize(input); 
    input.Close(); 


    /* x will be 0 because it won't be read from disk since non-serialized */ 
    Console.WriteLine("After Binary Read := " + fromdisk);

    }

}

