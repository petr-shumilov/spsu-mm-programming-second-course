package com.lab;

import java.io.*;
import java.net.Socket;

import static com.lab.Constants.SERVER_PORT;

/**
 * Created by Katrin on 28.09.2016.
 */
public class ClientForTest extends Thread {

    private String flag;
    private Socket socket;

    public ClientForTest(String flag) throws IOException {
        socket = new Socket("localhost", SERVER_PORT);
        this.flag = flag;
    }

    @Override
    public void run(){
        try {
            BufferedWriter clientOut = new BufferedWriter(new OutputStreamWriter(socket.getOutputStream(), "UTF-8"));
            clientOut.write(flag);
            clientOut.write("\n");
            clientOut.flush();

            BufferedReader clientIn = new BufferedReader(new InputStreamReader(socket.getInputStream(), "UTF-8"));
            clientIn.readLine();

            clientOut.write("end");
            clientOut.write("\n");
            clientOut.flush();

            clientIn.readLine();

            socket.close();
        } catch (IOException e) {
            e.printStackTrace();
        }


    }
}
