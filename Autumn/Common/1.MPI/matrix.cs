using System;
using System.IO;

class Matrix
{
    private int[,] matrix;
    public int Size;

    public const int INF = 10000000;

    public Matrix(string path)
    {
        StreamReader fileHandler = new StreamReader(path);

        this.Size = Int32.Parse(fileHandler.ReadLine());

        this.matrix = new int[this.Size, this.Size];

        this.init(INF, 0);

        string line;
        while ((line = fileHandler.ReadLine()) != null)
        {
            string[] param = line.Split(' ');
            int i = Int32.Parse(param[0]);
            int j = Int32.Parse(param[1]);
            int w = Int32.Parse(param[2]);
            this.matrix[i, j] = w;
        }
        fileHandler.Close();
    }

    private void init(int cell, int diag)
    {
        for (int h = 0; h < this.Size; ++h)
        {
            for (int w = 0; w < this.Size; ++w)
            {
                if (h == w)
                    this.matrix[h, w] = diag;
                else
                    this.matrix[h, w] = cell;
            }
        }
    }

    public void print(string path = "")
    {
        if (path != "")
        {
            StreamWriter outputFile = new StreamWriter(path);
            for (int i = 0; i < this.Size; ++i)
            {
                for (int j = 0; j < this.Size; ++j)
                    outputFile.Write(this.matrix[i, j] + " ");
                outputFile.WriteLine();
            }
            outputFile.Close();
            return;
        }
        for (int i = 0; i < this.Size; ++i)
        {
            for (int j = 0; j < this.Size; ++j)
                Console.Write(this.matrix[i, j] + " ");
            Console.WriteLine();
        }
    }

    public int[] getRow(int num)
    {
        int[] row = new int[this.Size];
        for (int j = 0; j < this.Size; ++j)
            row[j] = this.matrix[num, j];
        return row;
    }

    public int getCell(int i, int j)
    {
        return this.matrix[i, j];
    }

    public void updateCell(int i, int j, int value)
    {
        this.matrix[i, j] = value;
    }
}

