using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

public class SocketConnection
{
    static SocketConnection instance;
    public static SocketConnection Instance
    {
        get
        {
            if (instance != null) return instance;
            instance = new SocketConnection();
            return instance;
        }
    }
    private SocketConnection() { }


    private TcpClient socketConnection;
    private Thread clientReceiveThread;
    public bool ConnectionIsAlive { get { return socketConnection == null ? false : socketConnection.Connected; } }
    /// <summary>
    /// 拆解資料
    /// </summary>
    public Action<string> GetDataFunction;

    /// <summary>
    /// 斷線或錯誤
    /// </summary>
    public Action<string> disconectFunction=(s)=> { Debug.Log(s); };

    /// <summary>
    /// 開始連線
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    Action StartConnectToTcpServer(string ip, int port)
    {
        return () =>
        {
            try
            {
                socketConnection = new TcpClient(ip, port);
                socketConnection.NoDelay = true;
                Byte[] bytes = new Byte[1024*64];
                while (socketConnection.Connected)
                {
                    // Get a stream object for reading 				
                    using (NetworkStream stream = socketConnection.GetStream())
                    {
                        int length;
                        // Read incomming stream into byte arrary. 					
                        while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            Debug.Log("get");
                            var incommingData = new byte[length];
                            Array.Copy(bytes, 0, incommingData, 0, length);
                            // Convert byte array to string message. 						
                            string serverMessage = Encoding.UTF8.GetString(incommingData);
                            var ls = Regex.Split(serverMessage, "}{");
                            Debug.Log(serverMessage);
                            //GetDataFunction(serverMessage);
                            if (ls.Length > 1)
                            {
                                for (int i = 0; i < ls.Length; i++)
                                {
                                    if (i == 0)
                                    {
                                        ls[i] = ls[i] + "}";
                                    }
                                    else if (i == ls.Length - 1)
                                    {
                                        ls[i] = "{" + ls[i];
                                    }
                                    else
                                    {
                                        ls[i] = "{" + ls[i] + "}";
                                    }
                                    Debug.Log(ls[i]);
                                    GetDataFunction(ls[i]);
                                }
                            }
                            else GetDataFunction(serverMessage);
                        }
                    }
                }
            }
            catch (SocketException socketException)
            {
                disconectFunction(socketException.ToString());
                Debug.Log("Socket exception: " + socketException);
            }
        };
    }

    /// <summary>
    /// 傳送資訊到Server
    /// </summary>
    /// <param name="message">傳送Json</param>
    public void SentDataToServer(string message)
    {
        if (socketConnection == null || socketConnection.Connected == false)
        {
            disconectFunction("disconnet");
            return;
        }

        try
        {
            // Get a stream object for writing. 			
            NetworkStream stream = socketConnection.GetStream();
            if (stream.CanWrite)
            {
                // Convert string message to byte array.                 
                byte[] clientMessageAsByteArray = Encoding.UTF8.GetBytes(message);
                // Write byte array to socketConnection stream.                 
                stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
            }
        }
        catch (SocketException socketException)
        {
            disconectFunction(socketException.ToString());
            Debug.Log("Socket exception: " + socketException);
        }
    }

    public void DisconnectToTcpServer()
    {
        if (clientReceiveThread != null && clientReceiveThread.IsAlive) clientReceiveThread.Abort();
        Debug.Log("Stop Connect");
    }

    public void ConnectToTcpServer(string ip, int port)
    {
        try
        {
            Action action = StartConnectToTcpServer(ip, port);
            clientReceiveThread = new Thread(new ThreadStart(action));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
            Debug.Log("Star Connect");
        }
        catch (Exception e)
        {
            Debug.Log("On client connect exception " + e);
        }
    }
}