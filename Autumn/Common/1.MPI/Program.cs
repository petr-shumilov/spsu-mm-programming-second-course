using System;
using System.IO;
using System.Collections.Generic;
using MPI;
using System.Diagnostics;

class MPIHello
{
    static void Main(string[] args)
    {
        using (new MPI.Environment(ref args))
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Invalid argument! Usage: <inFile> <outFile>");
                System.Environment.Exit(1);
            }
            string inputFilePath = args[0];
            string outputFilePath = args[1];

            // init communication interface
            Intracommunicator comm = Communicator.world;

            // <master> context
            if (Communicator.world.Rank == 0)
            {
                // extract matrix from file
                Matrix matrix = new Matrix(inputFilePath);
                Console.WriteLine("Success read");

                comm.Send<int>(matrix.Size, 1, 4);

                int procNum = (comm.Size - 2);
                int blocksNumber = (matrix.Size % procNum == 0) ? (matrix.Size / procNum) : (matrix.Size / procNum) + 1;

                int tasksNum = (matrix.Size / procNum);
                int tasksOver = (matrix.Size % procNum);

                // sending to workers num of tasks
                for (int proc = 1; proc < comm.Size - 1; ++proc)
                {
                    int[] msg = { ((proc <= tasksOver) ? tasksNum + 1 : tasksNum), matrix.Size };
                    comm.Send<int[]>(msg, (proc + 1), 5);
                    //Console.WriteLine("RunTime " + matrix.Size  +  " " +  procNum);
                }

                // start floid 
                for (int k = 0; k < matrix.Size; ++k)
                {
                    // splitting up rows
                        
                    for (int t = 1; t <= blocksNumber; ++t)
                    {
                        // pid counter
                        int curWorkerPid = 2;

                        // bounds of blocks 
                        int blockBegin = procNum * (t - 1);
                        int blockEnd = procNum * t - 1;

                        for (int i = blockBegin; i <= blockEnd && i < matrix.Size; ++i)
                        {
                            // prepare for send to parall process
                            Edge ik = new Edge(i, k, matrix.getCell(i, k));
                            /*
                            * * ik - edge 
                            * * i'th row of Matrix for ij
                            * * k'th row of Matrix for kj 
                            */
                            Request msg = new Request(ik, matrix.getRow(i), matrix.getRow(k));
                            comm.Send<Request>(msg, curWorkerPid, 0);
                            //qwe[curWorkerPid]++;
                            curWorkerPid++;
                        }

                    }

                }
                Console.WriteLine("Master ready");
            }
            else if (Communicator.world.Rank == 1)
            {
                //Stopwatch stopWatch = new Stopwatch();
                //stopWatch.Start();

                int _size = comm.Receive<int>(0, 4);    
                // extract matrix from file
                //Matrix matrix = new Matrix(inputFilePath, 1);
                Console.WriteLine("Success read");

                int procNum = (comm.Size - 2);
                int blocksNumber = (_size % procNum == 0) ? (_size / procNum) : (_size / procNum) + 1;

                for (int k = 0; k < _size; ++k)
                {
                    for (int t = 1; t <= blocksNumber; ++t)
                    {
                        int blockBegin = procNum * (t - 1);
                        int blockEnd = procNum * t - 1;

                        for (int i = blockBegin; i <= blockEnd && i < _size; ++i)
                        {
                            Edge[] resp = comm.Receive<Edge[]>(Communicator.anySource, 4);
                            //for (int o = 0; o < resp.Length; ++o)
                            //    matrix.updateCell(resp[o].from, resp[o].to, resp[o].value);
                            
                        }
                    }
                }
                Console.WriteLine("Success output");
                // print final Matrix
                //matrix.print(outputFilePath);
               /* TimeSpan ts = stopWatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);*/
            }
            else if (Communicator.world.Rank >= 2 && Communicator.world.Rank <= comm.Size) // <worker> context
            {
                // recive num of tasks
                int[] msg = comm.Receive<int[]>(0, 5);
                int tasksNum = msg[0];
                int _size = msg[1];

                //Console.WriteLine("I'm process " + comm.Rank + "; taskNum " + tasksNum + ";" + _size);

                //int qw = 0;
                // loop
                for (int loop = 0; loop < tasksNum * _size; ++loop)
                {
                    // recive task data
                    Request req = comm.Receive<Request>(0, 0);
                    
                    //string output = string.Join(", ", req.row_i);
                    //Console.WriteLine("I'm process " + comm.Rank + ";");
                    // container for sending 
                    List<Edge> resp = new List<Edge>();

                    for (int j = 0; j < _size; ++j)
                    {
                        if (req.ik.value + req.row_k[j] < req.row_i[j])
                        {
                            Edge updatedEdge = new Edge(req.ik.from, j, req.ik.value + req.row_k[j]);
                            resp.Add(updatedEdge);
                        }
                    }
                    /**/
                    // send result
                    comm.Send<Edge[]>(resp.ToArray(), 1, 4);
                    //qw++;
                }
                //Console.WriteLine("I'm process " + comm.Rank + "; " + qw);
            }
        }
    }
}   