using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

/// <summary>
/// ���ݰ�����
/// </summary>
public enum PacketType
{
    /// <summary>
    /// ������δ����ʼ��
    /// </summary>
    None = 0,

    /// <summary>
    /// ���ӷ������ɹ�
    /// </summary>
    ConnectSuccess = 1,

    /// <summary>
    /// ���ӷ�����ʧ��
    /// </summary>
    ConnectFailed = 2,

    /// <summary>
    /// �յ��µ�TCP���ݰ�
    /// </summary>
    TcpPacket = 3,

    /// <summary>
    /// ���������ӶϿ�
    /// </summary>
    ConnectDisconnect = 4,
}

/// <summary>
/// ���������
/// </summary>
public class NetPacket
{
    /// <summary>
    /// ��������캯��
    /// </summary>
    /// <param name="packetType">���������</param>
    public NetPacket(PacketType packetType)
    {
        this.packetType = packetType;
        protoCode = 0;
        currRecv = 0;
    }

    /// <summary>
    /// ��������
    /// </summary>
    public PacketType packetType = PacketType.None;

    /// <summary>
    /// �����������TcpPacket,���ʾ�������Э��� ����������
    /// </summary>
    public int protoCode = 0;

    /// <summary>
    /// ���������������ڽ��հ�ͷʱ��ָ���ǰ�ͷ�յ������ֽ��ˣ�������ڽ��հ���ʱ��ָ���ǰ����յ������ֽ���
    /// </summary>
    public int currRecv = 0;

    /// <summary>
    /// ��ͷ���� ����ʱ����
    /// </summary>
    public byte[] PacketHeaderBytes = null;

    /// <summary>
    /// �������� ����ʱ����
    /// </summary>
    public byte[] PacketBodyBytes = null;

    /// <summary>
    /// ���������� ����ʱ����
    /// </summary>
    public byte[] PacketBytes = null;

    /// <summary>
    /// ����һ�����ñ�������ͷռ��8���ֽ�
    /// 1. ǰ4���ֽڱ�ʾ����ĳ���(��������ͷ����)
    /// 2. ��4���ֽڱ�ʾ�������Э���
    /// </summary>
    public static int HEADER_SIZE = 8;
}

/// <summary>
/// ��������� �̰߳�ȫ
/// </summary>
public class PacketQueue
{
    private Queue<NetPacket> netPackets = new Queue<NetPacket>();

    /// <summary>
    /// ����������
    /// </summary>
    /// <param name="netPacket"></param>
    public void Enqueue(NetPacket netPacket)
    {
        lock (netPackets)
        {
            netPackets.Enqueue(netPacket);
        }
    }

    /// <summary>
    /// �����������
    /// </summary>
    /// <returns></returns>
    public NetPacket Dequeue()
    {
        lock (netPackets)
        {
            if (netPackets.Count > 0)
            {
                return netPackets.Dequeue();
            }

            return null;
        }
    }

    /// <summary>
    /// ������������
    /// </summary>
    public void Clear()
    {
        lock (netPackets)
        {
            netPackets.Clear();
        }
    }
}

/// <summary>
/// TCP�ͻ�����
/// </summary>
public class TCPClient
{
    /// <summary>
    /// �������ӷ�����,������������̵߳���
    /// </summary>
    /// <param name="address">��������ַ</param>
    /// <param name="port">�������˿ں�</param>
    public void Connect(string address, int port)
    {
        lock (this)
        {
            if (socketState == false)
            {
                try
                {
                    Socket skt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    skt.BeginConnect(address, port, ConnectCallback, skt);
                }
                catch (Exception)
                {
                    packetQueue.Enqueue(new NetPacket(PacketType.ConnectFailed));
                }
            }
        }
    }

    /// <summary>
    /// ���߳�����ȡ�߶����е����������
    /// </summary>
    /// <returns></returns>
    public List<NetPacket> GetPackets()
    {
        List<NetPacket> packetList = new List<NetPacket>();

        NetPacket one = packetQueue.Dequeue();
        while (one != null)
        {
            packetList.Add(one);
            one = packetQueue.Dequeue();
        }

        return packetList;
    }

