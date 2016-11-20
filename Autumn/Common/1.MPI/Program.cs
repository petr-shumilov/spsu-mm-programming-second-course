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
                Matrix matrix = new Matrix(inputFilePath);
                //Console.WriteLine("Success read");

                int procNum = (comm.Size - 1);
                int blocksNumber = (matrix.Size % procNum == 0) ? (matrix.Size / procNum) : (matrix.Size / procNum) + 1;

                int tasksNum = (matrix.Size / procNum);
                int tasksOver = (matrix.Size % procNum);

                // sending to workers num of tasks
                for (int proc = 1; proc <= procNum; ++proc)
                {
                    int[] msg = {((proc <= tasksOver) ? tasksNum + 1 : tasksNum),  matrix.Size};
                    comm.Send<int[]>(msg, proc, 5);
                }

                // start floid 
                for (int k = 0; k < matrix.Size; ++k)
                {
                    // splitting up rows
                    for (int t = 1; t <= blocksNumber; ++t)
                    {
                        // pid counter
                        int curWorkerPid = 1;

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
                            curWorkerPid++;
                        }

                        // recieve and update Matrix
                        curWorkerPid = 1;
                        for (int i = blockBegin; i <= blockEnd && i < matrix.Size; ++i)
                        {
                            Edge[] resp = comm.Receive<Edge[]>(curWorkerPid, 2);
                            for (int o = 0; o < resp.Length; ++o)
                                matrix.updateCell(resp[o].from, resp[o].to, resp[o].value);
                            curWorkerPid++;
                        }
                    }
                }
                //Console.WriteLine("Success output");
                // print final Matrix
                matrix.print(outputFilePath);
            }
            else // <worker> context
            {
                // recive num of tasks
                int[] msg = comm.Receive<int[]>(0, 5);
                int tasksNum = msg[0];
                int _size = msg[1];

                // loop
                for(int loop = 0; loop < tasksNum * _size; ++loop)
                {
                    // recive task data
                    Request req = comm.Receive<Request>(0, 0);
                   
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

                    // send result
                    comm.Send<Edge[]>(resp.ToArray(), 0, 2);
                }
            }
        }
    }
}   