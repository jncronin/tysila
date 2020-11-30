namespace BareBones
{
    class Program
    {
        static int pos = 0;
 
        unsafe static void Main()
        {
            // Clear the screen
            for(int i = 0; i < 80 * 25 * 2; i++)
                *(byte *)(0xb8000 + i) = 0;
 
            // Say hi
            Print("Hello World!");
        }
 
        static void Print(string s)
        {
            foreach(char c in s)
                Print(c);
        }
 
        unsafe static void Print(char c)
        {
            *(byte *)(0xb8000 + pos) = (byte)c;
            *(byte *)(0xb8000 + pos + 1) = 0x0f;
            pos += 2;
        }
    }
}