    /// <summary>
    /// ���̵߳��ã����������
    /// </summary>
    /// <param name="pCode">Э���</param>
    /// <param name="body">�����ֽ���</param>
    public void SendAsync(int pCode, byte[] body)
    {
        byte[] protoCode = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(pCode));
        byte[] bodySize = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(body.Length));
        byte[] package = new byte[bodySize.Length + protoCode.Length + body.Length];
        Array.Copy(bodySize, 0, package, 0, bodySize.Length);
        Array.Copy(protoCode, 0, package, bodySize.Length, protoCode.Length);
        Array.Copy(body, 0, package, bodySize.Length + protoCode.Length, body.Length);

        SendAsync(package);
    }

    /// <summary>
    /// ���̵߳��ã����������ֽ���
    /// </summary>
    /// <param name="bytes"></param>
    private void SendAsync(byte[] bytes)
    {
        lock (this)
        {
            try
            {
                if (socketState == true)
                {
                    socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, SendCallBack, socket);
                }
            }
            catch (Exception)
            {
                Disconnect();
            }
        }
    }

    public void SendCallBack(IAsyncResult asyncResult)
    {
        lock (this)
        {
            try
            {
                Socket socket = (Socket)asyncResult.AsyncState;

                socket.EndSend(asyncResult);
            }
            catch (Exception)
            {
                Disconnect();
            }
        }
    }

    /// <summary>
    /// �������ӷ������Ļص�����
    /// </summary>
    /// <param name="asyncResult"></param>
    public void ConnectCallback(IAsyncResult asyncResult)
    {
        lock (this)
        {
            if (socketState == true)
            {
                return;
            }

            try
            {
                // ���ӳɹ�
                socket = (Socket)asyncResult.AsyncState;

                socketState = true;

                socket.EndConnect(asyncResult);

                packetQueue.Enqueue(new NetPacket(PacketType.ConnectSuccess));

                // ��ʼ�������ݰ���ͷ
                ReadPacket();
            }
            catch (Exception)
            {
                socket = null;

                socketState = false;

                packetQueue.Enqueue(new NetPacket(PacketType.ConnectFailed));
            }
        }
    }

    /// <summary>
    /// ���յ����ݰ���ͷ�Ļص�����
    /// </summary>
    /// <param name="asyncResult"></param>
    public void ReceiveHeader(IAsyncResult asyncResult)
    {
        lock (this)
        {
            try
            {
                NetPacket netPacket = (NetPacket)asyncResult.AsyncState;

                // ʵ�ʶ�ȡ�����ֽ���
                int readSize = socket.EndReceive(asyncResult);

                // �����������Ͽ�����
                if (readSize == 0)
                {
                    Disconnect();

                    return;
                }

                netPacket.currRecv += readSize;

                if (netPacket.currRecv == NetPacket.HEADER_SIZE)
                {
                    // �յ���Լ���İ�ͷ�ĳ���,�����±�ǣ�����׼�����հ���
                    netPacket.currRecv = 0;

                    // �˰��İ����С
                    int bodySize = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(netPacket.PacketHeaderBytes, 0));

                    // �˰���Э���
                    int protoCode = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(netPacket.PacketHeaderBytes, 4));
                    netPacket.protoCode = protoCode;

                    // ע��:��ЩЭ��ȷʵû�а��岿�֣��������� ��ʱ bodySize == 0
                    if (bodySize < 0)
                    {
                        Disconnect();
                        return;
                    }

                    // ��ʼ���հ���
                    netPacket.PacketBodyBytes = new byte[bodySize];

                    if (bodySize == 0)
                    {
                        packetQueue.Enqueue(netPacket);
                        // ��ʼ��ȡ��һ�ΰ�
                        ReadPacket();
                        return;
                    }

                    socket.BeginReceive(netPacket.PacketBodyBytes, 0, bodySize, SocketFlags.None, ReceiveBody, netPacket);
                }
                else
                {
                    // ��ͷ���ݻ�û������,�������հ�ͷ
                    int remainSize = NetPacket.HEADER_SIZE - netPacket.currRecv;
                    socket.BeginReceive(netPacket.PacketBodyBytes, netPacket.currRecv, remainSize, SocketFlags.None, ReceiveHeader, netPacket);
                }
            }
            catch (Exception)
            {
                Disconnect();
            }
        }
    }

    /// <summary>
    /// ���յ����ݰ�����Ļص�����
    /// </summary>
    /// <param name="asyncResult"></param>
    public void ReceiveBody(IAsyncResult asyncResult)
    {
        lock (this)
        {
            try
            {
                NetPacket netPacket = (NetPacket)asyncResult.AsyncState;

                int readSize = socket.EndReceive(asyncResult);

                if (readSize == 0)
                {
                    // �����������Ͽ�����
                    Disconnect();
                    return;
                }

                netPacket.currRecv += readSize;

                if (netPacket.currRecv == netPacket.PacketBodyBytes.Length)
                {
                    // �յ���Լ���İ��峤�� �����±��
                    netPacket.currRecv = 0;

                    packetQueue.Enqueue(netPacket);

                    // ��ʼ��ȡ��һ�ΰ�
                    ReadPacket();
                }
                else
                {
                    // û���յ��㹻�İ���ĳ���,�����հ���
                    int remainSize = netPacket.PacketBodyBytes.Length - netPacket.currRecv;
                    socket.BeginReceive(netPacket.PacketBodyBytes, netPacket.currRecv, remainSize, SocketFlags.None, ReceiveBody, netPacket);
                }
            }
            catch (Exception)
            {
                Disconnect();
            }
        }
    }

    /// <summary>
    /// �Ͽ���������,�п�����io�̵߳���,Ҳ���������̵߳���
    /// </summary>
    public void Disconnect()
    {
        lock (this)
        {
            if (socketState == true)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception)
                {
                    socket.Close();

                    socket = null;
                    socketState = false;
                    packetQueue.Clear();

                    packetQueue.Enqueue(new NetPacket(PacketType.ConnectDisconnect));
                }
            }
        }
    }

    public bool GetSocketState()
    {
        lock (this)
        {
            return socketState;
        }
    }

    private void ReadPacket()
    {
        // ����һ��Tcp�Ŀհ�
        NetPacket netPacket = new NetPacket(PacketType.TcpPacket);

        // Լ�����ǰ�ͷ8���ֽ�
        netPacket.PacketHeaderBytes = new byte[NetPacket.HEADER_SIZE];

        // ��ʼ����Զ�˷��������ݰ�ͷ
        socket.BeginReceive(netPacket.PacketHeaderBytes, 0, NetPacket.HEADER_SIZE, SocketFlags.None, ReceiveHeader, netPacket);
    }

    /// <summary>
    /// ���TCPClient�������Ŀͻ���socket
    /// </summary>
    private Socket socket = null;

    /// <summary>
    /// ���͸����߳̽��յ����������
    /// </summary>
    private PacketQueue packetQueue = new PacketQueue();

    /// <summary>
    /// ��ǰ����״̬ true �������� false ��δ����
    /// </summary>
    private bool socketState = false;
}